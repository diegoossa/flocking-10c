using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct SimulateBoids : ISystem
{
    private EntityQuery _boidQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Boid, LocalToWorld>();
        _boidQuery = state.GetEntityQuery(queryBuilder);
        state.RequireForUpdate(_boidQuery);
        state.RequireForUpdate<BoidSimulator>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        var boidsL2W = _boidQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        var boidEntities = _boidQuery.ToEntityArray(Allocator.TempJob);

        var findNeighboursJob = new FindNeighboursJob
        {
            BoidsL2W = boidsL2W,
            BoidEntities = boidEntities,
            Radius = 3f
        };
        var jobHandle = findNeighboursJob.ScheduleParallel(_boidQuery, state.Dependency);

        jobHandle.Complete();
    }
}

public partial struct StepSimulationJob : IJobEntity
{
    private void Execute(ref BoidAspect boid)
    {
    }
}

[BurstCompile]
public partial struct FindNeighboursJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public NativeArray<LocalToWorld> BoidsL2W;
    [NativeDisableParallelForRestriction]
    public NativeArray<Entity> BoidEntities;
    public float Radius;

    private void Execute(ref DynamicBuffer<Neighbours> neighbours, in LocalToWorld l2w)
    {
        neighbours.Clear();
        for (var i = 0; i < BoidEntities.Length; i++)
        {
            var distance = math.distance(BoidsL2W[i].Position, l2w.Position);
            if (distance < Radius && distance > 0)
            {
                neighbours.Add(new Neighbours {Neighbour = BoidEntities[i]});
            }
        }
    }
}