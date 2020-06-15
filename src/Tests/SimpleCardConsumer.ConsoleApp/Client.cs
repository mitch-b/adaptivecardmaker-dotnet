using AdaptiveCardMaker;
using Microsoft.Bot.Schema;
using System;

namespace SimpleCardConsumer.ConsoleApp
{
    public class Client
    {
        private readonly ICardGenerator _cardGenerator;
        public Client(ICardGenerator cardGenerator)
        {
            this._cardGenerator = cardGenerator;
        }

        public void Run()
        {
            string cardName = "demoCard3.json";
            var data = new
            {
                Name = "Jane"
            };

            Attachment attachment = this._cardGenerator.CreateAdaptiveCardAttachment(cardName, data);

            string templatedCardJson = attachment.Content.ToString();
            Console.WriteLine(templatedCardJson);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
