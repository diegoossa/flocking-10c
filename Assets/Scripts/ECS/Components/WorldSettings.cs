using Unity.Entities;
using Unity.Mathematics;

public struct WorldSettings : IComponentData
{
    public float InitialVelocity;
    public float BoidDensity;
    public int RoundWorldSizeToMultiplesOf;
    public float3 HalfWorldSize;
}