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

		for (int i = 0; i < 8; i++)
		{ 
			for (int j = 0; j < 8; j++)
			{
				Entity tile = _pool.CreateEntity().IsTile(true).AddGridPosition(i, j);

				_pool.collisionGrid.passible[i,j] = true;
				_pool.tileGrid.tiles[i, j] = tile;


				var obj = GameObject.Instantiate(Resources.Load("Tile") as GameObject);

				obj.Link(tile, _pool);
				obj.transform.position = tile.gridPosition.WorldPosition();
			}
		}
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}
}