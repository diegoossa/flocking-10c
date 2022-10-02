using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

/// <summary>
/// System to Spawn Boids
/// </summary>
[BurstCompile]
public partial struct SpawnBoids : ISystem
{
    private static readonly uint[] BoidCounts = {64, 256, 1024, 4096, 8192, 16384, 32768, 65536};
    private bool _initialSpawn;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidSpawner>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        // Reset setup when number keys are pressed
        for (var i = 0; i < BoidCounts.Length; ++i)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                BoidCounter.Instance.SetCounter(BoidCounts[i]);
                ResetSetup(ref state, BoidCounts[i]);
                break;
            }
        }

        // Perform an initial spawn
        if (!_initialSpawn)
        {
            BoidCounter.Instance.SetCounter(BoidCounts[0]);
            ResetSetup(ref state, BoidCounts[0]);
            _initialSpawn = true;
        }
    }

    /// <summary>
    /// Destroy all boids and spawn new boids
    /// </summary>
    /// <param name="state">Ref of the ISystem State</param>
    /// <param name="boidCount">Number of boids to instantiate</param>
    [BurstCompile]
    private void ResetSetup(ref SystemState state, uint boidCount)
    {
        var worldSettings = GetSingleton<WorldSettings>();
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var commandBufferParallelWriter = commandBuffer.AsParallelWriter();

        // Destroy all boid entities
        var destroyBoidsJob = new DestroyBoidsJob
        {
            EntityCommandBuffer = commandBufferParallelWriter
        };
        var jobHandle = destroyBoidsJob.ScheduleParallel(state.Dependency);

        // Decide world size based on boid count and density
        var worldSize =
            (int) math.ceil(math.pow(boidCount, 1f / 3) * worldSettings.BoidDensity /
                            worldSettings.RoundWorldSizeToMultiplesOf) * worldSettings.RoundWorldSizeToMultiplesOf;
        worldSettings.HalfWorldSize = new float3(worldSize / 2f);

        // Create entities for the boids
        var createBoidsJob = new CreateBoidsJob
        {
            EntityCommandBuffer = commandBufferParallelWriter,
            BoidCount = boidCount,
            WorldSize = worldSize,
            InitialVelocity = worldSettings.InitialVelocity
        };
        jobHandle = createBoidsJob.ScheduleParallel(jobHandle);

        jobHandle.Complete();
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();

        SetSingleton(worldSettings);
    }
}

/// <summary>
/// Job to destroy all the boids
/// </summary>
[BurstCompile]
public partial struct DestroyBoidsJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

    private void Execute([EntityInQueryIndex] int index, in BoidAspect boid)
    {
        EntityCommandBuffer.DestroyEntity(index, boid.Self);
    }
}

/// <summary>
/// Job to create new boids
/// </summary>
[BurstCompile]
public partial struct CreateBoidsJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
    [ReadOnly] public uint BoidCount;
    [ReadOnly] public float WorldSize;
    [ReadOnly] public float InitialVelocity;

    private void Execute([EntityInQueryIndex] int index, in BoidSpawnerAspect boidSpawner)
    {
        var random = new Random((uint) index + 1);

        // Initial State for the Boids
        var halfSpawnRange = new float3(WorldSize * 0.5f - 3f);

        for (var i = 0; i < BoidCount; i++)
        {
            // Spawn random boids
            var randomIndex = random.NextInt(0, boidSpawner.BoidAgentBuffer.Length);
            var boid = EntityCommandBuffer.Instantiate(index, boidSpawner.BoidAgentBuffer[randomIndex].BoidAgentEntity);
            var position = new float3(
                random.NextFloat(-halfSpawnRange.x, halfSpawnRange.x),
                random.NextFloat(-halfSpawnRange.y, halfSpawnRange.y),
                random.NextFloat(-halfSpawnRange.z, halfSpawnRange.z));
            EntityCommandBuffer.SetComponent(index, boid, new Boid
            {
                Position = position,
                Velocity = RandomUnitFloat3(ref random) * InitialVelocity,
                TeamId = boidSpawner.BoidAgentBuffer[randomIndex].TeamId
            });
            EntityCommandBuffer.AddComponent(index, boid, new Team
            {
                Acceleration = boidSpawner.BoidAgentBuffer[randomIndex].Acceleration,
                Drag = boidSpawner.BoidAgentBuffer[randomIndex].Drag
            });
            EntityCommandBuffer.SetComponent(index, boid,
                new LocalToWorldTransform {Value = UniformScaleTransform.FromPosition(position)});
        }
    }

    /// <summary>
    /// Random Unit float3
    /// </summary>
    /// <param name="random">Random ref</param>
    /// <returns>Random unit float3</returns>
    private static float3 RandomUnitFloat3(ref Random random)
    {
        var a = random.NextFloat(0, 2f * math.PI);
        var z = random.NextInt(-1, 1);
        var h = math.sqrt(1f - z * z);
        return new float3(h * math.cos(a), h * math.sin(a), z);
    }
}