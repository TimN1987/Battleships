using System.Windows;
using System.Windows.Controls;

namespace Battleships.MVVM.View.CustomControls
{
    /// <summary>
    /// Interaction logic for InformationPopUp.xaml
    /// </summary>
    public partial class InformationPopUp : UserControl
    {
        public InformationPopUp()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(string), typeof(InformationPopUp), new PropertyMetadata(null));

        public string Image
        {
            get => (string)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
    }
}
