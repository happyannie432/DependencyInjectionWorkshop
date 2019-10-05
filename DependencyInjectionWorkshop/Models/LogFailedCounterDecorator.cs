namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCounterDecorator : BaseAuthenticationDecorator
    {
        private readonly ILogger _Logger;
        private readonly IFailedCounter _FailedCounter;

        public LogFailedCounterDecorator(IAuthentication authentication ,ILogger logger, IFailedCounter failedCounter) : base(authentication)
        {
            _Logger = logger;
            _FailedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            var isValid =  base.Verify(accountId, inputPassword, otp);
            if (isValid)
            {

                return true;
            }

            LogFailedCount(accountId);
            return false;
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _FailedCounter.GetFailedCount(accountId);
            _Logger.Log($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}