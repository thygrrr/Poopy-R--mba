using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entitas;

public class FloorPlan : MonoBehaviour
{
	private Systems systems;
	private Systems physics;
	private Pool pool;


	void Awake()
	{
		pool = Pools.CreatePool();
		systems = new Systems();

		systems.Add(pool.CreateSystem(new LoadLevel()));
		systems.Add(pool.CreateSystem(new LinkViewsToEntities()));

		systems.Add(pool.CreateSystem(new Pickups()));

		systems.Add(pool.CreateSystem(new InitViewPositions()));
		systems.Add(pool.CreateSystem(new SpreadPoo()));

		physics = new Systems();
		physics.Add(pool.CreateSystem(new PlayerInput()));
		physics.Add(pool.CreateSystem(new Movement()));
	}


	void Start()
	{
		systems.Initialize();
		physics.Initialize();
	}


	void Update()
	{	
		systems.Execute();
	}


	void FixedUpdate()
	{
		physics.Execute();
	}


	void OnDestroy()
	{
		systems.TearDown();
		physics.TearDown();
	}
}
