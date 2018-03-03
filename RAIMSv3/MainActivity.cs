using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using RAIMSv3;

namespace RAIMS
{
    [Activity(Label = "RAIMS", MainLauncher = true, Theme = "@style/MyTheme")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FindViewById<Button>(Resource.Id.loginButton).Click += OnItemsClick;
        }
        void OnItemsClick(object sender, EventArgs e)
        {
            StartActivity(typeof(dashboard));
        }
    }
}

