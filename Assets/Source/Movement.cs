using System;
using Entitas;

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


			mover.ReplaceHeading(mover.move.direction);
			mover.RemoveMove();

			//Don't do anything else if out of battery
			if (mover.hasCharge && mover.charge.value <= 0) continue;

			//Hand over to travel system from here on
			mover.isOrienting = true;

			//Apply logical grid movement
			//Clamp agains the grid and collision rules
			x = Math.Max(0, x);
			y = Math.Max(0, y);
			x = Math.Min(x, _pool.collisionGrid.passible.GetLength(0) - 1);
			y = Math.Min(y, _pool.collisionGrid.passible.GetLength(1) - 1);

			//No movement supposed to happen (boundaries or obstacle)
			if (mover.gridPosition.Equals(x, y)) continue;
			if (!_pool.collisionGrid.passible[x, y]) continue;

			mover.ReplaceGridPosition(x, y);
			mover.isTraveling = true;
		}
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
		_movers = pool.GetGroup(Matcher.AllOf(Matcher.Move).NoneOf(Matcher.Traveling));
	}
}


