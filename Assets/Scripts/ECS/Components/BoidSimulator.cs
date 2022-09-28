using Unity.Entities;

public struct BoidSimulator : IComponentData
{
    public float InitialVelocity;
    public float MatchVelocityRate;
    public float AvoidanceRange;
    public float AvoidanceRate;
    public float CoherenceRate;
    public float ViewRange;
}