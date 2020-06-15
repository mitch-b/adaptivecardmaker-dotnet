using AdaptiveCards.Templating;
using AdaptiveExpressions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AdaptiveCardMaker
{
    public class CardGeneratorOptions
    {
        public Assembly AssemblyWithEmbeddedResources { get; set; } = Assembly.GetEntryAssembly();
        public string ManifestResourcePathFromNamespace { get; set; } = "Cards";
        public string ProjectNamespace { get; set; }
        public CardGeneratorOptions()
        {
        }
    }
    public class CardGeneratorOptions<T> : CardGeneratorOptions
    {
    }

    public interface ICardGenerator
    {
        Attachment CreateAdaptiveCardAttachment(string cardName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null);
    }

    public interface ICardGenerator<T> : ICardGenerator
    {
    }

    public class CardGenerator: ICardGenerator
    {
        internal CardGeneratorOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectNamespace">If not provided, reflection is used.</param>
        public CardGenerator(IOptions<CardGeneratorOptions> cardGeneratorOptions = null)
        {
            if (cardGeneratorOptions == null)
            {
                this._options = new CardGeneratorOptions();
            }
            else
            {
                this._options = cardGeneratorOptions.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardName"></param>
        /// <param name="data"></param>
        /// <param name="customFunctions">For examples, see https://github.com/microsoft/botbuilder-dotnet/blob/master/libraries/AdaptiveExpressions/ExpressionFunctions.cs </param>
        /// <returns></returns>
        public Attachment CreateAdaptiveCardAttachment(string cardName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null)
        {
            if (string.IsNullOrWhiteSpace(this._options.ProjectNamespace))
            {
                throw new Exception("Bad configuration of AdaptiveCardMaker. Please supply ProjectNamespace in service registration.");
            }

            var cardResourcePath = $"{this._options.ProjectNamespace}.{this._options.ManifestResourcePathFromNamespace}.{cardName}.json";

            using Stream stream = this._options.AssemblyWithEmbeddedResources.GetManifestResourceStream(cardResourcePath);
            using var reader = new StreamReader(stream);
            var templateJson = reader.ReadToEnd();
            var template = new AdaptiveCardTemplate(templateJson);
            string cardJson = templateJson;

            if (customFunctions?.Any() == true)
            {
                foreach (var func in customFunctions)
                {
                    Expression.Functions.Add(func.Type, func);
                }
            }

            if (data != null)
            {
                var context = new EvaluationContext
                {
                    Root = data
                };
                cardJson = template.Expand(context);
            }

            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
        }
    }

    public class CardGenerator<T>: CardGenerator, ICardGenerator<T>
    {
        public CardGenerator(IOptions<CardGeneratorOptions<T>> cardGeneratorOptions = null)
        {
            if (cardGeneratorOptions == null)
            {
                this._options = new CardGeneratorOptions<T>();
            }
            else
            {
                this._options = cardGeneratorOptions.Value;
            }
        }
    }
}
