using Telegram.Bot.Types;

namespace TG.Bot_Test
{
    public class UpdateAwaitHandle : IDisposable
    {
        private readonly ManualResetEvent resetEvent;
        private bool disposed;
        private Update? result;

        public UpdateAwaitHandle()
        {
            /*
             This class is designed to freeze the execution of a command until it receives a new update.
             Using ManualResetEvent we can block the thread in which the command handler is executed
             until the router gives a signal that an update for this ID has been found
            */

            resetEvent = new ManualResetEvent(false);
        }

        public Update Enter()
        {
            resetEvent.WaitOne();
            if (result == null)
                throw new ArgumentNullException("Resulting update");

            return result;
        }

        public void Close(Update update)
        {
            result = update;
            resetEvent.Set();
            Dispose();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            resetEvent.Dispose();
            GC.SuppressFinalize(this);
            disposed = true;
        }
    }
}
