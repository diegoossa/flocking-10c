using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct FindNeighbours : ISystem
{
    private EntityQuery _boidQuery;
    
    public void OnCreate(ref SystemState state)
    {
        using var queryBuilder = new EntityQueryBuilder(Allocator.TempJob)
            .WithAll<Boid>();
        _boidQuery = state.GetEntityQuery(queryBuilder);
        state.RequireForUpdate(_boidQuery);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var boidSimulation = SystemAPI.GetSingleton<BoidSimulator>();
        var boids = _boidQuery.ToComponentDataArray<Boid>(Allocator.TempJob);
        var boidEntities = _boidQuery.ToEntityArray(Allocator.TempJob);
        
        new FindNeighboursJob
        {
            Boids = boids,
            BoidEntities = boidEntities,
            Radius = boidSimulation.ViewRange
        }.ScheduleParallel(_boidQuery, state.Dependency);
        
        state.CompleteDependency();
        boids.Dispose();
        boidEntities.Dispose();
    }
}

[BurstCompile]
public partial struct FindNeighboursJob : IJobEntity
{
    [ReadOnly] [NativeDisableParallelForRestriction]
    public NativeArray<Boid> Boids;
    [ReadOnly] [NativeDisableParallelForRestriction]
    public NativeArray<Entity> BoidEntities;
    public float Radius;

    private void Execute(ref DynamicBuffer<Neighbours> neighbours, in LocalToWorldTransform localToWorldTransform)
    {
        neighbours.Clear();
        for (var i = 0; i < BoidEntities.Length; i++)
        {
            var distance = math.distance(Boids[i].Position, localToWorldTransform.Value.Position);
            if (distance < Radius && distance > 0)
            {
                neighbours.Add(new Neighbours
                {
                    Entity = BoidEntities[i], 
                    Position = Boids[i].Position, 
                    Velocity = Boids[i].Velocity,
                    TeamId = Boids[i].TeamId
                });
            }
        }
    }
}