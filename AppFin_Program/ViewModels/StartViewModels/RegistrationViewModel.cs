using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AppFin_Program.ViewModels.StartViewModels
{
    public class RegistrationViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        public string RouteKey => "registration";
        private readonly UserService _userService;
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
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
        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }
        private string? _email;
        public string? Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }
        public RegistrationViewModel()
        {

        }
        public RegistrationViewModel(UserService userService, Action<string> navigateTo)
        {
            _userService = userService;

            RegisterCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    StatusMessage = "Ошибка! Логин, Пароль или Подтверждение пароля не могут быть пустыми.";
                    return;
                }

                var existingUser = await _userService.GetUserByLoginAsync(Login);

                if (existingUser != null)
                {
                    StatusMessage = "Логин уже существует.";
                    return;
                }
                if (Password != ConfirmPassword)
                {
                    StatusMessage = "Пароли не совпадают.";
                    return;
                }
                if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@'))
                {
                    StatusMessage = "Неверный адрес электронной почты.";
                    return;
                }
                await _userService.RegisterUserAsync(Login, Password, Email);
                StatusMessage = "Registration successful!";
                await Task.Delay(1000);
                navigateTo("authorization");
            });

            CancelCommand = ReactiveCommand.Create(() => navigateTo("authorization"));
        }
    }
}
