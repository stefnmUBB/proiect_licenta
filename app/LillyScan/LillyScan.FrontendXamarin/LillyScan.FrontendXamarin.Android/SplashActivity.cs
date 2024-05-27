using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.AppCompat.App;
using Java.Lang;
using LillyScan.Backend.Math;
using LillyScan.FrontendXamarin.Droid.Utils;
using System;
using System.Threading.Tasks;

namespace LillyScan.FrontendXamarin.Droid
{
    [Activity(Label ="LillyScan", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            Task.Run(Startup);            
        }
        
        void Startup()
        {
            System.Diagnostics.Debug.WriteLine("CL Init");
            CLBinding.Init();
            System.Diagnostics.Debug.WriteLine("CL Run");

            PlatformConfig.DotMul = null;
            PlatformConfig.DotMul = CLBinding.DotMul;
            //Img2Col.Run();

            //Console.WriteLine(string.Join(", ", r));
            //JavaSystem.Exit(0);

            FrontendXamarin.Utils.RawBitmapIOAdapter.ImageSource2RawBitmap = Utils.RawBitmapIOAdapter.ToRawBitmap;
            FrontendXamarin.Utils.RawBitmapIOAdapter.RawBitmap2ImageSource = Utils.RawBitmapIOAdapter.ToImageSource;
            Backend.Initializer.Initialize();
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(FrontendXamarin.Utils.HTR).TypeHandle);
            //Log.Debug(TAG, "Startup work is finished - starting MainActivity.");
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}