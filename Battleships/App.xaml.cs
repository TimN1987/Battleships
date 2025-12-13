using System.IO;
using System.Windows;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Battleships;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    #region Fields
    private IServiceCollection _serviceCollection;
    private IServiceProvider _serviceProvider;
    private string _databaseFilePath;
    private string _connectionString;
    private string _fallbackLogFilePath;
    #endregion //Fields

    public App()
    {

        _databaseFilePath = GenerateDatabaseFilePath();
        _connectionString = $"Data Source=/{_databaseFilePath.Substring(3)}";
        _fallbackLogFilePath = GenerateFallbackLogFilePath();

        _serviceCollection = new ServiceCollection();
        _serviceCollection.ConfigureServices(_fallbackLogFilePath, _connectionString);
        _serviceProvider = _serviceCollection.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        var initializer = _serviceProvider.GetRequiredService<IDatabaseInitializer>();

        _ = initializer.InitializeDatabaseWithRetries();
        _serviceProvider.GetRequiredService<IMessageService>();
        _serviceProvider.GetRequiredService<ISoundService>();
        _serviceProvider.GetRequiredService<ICaptainService>();
    }

    private static string GenerateDatabaseFilePath()
    {
        string databaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battleships");

        if (!Directory.Exists(databaseFolderPath))
            Directory.CreateDirectory(databaseFolderPath);

        string databasePath = Path.Combine(databaseFolderPath, "savegames.sqlite");

        return databasePath;
    }

    private static string GenerateFallbackLogFilePath()
    {
        string fallbackFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battleships");

        if (!Directory.Exists(fallbackFolderPath))
            Directory.CreateDirectory(fallbackFolderPath);

        string fallbackLogFilePath = Path.Combine(fallbackFolderPath, "ErrorFallbackLog.txt");

        if (!File.Exists(fallbackLogFilePath))
        {
            var file = File.Create(fallbackLogFilePath);
            file.Close();
        }

        return fallbackLogFilePath;
    }
}

public static class ServiceCollectionExtensions
{

    public static void ConfigureServices(this IServiceCollection services, string fallbackLogFilePath, string connectionString)
    {
        //Factories
        services.AddSingleton<IViewFactory, ViewFactory>();
        services.AddSingleton<ILoggerFactory>(provider => new EventLoggerFactory(fallbackLogFilePath));
        services.AddSingleton<IEventAggregator, EventAggregator>();

        //Views - singletons
        services.AddSingleton<MainWindow>();
        services.AddSingleton<PlayGameView>();

        //Views - transient
        services.AddTransient<HomeView>();
        services.AddTransient<GameTypeView>();
        services.AddTransient<LoadGameView>();
        services.AddTransient<SaveGameView>();
        services.AddTransient<DifficultyView>();
        services.AddTransient<GeneralRulesSetUpView>();
        services.AddTransient<ClassicRulesSetUpView>();
        services.AddTransient<SalvoRulesSetUpView>();
        services.AddTransient<PlayerStartsView>();
        services.AddTransient<ShipPlacementView>();

        //ViewModels - singletons
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PlayGameViewModel>();

        //ViewModels - transient
        services.AddTransient<HomeViewModel>();
        services.AddTransient<GameTypeViewModel>();
        services.AddTransient<LoadGameViewModel>();
        services.AddTransient<SaveGameViewModel>();
        services.AddTransient<DifficultyViewModel>();
        services.AddTransient<GeneralRulesSetUpViewModel>();
        services.AddTransient<ClassicRulesSetUpViewModel>();
        services.AddTransient<SalvoRulesSetUpViewModel>();
        services.AddTransient<PlayerStartsViewModel>();
        services.AddTransient<ShipPlacementViewModel>();

        //Services
        services.AddSingleton<ISaveService, SaveService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<IDatabaseInitializer>(provider => new DatabaseInitializer(new EventLoggerFactory(fallbackLogFilePath), connectionString));
        services.AddSingleton<IGameRepository>(provider => new GameRepository(new EventLoggerFactory(fallbackLogFilePath), connectionString));
        services.AddSingleton<IGameSetUpService, GameSetUpService>();
        services.AddSingleton<ISoundService, SoundService>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<ICaptainService, CaptainService>();
    }

}
