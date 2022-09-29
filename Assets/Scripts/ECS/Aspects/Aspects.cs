using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
#pragma warning disable CS0414

public readonly partial struct BoidSpawnerAspect : IAspect
{
    private readonly RefRO<BoidSpawner> _boidSpawner;
    public readonly DynamicBuffer<BoidAgentData> BoidAgentBuffer;
}

public readonly partial struct BoidAspect : IAspect
{
    public readonly Entity Self;
    private readonly RefRW<Boid> _boid;
    private readonly RefRO<Team> _team;
    public readonly TransformAspect Transform;
    public readonly DynamicBuffer<Neighbours> Neighbours;

    public Team Team => _team.ValueRO;

    public int TeamId => _boid.ValueRO.TeamId;

    public float3 Velocity
    {
        get => _boid.ValueRO.Velocity;
        set => _boid.ValueRW.Velocity = value;
    }
    
    public float3 Position
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }
}