using Android.App;
using Android.Content.PM;
using Android.OS;
using Prism;
using Prism.Ioc;
using Xamarin.Forms.Platform.Android;

namespace XamCompressor.Droid
{
    [Activity(Theme = "@style/MainTheme",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize/*, HardwareAccelerated = false*/)]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        internal static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ////HardwareAccelerated options is disabled only for android 7.0 y 7.1
            //if (Build.VERSION.SdkInt != BuildVersionCodes.N && Build.VERSION.SdkInt != BuildVersionCodes.NMr1 && Build.VERSION.SdkInt != BuildVersionCodes.O)
            //{
            //    this.Window.SetFlags(
            //            Android.Views.WindowManagerFlags.HardwareAccelerated,
            //            Android.Views.WindowManagerFlags.HardwareAccelerated);
            //}


            base.OnCreate(savedInstanceState);
            Instance = this;
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(new AndroidInitializer()));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}

