using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using System;
using KcbBot.EchoBot.Services;
using KcbBot.EchoBot.Model.Data;

namespace KcbBot.EchoBot.Bots
{
    public class GeneralBot<T> : DialogBot<T>
        where T : Dialog
    {
        private readonly BotService _botService;

        public GeneralBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, BotService botService)
            : base(conversationState, userState, dialog, logger)
        {
            _botService = botService;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Get random greeting data
                    var greetingData = _botService.GetRandomGreeting();
                    var card = CreateGreetingCard(greetingData);

                    // Create an activity with the Adaptive Card attachment
                    var activity = MessageFactory.Attachment(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card
                    });

                    // Send the greeting Adaptive Card
                    await turnContext.SendActivityAsync(activity, cancellationToken);

                    // Run the dialog
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }

        private AdaptiveCard CreateGreetingCard(GreetingData greetingData)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3));
            if (!string.IsNullOrEmpty(greetingData.ImageUrl))
            {
                card.Body.Add(new AdaptiveImage
                {
                    Url = new Uri(greetingData.ImageUrl)
                });
            }
            if (!string.IsNullOrEmpty(greetingData.Text))
            {
                card.Body.Add(new AdaptiveTextBlock
                {
                    Text = greetingData.Text,
                    Wrap = true
                });
            }

            return card;
        }
    }
}
