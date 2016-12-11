﻿using Entitas;
using PicaVoxel;
using UnityEngine;

public class AnimateDirty : IInitializeSystem, IExecuteSystem, ISetPool
{
	private Pool _pool;
	private Group _group;

	public void SetPool(Pool pool)
	{
		_group = pool.GetGroup(Matcher.AllOf(Matcher.Dirty, Matcher.View));
	}

	private PicaVoxel.Volume _floor;

	private Color RandomColor(bool dirty)
	{
		if (!dirty) return new Color(UnityEngine.Random.Range(0.8f, 0.85f), UnityEngine.Random.Range(0.85f, 0.9f), UnityEngine.Random.Range(0.75f, 0.8f), 1.0f);

		return new Color(UnityEngine.Random.Range(0.3f, 0.35f), UnityEngine.Random.Range(0.2f, 0.3f), UnityEngine.Random.Range(0.0f, 0.1f), 1.0f);
	}

	public void Execute()
	{
		//Show dirty voxels from the filth layer in circle around dirty object's position
		foreach (var entity in _group.GetEntities())
		{
			var pos = entity.view.transform.position;
			pos.x += UnityEngine.Random.Range(-0.2f, 0.2f);
			pos.z += UnityEngine.Random.Range(-0.2f, 0.2f);

			_floor.SetVoxelStateAtWorldPosition(pos, VoxelState.Active);

			pos = entity.view.transform.position;
			pos.x += UnityEngine.Random.Range(-0.08f, 0.08f);
			pos.z += UnityEngine.Random.Range(-0.08f, 0.08f);

			_floor.SetVoxelStateAtWorldPosition(pos, VoxelState.Active);
		}
	}

	public void Initialize()
	{
		_floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Volume>();
	}
}