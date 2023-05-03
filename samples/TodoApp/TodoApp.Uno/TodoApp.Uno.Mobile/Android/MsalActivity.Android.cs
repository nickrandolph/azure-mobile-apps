using Android.Content;
using Microsoft.Identity.Client;

namespace TodoApp.Uno.Droid
{
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
       Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
       DataHost = "auth",
       DataScheme = "msal356fe7ef-d800-4264-99ce-d44568b84263")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}