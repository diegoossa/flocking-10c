using Unity.Entities;
using Unity.Mathematics;

public struct Boid : IComponentData { }

public struct Velocity : IComponentData
{
    public float3 Value;
}

public struct Team : IComponentData
{
    public float Acceleration;
    public float Drag;
}

public struct Neighbours : IBufferElementData
{
    public Entity Neighbour;
}