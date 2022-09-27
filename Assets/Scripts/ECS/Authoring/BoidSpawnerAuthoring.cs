using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct BoidAgentSettings
{
    public GameObject prefab;
    public float acceleration;
    public float drag;
}

public class BoidSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public BoidAgentSettings[] boidAgents;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<BoidSpawner>(entity);
        var agentBuffer = dstManager.AddBuffer<BoidAgentData>(entity);
        foreach (var boidAgent in boidAgents)
        {
            agentBuffer.Add(new BoidAgentData
            {
                BoidAgentEntity = conversionSystem.GetPrimaryEntity(boidAgent.prefab),
                Acceleration = boidAgent.acceleration, 
                Drag = boidAgent.drag
            });
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        foreach (var boidAgent in boidAgents)
        {
            referencedPrefabs.Add(boidAgent.prefab);
        }
    }
}