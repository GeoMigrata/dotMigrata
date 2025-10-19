using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces;

/// <summary>
/// Interface for components that want to observe simulation progress.
/// </summary>
public interface ISimulationListener
{
    /// <summary>
    /// Called before a simulation step begins.
    /// </summary>
    void OnStepStarted(World world, int step);

    /// <summary>
    /// Called after a simulation step completes.
    /// </summary>
    void OnStepCompleted(World world, int step);
}