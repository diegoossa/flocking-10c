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
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Boid, LocalToWorldTransform>();
        _boidQuery = state.GetEntityQuery(queryBuilder);
        state.RequireForUpdate(_boidQuery);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var boidSimulation = SystemAPI.GetSingleton<BoidSimulator>();
        var boidsLocalToWorld = _boidQuery.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);
        var boidEntities = _boidQuery.ToEntityArray(Allocator.TempJob);
        
        new FindNeighboursJob
        {
            BoidsLocalToWorldTransforms = boidsLocalToWorld,
            BoidEntities = boidEntities,
            Radius = boidSimulation.ViewRange
        }.ScheduleParallel(_boidQuery, state.Dependency);
        
        state.CompleteDependency();
    }
}

[BurstCompile]
public partial struct FindNeighboursJob : IJobEntity
{
    [ReadOnly] [NativeDisableParallelForRestriction]
    public NativeArray<LocalToWorldTransform> BoidsLocalToWorldTransforms;

    [ReadOnly] [NativeDisableParallelForRestriction]
    public NativeArray<Entity> BoidEntities;
    public float Radius;

    private void Execute(ref DynamicBuffer<Neighbours> neighbours, in LocalToWorldTransform localToWorldTransform)
    {
        neighbours.Clear();
        for (var i = 0; i < BoidEntities.Length; i++)
        {
            var distance = math.distance(BoidsLocalToWorldTransforms[i].Value.Position,
                localToWorldTransform.Value.Position);
            if (distance < Radius && distance > 0)
            {
                neighbours.Add(new Neighbours {Neighbour = BoidEntities[i]});
            }
        }
    }
}