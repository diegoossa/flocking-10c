using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
[UpdateAfter(typeof(FindNeighbours))]
public partial struct SimulateBoids : ISystem
{
    private EntityQuery _boidQuery;
    private ComponentLookup<Boid> _boidLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        using var queryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Boid, LocalToWorldTransform>();
        _boidQuery = state.GetEntityQuery(queryBuilder);
        state.RequireForUpdate(_boidQuery);
        state.RequireForUpdate<WorldSettings>();
        state.RequireForUpdate<BoidSimulator>();

        _boidLookup = state.GetComponentLookup<Boid>(true);
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _boidLookup.Update(ref state);

        var worldSettings = GetSingleton<WorldSettings>();
        var boidSimulation = GetSingleton<BoidSimulator>();

        var deltaTime = Time.DeltaTime;

        var jobHandle = new AvoidInsideBoundsOfCubeJob
        {
            HalfWorldSize = worldSettings.HalfWorldSize,
            ViewRange = boidSimulation.ViewRange,
            DeltaTime = deltaTime
        }.ScheduleParallel(_boidQuery, state.Dependency);

        jobHandle = new MatchVelocityJob
        {
            MatchVelocityRate = boidSimulation.MatchVelocityRate,
            DeltaTime = deltaTime,
            BoidLookup = _boidLookup
        }.ScheduleParallel(_boidQuery, jobHandle);

        jobHandle = new UpdateCoherenceJob
        {
            CoherenceRate = boidSimulation.CoherenceRate,
            DeltaTime = deltaTime,
            BoidLookup = _boidLookup
        }.ScheduleParallel(_boidQuery, jobHandle);

        jobHandle = new AvoidOthersJob
        {
            AvoidanceRange = boidSimulation.AvoidanceRange,
            AvoidanceRate = boidSimulation.AvoidanceRate,
            DeltaTime = deltaTime,
            BoidLookup = _boidLookup
        }.ScheduleParallel(_boidQuery, jobHandle);

        jobHandle = new UpdateBoidJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(_boidQuery, jobHandle);

        jobHandle.Complete();
    }
}


[BurstCompile]
public partial struct AvoidInsideBoundsOfCubeJob : IJobEntity
{
    public float3 HalfWorldSize;
    public float ViewRange;
    public float DeltaTime;

    private void Execute(ref BoidAspect boid)
    {
        boid.Velocity -= new float3(
            math.max(math.abs(boid.Position.x) - HalfWorldSize.x + ViewRange, 0) *
            math.sign(boid.Position.x) * 5f * DeltaTime,
            math.max(math.abs(boid.Position.y) - HalfWorldSize.y + ViewRange, 0) *
            math.sign(boid.Position.y) * 5f * DeltaTime,
            math.max(math.abs(boid.Position.z) - HalfWorldSize.z + ViewRange, 0) *
            math.sign(boid.Position.z) * 5f * DeltaTime);
    }
}

[BurstCompile]
public partial struct MatchVelocityJob : IJobEntity
{
    public float MatchVelocityRate;
    public float DeltaTime;
    [NativeDisableParallelForRestriction]
    [ReadOnly] public ComponentLookup<Boid> BoidLookup;

    private void Execute(ref BoidAspect boid)
    {
        if (boid.Neighbours.Length > 0)
        {
            var velocity = float3.zero;
            for (var i = 0; i < boid.Neighbours.Length; ++i)
            {
                if (BoidLookup.TryGetComponent(boid.Neighbours[i].Neighbour, out var neighbourBoid))
                {
                    if (neighbourBoid.TeamId == boid.TeamId)
                    {
                        velocity += neighbourBoid.Velocity;
                    }
                }
            }

            velocity /= boid.Neighbours.Length;
            boid.Velocity += (velocity - boid.Velocity) * MatchVelocityRate * DeltaTime;
        }
    }
}

[BurstCompile]
public partial struct UpdateCoherenceJob : IJobEntity
{
    public float CoherenceRate;
    public float DeltaTime;

    [NativeDisableParallelForRestriction] [ReadOnly]
    public ComponentLookup<Boid> BoidLookup;

    private void Execute(ref BoidAspect boid)
    {
        if (boid.Neighbours.Length > 0)
        {
            if (BoidLookup.TryGetComponent(boid.Neighbours[0].Neighbour, out var firstNeighbour))
            {
                var center = firstNeighbour.Position;
                for (var i = 1; i < boid.Neighbours.Length; ++i)
                {
                    if (BoidLookup.TryGetComponent(boid.Neighbours[i].Neighbour, out var neighbourBoid))
                    {
                        if (neighbourBoid.TeamId == boid.TeamId)
                        {
                            center += neighbourBoid.Position;
                        }
                    }
                }
                center *= 1.0f / boid.Neighbours.Length;
                boid.Velocity += (center - boid.Transform.Position) * CoherenceRate * DeltaTime;
            }
        }
    }
}

[BurstCompile]
public partial struct AvoidOthersJob : IJobEntity
{
    public float AvoidanceRange;
    public float AvoidanceRate;
    public float DeltaTime;

    [NativeDisableParallelForRestriction] [ReadOnly]
    public ComponentLookup<Boid> BoidLookup;

    private void Execute(ref BoidAspect boid)
    {
        if (boid.Neighbours.Length > 0)
        {
            var myPosition = boid.Transform.Position;
            var minDistSqr = AvoidanceRange * AvoidanceRange;
            var step = float3.zero;
            for (var i = 0; i < boid.Neighbours.Length; ++i)
            {
                if (BoidLookup.TryGetComponent(boid.Neighbours[i].Neighbour, out var neighbour))
                {
                    var delta = myPosition - neighbour.Position;
                    var deltaSqr = math.lengthsq(delta);
                    if (deltaSqr > 0 && deltaSqr < minDistSqr)
                    {
                        step += delta / math.sqrt(deltaSqr);
                    }
                }
            }
            boid.Velocity += step * AvoidanceRate * DeltaTime;
        }
    }
}

[BurstCompile]
public partial struct UpdateBoidJob : IJobEntity
{
    public float DeltaTime;

    private void Execute(ref BoidAspect boid)
    {
        var velocity = boid.Velocity;
        velocity += math.normalize(velocity) * (boid.Team.Acceleration * DeltaTime);
        velocity *= 1f - 30f * boid.Team.Drag * DeltaTime;
        boid.Velocity = velocity;
        boid.Transform.Position += boid.Velocity * DeltaTime;
    }
}