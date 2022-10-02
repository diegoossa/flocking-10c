using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/* Quadrants based on CodeMonkey https://www.youtube.com/watch?v=hP4Vu6JbzSo */

/// <summary>
/// System to Find Neighbours of each boid
/// </summary>
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial struct FindNeighbours : ISystem
{
    // Quadrant Settings
    private const int QuadrantMultiplier = 1000;
    private const int QuadrantCellSize = 50;
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
        var boidSimulation = SystemAPI.GetSingleton<BoidSimulationSettings>();
        var boids = _boidQuery.ToComponentDataArray<Boid>(Allocator.TempJob);

        // Get the boids inside each Quadrant
        var quadrantMultiHashMap = new NativeMultiHashMap<int, Boid>(boids.Length, Allocator.TempJob);
        var setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            QuadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter()
        };
        var jobHandle = setQuadrantDataHashMapJob.ScheduleParallel(state.Dependency);

        // Find neighbours on the quadrant
        jobHandle = new FindNeighboursInQuadrantJob
        {
            QuadrantMultiHashMap = quadrantMultiHashMap,
            Radius = boidSimulation.ViewRange
        }.ScheduleParallel(_boidQuery, jobHandle);

        jobHandle.Complete();

        boids.Dispose();
        quadrantMultiHashMap.Dispose();
    }

    public static int GetHashMapKey(float3 position)
    {
        return (int) (math.floor(position.x / QuadrantCellSize) +
                      QuadrantMultiplier * math.floor(position.y / QuadrantCellSize) +
                      QuadrantMultiplier * math.floor(position.z / QuadrantCellSize));
    }
}

[BurstCompile]
public partial struct SetQuadrantDataHashMapJob : IJobEntity
{
    public NativeMultiHashMap<int, Boid>.ParallelWriter QuadrantMultiHashMap;

    private void Execute(in BoidAspect boid)
    {
        var hashKey = FindNeighbours.GetHashMapKey(boid.Position);
        QuadrantMultiHashMap.Add(hashKey, boid.Boid);
    }
}

[BurstCompile]
public partial struct FindNeighboursInQuadrantJob : IJobEntity
{
    [ReadOnly] public NativeMultiHashMap<int, Boid> QuadrantMultiHashMap;
    public float Radius;

    private void Execute(ref DynamicBuffer<AllNeighbourData> allNeighbours, ref DynamicBuffer<TeamNeighbourData> teamNeighbours, in Boid boid)
    {
        teamNeighbours.Clear();
        allNeighbours.Clear();

        var hashKey = FindNeighbours.GetHashMapKey(boid.Position);
        if (QuadrantMultiHashMap.TryGetFirstValue(hashKey, out var boidInQuadrant, out var iterator))
        {
            do
            {
                var distance = math.distance(boidInQuadrant.Position, boid.Position);
                if (distance < Radius && distance > 0.1f)
                {
                    allNeighbours.Add(new AllNeighbourData
                    {
                        Position = boidInQuadrant.Position,
                    });
                    if (boidInQuadrant.TeamId == boid.TeamId)
                    {
                        teamNeighbours.Add(new TeamNeighbourData
                        {
                            Position = boidInQuadrant.Position,
                            Velocity = boidInQuadrant.Velocity,
                        });
                    }
                }
            } while (QuadrantMultiHashMap.TryGetNextValue(out boidInQuadrant, ref iterator));
        }
    }
}

/*
[BurstCompile]
public partial struct FindNeighboursJob : IJobEntity
{
    [ReadOnly] [NativeDisableParallelForRestriction]
    public NativeArray<Boid> Boids;

    public float Radius;

    private void Execute(ref DynamicBuffer<AllNeighbours> allNeighbours,
        ref DynamicBuffer<TeamNeighbours> teamNeighbours, in Boid boid)
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
*/