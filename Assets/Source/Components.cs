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

[SingleEntity]
public class CollisionGrid : IComponent
{
	public bool[,] passible;
}

[SingleEntity]
public class TileGrid : IComponent
{
	public Entity[,] tiles;
}


public class Roomy : IComponent
{
	
}

public class GridPosition : IComponent
{
	public int x;
	public int y;

	public Vector3 WorldPosition()
	{
		return new Vector3(0.5f*x, 0, 0.5f*y);
	}
}