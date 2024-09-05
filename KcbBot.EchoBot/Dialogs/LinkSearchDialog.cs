using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using System.Threading;
using System;
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
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Here is the link: https://prothom-alo.com"), cancellationToken);
            }
            else if (option.Equals("search", StringComparison.OrdinalIgnoreCase))
            {
                // Prompt the user for search text
                return await stepContext.BeginDialogAsync(nameof(TextPrompt), null, cancellationToken);
            }
            else if (option is string searchText && !string.IsNullOrEmpty(searchText))
            {
                // Call the API for search results
                var searchResult = await CallSearchApi(searchText);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Search results: {searchResult}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }

        private async Task<string> CallSearchApi(string searchText)
        {
            var searchUrl = $"https://seartecxt/{searchText}";
            // Implement your HTTP request logic here
            // For example, using HttpClient to call the API:
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(searchUrl);
            return response;
        }
    }
}
