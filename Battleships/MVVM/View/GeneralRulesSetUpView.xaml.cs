using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for GeneralRulesView.xaml
    /// </summary>
    public partial class GeneralRulesSetUpView : UserControl
    {
        public GeneralRulesSetUpView(GeneralRulesSetUpViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
