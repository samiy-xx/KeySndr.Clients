using System;
using Android.App;
using Android.Content;
using Android.Net;

namespace KeySndr.Clients.Mobile.Droid
{
    [BroadcastReceiver]
    [IntentFilter(new[] { ConnectivityManager.ConnectivityAction })]
    public class NetworkStateReceiver : BroadcastReceiver
    {
        public EventHandler<EventArgs> OnNetworkStateChanged;
        public override void OnReceive(Context context, Intent intent)
        {
            OnNetworkStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public enum NetworkState
    {
        Unknown,
        ConnectedWifi,
        ConnectedData,
        Connecting,
        Disconnected
    }
}