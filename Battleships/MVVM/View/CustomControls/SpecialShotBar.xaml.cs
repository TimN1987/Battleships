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

namespace Battleships.MVVM.View.CustomControls
{
    /// <summary>
    /// Interaction logic for SpecialShotBar.xaml
    /// </summary>
    public partial class SpecialShotBar : UserControl
    {
        public SpecialShotBar()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SpecialShotNameProperty =
            DependencyProperty.Register("SpecialShotName", typeof(string), typeof(SpecialShotBar), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty HitCountProperty =
            DependencyProperty.Register("HitCount", typeof(int), typeof(SpecialShotBar), new PropertyMetadata(0));
        public static readonly DependencyProperty HitTargetProperty = 
            DependencyProperty.Register("HitTarget", typeof(int), typeof(SpecialShotBar), new PropertyMetadata(0));

        public string SpecialShotName
        {
            get => (string)GetValue(SpecialShotNameProperty);
            set => SetValue(SpecialShotNameProperty, value);
        }

        public int HitCount
        {
            get => (int)GetValue(HitCountProperty);
            set => SetValue(HitCountProperty, value);
        }

        public int HitTarget
        {
            get => (int)GetValue(HitTargetProperty);
            set => SetValue(HitTargetProperty, value);
        }
    }
}
