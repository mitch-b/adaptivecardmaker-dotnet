using AdaptiveExpressions;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace AdaptiveCardMaker
{
    public interface ICardGenerator
    {
        Attachment CreateAdaptiveCardAttachment(string cardName, dynamic data = null, IEnumerable<ExpressionEvaluator> customFunctions = null);
    }

    public interface ICardGenerator<T> : ICardGenerator
    {
    }
}
