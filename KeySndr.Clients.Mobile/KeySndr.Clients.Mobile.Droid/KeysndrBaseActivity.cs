using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Webkit;
using KeySndr.Common;

namespace KeySndr.Clients.Mobile.Droid
{
    public class KeysndrBaseActivity : Activity
    {
        protected WebView WebView;
        protected AppPreferences Preferences;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            LoadPreferences();
            AssignWebView();
            SetupWebView();
        }

        protected override void OnResume()
        {
            base.OnResume();
            LoadPreferences();
        }

        protected void SetLandScapeOrientation()
        {
            RequestedOrientation = ScreenOrientation.Landscape;
        }

        protected void SetPortraitOrientation()
        {
            RequestedOrientation = ScreenOrientation.Portrait;
        }

        protected void SetUserOrientation()
        {
            RequestedOrientation = ScreenOrientation.User;
        }

        private void AssignWebView()
        {
            WebView = FindViewById<WebView>(Resource.Id.webview);
        }

        private void SetupWebView()
        {
            var bVersion = (int)Build.VERSION.SdkInt;
            WebView.SetLayerType(bVersion >= 19 ? LayerType.Hardware : LayerType.Software, null);
            WebView.Settings.JavaScriptEnabled = true;
            WebView.Settings.UseWideViewPort = true;
            WebView.Settings.LoadWithOverviewMode = true;
            WebView.Settings.DomStorageEnabled = true;
            WebView.Settings.SetRenderPriority(WebSettings.RenderPriority.High);
            if (!Preferences.UseCache)
            {
                WebView.Settings.CacheMode = CacheModes.NoCache;
                WebView.Settings.SetAppCacheEnabled(false);
            }
            WebView.SetWebViewClient(new CustomWebClient(this));
        }

        private void LoadPreferences()
        {
            Preferences =
                AndroidAppPreferences.Create(Application.Context.GetSharedPreferences(KeySndrApplication.AppPreferencesId, FileCreationMode.Private));
        }
    }
}