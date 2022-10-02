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
    public readonly DynamicBuffer<AllNeighbourData> AllNeighbours;
    public readonly DynamicBuffer<TeamNeighbourData> TeamNeighbours;

    public Team Team => _team.ValueRO;

    public Boid Boid => _boid.ValueRO;
    
    public float3 Velocity
    {
        get => _boid.ValueRO.Velocity;
        set => _boid.ValueRW.Velocity = value;
    }
    
    public float3 Position
    {
        get => _boid.ValueRO.Position;
        set => _boid.ValueRW.Position = value;
    }
}