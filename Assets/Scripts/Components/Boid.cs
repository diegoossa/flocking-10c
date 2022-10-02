using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Boid Data
/// </summary>
public struct Boid : IComponentData
{
    public int TeamId;
    public float3 Velocity;
    public float3 Position;
}

/// <summary>
/// Team Settings
/// </summary>
public struct Team : IComponentData
{
    public float Acceleration;
    public float Drag;
}

/// <summary>
/// Buffer Element of neighbour boids within a range
/// </summary>
public struct AllNeighbourData : IBufferElementData
{
    public float3 Position;
}

/// <summary>
/// Buffer Element of neighbour boids of the same team within a range
/// </summary>
public struct TeamNeighbourData : IBufferElementData
{
    public float3 Velocity;
    public float3 Position;
}