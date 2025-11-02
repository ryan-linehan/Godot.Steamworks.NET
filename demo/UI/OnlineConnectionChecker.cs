using System;
using System.Net.Http;
using System.Threading.Tasks;

public static class OnlineConnectionChecker
{
    public static bool IsOnline { get; private set; } = false;
    private static HttpClient httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(1),
        BaseAddress = new Uri("https://google.com")
    };
    public static async Task RefreshOnlineStatus()
    {
        try
        {
            var result = await httpClient.GetAsync("");    
            IsOnline = result.IsSuccessStatusCode;
        }
        catch
        {
            IsOnline = false;
        }
    }
}