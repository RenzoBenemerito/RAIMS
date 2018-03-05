using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using RAIMS;
using RAIMSv3;
namespace RAIMSv3
{
    [Activity(Label = "signUp")]
    public class signUp : Activity
    {
        private EditText username;
        private EditText password;
        private EditText age;
        private EditText contactNum;
        private EditText name;
        private EditText doctor;
        private Spinner gender;
        private Button signButton;
       
        private string genderItem;
        const string applicationURL = @"https://raimsapp.azurewebsites.net";
      

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.signUp);
            username = FindViewById<EditText>(Resource.Id.username);
            password = FindViewById<EditText>(Resource.Id.password);
            //age = FindViewById<EditText>(Resource.Id.age);
            contactNum = FindViewById<EditText>(Resource.Id.contactNum);
            name = FindViewById<EditText>(Resource.Id.name);
            doctor = FindViewById<EditText>(Resource.Id.doctor);
            age = FindViewById<EditText>(Resource.Id.age);
            gender = FindViewById<Spinner>(Resource.Id.gender);

            var adapter = ArrayAdapter.CreateFromResource(
            this, Resource.Array.dropdown, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            gender.Adapter = adapter;
            //spinner

            //CONNECTION
           
            signButton = FindViewById<Button>(Resource.Id.signButton);
           
            // Create the client instance, using the mobile app backend URL.

         
        }



        

        //[Java.Interop.Export()]
       
    }
}