using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Runtime;
using KeySndr.Clients.Mobile.Droid.events;
using KeySndr.Common.Providers;

namespace KeySndr.Clients.Mobile.Droid
{
    [Application(Icon = "@mipmap/ic_launcher")]
    public class KeySndrApplication : Application
    {
        public const string AppPreferencesId = "KeySndr";
        private NetworkState networkState;
        private NetworkStateReceiver networkStateReceiver;
        public EventHandler<OnNetworkStateChangeArgs> OnNetworkStateChange;
        public KeySndrApplication(IntPtr handle, JniHandleOwnership ownerShip)
            : base(handle, ownerShip)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Init();
            RegisterProviders();
        }

        private void Init()
        {
            InitStateReceiver();
            Context.RegisterReceiver(networkStateReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
        }
        private void RegisterProviders()
        {
            ObjectFactory.AddProvider(new WebConnectionProvider());
        }

        private void InitStateReceiver()
        {
            networkStateReceiver = new NetworkStateReceiver();
            networkStateReceiver.OnNetworkStateChanged += OnNetworkStateChanged;
        }

        private void OnNetworkStateChanged(object sender, EventArgs eventArgs)
        {
            var currentStatus = networkState;
            UpdateNetworkState();
            if (currentStatus == networkState)
                return;

            OnNetworkStateChange?.Invoke(this, new OnNetworkStateChangeArgs(networkState));
            if (networkState == NetworkState.ConnectedData || networkState == NetworkState.ConnectedWifi)
            {

            }
            else
            {

            }
        }

        private void UpdateNetworkState()
        {
            var connectivityManager = (ConnectivityManager)Context.GetSystemService(ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo == null)
            {
                networkState = NetworkState.Disconnected;
                return;
            }
            if (activeNetworkInfo.IsConnectedOrConnecting)
            {
                networkState = activeNetworkInfo.Type == ConnectivityType.Wifi ?
                    NetworkState.ConnectedWifi : NetworkState.ConnectedData;
            }
            else
            {
                networkState = NetworkState.Disconnected;
            }
        }

        public NetworkState GetCurrentNetworkState()
        {
            var connectivityManager = (ConnectivityManager)Context.GetSystemService(ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo == null)
                return NetworkState.Disconnected;

            if (activeNetworkInfo.IsConnected)
                return activeNetworkInfo.Type == ConnectivityType.Wifi
                    ? NetworkState.ConnectedWifi
                    : NetworkState.ConnectedData;
            else
                return NetworkState.Disconnected;
        }

        public void Close()
        {
            if (networkStateReceiver == null)
                return;

            Context.UnregisterReceiver(networkStateReceiver);
            networkStateReceiver.OnNetworkStateChanged -= OnNetworkStateChanged;
        }

        public NetworkState GetNetworkState()
        {
            UpdateNetworkState();
            return networkState;
        }
    }
}