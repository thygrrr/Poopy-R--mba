using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Entitas;
using Entitas.CodeGenerator;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class Dirty : IComponent
{
}

public class View : IComponent
{
	public Transform transform;
}

public class Tile : IComponent
{
}

public class Charge : IComponent
{
	public int value;
}

[SingleEntity]
public class CollisionGrid : IComponent
{
	public bool[,] passible;
	public bool[,] occupied;
}

[SingleEntity]
public class TileGrid : IComponent
{
	public Entity[,] tiles;
}

[SingleEntity]
public class Score : IComponent
{
	public int value;
}

[SingleEntity]
public class Percentage : IComponent
{
	public float value;
}

public class Roomy : IComponent
{
}

public class Poop : IComponent
{
}

public class Move : IComponent
{
	public Direction direction;

	public enum Direction
	{
		Up,
		Left,
		Down,
		Right
	}

	private static System.Random rng = new System.Random();

	public static Direction Random()
	{
		var v = Enum.GetValues(typeof(Direction));
		return (Direction)v.GetValue(rng.Next(v.Length));
	}

	public static Direction[] Shuffled()
	{
		Direction[] result = {Direction.Up, Direction.Down, Direction.Left, Direction.Right};

		// Knuth shuffle algorithm :: courtesy of Wikipedia :)
		for (int t = 0; t < result.Length; t++)
		{
			var tmp = result[t];
			int r = UnityEngine.Random.Range(t, result.Length);
			result[t] = result[r];
			result[r] = tmp;
		}

		return result;
	}


	public static void Apply(Direction d, ref int x, ref int y)
	{
		switch (d)
		{
			case Direction.Up:
				y += 1;
				return;

			case Direction.Down:
				y -= 1;
				return;

			case Direction.Right:
				x += 1;
				return;

			case Direction.Left:
				x -= 1;
				return;
		}
	}
}

public class Heading : IComponent
{
	public Move.Direction direction;

	public static Dictionary<Move.Direction, float> angles = new Dictionary<Move.Direction, float>()
	{
		{Move.Direction.Up, 0},
		{Move.Direction.Down, 180},
		{Move.Direction.Right, 90},
		{Move.Direction.Left, -90},
	};
}

public class Traveling : IComponent
{
}

public class Orienting : IComponent
{
}

public class GridPosition : IComponent
{
	public int x;
	public int y;

	public Vector3 WorldPosition ()
	{
		return new Vector3(0.5f * x, -0.025f, 0.5f * y);
	}

	public bool Equals (int x, int y)
	{
		return this.x == x && this.y == y;
	}
}

	public class InputReceiver : IComponent
	{
	}


	public class Physics : IComponent
	{
		public Rigidbody rigidbody;
	}


public class PooDistance : IComponent
{
	public int distance;
}

public class Impassible : IComponent
{
}

[SingleEntity]
public class Failure : IComponent
{
	
}

[SingleEntity]
public class Success : IComponent
{
	
}