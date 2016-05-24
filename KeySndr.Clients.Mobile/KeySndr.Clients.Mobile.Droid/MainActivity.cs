using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Views;
using Android.OS;
using Android.Support.V4.View;
using Android.Webkit;
using Android.Widget;
using Java.Lang;
using KeySndr.Clients.Mobile.Droid.dialogs;
using KeySndr.Clients.Mobile.Droid.events;
using KeySndr.Common;
using KeySndr.Common.Providers;
using Org.Apache.Http.Client.Params;
using Object = Java.Lang.Object;
using System.Net.Http;
using Org.Apache.Http.Conn.Schemes;

namespace KeySndr.Clients.Mobile.Droid
{
	[Activity (Label = "KeySndr", MainLauncher = true, Icon = "@mipmap/ic_launcher", LaunchMode = LaunchMode.SingleTop, HardwareAccelerated = true)]
	public class MainActivity : Activity
	{
	    private const string UrlKey = "URL";
	    //private const string FullScreenKey = "FullScreen";

	    private WebView webView;
        private IWebConnectionProvider webConnection;
        private AppPreferences preferences;
        private InputConfigurationsSelectDialog configurationsSelectDialog;
        private InputConfiguration inputConfiguration;
	    private string currentUrl;
	    private string currentBaseUrl;
	    private string serverVersion;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
            currentBaseUrl = "file://android_asset/";
            currentUrl = "file:///android_asset/index.html";

            if (bundle != null && bundle.ContainsKey(UrlKey))
                currentUrl = bundle.GetString(UrlKey);

            SetContentView(Resource.Layout.Main);
            
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
                LoadServerVersion();
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

	    private void LoadServerVersion()
	    {
	        webConnection.RequestAssemblyVersion()
                .ContinueWith(ServerVersionReceived);
	    }

	    private void ServerVersionReceived(Task<ApiResult<string>> task)
	    {
	        var result = task.Result;
	        serverVersion = result.Content;
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
            var bVersion = (int) Build.VERSION.SdkInt;
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

        private void OpenSettings()
        {
            var intent = new Intent(this, typeof(SettingsActivity));
            intent.PutExtra("search", true);
            
            StartActivity(intent);
        }

	    private void ReloadToFullScreen()
	    {
	        var intent = new Intent(this, typeof(FullScreenActivity));
	        intent.PutExtra(UrlKey, currentUrl);
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
                case Resource.Id.action_load_fullscreen:
                    ReloadToFullScreen();
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void SendRequestForConfigurations()
        {
            webConnection.RequestConfigurationItems()
                .ContinueWith(RequestedConfigurationsReceived);
        }

        private void RequestedConfigurationsReceived(Task<ApiResult<IEnumerable<ConfigurationListItem>>> task)
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
            currentBaseUrl = inputConfiguration.HasView
                ? $"http://{preferences.Ip}:{preferences.Port}/Views/"
                : $"http://{preferences.Ip}:{preferences.Port}/manage/";


            if (preferences.UseCache)
            {
                currentUrl = inputConfiguration.HasView
                ? $"{currentBaseUrl}{inputConfiguration.View}/index.html"
                : $"{currentBaseUrl}play-grid.html?name={inputConfiguration.Name}";
            }
            else
            {
                currentUrl = inputConfiguration.HasView
                ? $"{currentBaseUrl}{inputConfiguration.View}/index.html?rnd={GetRandomUrlPart()}"
                : $"{currentBaseUrl}play-grid.html?name={inputConfiguration.Name}&rnd={GetRandomUrlPart()}";
            }
                

            RunOnUiThread(new Runnable(SetupView));
        }

	    private string GetRandomUrlPart()
	    {
	        return Guid.NewGuid().ToString("n");
	    }

        private void SetupView()
        {  
            LoadUrl(); 
        }

	    

	    internal class CustomPagerAdapter : PagerAdapter
	    {
	        public override bool IsViewFromObject(View view, Object objectValue)
	        {
	            throw new NotImplementedException();
	        }

	        public override int Count { get; }
	    }
    }
}


