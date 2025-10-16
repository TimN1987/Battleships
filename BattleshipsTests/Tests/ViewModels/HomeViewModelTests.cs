using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.Services;
using Battleships.MVVM.ViewModel;
using Battleships.MVVM.Factories;
using Moq;
using System.Reflection;
using Battleships.MVVM.View;

namespace BattleshipsTests.Tests.ViewModels
{
    public class HomeViewModelTests
    {
        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IEventLogger> _eventLoggerMock;
        private readonly HomeViewModel _viewModel;

        public HomeViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _eventLoggerMock = new Mock<IEventLogger>();

            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_eventLoggerMock.Object);

            _gameRepositoryMock
                .Setup(repo => repo.CheckForAutosaveFile())
                .ReturnsAsync(false);

            _viewModel = new HomeViewModel(
                _eventAggregatorMock.Object,
                _gameRepositoryMock.Object,
                _loggerFactoryMock.Object);
        }

        #region Command Tests
        /// <summary>
        /// Tests that the NewGameCommand, ContinueGameCommand, and LoadGameCommand are not null when initialized.
        /// </summary>
        [Fact]
        public void Commands_AreInitialized_NotNull()
        {
            // Act
            var newGameCommand = _viewModel.NewGameCommand;
            var continueGameCommand = _viewModel.ContinueGameCommand;
            var loadGameCommand = _viewModel.LoadGameCommand;

            // Assert
            Assert.NotNull(newGameCommand);
            Assert.NotNull(continueGameCommand);
            Assert.NotNull(loadGameCommand);
        }

        /// <summary>
        /// Tests that the NewGameCommand can be executed. CanExecute should return the same boolean value 
        /// as the AutosaveFileExists property.
        /// </summary>
        /// <param name="autosaveExists">A boolean value used to set the test AutosaveFileExists property.</param>
        /// <param name="expectedCanExecute">A boolean value representing the expected return from CanExecute.</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void ContinueGameCommand_CanExecute_ShouldMatchAutosaveFileExists(bool autosaveExists, bool expectedCanExecute)
        {
            // Arrange
            _viewModel.AutosaveFileExists = autosaveExists;

            // Act
            var canExecute = _viewModel.ContinueGameCommand.CanExecute(null);

            // Assert
            Assert.Equal(expectedCanExecute, canExecute);
        }

        /// <summary>
        /// Tests that the NewGameCommand successfully publishes a NavigationEvent to the EventAggregator of 
        /// tpye GameTypeView.
        /// </summary>
        [Fact]
        public void NewGameCommand_NavigateToGameTypeViewCalled_EventAggregatorCalledOnce()
        {
            // Arrange
            var eventMock = new Mock<NavigationEvent>();
            _eventAggregatorMock.Setup(agg => agg.GetEvent<NavigationEvent>()).Returns(eventMock.Object);

            // Act
            _viewModel.NewGameCommand.Execute(null);

            // Assert
            eventMock.Verify(evt => evt.Publish(typeof(GameTypeView)), Times.Once);
        }

        /// <summary>
        /// Tests that the LoadGameCommand successfully publishes a NavigationEvent to the EventAggregator of 
        /// type LoadGameView.
        /// </summary>
        [Fact]
        public void LoadGameCommand_NavigateToLoadGameViewCalled_EventAggregatorCalledOnce()
        {
            // Arrange
            var eventMock = new Mock<NavigationEvent>();
            _eventAggregatorMock.Setup(agg => agg.GetEvent<NavigationEvent>()).Returns(eventMock.Object);

            // Act
            _viewModel.LoadGameCommand.Execute(null);

            // Assert
            eventMock.Verify(evt => evt.Publish(typeof(LoadGameView)), Times.Once);
        }

        /// <summary>
        /// Tests that the ContinueGameCommand successfully publishes a NavigationEvent to the EventAggregator 
        /// of type PlayGameView and an AutosaveEvent to indicate that the autosave game should be loaded.
        /// </summary>
        [Fact]
        public void ContinueGameCommand_ShouldPublish_NavigationAndAutosaveEvents()
        {
            // Arrange
            var navigationEventMock = new Mock<NavigationEvent>();
            var autosaveEventMock = new Mock<AutosaveEvent>();

            _eventAggregatorMock.Setup(x => x.GetEvent<NavigationEvent>())
                                .Returns(navigationEventMock.Object);
            _eventAggregatorMock.Setup(x => x.GetEvent<AutosaveEvent>())
                                .Returns(autosaveEventMock.Object);

            // Act
            _viewModel.ContinueGameCommand.Execute(null);

            // Assert
            navigationEventMock.Verify(ev => ev.Publish(typeof(PlayGameView)), Times.Once);
            autosaveEventMock.Verify(ev => ev.Publish(), Times.Once);
        }

        #endregion //Command Tests

    }
}
