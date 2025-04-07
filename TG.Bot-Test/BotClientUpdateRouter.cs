using System.Diagnostics.CodeAnalysis;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TG.Bot_Test
{
    public class BotClientUpdateRouter : IUpdateHandler
    {
        private readonly CommandHandlerHelper commandHelper;
        private readonly List<long> busyWorkers;
        private readonly Dictionary<string, CommandHandler> commandHandlers;
        internal readonly Dictionary<long, UpdateAwaitHandle> updateAwaiters;

        public BotClientUpdateRouter()
        {
            commandHelper = new CommandHandlerHelper(this);
            commandHandlers = new Dictionary<string, CommandHandler>();
            busyWorkers = new List<long>();
            updateAwaiters = new Dictionary<long, UpdateAwaitHandle>();
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            // Something went wrong on update handling
            Console.WriteLine(exception);
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Update received
            await Task.Yield();
            Console.WriteLine("Received an update!");

            // Getting update's user ID
            if (!UpdateHasUserId(update, out long userId))
            {
                // Update's user ID not found
                Console.WriteLine("Update has no entities that might have an user ID");
                return;
            }

            // Checking if bot is laready working with this user
            if (busyWorkers.Contains(userId))
            {
                // Bot busy
                Console.WriteLine("The bot is already working with user ({0})", userId);
                return;
            }

            // Checking for awaiter registered to this ID
            if (updateAwaiters.TryGetValue(userId, out UpdateAwaitHandle? awaitHandler))
            {
                // awaiter exists, and waits for next update
                Console.WriteLine("Update's user ID is founded in awaiters ({0})", userId);
                awaitHandler.Close(update);
                return;
            }

            // Checking if update contains a message
            if (update.Message != null)
            {
                // Message handling
                Console.WriteLine("Update contains an message");

                // Checking if message has special 'BotCommand' entity on start
                if (!IsCommand(update.Message, out string? cmdVerb, out string[]? cmdArgs))
                {
                    // Message has no command entity
                    Console.WriteLine("Update does not contain a command");
                    await botClient.SendMessage(update.Message.Chat, "Message is not a command", cancellationToken: cancellationToken);
                    return;
                }
                
                // Checking if given commad exist in registered command handlers
                if (!commandHandlers.TryGetValue(cmdVerb, out CommandHandler? handler))
                {
                    // Handler not found
                    Console.WriteLine("Command \"{0}\" wasn't found in dictionary", cmdVerb);
                    await botClient.SendMessage(update.Message.Chat, "Command not recognized", cancellationToken: cancellationToken);
                    return;
                }

                // Registering busy user ID
                busyWorkers.Add(userId);

                // Executing command handler
                Console.WriteLine("Executing command \"{0}\"", cmdVerb);
                handler.Invoke(botClient, update.Message.Chat, cmdArgs, commandHelper);
                
                // Releasing user ID
                busyWorkers.Remove(userId);
                return;
            }

            // Unsupported update type
            Console.WriteLine("Unsupported update type");
        }

        public void RegisterCommandHandler(string commandVerb, CommandHandler commandHandler)
        {
            // Registering new named command handler
            commandHandlers.Add(commandVerb, commandHandler);
        }

        internal static bool IsCommand(Message message, [NotNullWhen(true)] out string? cmdVerb, [NotNullWhen(true)] out string[]? cmdArgs)
        {
            cmdVerb = null;
            cmdArgs = null;

            if (message.Entities == null)
                return false;

            if (message.Entities.Length == 0)
                return false;

            MessageEntity? cmdEntity = message.Entities.FirstOrDefault(x => x.Type == MessageEntityType.BotCommand);
            if (cmdEntity == null)
                return false;

            if (cmdEntity.Offset != 0)
                return false;

            if (cmdEntity.Length < 2)
                return false;

            if (string.IsNullOrWhiteSpace(message.Text))
                return false;

            string[] cmdSplit = message.Text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            cmdVerb = cmdSplit[0].TrimStart('/');
            cmdArgs = cmdSplit.Skip(1).ToArray();
            return true;
        }

#pragma warning disable CS8602
        internal static bool UpdateHasUserId(Update update, out long Id)
        {
            Id = update.Type switch
            {
                UpdateType.Message => update.Message.From.Id,
                UpdateType.InlineQuery => update.InlineQuery.From.Id,
                UpdateType.CallbackQuery => update.CallbackQuery.From.Id,
                _ => -1
            };

            return Id != -1;
        }
#pragma warning restore
    }
}
