using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class Player : MonoBehaviour {

	public TeamInfo team;
	private Vector2 _grdLocation;
	private Vector2 _positionOffset;	//Offsets position based on how far along move action is
	private PlayerState _currentState;
	public int PlayerNumber;
	public GameManager gm; 
	private float currentActionProgress;
	public Settings sRef;
	public DirectionEnum facing;
	private GameObject _prfbTower;
	private Tower towerInProgress;
	private int _vision = 5;

	private float _jiggleRange = 0.1f;			//Max distance from center of grid the player will jiggle
	
	private bool _pulsating;	//Set to false every Update function; Pulsate sets it to true; if false at end of update, resets scale and _expanding
	private bool _expanding;	//Used during pulsating
	private Vector3 _defaultScale = new Vector3 (0.5f, 0.5f, 1f);
	private float _maxScale = 0.9f;
	private float _minScale = 0.5f;
//	private float _expandRate = 0.007f;
//	private float _contractRate = 0.04f;
	private float _expandTime = 0.8f;
	private float _contractTime = 0.2f;
	private float pulsateProgress;
	private List<AltarType> altars;

	public AudioClip playerMove;
	public AudioClip influenceStart;
	public AudioClip towerBuilding;
	public AudioClip towerBuilt;

	public PlayerState currentState {
		get {
			return _currentState;
		}
	}

	/// <summary>
	/// Gets or sets the grd location.  Sets transform to corresponding center square of tile.  
	/// I'm not sure if I like that it does this, maybe we should break into seperate calls?  
	/// </summary>
	/// <value>Int Vector 2 of grid coordinates.  	The grd location.</value>
	public Vector2 grdLocation{
		get{
			return _grdLocation;
		}
		set{
			transform.position = GameManager.wrldPositionFromGrdPosition(value);
			
			_grdLocation = value;
		}
	}

	public int teamNumber{
		get{
			return team.teamNumber;
		}
	}

	// Use this for initialization
	void Start () {
		altars = new List<AltarType>();
		_currentState = PlayerState.standing;
		_prfbTower = (GameObject)Resources.Load("Prefabs/Tower");
		sRef = GameObject.Find ("Settings").GetComponent<Settings>();
		gm = GameObject.Find ("GameManager").GetComponent<GameManager>();
		RevealTiles ();
	}
	
	
	// Update is called once per frame
	void Update (){ 
		
		DirectionEnum? x = getStickDirection();
		BaseTile currentTile = gm.tiles[(int)grdLocation.x,(int)grdLocation.y].GetComponent<BaseTile>();
		bool buildButtonDown = getPlayerBuild();
		
		_pulsating = false;	//Pulsate () sets this to true; if false at the end of this method, reset scale and _expanding

		_positionOffset = new Vector2 (0,0);	//This can't possibly be the right way to do this - Josh
				
		switch( currentState){
			
			case PlayerState.standing:
			//If we are standing and we get an input, handle it.
				
				if(x.HasValue && !buildButtonDown){
					if(currentTile.GetDirection(x.Value) != null){
						BaseTile MovingInto = currentTile.GetDirection(x.Value);
						//Debug.Log(string.Format("x:{0}, y: {1}", MovingInto.brdXPos, MovingInto.brdYPos));
						float vpsRate = MovingInto.GetRate(this, gm) * sRef.vpsBaseMove;
						addProgressToAction(vpsRate);
						setDirection(x.Value);
						_currentState = PlayerState.moving;
						
					}
					else{
						setDirection(x.Value);
					}
					
					
				}
				
				//Rotating - doesn't fit with old comments or build button state diagram, but can hopefully be refactored later to fit better
				if (buildButtonDown && 
					currentTile.tower != null && 
					x.HasValue &&
					x != currentTile.tower.GetComponent<Tower>().facing && 
				    (currentTile.tower.GetComponent<Tower>().currentState == TowerState.Basic || 				//Making sure the tower is at least complete at basic level
				     currentTile.tower.GetComponent<Tower>().currentState == TowerState.BuildingAdvanced || 	//Is there a better way of doing this?
		 			 currentTile.tower.GetComponent<Tower>().currentState == TowerState.Advanced)) 
		 		{
					float vpsRate = sRef.vpsBaseRotate;
					addProgressToAction (vpsRate);
					setDirection(x.Value);
					_currentState = PlayerState.rotating;
				}
				
				//If tower
					//if stick
						//begin rotate build
					//else
						//if level 1
							// begin upgade
				//else 
					//if no control
						//Start  adding control
					//if other team
						//start removing 
					// if us 
						//start building tower				
				if( buildButtonDown){
					//NO TOWER HERE, GOTTA DO STUFF.
					//Check influence fist
					if(currentTile.controllingTeam !=null){
						
						if(currentTile.controllingTeam.teamNumber == team.teamNumber){
							
							if(currentTile.percControlled < 100f){
							Pulsate ();
								_currentState = PlayerState.influencing;
							}
							else if(currentTile.tower == null || currentTile.tower.GetComponent<Tower>().percActionComplete < 100f){
								Pulsate ();
								_currentState = PlayerState.building;
								float vpsBuildRate = sRef.vpsBaseBuild;
								addProgressToAction(vpsBuildRate);
								
								gm.PlaySFX(towerBuilding, 1.0f);
								GameObject towerBeingBuilt;
								if (currentTile.tower == null) { 
									towerBeingBuilt = (GameObject)GameObject.Instantiate(_prfbTower, new Vector3(0,0,0), Quaternion.identity);
								}
								else {
									towerBeingBuilt = currentTile.tower;
								}
								towerInProgress = towerBeingBuilt.GetComponent<Tower>();
								towerInProgress.startBuilding(currentTile.gameObject, this.gameObject, vpsBuildRate);
								towerInProgress.setDirection(facing);
							}
						}
						else{
							_currentState = PlayerState.influencing;
						}
					}
					else{
					Pulsate ();
						gm.PlaySFX(influenceStart, 1.0f);
						float vpsInfluenceRate = sRef.vpsBaseInfluence * getAltarInfluenceBoost();
						addProgressToAction(vpsInfluenceRate);
						_currentState = PlayerState.influencing;
						currentTile.startInfluence(currentActionProgress, team);
					}
				}
				
			break;
			
			//IF stick holds the same direction keep doing move
			//IF it changes, remove all progress (PLACEHOLDER, feel free to nuke)
			//if it completes, move to next tile, set state to standing
			case PlayerState.moving:
				
			
				if(x.HasValue && x.Value == facing){
					gm.PlaySFX(playerMove, 0.8f);
					BaseTile MovingInto = currentTile.GetComponent<BaseTile>().GetDirection(x.Value);
					float vpsRate = MovingInto.GetRate(this, gm) * sRef.vpsBaseMove *getAltarSpeedBoost();
					addProgressToAction(vpsRate);
					
					if(currentActionProgress > sRef.baseRequired){
						DoMove(MovingInto);
						currentActionProgress = 0f;
						_currentState = PlayerState.standing;	
					}

					else{ 
					
					//Move avatar according to how far along the action is

					float offset = currentActionProgress / sRef.baseRequired;
					offset = Mathf.Pow (10, offset);
					offset = Mathf.Log10 (offset);
					
						switch (facing) {
							
							case DirectionEnum.East: 
								_positionOffset = new Vector2 (offset, 0);
								break;
							
							case DirectionEnum.West:
								_positionOffset = new Vector2 (-1 * offset, 0);
								break;

							case DirectionEnum.North:
								_positionOffset = new Vector2 (0, offset);
								break;
										
							case DirectionEnum.South:
								_positionOffset = new Vector2 (0, -1 * offset);
								break;
								
						}
						
					}
					
				}
				else{
					gm.StopSFX();
					_currentState= PlayerState.standing; 
					currentActionProgress = 0f;
				}
			break;	
			
			case PlayerState.building:
				if(buildButtonDown && currentTile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
				//	Jiggle ();	//Gotta jiggle
					Pulsate (); 
					
					//Debug.Log ("In Build");
					if(currentTile.controllingTeam != null){
//						Debug.Log ("current Team");
						if(currentTile.controllingTeam.teamNumber == team.teamNumber){
							//Check for a tower in progress and start building!s
							if(towerInProgress != null){
								Debug.Log("attempting to build");
								float vpsBuildRate = sRef.vpsBaseBuild * getAltarBuildBoost ();
								towerInProgress.addBuildingProgress(vpsBuildRate);
								if(x.HasValue){
									setDirection(x.Value);
									towerInProgress.setDirection(x.Value);
								}
								if(towerInProgress.percActionComplete > 100f){
								
									Debug.Log("Finished?");
									gm.StopSFX();
									gm.PlaySFX(towerBuilt, 1.0f);
									towerInProgress.finishAction();
									_currentState = PlayerState.standing;
									currentActionProgress = 0f;
								}
							}
						}
						else{
							
						}
					}
					else
					{
						gm.StopSFX();
						float vpsInfluenceRate = sRef.vpsBaseInfluence;
						addProgressToAction(vpsInfluenceRate);
					}
					
				}
				else{
					gm.StopSFX();
					_currentState =  PlayerState.standing;
					
					//TODO: figure out what happens when we abandon the tile
				}
			break;
			
			case PlayerState.influencing:
				if(buildButtonDown && currentTile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
			//		Jiggle ();	//Gotta jiggle
					Pulsate ();
					gm.PlaySFX(influenceStart, 1.0f);
					if(currentTile.controllingTeam != null){
						if(currentTile.controllingTeam.teamNumber  == teamNumber)
						{
							Debug.Log("Adding Influence");
							float test = currentTile.addInfluenceReturnOverflow( sRef.vpsBaseInfluence * getAltarInfluenceBoost() * Time.deltaTime);
							if(test > 0){
								_currentState = PlayerState.standing;
							}
						}
						else{
							float test = currentTile.subTractInfluence(  sRef.vpsBaseInfluence * getAltarInfluenceBoost() * Time.deltaTime, team);
							if(test > 0f){
								currentTile.addInfluenceReturnOverflow(test);
							}
						}
						/*
						float modifier = (currentTile.controllingTeam.teamNumber  == teamNumber) ? 1 : -1;
						float vpsInfluenceRate = sRef.vpsBaseInfluence * modifier * getAltarInfluenceBoost();
						addProgressToAction(vpsInfluenceRate);
						currentTile.addProgressToInfluence(vpsInfluenceRate);
						if(currentTile.percControlled >= 100f){
							///Shoudl this be here or in the BaseTileObject?
							currentTile.finishInfluence();
							_currentState = PlayerState.standing;
							currentActionProgress = 0;
						}
						
						if(currentTile.percControlled <= 0f){
							currentTile.clearInfluence();
							currentActionProgress = 0;
						}*/
						
						if (x.HasValue) { 
							setDirection (x.Value);
						}
					
						
					} else{
					///TODO catch fully influenced Tile!
					}
				}
				else{
				///TODO: add reset to tile in case of change
					//need to reset currenttile to previousState
					_currentState = PlayerState.standing;
					gm.StopSFX();
				}	
			break;
			
			case PlayerState.rotating: 
			
				if (buildButtonDown) {
				
					Pulsate ();
					
					float vpsRotateRate = sRef.vpsBaseRotate;
					addProgressToAction (vpsRotateRate);
					Tower tower = currentTile.tower.GetComponent<Tower>();
					tower.addRotateProgress (vpsRotateRate);
					
					if (tower.percRotateComplete >= 100f) {
	
						currentActionProgress = 0;
						tower.Rotate (facing);
						tower.percRotateComplete = 0f;
						_currentState = PlayerState.standing;
						gm.StopSFX ();
					
					}
					
				}
					
				else {
				
					currentActionProgress = 0;
					currentTile.tower.GetComponent<Tower>().percRotateComplete = 0f;
					_currentState = PlayerState.standing;
					gm.StopSFX();
				
				}
			
			break;
			
		}
		
		//Set position based on offset
		transform.position = new Vector3 
			(GameManager.wrldPositionFromGrdPosition(grdLocation).x + _positionOffset.x / 2,
			 GameManager.wrldPositionFromGrdPosition(grdLocation).y + _positionOffset.y / 2, -1);
			
		//If not pulsating, reset scale and _expanding
		if (!_pulsating) {
			transform.localScale = new Vector3 (_defaultScale.x, _defaultScale.y, _defaultScale.z);
			_expanding = true;
			pulsateProgress = 0f;
		}
						
	}
	
	public void DoMove(BaseTile MoveTo){
		grdLocation = new Vector2(MoveTo.brdXPos, MoveTo.brdYPos);
		RevealTiles ();
	}
	
	/// <summary>;
	/// Change teams, part of the setup process
	/// </summary>
	/// <param name="t">T.</param>
	public void SetTeam(TeamInfo t){
		team = t;
		GetComponent<MeshRenderer> ().material.color = team.teamColor;
		MoveToTeamStart ();
	}

/// <summary>
/// Moves to team start.
/// </summary>
	public void MoveToTeamStart(){
		if (team != null) {
			grdLocation = team.startingLocation;		
		}
		_positionOffset = new Vector2 (0,0);
	}

	/// <summary>
	/// Gets the correct X axis of the stick based on player number
	/// </summary>
	/// <returns>The player X axis.</returns>
	private float getPlayerXAxis(){
		//Input.GetAxis("HorizontalPlayer"+PlayerNumber);
		return Input.GetAxis("HorizontalPlayer"+PlayerNumber);	
	}
				
	/// <summary>
	/// Gets the correct Y axis of the stick based on player number
	/// </summary>
	/// <returns>The player X axis.</returns>
	private float getPlayerYAxis(){
		//Debug.Log(Input.GetAxis("VerticalPlayer"+PlayerNumber));
		return Input.GetAxis("VerticalPlayer"+PlayerNumber);	
	}

	/// <summary>
	/// Gets the Build aixs based on player number
	/// </summary>
	/// <returns>The player X axis.</returns>
	private bool getPlayerBuild(){
		return Input.GetButton("BuildPlayer"+PlayerNumber);	
	}

	/// <summary>
	/// return which direction the stick is being held in
	/// </summary>
	/// <returns>The stick direction.</returns>
	private DirectionEnum? getStickDirection(){
		float xStick = getPlayerXAxis();
		float yStick = getPlayerYAxis();

		if ((Mathf.Abs (xStick) + Mathf.Abs (yStick)) > 0) {
			if (Mathf.Abs (xStick) > Mathf.Abs (yStick)) {
					if (xStick > 0) {
							return DirectionEnum.East;
					} else {
							return DirectionEnum.West;
					}
			} else {
					if (yStick > 0) {
							return DirectionEnum.North;
					} else {
							return DirectionEnum.South;
				} 
			}
		}else {
			return null;
		}
	}
	
	
	private float getAngleForDir(DirectionEnum N){
		switch (N){
			case DirectionEnum.North:
			return 0f;
			
		case DirectionEnum.East:
			return 90f;
			
		case DirectionEnum.South:
			return 180f;
			
		case DirectionEnum.West:
			return 270f;
			
		}
		return 0;
	} 
	private void setDirection(DirectionEnum N){
		
		float rotAngle = getAngleForDir(N);
		float currentRotAngle = getAngleForDir(facing);
		
		facing = N;
		transform.RotateAround(transform.position, new Vector3(0,0,1), currentRotAngle);
		transform.RotateAround(transform.position, new Vector3(0,0,-1), rotAngle);
	}
/// <summary>
/// 
/// </summary>
/// <param name="rate"></param>
	private void addProgressToAction(float rate){
		currentActionProgress+= rate*Time.deltaTime;
		
	}
	
	/// <summary>
	/// Reveals nearby tiles.
	/// </summary>
	public void RevealTiles () {
		
		for (int i = _vision * -1; i <= _vision; i++) {
			for (int j = (_vision - Mathf.Abs (i)) * -1; j <= _vision - Mathf.Abs (i); j++) {
				GameObject tile;
				try { tile = gm.tiles[(int)_grdLocation.x + j, (int)_grdLocation.y + i]; }
					catch { break; }
				if (tile != null) {
					tile.GetComponent<BaseTile>().IsRevealed = true;
				}		
			}
		}
	}
	
	/// <summary>
	/// Jiggle this instance.
	/// </summary>
	private void Jiggle () {
	
//		_positionOffset = new Vector2 (Random.Range (-1 * _jiggleRange, _jiggleRange), Random.Range(-1 * _jiggleRange, _jiggleRange));
		_positionOffset = new Vector2 (Random.Range (-1 * _jiggleRange, _jiggleRange), 0);
		
	}
	
	/// <summary>
	/// Pulsate this instance.
	/// </summary>
	private void Pulsate () {
	
		_pulsating = true;
	
		if (_expanding) {
		
			float expandRate = sRef.baseRequired / _expandTime;
			pulsateProgress += expandRate * Time.deltaTime;
			
			
			float expandAmount = (_maxScale - _minScale) * (expandRate * Time.deltaTime / sRef.baseRequired);
			transform.localScale = new Vector3 (transform.localScale.x + expandAmount, transform.localScale.y + expandAmount, 1);

			if (pulsateProgress >= sRef.baseRequired) {
				transform.localScale = new Vector3 (_maxScale, _maxScale, 1);			
				_expanding = false;
				pulsateProgress = 0f;
			}			
			
			/**	transform.localScale = new Vector3 (transform.localScale.x + _expandRate, transform.localScale.y + _expandRate, 1);		
			
			if (transform.localScale.x >= _maxScale) {
				transform.localScale = new Vector3 (_maxScale, _maxScale, 1);						
				_expanding = false;
			}
			*/
			
		}
		
		else {
	
			float contractRate = sRef.baseRequired / _contractTime;
			pulsateProgress += contractRate * Time.deltaTime;
	
			float contractAmount = (_maxScale - _minScale) * (contractRate * Time.deltaTime / sRef.baseRequired);
			transform.localScale = new Vector3 (transform.localScale.x - contractAmount, transform.localScale.y - contractAmount, 1);			
			
			if (pulsateProgress >= sRef.baseRequired) {
				transform.localScale = new Vector3 (_minScale, _minScale, 1);			
				_expanding = true;	
				pulsateProgress = 0f;
			}			
			
/**			transform.localScale = new Vector3 (transform.localScale.x - _contractRate, transform.localScale.y - _contractRate, 1);

			if (transform.localScale.x <= _minScale) {
				transform.localScale = new Vector3 (_minScale, _minScale, 1);		
				_expanding = true;
			}
			*/
		}		
	}
	
	/// <summary>
	/// ONLY used for debug for player
	/// </summary>
	private void OnGUI(){
	
		if(sRef.debugMode){
			GUI.Box (new Rect (10+200*(PlayerNumber-1),10,200,90), string.Format("Player {0}\r\nState: {1}\r\npercentcomplete{2}",PlayerNumber, currentState,currentActionProgress));	
		}
	}
	
	private float getAltarSpeedBoost(){
		List<AltarType> a = gm.getNetworkedAltars(team);
		if(a.Contains(AltarType.Choyutzol)){
			return 2f;
		}else{
			return 1f;
		}
	}
	
	private float getAltarBuildBoost(){
		List <AltarType> a = gm.getNetworkedAltars(team);
		if(a.Contains(AltarType.Tikumose)){
			return 2f;
		}else{
			return 1f;
		}
	}
	
	private float getAltarInfluenceBoost(){
		List <AltarType> a = gm.getNetworkedAltars(team);
		if(a.Contains(AltarType.Khepru)){
			return 2f;
		}else{
			return 1f;
		}
	}
	
}
