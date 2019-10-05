using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtp
    {
        string GetOpt(string accountId);
    }

    public class OtpService : IOtp
    {
        public OtpService()
        {
        }

        public string GetOpt(string accountId)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId)
                .Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return response.Content.ReadAsAsync<string>().Result;
        }
    }
}