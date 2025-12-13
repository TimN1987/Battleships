using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for Title.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView(HomeViewModel homeViewModel)
        {
            InitializeComponent();
            DataContext = homeViewModel;
        }
    }
}
