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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Battleships.MVVM.Services;
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
    }

    private void PlayGameView_Loaded(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
        Mouse.Capture(null);
        var pos = Mouse.GetPosition(this);
        VisualTreeHelper.HitTest(this, pos);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(this);
    }

    private void Storyboard_Completed(object sender, EventArgs e)
    {
        Keyboard.Focus(this);
        this.Focusable = true;
        this.Focus();
    }
}
