using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for SalvoRulesSetUpView.xaml
    /// </summary>
    public partial class SalvoRulesSetUpView : UserControl
    {
        public SalvoRulesSetUpView(SalvoRulesSetUpViewModel salvoRulesSetUpViewModel)
        {
            InitializeComponent();
            DataContext = salvoRulesSetUpViewModel;
        }
    }
}
