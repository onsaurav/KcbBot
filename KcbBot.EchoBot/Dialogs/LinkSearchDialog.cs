using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using System.Threading;
using System;
using AdaptiveCards;
using KcbBot.EchoBot.Helper;
using KcbBot.EchoBot.Common;
using System.Net.Http;

namespace KcbBot.EchoBot.Dialogs
{
    public class LinkSearchDialog : ComponentDialog
    {
        public LinkSearchDialog() : base(nameof(LinkSearchDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptionsStepAsync,
                HandleOptionSelectionStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ShowOptionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Please choose an option:");
            var buttons = new[]
            {
                new CardAction(ActionTypes.ImBack, "Link", value: "link"),
                new CardAction(ActionTypes.ImBack, "Search", value: "search")
            };
            var replyWithButtons = MessageFactory.Carousel(new[]
            {
                new HeroCard
                {
                    Buttons = buttons
                }.ToAttachment()
            });

            await stepContext.Context.SendActivityAsync(replyWithButtons, cancellationToken);
            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> HandleOptionSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var option = stepContext.Result.ToString();

            if (option.Equals("link", StringComparison.OrdinalIgnoreCase))
            {
                var linkTest = @"https://www.prothomalo.com";
                string cardJson = JsonHelper.ReadCardJson(Metadata.LinkCard);
                cardJson = cardJson.Replace("{linkUrl}", linkTest);
                var linkCard = AdaptiveCard.FromJson(cardJson).Card;

                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = linkCard
                }), cancellationToken);
            }
            else if (option.Equals("search", StringComparison.OrdinalIgnoreCase))
            {
                // Prompt the user for search text
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your search text:"), }, cancellationToken);
            }
            else
            {
                // Handle the search result
                var searchText = option;
                var searchResult = ""; // await CallSearchApi(searchText);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Search results: {searchResult}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<string> CallSearchApi(string searchText)
        {
            var searchUrl = $"https://seartecxt/{searchText}";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(searchUrl);
            return response;
        }
    }
}
