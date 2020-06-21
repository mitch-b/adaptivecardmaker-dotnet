using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Xunit;

namespace AdaptiveCardMaker.Tests
{
    public class CardGeneratorTests
    {
        public static IEnumerable<object[]> RelativeOrderedPathToCardsTestCases => new[]
                {
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { "Cards" } }, "Cards." },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { (string)null } }, "" },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { "" } }, "" },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { " " } }, "" },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { "My", "Cards" } }, "My.Cards." },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { "My", "Path", "To", "Cards" } }, "My.Path.To.Cards." },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { "My", "Path ", "", "To", "Cards" } }, "My.Path.To.Cards." },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { "My.Path.Cards" } }, "My.Path.Cards." },
                    new object[] { new CardGeneratorOptions { RelativeOrderedPathToCards = new[] { "My", ".Path.Cards." } }, "My.Path.Cards." }
                };

        [Theory]
        [MemberData(nameof(RelativeOrderedPathToCardsTestCases))]
        public void CardGeneratorCreatesPathToCards(CardGeneratorOptions cardGeneratorOptions, string expectedPath)
        {
            var generator = new CardGenerator(Options.Create(cardGeneratorOptions));
            Assert.True(generator._resourceRoot == expectedPath);
        }
    }
}
