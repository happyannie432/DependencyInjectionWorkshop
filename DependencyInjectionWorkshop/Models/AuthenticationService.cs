using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounter
    {
        public FailedCounter()
        {
        }

        public void AddFailedCount(string accountId)
        {
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OptService _optService;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCounter _failedCounter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _optService = new OptService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
        }

        public bool Verify(string accountId, string inputPassword, string otp)
        {
            if (GetIsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var password = _profileDao.GetPasswordFromDb(accountId);
            var hashPassword = _sha256Adapter.GetHashPassword(inputPassword);
            var currentOpt = _optService.CurrentOpt(accountId);

            if (hashPassword != password || currentOpt != otp)
            {
                _failedCounter.AddFailedCount(accountId);
                GetFailedCount(accountId);
                _slackAdapter.Notify("Login Failed");

                return false;
            }

            ResetFailedCount(accountId);
            return true;
        }


        //after extract it show static means there's no dependency with current instance (class)
        private static bool GetIsAccountLocked(string accountId)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isAccountLocked;
        }

        private static void ResetFailedCount(string accountId)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
         
        //move instance method
        private  void GetFailedCount(string accountId)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        //extract class on this
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}