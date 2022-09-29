using Unity.Entities;
using Unity.Mathematics;

public struct BoidSimulator : IComponentData
{
    // Simulation settings
    public float InitialVelocity;
    public float MatchVelocityRate;
    public float AvoidanceRange;
    public float AvoidanceRate;
    public float CoherenceRate;
    public float ViewRange;
}

public struct WorldSize : IComponentData
{
    public float3 Value;
}