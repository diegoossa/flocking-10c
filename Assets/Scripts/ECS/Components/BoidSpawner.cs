using Unity.Entities;

public struct BoidSpawner : IComponentData
{
    public float InitialVelocity;
    public float BoidDensity;
    public int RoundWorldSizeToMultiplesOf;
}

[InternalBufferCapacity(3)]
public struct BoidAgentData : IBufferElementData
{
    public Entity BoidAgentEntity;
    public float Acceleration;
    public float Drag;
}