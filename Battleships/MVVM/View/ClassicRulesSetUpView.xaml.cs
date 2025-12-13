using System.Windows.Controls;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View
{
    /// <summary>
    /// Interaction logic for ClassicRulesSetUpView.xaml
    /// </summary>
    public partial class ClassicRulesSetUpView : UserControl
    {
        public ClassicRulesSetUpView(ClassicRulesSetUpViewModel classicRulesSetUpViewModel)
        {
            InitializeComponent();
            DataContext = classicRulesSetUpViewModel;
        }
    }
}
