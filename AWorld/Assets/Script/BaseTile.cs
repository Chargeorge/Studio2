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
	private float _percControlled;
	public float percControlled{
		get{
			return _percControlled;
		}
	}
	public TeamInfo owningTeam;  //OWNING team is the official owning team, use it for defining networks and movement cost.
	
	private Settings _sRef;
	public Settings sRef{
		get{
			if(_sRef == null){
				_sRef = Settings.SettingsInstance;
			}
			return _sRef;
		}
		set{
			_sRef = value;
		}
	}
	
	private GameManager _gm;
	public GameManager gm{
		get{
			if(_gm == null){
				_gm = GameManager.GameManagerInstance;
			}
			return _gm;
		}
		set{
			_gm = value;
		}
	}
	public GameObject beacon;
	private Animator anim;
	private GameObject _scoreBitTarget;
			
	public TileState currentState;
	private int _brdXPos;
	private int _brdYPos;
	public TileTypeEnum currentType;
	private int _ident;
	private int _MoveCost;
	public List<AStarholder> networkToBase;
	private int? _distanceToHomeBase;
	//Delegate used for different A* methods
	public delegate List<BaseTile> GetLocalTiles(BaseTile Base, TeamInfo T);
	
	private GameObject _qudBaseLayer;
	private GameObject _qudFogLowerLayer;
	private GameObject _qudFogUpperLayer;
	private GameObject _qudNoBuildLayer;
	private GameObject _qudOwnedLayer;
	private GameObject _qudPulsingOwnedLayer;
		
	private bool _isRevealed;
	private bool _isHover;
	private bool _isHighlighted;
	private bool _isSelected;
	
	private BaseTile _North;
	private BaseTile _South;
	private BaseTile _East;
	private BaseTile _West;
	
	public bool tiltingFromPlayer;
	public bool tiltingFromBeacon;
	private float _jiggleRange = 0.05f;  //Max distance from center of position the tile will jiggle
	
	private float _maxTiltAngle = 10f;
	private float tiltRate = Mathf.PI;
	private float tiltTimer = 0f;
	private bool tiltingLeft;
	
	private ParticleSystem _PS;
	
	private Color _highlightColor = new Color(0f,0f, 0f);
	
	#region get_n_set
	public GameObject qudBaseLayer {
		get {
			if(_qudBaseLayer == null){
				_qudBaseLayer = transform.FindChild("BaseLayer").gameObject;
			}
			return _qudBaseLayer;
		}
		set {
			_qudBaseLayer = value;
		}
	}	
	
	public GameObject qudFogUpperLayer {
		get {
			if(_qudFogUpperLayer == null){
				_qudFogUpperLayer = transform.FindChild("FogUpperLayer").gameObject;
			}
			return _qudFogUpperLayer;
		}
		set {
			_qudFogUpperLayer = value;
		}
	}	
	
	public GameObject qudFogLowerLayer {
		get {
			if(_qudFogLowerLayer == null){
				_qudFogLowerLayer = transform.FindChild("FogLowerLayer").gameObject;
			}
			return _qudFogLowerLayer;
		}
		set {
			_qudFogLowerLayer = value;
		}
	}
	
	public GameObject qudNoBuildLayer {
		get {
			if(_qudNoBuildLayer == null){
				_qudNoBuildLayer = transform.FindChild("NoBuildLayer").gameObject;
			}
			return _qudNoBuildLayer;
		}
		set {
			_qudNoBuildLayer = value;
		}
	}	

	public GameObject qudPulsingOwnedLayer {
		get {
			if(_qudPulsingOwnedLayer == null){
				_qudPulsingOwnedLayer = transform.FindChild("PulsingOwnedLayer").gameObject;
			}
			return _qudPulsingOwnedLayer;
		}
		set {
			_qudPulsingOwnedLayer = value;
		}
	}
		
	public GameObject qudOwnedLayer {
		get {
			if(_qudOwnedLayer == null){
				_qudOwnedLayer = transform.FindChild("OwnedLayer").gameObject;
			}
			return _qudOwnedLayer;
		}
		set {
			_qudOwnedLayer = value;
		}
	}
	
	public GameObject scoreBitTarget {
		get {
			if(_scoreBitTarget == null){
				_scoreBitTarget = transform.FindChild("ScoreBitTarget").gameObject;
			}
			return _scoreBitTarget;
		}
		set {
			_scoreBitTarget = value;
		}
	}

	public int? distanceToHomeBase {
		get {
			return _distanceToHomeBase;
		}

	}	

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

	public bool IsRevealed {
		get {
			return this._isRevealed;
		}
		set {
			_isRevealed = value;
			qudFogUpperLayer.renderer.enabled = !value;
			qudFogLowerLayer.renderer.enabled = !value;
		}
	}
	
	public bool IsHover {
		get {
			return this._isHover;
		}
		set {
			_isHover = value;
		}
	}	
	
	public bool IsHighlighted {
		get {
			return this._isHighlighted;
		}
		set {
			_isHighlighted = value;
		}
	}	
	
	public bool IsSelected {
		get {
			return this._isSelected;
		}
		set {
			_isSelected = value;
		}
	}	
	
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
	
	public int MoveCost {
		get {
			return this._MoveCost;
		}
		set {
			_MoveCost = value;
		}
	}
	
	public ParticleSystem PS{
		get{
			if(_PS == null) {_PS = GetComponent<ParticleSystem>();}
			return _PS;
		}
	}
	
