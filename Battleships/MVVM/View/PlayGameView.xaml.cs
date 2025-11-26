using Battleships.MVVM.Services;
using Battleships.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Battleships.MVVM.View;

/// <summary>
/// Interaction logic for PlayGame.xaml
/// </summary>
public partial class PlayGameView : UserControl
{
    public PlayGameView(PlayGameViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += PlayGameView_Loaded;

        AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ReturnFocusToGrid));
    }

    private void PlayGameView_Loaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            FocusGrid();
        }), DispatcherPriority.ContextIdle);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        FocusGrid();
    }

    private void Storyboard_Completed(object sender, EventArgs e)
    {
        FocusGrid();
    }

    private void FocusGrid()
    {
        if (ComputerGrid.IsLoaded)
        {
            ComputerGrid.Focus();
            Keyboard.Focus(ComputerGrid);
        }
    }

    private void ReturnFocusToGrid(object sender, RoutedEventArgs e)
    {
        FocusGrid();
    }
}
