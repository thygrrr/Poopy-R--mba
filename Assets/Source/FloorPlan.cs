using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entitas;

public class FloorPlan : MonoBehaviour
{
	private Systems systems;
	private Pool pool;


	void Awake()
	{
		pool = Pools.CreatePool();
		systems = new Systems();

		systems.Add(pool.CreateSystem(new LoadLevel()));
		systems.Add(pool.CreateSystem(new LinkViewsToEntities()));

		systems.Add(pool.CreateSystem(new InitViewPositions()));
		systems.Add(pool.CreateSystem(new UpdateViewOrientations()));
		systems.Add(pool.CreateSystem(new UpdateViewPositions()));

		systems.Add(pool.CreateSystem(new Pickups()));
		systems.Add(pool.CreateSystem(new SpreadPoo()));
		systems.Add(pool.CreateSystem(new DrainBattery()));

		systems.Add(pool.CreateSystem(new PlayerInput()));
		systems.Add(pool.CreateSystem(new Movement()));

		systems.Add(pool.CreateSystem(new LevelEnd()));
	}


	void Start()
	{
		systems.Initialize();

		pool.isNextLevel = true;
	}


	void Update()
	{	
		systems.Execute();
	}


	void OnDestroy()
	{
		systems.TearDown();
	}
}
