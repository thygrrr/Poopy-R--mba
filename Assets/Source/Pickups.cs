using System.Collections.Generic;
using Entitas;

public class Pickups : IReactiveSystem, IEnsureComponents, ISetPool
{
	private Pool _pool;

	public TriggerOnEvent trigger
	{
		get { return Matcher.Traveling.OnEntityRemoved(); }
	}

	public IMatcher ensureComponents
	{
		get { return Matcher.AllOf(Matcher.Roomy).NoneOf(Matcher.Traveling); }
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

			// Start spreading the filth afterwards :D
			if (tile.isPoop)
			{
				tile.isPoop = false;
				entity.isDirty = true;
			}
		}
	}
}