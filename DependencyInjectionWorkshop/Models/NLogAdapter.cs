namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Log(string message);
    }

    public class NLogAdapter : ILogger
    {
        public NLogAdapter()
        {
        }

        public void Log(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}