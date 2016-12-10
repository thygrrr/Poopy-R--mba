using System.Collections.Generic;
using System.ComponentModel;
using Entitas;
using UnityEngine;

public class LoadLevel : IInitializeSystem, ISetPool
{
	private Pool _pool;

	public void Initialize()
	{
		_pool.SetCollisionGrid(new bool[8, 8]);
		_pool.SetTileGrid(new Entity[8, 8]);


		Entity roomy = _pool.CreateEntity().IsRoomy(true).IsInputReceiver(true).AddGridPosition(0, 0);
		var roomyObj = GameObject.Instantiate(Resources.Load("Roomy") as GameObject);
		roomy.AddView(roomyObj.transform);


		for (int i = 0; i < 8; i++)
		{ 
			for (int j = 0; j < 8; j++)
			{
				Entity tile = _pool.CreateEntity().IsTile(true).AddGridPosition(i, j);

				_pool.collisionGrid.passible[i,j] = true;
				_pool.tileGrid.tiles[i, j] = tile;


				var tileObj = GameObject.Instantiate(Resources.Load("Tile") as GameObject);
				tile.AddView(tileObj.transform);
			}
		}
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}
}


public class PlayerInput : IExecuteSystem, ISetPool
{
	private Pool _pool;
	private Group _receivers;

	public void SetPool(Pool pool)
	{
		_pool = pool;
		_receivers = pool.GetGroup(Matcher.AllOf(Matcher.InputReceiver).NoneOf(Matcher.Move));
	}

	public void Execute()
	{
		foreach (var receiver in _receivers.GetEntities())
		{
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			{
				receiver.AddMove(Move.Direction.Up);
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			{
				receiver.AddMove(Move.Direction.Left);
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			{
				receiver.AddMove(Move.Direction.Down);
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			{
				receiver.AddMove(Move.Direction.Right);
			}
		}
	}
}

public class Movement : IExecuteSystem, ISetPool
{
	private Pool _pool;
	private Group _movers;

	public void Execute()
	{
		foreach (Entity mover in _movers.GetEntities())
		{
			switch (mover.move.direction)
			{
				case Move.Direction.Up:
					mover.ReplaceGridPosition(mover.gridPosition.x, mover.gridPosition.y+1);
					break;
				case Move.Direction.Left:
					mover.ReplaceGridPosition(mover.gridPosition.x-1, mover.gridPosition.y);
					break;
				case Move.Direction.Down:
					mover.ReplaceGridPosition(mover.gridPosition.x, mover.gridPosition.y-1);
					break;
				case Move.Direction.Right:
					mover.ReplaceGridPosition(mover.gridPosition.x+1, mover.gridPosition.y);
					break;
			}

			mover.RemoveMove();
		}
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
		_movers = pool.GetGroup(Matcher.Move);
	}
}

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

public class UpdateViewPositions : IReactiveSystem, IEnsureComponents
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
		get { return Matcher.GridPosition.OnEntityAdded(); }
	}

	public IMatcher ensureComponents
	{
		get { return Matcher.View; }
	}
}
