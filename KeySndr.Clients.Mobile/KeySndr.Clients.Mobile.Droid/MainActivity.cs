using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
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
	    private WebView webView;
        private IWebConnectionProvider webConnection;
        private AppPreferences preferences;
        private InputConfigurationsSelectDialog configurationsSelectDialog;
        private InputConfiguration inputConfiguration;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
            webView = FindViewById<WebView>(Resource.Id.webview);
            LoadPreferences();
            SetupWebView();
            CheckFirstRun();
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

	    private void LoadPreferences()
        {
            preferences =
                AndroidAppPreferences.Create(Application.Context.GetSharedPreferences(KeySndrApplication.AppPreferencesId, FileCreationMode.Private));
        }

        private void SetupWebView()
	    {
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.UseWideViewPort = true;
            webView.Settings.LoadWithOverviewMode = true;
            webView.Settings.CacheMode = CacheModes.NoCache;
            webView.Settings.SetAppCacheEnabled(false);
        }

	    private void LoadViewUrl()
	    {
            webView.LoadUrl($"http://{preferences.Ip}:{preferences.Port}/{inputConfiguration.View}/index.html");
        }

        private void LoadGridUrl()
        {
            webView.LoadUrl($"http://{preferences.Ip}:{preferences.Port}/manage/play-grid.html?name={inputConfiguration.Name}");
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
            RunOnUiThread(new Runnable(() =>
            {
                inputConfiguration = apiResult.Content;
                SetupView();
            }));
        }

        private void SetupView()
        {
            if (inputConfiguration.HasView)
                LoadViewUrl();
            else
                LoadGridUrl();
        }
    }
}


