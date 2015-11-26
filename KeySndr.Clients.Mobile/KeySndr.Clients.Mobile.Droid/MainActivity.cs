using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Views;
using Android.OS;
using Android.Webkit;
using Java.Lang;
using KeySndr.Clients.Mobile.Droid.dialogs;
using KeySndr.Clients.Mobile.Droid.events;
using KeySndr.Common;
using KeySndr.Common.Providers;

namespace KeySndr.Clients.Mobile.Droid
{
	[Activity (Label = "KeySndrWeb", MainLauncher = true, Icon = "@mipmap/ic_launcher", LaunchMode = LaunchMode.SingleTop)]
	public class MainActivity : Activity
	{
	    private const string UrlKey = "URL";

	    private WebView webView;
        private IWebConnectionProvider webConnection;
        private AppPreferences preferences;
        private InputConfigurationsSelectDialog configurationsSelectDialog;
        private InputConfiguration inputConfiguration;
	    private string currentUrl;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
            SetContentView(Resource.Layout.Main);
            currentUrl = "file:///android_asset/index.html";
            
            if (bundle != null && bundle.ContainsKey(UrlKey))
                currentUrl = bundle.GetString(UrlKey);

            LoadPreferences();
            AssignWebView();
            SetupWebView();
            CheckFirstRun();

            LoadUrl();
        }

	    protected override void OnResume()
	    {
	        base.OnResume();
            LoadPreferences();
            webConnection = ObjectFactory.GetProvider<IWebConnectionProvider>();
            if (!string.IsNullOrEmpty(preferences.Ip))
            {
                webConnection.SetBaseAddress(preferences.Ip, preferences.Port);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(UrlKey, currentUrl);
        }

        protected override void OnDestroy()
	    {
	        if (configurationsSelectDialog != null && configurationsSelectDialog.IsShowing)
                configurationsSelectDialog.Dismiss();
            base.OnDestroy();
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
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.UseWideViewPort = true;
            webView.Settings.LoadWithOverviewMode = true;
            webView.Settings.CacheMode = CacheModes.NoCache;
            webView.Settings.SetAppCacheEnabled(false);
        }

	    private void LoadUrl()
	    {
	        webView.LoadUrl(currentUrl);
	    }

        private void OpenSettings()
        {
            var intent = new Intent(this, typeof(SettingsActivity));
            intent.PutExtra("search", true);
            StartActivity(intent);
        }

        private void CheckFirstRun()
        {
            if (preferences.FirtsTimeRunning)
                OpenSettings();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_activity_actions, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_load_confs:
                    SendRequestForConfigurations();
                    break;
                case Resource.Id.action_settings:
                    OpenSettings();
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void SendRequestForConfigurations()
        {
            webConnection.RequestConfigurations()
                .ContinueWith(RequestedConfigurationsReceived);
        }

        private void RequestedConfigurationsReceived(Task<ApiResult<IEnumerable<string>>> task)
        {
            var apiResult = task.Result;
            RunOnUiThread(() =>
            {
                configurationsSelectDialog = new InputConfigurationsSelectDialog(this, apiResult.Content);
                configurationsSelectDialog.OnSelectConfiguration +=
                    delegate (object sender, OnSelectConfigurationArgs args)
                    {
                        configurationsSelectDialog.Dismiss();
                        webConnection.RequestConfiguration(args.Name)
                            .ContinueWith(RequestedConfigurationReceived);
                    };
                configurationsSelectDialog.Show();
            });
        }

        private void RequestedConfigurationReceived(Task<ApiResult<InputConfiguration>> task)
        {
            var apiResult = task.Result;
            inputConfiguration = apiResult.Content;
            currentUrl = inputConfiguration.HasView 
                ? $"http://{preferences.Ip}:{preferences.Port}/Views/{inputConfiguration.View}/index.html" 
                : $"http://{preferences.Ip}:{preferences.Port}/manage/play-grid.html?name={inputConfiguration.Name}";

            RunOnUiThread(new Runnable(SetupView));
        }

        private void SetupView()
        {  
            LoadUrl(); 
        }
    }
}


