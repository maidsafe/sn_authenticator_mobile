﻿using System;
using System.Linq;
using System.Windows.Input;
using SafeAuthenticator.Helpers;
using SafeAuthenticator.Models;
using SafeAuthenticator.Native;
using SafeAuthenticator.Services;
using Xamarin.Forms;

namespace SafeAuthenticator.ViewModels
{
    internal class HomeViewModel : BaseViewModel
    {
        private bool _isRefreshing;

        public ICommand RefreshAccountsCommand { get; }

        public ICommand SettingsCommand { get; }

        public ObservableRangeCollection<RegisteredAppModel> Apps { get; set; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            private set => SetProperty(ref _isRefreshing, value);
        }

        private RegisteredAppModel _selectedRegisteredAccount;

        public RegisteredAppModel SelectedRegisteredAccount
        {
            get => _selectedRegisteredAccount;

            set
            {
                if (value == null)
                {
                    return;
                }

                OnAccountSelected(value);
                SetProperty(ref _selectedRegisteredAccount, value);
            }
        }

        public HomeViewModel()
        {
            IsRefreshing = false;
            Apps = new ObservableRangeCollection<RegisteredAppModel>();
            RefreshAccountsCommand = new Command(OnRefreshAccounts);
            SettingsCommand = new Command(OnSettings);
            Device.BeginInvokeOnMainThread(OnRefreshAccounts);

            MessagingCenter.Subscribe<AppInfoViewModel>(this, MessengerConstants.RefreshHomePage, (sender) => { OnRefreshAccounts(); });
            MessagingCenter.Subscribe<AuthService, IpcReq>(this, MessengerConstants.RefreshHomePage, (sender, decodeResult) =>
            {
                var decodedType = decodeResult.GetType();

                // Authentication Request
                if (decodedType == typeof(AuthIpcReq))
                {
                    var ipcReq = (AuthIpcReq)decodeResult;
                    var app = new RegisteredAppModel(ipcReq.AuthReq.App, ipcReq.AuthReq.Containers);
                    var isAppContainerPresent = ipcReq.AuthReq.AppContainer;
                    var appOwnContainer = new ContainerPermissionsModel()
                    {
                        ContainerName = "App's own Container",
                        Access = new PermissionSetModel
                        {
                            Read = true,
                            Insert = true,
                            Update = true,
                            Delete = true,
                            ManagePermissions = true
                        }
                    };

                    // Add app to registeredAppList if not present
                    if (!Apps.Contains(app))
                    {
                        // Adding app's own container if present
                        if (isAppContainerPresent)
                        {
                            app.Containers.Add(appOwnContainer);
                            app.Containers.ReplaceRange(app.Containers.OrderBy(a => a.ContainerName).ToObservableRangeCollection());
                        }
                        var registeredApps = Apps;
                        registeredApps.Add(app);
                        registeredApps = registeredApps.OrderBy(a => a.AppName).ToObservableRangeCollection();
                        Apps.ReplaceRange(registeredApps);
                    }

                    // If app already exists in registeredAppList, and app's own container is requested but not previously added
                    else if (isAppContainerPresent)
                    {
                        var registeredAppsItem = Apps.FirstOrDefault(a => a.AppId == app.AppId);
                        var container = registeredAppsItem.Containers.FirstOrDefault(a => a.ContainerName == "App's own Container");
                        if (container == null)
                        {
                            registeredAppsItem.Containers.Add(appOwnContainer);
                            registeredAppsItem.Containers.ReplaceRange(registeredAppsItem.Containers.OrderBy(a => a.ContainerName).ToObservableRangeCollection());
                        }
                    }
                }

                // Container Request
                else if (decodedType == typeof(ContainersIpcReq))
                {
                    var ipcReq = (ContainersIpcReq)decodeResult;
                    var app = new RegisteredAppModel(ipcReq.ContainersReq.App, ipcReq.ContainersReq.Containers);

                    var registeredAppsItem = Apps.FirstOrDefault(a => a.AppId == app.AppId);
                    foreach (var container in app.Containers)
                    {
                        var containersItem = registeredAppsItem.Containers.FirstOrDefault(a => a.ContainerName == container.ContainerName);

                        // Add new containers
                        if (containersItem == null)
                            registeredAppsItem.Containers.Add(container);

                        // Update permission set of existing containers
                        else
                            containersItem.Access = container.Access;
                    }
                    registeredAppsItem.Containers.ReplaceRange(registeredAppsItem.Containers.OrderBy(a => a.ContainerName).ToObservableRangeCollection());
                }
            });
        }

        ~HomeViewModel()
        {
            MessagingCenter.Unsubscribe<AppInfoViewModel>(this, MessengerConstants.RefreshHomePage);
            MessagingCenter.Unsubscribe<AuthService, RegisteredAppModel>(this, MessengerConstants.RefreshHomePage);
        }

        private void OnAccountSelected(RegisteredAppModel appModelInfo)
        {
            MessagingCenter.Send(this, MessengerConstants.NavAppInfoPage, appModelInfo);
        }

        private void OnSettings()
        {
            MessagingCenter.Send(this, MessengerConstants.NavSettingsPage);
        }

        private async void OnRefreshAccounts()
        {
            try
            {
                IsRefreshing = true;
                var registeredApps = await Authenticator.GetRegisteredAppsAsync();
                registeredApps = registeredApps.OrderBy(a => a.AppName).ToList();
                Apps.ReplaceRange(registeredApps);
            }
            catch (FfiException ex)
            {
                var errorMessage = Utilities.GetErrorMessage(ex);
                await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Refresh Accounts Failed: {ex.Message}", "OK");
            }
            IsRefreshing = false;
        }

        public async void HandleAuthenticationReq()
        {
            if (string.IsNullOrEmpty(Authenticator.AuthenticationReq))
            {
                return;
            }

            await Authenticator.HandleUrlActivationAsync(Authenticator.AuthenticationReq);
            Authenticator.AuthenticationReq = null;
        }
    }
}
