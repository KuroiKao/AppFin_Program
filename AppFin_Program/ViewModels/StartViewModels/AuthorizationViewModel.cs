using AppFin_Program.Models;
using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using ReactiveUI;
using System;
using System.Reactive;

namespace AppFin_Program.ViewModels.StartViewModels
{
    public class AuthorizationViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        public string RouteKey => "authorization";
        private readonly AuthenticationService _authenticationService;
        private readonly UserSessionService _userSessionService;
        public ReactiveCommand<Unit, bool> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> RegistrationCommand { get; }

        private string _login;
        public string Login
        {
            get => _login;
            set => this.RaiseAndSetIfChanged(ref _login, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public AuthorizationViewModel(){}
        public AuthorizationViewModel(AuthenticationService authenticationService, UserSessionService userSessionService, Action<string> navigateTo)
        {
            _authenticationService = authenticationService;
            _userSessionService = userSessionService;

            Login = "admin";
            Password = "123456";

            LoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var user = await _authenticationService.AuthenticateAsync(Login, Password);

                if (user != null)
                {
                    _userSessionService.SetCurrentUserId(user.Id);
                    StatusMessage = "Login successful!";
                    navigateTo("home");
                    return true;
                }
                else
                {
                    StatusMessage = "Invalid login or password.";
                    return false;
                }
            },
            this.WhenAnyValue(vm => vm.Login, vm => vm.Password,
                (login, password) =>
                    !string.IsNullOrWhiteSpace(login) &&
                    !string.IsNullOrWhiteSpace(password)));

            RegistrationCommand = ReactiveCommand.Create(() =>
            {
                navigateTo("registration");
            });
        }
    }
}
