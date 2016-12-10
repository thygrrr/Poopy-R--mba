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
	}


	void Start()
	{
		systems.Initialize();
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
