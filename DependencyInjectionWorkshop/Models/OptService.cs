using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OptService
    {
        public OptService()
        {
        }

        public string CurrentOpt(string accountId)
        {
            var response = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/otps", accountId)
                .Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            return response.Content.ReadAsAsync<string>().Result;
        }
    }
}