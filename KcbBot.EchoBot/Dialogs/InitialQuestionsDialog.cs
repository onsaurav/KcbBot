using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using KcbBot.EchoBot.Services;
using KcbBot.EchoBot.Model;

namespace KcbBot.EchoBot.Dialogs
{
    public class InitialQuestionsDialog : ComponentDialog
    {
        private readonly ConfigService _configService;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public InitialQuestionsDialog(ConfigService configService, UserState userState)
            : base(nameof(InitialQuestionsDialog))
        {
            _configService = configService;
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var waterfallSteps = new List<WaterfallStep>();

            for (int i = 0; i < _configService.InitialQuestions.Count; i++)
            {
                var index = i; // Capture the loop variable
                waterfallSteps.Add(async (stepContext, cancellationToken) => await AskInitialQuestionAsync(stepContext, index, cancellationToken));
                waterfallSteps.Add(async (stepContext, cancellationToken) => await GetValueAsync(stepContext, index, cancellationToken));
            }

            waterfallSteps.Add(async (stepContext, cancellationToken) => await EndInitialQuestionsAsync(stepContext, cancellationToken));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskInitialQuestionAsync(WaterfallStepContext stepContext, int questionIndex, CancellationToken cancellationToken)
        {

            int currentIndex;

            // Check if stepContext.Result has a value and use it if present
            if (stepContext.Result != null)
            {
                currentIndex = (int)stepContext.Result;
            }
            else
            {
                // Check if stepContext.Options is not null and has a property named "Index"
                if (stepContext.Options != null && stepContext.Options.GetType().GetProperty("Index") != null)
                {
                    currentIndex = (int)stepContext.Options.GetType().GetProperty("Index").GetValue(stepContext.Options, null);
                }
                else
                {
                    // If neither stepContext.Result nor stepContext.Options["Index"] has a value, use questionIndex
                    currentIndex = questionIndex;
                }
            }
            // Check if currentIndex is out of range
            if (currentIndex >= _configService.InitialQuestions.Count)
            {
                return await EndInitialQuestionsAsync(stepContext, cancellationToken);
            }

            stepContext.Values["Index"] = currentIndex;

            var initialQuestion = _configService.InitialQuestions[currentIndex];
            var actions = new List<AdaptiveAction>();

            foreach (var option in initialQuestion.Options)
            {
                actions.Add(new AdaptiveSubmitAction
                {
                    Title = option,
                    Data = new { question = $"InitialQuestion{currentIndex + 1}", answer = option }
                });
            }

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = initialQuestion.Question,
                        Wrap = true
                    }
                },
                Actions = actions
            };

            var activity = MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            });

            await stepContext.Context.SendActivityAsync(activity, cancellationToken);

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> GetValueAsync(WaterfallStepContext stepContext, int questionIndex, CancellationToken cancellationToken)
        {
            int currentIndex = (int)stepContext.Values["Index"];

            // Retrieve the value from the activity
            var value = stepContext.Context.Activity.Value;
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Retrieve the available options for the current question
            var InitialQuestions = _configService.InitialQuestions[currentIndex];
            var validOptions = InitialQuestions.Options;

            bool isValidResponse = false;

            if (value == null)
            {
                // If value is null, it means the user might have typed a response instead of clicking a button
                var userResponse = stepContext.Context.Activity.Text?.Trim().ToLower();

                if (!string.IsNullOrEmpty(userResponse) && validOptions.Contains(userResponse))
                {
                    isValidResponse = true;
                    //Save bot's question
                    userProfile.ConversationId = stepContext.Context.Activity.Conversation.Id;
                    if (userProfile.ChatHistory == null)
                    {
                        userProfile.ChatHistory = new List<ChatHistory>();
                    }
                    userProfile.ChatHistory.Add(new ChatHistory("Bot", userProfile.ConversationId, InitialQuestions.Question));
                    await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

                    //Save user respopnse
                    userProfile.ChatHistory.Add(new ChatHistory("User", userProfile.ConversationId, userResponse));
                    await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);
                }
            }
            else
            {
                // Convert the value to a JObject for easier manipulation
                var valueJObject = value as JObject;
                var answer = valueJObject?["answer"]?.ToString();

                // Check if the answer is "other" and fetch the "otherInput"
                if (answer == "other")
                {
                    answer = valueJObject?["otherInput"]?.ToString();
                }

                if (!string.IsNullOrEmpty(answer))
                {
                    isValidResponse = true;

                    userProfile.ConversationId = stepContext.Context.Activity.Conversation.Id;
                    if (userProfile.ChatHistory == null)
                    {
                        userProfile.ChatHistory = new List<ChatHistory>();
                    }
                    userProfile.ChatHistory.Add(new ChatHistory("Bot", userProfile.ConversationId, InitialQuestions.Question));
                    await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

                    userProfile.ChatHistory.Add(new ChatHistory("User", userProfile.ConversationId, answer));
                    await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);
                }
            }
            if (isValidResponse)
            {
                // If the response is valid, increment the current index and move to the next step
                currentIndex++;
                return await stepContext.NextAsync(currentIndex, cancellationToken);
            }
            else
            {
                // If the response is invalid, re-prompt the same question with the current index
                await stepContext.Context.SendActivityAsync("Please select a valid option from the buttons provided or type a valid response.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, new { Index = currentIndex }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> EndInitialQuestionsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Pass the responses back to the main dialog
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
