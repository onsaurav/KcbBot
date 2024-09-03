using AdaptiveCards;
using KcbBot.EchoBot.Common;
using KcbBot.EchoBot.Helper;
using KcbBot.EchoBot.Model.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KcbBot.EchoBot.Bots
{
    public class KcbBot : ActivityHandler
    {
        private readonly IStatePropertyAccessor<ConversationData> _conversationDataAccessor;

        public KcbBot(UserState userState, ConversationState conversationState)
        {
            _conversationDataAccessor = conversationState.CreateProperty<ConversationData>("ConversationData");
        }

        // Handle Message Activities (standard messages from the user)
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                string replyMessage = turnContext.Activity.Text.ToLower();
                await turnContext.SendActivityAsync(MessageFactory.Text(replyMessage), cancellationToken);
            }            
        }

        // Handle Invoke Activities (such as Adaptive Card actions)
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Invoke)
            {
                var actionData = turnContext.Activity.Value as JObject;
                if (actionData != null)
                {
                    var action = actionData["action"]?.ToString();
                    if (action == "platform_selection")
                    {
                        var selectedPlatform = actionData["platform"]?.ToString();
                        if (!string.IsNullOrEmpty(selectedPlatform))
                        {
                            // Retrieve the conversation data
                            var conversationData = await _conversationDataAccessor.GetAsync(turnContext, () => new ConversationData());
                            conversationData.SelectedPlatform = selectedPlatform;

                            // Save the updated data
                            await _conversationDataAccessor.SetAsync(turnContext, conversationData, cancellationToken);

                            // Send confirmation message
                            await turnContext.SendActivityAsync(MessageFactory.Text($"You selected {selectedPlatform}."), cancellationToken);

                            // Return a successful response
                            return new InvokeResponse { Status = 200 };
                        }
                    }
                }

                // Return an error response if action type is unrecognized
                return new InvokeResponse { Status = 400 };
            }

            // For non-Invoke activities
            return await base.OnInvokeActivityAsync(turnContext, cancellationToken);
        }


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeText = "You are welcome here!";
                    string cardJson = JsonHelper.ReadCardJson(Metadata.WelcomeCard);
                    cardJson = cardJson.Replace("{name}", "USER").Replace("{dynamicText}", welcomeText);
                    var welcomeCard = AdaptiveCard.FromJson(cardJson).Card;

                    var welcomeAttachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = welcomeCard
                    };
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(welcomeAttachment), cancellationToken);

                    cardJson = JsonHelper.ReadCardJson(Metadata.PlatformSelectionCard);
                    var platformSelectionCard = AdaptiveCard.FromJson(cardJson).Card;

                    var platformAttachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = platformSelectionCard
                    };
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(platformAttachment), cancellationToken);
                }
            }
        }
    }
}
