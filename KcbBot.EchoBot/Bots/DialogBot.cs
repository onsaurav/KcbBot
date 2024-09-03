using KcbBot.EchoBot.Dialogs;
using KcbBot.EchoBot.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KcbBot.EchoBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
#pragma warning disable SA1401 // Fields should be private
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
#pragma warning restore SA1401 // Fields should be private

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            var messageText = turnContext.Activity.Text?.Trim();

            // Retrieve the user profile
            var userProfile = await ((MainDialog)Dialog).GetUserProfileAsync(turnContext, cancellationToken);

            // Check if the conversation is complete
            if (userProfile.ConversationComplete)
            {
                // Ignore the message and do not respond
                await turnContext.SendActivityAsync("This conversation has already been completed. If you have any further questions, please start a new conversation by closing and reopening the chatbox. Thank you!", cancellationToken: cancellationToken);
                return;
            }

            // Check if the user sent the end conversation command
            if (!string.IsNullOrEmpty(messageText) && messageText.Equals("#endUserConversation", StringComparison.OrdinalIgnoreCase))
            {
                var transcriptJson = Newtonsoft.Json.JsonConvert.SerializeObject(userProfile.ChatHistory);

                var chatLog = new ChatLog
                {
                    Ip = "32432",
                    ChatId = userProfile.ConversationId,
                    Transcript = transcriptJson,
                    StartDate = userProfile.ChatHistory.First().chatTime,
                    EndDate = userProfile.ChatHistory.Last().chatTime
                };

                await ((MainDialog)Dialog).ChatLogService.SaveChatLogAsync(chatLog);

                // Create a summary of the chat log to send back to the user
                // var chatSummary = userProfile.ChatHistory.Select(chat => $"{chat.Sender} said: {chat.Message}");
                // var chatSummaryText = string.Join("\n", chatSummary);

                // await turnContext.SendActivityAsync("Conversation ended and chat history saved. Here is a summary of the chat:", cancellationToken: cancellationToken);
                // await turnContext.SendActivityAsync(chatSummaryText, cancellationToken: cancellationToken);
                return;
            }

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
