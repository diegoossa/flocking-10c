using Unity.Entities;
using Unity.Transforms;
#pragma warning disable CS0414

public readonly partial struct BoidSpawnerAspect : IAspect
{
    private readonly RefRO<BoidSpawner> _boidSpawner;
    public readonly DynamicBuffer<BoidAgentData> BoidAgentBuffer;
    public BoidSpawner BoidSpawner => _boidSpawner.ValueRO;
}

public readonly partial struct BoidAspect : IAspect
{
    public readonly Entity Self;
    private readonly RefRO<Boid> _boid;
    private readonly RefRO<Team> _team;
    private readonly RefRW<Velocity> _velocity;
    public readonly TransformAspect Transform;
    public readonly DynamicBuffer<Neighbours> Neighbours;

    public Team Team => _team.ValueRO;
    
    public Velocity Velocity
    {
        get => _velocity.ValueRO;
        set => _velocity.ValueRW = value;
    }
}