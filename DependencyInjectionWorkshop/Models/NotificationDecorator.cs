namespace DependencyInjectionWorkshop.Models
{

    public class BaseAuthenticationDecorator : IAuthentication
    {
        private readonly IAuthentication _Authentication;

        public BaseAuthenticationDecorator(IAuthentication authentication)
        {
            _Authentication = authentication;
        }

        public virtual bool Verify(string accountId, string inputPassword, string otp)
        {
            return _Authentication.Verify(accountId, inputPassword, otp);
        }
    }


    public class NotificationDecorator : BaseAuthenticationDecorator
    {
        private readonly INotification _Notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification) : base(authenticationService)
        {
            _Notification = notification;
        }

        public void Notify(string accountId)
        {
            _Notification.Notify($"{accountId} Login Failed");
        }

        
        public override bool Verify(string accountId, string inputPassword, string otp)
        {
            var isValid = base.Verify(accountId, inputPassword, otp);
            if (!isValid)
            {
                Notify(accountId);
                return false;
            }
            return true;
        }
    }
}