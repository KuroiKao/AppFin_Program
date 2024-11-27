using AppFin_Program.ViewModels.MainViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System;
using AppFin_Program.ViewModels.StartViewModels;
using AppFin_Program.ViewModels.WindowViewModels;
using static AppFin_Program.ViewModels.StartViewModels.AuthorizationViewModel;

namespace AppFin_Program.ViewModels.RoutingViewModels
{
    public class RoutingViewModel : ViewModelBase
    {
        private IRoutableViewModel? _currentView;
        private readonly SessionService _sessionService;

        public IRoutableViewModel? CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
        private readonly Dictionary<string, Func<IRoutableViewModel>> _routes;

        public RoutingViewModel()
        {
            _sessionService = new SessionService();

            _routes = new Dictionary<string, Func<IRoutableViewModel>>
        {
            { "authorization", () => new AuthorizationViewModel(_sessionService, NavigateTo) },
            { "registration", () => new RegistrationViewModel(NavigateTo) },
            { "home", () => new HomeViewModel(_sessionService, NavigateTo) }
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
