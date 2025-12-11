using dotMigrata.Logic.Interfaces;
using dotMigrata.Simulation.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace dotMigrata.Simulation.Extensions;

/// <summary>
/// Configuration options for dotMigrata services.
/// </summary>
public sealed class DotMigrataOptions
{
    private readonly List<Action<IServiceCollection>> _configurations = [];

    /// <summary>
    /// Registers a custom attraction calculator.
    /// </summary>
    /// <typeparam name="TCalculator">The attraction calculator type.</typeparam>
    public void UseAttractionCalculator<TCalculator>() where TCalculator : class, IAttractionCalculator
    {
        _configurations.Add(services => services.AddAttractionCalculator<TCalculator>());
    }

    /// <summary>
    /// Registers a custom migration calculator.
    /// </summary>
    /// <typeparam name="TCalculator">The migration calculator type.</typeparam>
    public void UseMigrationCalculator<TCalculator>() where TCalculator : class, IMigrationCalculator
    {
        _configurations.Add(services => services.AddMigrationCalculator<TCalculator>());
    }

    /// <summary>
    /// Registers a custom stability criteria.
    /// </summary>
    /// <typeparam name="TCriteria">The stability criteria type.</typeparam>
    public void UseStabilityCriteria<TCriteria>() where TCriteria : class, IStabilityCriteria
    {
        _configurations.Add(services => services.AddStabilityCriteria<TCriteria>());
    }

    /// <summary>
    /// Adds a custom simulation stage.
    /// </summary>
    /// <typeparam name="TStage">The stage type.</typeparam>
    public void AddStage<TStage>() where TStage : class, ISimulationStage
    {
        _configurations.Add(services => services.AddSimulationStage<TStage>());
    }

    /// <summary>
    /// Applies all configured options to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    internal void ApplyTo(IServiceCollection services)
    {
        foreach (var configure in _configurations)
            configure(services);
    }
}