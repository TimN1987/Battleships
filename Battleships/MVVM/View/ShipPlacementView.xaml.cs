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
using Battleships.MVVM.Services;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.View;

/// <summary>
/// Interaction logic for ShipPlacementView.xaml
/// </summary>
public partial class ShipPlacementView : UserControl
{
    public ShipPlacementView(ShipPlacementViewModel shipPlacementViewModel)
    {
        InitializeComponent();
        DataContext = shipPlacementViewModel;

        Loaded += ShipPlacementView_Loaded;
    }

    private void ShipPlacementView_Loaded(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
    }
}
