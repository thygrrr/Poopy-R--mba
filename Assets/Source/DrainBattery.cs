using System.Collections.Generic;
using Entitas;
using Entitas.Unity.VisualDebugging;
using UnityEngine;
using UnityEngine.UI;

public class DrainBattery : IReactiveSystem, IEnsureComponents, ISetPool
{
	private Pool _pool;
	

	public TriggerOnEvent trigger
	{
		get { return Matcher.Traveling.OnEntityRemoved(); }
	}

	public IMatcher ensureComponents
	{
		get { return Matcher.AllOf(Matcher.Charge); }
	}
	

	public void Execute(List<Entity> entities)
	{
		foreach (var mover in entities)
		{
			mover.ReplaceCharge(mover.charge.value - 1);
			GameObject.FindGameObjectWithTag("Charge").GetComponent<Text>().text = "Charge " + mover.charge.value;

			if (mover.charge.value == 0)
			{
				if (Mathf.Round(_pool.percentage.value) < 100)
				{
					_pool.isSuccess = false;
					_pool.isFailure = true;
				}
				else
				{
					_pool.isSuccess = true;
					_pool.isFailure = false;
				}
			}
		}
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}
}