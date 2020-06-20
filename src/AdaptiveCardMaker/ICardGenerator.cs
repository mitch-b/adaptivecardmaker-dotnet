using AdaptiveExpressions;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdaptiveCardMaker
{
    public interface ICardGenerator
    {
        /// <summary>
        /// Create AdaptiveCard Attachment from JSON template using template engine. 
        /// </summary>
        /// <param name="cardName">Name of embedded resource to read. Example, <code>welcomeCard.json</code></param>
        /// <param name="data">Optional, dynamic object containing data to inject into card template</param>
        /// <param name="customFunctions">Optional, for examples, see https://github.com/microsoft/botbuilder-dotnet/blob/master/libraries/AdaptiveExpressions/ExpressionFunctions.cs </param>
        /// <returns></returns>
        Attachment CreateAdaptiveCardAttachment(string cardName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null);
        /// <summary>
        /// Create AdaptiveCard Attachment from JSON template using template engine. 
        /// </summary>
        /// <param name="cardName">Name of embedded resource to read. Example, <code>welcomeCard.json</code></param>
        /// <param name="data">Optional, dynamic object containing data to inject into card template</param>
        /// <param name="customFunctions">Optional, for examples, see https://github.com/microsoft/botbuilder-dotnet/blob/master/libraries/AdaptiveExpressions/ExpressionFunctions.cs </param>
        /// <returns></returns>
        Task<Attachment> CreateAdaptiveCardAttachmentAsync(string cardName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null);
    }

    public interface ICardGenerator<T> : ICardGenerator
    {
    }
}
