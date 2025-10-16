using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Battleships.MVVM.Factories
{
    /// <summary>
    /// Defines a contract for creating views (specifically UserControls) dynamically.
    /// This interface is used for retrieving UserControls with their dependencies injected.
    /// </summary>
    public interface IViewFactory
    {
        UserControl CreateView(Type newView);
    }

    /// <summary>
    /// A class that implements the <see cref="IViewFactory"/> interface and provides functionality 
    /// for retrieving UserControls from the Service Collection. It resolves the requested view from 
    /// the DI container and injects any required dependencies.
    /// </summary>
    public class ViewFactory(IServiceProvider serviceProvider) : IViewFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "Service Provider cannot be null.");

        /// <summary>
        /// Retrieves a UserControl of type <paramref name="viewType"/> from the Service Collection.
        /// The view is resolved and all required dependencies are injected automatically.
        /// </summary>
        /// <param name="viewType">The UserControl that needs to be displayed.</param>
        /// <returns>A UserControl from the Service Collection.</returns>
        /// <example>
        /// The following example shows how to set the CurrentView for binding to a ContentControl.
        /// <code>
        /// var viewFactory = new ViewFactory(serviceProvider);
        /// CurrentView = (UserControl)viewFactory.CreateView(typeof(HomeView));
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">Thrown when the requested UserControl is not 
        /// correctly registered in the ServiceCollection.</exception>
        /// <exception cref="ArgumentException">Thrown when the argument is not of type 
        /// UserControl.</exception>
        public UserControl CreateView(Type newView)
        {
            try
            {
                ValidateViewType(newView);

                return (UserControl)_serviceProvider.GetRequiredService(newView);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to resolve '{newView.Name}'. Ensure it is registered in the ServiceProvider as a UserControl.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"The provided type '{newView.Name}' is not a UserControl. Ensure correct DI registration.", ex);
            }
        }

        /// <summary>
        /// Validates the <paramref name="viewType"/> parameter to ensure that it inherits from UserControl.
        /// </summary>
        /// <param name="viewType">The view to be validated.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="viewType"/> parameter is 
        /// not a UserControl.</exception>
        private static void ValidateViewType(Type viewType)
        {
            if (viewType is null)
            {
                throw new ArgumentException("The viewType cannot be null.", nameof(viewType));
            }

            if (!typeof(UserControl).IsAssignableFrom(viewType))
            {
                throw new ArgumentException($"The type '{viewType.Name}' must inherit from UserControl.");
            }
        }

    }
}
