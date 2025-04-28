
namespace ServerNetworkAPI.dev.Services
{
    public class InternetConnectivityService
    {
        private static readonly string[] _testUrls =
        {
            "https://www.google.com",
            "https://www.cloudflare.com"
        };

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

        public static async Task<bool> IsInternetAvailableAsync()
        {
            using var client = new HttpClient { Timeout = _timeout };

            foreach (var url in _testUrls)
            {
                try
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignore 
                }
            }

            return false;
        }
    }
}
