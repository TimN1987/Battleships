using System.Diagnostics;
using System.Text;
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
using System.Windows.Threading;
using Battleships.MVVM.Services;
using Battleships.MVVM.ViewModel;

namespace Battleships
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IEventAggregator _eventAggregator;

        public MainWindow(MainViewModel viewModel, IEventAggregator eventAggregator)
        {
            InitializeComponent();
            DataContext = viewModel;

            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<FullScreenEvent>().Subscribe(isFullScreen => ChangeFullScreenSetting(isFullScreen));

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;

            Loaded += MainWindow_Loaded;
            KeyDown += KeyDown_Escape;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)FindResource("LoadingScreenFade");
            storyboard.Begin();
        }

        private void ChangeFullScreenSetting(bool isFullScreen)
        {
            if (isFullScreen)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private void BackgroundMusic_MediaEnded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<BackgroundMusicEndedEvent>().Publish();
        }

        private void KeyDown_Escape(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ChangeFullScreenSetting(true);
                
                _eventAggregator.GetEvent<KeyDownFullScreenEvent>().Publish();
            }
        }
    }
}