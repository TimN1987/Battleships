using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel;
using Moq;

namespace BattleshipsTests.Tests.Factories
{
    public class ViewFactoryTests : IDisposable
    {
        #region Fields
        private readonly Mock<IServiceProvider> _serviceProvider;
        private readonly Mock<IEventAggregator> _eventAggregator;
        private readonly Mock<ISaveService> _saveService;
        private readonly Mock<IGameRepository> _gameRepository;
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<IGameSetUpService> _gameSetUpService;
        private readonly Mock<IEventLogger> _eventLogger;

        private readonly Mock<HomeViewModel> _homeViewModel;
        private readonly Mock<ClassicRulesSetUpViewModel> _classicRulesSetUpViewModel;
        private readonly Mock<ShipPlacementViewModel> _shipPlacementViewModel;
        private readonly Mock<PlayGameViewModel> _playGameViewModel;

        private readonly Mock<CreateGameEvent> _createGameEvent;
        #endregion //Fields

        public ViewFactoryTests()
        {
            _serviceProvider = new Mock<IServiceProvider>();
            _eventAggregator = new Mock<IEventAggregator>();
            _gameRepository = new Mock<IGameRepository>();
            _saveService = new Mock<ISaveService>();
            _loggerFactory = new Mock<ILoggerFactory>();
            _gameSetUpService = new Mock<IGameSetUpService>();
            _eventLogger = new Mock<IEventLogger>();

            _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
              .Returns(_eventLogger.Object);

            _createGameEvent = new Mock<CreateGameEvent>();

            _eventAggregator.Setup(ea => ea.GetEvent<CreateGameEvent>())
                .Returns(_createGameEvent.Object);

            _homeViewModel = new Mock<HomeViewModel>(_eventAggregator.Object, _gameRepository.Object, _loggerFactory.Object);
            _classicRulesSetUpViewModel = new Mock<ClassicRulesSetUpViewModel>(_eventAggregator.Object, _gameSetUpService.Object);
            _shipPlacementViewModel = new Mock<ShipPlacementViewModel>(_eventAggregator.Object, _gameSetUpService.Object);
            _playGameViewModel = new Mock<PlayGameViewModel>(_eventAggregator.Object, _saveService.Object, _loggerFactory.Object);

            _serviceProvider
                .Setup(sp => sp.GetService(typeof(HomeViewModel)))
                .Returns(_homeViewModel.Object);
            _serviceProvider
                .Setup(sp => sp.GetService(typeof(ClassicRulesSetUpViewModel)))
                .Returns(_classicRulesSetUpViewModel.Object);
            _serviceProvider
                .Setup(sp => sp.GetService(typeof(ShipPlacementViewModel)))
                .Returns(_shipPlacementViewModel.Object);
            _serviceProvider
                .Setup(sp => sp.GetService(typeof(PlayGameViewModel)))
                .Returns(_playGameViewModel.Object);
        }

        /// <summary>
        /// Tests the constructor of the ViewFactory class. It checks if the constructor throws an 
        /// ArgumentNullException when the ServiceProvider is null.
        /// </summary>
        #region Constructor Tests
        [Fact]
        public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ViewFactory(null!));

            //Assert
            Assert.Contains("Service Provider cannot be null.", exception.Message);
        }

        /// <summary>
        /// Tests the constructor of the ViewFactory class. It checks that the view factory property is 
        /// not null after instantiation.
        /// </summary>
        [Fact]
        public void Constructor_ValidServiceProvider_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var viewFactory = new ViewFactory(_serviceProvider.Object);

            //Assert
            Assert.NotNull(viewFactory);
        }
        #endregion //Constructor Tests

        #region View Creation Tests
        /// <summary>
        /// Calls the CreateView method with a null argument. It should throw a NullReferenceException when 
        /// the method tries to access the null type.
        /// </summary>
        [Fact]
        public void CreateView_NullType_ThrowsNullReferenceException()
        {
            //Arrange
            var viewFactory = new ViewFactory(_serviceProvider.Object);

            //Act & Assert
            var exception = Assert.Throws<NullReferenceException>(() =>
                                        viewFactory.CreateView(null!));
        }

        /// <summary>
        /// Tests the CreateView method with an invalid type. It should throw an ArgumentException.
        /// </summary>
        [Fact]
        public void CreateView_InvalidType_ThrowsArgumentException()
        {
            // Arrange
            var viewFactory = new ViewFactory(_serviceProvider.Object);
            var invalidType = typeof(string);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                viewFactory.CreateView(invalidType));
        }

        /// <summary>
        /// Tests the CreateView method with a UserControl type that is not registered in the ServiceProvider.
        /// </summary>
        [Fact]
        public void CreateView_WhenViewTypeIsUnregistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var viewFactory = new ViewFactory(_serviceProvider.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                viewFactory.CreateView(typeof(TestView)));
            Assert.Equal("Failed to resolve 'TestView'. Ensure it is registered in the ServiceProvider as a UserControl.", exception.Message);
        }
        #endregion //View Creation Tests

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}