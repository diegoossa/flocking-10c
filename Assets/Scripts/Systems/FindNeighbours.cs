using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
        var boidSimulation = SystemAPI.GetSingleton<BoidSimulionSettings>();
        var boids = _boidQuery.ToComponentDataArray<Boid>(Allocator.TempJob);
        var boidEntities = _boidQuery.ToEntityArray(Allocator.TempJob);
        
        new FindNeighboursJob
        {
            Boids = boids,
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
    public float Radius;

    private void Execute(ref DynamicBuffer<AllNeighbours> allNeighbours, ref DynamicBuffer<TeamNeighbours> teamNeighbours, in Boid boid)
    {
        teamNeighbours.Clear();
        allNeighbours.Clear();
        
        for (var i = 0; i < Boids.Length; i++)
        {
            var distance = math.distance(Boids[i].Position, boid.Position);
            if (distance < Radius && distance > 0.1f)
            {
                allNeighbours.Add(new AllNeighbours
                {
                    Position = Boids[i].Position, 
                });
                if (Boids[i].TeamId == boid.TeamId)
                {
                    teamNeighbours.Add(new TeamNeighbours
                    {
                        Position = Boids[i].Position, 
                        Velocity = Boids[i].Velocity,
                    });
                }
            }
        }
    }
}