using System;
using System.Collections.Generic;
using System.IO;
using Entitas;
using Entitas.Unity.VisualDebugging;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

public class LoadLevel : IInitializeSystem, ISetPool
{
	private Pool _pool;

	//Roughly ordered by size (magic knowledge.
	private static readonly string[] assets =
	{
		"Table4", "Table3s", "Table3n", "Table3e", "Table3w", "Table2v", "Table2h",
		"Table1n", "Table1s", "Table", "Sofa2e", "Sofa2w", "Sofa2s", "Sofa2n", "Chairn", "Chairs", "Chaire", "Chairw"
	};


	private static GameObject[] obstacles;
	private static Footprint[] footprints;

	public void Initialize()
	{
		_pool.SetCollisionGrid(new bool[8, 8], new bool[8, 8]);
		_pool.SetTileGrid(new Entity[8, 8]);
		_pool.SetScore(0);


		var difficulty = 10;

		GenerateLevel(difficulty);

		LoadObstacles();

		Entity roomy = _pool.GetGroup(Matcher.Roomy).GetSingleEntity();
		roomy.AddCharge(difficulty + DistanceToPoo(roomy.gridPosition.x, roomy.gridPosition.y));

		//TODO: Multiply with a difficulty modifier, perfect gameplay can be hard.



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


	private void GenerateLevel(int difficulty)
	{
		//Generate Level
		for (int x = 0; x < _pool.collisionGrid.passible.GetLength(0); x++)
		{
			for (int y = 0; y < _pool.collisionGrid.passible.GetLength(1); y++)
			{
				_pool.collisionGrid.passible[x, y] = false;
				_pool.collisionGrid.occupied[x, y] = false;
				_pool.tileGrid.tiles[x, y] = _pool.CreateEntity().IsTile(true).AddGridPosition(x, y);
			}
		}

		CarvePath(difficulty);
	}

	private void CarvePath(int length)
	{
		var x_start = Random.Range(0, _pool.collisionGrid.passible.GetLength(0)-1);
		var y_start = Random.Range(0, _pool.collisionGrid.passible.GetLength(1)-1);

		Entity roomy =
	_pool.CreateEntity()
		.IsRoomy(true)
		.IsInputReceiver(true)
		.AddGridPosition(x_start, y_start)
		.AddHeading(Move.Direction.Up);

		var roomyObj = Resources.Load("Roomy") as GameObject;
		roomy.AddView(GameObject.Instantiate(roomyObj).transform);


		BackTrackCarve(length, x_start, y_start);
	}


	private bool BackTrackCarve(int length, int x, int y)
	{
		if (x >= 0 && y >= 0 && x < _pool.collisionGrid.passible.GetLength(0) && y < _pool.collisionGrid.passible.GetLength(1))
		{
			if (_pool.collisionGrid.passible[x, y]) return false;

			_pool.collisionGrid.passible[x, y] = true;

			if (length == 0)
			{
				//Place Poo at end of path.
				var tile = _pool.tileGrid.tiles[x, y];
				tile.isPoop = true;
				var pooObj = Resources.Load("Poo") as GameObject;
				tile.AddView(GameObject.Instantiate(pooObj).transform);

				return true;
			}

			foreach (Move.Direction d in Move.Shuffled())
			{
				var xn = x;
				var yn = y;

				Move.Apply(d, ref xn, ref yn);

				if (BackTrackCarve(length - 1, xn, yn)) return true;
			}

			_pool.collisionGrid.passible[x, y] = false;
		}
		return false;
	}


	//A rough implementation of dijkstra's algorithm
	private int DistanceToPoo(int x_start, int y_start)
	{
		List<Entity> Q = new List<Entity>();

		for (int x = 0; x < _pool.collisionGrid.passible.GetLength(0); x++)
		{
			for (int y = 0; y < _pool.collisionGrid.passible.GetLength(1); y++)
			{
				if (_pool.collisionGrid.passible[x, y])
				{
					var tile = _pool.tileGrid.tiles[x, y];
					tile.AddPooDistance(int.MaxValue);
					Q.Add(tile);
				}
			}
		}

		//Source
		_pool.tileGrid.tiles[x_start, y_start].ReplacePooDistance(0);


		while (Q.Count > 0)
		{
			Entity u = null;
			var min_dist = int.MaxValue;

			foreach (var v in Q)
			{
				if (v.pooDistance.distance < min_dist)
				{
					u = v;
					min_dist = v.pooDistance.distance;
				} 			
			}
			Q.Remove(u);

			//Process neighbors of u
			var x = u.gridPosition.x;
			var y = u.gridPosition.y;

			foreach (Move.Direction d in Move.Shuffled())
			{
				var xn = x;
				var yn = y;

				Move.Apply(d, ref xn, ref yn);

				if (xn >= 0 && yn >= 0 && xn < _pool.collisionGrid.passible.GetLength(0) &&
				    yn < _pool.collisionGrid.passible.GetLength(1))
				{
					var v = _pool.tileGrid.tiles[xn, yn];
					
					//Array can contain impassible tiles
					if (!v.hasPooDistance) continue; 

					var alt = u.pooDistance.distance + 1;
					if (alt < v.pooDistance.distance)
					{
						v.ReplacePooDistance(alt);
					}
				}
			}
		}

		//At the end of this, the piece of poop has it's minimum distance to x_start, y_start
		return _pool.GetGroup(Matcher.Poop).GetSingleEntity().pooDistance.distance;
	}


	bool Fits(int x, int y, bool[,] mask)
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


	private
		void Occupy
		(int x, int y, bool[,] mask)
	{
		//CAVEAT: This works because we just checked it Fits(). No warranties. :)
		for (int i = 0; i < mask.GetLength(0); i++)
		{
			for (int j = 0; j < mask.GetLength(1); j++)
			{
				_pool.collisionGrid.occupied[x + i, y + j] = mask[i, j];
			}
		}
	}

	private
		void Place
		(int x, int y, GameObject prefab)
	{
		var obj = GameObject.Instantiate(prefab);
		_pool.tileGrid.tiles[x, y].AddView(obj.transform);
	}


	private
		void LoadObstacles
		()
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

	public void SetPool (Pool pool)
	{
		_pool = pool;
	}
}