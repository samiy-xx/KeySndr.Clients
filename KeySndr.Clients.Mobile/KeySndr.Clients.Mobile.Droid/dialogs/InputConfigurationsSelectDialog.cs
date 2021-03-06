using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using KeySndr.Clients.Mobile.Droid.events;
using KeySndr.Common;

namespace KeySndr.Clients.Mobile.Droid.dialogs
{
    public class InputConfigurationsSelectDialog : Dialog
    {
        private Spinner spinner;
        private Button okButton;
        private Button cancelButton;
        private IEnumerable<ConfigurationListItem> configurations;
        private readonly Activity activity;
        public event EventHandler<OnSelectConfigurationArgs> OnSelectConfiguration;

        public InputConfigurationsSelectDialog(Activity context, IEnumerable<ConfigurationListItem> c) : base(context)
        {
            activity = context;
            configurations = c;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature((int)WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.select_configuration_dialog);
            ReOrderConfigurations();
            okButton = FindViewById<Button>(Resource.Id.selectConfigurationButtonOk);
            cancelButton = FindViewById<Button>(Resource.Id.selectConfigurationButtonCancel);
            spinner = FindViewById<Spinner>(Resource.Id.selectConfigurationSpinner);
            spinner.Adapter = new ArrayAdapter(activity, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                configurations.ToList());

            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;
            base.OnCreate(savedInstanceState);
        }

        private void ReOrderConfigurations()
        {
            configurations = configurations.OrderBy(a => a.Name);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            var spinnerValue = (string)spinner.GetItemAtPosition(spinner.SelectedItemPosition);
            OnSelectConfiguration?.Invoke(this, new OnSelectConfigurationArgs(spinnerValue));
        }
        

        private int GetSpinnerPosition(Spinner spinner, string key)
        {
            for (var i = 0; i < spinner.Count; i++)
            {
                var item = (string)spinner.GetItemAtPosition(i);
                if (key.Equals(item))
                    return i;
            }
            return -1;
        }


    }
}