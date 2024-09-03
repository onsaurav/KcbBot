// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using AdaptiveCards;
using KcbBot.EchoBot.Common;
using KcbBot.EchoBot.Helper;
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
                cardJson = JsonHelper.ReadCardJson("greetingCard.json");
            }
            else if (userMessage.Contains("bye") || userMessage.Contains("all the best"))
            {
                cardJson = JsonHelper.ReadCardJson("farewellCard.json");
            }
            else
            {
                cardJson = JsonHelper.ReadCardJson("chatCard.json");
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
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    //var welcomeText = "Welcome!";
                    //await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);

                    var welcomeText = "You are welcome here!";
                    string cardJson = JsonHelper.ReadCardJson(Metadata.WelcomeCard);
                    cardJson = cardJson.Replace("{name}", "USER").Replace("{dynamicText}", welcomeText);
                    var welcomeCard = AdaptiveCard.FromJson(cardJson).Card;
                    var reply = MessageFactory.Attachment(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = welcomeCard
                    });
                    await turnContext.SendActivityAsync(reply, cancellationToken);

                    cardJson = JsonHelper.ReadCardJson(Metadata.PlatformSelectionCard);
                    var platformSelectionCard = AdaptiveCard.FromJson(cardJson).Card;
                    var nextReply = MessageFactory.Attachment(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = platformSelectionCard
                    });
                    await turnContext.SendActivityAsync(nextReply, cancellationToken);
                }
            }
        }
    }
}
