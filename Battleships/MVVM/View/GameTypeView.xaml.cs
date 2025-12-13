using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for GameType.xaml
    /// </summary>
    public partial class GameTypeView : UserControl
    {
        public GameTypeView(GameTypeViewModel gameTypeViewModel)
        {
            InitializeComponent();
            DataContext = gameTypeViewModel;
        }
    }
}
