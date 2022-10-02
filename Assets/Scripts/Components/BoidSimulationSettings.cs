using Unity.Entities;

/// <summary>
/// Settings for the simulation
/// </summary>
public struct BoidSimulationSettings : IComponentData
{
    public float MatchVelocityRate;
    public float AvoidanceRange;
    public float AvoidanceRate;
    public float CoherenceRate;
    public float ViewRange;
}