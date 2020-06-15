using AdaptiveCardMaker;
using AdaptiveCards;
using AdaptiveCards.Rendering.Html;
using AdaptiveExpressions;
using Antlr4.Runtime.Misc;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using TestCardConsumer.CardLibrary;

namespace TestCardConsumer.ConsoleApp
{
    public class Client
    {
        private readonly string _adaptiveCardHtmlStylesheetUrl = "https://raw.githubusercontent.com/microsoft/AdaptiveCards/main/source/nodejs/adaptivecards-site/themes/adaptivecards/source/css/style.css";
        private readonly CardGenerator _cardGenerator;
        private readonly CardGenerator<ImportedCardLibrary> _cardGeneratorImportedCardLibrary;
        public Client(CardGenerator cardGenerator, CardGenerator<ImportedCardLibrary> cardGeneratorImportedCardLibrary)
        {
            this._cardGenerator = cardGenerator;
            this._cardGeneratorImportedCardLibrary = cardGeneratorImportedCardLibrary;
        }

        public void Run()
        {
            string cardName, templatedCardJson;

            cardName = "demoCard1";
            // generate Adaptive Card JSON
            templatedCardJson = GenerateCardJsonFromEmbeddedResource(this._cardGenerator, cardName);
            Console.WriteLine(templatedCardJson);
            // render as HTML from Adaptive Card JSON
            RenderAdaptiveCardInHtmlFromJson(templatedCardJson);


            cardName = "expressionTest1";
            var expressions = new List<ExpressionEvaluator>
            {
                new ExpressionEvaluator(
                    "stringFormat",
                    ExpressionFunctions.Apply((args) =>
                        {
                            string formattedString = "";

                            // argument is packed in sequential order as defined in the template
                            // For example, suppose we have "${stringFormat(strings.myName, person.firstName, person.lastName)}"
                            // args will have following entries
                            // args[0]: strings.myName
                            // args[1]: person.firstName
                            // args[2]: strings.lastName 
                            if (args[0] != null && args[1] != null && args[2] != null)
                            {
                                string formatString = args[0].ToString();
                                string[] stringArguments = { args[1].ToString(), args[2].ToString() };
                                formattedString = string.Format(formatString, stringArguments);
                            }
                            return formattedString;
                        }),
                    ReturnType.String
                ),
            };
            templatedCardJson = GenerateCardJsonFromEmbeddedResource(this._cardGenerator, cardName, expressions);
            Console.WriteLine(templatedCardJson);
            // render as HTML from Adaptive Card JSON
            RenderAdaptiveCardInHtmlFromJson(templatedCardJson);

            cardName = "demoCard2";
            // generate Adaptive Card JSON
            templatedCardJson = GenerateCardJsonFromEmbeddedResourceOfImportedCardLibrary(this._cardGeneratorImportedCardLibrary, cardName);
            Console.WriteLine(templatedCardJson);
            // render as HTML from Adaptive Card JSON
            RenderAdaptiveCardInHtmlFromJson(templatedCardJson);


            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Main function that calls this library's code.
        /// First will 
        /// </summary>
        /// <param name="cardName"></param>
        /// <returns></returns>
        private string GenerateCardJsonFromEmbeddedResource(CardGenerator cardGenerator, string cardName, IEnumerable<ExpressionEvaluator> customFunctions = null)
        {
            string dataJson = null;
            var cardDataResourcePath = $"TestCardConsumer.ConsoleApp.Cards.{cardName}.data.json";
            using Stream stream = GetType().Assembly.GetManifestResourceStream(cardDataResourcePath);
            if (stream == null)
            {
                Console.WriteLine("!!! Data JSON could not be loaded - it may not exist for this card or it is not set with compile action of Embedded Resource and not copied to output");
            }
            else
            {
                using var reader = new StreamReader(stream);
                dataJson = reader.ReadToEnd();
            }

            dynamic data = string.IsNullOrWhiteSpace(dataJson) ? null : JsonConvert.DeserializeObject<dynamic>(dataJson);
            Attachment attachment = cardGenerator.CreateAdaptiveCardAttachment(cardName, data, customFunctions);
            var templatedCardJson = attachment.Content.ToString();
            return templatedCardJson;
        }

        /// <summary>
        /// Main function that calls this library's code.
        /// First will 
        /// </summary>
        /// <param name="cardName"></param>
        /// <returns></returns>
        private string GenerateCardJsonFromEmbeddedResourceOfImportedCardLibrary(CardGenerator cardGenerator, string cardName, IEnumerable<ExpressionEvaluator> customFunctions = null)
        {
            string dataJson = null;
            var cardDataResourcePath = $"TestCardConsumer.CardLibrary.MyCards.{cardName}.data.json";
            using Stream stream = Assembly.GetAssembly(typeof(ImportedCardLibrary)).GetManifestResourceStream(cardDataResourcePath);
            if (stream == null)
            {
                Console.WriteLine("!!! Data JSON could not be loaded - it may not exist for this card or it is not set with compile action of Embedded Resource and not copied to output");
            }
            else
            {
                using var reader = new StreamReader(stream);
                dataJson = reader.ReadToEnd();
            }

            dynamic data = string.IsNullOrWhiteSpace(dataJson) ? null : JsonConvert.DeserializeObject<dynamic>(dataJson);
            Attachment attachment = cardGenerator.CreateAdaptiveCardAttachment(cardName, data, customFunctions);
            var templatedCardJson = attachment.Content.ToString();
            return templatedCardJson;
        }

        /// <summary>
        /// Not using library code here. Using AdaptiveCards.Renderer.Html package to 
        /// take the generated card JSON, parse it, and ensure default renderer doesn't fail 
        /// with provided output from AdaptiveCardMaker code.
        /// </summary>
        /// <param name="cardJson"></param>
        private void RenderAdaptiveCardInHtmlFromJson(string cardJson)
        {
            var renderer = new AdaptiveCardRenderer();
            AdaptiveCard card = AdaptiveCard.FromJson(cardJson).Card;

            try
            {
                // Render the card
                RenderedAdaptiveCard renderedCard = renderer.RenderCard(card);

                // Get the output HTML 
                HtmlTag html = renderedCard.Html;

                // (Optional) Check for any renderer warnings
                // This includes things like an unknown element type found in the card
                // Or the card exceeded the maximum number of supported actions, etc
                IList<AdaptiveWarning> warnings = renderedCard.Warnings;
                if (warnings?.Any() == true)
                {
                    foreach (var warning in warnings)
                    {
                        Console.WriteLine($"!!! Renderer Warning: {warning}");
                    }
                }
                else
                {
                    string fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".html";
                    using var swXLS = new StreamWriter(fileName);
                    var styleCode = DownloadFromUrl(this._adaptiveCardHtmlStylesheetUrl);
                    swXLS.Write($"<style>{styleCode}</style>");
                    swXLS.Write(html.ToString());
                    swXLS.Close();
                    OpenUrl(fileName);
                }
            }
            catch (AdaptiveException ex)
            {
                Console.WriteLine($"!!! Renderer Failure: {ex.Message}");
            }
        }

        /// <summary>
        /// Utility function to open a URL. Nothing to see here other than visual usability testing
        /// </summary>
        /// <param name="url"></param>
        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true, UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Utility function to open a URL. Nothing to see here other than visual usability testing
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string DownloadFromUrl(string url)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            using StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            return result;
        }
    }
}
