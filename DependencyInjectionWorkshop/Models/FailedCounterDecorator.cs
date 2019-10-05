namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : BaseAuthenticationDecorator
    {
        private readonly IFailedCounter _FailedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _FailedCounter = failedCounter;
        }

        private void ResetFailedCount(string accountId)
        {
            _FailedCounter.ResetFailedCount(accountId);
        }
        
        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            CheckIsAccountLock(accountId);
            var isValid = base.Verify(accountId, inputPassword, otp);
            if (isValid)
            {
                ResetFailedCount(accountId);
            }
            else
            {
                AddFailedCount(accountId);
            }

            //ctr shift enter to show else case
            return isValid;
        }

        private void AddFailedCount(string accountId)
        {
            _FailedCounter.AddFailedCount(accountId);
        }

        private void CheckIsAccountLock(string accountId)
        {
            if (_FailedCounter.IsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }
    }
}