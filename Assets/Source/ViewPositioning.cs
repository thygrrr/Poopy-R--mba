using System.Collections.Generic;
using Entitas;
using UnityEngine;


public class UpdateViewPositions : IExecuteSystem, ISetPool
{
	private Group _movers;

	public void SetPool(Pool pool)
	{
		_movers = pool.GetGroup(Matcher.AllOf(Matcher.View, Matcher.GridPosition, Matcher.Traveling).NoneOf(Matcher.Orienting));
	}

	public void Execute()
	{
		foreach (var mover in _movers.GetEntities())
		{
			var goal = mover.gridPosition.WorldPosition();
			mover.view.transform.position = Vector3.MoveTowards(mover.view.transform.position, goal, Time.deltaTime*1.5f);

			if (goal == mover.view.transform.position) mover.isTraveling = false;
		}
	}
}



public class UpdateViewOrientations : IExecuteSystem, ISetPool
{
	private Group _movers;

	public void SetPool(Pool pool)
	{
		_movers = pool.GetGroup(Matcher.AllOf(Matcher.View, Matcher.Heading));
	}

	public void Execute()
	{
		foreach (var mover in _movers.GetEntities())
		{
			var goal = Quaternion.AngleAxis(Heading.angles[mover.heading.direction], Vector3.up);

			mover.view.transform.rotation = Quaternion.RotateTowards(mover.view.transform.rotation, goal, Time.deltaTime*540.0f);

			if (goal == mover.view.transform.rotation) mover.isOrienting = false;
		}
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



public class LinkViewsToEntities : IReactiveSystem, ISetPool
{
	private Pool _pool;
	public void Execute(List<Entity> entities)
	{
		foreach (var entity in entities)
		{
			//Bind Gameobject
			entity.view.transform.gameObject.Link(entity, _pool);

			//Bind Rigidbody
			var rigidbody = entity.view.transform.GetComponent<Rigidbody>();
			if (rigidbody != null)
			{
				entity.AddPhysics(rigidbody);
			}
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