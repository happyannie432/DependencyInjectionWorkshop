using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _Profile;
        private readonly IHash _Hash;
        private readonly IOptService _OptService;
        private readonly INotification _Notification;
        private readonly IFailedCounter _FailedCounter;
        private readonly ILogger _Logger;

     
        public AuthenticationService()
        {
            _Profile = new ProfileDao();
            _Hash = new Sha256Adapter();
            _OptService = new OptService();
            _Notification = new SlackAdapter();
            _FailedCounter = new FailedCounter();
            _Logger = new NLogAdapter();
        }

        //alt + insert 
        public AuthenticationService(IProfile profile, IHash hash, IOptService optService, INotification notification, IFailedCounter failedCounter, ILogger logger)
        {
            _Profile = profile;
            _Hash = hash;
            _OptService = optService;
            _Notification = notification;
            _FailedCounter = failedCounter;
            _Logger = logger;
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            if (_FailedCounter.GetIsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var password = _Profile.GetPasswordFromDb(accountId);
            var hashPassword = _Hash.GetHashPassword(inputPassword);
            var currentOpt = _OptService.CurrentOpt(accountId);

            if (hashPassword != password || currentOpt != otp)
            {
                _FailedCounter.AddFailedCount(accountId);
                _Logger.Log(accountId, _FailedCounter.GetFailedCount(accountId));
                _Notification.Notify("Login Failed");
                return false;
            }

            _FailedCounter.ResetFailedCount(accountId);
            return true;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}