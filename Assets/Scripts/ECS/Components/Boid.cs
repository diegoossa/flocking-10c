using Unity.Entities;
using Unity.Mathematics;

public struct Boid : IComponentData { }

public struct Velocity : IComponentData
{
    public float3 Value;
}