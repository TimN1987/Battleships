using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for DifficultyView.xaml
    /// </summary>
    public partial class DifficultyView : UserControl
    {
        public DifficultyView(DifficultyViewModel difficultyViewModel)
        {
            InitializeComponent();
            DataContext = difficultyViewModel;
        }
    }
}
