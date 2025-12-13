using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for PlayerStartsView.xaml
    /// </summary>
    public partial class PlayerStartsView : UserControl
    {
        private PlayerStartsViewModel _viewModel;

        public PlayerStartsView(PlayerStartsViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
