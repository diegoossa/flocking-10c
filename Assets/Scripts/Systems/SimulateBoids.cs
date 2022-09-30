using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
[UpdateAfter(typeof(FindNeighbours))]
public partial struct SimulateBoids : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldSettings>();
        state.RequireForUpdate<BoidSimulionSettings>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var worldSettings = GetSingleton<WorldSettings>();
        var simulationSettings = GetSingleton<BoidSimulionSettings>();

        var deltaTime = SystemAPI.Time.DeltaTime;

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

    private void Execute(ref BoidAspect boid)
    {
        if (boid.TeamNeighbours.Length <= 0)
            return;

        var velocity = float3.zero;
        for (var i = 0; i < boid.TeamNeighbours.Length; ++i)
        {
            velocity += boid.TeamNeighbours[i].Velocity;
        }

        velocity /= boid.TeamNeighbours.Length;
        boid.Velocity += (velocity - boid.Velocity) * (MatchVelocityRate * DeltaTime);
    }
}

[BurstCompile]
public partial struct UpdateCoherenceJob : IJobEntity
{
    public float CoherenceRate;
    public float DeltaTime;

    private void Execute(ref BoidAspect boid)
    {
        if (boid.TeamNeighbours.Length <= 0)
            return;

        var center = boid.TeamNeighbours[0].Position;
        for (var i = 1; i < boid.TeamNeighbours.Length; ++i)
        {
            center += boid.TeamNeighbours[i].Position;
        }

        center *= 1.0f / boid.TeamNeighbours.Length;
        boid.Velocity += (center - boid.Position) * CoherenceRate * DeltaTime;
    }
}

[BurstCompile]
public partial struct AvoidOthersJob : IJobEntity
{
    [ReadOnly] public float AvoidanceRange;
    [ReadOnly] public float AvoidanceRate;
    [ReadOnly] public float DeltaTime;

    private void Execute(ref BoidAspect boid)
    {
        if (boid.AllNeighbours.Length <= 0)
            return;

        var myPosition = boid.Position;
        var minDistSqr = AvoidanceRange * AvoidanceRange;
        var step = float3.zero;
        for (var i = 0; i < boid.AllNeighbours.Length; ++i)
        {
            var delta = myPosition - boid.AllNeighbours[i].Position;
            var deltaSqr = math.lengthsq(delta);
            if (deltaSqr > 0 && deltaSqr < minDistSqr)
            {
                step += delta / math.sqrt(deltaSqr);
            }
        }

        boid.Velocity += step * (AvoidanceRate * DeltaTime);
    }
}

[BurstCompile]
public partial struct UpdateVelocityJob : IJobEntity
{
    public float DeltaTime;

    private void Execute(ref BoidAspect boid)
    {
        var velocity = boid.Velocity;
        velocity += math.normalize(velocity) * (boid.Team.Acceleration * DeltaTime);
        velocity *= 1.0f - 30.0f * boid.Team.Drag * DeltaTime;
        boid.Velocity = velocity;
    }
}

[BurstCompile]
public partial struct UpdatePositionJob : IJobEntity
{
    public float DeltaTime;
    private void Execute(ref BoidAspect boid)
    {
        boid.Position += boid.Velocity * DeltaTime;
        boid.Transform.Position = boid.Position;
        boid.Transform.LookAt(boid.Position + boid.Velocity);
    }
}