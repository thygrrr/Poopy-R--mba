using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class LinkViewsToEntities : IReactiveSystem, ISetPool
{
	private Pool _pool;
	public void Execute(List<Entity> entities)
	{
		foreach (var entity in entities)
		{
			entity.view.transform.gameObject.Link(entity, _pool);
		}
	}

	public TriggerOnEvent trigger
	{
		get
		{
			return Matcher.View.OnEntityAdded();
		}
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}
}

public class UpdateViewPositions : IExecuteSystem, ISetPool, IEnsureComponents
{
	private Pool _pool;
	private Group _movers;

	public void SetPool(Pool pool)
	{
		_pool = pool;
		_movers = pool.GetGroup(Matcher.AllOf(Matcher.View, Matcher.GridPosition));
	}

	public void Execute()
	{
		foreach (var mover in _movers.GetEntities())
		{
			var world = mover.gridPosition.WorldPosition();

			mover.view.transform.position = Vector3.MoveTowards(mover.view.transform.position, world, Time.deltaTime);
		}
	}

	public TriggerOnEvent trigger
	{
		get { return Matcher.View.OnEntityAdded(); }
	}

	public IMatcher ensureComponents
	{
		get { return Matcher.AllOf(Matcher.View, Matcher.GridPosition); }
	}
}

public class InitViewPositions : IReactiveSystem, IEnsureComponents
{
	public void Execute(List<Entity> entities)
	{
		foreach (var entity in entities)
		{
			entity.view.transform.position = entity.gridPosition.WorldPosition();
		}
	}

	public TriggerOnEvent trigger
	{
		get { return Matcher.View.OnEntityAdded(); }
	}

	public IMatcher ensureComponents
	{
		get { return Matcher.AllOf(Matcher.View, Matcher.GridPosition); }
	}
}