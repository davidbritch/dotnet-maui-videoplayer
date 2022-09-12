#if IOS || MACCATALYST
using PlatformView = VideoPlayback.Platforms.MaciOS.MauiVideoPlayer;
#elif ANDROID
using PlatformView = VideoPlayback.Platforms.Android.MauiVideoPlayer;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif
using VideoPlayback.Controls;

namespace VideoPlayback.Handlers
{
    public interface IVideoHandler : IViewHandler
    {
        new IVideo VirtualView { get; }
        new PlatformView PlatformView { get; }
    }
}
