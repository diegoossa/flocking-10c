using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Boid World Settings
/// </summary>
public struct WorldSettings : IComponentData
{
    public float InitialVelocity;
    public float BoidDensity;
    public int RoundWorldSizeToMultiplesOf;
    public float3 HalfWorldSize;
}