using AppFin_Program.Models;
using AppFin_Program.ViewModels.MainViewModels;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AppFin_Program.ViewModels.StartViewModels
{
    public class RegistrationViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        public string RouteKey => "registration";
        private readonly FinAppDataBaseContext _dbContext;
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
        private bool _isLoginEmpty;
        public bool IsLoginEmpty
        {
            get => _isLoginEmpty;
            set => this.RaiseAndSetIfChanged(ref _isLoginEmpty, value);
        }
        private bool _isPasswordEmpty;
        public bool IsPasswordEmpty
        {
            get => _isPasswordEmpty;
            set => this.RaiseAndSetIfChanged(ref _isPasswordEmpty, value);
        }
        private bool _isConfirmPasswordEmpty;
        public bool IsConfirmPasswordEmpty
        {
            get => _isConfirmPasswordEmpty;
            set => this.RaiseAndSetIfChanged(ref _isConfirmPasswordEmpty, value);
        }
        private bool _isEmailEmpty;
        public bool IsEmailEmpty
        {
            get => _isEmailEmpty;
            set => this.RaiseAndSetIfChanged(ref _isEmailEmpty, value);
        }
        public RegistrationViewModel(){}
        public RegistrationViewModel(Action<string> navigateTo)
        {
            _dbContext = new FinAppDataBaseContext();

            RegisterCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                IsLoginEmpty = string.IsNullOrWhiteSpace(Login);
                IsPasswordEmpty = string.IsNullOrWhiteSpace(Password);
                IsConfirmPasswordEmpty = string.IsNullOrWhiteSpace(ConfirmPassword);
                IsEmailEmpty = string.IsNullOrWhiteSpace(Email);

                if (IsLoginEmpty || IsPasswordEmpty || IsConfirmPasswordEmpty)
                {
                    StatusMessage = "Login, Password, or Confirm Password cannot be empty.";
                    return;
                }

                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == Login);
                if (existingUser != null)
                {
                    StatusMessage = "Login already exists.";
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    StatusMessage = "Passwords do not match.";
                    return;
                }

                if (IsEmailEmpty)
                {
                    return;
                }
                else if (!Email.Contains("@mail.ru" + "gmail.com"))
                {
                    StatusMessage = "Not a valid E-Mail-Address";
                    return;
                }

                var newUser = new User
                {
                    Login = Login,
                    Password = Password,
                    Email = Email
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
                StatusMessage = "Registration successful!";
                await Task.Delay(1000);
                navigateTo("authorization");
            });

            CancelCommand = ReactiveCommand.Create(() => navigateTo("authorization"));
        }
    }
}
