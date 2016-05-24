
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using KeySndr.Common;

namespace KeySndr.Clients.Mobile.Droid
{
    [Activity(Label = "KeySndr", ParentActivity = typeof(MainActivity))]
    public class FullScreenActivity : Activity
    {
        private const string UrlKey = "URL";
        private AppPreferences preferences;
        private WebView webView;
        private string currentUrl;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            currentUrl = "file:///android_asset/index.html";
            if (Intent.Extras != null)
            {
                if (Intent.Extras.ContainsKey(UrlKey))
                    currentUrl = Intent.Extras.GetString(UrlKey);
            }
            SetContentView(Resource.Layout.Main);
            

            LoadPreferences();
            AssignWebView();
            SetupWebView();

            LoadUrl();

        }

        private void LoadPreferences()
        {
            preferences =
                AndroidAppPreferences.Create(Application.Context.GetSharedPreferences(KeySndrApplication.AppPreferencesId, FileCreationMode.Private));
        }

        private void AssignWebView()
        {
            webView = FindViewById<WebView>(Resource.Id.webview);
        }

        private void SetupWebView()
        {
            var bVersion = (int)Build.VERSION.SdkInt;
            webView.SetLayerType(bVersion >= 19 ? LayerType.Hardware : LayerType.Software, null);
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.UseWideViewPort = true;
            webView.Settings.LoadWithOverviewMode = true;
            webView.Settings.DomStorageEnabled = true;
            if (!preferences.UseCache)
            {
                webView.Settings.CacheMode = CacheModes.NoCache;
                webView.Settings.SetAppCacheEnabled(false);
            }
            webView.SetWebViewClient(new CustomWebClient(this));
        }

        private void LoadUrl()
        {
            webView.LoadUrl(currentUrl);
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