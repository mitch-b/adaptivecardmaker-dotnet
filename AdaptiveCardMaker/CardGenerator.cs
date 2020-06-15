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
    public class CardGenerator: ICardGenerator
    {
        internal readonly CardGeneratorOptions _options;

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

            if (string.IsNullOrWhiteSpace(this._options.ProjectNamespace))
            {
                throw new Exception("Bad configuration of AdaptiveCardMaker. Please supply a value for ProjectNamespace in service registration.");
            }

            this._options.ManifestResourceRoot = $"{this._options.ProjectNamespace}.";
            if (!string.IsNullOrWhiteSpace(this._options.ManifestResourcePathFromNamespace))
            {
                this._options.ManifestResourceRoot += $"{this._options.ManifestResourcePathFromNamespace}.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardFileName">Name of embedded resource to read. Example, <code>welcomeCard.json</code></param>
        /// <param name="data">Optional, dynamic object containing data to inject into card template</param>
        /// <param name="customFunctions">Optional, for examples, see https://github.com/microsoft/botbuilder-dotnet/blob/master/libraries/AdaptiveExpressions/ExpressionFunctions.cs </param>
        /// <returns></returns>
        public Attachment CreateAdaptiveCardAttachment(string cardFileName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null)
        {
            var cardResourcePath = $"{this._options.ManifestResourceRoot}{cardFileName}";

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

    public class CardGenerator<T> : CardGenerator, ICardGenerator<T>
    {
        public CardGenerator(IOptions<CardGeneratorOptions<T>> cardGeneratorOptions = null)
            : base(cardGeneratorOptions)
        {
        }
    }
}
