using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Identity.Client;
using Uno.AuthenticationBroker;

namespace TodoApp.Uno.Droid
{
    [Activity(
        MainLauncher = true,
        ConfigurationChanges = global::Uno.UI.ActivityHelper.AllConfigChanges,
        WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
    )]
    public class MainActivity : Microsoft.UI.Xaml.ApplicationActivity
    {
        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
        }
    }


    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
       Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
       DataHost = "auth",
       DataScheme = "msal356fe7ef-d800-4264-99ce-d44568b84263")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}