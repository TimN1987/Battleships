using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for SaveGameView.xaml
    /// </summary>
    public partial class SaveGameView : UserControl
    {
        public SaveGameView(SaveGameViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
