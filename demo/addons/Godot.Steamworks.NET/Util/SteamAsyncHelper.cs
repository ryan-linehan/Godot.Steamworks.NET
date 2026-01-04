#if GODOT_PC
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
#else
// Stub implementation for non-desktop platforms
namespace Godot.Steamworks.Net.Util;

/// <summary>
/// Stub helper class for non-desktop platforms.
/// </summary>
public static class SteamAsyncHelper
{
    // This class is intentionally empty on non-desktop platforms.
}
#endif
