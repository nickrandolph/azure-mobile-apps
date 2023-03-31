using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Data.MVVM;
using TodoApp.Data.Services;
using Windows.System;
using Microsoft.Datasync.Client;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Linq;
using Uno.UI.MSAL;
using Uno.UI;

namespace TodoApp.Uno;

public sealed partial class MainPage : Page, IMVVMHelper {
    private readonly TodoListViewModel _viewModel;
    private readonly ITodoService _service;
    private IPublicClientApplication? _identityClient;

    public MainPage() {
        this.InitializeComponent();

        _service = new RemoteTodoService(GetAuthenticationToken);
        _viewModel = new TodoListViewModel(this, _service);
        mainContainer.DataContext = _viewModel;
    }

    public async Task<AuthenticationToken> GetAuthenticationToken()
    {
        //if (_identityClient == null)
        //{
        //    _identityClient = PublicClientApplicationBuilder.Create(Constants.ApplicationId)
        //        .WithAuthority(AzureCloudInstance.AzurePublic, "common")
        //        .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
        //        .WithUnoHelpers()
        //        .Build();
        //}

        if (_identityClient == null)
        {
#if __ANDROID__
            var ctx = ContextHelper.Current as Android.App.Activity;
            _identityClient = PublicClientApplicationBuilder
            .Create(Constants.ApplicationId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithRedirectUri($"msal{Constants.ApplicationId}://auth")
            .WithUnoHelpers()
            //.WithParentActivityOrWindow(() => ContextHelper.Current as Android.App.Activity)
            .Build();
#elif __IOS__
        _identityClient = PublicClientApplicationBuilder
            .Create(Constants.ApplicationId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
            .WithRedirectUri($"msal{Constants.ApplicationId}://auth")
            .Build();
#elif WINDOWS
            _identityClient = PublicClientApplicationBuilder
                .Create(Constants.ApplicationId)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();
#else
            _identityClient = PublicClientApplicationBuilder
                .Create(Constants.ApplicationId)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .WithRedirectUri("http://localhost")
                .Build();

#endif
        }


        var accounts = await _identityClient.GetAccountsAsync();
        AuthenticationResult? result = null;
        try
        {
            result = await _identityClient
                .AcquireTokenSilent(Constants.Scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            result = await _identityClient
                .AcquireTokenInteractive(Constants.Scopes)
                .WithUnoHelpers()
                .ExecuteAsync();
        }
        catch (Exception ex)
        {
            // Display the error text - probably as a pop-up
            System.Diagnostics.Debug.WriteLine($"Error: Authentication failed: {ex.Message}");
        }

        return new AuthenticationToken
        {
            DisplayName = result?.Account?.Username ?? "",
            ExpiresOn = result?.ExpiresOn ?? DateTimeOffset.MinValue,
            Token = result?.AccessToken ?? "",
            UserId = result?.Account?.Username ?? ""
        };
    }


    #region IMVVMHelper
    public Task RunOnUiThreadAsync(Action func) {
        DispatcherQueue.TryEnqueue(() => func());
        return Task.CompletedTask;
    }

    public async Task DisplayErrorAlertAsync(string title, string message) {
        var dialog = new ContentDialog {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };
#if WINDOWS
        dialog.XamlRoot = Content.XamlRoot;
#endif
        await dialog.ShowAsync();
    }
    #endregion

    #region Event Handlers

    private void GridLoadedEventHandler(object sender, RoutedEventArgs e) {
        _viewModel.OnActivated();
    }

    private void TextboxKeyDownHandler(object sender, KeyRoutedEventArgs e) {
        if (e.Key == VirtualKey.Enter) {
            AddItemToList();
        }
    }

    /// <summary>
    /// Event handler that is triggered when the Add Item button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddItemClickHandler(object sender, RoutedEventArgs e) {
        AddItemToList();
    }

    private async void AddItemToList() {
        await _viewModel.AddItemAsync(textboxControl.Text.Trim());
        await RunOnUiThreadAsync(() => textboxControl.Text = String.Empty);
    }

    /// <summary>
    /// Event handler that is triggered when the check box next to an item is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CheckboxClickHandler(object sender, RoutedEventArgs e) {
        if (sender is Microsoft.UI.Xaml.Controls.CheckBox cb) {
            await _viewModel.UpdateItemAsync(cb.Tag as string, cb.IsChecked ?? false);
        }
    }

    /// <summary>
    /// Event handler that is triggered when the Refresh Items button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RefreshItemsClickHandler(object sender, RoutedEventArgs e) {
        await _viewModel.RefreshItemsAsync();
    }
    #endregion


}
