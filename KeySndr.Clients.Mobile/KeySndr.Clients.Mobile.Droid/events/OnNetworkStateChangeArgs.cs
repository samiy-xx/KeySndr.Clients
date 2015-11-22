using System;

namespace KeySndr.Clients.Mobile.Droid.events
{
    public class OnNetworkStateChangeArgs : EventArgs
    {
        public NetworkState State { get; private set; }
        public OnNetworkStateChangeArgs(NetworkState state)
            : base()
        {
            State = state;
        }
    }
}