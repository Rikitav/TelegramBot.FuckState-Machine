using Telegram.Bot;
using Telegram.Bot.Types;

namespace TG.Bot_Test
{
    public delegate void CommandHandler(ITelegramBotClient client, Chat chat, string[] args, CommandHandlerHelper helper);

    public class CommandHandlerHelper
    {
        private readonly BotClientUpdateRouter updateRouter;

        internal CommandHandlerHelper(BotClientUpdateRouter router)
        {
            updateRouter = router;
        }

        public Update AwaitUpdate(Chat fromChat)
        {
            // Registering awaiter in update router
            Console.WriteLine("Await requested");
            UpdateAwaitHandle handler = new UpdateAwaitHandle();
            updateRouter.updateAwaiters.Add(fromChat.Id, handler);

            // Awaiter for router to unlock handle
            Update awaitUpdate = handler.Enter();

            // Unregistering awaiter
            updateRouter.updateAwaiters.Remove(fromChat.Id);
            return awaitUpdate;
        }

        public Message AwaitMessage(Chat fromChat)
        {
            // Registering awaiter in update router
            Console.WriteLine("Message await requested");
            UpdateAwaitHandle handler = new UpdateAwaitHandle();
            updateRouter.updateAwaiters.Add(fromChat.Id, handler);

            // Awaiter for router to unlock handle
            Update awaitUpdate = handler.Enter();

            // Checking update for expecting type
            while (awaitUpdate.Message == null)
                awaitUpdate = handler.Enter();

            // Unregistering awaiter
            updateRouter.updateAwaiters.Remove(fromChat.Id);
            return awaitUpdate.Message;
        }

        public CallbackQuery AwaitCallbackQuery(Chat fromChat)
        {
            // Registering awaiter in update router
            Console.WriteLine("Message await requested");
            UpdateAwaitHandle handler = new UpdateAwaitHandle();
            updateRouter.updateAwaiters.Add(fromChat.Id, handler);

            // Awaiter for router to unlock handle
            Update awaitUpdate = handler.Enter();

            // Checking update for expecting type
            while (awaitUpdate.CallbackQuery == null)
                awaitUpdate = handler.Enter();

            // Unregistering awaiter
            updateRouter.updateAwaiters.Remove(fromChat.Id);
            return awaitUpdate.CallbackQuery;
        }
    }
}
