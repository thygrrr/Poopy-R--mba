using System.Security.Cryptography.X509Certificates;
using Entitas;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class LoadLevel : IInitializeSystem, ISetPool
{
	private Pool _pool;

	//Roughly ordered by size (magic knowledge.
	private static readonly string[] assets = { "Table4", "Table3s", "Table3n", "Table3e", "Table3w", "Table2v", "Table2h", "Table1n", "Table1s", "Table", "Sofa2e", "Sofa2w", "Sofa2s", "Sofa2n", "Chairn", "Chairs", "Chaire", "Chairw" };

	private static GameObject[] obstacles;
	private static Footprint[] footprints;

	public void Initialize()
	{
		_pool.SetCollisionGrid(new bool[8, 8], new bool[8, 8]);
		_pool.SetTileGrid(new Entity[8, 8]);
		_pool.SetScore(0);


		Entity roomy = _pool.CreateEntity().IsRoomy(true).IsInputReceiver(true).AddGridPosition(0, 0).AddCharge(8 * 8 - 1).AddHeading(Move.Direction.Up);
		var roomyObj = GameObject.FindGameObjectWithTag("Player");
		roomy.AddView(roomyObj.transform);

//		var pooObj = GameObject.FindGameObjectWithTag("Poo");
//		Entity pooTile = _pool.tileGrid.tiles[(int)(pooObj.transform.position.x * 2.0f), (int)(pooObj.transform.position.z * 2.0)];
//		pooTile.AddView(pooObj.transform);
//		pooTile.isPoop = true;

		GenerateLevel();
		LoadObstacles();


		//DEBUG blocks
		/*
		for (int x = 0; x < _pool.collisionGrid.passible.GetLength(0); x++)
		{
			for (int y = 0; y < _pool.collisionGrid.passible.GetLength(1); y++)
			{
				var prefab = Resources.Load("Blocked") as GameObject;
				var obj = GameObject.Instantiate(prefab);
				obj.transform.position = new GridPosition() { x = x, y = y }.WorldPosition();

				if (!_pool.collisionGrid.passible[x, y])
				{
					obj.GetComponent<MeshRenderer>().material.color = Color.blue;
				}
				if (_pool.collisionGrid.occupied[x, y])
				{
					obj.GetComponent<MeshRenderer>().material.color = Color.red;
				}
				if (_pool.collisionGrid.occupied[x, y] && _pool.collisionGrid.passible[x, y])
				{
					obj.GetComponent<MeshRenderer>().material.color = Color.magenta;
				}
			}
		}
		*/

	}


	private void GenerateLevel()
	{
		//Generate Level
		for (int x = 0; x < _pool.collisionGrid.passible.GetLength(0); x++)
		{
			for (int y = 0; y < _pool.collisionGrid.passible.GetLength(1); y++)
			{
				_pool.collisionGrid.passible[x, y] = (Random.Range(0, 4) > 2);
				_pool.collisionGrid.occupied[x, y] = false;
				_pool.tileGrid.tiles[x, y] = _pool.CreateEntity().IsTile(true).AddGridPosition(x, y);
			}
		}

		//Reserve a spot for our roomy. TODO: Randomize as well
		_pool.collisionGrid.passible[0,0] = false;
	}


	private bool Fits(int x, int y, bool[,] mask)
	{
		for (int i = 0; i < mask.GetLength(0); i++)
		{
			for (int j = 0; j < mask.GetLength(1); j++)
			{
				//Out of bounds
				if (x + i >= _pool.collisionGrid.passible.GetLength(0)) return false;
				if (y + j >= _pool.collisionGrid.passible.GetLength(1)) return false;

				var passible = _pool.collisionGrid.passible[x + i, y + j];
				var occupied = _pool.collisionGrid.occupied[x + i, y + j];

				//Check if mask fits.
				if (mask[i, j] && (passible || occupied)) return false;

				//This means the mask has a hole but the map requires it
				//to be solid. This prevents overlapping furniture.
				if (!mask[i, j] && !passible) return false;
			}
		}
		return true;
	}


	private void Occupy(int x, int y, bool[,] mask)
	{
		//CAVEAT: This works because we just checked it Fits(). No warranties. :)
		for (int i = 0; i < mask.GetLength(0); i++)
		{
			for (int j = 0; j < mask.GetLength(1); j++)
			{
				_pool.collisionGrid.occupied[x + i, y + j] = mask[i,j];
			}
		}
	}

	private void Place(int x, int y, GameObject prefab)
	{
		var obj = GameObject.Instantiate(prefab);
		_pool.tileGrid.tiles[x, y].AddView(obj.transform);
	}


	private void LoadObstacles()
	{
		obstacles = new GameObject[assets.Length];
		footprints = new Footprint[assets.Length];

		for (int i = 0; i < assets.Length; i++)
		{
			obstacles[i] = Resources.Load(assets[i]) as GameObject;
			footprints[i] = obstacles[i].GetComponent<Footprint>();
		}

		for (int x = 0; x < _pool.collisionGrid.passible.GetLength(0); x++)
		{
			for (int y = 0; y < _pool.collisionGrid.passible.GetLength(1); y++)
			{
				for (int i = 0; i < assets.Length; i++)
				{
					if (Fits(x, y, footprints[i].mask))
					{
						Occupy(x, y, footprints[i].mask);
						Place(x, y, obstacles[i]);
						break;
					}
				}
				
			}
		}
	}

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}
}