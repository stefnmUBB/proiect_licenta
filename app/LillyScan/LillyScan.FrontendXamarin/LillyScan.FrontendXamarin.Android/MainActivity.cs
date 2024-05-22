using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Hardware.Usb;
using Xamarin.Essentials;
using System.IO;

namespace LillyScan.FrontendXamarin.Droid
{
    [Activity(Label = "LillyScan", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            FrontendXamarin.Utils.RawBitmapIOAdapter.ImageSource2RawBitmap = Utils.RawBitmapIOAdapter.ToRawBitmap;
            FrontendXamarin.Utils.RawBitmapIOAdapter.RawBitmap2ImageSource = Utils.RawBitmapIOAdapter.ToImageSource;
            Backend.Initializer.Initialize();
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(FrontendXamarin.Utils.HTR).TypeHandle);


            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);            


            LoadApplication(new App());                    
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}