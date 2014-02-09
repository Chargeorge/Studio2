using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseTile : MonoBehaviour {
	
	
	public GameObject qudHoverLayer;
	public GameObject qudSelectedLayer;
	public GameObject qudHighlightLayer;
	
	
	private int _brdXPos;
	private int _brdYPos;
	
	private int _ident;

	private Color _highlightColor = new Color(0f,0f, 0f);

	public Color HighlightColor {
		get {
			return this._highlightColor;
		}
		set {
			_highlightColor = value;
		}
	}	
	
	public int Ident {
		get {
			return this._ident;
		}
	}	
	public int brdXPos {
		get {
			return this._brdXPos;
		}
		set {
			_brdXPos = value;
		}
	}
	public int brdYPos {
		get {
			return this._brdYPos;
		}
		set {
			_brdYPos = value;
		}
	}	
	
	private bool _isHover;
	public bool IsHover {
		get {
			return this._isHover;
		}
		set {
			_isHover = value;
		}
	}	

	private bool _isHighlighted;

	public bool IsHighlighted {
		get {
			return this._isHighlighted;
		}
		set {
			_isHighlighted = value;
		}
	}	
	private bool _isSelected;

	public bool IsSelected {
		get {
			return this._isSelected;
		}
		set {
			_isSelected = value;
		}
	}	
		
	
		
	
	private BaseTile _North;
	private BaseTile _South;
	private BaseTile _East;
	private BaseTile _West;
	
	private int _MoveCost;
	private float _DamageModifier;
	
	

	
	public BaseTile East {
		get {
			return this._East;
		}
		set {
			_East = value;
		}
	}

	public BaseTile North {
		get {
			return this._North;
		}
		set {
			_North = value;
		}
	}

	public BaseTile South {
		get {
			return this._South;
		}
		set {
			_South = value;
		}
	}

	public BaseTile West {
		get {
			return this._West;
		}
		set {
			_West = value;
		}
	}
	

	public float DamageModifier {
		get {
			return this._DamageModifier;
		}
		set {
			_DamageModifier = value;
		}
	}

	public int MoveCost {
		get {
			return this._MoveCost;
		}
		set {
			_MoveCost = value;
		}
	}
	// Use this for initialization
	void Start () {
		_ident = Random.Range(1, 10000000);
	}
	
	public BaseTile GetDirection(DirectionEnum dir){
		switch (dir){
			case DirectionEnum.North: return North;
			case DirectionEnum.East: return East;
			case DirectionEnum.South: return South;
				case DirectionEnum.West: return West;
		default: return null;
		}
	}
	// Update is called once per frame
	void Update () {
		
		qudHighlightLayer.GetComponent<MeshRenderer>().enabled = (IsHighlighted) ? true : false ;
		
		
		qudHighlightLayer.renderer.material.color = _highlightColor;
		qudHoverLayer.GetComponent<MeshRenderer>().enabled = (IsHover) ? true : false ;
		if(IsHover){
	
		}
		
		Component[] meshes = qudSelectedLayer.GetComponentsInChildren<MeshRenderer>(); 
		//if(IsSelected) Debug.Log ("Selected");
		for (int i = 0; i< meshes.Length; i++){
			
			((MeshRenderer)meshes[i]).enabled = IsSelected;	
			
		}
		
	}
	
	public static void createTile(TileTypeEnum et, GameObject currentTile){
		
		
		currentTile.GetComponent<BaseTile>().IsHover = false;
		currentTile.GetComponent<BaseTile>().IsSelected = false;
		currentTile.GetComponent<BaseTile>().IsHighlighted = false;
		
		switch (et){
			case TileTypeEnum.regular:
				currentTile.renderer.material = (Material)Resources.Load("Sprites/Materials/Regular");
			break;

			case TileTypeEnum.water:
				currentTile.renderer.material = (Material)Resources.Load("Sprites/Materials/Water");
			break;
		}
			
		//Debug.Log (Resources.Load("Sprites/Materials/River").name);
	}
	///Old section needs to be fixed
	/// 
	/*
	public  List<BaseTile> getReachable(int currentAp){
		Dictionary<int,  BaseTile> visited = new Dictionary<int, BaseTile>();
		if(North!=null && North.Hero == null){
			if(currentAp - North.MoveCost >=0){
				getReachableBody(North, visited, currentAp-North.MoveCost);
			}
		}
		if(South!=null && South.Hero == null){
			if(currentAp - South.MoveCost >=0){
				getReachableBody(South,visited, currentAp-South.MoveCost);
			}
		}
		if(East!=null&& East.Hero == null){
			if(currentAp - East.MoveCost >=0){
				getReachableBody(East,visited, currentAp-East.MoveCost);
			}
		}
		if(West !=null && West.Hero == null){
			if(currentAp - West.MoveCost >=0){
				getReachableBody(West,visited, currentAp-West.MoveCost);
			}
		}
		
		List<BaseTile> returnable = new List<BaseTile>();
		foreach(KeyValuePair<int, BaseTile> entry in visited)
		{
		    returnable.Add(entry.Value);
		}
		
		return returnable;
	}
	
	public void getReachableBody(BaseTile Current, Dictionary<int, BaseTile> visited, int currentAp){
		if(!visited.ContainsKey(Current.Ident)){
			visited.Add(Current.Ident,Current);
			if(Current.North!=null  && Current.North.Hero == null){
				if(currentAp - Current.North.MoveCost >=0){
					getReachableBody(Current.North, visited, currentAp-Current.North.MoveCost);
				}
			}
			if(Current.South!=null && Current.South.Hero == null){
				if(currentAp - Current.South.MoveCost >=0){
					getReachableBody(Current.South,visited, currentAp-Current.South.MoveCost);
				}
			}
			if(Current.East!=null  && Current.East.Hero == null){
				if(currentAp - Current.East.MoveCost >=0){
					getReachableBody(Current.East,visited, currentAp-Current.East.MoveCost);
				}
			}
			if (Current.West!=null  && Current.West.Hero == null){
				if(currentAp - Current.West.MoveCost >=0){
					getReachableBody(Current.West,visited, currentAp-Current.West.MoveCost);
				}
			}
		}
	}
	
	public List<BaseTile> getLocalOpenTiles(int currentAp){
		List<BaseTile> returnable = new List<BaseTile>();
		if(North!=null && North.Hero == null){
			if(currentAp - North.MoveCost >=0){
				returnable.Add(North);
			}
		}
		if(South!=null && South.Hero == null){
			if(currentAp - South.MoveCost >=0){
				returnable.Add(South);
			}
		}
		if(East!=null&& East.Hero == null){
			if(currentAp - East.MoveCost >=0){
				returnable.Add(East);
			}
		}
		if(West !=null && West.Hero == null){
			if(currentAp - West.MoveCost >=0){
				returnable.Add(West);
			}
		}
			return returnable;
	
	}*/


	/// <summary>
	///  STUB method
	/// </summary>
	/// <returns>The local open tiles.</returns>
	/// <param name="currentAp">Current ap.</param>
	public List<BaseTile> getLocalOpenTiles(int currentAp){
		return null;
	}
		
	public static List<AStarholder> aStarSearch(BaseTile start, BaseTile end, int currentAp){
		Dictionary<int, AStarholder> closedSet  = new Dictionary<int, AStarholder>();		
		Dictionary<int, AStarholder> openSet = new Dictionary<int, AStarholder>();
		
		AStarholder current = new AStarholder(start, null);
		current.hurVal = current.current.calcHueristic(current.current, end);
		current.apAtThisTurn = currentAp;
		current.pathCostToHere = 0;
		
		openSet.Add(current.current.Ident, current);
		List<AStarholder> returnable = new List<AStarholder>();
		while (openSet.Count >0){
			openSet.Remove(current.current.Ident);
			closedSet.Add(current.current.Ident, current);
			
			if(current.current.Ident == end.Ident){ BaseTile.reconstructPath(start, current, returnable); return returnable;}
			
			List<BaseTile> listies = current.current.getLocalOpenTiles(int.MaxValue);
			listies.ForEach(delegate (BaseTile bt){
				if(!closedSet.ContainsKey(bt.Ident)){
					AStarholder newOpen = new AStarholder(bt, current);
					newOpen.hurVal = newOpen.current.calcHueristic(newOpen.current, end);
					newOpen.apAtThisTurn = current.apAtThisTurn - newOpen.current.MoveCost;
					newOpen.pathCostToHere = newOpen.current.MoveCost + current.pathCostToHere;
					if(openSet.ContainsKey(bt.Ident)){
						if(openSet[bt.Ident].fVal > newOpen.fVal){
							openSet[bt.Ident] = newOpen;
						}
					}
					else{
						openSet.Add (bt.Ident, newOpen);
					}
				}
			});
			current = null;
			foreach(KeyValuePair<int, AStarholder> val in openSet){
				if(current == null){
					current = val.Value;
				}
				if(current.fVal > val.Value.fVal){
					current = val.Value;
				}
			}
		}
		//Ran out of moves, find the lowest HurVal and return path to it
		
		AStarholder finalNode = null;
		foreach(KeyValuePair<int, AStarholder> val in openSet){
			
		
			if(finalNode == null){
				finalNode = val.Value;
			}
			if(current.fVal > val.Value.fVal){
				finalNode = val.Value;
			}
			
			
		}
		
		 BaseTile.reconstructPath(start, finalNode, returnable);
		return returnable;
	}
	
	public static void reconstructPath (BaseTile start, AStarholder a, List<AStarholder> returnable){
		if(a!= null){
			if(a.current.Ident == start.Ident){
				returnable.Add(a);
				
			}else{
				reconstructPath(start, a.parent,  returnable);
				returnable.Add(a);
				
			}
		}
	}
	
	public int calcHueristic(BaseTile start, BaseTile end){
		int manhattan = Mathf.Abs(end.brdXPos - start.brdXPos) + Mathf.Abs(end.brdYPos - start.brdYPos);
		return manhattan*4; //Average tile cost;
	}
	
	public List<BaseTile> getDirections(){
		List<BaseTile> returnable = new List<BaseTile>();
		if(this.North != null) returnable.Add (this.North);
		if(this.South != null) returnable.Add (this.South);
		if(this.East != null) returnable.Add (this.East);
		if(this.West != null) returnable.Add (this.West);
		return returnable;
	}
	
	public DirectionEnum? getRelation(BaseTile toCheck){
		if(toCheck.brdXPos == brdXPos && toCheck.brdYPos == brdYPos+1) return DirectionEnum.North;
		if(toCheck.brdXPos == brdXPos && toCheck.brdYPos == brdYPos-1) return DirectionEnum.South;
		if(toCheck.brdXPos == brdXPos+1 && toCheck.brdYPos == brdYPos) return DirectionEnum.East;
		if(toCheck.brdXPos == brdXPos-1 && toCheck.brdYPos == brdYPos) return DirectionEnum.West;
		return null;
	}
	
	public BaseTile getClosestOpenTile(BaseTile ToCheck){
		List<BaseTile> open = getLocalOpenTiles(int.MaxValue);
		BaseTile returnable = null;
		int minVal = int.MaxValue;
		open.ForEach(delegate(BaseTile obj) {
			if(ToCheck.calcHueristic(ToCheck, obj) < minVal){ returnable = obj; minVal = ToCheck.calcHueristic(ToCheck, obj);}
		});
		return returnable;
	}
	
}
