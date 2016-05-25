using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace KeySndr.Clients.Mobile.Droid
{
    [Activity(Label = "KeySndr", ParentActivity = typeof(MainActivity), LaunchMode = LaunchMode.SingleTop)]
    public class FullScreenActivity : KeysndrBaseActivity
    {
        private const string UrlKey = "URL";
        private const string OrientationKey = "Orientation";
        private string currentUrl;

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            base.OnCreate(bundle);
            
            currentUrl = "file:///android_asset/index.html";
            if (Intent.Extras != null)
                if (Intent.Extras.ContainsKey(UrlKey))
                    currentUrl = Intent.Extras.GetString(UrlKey);
            LoadUrl(); 
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!Intent.Extras.ContainsKey(OrientationKey))
                return;

            var mode = Intent.Extras.GetString(OrientationKey).ToLower();
            if (mode == "landscape" || mode == "portrait")
                RequestedOrientation = mode == "landscape"
                    ? ScreenOrientation.Landscape
                    : ScreenOrientation.Portrait;
        }

        private void LoadUrl()
        {
            WebView.LoadUrl(currentUrl);
        }

        public override void OnBackPressed()
        {
            Finish();
            base.OnBackPressed();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_activity_actions, menu);
            return base.OnCreateOptionsMenu(menu);
        }
    }
}