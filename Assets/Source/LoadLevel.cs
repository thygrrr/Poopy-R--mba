using Entitas;
using UnityEngine;

public class LoadLevel : IInitializeSystem, ISetPool
{
	private Pool _pool;

	public void Initialize()
	{
		_pool.SetCollisionGrid(new bool[8, 8]);
		_pool.SetTileGrid(new Entity[8, 8]);
		_pool.SetScore(0);


		Entity roomy = _pool.CreateEntity().IsRoomy(true).IsInputReceiver(true).AddGridPosition(0, 0).AddCharge(8 * 8 - 1).AddHeading(Move.Direction.Up);
		var roomyObj = GameObject.FindGameObjectWithTag("Player");
		roomy.AddView(roomyObj.transform);


		for (int i = 0; i < 8; i++)
		{ 
			for (int j = 0; j < 8; j++)
			{
				Entity tile = _pool.CreateEntity().IsTile(true).AddGridPosition(i, j);

				_pool.collisionGrid.passible[i,j] = true;
				_pool.tileGrid.tiles[i, j] = tile;
			}
		}

		var pooObj = GameObject.FindGameObjectWithTag("Poo");
		Entity pooTile = _pool.tileGrid.tiles[(int)(pooObj.transform.position.x * 2.0f), (int)(pooObj.transform.position.z * 2.0)];
		pooTile.AddView(pooObj.transform);
		pooTile.isPoop = true;

	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}
}