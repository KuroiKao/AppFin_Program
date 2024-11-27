using AppFin_Program.Models;
using AppFin_Program.ViewModels.MainViewModels;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AppFin_Program.ViewModels.StartViewModels
{
    public class AuthorizationViewModel : ViewModelBase, RoutingViewModels.IRoutableViewModel
    {
        public string RouteKey => "authorization";
        private readonly FinAppDataBaseContext _dbContext;
        private readonly SessionService _sessionService;
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

        public class SessionService
        {
            public User? CurrentUser { get; private set; }

            public void SetCurrentUser(User user)
            {
                CurrentUser = user;
            }
        }

        public AuthorizationViewModel(SessionService sessionService, Action<string> navigateTo)
        {
            Login = "admin";
            Password = "123456";

            _dbContext = new FinAppDataBaseContext();            
            _sessionService = sessionService;

            LoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {                
                if (await AuthenticateUser())
                {
                    navigateTo("home");
                    return true;
                }
                else
                {
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

        private async Task<bool> AuthenticateUser()
        {           
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(x => x.Login == Login);

                if (user != null)
                {
                    if (user.Password == Password)
                    {
                        StatusMessage = "Login successful!";
                        _sessionService.SetCurrentUser(user);
                        return true;
                    }
                    else
                    {
                        StatusMessage = "Invalid password.";
                        return false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"User not found");
                    StatusMessage = "User not found.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during login: {ex.Message}");
                StatusMessage = "An error occurred during login.";
                return false;
            }
        }
        
        public AuthorizationViewModel()
        {
            throw new NotImplementedException();
        }
    }
}
