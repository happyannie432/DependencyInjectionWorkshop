using System;
using System.Net.Http;

//1. IAuthentication
//2. extract method
//3. create class for notify
//4. implement
//5. constructor 


namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string inputPassword, string otp);
    }

    public class FailedCounterDecorator : BaseAuthenticationDecorator
    {
        private IAuthentication _Authentication;
        private readonly IFailedCounter _FailedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _Authentication = authentication;
            _FailedCounter = failedCounter;
        }

        private void ResetFailedCount(string accountId)
        {
            _FailedCounter.ResetFailedCount(accountId);
        }
        
        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            var isValid = base.Verify(accountId, inputPassword, otp);
            if (isValid)
            {
                ResetFailedCount(accountId);
            }
            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _Profile;
        private readonly IHash _Hash;
        private readonly IOtpService _OtpService;
        private readonly INotification _Notification;
        private readonly IFailedCounter _FailedCounter;
        private readonly ILogger _Logger;
        private readonly FailedCounterDecorator _FailedCounterDecorator;


        public AuthenticationService()
        {
            _Profile = new ProfileDao();
            _Hash = new Sha256Adapter();
            _OtpService = new OtpService();
            _Notification = new SlackAdapter();
            _FailedCounter = new FailedCounter();
            _Logger = new NLogAdapter();
        }

        //alt + insert 
        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService, INotification notification, IFailedCounter failedCounter, ILogger logger)
        {
            _Profile = profile;
            _Hash = hash;
            _OtpService = otpService;
            _Notification = notification;
            _FailedCounter = failedCounter;
            _Logger = logger;
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            if (_FailedCounter.IsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var password = _Profile.GetPassword(accountId);
            var hashPassword = _Hash.ComputeHash(inputPassword);
            var currentOpt = _OtpService.GetOpt(accountId);

            if (hashPassword != password || currentOpt != otp)
            {
                _FailedCounter.AddFailedCount(accountId);
                int failedCount = _FailedCounter.GetFailedCount(accountId);
                _Logger.Log($"accountId:{accountId} failed times:{failedCount}");
                //_NotificationDecorator.Notify(accountId);
                return false;
            }

            //        _FailedCounterDecorator.ResetFailedCount(accountId);
            return true;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}