using Android.Content;
using KeySndr.Common;

namespace KeySndr.Clients.Mobile.Droid
{
    public class AndroidAppPreferences : AppPreferences
    {
        private readonly ISharedPreferences prefs;

        public AndroidAppPreferences(ISharedPreferences p)
        {
            prefs = p;
        }

        public static AppPreferences Create(ISharedPreferences prefs)
        {
            return new AndroidAppPreferences(prefs)
            {
                FirtsTimeRunning = prefs.GetBoolean(AppPreferences.FirstTimeKey, true),
                Ip = prefs.GetString(AppPreferences.IpKey, ""),
                Port = prefs.GetInt(AppPreferences.PortKey, 0),
                UseSounds = prefs.GetBoolean(AppPreferences.UseSoundsKey, true),
                UseCache = prefs.GetBoolean(AppPreferences.UseCacheKey, true)
            };
        }

        public override void Write()
        {
            var editor = prefs.Edit();
            editor.PutBoolean(AppPreferences.FirstTimeKey, FirtsTimeRunning);
            editor.PutString(AppPreferences.IpKey, Ip);
            editor.PutInt(AppPreferences.PortKey, Port);
            editor.PutBoolean(AppPreferences.UseSoundsKey, UseSounds);
            editor.PutBoolean(AppPreferences.UseCacheKey, UseCache);
            editor.Commit();
        }
    }
}