namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Log(string accountId, int failedCount);
    }

    public class NLogAdapter : ILogger
    {
        public NLogAdapter()
        {
        }

        public void Log(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}