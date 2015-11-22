using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using KeySndr.Clients.Mobile.Droid.BeaconLib;

namespace KeySndr.Clients.Mobile.Droid
{
    [Activity(Label = "KeySndr - Settings", ParentActivity = typeof(MainActivity))]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "md51da046eae7a5bd2118a1f1a718985921.MainActivity")]
    public class SettingsActivity : Activity
    {
        private EditText editIpView;
        private EditText editPortView;
        private CheckBox editSoundView;
        private IMenuItem refreshMenuItem;

        private Probe probe;
        private Timer t;
        private int counter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Settings);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            counter = 0;
            var prefs =
                AndroidAppPreferences.Create(Application.Context.GetSharedPreferences(KeySndrApplication.AppPreferencesId, FileCreationMode.Private));

            editIpView = FindViewById<EditText>(Resource.Id.ipEditText);
            editPortView = FindViewById<EditText>(Resource.Id.portEditText);
            editSoundView = FindViewById<CheckBox>(Resource.Id.enableSoundCheckBox);

            if (!string.IsNullOrEmpty(prefs.Ip))
                editIpView.Text = prefs.Ip;
            if (prefs.Port > 0)
                editPortView.Text = prefs.Port.ToString();
            editSoundView.Checked = prefs.UseSounds;


            probe = new Probe("KeySndrServer");
            probe.BeaconsUpdated += Probe_BeaconsUpdated;


            t = new Timer(1000);
            t.Elapsed += TimerOnElapsed;
            if (Intent.Extras == null || !Intent.Extras.ContainsKey("search"))
                return;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (counter == 0)
                DoProbe();

            t.Interval = 1000;
            if (counter >= 5)
            {
                t.Stop();
                if (probe.Running)
                    StopProbing();
            }
            counter++;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.settings_activity_actions, menu);
            refreshMenuItem = menu.FindItem(Resource.Id.action_refresh);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_refresh:
                    counter = 0;
                    t.Interval = 100;
                    t.Start();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void Probe_BeaconsUpdated(IEnumerable<BeaconLocation> locations)
        {
            var locationList = locations.ToList();
            locationList.Sort((a, b) => b.LastAdvertised.CompareTo(a.LastAdvertised));
            var location = locationList
                .FirstOrDefault();
            if (location == null)
                return;

            StopProbing();

            RunOnUiThread(() =>
            {
                editIpView.Text = location.Address.Address.ToString();
                editPortView.Text = location.Address.Port.ToString();
            });

        }

        protected override void OnResume()
        {
            t.Start();
            base.OnResume();
        }

        protected override void OnPause()
        {
            var prefs =
                AndroidAppPreferences.Create(Application.Context.GetSharedPreferences(KeySndrApplication.AppPreferencesId, FileCreationMode.Private));
            prefs.SetIp(GetString(editIpView, "127.0.0.1"))
                .SetPort(int.Parse(GetString(editPortView, "6666")))
                .SetUseSounds(editSoundView.Checked)
                .SetFirstTimeRunning(false)
                .Write();

            if (t.Enabled)
                t.Stop();

            try
            {
                probe?.Stop();
            }
            catch (Exception)
            {
            }
            refreshMenuItem?.CollapseActionView();
            InvalidateOptionsMenu();
            base.OnPause();
        }

        private void StopProbing()
        {
            RunOnUiThread(() =>
            {
                refreshMenuItem?.CollapseActionView();
                InvalidateOptionsMenu();
            });
            try
            {
                probe?.Stop();
            }
            catch (Exception e) { }
        }

        private bool CanProbe()
        {
            var app = (KeySndrApplication)Application;
            var acceptedStates = new[] { NetworkState.ConnectedData, NetworkState.ConnectedWifi };
            return acceptedStates.Contains(app.GetCurrentNetworkState());
        }
        private void DoProbe()
        {

            if (!probe.IsDone)
            {
                Log.Debug("KEY_SNDR_PROBE", "Probe is still running");
                return;
            }
            counter = 0;
            try
            {
                if (!CanProbe())
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "Can not probe, No networking", ToastLength.Short).Show();
                    });
                    return;
                }
                probe.Start();

                if (refreshMenuItem != null && !refreshMenuItem.IsActionViewExpanded)
                {
                    RunOnUiThread(() =>
                    {
                        refreshMenuItem.SetActionView(Resource.Layout.intermediate_progress);
                        refreshMenuItem.ExpandActionView();
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error("KEY_SNDR_PROBE", e.Message);
            }
        }

        private string GetString(TextView t, string tmp)
        {
            return string.IsNullOrEmpty(t.Text) ? tmp : t.Text;
        }
    }
}