﻿using System;
using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Helpers;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        private readonly IUserService _userService;
        private readonly ILoggerService _loggingService;

        public override ViewType Type => ViewType.Login;

        private Login _login;
        public Login Login
        {
            get { return _login; }
            private set { Set(ref _login, value); }
        }

        private bool _rememberMe;
        public bool RememberMe
        {
            get { return _rememberMe; }
            set { Set(ref _rememberMe, value); }
        }

        public RelayCommand LoginCommand { get; private set; }
        #endregion

        #region Constructors
        public LoginViewModel(INavigationService navigationService,
            ISettingsService settingsService,
            IUserService userService,
            ILoggerService loggingService)
            : base(navigationService)
        {
            _settingsService = settingsService;
            _userService = userService;
            _loggingService = loggingService;

            Login = new Login();
            LoginCommand = new RelayCommand(OnLogin);
        }
        #endregion

        #region Methods
        private async void OnLogin()
        {
            try
            {
                if (!Login.ValidateProperties())
                {
                    ShowMessageControl(Login.GetAllErrors());
                    return;
                }

                ShowBusyControl("Authenticating");

                var user = await _userService.LoginAsync(Login);
                if (user == null)
                {
                    HideBusyControl();
                    ShowMessageControl("Authentication failed. Incorrect username or password");
                    return;
                }

                await GetAndSaveChannelAsync(user);
                await SaveDefaultLocationAsync(user);

                HideBusyControl();

                MessengerInstance.Send(user);
                NavigateTo(ViewType.Settings);
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
                HideBusyControl();
            }
        }

        private async Task GetAndSaveChannelAsync(User user)
        {
            var channel = await _userService.GetChannelAsync(user);
            if (channel == null)
            {
                HideBusyControl();
                ShowMessageControl("Operation of getting channel is failed");
                return;
            }

            if (RememberMe)
                await _settingsService.UpdateSettingsAsync(user, channel);
            else
                await _settingsService.UpdateSettingsAsync(null, channel);
        }

        private async Task SaveDefaultLocationAsync(User user)
        {
            if (_settingsService.Settings.Locations.Any())
                return;

            var defaultLocation = user.Locations.SingleOrDefault(x => x.Id == user.DefaulLocationId);
            if (defaultLocation != null)
                await _settingsService.AddLocationAsync(defaultLocation);
        }
        #endregion
    }
}