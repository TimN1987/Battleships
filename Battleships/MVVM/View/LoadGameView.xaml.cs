using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for LoadGame.xaml
    /// </summary>
    public partial class LoadGameView : UserControl
    {
        public LoadGameView(LoadGameViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