#endregion			

	float[] influenceAdded;
	private float influenceThisFrame;
	// Use this for initialization
	void Start () {
		_ident = UnityEngine.Random.Range(1, 10000000);
		currentState = TileState.normal;
		networkToBase = new List<AStarholder>();
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
		sRef = Settings.SettingsInstance;
		influenceAdded = new float[6];
		transform.Find("NoBuildLayer").renderer.material.color = new Color32 (200,200,200,255);
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
			//controllingTeamColor.a = (byte) (255*(_percControlled/100f));


			transform.Find("NoBuildLayer").renderer.material.color = controllingTeam.tileColor;
			
			if (owningTeam != null && owningTeam == controllingTeam) {
				controllingTeamColor.a = (byte) (255-255*((1.0f-_percControlled/100f) * (1.0f - sRef.percMaxInfluenceColor)));
			}
			else {
				controllingTeamColor.a = (byte) (255*(_percControlled/100f) * sRef.percMaxInfluenceColor);
			}
			
			if (_percControlled >= 100f) controllingTeamColor.a = (byte) 255;
			
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
		/*
		Component[] meshes = qudSelectedLayer.GetComponentsInChildren<MeshRenderer>(); 
		//if(IsSelected) Debug.Log ("Selected");
		for (int i = 0; i< meshes.Length; i++){
			
			((MeshRenderer)meshes[i]).enabled = IsSelected;	
			
		}
		*/
		if(owningTeam== null){
			//transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
		}
		else{	//Removing this; will use outline to show where player is, not who owns it
//			transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = true;
//			transform.Find("OwnedLayer").GetComponent<MeshRenderer>().material.color = owningTeam.getHighLightColor();

			if(_distanceToHomeBase.HasValue && _distanceToHomeBase > 0){
				//int indexVal = ((gm.currentMarquee  - distanceToHomeBase.Value % sRef.marqueeCount) % sRef.marqueeCount);
				int indexVal = gm.currentMarquee - distanceToHomeBase.Value % sRef.marqueeCount;
				if (indexVal <= 0) indexVal += sRef.marqueeCount;
				//indexVal = (indexVal< 0) ? indexVal = 0 : indexVal;
				if(indexVal  < owningTeam.marqueeColorList.Count && indexVal >=0){
					qudPulsingOwnedLayer.renderer.material.color = owningTeam.marqueeColorList[indexVal];
				}
			}
		}
		
//		Vector3 jigglePos = transform.position;
//		Quaternion jiggleRot = transform.rotation;
		
		if (!Pause.paused) {
			float tiltZ = 0;
			if (tiltingFromPlayer || tiltingFromBeacon) {
		//			Vector3 positionOffset = new Vector3 (UnityEngine.Random.Range (-1 * _jiggleRange, _jiggleRange), UnityEngine.Random.Range (-1 * _jiggleRange, _jiggleRange), 0);
		//			jigglePos = transform.position + positionOffset;
				tiltTimer += Time.deltaTime;
				tiltZ = Mathf.Sin(tiltTimer * tiltRate) * _maxTiltAngle;
		//			Debug.Log (_percControlled);
				/**
				Quaternion toSlerp;
				if (tiltingLeft) {
					toSlerp = Quaternion.Euler (0, 0, _maxTiltAngle);
				}
				else {
					toSlerp = Quaternion.Euler (0, 0, -1 * _maxTiltAngle);
				}
				*/
							
			}
			
			else { 
				tiltingLeft = false;
			}
			
			Vector3 endRot = new Vector3 (0,0,tiltZ);
			
			//Needs optimizizizization
			qudBeaconLayer.transform.localEulerAngles = endRot;
			qudBaseLayer.transform.localEulerAngles = endRot;
			qudSelectedLayer.transform.localEulerAngles = endRot;
			qudInfluenceLayer.transform.localEulerAngles = endRot;
			qudPulsingOwnedLayer.transform.localEulerAngles = endRot;
			qudOwnedLayer.transform.localEulerAngles = endRot;
			
			if (beacon != null) {
				beacon.gameObject.transform.FindChild ("Platform").transform.localEulerAngles = endRot;
			}
		}
	}

	public bool getActionable(TeamInfo attemptingAction, bool playerInfluencing){
		float influenceOffset = (playerInfluencing) ? sRef.vpsBasePlayerInfluence * Time.deltaTime : 0; 
		float averageInfluence = getAverageInfluence();
		if(controllingTeam != attemptingAction){
			//check influence count
			if(averageInfluence + influenceOffset - sRef.vpsBasePlayerInfluence * Time.deltaTime < 0){
				return true;
			}
			else{
				return false;
			}

		}
		else{
			if(beacon != null){
				if(beacon.GetComponent<Beacon>().currentState == BeaconState.Advanced){
					return false;
				}
				else{
					return true;
				}
			}
			else{
				if(owningTeam == attemptingAction && !buildable()){
					return false;
				}
				else{
					return true;
				}
			}
		}
	}


	
	public static void createTile(TileTypeEnum et, GameObject currentTile){
		
		currentTile.GetComponent<BaseTile>().IsHover = false;
		currentTile.GetComponent<BaseTile>().IsSelected = false;
		currentTile.GetComponent<BaseTile>().IsHighlighted = false;
		
		switch (et){
			case TileTypeEnum.regular:
				currentTile.GetComponent<BaseTile>().qudBaseLayer.renderer.material = (Material)Resources.Load("Sprites/Materials/Regular");
				currentTile.GetComponent<BaseTile>().currentType = TileTypeEnum.regular;
			break;

			case TileTypeEnum.water:
				currentTile.GetComponent<BaseTile>().qudBaseLayer.renderer.material = (Material)Resources.Load("Sprites/Materials/Water");
				currentTile.GetComponent<BaseTile>().currentType = TileTypeEnum.water;
			break;
		}
			
	}

	
	public void setTileType(TileTypeEnum TT){
		switch (TT){
		case TileTypeEnum.regular:
			gameObject.transform.FindChild ("BaseLayer").renderer.material = (Material)Resources.Load("Sprites/Materials/Regular");
			currentType = TileTypeEnum.regular;
			break;
			
		case TileTypeEnum.water:
			gameObject.transform.FindChild ("BaseLayer").renderer.material = (Material)Resources.Load("Sprites/Materials/Water");
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
			if (GameManager.GameManagerInstance.getCapturedAltars(testing.team).Contains (AltarType.Thotzeti)){
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
		_percControlled = initialProgress;
	}
	
	public void addProgressToInfluence(float rate){
		_percControlled += rate*Time.deltaTime;
		influenceThisFrame += rate*Time.deltaTime;
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
			_percControlled -= rate*Time.deltaTime;
			influenceThisFrame -= rate*Time.deltaTime;
			if(_percControlled <=0) {
				_percControlled = 0;
				flipInfluence(newTeam);
			}
		}else{
			if(controllingTeam == null){
				startInfluence(rate*Time.deltaTime, newTeam);
			}
			else{
				_percControlled += rate*Time.deltaTime;
				influenceThisFrame += rate*Time.deltaTime;
				if(_percControlled >= 100) {
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
		if(_percControlled > 0){
			_percControlled -= amt;
			influenceThisFrame -= amt;
			return 0f;
		}

		else{
			float returnable = Math.Abs (_percControlled);
			flipInfluence(subtractingTeam);
			return returnable;
			
		}
		
	}
	
	///
	public float addInfluenceReturnOverflow(float amt){
		if(_percControlled < 100f){
			_percControlled += amt;
			influenceThisFrame += amt;
		}
		if(_percControlled >100f){
			float returnable = _percControlled -100f;
			finishInfluence();
			return returnable;
		}
		return 0f;
	}
	
	/// <summary>
	/// Sets tile to neutral control
	/// </summary>
	/// <param name="newTeam">New team.</param>
	public void flipInfluence(TeamInfo newTeam){
		if(owningTeam != null){
			owningTeam.score -= getTileScore();
		}
		controllingTeam = newTeam;
		_percControlled = 0;
		owningTeam = null;
		
		Beacon localBeacon = GetComponentInChildren<Beacon>();
		Altar localAltar = GetComponentInChildren<Altar>();
		
		if(localBeacon !=null){
			
		}
		if(localAltar !=null){
			localAltar.setControl(null);
		}
		//TODO: possible huge performance hit here
		gm.tileSendMessage("setDistanceToHomeBase");

	}
	/// <summary>
	/// Finish adding influence, set which team controls it
	/// </summary>
	public void finishInfluence(){
		///TODO: add end semaphore stuff her
		if(controllingTeam != owningTeam){
			if(_percControlled > 100f){
				_percControlled = 100f;
				currentState = TileState.normal;
				if(controllingTeam.teamNumber == 1)	audio.clip = sRef.InfluenceDoneLo;
				if(controllingTeam.teamNumber == 2) audio.clip = sRef.InfluenceDoneHi;


				if (!audio.isPlaying) {
					audio.volume = sRef.playerInfluenceDoneVolume;
					audio.Play (); //activate this if we want every tile influenced to trigger a sound, including the ones influenced by beacons
				} 

			}
			owningTeam = controllingTeam;
			Beacon localBeacon = GetComponentInChildren<Beacon>();
			Altar localAltar = GetComponentInChildren<Altar>();
			owningTeam.ScoreFromTile (getTileScore ());
			
			if(localAltar !=null){
				localAltar.setControl(owningTeam);
			}
			if(localBeacon!= null){
				localBeacon.setTeam (owningTeam);
				localBeacon.UpdateInfluencePatterns();
			}
			
			Reveal (sRef.influenceRevealRange);
			//TODO: possible huge performance hit here
			gm.tileSendMessage("setDistanceToHomeBase");
			//setDistanceToHomeBase();
			PS.startColor = controllingTeam.beaconColor;
			PS.Emit(100);


		}
		
	}
	public void clearInfluence(){
		_percControlled = 0;
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
				try { tile = GameManager.GameManagerInstance.tiles[_brdXPos + j, _brdYPos + i]; }
				catch { tile = null; }
				if (tile != null) {
					tile.GetComponent<BaseTile>().IsRevealed = true;
					if (tile.GetComponent<BaseTile>().beacon != null){
						tile.GetComponent<BaseTile>().beacon.transform.FindChild ("Arrow").GetComponent<MeshRenderer>().enabled = true;	
						tile.GetComponent<BaseTile>().beacon.transform.FindChild ("Arrow").GetComponent<MeshRenderer>().enabled = true;	
					}
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

	public int getDistanceToHomeBase(){
		if(owningTeam != null){
			List<AStarholder> As = 	BaseTile.aStarSearch(this,gm.getTeamBase(owningTeam),int.MaxValue, getLocalSameTeamTiles, owningTeam);
			return As.Count;
		}
		return 0;
	}

	public void setDistanceToHomeBase(){
		_distanceToHomeBase = getDistanceToHomeBase();
		if(_distanceToHomeBase ==0){
			qudPulsingOwnedLayer.renderer.enabled = false;
		}
		else{
			qudPulsingOwnedLayer.renderer.enabled = true;
		}

	}

	
	public float getTileScore(){
		List <AltarType> a = gm.getCapturedAltars(controllingTeam);
		
		if(a.Contains(AltarType.Khepru)){
			return sRef.valTileConvertScore * sRef.coefKhepru;
		}else{
			return sRef.valTileConvertScore;
		}
	}
		
	public bool tooCloseToBeacon() {
		for (int i = sRef.beaconNoBuildRange * -1; i <= sRef.beaconNoBuildRange; i++){
			for (int j = (sRef.beaconNoBuildRange - Mathf.Abs (i)) * -1; j <= sRef.beaconNoBuildRange - Mathf.Abs (i); j++) {
				GameObject tile;
				try { tile = GameManager.GameManagerInstance.tiles[_brdXPos + j, _brdYPos + i]; }
				catch { tile = null; }
				if (tile != null && 
					tile.GetComponent<BaseTile>().beacon != null && 
					tile.GetComponent<BaseTile>().beacon.GetComponent<Beacon>().currentState != BeaconState.BuildingBasic) 
				{
					if(this.beacon == null){
						qudNoBuildLayer.renderer.enabled = true;
					}
					return true;
				}
			}
		}
		return false;
	}

	public bool buildable(){
		if(getLocalAltar() != null){
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
		if(tooCloseToBeacon()){
			return false;
		}
		return true;
	}

	public float percentInfluenced(){
		return _percControlled;
	}

	public void LateUpdate() 
	{
		if(influenceThisFrame != 0f){
			influenceAdded[Time.frameCount % 5] = influenceThisFrame;
			influenceThisFrame = 0;
		}
	}

	public float getAverageInfluence(){
		return influenceThisFrame;
	}
	
	
	public void Reset(){
		this.setTileType(TileTypeEnum.regular);
	}
}

