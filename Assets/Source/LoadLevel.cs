using System;
using System.Collections.Generic;
using System.IO;
using Entitas;
using Entitas.Unity.VisualDebugging;
using PicaVoxel;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class LoadLevel : IReactiveSystem, ISetPool, IComparer<Entity>
{
	private Pool _pool;

	private int level = int.MinValue;

	//Roughly ordered by size (magic knowledge.
	private static string[] huge =
	{
		"Table4", "Table3s", "Table3n", "Table3e", "Table3w", "Table2v", "Table2h",
		"Table1n", "Table1s", "Table"
	};

	private static string[] large =
	{
		"Boxes", "CoffeeTable",
	};

	private static string[] medium =
	{
		 "Desk",  "Wall2v",  "Sofa2e", "Sofa2w", "Boxes2", "Boxes3", "Sofa2s", "Sofa2n", "Boxes2", "Boxes3", 
	};

	private static string[] small =
	{
		 "Chairn", "Chairs", "Chaire"
	};

	private static string[] lights =
	{
		 "Lamp", "Lamp", "Television"
	};

	private static readonly int[] difficulty =
	{
		5, 6, 8, 10, 12, 14, 16, 20, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 38, 40
	};


	public void DestroyLoadedObjects()
	{		
		//Clear floor
		var _floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Volume>();

		for (int x = 0; x < _floor.Frames[0].XSize; x++)
			for (int y = 0; y < _floor.Frames[0].YSize; y++)
				for (int z = 0; z < _floor.Frames[0].ZSize; z++)
				_floor.SetVoxelStateAtArrayPosition(new PicaVoxelPoint(x, y, z), VoxelState.Hidden);

		_floor.UpdateAllChunks();

		foreach (var entity in _pool.GetGroup(Matcher.View).GetEntities())
		{
			Object.Destroy(entity.view.transform.gameObject);
			_pool.DestroyEntity(entity);
		}

		foreach (var entity in _pool.GetGroup(Matcher.Tile).GetEntities())
		{
			_pool.DestroyEntity(entity);
		}
	}

	public void Initialize()
	{
		DestroyLoadedObjects();

		_pool.ReplaceCollisionGrid(new bool[8, 8], new bool[8, 8]);
		_pool.ReplaceTileGrid(new Entity[8, 8]);

		if (!_pool.hasScore) _pool.SetScore(0);

		_pool.ReplacePercentage(0);

		GameObject.FindGameObjectWithTag("Percentage").GetComponent<Text>().text = string.Format("Spread {0}%", (int)Mathf.Round(_pool.percentage.value));

		var length = difficulty[Mathf.Min(difficulty.Length - 1, level)];
		GameObject.FindGameObjectWithTag("Level").GetComponent<Text>().text = "Level " + (level+1);

		GenerateLevel(length);

		Entity roomy = _pool.GetGroup(Matcher.Roomy).GetSingleEntity();
		roomy.AddCharge(length + DistanceToPoo(roomy.gridPosition.x, roomy.gridPosition.y));
		GameObject.FindGameObjectWithTag("Charge").GetComponent<Text>().text = "Charge " + roomy.charge.value;

		LoadObstacles(lights, false);
		LoadObstacles(huge, false);
		LoadObstacles(large, false);
		LoadObstacles(medium);
		LoadObstacles(small);

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
				_pool.tileGrid.tiles[x, y] = _pool.CreateEntity().IsTile(true).AddGridPosition(x, y).IsImpassible(true);
			}
		}

		CarvePath(difficulty);
	}

	private void CarvePath(int length)
	{
		var x_start = Random.Range(2, _pool.collisionGrid.passible.GetLength(0) - 3);
		var y_start = Random.Range(2, _pool.collisionGrid.passible.GetLength(1) - 3);

		//TODO: Don't spawn poor roomy in this lonely function
		Entity roomy =
			_pool.CreateEntity()
				.IsRoomy(true)
				.IsInputReceiver(true)
				.AddGridPosition(x_start, y_start)
				.AddHeading(Move.Direction.Up);

		var roomyObj = Resources.Load("Roomy") as GameObject;
		roomy.AddView(GameObject.Instantiate(roomyObj).transform);

		//Occupied so no furniture "arches" can cover it
		_pool.collisionGrid.occupied[x_start, y_start] = true;


		BackTrackCarve(length, x_start, y_start);
	}


	private bool BackTrackCarve(int length, int x, int y)
	{
		if (x >= 0 && y >= 0 && x < _pool.collisionGrid.passible.GetLength(0) && y < _pool.collisionGrid.passible.GetLength(1))
		{
			if (_pool.collisionGrid.passible[x, y]) return false;

			_pool.collisionGrid.passible[x, y] = true;
			_pool.tileGrid.tiles[x, y].isImpassible = false;

			if (length == 0)
			{
				//Place Poo at end of path.
				var tile = _pool.tileGrid.tiles[x, y];
				tile.isPoop = true;
				var pooObj = Resources.Load("Poo") as GameObject;
				tile.AddView(GameObject.Instantiate(pooObj).transform);

				//Occupied so no furniture "arches" can cover it
				_pool.collisionGrid.occupied[x, y] = true;

				return true;
			}

			foreach (Move.Direction d in Move.Shuffled())
			{
				var xn = x;
				var yn = y;

				Move.Apply(d, ref xn, ref yn);

				if (BackTrackCarve(length - 1, xn, yn)) return true;
			}

			//No good path found, track back
			_pool.collisionGrid.passible[x, y] = false;
			_pool.tileGrid.tiles[x, y].isImpassible = true;
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

				//Don't build anything on occupied tiles
				if (occupied) return false;

				//Check if mask fits the passible plots
				if (mask[i, j] && passible) return false;

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


	private void Shuffle<T, U>(T[] array1, U[] array2)
	{
		// Knuth shuffle algorithm :: courtesy of Wikipedia :)
		for (int t = 0; t < array1.Length; t++)
		{
			int r = UnityEngine.Random.Range(t, array1.Length);
			var tmp1 = array1[t];
			array1[t] = array1[r];
			array1[r] = tmp1;

			var tmp2 = array2[t];
			array2[t] = array2[r];
			array2[r] = tmp2;
		}
	}

	private void Shuffle<T>(T[] array1)
	{
		// Knuth shuffle algorithm :: courtesy of Wikipedia :)
		for (int t = 0; t < array1.Length; t++)
		{
			int r = UnityEngine.Random.Range(t, array1.Length);
			var tmp1 = array1[t];
			array1[t] = array1[r];
			array1[r] = tmp1;
		}
	}

	private void LoadObstacles(string[] assets, bool shuffle = true)
	{
		var obstacles = new GameObject[assets.Length];
		var footprints = new Footprint[assets.Length];

		for (int i = 0; i < assets.Length; i++)
		{
			obstacles[i] = Resources.Load(assets[i]) as GameObject;
			footprints[i] = obstacles[i].GetComponent<Footprint>();
		}

		//We shuffle the tiles as well.
		var group = _pool.GetGroup(Matcher.Impassible);
		var spots = group.GetEntities();
		Array.Sort<Entity>(spots, this);

		//Select small spots first for lights.
		if (!shuffle) Array.Reverse(spots);

		if (shuffle)
		{
			//Attempt to fill each spot with random assets
			foreach (var spot in spots)
			{
				var x = spot.gridPosition.x;
				var y = spot.gridPosition.y;
				Shuffle<GameObject, Footprint>(obstacles, footprints);
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
		else
		{
			//Attempt to place each asset.
			for (int i = 0; i < assets.Length; i++)
			{
				foreach (var spot in spots)
				{
					var x = spot.gridPosition.x;
					var y = spot.gridPosition.y;

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

	public int Compare(Entity a, Entity b)
	{
		var xmax = _pool.collisionGrid.passible.GetLength(0);
		var ymax = _pool.collisionGrid.passible.GetLength(1);

		var score_a = 0;
		for (int i = a.gridPosition.x; i < Mathf.Min(a.gridPosition.x + 3, xmax); i++)
		{
			for (int j = a.gridPosition.y; j < Mathf.Min(a.gridPosition.y + 3, ymax); j++)
			{
				if (!_pool.collisionGrid.passible[i, j])
					score_a++;
			}
		}
		if (a.gridPosition.x == 0 || a.gridPosition.y == 0 || a.gridPosition.x == xmax - 1 || a.gridPosition.y == ymax - 1)
			score_a += 10;

		var score_b = 0;
		for (int i = b.gridPosition.x; i < Mathf.Min(b.gridPosition.x + 3, xmax); i++)
		{
			for (int j = b.gridPosition.y; j < Mathf.Min(b.gridPosition.y + 3, ymax); j++)
			{
				if (!_pool.collisionGrid.passible[i, j])
					score_b++;
			}
		}
		if (b.gridPosition.x == 0 || b.gridPosition.y == 0 || b.gridPosition.x == xmax - 1 || b.gridPosition.y == ymax - 1)
			score_b += 10;

		return score_b - score_a;
	}

	public void Execute(List<Entity> entities)
	{
		if (level < 0)
		{
			level = 0;
		}
		else
		{
			level++;
		}
		_pool.isNextLevel = false;
		Initialize();
	}

public TriggerOnEvent trigger {
		get { return Matcher.NextLevel.OnEntityAdded(); }
	}
}