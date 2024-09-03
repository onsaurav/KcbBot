// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KcbBot.EchoBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //var replyText = $"Echo: {turnContext.Activity.Text}";
            //await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

            var userMessage = turnContext.Activity.Text.ToLower();
            string cardJson;

            // Determine which card to send based on user message
            if (userMessage.Contains("hi") || userMessage.Contains("hello"))
            {
                cardJson = ReadCardJson("greetingCard.json");
            }
            else if (userMessage.Contains("bye") || userMessage.Contains("all the best"))
            {
                cardJson = ReadCardJson("farewellCard.json");
            }
            else
            {
                cardJson = ReadCardJson("chatCard.json");
            }

            // Replace placeholders with dynamic content
            string userName = "User"; // Replace with actual user data if available
            cardJson = cardJson.Replace("{name}", userName).Replace("{dynamicText}", userMessage);

            // Create an AdaptiveCard instance from the JSON
            var adaptiveCard = AdaptiveCard.FromJson(cardJson).Card;

            // Send the Adaptive Card as a response
            var reply = MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            });

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        private string ReadCardJson(string fileName)
        {
            // Construct the full path to the file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "cards", fileName);

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{fileName}' was not found in the 'Resources' directory.");
            }

            // Read the content of the file and return it as a string
            return File.ReadAllText(filePath);
        }
    }
}
