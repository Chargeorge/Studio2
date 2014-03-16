using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
public class BaseTile : MonoBehaviour {
	
	
	public GameObject qudBeaconLayer;
	public GameObject qudSelectedLayer;
	public GameObject qudInfluenceLayer;
	
	public TeamInfo controllingTeam;  //LOGIC TO ADD owningTeam ONLY CHANGES when controlling team reaches 100 or 0
	public float percControlled;
	public TeamInfo owningTeam;  //OWNING team is the official owning team, use it for defining networks and movement cost.
	
	public GameObject beacon;
	private Animator anim;
			
	public TileState currentState;
	private int _brdXPos;
	private int _brdYPos;
	public TileTypeEnum currentType;
	private int _ident;
	public List<AStarholder> networkToBase;
	private GameObject _qudFogLayer;

	public GameObject qudFogLayer {
		get {
			if(_qudFogLayer == null){
				_qudFogLayer = transform.FindChild("Fog").gameObject;
			}
			return _qudFogLayer;
		}
		set {
			_qudFogLayer = value;
		}
	}	
	
	private Settings sRef;
	
	private int _influenceRevealRange = 3;
	private GameManager gm;
	//Delegate used for different A* methods
	public delegate List<BaseTile> GetLocalTiles(BaseTile Base, TeamInfo T);

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
	
