using System;
using Entitas;
using UnityEngine;

public class Movement : IExecuteSystem, ISetPool
{
	private Group _movers;

	public void Execute()
	{
		foreach (Entity mover in _movers.GetEntities())
		{
			if (mover.hasCharge && mover.charge.value > 0)
			{
				switch (mover.move.direction)
				{
					case Move.Direction.Up:
						mover.physics.rigidbody.AddForce(10 * mover.view.transform.forward, ForceMode.Acceleration);
						break;
					case Move.Direction.Left:
						mover.physics.rigidbody.AddTorque(-15*mover.view.transform.up, ForceMode.Acceleration);
						break;
					case Move.Direction.Down:
						mover.physics.rigidbody.AddForce(-10*mover.view.transform.forward, ForceMode.Acceleration);
						break;
					case Move.Direction.Right:
						mover.physics.rigidbody.AddTorque(15*mover.view.transform.up, ForceMode.Acceleration);
						break;
				}

				//TODO: Move to battery system
				//mover.ReplaceCharge(mover.charge.value - 1);
			}

			mover.RemoveMove();
		}
	}

	public void SetPool(Pool pool)
	{
		_movers = pool.GetGroup(Matcher.AllOf(Matcher.Move, Matcher.Physics));
	}
}