using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Support.V4.View;
using Android.Webkit;
using Java.Lang;
using KeySndr.Clients.Mobile.Droid.dialogs;
using KeySndr.Clients.Mobile.Droid.events;
using KeySndr.Common;
using KeySndr.Common.Providers;
using Object = Java.Lang.Object;


namespace KeySndr.Clients.Mobile.Droid
{
	[Activity (Label = "KeySndr", MainLauncher = true, Icon = "@mipmap/ic_launcher", LaunchMode = LaunchMode.SingleTop, HardwareAccelerated = true)]
	public class MainActivity : KeysndrBaseActivity
	{
	    private const string UrlKey = "URL";
	    private const string ConfigKey = "InputConfig";
	    private const string OrientationKey = "Orientation";

        private IWebConnectionProvider webConnection;
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

            if (bundle != null)
            {
                if (bundle.ContainsKey(UrlKey))
                    currentUrl = bundle.GetString(UrlKey);
                if (bundle.ContainsKey(ConfigKey))
                    inputConfiguration = JsonSerializer.Deserialize<InputConfiguration>(bundle.GetString(ConfigKey));
            }

            CheckFirstRun();
            LoadUrl();
		}

	    protected override void OnResume()
	    {
	        base.OnResume();
            webConnection = ObjectFactory.GetProvider<IWebConnectionProvider>();
            if (!string.IsNullOrEmpty(Preferences.Ip))
            {
                webConnection.SetBaseAddress(Preferences.Ip, Preferences.Port);
                LoadServerVersion();
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(UrlKey, currentUrl);
            outState.PutString(ConfigKey, JsonSerializer.Serialize(inputConfiguration));
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

	    private void LoadUrl()
	    {
	        WebView.LoadUrl(currentUrl);
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
	        if (!string.IsNullOrEmpty(inputConfiguration?.GridSettings?.Mode))
	            intent.PutExtra(OrientationKey, inputConfiguration.GridSettings.Mode);
            StartActivity(intent);
	    }

        private void CheckFirstRun()
        {
            if (Preferences.FirtsTimeRunning)
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
                ? $"http://{Preferences.Ip}:{Preferences.Port}/Views/"
                : $"http://{Preferences.Ip}:{Preferences.Port}/manage/";


            if (Preferences.UseCache)
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

            var mode = inputConfiguration?.GridSettings?.Mode.ToLower();
            if (mode != null && (mode == "landscape" || mode == "portrait"))
            {
                if (mode == "landscape")
                    SetLandScapeOrientation();
                else 
                    SetPortraitOrientation();
            }
            else
            {
                SetUserOrientation();
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


