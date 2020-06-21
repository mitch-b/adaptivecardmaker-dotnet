using AdaptiveCards.Templating;
using AdaptiveExpressions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdaptiveCardMaker
{
    public class CardGenerator: ICardGenerator
    {
        internal readonly CardGeneratorOptions _options;
        internal readonly string _resourceRoot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardGeneratorOptions"></param>
        public CardGenerator(IOptions<CardGeneratorOptions> cardGeneratorOptions = null)
        {
            this._options = cardGeneratorOptions != null ? cardGeneratorOptions.Value
                : new CardGeneratorOptions();

            if (this._options.AssemblyWithEmbeddedResources == null)
            {
                throw new ArgumentException($"Bad configuration of AdaptiveCardMaker. Please supply a value for {nameof(this._options.AssemblyWithEmbeddedResources)} in service registration.");
            }

            this._resourceRoot = string.Empty;
            if (this._options.RelativeOrderedPathToCards?.Any(p => !string.IsNullOrWhiteSpace(p)) == true)
            {
                this._resourceRoot = string.Join(".", 
                    this._options.RelativeOrderedPathToCards
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim().Trim('.')))
                    + ".";
            }
        }

        /// <summary>
        /// Create AdaptiveCard Attachment from JSON template using template engine. 
        /// </summary>
        /// <param name="cardName">Name of embedded resource to read. Example, <code>welcomeCard.json</code></param>
        /// <param name="data">Optional, dynamic object containing data to inject into card template</param>
        /// <param name="customFunctions">Optional, for examples, see https://github.com/microsoft/botbuilder-dotnet/blob/master/libraries/AdaptiveExpressions/ExpressionFunctions.cs </param>
        public async Task<Attachment> CreateAdaptiveCardAttachmentAsync(string cardFileName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null)
        {
            var cardResourcePath = $"{this._resourceRoot}{cardFileName}";
            var embeddedProvider = new EmbeddedFileProvider(this._options.AssemblyWithEmbeddedResources);
            using var stream = embeddedProvider.GetFileInfo(cardResourcePath).CreateReadStream();
            if (stream == null)
            {
                throw new Exception($"Embedded resource named '{cardResourcePath}' could not be " 
                    + $"found in '{this._options.AssemblyWithEmbeddedResources.GetName().Name}'. "
                    + "Please ensure the card file is marked with Build Action of 'Embedded resource' and Copied to Output Directory.");
            }
            using var reader = new StreamReader(stream);
            var templateJson = await reader.ReadToEndAsync();
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

        /// <summary>
        /// Create AdaptiveCard Attachment from JSON template using template engine. 
        /// </summary>
        /// <param name="cardName">Name of embedded resource to read. Example, <code>welcomeCard.json</code></param>
        /// <param name="data">Optional, dynamic object containing data to inject into card template</param>
        /// <param name="customFunctions">Optional, for examples, see https://github.com/microsoft/botbuilder-dotnet/blob/master/libraries/AdaptiveExpressions/ExpressionFunctions.cs </param>
        /// <returns></returns>
        public Attachment CreateAdaptiveCardAttachment(string cardFileName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null)
        {
            return this.CreateAdaptiveCardAttachmentAsync(cardFileName, data, customFunctions).GetAwaiter().GetResult();
        }
    }

    public class CardGenerator<T> : CardGenerator, ICardGenerator<T>
    {
        public CardGenerator(IOptions<CardGeneratorOptions<T>> cardGeneratorOptions = null)
            : base(cardGeneratorOptions)
        {
        }
    }
}
