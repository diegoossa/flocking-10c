using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

/// <summary>
/// Perform the Simulation of the Boids
/// </summary>
[BurstCompile]
[UpdateAfter(typeof(FindNeighbours))]
public partial struct SimulateBoids : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldSettings>();
        state.RequireForUpdate<BoidSimulationSettings>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var worldSettings = GetSingleton<WorldSettings>();
        var simulationSettings = GetSingleton<BoidSimulationSettings>();

        var deltaTime = Time.DeltaTime;

        var jobHandle = new AvoidInsideBoundsOfCubeJob
        {
            HalfWorldSize = worldSettings.HalfWorldSize,
            ViewRange = simulationSettings.ViewRange,
            DeltaTime = deltaTime
        }.ScheduleParallel(state.Dependency);

        jobHandle = new MatchVelocityJob
        {
            MatchVelocityRate = simulationSettings.MatchVelocityRate,
            DeltaTime = deltaTime,
        }.ScheduleParallel(jobHandle);

        jobHandle = new UpdateCoherenceJob
        {
            CoherenceRate = simulationSettings.CoherenceRate,
            DeltaTime = deltaTime,
        }.ScheduleParallel(jobHandle);

        jobHandle = new AvoidOthersJob
        {
            AvoidanceRange = simulationSettings.AvoidanceRange,
            AvoidanceRate = simulationSettings.AvoidanceRate,
            DeltaTime = deltaTime,
        }.ScheduleParallel(jobHandle);

        jobHandle = new UpdateVelocityJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(jobHandle);

        jobHandle = new UpdatePositionJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(jobHandle);
        jobHandle.Complete();
    }
}

/// <summary>
/// Move inside the World Bounds
/// </summary>
[BurstCompile]
public partial struct AvoidInsideBoundsOfCubeJob : IJobEntity
{
    public float3 HalfWorldSize;
    public float ViewRange;
    public float DeltaTime;

    private void Execute(ref Boid boid)
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

/// <summary>
/// Match velocity of the neighbour boids of the same team
/// </summary>
[BurstCompile]
public partial struct MatchVelocityJob : IJobEntity
{
    public float MatchVelocityRate;
    public float DeltaTime;

    private void Execute(ref Boid boid, in DynamicBuffer<TeamNeighbourData> teamData)
    {
        if (teamData.Length <= 0)
            return;

        var velocity = float3.zero;
        for (var i = 0; i < teamData.Length; ++i)
        {
            velocity += teamData[i].Velocity;
        }

        velocity /= teamData.Length;
        boid.Velocity += (velocity - boid.Velocity) * (MatchVelocityRate * DeltaTime);
    }
}

/// <summary>
/// Stay close to the neighbour boids of the same team
/// </summary>
[BurstCompile]
public partial struct UpdateCoherenceJob : IJobEntity
{
    public float CoherenceRate;
    public float DeltaTime;

    private void Execute(ref Boid boid, in DynamicBuffer<TeamNeighbourData> teamData)
    {
        if (teamData.Length <= 0)
            return;

        var center = teamData[0].Position;
        for (var i = 1; i < teamData.Length; ++i)
        {
            center += teamData[i].Position;
        }

        center *= 1.0f / teamData.Length;
        boid.Velocity += (center - boid.Position) * CoherenceRate * DeltaTime;
    }
}

/// <summary>
/// Avoid other boids
/// </summary>
[BurstCompile]
public partial struct AvoidOthersJob : IJobEntity
{
    [ReadOnly] public float AvoidanceRange;
    [ReadOnly] public float AvoidanceRate;
    [ReadOnly] public float DeltaTime;

    private void Execute(ref Boid boid, in DynamicBuffer<AllNeighbourData> allNeighbours)
    {
        if (allNeighbours.Length <= 0)
            return;

        var myPosition = boid.Position;
        var minDistSqr = AvoidanceRange * AvoidanceRange;
        var step = float3.zero;
        for (var i = 0; i < allNeighbours.Length; ++i)
        {
            var delta = myPosition - allNeighbours[i].Position;
            var deltaSqr = math.lengthsq(delta);
            if (deltaSqr > 0 && deltaSqr < minDistSqr)
            {
                step += delta / math.sqrt(deltaSqr);
            }
        }

        boid.Velocity += step * (AvoidanceRate * DeltaTime);
    }
}

/// <summary>
/// Update velocity based on the Team settings
/// </summary>
[BurstCompile]
public partial struct UpdateVelocityJob : IJobEntity
{
    public float DeltaTime;

    private void Execute(ref Boid boid, in Team team)
    {
        var velocity = boid.Velocity;
        velocity += math.normalize(velocity) * (team.Acceleration * DeltaTime);
        velocity *= 1.0f - 30.0f * team.Drag * DeltaTime;
        boid.Velocity = velocity;
    }
}

/// <summary>
/// Update position of the boids
/// </summary>
[BurstCompile]
public partial struct UpdatePositionJob : IJobEntity
{
    public float DeltaTime;
    private void Execute(ref Boid boid, ref LocalTransform localTransform)
    {
        boid.Position += boid.Velocity * DeltaTime;
        boid.Position = boid.Position;
        localTransform.Rotation = quaternion.LookRotationSafe(boid.Position + boid.Velocity, math.up());
    }
}