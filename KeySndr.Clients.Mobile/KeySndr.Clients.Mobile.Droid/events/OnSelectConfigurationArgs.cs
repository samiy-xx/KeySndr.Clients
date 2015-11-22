using System;

namespace KeySndr.Clients.Mobile.Droid.events
{
    public class OnSelectConfigurationArgs : EventArgs
    {
        public string Name { get; private set; }

        public OnSelectConfigurationArgs(string name)
            : base()
        {
            Name = name;
        }
    }
}