using AppFin_Program.ViewModels.RoutingViewModels;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using ReactiveUI;
using System.Reactive;

namespace AppFin_Program.ViewModels.MainViewModels
{
    public class MainWindowViewModel : RoutingViewModels.RoutingViewModel
    {
        public ReactiveCommand<Unit, Unit> NavigateAuthorizationCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateRegistrationCommand { get; }
        public RoutingViewModel CurrentRouter { get; }

        public MainWindowViewModel()
        {
            var router = new RoutingViewModel();

            NavigateAuthorizationCommand = ReactiveCommand.Create(() => router.NavigateTo("authorization"));
            NavigateRegistrationCommand = ReactiveCommand.Create(() => router.NavigateTo("registration"));

            CurrentRouter = router;
        }
    }
}
