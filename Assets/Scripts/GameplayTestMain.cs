using System.Collections.Generic;
using UnityEngine;

public class GameplayTestMain : MonoBehaviour 
{
	private const float BOID_DENSITY = 4f;
	private const int ROUND_WORLD_SIZE_TO_MULTIPLES_OF = 5;

	private static uint[] s_boidCounts = new uint[]
	{
		64,
		256,
		1024,
		4096,
	};

	private static readonly BoidSimulator.Team[] s_teams = new BoidSimulator.Team[]
	{
		// Red
		new BoidSimulator.Team
		{
			Acceleration = 4,
			Drag = .02f,
		},

		// Green
		new BoidSimulator.Team
		{
			Acceleration = 4,
			Drag = .03f,
		},

		// Blue
		new BoidSimulator.Team
		{
			Acceleration = 11,
			Drag = .04f,
		},
	};

	[SerializeField]
	private GameObject[] m_agentPrefabs;

	private BoidSimulator m_boidSimulator = new BoidSimulator();
	private List<GameObject> m_boidGameObjects = new List<GameObject>();

	private void Start() 
	{
		ResetSetup(s_boidCounts[0]);
		this.enabled = true;
	}

	private void Update() 
	{
		// Reset setup when number keys are pressed
		for (int i = 0; i < s_boidCounts.Length; ++i) 
		{
			if (Input.GetKeyDown(KeyCode.Alpha1 + i)) 
			{
				ResetSetup(s_boidCounts[i]);
				break;
			}
		}

		m_boidSimulator.StepSimulation(Time.deltaTime);

		UpdateGameObjects();
	}

	private void ResetSetup(uint boidCount)
	{
		// Destroy all game objects
		foreach (var gameObject in m_boidGameObjects)
		{
			GameObject.Destroy(gameObject);
		}
		m_boidGameObjects.Clear();

		// Decide world size based on boid count and density
		int worldSize = Mathf.CeilToInt(Mathf.Pow(boidCount, 1f / 3) * BOID_DENSITY / ROUND_WORLD_SIZE_TO_MULTIPLES_OF) * ROUND_WORLD_SIZE_TO_MULTIPLES_OF;

		// Reset boid simulator
		m_boidSimulator.Reset(new Vector3(worldSize, worldSize, worldSize), boidCount, s_teams);

		// Create game objects for the boids
		for (int boidIndex = 0; boidIndex < m_boidSimulator.Boids.Length; ++boidIndex)
		{
			var boid = m_boidSimulator.Boids[boidIndex];
			var prefab = m_agentPrefabs[boid.Team];
			var gameObject = GameObject.Instantiate(prefab, boid.Position, Quaternion.identity);
			m_boidGameObjects.Add(gameObject);
		}
	}

	private void UpdateGameObjects()
	{
		//Debug.Log(m_boidSimulator.Boids.Length);
		for (int boidIndex = 0; boidIndex < m_boidSimulator.Boids.Length; ++boidIndex)
		{
			var boid = m_boidSimulator.Boids[boidIndex];
			var gameObject = m_boidGameObjects[boidIndex];
			gameObject.transform.position = boid.Position;
			gameObject.transform.LookAt(boid.Position + boid.Velocity);
		}
	}
}
