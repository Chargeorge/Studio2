using System;

public class AStarholder
{
	public BaseTile current;
	public AStarholder parent;
	public int hurVal;
	public int pathCostToHere;
	public bool reachableThisTurn;
	public int apAtThisTurn;
	public int fVal{
		get{
			return hurVal+pathCostToHere;
		}
	}
	public AStarholder (BaseTile current, AStarholder parent)
	{
		this.current = current;
		this.parent = parent;
	}
}

