using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using Entitas;
using PicaVoxel;
using UnityEngine;

public class LoadLevel : IInitializeSystem, ISetPool
{
	private Pool _pool;

	public void Initialize()
	{
		_pool.SetCollisionGrid(new bool[8, 8]);
		_pool.SetTileGrid(new Entity[8, 8]);


		Entity roomy = _pool.CreateEntity().IsRoomy(true).IsInputReceiver(true).AddGridPosition(0, 0).AddCharge(8*8-1);
		var roomyObj = GameObject.FindGameObjectWithTag("Player");
		roomy.AddView(roomyObj.transform);


		for (int i = 0; i < 8; i++)
		{ 
			for (int j = 0; j < 8; j++)
			{
				Entity tile = _pool.CreateEntity().IsTile(true).IsDirty(true).AddGridPosition(i, j);

				_pool.collisionGrid.passible[i,j] = true;
				_pool.tileGrid.tiles[i, j] = tile;
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
	private Group _receivers;

	public void SetPool(Pool pool)
	{
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
			var x = mover.gridPosition.x;
			var y = mover.gridPosition.y;
			switch (mover.move.direction)
			{
				case Move.Direction.Up:
					y += 1;
					break;
				case Move.Direction.Left:
					x -= 1;
					break;
				case Move.Direction.Down:
					y -= 1;
					break;
				case Move.Direction.Right:
					x += 1;
					break;
			}
	
			mover.RemoveMove();

			if (mover.hasCharge)
			{
				if (mover.charge.value <= 0)
				{
					return;
				}

				mover.ReplaceCharge(mover.charge.value - 1);
			}

			//Clamp agains the grid and collision rules
			x = Math.Max(0, x);
			y = Math.Max(0, y);
			x = Math.Min(x, _pool.collisionGrid.passible.GetLength(0)-1);
			y = Math.Min(y, _pool.collisionGrid.passible.GetLength(1)-1);

			if (!mover.gridPosition.Equals(x, y))
			{
				if (_pool.collisionGrid.passible[x,y])
				{ 
					mover.ReplaceGridPosition(x, y);
				}
			}
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

public class AnimateDirty : IInitializeSystem, IReactiveSystem
{
	private PicaVoxel.Volume _floor;

	public TriggerOnEvent trigger
	{
		get { return Matcher.Dirty.OnEntityAddedOrRemoved(); }
	}

	private Color RandomColor(bool dirty)
	{
		if (!dirty) return new Color(UnityEngine.Random.Range(0.8f, 0.85f), UnityEngine.Random.Range(0.85f, 0.9f), UnityEngine.Random.Range(0.75f, 0.8f), 1.0f);

		return new Color(UnityEngine.Random.Range(0.3f, 0.35f), UnityEngine.Random.Range(0.2f, 0.3f), UnityEngine.Random.Range(0.0f, 0.1f), 1.0f);
	}

	public void Execute(List<Entity> entities)
	{
		foreach (var entity in entities)
		{
			var pos = entity.gridPosition.WorldPosition();

			for (int i = -5; i < 6; i++)
			{
				for (int j = -5; j < 6; j++)
				{
					_floor.SetVoxelStateAtWorldPosition(
						pos + Vector3.right * 0.049f * i + Vector3.forward * 0.049f * j,
						entity.isDirty ? VoxelState.Active : VoxelState.Hidden
					);
				}
			}
		}
	}

	public void Initialize()
	{
		_floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Volume>();
	}
}

public class VacuumTiles : IReactiveSystem, IEnsureComponents, ISetPool
{
	private Pool _pool;

	public TriggerOnEvent trigger
	{
		get { return Matcher.GridPosition.OnEntityAdded(); }
	}

	public IMatcher ensureComponents
	{
		get { return Matcher.Roomy; }
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}

	public void Execute(List<Entity> entities)
	{
		foreach (var entity in entities)
		{
			var tile = _pool.tileGrid.tiles[entity.gridPosition.x, entity.gridPosition.y];

			//TODO: If tile is still dirty, play a specific sound

			tile.isDirty = false;
		}
	}
}