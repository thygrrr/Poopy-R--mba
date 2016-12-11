using System.Collections.Generic;
using System.Security.Permissions;
using Entitas;
using Entitas.CodeGenerator;
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

public class Roomy : IComponent
{
	
}

public class Poop : IComponent
{ }

public class Move : IComponent
{
	public Direction direction;

	public enum Direction
	{
		Up, Left, Down, Right
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

	public Vector3 WorldPosition()
	{
		return new Vector3(0.5f*x, -0.025f, 0.5f*y);
	}

	public bool Equals(int x, int y)
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