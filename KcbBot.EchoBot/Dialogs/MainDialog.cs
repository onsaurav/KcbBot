using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using System;
using System.Linq;
using Newtonsoft.Json;
using KcbBot.EchoBot.Services;
using KcbBot.EchoBot.Model;
using KcbBot.EchoBot.Model.Data;

namespace KcbBot.EchoBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly BotService _botService;
        private readonly ExternalApiService _externalApiService;
        private readonly ConfigService _configService;
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly ChatLogService _chatLogService;

        public MainDialog(BotService botService, ExternalApiService externalApiService, ConfigService configService, ChatLogService chatLogService, UserState userState)
            : base(nameof(MainDialog))
        {
            _botService = botService;
            _externalApiService = externalApiService;
            _configService = configService;
            _userState = userState;
            _chatLogService = chatLogService;
            _userProfileAccessor = _userState.CreateProperty<UserProfile>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                StartInitialQuestionsDialogAsync,
                StartExternalApiDialogAsync,
                EndSurveyDialogAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ExternalApiDialog(_botService, _externalApiService, _userState));
            AddDialog(new SurveyDialog(_configService, _userState));
            AddDialog(new InitialQuestionsDialog(_configService, _userState));

            InitialDialogId = nameof(WaterfallDialog);
        }

        public ChatLogService ChatLogService => _chatLogService;

        public async Task<UserProfile> GetUserProfileAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await _userProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);
        }

        private async Task<DialogTurnResult> StartInitialQuestionsDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(InitialQuestionsDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> StartExternalApiDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(ExternalApiDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndSurveyDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // End the dialog
            return await stepContext.BeginDialogAsync(nameof(SurveyDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Retrieve user profile from user state
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Serialize ChatHistory to JSON string remove this logic if the API is updated
            var transcriptJson = JsonConvert.SerializeObject(userProfile.ChatHistory);

            // Create ChatLog object
            var chatLog = new ChatLog
            {
                Ip = "32432",
                ChatId = userProfile.ConversationId,
                Transcript = transcriptJson,
                StartDate = userProfile.ChatHistory.First().chatTime,
                EndDate = userProfile.ChatHistory.Last().chatTime
            };

            // Save chat log
            await _chatLogService.SaveChatLogAsync(chatLog);

            // Mark the conversation as complete
            userProfile.ConversationComplete = true;
            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            await stepContext.Context.SendActivityAsync("Thank you for sharing your details! Have a great day!", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
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
