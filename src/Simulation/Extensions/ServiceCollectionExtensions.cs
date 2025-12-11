using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Simulation.Builders;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Pipeline;
using dotMigrata.Simulation.Stability;
using dotMigrata.Snapshot.Migration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotMigrata.Simulation.Extensions;

/// <summary>
/// Provides extension methods for configuring dotMigrata services in an IServiceCollection.
/// </summary>
/// <remarks>
/// These extension methods follow Microsoft's dependency injection best practices
/// and allow seamless integration with ASP.NET Core and other DI-enabled applications.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <param name="services">The service collection to add services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds dotMigrata core services to the service collection with default implementations.
        /// </summary>
        /// <returns>The service collection for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddDotMigrata()
        {
            ArgumentNullException.ThrowIfNull(services);

            // Core calculators
            services.TryAddSingleton<IAttractionCalculator, StandardAttractionCalculator>();
            services.TryAddSingleton<IMigrationCalculator, StandardMigrationCalculator>();

            // Stability criteria
            services.TryAddSingleton<IStabilityCriteria, DefaultStabilityCriteria>();

            // Snapshot migration manager
            services.TryAddSingleton<SnapshotMigrationManager>();

            // Simulation stages
            services.TryAddTransient<MigrationDecisionStage>();
            services.TryAddTransient<MigrationExecutionStage>();

            // Builders
            services.TryAddTransient<SimulationBuilder>();

            return services;
        }

        /// <summary>
        /// Adds dotMigrata core services with a custom configuration action.
        /// </summary>
        /// <param name="configure">Action to configure dotMigrata options.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or configure is null.</exception>
        public IServiceCollection AddDotMigrata(Action<DotMigrataOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            var options = new DotMigrataOptions();
            configure(options);

            services.AddDotMigrata();

            // Apply custom registrations from options
            options.ApplyTo(services);

            return services;
        }

        /// <summary>
        /// Registers a custom attraction calculator implementation.
        /// </summary>
        /// <typeparam name="TCalculator">The attraction calculator type to register.</typeparam>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddAttractionCalculator<TCalculator>()
            where TCalculator : class, IAttractionCalculator
        {
            services.Replace(ServiceDescriptor.Singleton<IAttractionCalculator, TCalculator>());
            return services;
        }

        /// <summary>
        /// Registers a custom migration calculator implementation.
        /// </summary>
        /// <typeparam name="TCalculator">The migration calculator type to register.</typeparam>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddMigrationCalculator<TCalculator>()
            where TCalculator : class, IMigrationCalculator
        {
            services.Replace(ServiceDescriptor.Singleton<IMigrationCalculator, TCalculator>());
            return services;
        }

        /// <summary>
        /// Registers a custom stability criteria implementation.
        /// </summary>
        /// <typeparam name="TCriteria">The stability criteria type to register.</typeparam>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddStabilityCriteria<TCriteria>()
            where TCriteria : class, IStabilityCriteria
        {
            services.Replace(ServiceDescriptor.Singleton<IStabilityCriteria, TCriteria>());
            return services;
        }

        /// <summary>
        /// Registers a custom simulation stage.
        /// </summary>
        /// <typeparam name="TStage">The stage type to register.</typeparam>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddSimulationStage<TStage>()
            where TStage : class, ISimulationStage
        {
            services.AddTransient<ISimulationStage, TStage>();
            return services;
        }
    }
}
