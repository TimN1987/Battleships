using Battleships.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
