using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial class SpawnBoids : SystemBase
{
    private static readonly uint[] BoidCounts = {64, 256, 1024, 4096};
    
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<BoidSpawner>();
        RequireSingletonForUpdate<BoidSimulator>();
    }

    protected override void OnUpdate()
    {
        // Reset setup when number keys are pressed
        for (var i = 0; i < BoidCounts.Length; ++i)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ResetSetup(BoidCounts[i]);
                break;
            }
        }
    }

    private void ResetSetup(uint boidCount)
    {
        var boidSimulator = GetSingleton<BoidSimulator>();
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var commandBufferParallelWriter = commandBuffer.AsParallelWriter();

        // Destroy all boid entities
        var jobHandle = new DestroyBoidsJob
        {
            EntityCommandBuffer = commandBufferParallelWriter
        }.ScheduleParallel();

        // Reset boid simulator
        boidSimulator.Reset();

        // Create entities for the boids
        jobHandle = Entities
            .ForEach((int entityInQueryIndex, in DynamicBuffer<BoidAgentData> boidAgentBuffer, in BoidSpawner spawner) =>
                {
                    for (var i = 0; i < boidCount; i++)
                    {
                        commandBufferParallelWriter.Instantiate(
                            entityInQueryIndex,
                            boidAgentBuffer[i % boidAgentBuffer.Length].BoidAgentEntity);
                    }
                }
            ).Schedule(jobHandle);

        jobHandle.Complete();
        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }
}

/// <summary>
/// Job to destroy all the boids
/// </summary>
[BurstCompile]
public partial struct DestroyBoidsJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

    private void Execute([EntityInQueryIndex] int index, in Entity entity, in Boid boid)
    {
        EntityCommandBuffer.DestroyEntity(index, entity);
    }
}