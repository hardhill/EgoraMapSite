using Android.App;
using Android.Widget;
using Android.OS;

namespace Routes
{
    [Activity(Label = "Routes", MainLauncher = true, Icon = "@drawable/icon")]
    public class RouteListForm : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.RouteList);
        }
    }
}

