using AppFin_Program.Models;
using AppFin_Program.Services;
using AppFin_Program.ViewModels.MainViewModels;
using AppFin_Program.ViewModels.StartViewModels;
using AppFin_Program.ViewModels.WindowViewModels;
using Microsoft.EntityFrameworkCore;
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

        public RoutingViewModel(IDbContextFactory<FinAppDataBaseContext> dbContextFactory)
        {            
            var userSessionService = new UserSessionService();
            var categoryService = new CategoryService(dbContextFactory);
            var authenticationService = new AuthenticationService(dbContextFactory);
            var userService = new UserService(dbContextFactory);
            var reportService = new ReportService(dbContextFactory, userSessionService);
            var budgetService = new BudgetService(dbContextFactory, userSessionService);
            var trasnactionService = new TransactionService(dbContextFactory, userSessionService, budgetService);
            var goalService = new GoalService(dbContextFactory, userSessionService);


            _routes = new Dictionary<string, Func<IRoutableViewModel>>
            {
            { "authorization", () => new AuthorizationViewModel(authenticationService, userSessionService, NavigateTo) },
            { "registration", () => new RegistrationViewModel(userService, NavigateTo) },
            { "home", () => new HomeViewModel(trasnactionService, userSessionService, categoryService, budgetService, NavigateTo) },
            { "report", () => new ReportViewModel(userSessionService, trasnactionService, reportService, NavigateTo)},
            { "goal", () => new GoalViewModel(goalService, userSessionService, budgetService, NavigateTo) }
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
