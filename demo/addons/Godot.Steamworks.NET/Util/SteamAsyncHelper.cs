using System.Threading.Tasks;
using Steamworks;

namespace Godot.Steamworks.Net.Util;
/// <summary>
/// Helper class to convert Steam CallResults to async/await pattern
/// </summary>
public static class SteamAsyncHelper
{
    public static Task<(T result, bool ioFailure)> CallAsync<T>(SteamAPICall_t apiCall) where T : struct
    {
        var tcs = new TaskCompletionSource<(T, bool)>();
        var callResult = CallResult<T>.Create((result, ioFailure) =>
        {
            tcs.SetResult((result, ioFailure));
        });
        callResult.Set(apiCall);
        return tcs.Task;
    }
}