using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KcbBot.EchoBot.Services;
using KcbBot.EchoBot.Model;
using KcbBot.EchoBot.Model.Data;
using Microsoft.Extensions.FileSystemGlobbing.Internal;


namespace KcbBot.EchoBot.Dialogs
{
    public class ExternalApiDialog : ComponentDialog
    {
        private readonly ExternalApiService _externalApiService;
        private readonly BotService _botService;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public ExternalApiDialog(BotService botService, ExternalApiService externalApiService, UserState userState)
            : base(nameof(ExternalApiDialog))
        {
            _botService = botService;
            _externalApiService = externalApiService;
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                AskUserQueryAsync,
                ProcessUserInputAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskUserQueryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Check if there is a textResponse in the context options
            if (stepContext.Options is string textResponse && !string.IsNullOrEmpty(textResponse))
            {

                if (textResponse.Contains("image") || textResponse.Contains("video"))
                {
                    string url = (textResponse.Contains("image") ? 
                        @"http://localhost:3978/images/sample/sample.jpg": 
                        @"http://localhost:3978/images/sample/sample.mp4");
                    var card = CreateCompositeImgVidCard(new List<string> { url });

                    await SendAdaptiveCard(stepContext.Context, card, cancellationToken);
                }
                else
                {
                    if (textResponse.Contains(".jpg") || textResponse.Contains(".gif") || textResponse.Contains(".png") || textResponse.Contains(".mp4"))
                    {
                        var urls = ExtractUrls(textResponse);
                        var card = CreateCompositeImgVidCard(urls);

                        await SendAdaptiveCard(stepContext.Context, card, cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(textResponse, cancellationToken: cancellationToken);
                    }
                }
                // Prompt the user again after sending the adaptive card
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(""),
                };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
            else
            {
                var greeting2 = _botService.GetRandomGreeting2();
                var card = CreateGreetingCard(greeting2);

                // Create the attachment
                var attachment = new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };

                // Send the Adaptive Card to the user
                var activity = MessageFactory.Attachment(attachment);
                await stepContext.Context.SendActivityAsync(activity, cancellationToken);

                // Store bot message in user state
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                userProfile.ConversationId = stepContext.Context.Activity.Conversation.Id;
                if (userProfile.ChatHistory == null)
                {
                    userProfile.ChatHistory = new List<ChatHistory>();
                }
                userProfile.ChatHistory.Add(new ChatHistory("Bot", userProfile.ConversationId, greeting2.Text)); // Store bot message
                await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

                // Return Dialog.EndOfTurn to wait for the user's response
                return Dialog.EndOfTurn;
            }
        }
        private async Task<DialogTurnResult> ProcessUserInputAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInput = (string)stepContext.Result;

            //this line must be changed
            return await stepContext.ReplaceDialogAsync(InitialDialogId, userInput, cancellationToken);

            if (userInput.Equals("end", StringComparison.OrdinalIgnoreCase))
            {
                // End the dialog
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }


            var chatId = stepContext.Context.Activity.Conversation.Id;

            try
            {
                var response = await _externalApiService.CallPredictionApiAsync(userInput, chatId);
                var jsonResponse = JObject.Parse(response);
                var textResponse = jsonResponse["text"]?.ToString() ?? "";

                // Store user message in user state
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                userProfile.User = stepContext.Context.Activity.From.Name;
                userProfile.ConversationId = stepContext.Context.Activity.Conversation.Id;
                if (userProfile.ChatHistory == null)
                {
                    userProfile.ChatHistory = new List<ChatHistory>();
                }
                userProfile.ChatHistory.Add(new ChatHistory(userProfile.User, userProfile.ConversationId, userInput)); // Store user message

                // Store bot message in user state
                userProfile.ChatHistory.Add(new ChatHistory("Bot", userProfile.ConversationId, textResponse)); // Store bot message
                await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);
                // Replace the dialog to AskUserQueryAsync to show the card
                return await stepContext.ReplaceDialogAsync(InitialDialogId, textResponse, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling external API: {ex.Message}");
                await stepContext.Context.SendActivityAsync($"Error calling external API: {ex.Message}", cancellationToken: cancellationToken);
                // Optionally, you can end the dialog here if an error occurs
                // return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                // For simplicity, continue the dialog flow
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
        }

        private AdaptiveCard CreateAdaptiveCard(string textResponse, string url = "")
        {
            var imageUrl = (string.IsNullOrEmpty(url)? "https://static.vecteezy.com/system/resources/previews/003/005/221/original/api-application-programming-interface-illustration-vector.jpg": url);

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveImage
                    {
                        Url = new Uri(imageUrl)
                    },
                    new AdaptiveTextBlock
                    {
                        Text = textResponse,
                        Wrap = true
                    }
                }
            };

            return card;
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

        private async Task SendAdaptiveCard(ITurnContext turnContext, AdaptiveCard card, CancellationToken cancellationToken)
        {
            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            var activity = MessageFactory.Attachment(attachment);
            await turnContext.SendActivityAsync(activity, cancellationToken);
        }
        private AdaptiveCard CreateCompositeImgVidCard(IEnumerable<string> urls)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 3));
            foreach (var url in urls)
            {
                if (url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    var filename = ExtractFilename(url);
                    // Add the text block with the filename
                    card.Body.Add(new AdaptiveTextBlock
                    {
                        Text = filename,
                        Wrap = true
                    });
                    card.Body.Add(new AdaptiveImage
                    {
                        Url = new Uri(url),
                        AltText = filename
                    });
                }
                else if (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    var filename = ExtractFilename(url);
                    // Add the text block with the filename
                    card.Body.Add(new AdaptiveTextBlock
                    {
                        Text = filename,
                        Wrap = true
                    });
                    card.Body.Add(new AdaptiveMedia
                    {
                        Sources = new List<AdaptiveMediaSource>
                {
                    new AdaptiveMediaSource
                    {
                        Url = url,
                        MimeType = "video/mp4"
                    }
                },
                        AltText = filename
                    });
                }
            }

            return card;
        }
        private string ExtractFilename(string url)
        {
            // Extract the filename from the URL
            var filename = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                // Decode the URL to handle URL-encoded characters
                var decodedUrl = Uri.UnescapeDataString(url);

                // Split the URL by the first whitespace to handle filenames with spaces
                var parts = decodedUrl.Split(new[] { ' ' }, 2);
                if (parts.Length > 1)
                {
                    // Extract the filename part and remove the extension
                    var filenameWithExtension = parts[1].TrimEnd(')');
                    filename = Path.GetFileNameWithoutExtension(filenameWithExtension);
                }
            }
            return filename;
        }

        private IEnumerable<string> ExtractUrls(string textResponse)
        {
            var regex = new Regex(@"https?:\/\/[^\s\)]+(?:\s+[^\s\)]+)*");
            var matches = regex.Matches(textResponse);
            return matches.Select(m => m.Value);
        }
    }
}