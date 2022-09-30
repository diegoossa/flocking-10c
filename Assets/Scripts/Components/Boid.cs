using Unity.Entities;
using Unity.Mathematics;

public struct Boid : IComponentData
{
    public int TeamId;
    public float3 Velocity;
    public float3 Position;
}

public struct Team : IComponentData
{
    public float Acceleration;
    public float Drag;
}

public struct AllNeighbours : IBufferElementData
{
    public float3 Position;
}

public struct TeamNeighbours : IBufferElementData
{
    public float3 Velocity;
    public float3 Position;
}