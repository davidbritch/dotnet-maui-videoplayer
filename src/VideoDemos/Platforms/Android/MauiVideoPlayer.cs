using Android.Content;
using Android.Views;
using Android.Widget;
using VideoDemos.Controls;
using Uri = Android.Net.Uri;

namespace VideoDemos.Platforms.Android
{
    public class MauiVideoPlayer : RelativeLayout
    {
        VideoView _videoView;
        MediaController _mediaController;    // Used to display transport controls
        bool _isPrepared;
        Context _context;
        IVideo _video;

        public MauiVideoPlayer(Context context, IVideo video) : base(context)
        {
            _context = context;
            _video = video;

            // Save the VideoView for future reference
            _videoView = new VideoView(context);

            // Put the VideoView in a RelativeLayout
            AddView(_videoView);

            // Center the VideoView in the RelativeLayout
            LayoutParams layoutParams =
                new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            layoutParams.AddRule(LayoutRules.CenterInParent);
            _videoView.LayoutParameters = layoutParams;

            // Handle events
            _videoView.Prepared += OnVideoViewPrepared;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _videoView.Prepared -= OnVideoViewPrepared;
                _videoView.Dispose();
                _videoView = null;
                _video = null;
            }

            base.Dispose(disposing);
        }

        public void UpdateTransportControlsEnabled()
        {
            if (_video.AreTransportControlsEnabled)
            {
                _mediaController = new MediaController(_context);
                _mediaController.SetMediaPlayer(_videoView);
                _videoView.SetMediaController(_mediaController);
            }
            else
            {
                _videoView.SetMediaController(null);
                if (_mediaController != null)
                {
                    _mediaController.SetMediaPlayer(null);
                    _mediaController = null;
                }
            }
        }

        public void UpdateSource()
        {
            _isPrepared = false;
            bool hasSetSource = false;

            if (_video.Source is UriVideoSource)
            {
                string uri = (_video.Source as UriVideoSource).Uri;
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    _videoView.SetVideoURI(Uri.Parse(uri));
                    hasSetSource = true;
                }
            }
            else if (_video.Source is FileVideoSource)
            {
                string filename = (_video.Source as FileVideoSource).File;
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    _videoView.SetVideoPath(filename);
                    hasSetSource = true;
                }
            }
            else if (_video.Source is ResourceVideoSource)
            {
                string package = Context.PackageName;
                string path = (_video.Source as ResourceVideoSource).Path;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    string assetFilePath = "content://" + package + "/" + path;
                    _videoView.SetVideoPath(assetFilePath);
                    hasSetSource = true;
                }
            }

            if (hasSetSource && _video.AutoPlay)
            {
                _videoView.Start();
            }
        }

        public void UpdatePosition()
        {
            if (Math.Abs(_videoView.CurrentPosition - _video.Position.TotalMilliseconds) > 1000)
            {
                _videoView.SeekTo((int)_video.Position.TotalMilliseconds);
            }
        }

        void OnVideoViewPrepared(object sender, EventArgs args)
        {
            _isPrepared = true;
            ((IVideoController)_video).Duration = TimeSpan.FromMilliseconds(_videoView.Duration);
        }

        public void UpdateStatus()
        {
            VideoStatus status = VideoStatus.NotReady;

            if (_isPrepared)
            {
                status = _videoView.IsPlaying ? VideoStatus.Playing : VideoStatus.Paused;
            }

            ((IVideoController)_video).Status = status;

            // Set Position property
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(_videoView.CurrentPosition);
            _video.Position = timeSpan;
        }

        public void PlayRequested(TimeSpan position)
        {
            _videoView.Start();
            System.Diagnostics.Debug.WriteLine($"Video playback from {position.Hours:X2}:{position.Minutes:X2}:{position.Seconds:X2}.");
        }

        public void PauseRequested(TimeSpan position)
        {
            _videoView.Pause();
            System.Diagnostics.Debug.WriteLine($"Video paused at {position.Hours:X2}:{position.Minutes:X2}:{position.Seconds:X2}.");
        }

        public void StopRequested(TimeSpan position)
        {
            // Stops and releases the media player
            _videoView.StopPlayback();
            System.Diagnostics.Debug.WriteLine($"Video stopped at {position.Hours:X2}:{position.Minutes:X2}:{position.Seconds:X2}.");

            // Ensure the video can be played again
            _videoView.Resume();
        }
    }
}
