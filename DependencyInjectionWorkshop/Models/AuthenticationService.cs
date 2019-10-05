using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _ProfileDao;
        private readonly Sha256Adapter _Sha256Adapter;
        private readonly OptService _OptService;
        private readonly SlackAdapter _SlackAdapter;
        private readonly FailedCounter _FailedCounter;
        private readonly NLogAdapter _NLogAdapter;

     
        public AuthenticationService()
        {
            _ProfileDao = new ProfileDao();
            _Sha256Adapter = new Sha256Adapter();
            _OptService = new OptService();
            _SlackAdapter = new SlackAdapter();
            _FailedCounter = new FailedCounter();
            _NLogAdapter = new NLogAdapter();
        }
        //alt + insert 
        public AuthenticationService(ProfileDao profileDao, Sha256Adapter sha256Adapter, OptService optService, SlackAdapter slackAdapter, FailedCounter failedCounter, NLogAdapter nLogAdapter)
        {
            _ProfileDao = profileDao;
            _Sha256Adapter = sha256Adapter;
            _OptService = optService;
            _SlackAdapter = slackAdapter;
            _FailedCounter = failedCounter;
            _NLogAdapter = nLogAdapter;
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            if (_FailedCounter.GetIsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var password = _ProfileDao.GetPasswordFromDb(accountId);
            var hashPassword = _Sha256Adapter.GetHashPassword(inputPassword);
            var currentOpt = _OptService.CurrentOpt(accountId);

            if (hashPassword != password || currentOpt != otp)
            {
                _FailedCounter.AddFailedCount(accountId);
                LogFailedCount(accountId);
                _SlackAdapter.Notify("Login Failed");
                return false;
            }

            _FailedCounter.ResetFailedCount(accountId);
            return true;
        }

        private  void LogFailedCount(string accountId)
        {
            int failedCount = _FailedCounter.FailedCount(accountId);
            _NLogAdapter.Log(accountId, failedCount);
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}