	private bool _isRevealed;
	public bool IsRevealed {
		get {
			return this._isRevealed;
		}
		set {
			_isRevealed = value;
			qudFogLayer.renderer.enabled = !value;
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
		_ident = UnityEngine.Random.Range(1, 10000000);
		currentState = TileState.normal;
		networkToBase = new List<AStarholder>();
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
		sRef = Settings.SettingsInstance;
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
		if(controllingTeam != null){
			
			qudInfluenceLayer.SetActive(true);
			Color32 controllingTeamColor = controllingTeam.tileColor;
			//controllingTeamColor.a = (byte) (255*(percControlled/100f));
			
			controllingTeamColor.a = (byte) (255*(percControlled/100f) * 0.5f);

			if (percControlled >= 100f) controllingTeamColor.a = (byte) 255;
			
			qudInfluenceLayer.renderer.material.color = controllingTeamColor;
//			qudInfluenceLayer.renderer.material.color = controllingTeam.tileColor;			
			qudInfluenceLayer.GetComponent<MeshRenderer>().enabled = true;
		}
		else{
			qudBeaconLayer.GetComponent<MeshRenderer>().enabled = false;
		}
		qudBeaconLayer.GetComponent<MeshRenderer>().enabled = (IsHover) ? true : false ;
		if(IsHover){

		}
		
		Component[] meshes = qudSelectedLayer.GetComponentsInChildren<MeshRenderer>(); 
		//if(IsSelected) Debug.Log ("Selected");
		for (int i = 0; i< meshes.Length; i++){
			
			((MeshRenderer)meshes[i]).enabled = IsSelected;	
			
		}
		
		if(owningTeam== null){
			transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
		}
		else{	//Removing this; will use outline to show where player is, not who owns it
//			transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = true;
//			transform.Find("OwnedLayer").GetComponent<MeshRenderer>().material.color = owningTeam.getHighLightColor();
		}
	}
	
	public static void createTile(TileTypeEnum et, GameObject currentTile){
		
		
		currentTile.GetComponent<BaseTile>().IsHover = false;
		currentTile.GetComponent<BaseTile>().IsSelected = false;
		currentTile.GetComponent<BaseTile>().IsHighlighted = false;
		
		switch (et){
			case TileTypeEnum.regular:
				currentTile.renderer.material = (Material)Resources.Load("Sprites/Materials/Regular");
				currentTile.GetComponent<BaseTile>().currentType = TileTypeEnum.regular;
			break;

			case TileTypeEnum.water:
				currentTile.renderer.material = (Material)Resources.Load("Sprites/Materials/Water");
				currentTile.GetComponent<BaseTile>().currentType = TileTypeEnum.water;
			break;
		}
			
	}

	
	public void setTileType(TileTypeEnum TT){
		switch (TT){
		case TileTypeEnum.regular:
			gameObject.renderer.material = (Material)Resources.Load("Sprites/Materials/Regular");
			currentType = TileTypeEnum.regular;
			break;
			
		case TileTypeEnum.water:
			gameObject.renderer.material = (Material)Resources.Load("Sprites/Materials/Water");
			currentType = TileTypeEnum.water;
			break;
		}
	}
	/// <summary>
	/// Recurse through tileset to find if we can reach all 4 edges
	/// </summary>
	public bool findEdges(){
		bool northFound = false, southFound = false, eastFound = false, westFound = false;
		List<BaseTile> tiles = getLocalTraversableTiles(this, null);
		Dictionary<int, BaseTile> visited = new Dictionary<int, BaseTile >();
		foreach(BaseTile B in tiles){
			findEdgesInner(ref northFound, ref southFound, ref eastFound, ref westFound,visited,getLocalTraversableTiles,   B);
		}
		if(northFound && southFound && westFound && eastFound){
			return true;
		}
		else{
			return false;
		}
	}

	public void findEdgesInner(ref bool northFound, 
	                          ref bool southFound, 
	                          ref bool eastFound,
	                          ref bool westFound, 
	                          Dictionary<int, BaseTile> visited, 
	                          GetLocalTiles LocalFunction, BaseTile current){

		if(!visited.ContainsKey(current.Ident)){
			visited.Add(current.Ident, current);
			if(current.North == null){
				northFound = true;
			}
			if(current.West == null){
				westFound = true;
			}
			if(current.East == null){
				eastFound = true;
			}
			if(current.South == null){
				southFound = true;
			}

			List<BaseTile> tilesToVisit  = LocalFunction(current, controllingTeam);
			foreach (BaseTile B in tilesToVisit){
				findEdgesInner(ref northFound, ref southFound, ref eastFound, ref westFound, visited, LocalFunction, B);
			}
		}
		
	}
	#region oldCode
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
	
	#endregion oldCode


	/// <summary>
	///  STUB method
	/// </summary>
	/// <returns>The local open tiles.</returns>
	/// <param name="currentAp">Current ap.</param>
	public static List<BaseTile> getLocalTraversableTiles(BaseTile current, TeamInfo T){
	//	if(
		List<BaseTile> returnable = current.getLocalTiles();
		returnable.RemoveAll(x => x.currentType == TileTypeEnum.water);
		return returnable;
	}
		
	public static List<BaseTile> getLocalSameTeamTiles(BaseTile current, TeamInfo T){	
		List<BaseTile> returnable = current.getLocalTiles();
		returnable.RemoveAll(x => x.owningTeam == null);
		returnable.RemoveAll(x => x.owningTeam.teamNumber != T.teamNumber);
		return returnable;
		
	}
	
	public static List<BaseTile> getLocalNonTiles(BaseTile current, TeamInfo T){
		return null;
	}

	public List<BaseTile> getLocalTiles(){
		List<BaseTile> returnable = new List<BaseTile>();
		if(North!= null) returnable.Add(North);
		if(South!= null) returnable.Add(South);
		if(East!= null) returnable.Add(East);
		if(West!= null) returnable.Add(West);
		return returnable;
	}
		
	public static List<AStarholder> aStarSearch(BaseTile start, BaseTile end, int currentAp, GetLocalTiles LocalMethod, TeamInfo team){
		Dictionary<int, AStarholder> closedSet  = new Dictionary<int, AStarholder>();		
		Dictionary<int, AStarholder> openSet = new Dictionary<int, AStarholder>();
		
		AStarholder current = new AStarholder(start, null);
		current.hurVal = current.current.calcHueristic(current.current, end);
		//current.apAtThisTurn = currentAp;
		current.pathCostToHere = 0;
		
		openSet.Add(current.current.Ident, current);
		List<AStarholder> returnable = new List<AStarholder>();
		while (openSet.Count >0){
			openSet.Remove(current.current.Ident);
			closedSet.Add(current.current.Ident, current);
			
			if(current.current.Ident == end.Ident){ BaseTile.reconstructPath(start, current, returnable); return returnable;}
			
			List<BaseTile> listies =LocalMethod(current.current, team);
			listies.ForEach(delegate (BaseTile bt){
				if(!closedSet.ContainsKey(bt.Ident)){
					AStarholder newOpen = new AStarholder(bt, current);
					newOpen.hurVal = newOpen.current.calcHueristic(newOpen.current, end);
					//newOpen.apAtThisTurn = current.apAtThisTurn - newOpen.current.MoveCost;
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
	
	public BaseTile getClosestOpenTile(BaseTile ToCheck, TeamInfo T, GetLocalTiles locals){
		List<BaseTile> open = locals(ToCheck, T);
		BaseTile returnable = null;
		int minVal = int.MaxValue;
		open.ForEach(delegate(BaseTile obj) {
			if(ToCheck.calcHueristic(ToCheck, obj) < minVal){ returnable = obj; minVal = ToCheck.calcHueristic(ToCheck, obj);}
		});
		return returnable;
	}
	
	/// <summary>
	/// Protyping stub for now.
	/// </summary>
	/// <returns>The rate.</returns>
	/// <param name="testing">Testing.</param>
	public float GetRate(Player testing){
		if(currentType == TileTypeEnum.water){
			if (GameObject.Find ("GameManager").GetComponent<GameManager>().getCapturedAltars(testing.team).Contains (AltarType.Thotzeti)){
				return 1f;
			}
			else{
				return 0f;
			}
		}
		if(controllingTeam != null){
			if(controllingTeam.teamNumber == testing.teamNumber){
				return 2f; 
			}
			else{
				return .5f;
			}
		}
		return 1;
	}
	
	public void startInfluence(float initialProgress, TeamInfo team){
		///TODO: add start semaphore stuff here
		currentState = TileState.beingInfluenced;
		controllingTeam = team;
		percControlled = initialProgress;
	}
	
	public void addProgressToInfluence(float rate){
		percControlled += rate*Time.deltaTime;
	}
	/// <summary>
	/// adds influence, if influence is maxed it returns true
	/// </summary>
	/// <returns><c>true</c>, if progress to influence was added, <c>false</c> otherwise.</returns>
	/// <param name="rate">Rate.</param>
	/// <param name="newTeam">New team.</param>
	public bool addProgressToInfluence(float rate, TeamInfo newTeam){
		bool returnable  = false;
		if(controllingTeam != null && controllingTeam.teamNumber != newTeam.teamNumber){
			percControlled -= rate*Time.deltaTime;
			if(percControlled <=0) {
				percControlled = 0;
				flipInfluence(newTeam);
			}
		}else{
			if(controllingTeam == null){
				startInfluence(rate*Time.deltaTime, newTeam);
			}
			else{
				percControlled += rate*Time.deltaTime;
				if(percControlled >= 100) {
					finishInfluence();
					returnable = true;
				}
			}
		}
		return returnable;
	}
	
	/// <summary>
	/// Subtract any influence, return any influence over the flip
	/// </summary>
	/// <returns>The tract influence.</returns>
	/// <param name="rate">Rate.</param>
	public float subTractInfluence(float amt, TeamInfo subtractingTeam){
		if(percControlled > 0){
			percControlled -= amt;
		}
		if(percControlled >0){
			return 0f;
		}
		else{
			float returnable = Math.Abs (percControlled);
			flipInfluence(subtractingTeam);
			return returnable;
			
		}
		
	}
	
	///
	public float addInfluenceReturnOverflow(float amt){
		if(percControlled < 100f){
			percControlled += amt;
			
		}
		if(percControlled >100f){
			float returnable =  percControlled -100f;
			finishInfluence();
			return returnable;
		}
		return 0f;
	}
	
	
	public void flipInfluence(TeamInfo newTeam){
		owningTeam.score -= getTileScore();
		
		controllingTeam = newTeam;
		percControlled = 0;
		owningTeam = null;
		
		Beacon localBeacon = GetComponentInChildren<Beacon>();
		Altar localAltar = GetComponentInChildren<Altar>();
		
		if(localBeacon !=null){
			
		}
		if(localAltar !=null){
			localAltar.setControl(null);
		}
	}
	public void finishInfluence(){
		///TODO: add end semaphore stuff her
		if(percControlled > 100f){
			percControlled = 100f;
			currentState = TileState.normal;
		}
		owningTeam = controllingTeam;
		Beacon localBeacon = GetComponentInChildren<Beacon>();
		Altar localAltar = GetComponentInChildren<Altar>();
		owningTeam.score += getTileScore();
		
		if(localAltar !=null){
			localAltar.setControl(owningTeam);
		}
		if(localBeacon!= null){

			localBeacon.setTeam (owningTeam);

		}
		
		Reveal (_influenceRevealRange);
		
	}
	public void clearInfluence(){
		percControlled = 0;
		controllingTeam = null;
		owningTeam = null;
		
		Beacon localBeacon = GetComponentInChildren<Beacon>();
		Altar localAltar = GetComponentInChildren<Altar>();
	}
	
	
	
	/*void OnMouseOver() {
		GameObject.Find("GameManager").GetComponent<GameManager>().debugMouse = this;
	}*/
	
	TeamInfo getHomeTeam(TeamInfo t){
		Home homeBase = GetComponentInChildren<Home>();
		if (homeBase != null){
			return homeBase.team;
		}
		else{
			return null;
		}
	}
	
	/// <summary>
	/// Reveal the specified range.
	/// </summary>																 x
	/// <param name="range">The maximum distance at which new tiles should be revealed. Ex 1 = xox</param>
	///																			 x
	public void Reveal (int range) {
		for (int i = range * -1; i <= range; i++){
			for (int j = (range - Mathf.Abs (i)) * -1; j <= range - Mathf.Abs (i); j++) {
				GameObject tile;
				try { tile = GameObject.Find ("GameManager").GetComponent<GameManager>().tiles[_brdXPos + j, _brdYPos + i]; }
				catch { tile = null; }
				if (tile != null) {
					tile.GetComponent<BaseTile>().IsRevealed = true;
				}		
			}
		}
	}
	
	public Altar getLocalAltar(){
		Altar localAltar = GetComponentInChildren<Altar>();
		return localAltar;
	}
	
	public bool checkNetworkToHomeBase(){
		if(owningTeam != null){
			List<AStarholder> As = 	BaseTile.aStarSearch(this,gm.getTeamBase(owningTeam),int.MaxValue, getLocalSameTeamTiles, owningTeam);
			if(As.Count> 0){
				networkToBase = As;
				return true;
				
			}
			else{
				return false;
			}
		}
		return false;
	}
	
	public float getTileScore(){
		return sRef.valTileConvertScore;
	}
	
	public bool tooCloseToBeacon() {
		for (int i = sRef.beaconNoBuildRange * -1; i <= sRef.beaconNoBuildRange; i++){
			for (int j = (sRef.beaconNoBuildRange - Mathf.Abs (i)) * -1; j <= sRef.beaconNoBuildRange - Mathf.Abs (i); j++) {
				GameObject tile;
				try { tile = GameObject.Find ("GameManager").GetComponent<GameManager>().tiles[_brdXPos + j, _brdYPos + i]; }
				catch { tile = null; }
				if (tile != null && 
					tile.GetComponent<BaseTile>().beacon != null && 
					tile.GetComponent<BaseTile>().beacon.GetComponent<Beacon>().currentState != BeaconState.BuildingBasic) 
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool buildable(){
		if(transform.Find ("Altar") != null){
			return false;
		}
		if(currentType ==TileTypeEnum.water){
			return false;
		}
		if(gm.teams[0].startingLocation.x ==brdXPos && gm.teams[0].startingLocation.y ==brdYPos){
			return false;
		}
		if(gm.teams[1].startingLocation.x ==brdXPos && gm.teams[1].startingLocation.y ==brdYPos){
			return false;
		}
		return true;
	}
}
