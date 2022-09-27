using Unity.Entities;

public struct BoidSpawner : IComponentData
{
    
}

[InternalBufferCapacity(3)]
public struct BoidAgentData : IBufferElementData
{
    public Entity BoidAgentEntity;
    public float Acceleration;
    public float Drag;
}