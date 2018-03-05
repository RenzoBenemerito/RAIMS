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
using Android.Support.V7.App;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using RAIMSv3;
using Android.Support.V4.Widget;
using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using Android.Graphics;
using Android.Provider;
using Android.Content.PM;
using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Microsoft.Cognitive.CustomVision.Prediction;
using Java.Util;

namespace RAIMS
{

    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }
    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            return resizedBitmap;
        }
    }
    [Activity(Label = "Camera", Theme = "@style/MyTheme2")]
    
    public class camera : ActionBarActivity
    {
        private SupportToolbar mToolbar;
        private MyActionBarDrawerToggle mDrawerToggle;
        private DrawerLayout mDrawerLayout;
        private ListView mLeftDrawer;
        private ArrayAdapter mLeftAdapter;
        private List<string> mLeftDataSet;
        private ImageView _imageView;
        private static Project project;
        private string resultSet;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.camera);
            mToolbar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            mLeftDrawer = FindViewById<ListView>(Resource.Id.left_drawer);
            //picture
            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();

                Button button = FindViewById<Button>(Resource.Id.myButton);
                _imageView = FindViewById<ImageView>(Resource.Id.imageView1);
                button.Click += TakeAPicture;
            }

            mLeftDrawer.Tag = 0;

            SetSupportActionBar(mToolbar);

            mLeftDataSet = new List<string>();
            mLeftDataSet.Add("Home");
            mLeftDataSet.Add("Test");
            mLeftDataSet.Add("Therapy");
            mLeftDataSet.Add("History");
            mLeftDataSet.Add("Log-Out");
            mLeftAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mLeftDataSet);
            mLeftDrawer.Adapter = mLeftAdapter;

            mDrawerToggle = new MyActionBarDrawerToggle(
                this,                           //Host Activity
                mDrawerLayout,                  //DrawerLayout
                0,     //Opened Message
                0     //Closed Message
            );

            mDrawerLayout.SetDrawerListener(mDrawerToggle);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            mDrawerToggle.SyncState();

            mLeftDrawer.ItemClick += MLeftDrawer_ItemClick;


        }
        private string connectToAnalyze()
        {
            // custom vision
            TrainingApi trainingAPI = new TrainingApi() { ApiKey = "1360135bcfaa45d5b6d69dc06dcb04e4" };
            Guid _projectkey = new Guid("ea8591bc-bff1-47d6-b46d-64938d2d8b58");

            project = trainingAPI.GetProject(_projectkey);

            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = "65125a9d5a774acaaeaa0f9705404307" };
            // 28bf5d1c-1025-4d3f-a7d7-4dfd4b65f4a1   iteration key


            ArrayList lists = new ArrayList();

            byte[] imgArr = GetFileAsByteArray(App._file.AbsolutePath);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(imgArr);
            var result = endpoint.PredictImage(project.Id, stream, new Guid("28bf5d1c-1025-4d3f-a7d7-4dfd4b65f4a1"));
            foreach (var prediction in result.Predictions)
            {
                lists.Add($"{prediction.Tag}");
            }
            return lists.Get(0).ToString();
        }
        static byte[] GetFileAsByteArray(string filePath)
        {
            System.IO.FileStream fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        private void TakeAPicture(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
            StartActivityForResult(intent, 0);
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display.
            // Loading the full sized image will consume to much memory
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = _imageView.Height;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                _imageView.SetImageBitmap(App.bitmap);
                App.bitmap = null;
            }
            //send result to next activity
            resultSet = connectToAnalyze();
            FindViewById<Button>(Resource.Id.sendButton).Click += Camera_Click;

            // Dispose of the Java side bitmap.
            GC.Collect();
        }

        private void Camera_Click(object sender, EventArgs e)
        {
            var send = new Intent(this, typeof(result));
            send.PutExtra("MyData", resultSet);
            StartActivity(send);
        }

        private void MLeftDrawer_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (mLeftDataSet[e.Position] == "Home")
            {
                StartActivity(typeof(dashboard));
            }
            else if (mLeftDataSet[e.Position] == "Test")
            {
                StartActivity(typeof(test));
            }
            else if (mLeftDataSet[e.Position] == "Therapy")
            {
                StartActivity(typeof(therapy));
            }
            else if (mLeftDataSet[e.Position] == "History")
            {
                StartActivity(typeof(history));
            }
            else if (mLeftDataSet[e.Position] == "Log-Out")
            {
                StartActivity(typeof(MainActivity));
            }

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {

                case Android.Resource.Id.Home:
                    //The hamburger icon was clicked which means the drawer toggle will handle the event
                    //all we need to do is ensure the right drawer is closed so the don't overlap
                    mDrawerToggle.OnOptionsItemSelected(item);
                    return true;


                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if (mDrawerLayout.IsDrawerOpen((int)GravityFlags.Left))
            {
                outState.PutString("DrawerState", "Opened");
            }

            else
            {
                outState.PutString("DrawerState", "Closed");
            }

            base.OnSaveInstanceState(outState);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            mDrawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            mDrawerToggle.OnConfigurationChanged(newConfig);
        }
    }
}

