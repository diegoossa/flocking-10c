using Unity.Entities;

/// <summary>
/// Spawner Component
/// </summary>
public struct BoidSpawner : IComponentData
{
}

/// <summary>
/// Buffer Element for Boid Agents
/// </summary>
[InternalBufferCapacity(3)]
public struct BoidAgentData : IBufferElementData
{
    public Entity BoidAgentEntity;
    public float Acceleration;
    public float Drag;
    public int TeamId;
}