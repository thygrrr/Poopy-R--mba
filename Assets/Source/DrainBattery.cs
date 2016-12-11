using System.Collections.Generic;
using Entitas;

public class DrainBattery : IReactiveSystem, IEnsureComponents
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
		}
	}
}