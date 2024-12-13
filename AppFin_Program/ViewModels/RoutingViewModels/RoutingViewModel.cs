using AppFin_Program.Models;
using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using AppFin_Program.ViewModels.StartViewModels;
using AppFin_Program.ViewModels.WindowViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace AppFin_Program.ViewModels.RoutingViewModels
{
    public class RoutingViewModel : ViewModelBase
    {
        private IRoutableViewModel? _currentView;

        public IRoutableViewModel? CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
        private readonly Dictionary<string, Func<IRoutableViewModel>> _routes;

        public RoutingViewModel()
        {
            var dbcontext = new FinAppDataBaseContext();
            var userSessionService = new UserSessionService();
            var trasnactionService = new TransactionService(dbcontext, userSessionService);
            var categoryService = new CategoryService(dbcontext);
            var authenticationService = new AuthenticationService(dbcontext);
            var userService = new UserService(dbcontext);
            var reportService = new ReportService(dbcontext, userSessionService);

            _routes = new Dictionary<string, Func<IRoutableViewModel>>
            {
            { "authorization", () => new AuthorizationViewModel(authenticationService, userSessionService, NavigateTo) },
            { "registration", () => new RegistrationViewModel(userService, NavigateTo) },
            { "home", () => new HomeViewModel(trasnactionService, userSessionService, categoryService, NavigateTo) },
            { "report", () => new ReportViewModel(userSessionService, trasnactionService, reportService, NavigateTo)} 
        };
            NavigateTo("authorization");
        }

        public void NavigateTo(string routeKey)
        {
            if (_routes.TryGetValue(routeKey, out var viewModelFactory))
            {
                CurrentView = viewModelFactory();
            }
            else
            {
                throw new ArgumentException($"Route with key '{routeKey}' not found.");
            }
        }
    }
}
