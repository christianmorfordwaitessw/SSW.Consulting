﻿using System;
using System.Windows.Input;
using SSW.Consulting.Services;
using Xamarin.Forms;
using Xamarin.Essentials;
using SSW.Consulting.Models;
using Microsoft.AppCenter.Auth;

namespace SSW.Consulting.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public ICommand LoginTappedCommand { get; set; }
        private IUserService _userService { get; set; }
        public bool isRunning { get; set; }
        public bool LoginButtonEnabled { get { return !isRunning; } }

        public string ButtonText { get; set; }

        public LoginPageViewModel(IUserService userService)
        {
            _userService = userService;
            LoginTappedCommand = new Command(SignIn);
            ButtonText = "Sign up / Log in";
            OnPropertyChanged("ButtonText");
            Refresh();
        }

        private async void SignIn()
        {
            isRunning = true;
            RaisePropertyChanged(new string[] { "isRunning", "LoginButtonEnabled" });

            ApiStatus status;
            try
            {
                status = await _userService.SignInAsync();
            }
            catch (Exception)
            {
                status = ApiStatus.LoginFailure;
            }

            switch (status)
            {
                case ApiStatus.Success:
                    Application.Current.MainPage = Resolver.Resolve<AppShell>();
                    await Shell.Current.GoToAsync("//main");
                    break;
                case ApiStatus.Unavailable:
                    await App.Current.MainPage.DisplayAlert("Service Unavailable", "Looks like the SSW.Consulting service is not currently available. Please try again later.", "OK");
                    break;
                case ApiStatus.LoginFailure:
                    await App.Current.MainPage.DisplayAlert("Login Failure", "There seems to have been a problem logging you in. Please try again.", "OK");
                    break;
                default:
                    await App.Current.MainPage.DisplayAlert("Unexpected Error", "Something went wrong there, please try again later.", "OK");
                    break;
            }

            isRunning = false;
            RaisePropertyChanged(new string[] { "isRunning", "LoginButtonEnabled" });
        }

        private async void Refresh()
        {
            bool loggedIn = await _userService.IsLoggedInAsync();

            if (loggedIn)
            {
                isRunning = true;
                ButtonText = "Logging you in...";
                RaisePropertyChanged("isRunning", "ButtonText", "LoginButtonEnabled");

                UserInformation userInfo = await Auth.SignInAsync();
                string token = userInfo.AccessToken;
                await SecureStorage.SetAsync("auth_token", token);

                await _userService.UpdateMyDetailsAsync();

                Application.Current.MainPage = Resolver.Resolve<AppShell>();
                await Shell.Current.GoToAsync("//main");
            }
        }
    }
}
