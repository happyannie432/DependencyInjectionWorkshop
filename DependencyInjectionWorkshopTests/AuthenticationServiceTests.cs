using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;
using System;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string _DefaultHashedPassword = "my hashed password";
        private const string _DefaultAccount = "joey";
        private const string _DefaultOtp = "123";
        private const string _DefaultPassword = "abc";
        private const int _DefaultFailedCount = 3;
        private IProfile _ProfileDao;
        private IOtpService _Otp;
        private IHash _Hash;
        private IFailedCounter _FailCounter;
        private INotification _Notification;
        private ILogger _Logger;
        private AuthenticationService _AuthenticationService;

        [SetUp]
        public void Setup()
        {
            _ProfileDao = Substitute.For<IProfile>();
            _Otp = Substitute.For<IOtpService>();
            _Hash = Substitute.For<IHash>();
            _FailCounter = Substitute.For<IFailedCounter>();
            _Notification = Substitute.For<INotification>();
            _Logger = Substitute.For<ILogger>();
            _AuthenticationService = new AuthenticationService(_ProfileDao, _Hash, _Otp, _Notification, _FailCounter, _Logger);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);

            GivenHash(_DefaultPassword, _DefaultHashedPassword);

            GivenOtp(_DefaultAccount, _DefaultOtp);

            ShouldBeValid(_DefaultAccount, _DefaultPassword, _DefaultOtp);
        }

        private bool WhenValid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);

            GivenHash(_DefaultPassword, _DefaultHashedPassword);

            GivenOtp(_DefaultAccount, _DefaultOtp);

            var isValid = _AuthenticationService.Verify(_DefaultAccount, _DefaultPassword, _DefaultOtp);
            return isValid;
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);

            GivenHash(_DefaultPassword, _DefaultHashedPassword);

            GivenOtp(_DefaultAccount, _DefaultOtp);

            ShouldBeInvalid(_DefaultAccount, _DefaultPassword, "wrong otp");
        }

        private bool WhenInvalid()
        {
            GivenPassword(_DefaultAccount, _DefaultHashedPassword);

            GivenHash(_DefaultPassword, _DefaultHashedPassword);

            GivenOtp(_DefaultAccount, _DefaultOtp);

            return _AuthenticationService.Verify(_DefaultAccount, _DefaultPassword, "wrong otp");
        }

        [Test]
        public void should_notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(_DefaultAccount);
        }

        [Test]
        public void should_reset_failedcount_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount(_DefaultAccount);
        }

        [Test]
        public void should_add_failedcount_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount(_DefaultAccount);
        }

        [Test]
        public void account_is_locked()
        {
            WhenAccountIsLocked(true);

            ShouldThrow<FailedTooManyTimesException>();
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(_DefaultFailedCount);
            WhenInvalid();
            ShouldLogFailedCount(_DefaultAccount, _DefaultFailedCount);

        }

        private void ShouldLogFailedCount(string account, int failedCount)
        {
            string accountId = Arg.Is<string>(s => s.Contains(account) && s.Contains(failedCount.ToString()));
            _Logger.Received(1)
                .Log(accountId);
        }

        private void GivenFailedCount(int failedCount)
        {
            _FailCounter.GetFailedCount(Arg.Is<string>(s => s.Contains(_DefaultAccount))).Returns(failedCount);
        }

        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => _AuthenticationService.Verify(_DefaultAccount, _DefaultHashedPassword, _DefaultOtp);

            Assert.Throws<TException>(action);
        }

        private void WhenAccountIsLocked(bool isLocked)
        {
            _FailCounter.IsAccountLocked(_DefaultAccount).Returns(isLocked);
        }


        private void ShouldAddFailedCount(string account)
        {
            _FailCounter.Received().AddFailedCount(Arg.Is<string>(s => s.Contains(account)));
        }

        private void ShouldResetFailedCount(string account)
        {
            _FailCounter.Received(1).ResetFailedCount(Arg.Is<string>(a => a.Contains(account)));
        }

        private void ShouldNotify(string account)
        {
            _Notification.Received(1).Notify(Arg.Is<string>(m => m.Contains(account)));
        }

        private void ShouldBeValid(string account, string password, string otp)
        {
            var isValid = _AuthenticationService.Verify(account, password, otp);

            Assert.IsTrue(isValid);
        }
        private void ShouldBeInvalid(string account, string password, string otp)
        {
            var isValid = _AuthenticationService.Verify(account, password, otp);

            Assert.IsFalse(isValid);
        }

        private void GivenHash(string password, string hashedPassword)
        {
            _Hash.ComputeHash(password).Returns(hashedPassword);
        }

        private void GivenOtp(string account, string otp)
        {
            _Otp.GetOpt(account).Returns(otp);
        }

        private void GivenPassword(string account, string hashedPassword)
        {
            _ProfileDao.GetPassword(account).Returns(hashedPassword);
        }
    }
}