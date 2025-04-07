using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TG.Bot_Test;

namespace TelegramBotTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string botToken = File.ReadAllText("Token.txt");
            CancellationTokenSource cts = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.InlineQuery, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            };

            Console.WriteLine("Initilizing update router...");
            BotClientUpdateRouter updateRouter = new BotClientUpdateRouter();
            updateRouter.RegisterCommandHandler("start", Start);
            updateRouter.RegisterCommandHandler("query", InlineKeyboardCallbackQuery);

            Console.WriteLine("Initilizing bot client...");
            TelegramBotClient botClient = new TelegramBotClient(botToken, httpClient, cts.Token);
            botClient.StartReceiving(updateRouter, receiverOptions, cts.Token);

            Console.WriteLine("Getting bot information...");
            User botUser = botClient.GetMe().Result;

            Console.WriteLine("Bot \"{0}\" was started!", botUser.Username);
            Thread.Sleep(-1);
        }

        private static async void Start(ITelegramBotClient client, Chat chat, string[] args, CommandHandlerHelper helper)
        {
            await client.SendMessage(chat, "Start command invoked");
            if (args.Length > 0)
            {
                await client.SendMessage(chat, "Arguments found!");
                await client.SendMessage(chat, string.Join(";", args));
            }

            await client.SendMessage(chat, "Type your name : ");
            string? userName = helper.AwaitMessage(chat).Text;
            await client.SendMessage(chat, string.IsNullOrEmpty(userName) ? "So you have no name?" : "Hello, " + userName + "!");
        }

        private static async void InlineKeyboardCallbackQuery(ITelegramBotClient client, Chat chat, string[] args, CommandHandlerHelper helper)
        {
            InlineKeyboardMarkup keyboardMarkup1 = new InlineKeyboardMarkup(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("1", "query1"),
                InlineKeyboardButton.WithCallbackData("2", "query2"),
                InlineKeyboardButton.WithCallbackData("3", "query3")
            });

            InlineKeyboardMarkup keyboardMarkup2 = new InlineKeyboardMarkup(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("1", "query1"),
                InlineKeyboardButton.WithCallbackData("2", "query2"),
                InlineKeyboardButton.WithCallbackData("3", "query3")
            });

            InlineKeyboardMarkup keyboardMarkup3 = new InlineKeyboardMarkup(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("1", "query1"),
                InlineKeyboardButton.WithCallbackData("2", "query2"),
                InlineKeyboardButton.WithCallbackData("3", "query3")
            });

            Message optionMessage = await client.SendMessage(chat, "Select option");

            await client.EditMessageReplyMarkup(chat, optionMessage.MessageId, keyboardMarkup1);
            string? query1 = helper.AwaitCallbackQuery(chat).Data;

            await client.EditMessageReplyMarkup(chat, optionMessage.MessageId, keyboardMarkup2);
            string? query2 = helper.AwaitCallbackQuery(chat).Data;

            await client.EditMessageReplyMarkup(chat, optionMessage.MessageId, keyboardMarkup3);
            string? query3 = helper.AwaitCallbackQuery(chat).Data;

            await client.SendMessage(chat, "Selected : " + query1 + " " + query2 + " " + query3);
        }
    }
}