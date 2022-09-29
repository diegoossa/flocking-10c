using Unity.Entities;

public struct BoidSpawner : IComponentData
{
    public float InitialVelocity;
}

[InternalBufferCapacity(3)]
public struct BoidAgentData : IBufferElementData
{
    public Entity BoidAgentEntity;
    public float Acceleration;
    public float Drag;
    public int TeamId;
}