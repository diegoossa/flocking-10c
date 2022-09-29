// using Unity.Burst;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using Unity.Entities;
// using Unity.Mathematics;
// using static Unity.Entities.SystemAPI;
//
// [BurstCompile]
// [UpdateAfter(typeof(FindNeighbours))]
// public partial struct MatchVelocity : ISystem
// {
//     private ComponentLookup<Boid> _boidLookup;
//
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<BoidSimulator>();
//         _boidLookup = state.GetComponentLookup<Boid>(true);
//     }
//
//     public void OnDestroy(ref SystemState state)
//     {
//     }
//
//     public void OnUpdate(ref SystemState state)
//     {
//         _boidLookup.Update(ref state);
//
//         var boidSimulation = GetSingleton<BoidSimulator>();
//         var deltaTime = Time.DeltaTime;
//
//         var jobHandle = new MatchVelocityJob
//         {
//             MatchVelocityRate = boidSimulation.MatchVelocityRate,
//             DeltaTime = deltaTime,
//             BoidLookup = _boidLookup
//         }.ScheduleParallel(state.Dependency);
//
//         jobHandle.Complete();
//     }
// }
//
// [BurstCompile]
// public partial struct MatchVelocityJob : IJobEntity
// {
//     public float MatchVelocityRate;
//     public float DeltaTime;
//
//     [NativeDisableContainerSafetyRestriction]
//     [ReadOnly]
//     public ComponentLookup<Boid> BoidLookup;
//
//     private void Execute(ref Boid boid, in DynamicBuffer<Neighbours> neighbours)
//     {
//         if (neighbours.Length > 0)
//         {
//             var velocity = float3.zero;
//             for (var i = 0; i < neighbours.Length; ++i)
//             {
//                 var neighbourBoid = BoidLookup[neighbours[i].Entity];
//                 if (neighbourBoid.TeamId == boid.TeamId)
//                 {
//                     velocity += neighbourBoid.Velocity;
//                 }
//             }
//
//             velocity /= neighbours.Length;
//             boid.Velocity += (velocity - boid.Velocity) * MatchVelocityRate * DeltaTime;
//         }
//     }
// }