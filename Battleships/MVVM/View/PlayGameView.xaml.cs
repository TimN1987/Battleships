using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Battleships.MVVM.ViewModel;

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
