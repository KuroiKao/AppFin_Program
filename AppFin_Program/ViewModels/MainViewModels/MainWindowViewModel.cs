using AppFin_Program.ViewModels.RoutingViewModels;
using ReactiveUI;
using System.Reactive;

namespace AppFin_Program.ViewModels.MainViewModels
{
    public class MainWindowViewModel : RoutingViewModel
    {
        public RoutingViewModel CurrentRouter { get; }

        public MainWindowViewModel()
        {
            var router = new RoutingViewModel();

            CurrentRouter = router;
        }
    }
}
