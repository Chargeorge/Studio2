using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;

public class Player : MonoBehaviour {

	public TeamInfo team;
	private Vector2 _grdLocation;
	private Vector2 _positionOffset;	//Offsets position based on how far along move action is
	private PlayerState _currentState;
	public int PlayerNumber;
	public GameManager gm;
	private InputDevice _controller;
	public float currentActionProgress;
	private float previousActionProgress;
	public float previousFrameProgress;
	public Settings sRef;
	public DirectionEnum facing;
	private GameObject _prfbBeacon;
	private Beacon beaconInProgress;
	private int _vision = 3;
	private bool _invalidAction;
	private int _invalidActionFrames = 0;
	private float _jiggleRange = 0.1f;			//Max distance from center of grid the player will jiggle
	private float _lastActionProgress;
	
	private bool _pulsating;	//Set to false every Update function; Pulsate sets it to true; if false at end of update, resets scale and _expanding
	private bool _expanding;	//Used during pulsating
	private Vector3 _defaultScale = new Vector3 (0.5f, 0.5f, 1f);
//	private float _expandRate = 0.007f;
//	private float _contractRate = 0.04f;
	private float _expandTime = 0.8f;
	private float _contractTime = 0.2f;
	private float pulsateProgress;
	
	private List<AltarType> altars;

	//bool isPlaying = false;
	//public AudioClip playerMove;
	//public AudioClip influenceStart;
	//public AudioClip influenceDone;
	//public AudioClip invalid_Input;
	
	private BaseTile _currentTile;

	public BaseTile currentTile {
		get {
			return _currentTile;
		}
	}
	
	public AudioSource audioSourceMove;
	public AudioSource audioSourceMoveEnemyTerrain;
	public AudioSource audioSourceInvalid;
	public AudioSource audioSourceInfluenceStart;
	public AudioSource audioSourceInfluenceDone;

	public Texture winTexture;
	
	public Vector3 teleportTarget;
	
	public Vector2 moveVector;

	public PlayerState currentState {
		get {
			return _currentState;
		}
	}

	private GameObject qudProgessCircle;
	private GameObject qudActionableGlow;
	private Hashtable jiggleHash;
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
			transform.parent.position = GameManager.wrldPositionFromGrdPosition(value);
			
			_grdLocation = value;
		}
	}

	public int teamNumber{
		get{
			return team.teamNumber;
		}
	}
	
	int actionProgressTicker;
	float[] actionProgress;

	
	// Use this for initialization
	void Start () {
		actionProgressTicker = 0;
		actionProgress = new float[3];
		altars = new List<AltarType>();
		_currentState = PlayerState.standing;
		_prfbBeacon = (GameObject)Resources.Load("Prefabs/Beacon");
		sRef = Settings.SettingsInstance;
		gm = GameManager.GameManagerInstance;
		qudProgessCircle = transform.parent.FindChild("ActionTimer").gameObject;
		qudProgessCircle.renderer.material.color = team.beaconColor;
		qudActionableGlow = transform.parent.FindChild("ActionableGlow").gameObject;
		jiggleHash = new Hashtable();
		jiggleHash.Add ("x", _jiggleRange);
		jiggleHash.Add ("y", _jiggleRange);
		jiggleHash.Add("time", .02f);
		jiggleHash.Add("oncomplete", "resetPos");
		jiggleHash.Add("oncompletetarget", gameObject);
		
		
		if(PlayerNumber == 1){
			winTexture = gm.winTexture1;
			audioSourceMove.clip = Resources.Load("SFX/Player_Moving_Lo") as AudioClip;
			audioSourceInfluenceDone.clip = Resources.Load("SFX/Influence_Done_Lo") as AudioClip;
			audioSourceInfluenceDone.Play();
			audioSourceMove.Play();
		} else {
			winTexture = gm.winTexture2;
			audioSourceMove.clip = Resources.Load("SFX/Player_Moving_Hi") as AudioClip;
			audioSourceInfluenceDone.clip = Resources.Load("SFX/Influence_Done_Hi") as AudioClip;
			audioSourceInfluenceDone.Play();
			audioSourceMove.Play();
		}
	

	}
	
	// Update is called once per frame
	void Update (){ 
		_controller = (InputManager.Devices.Count > PlayerNumber-1) ? InputManager.Devices[PlayerNumber-1] : null;
		_currentTile =  gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
		
		if(_controller == null){
			Debug.Log ("InputDevice");
		}
		if(gm.currentState == GameState.playing){
			if(_invalidAction) {
				_invalidActionFrames++;
			}
			else{
				_invalidActionFrames = 0;
			}
			_invalidAction = false;
			previousActionProgress = currentActionProgress;	
			DirectionEnum? x = getStickDirection();
			//BaseTile currentTile = gm.tiles[(int)grdLocation.x,(int)grdLocation.y].GetComponent<BaseTile>();	//Not used with free movement
	
			
			bool buildButtonDown = getPlayerBuild();
			if (!buildButtonDown) { audioSourceInvalid.Stop (); }
			//if(x.HasValue) Debug.Log(x.Value);
			_pulsating = false;	//Pulsate () sets this to true; if false at the end of this method, reset scale and _expanding
			
			if (OpponentsOnTile ().Count == 0) {
				currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = true;
				currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().material.color = team.highlightColor;
			}
			
			else {
				currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = true;
				currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().material.color = sRef.sharedTileColor;
			}
			
			//_positionOffset = new Vector2 (0,0);	//This can't possibly be the right way to do this - Josh
			if(gm.currentState == GameState.playing){
				switch(currentState){
					
					case PlayerState.teleporting:
		/**				if (transform.parent.position == teleportTarget) {
							_currentState = PlayerState.standing;
						}
						else {
							transform.parent.position = Vector2.Lerp(transform.parent.position, teleportTarget, sRef.teleportRate);
						}
		*/				
						Vector3 newPos = Vector2.Lerp(transform.parent.position, teleportTarget, sRef.teleportRate);
						newPos.z = transform.parent.position.z;
						transform.parent.position = newPos;
						
						BaseTile newTile = gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
						if (newTile != currentTile) {
//							Debug.Log ("Getting New Tile");
							//currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
							doNewTile(currentTile, newTile);
						}
		//				if(currentTile == team.goGetHomeTile().GetComponent<BaseTile>()){
						if(closeEnoughToTarget(newPos, teleportTarget, sRef.closeEnoughDistanceTeleport)){
					
		//					Vector3 homePos = currentTile.transform.parent.position;
		//					homePos.z = transform.parent.position.z;
		//					transform.parent.position = homePos;
							_currentState = PlayerState.standing;
						}
						
					break;
					
					case PlayerState.standing:
					
						audioLerp (audioSourceMove, 0.0f, sRef.moveVolumeLerpRate);
						audioLerp (audioSourceMoveEnemyTerrain, 0.0f, sRef.moveVolumeLerpRate);
						if (audioSourceInfluenceStart.volume > 0.01f) { audioLerp (audioSourceInfluenceStart, 0.0f, sRef.playerInfluenceStartVolumeLerpRate); 
							} else { audioSourceInfluenceStart.Stop (); }
					
					//If we are standing and we get an input, handle it.
		//			Debug.Log (string.Format("Player number {0}, buld button down: {1}", PlayerNumber, buildButtonDown));
						if (!Pause.paused) qudProgessCircle.renderer.enabled = false;
						if(x.HasValue && !buildButtonDown) {
							
							setDirection(x.Value);	//Still need a 4-directional facing for building/rotating beacons
							moveVector = new Vector2 (getPlayerXAxis(), getPlayerYAxis());
							_currentState = PlayerState.moving;
							
							/**if(currentTile.GetDirection(x.Value) != null){
								BaseTile MovingInto = currentTile.GetDirection(x.Value);
								//Debug.Log(string.Format("x:{0}, y: {1}", MovingInto.brdXPos, MovingInto.brdYPos));
								float vpsRate = MovingInto.GetRate(this) * sRef.vpsBaseMove;
								addProgressToAction(vpsRate);
								setDirection(x.Value);
								_currentState = PlayerState.moving;
									
							}
							else{
								setDirection(x.Value);
							}
							*/						
						}
						
						if (x.HasValue) {
							setDirection (x.Value);
						}
						
						//Rotating
						if (buildButtonDown && 
					    	currentTile.owningTeam == team &&
					    	currentTile.beacon != null && 
							facing != currentTile.beacon.GetComponent<Beacon>().facing && 
						    (currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.Basic || 			//Making sure the beacon is at least complete at basic level
						     currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.BuildingAdvanced || 	//Is there a better way of doing this?
				 			 currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.Advanced)) 
				 		{
				 			//Debug.Log("in rotate");
							if (audioSourceInfluenceStart.volume > 0.01f) { audioLerp (audioSourceInfluenceStart, 0.0f, sRef.playerInfluenceStartVolumeLerpRate);
								} else { audioSourceInfluenceStart.Stop (); }
						
				 			/**
				 			//Tikumose: Instant rotation
				 			if (gm.getCapturedAltars(team).Contains (AltarType.Tikumose) && currentTile.beacon.GetComponent<Beacon>().controllingTeam == team) {
								currentActionProgress = 0;
								currentTile.beacon.GetComponent<Beacon>().Rotate (facing);
								currentTile.beacon.GetComponent<Beacon>().percRotateComplete = 0f;
							}
							else {
							*/
								float vpsRate = sRef.vpsBaseRotate;
								addProgressToAction (vpsRate);
								
		//						setDirection(x.Value);
								
		
								currentTile.beacon.GetComponent<Beacon>().startRotating (facing);
								_currentState = PlayerState.rotating;
								
		//					}
						}
						
						//Upgrading
						else if (buildButtonDown && 
					    	currentTile.owningTeam == team &&
					    	currentTile.beacon != null &&
							facing == currentTile.beacon.GetComponent<Beacon>().facing &&
							(currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.Basic ||
							 currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.BuildingAdvanced)) 
						{
							//Debug.Log("in upgrade");
							if (audioSourceInfluenceStart.volume > 0.01f) { audioLerp (audioSourceInfluenceStart, 0.0f, sRef.playerInfluenceStartVolumeLerpRate);
								} else { audioSourceInfluenceStart.Stop (); }
								
							float vpsRate = sRef.vpsBaseUpgrade;
							addProgressToAction (vpsRate);
							_currentState = PlayerState.upgrading;
							currentTile.beacon.GetComponent<Beacon>().startUpgrading();
							currentTile.beacon.GetComponent<Beacon>().losingUpgradeProgress = false;
						} else {
						
						
						//If beacon
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
								//start building beacon				
							if (buildButtonDown){
								
								List<Player> opponentsOnSameTile = OpponentsOnTile ();
								if (opponentsOnSameTile.Count != 0) {
									foreach (Player p in opponentsOnSameTile) { 
										if (p.IsActing ()) {
											teleportTarget = team.startingLocation;
											teleportTarget = Vector2.Lerp(new Vector2 (transform.parent.position.x, transform.position.y), team.startingLocation, 0.25f);
											//teleportTarget = (transform.parent.position + team.goGetHomeTile().transform.parent.position) / 4.0f;
											_currentState = PlayerState.teleporting;
										}
									}
								}
								
								if (_currentState != PlayerState.teleporting) {
								
									moveTowardCenterOfTile (currentTile);
									//NO BEACON HERE, GOTTA DO STUFF.
									//Check influence first
									if(currentTile.controllingTeam !=null){
										//Debug.Log ("IN weird place");
										if(currentTile.controllingTeam.teamNumber == team.teamNumber){
											if(currentTile.percControlled < 100f){
											Pulsate ();
												_currentState = PlayerState.influencing;
												//Debug.Log("Set Influencing");
											}
																	
											//DO Tile Control 
											else if(currentTile.getLocalAltar()!=null ){
												
												currentTile.getLocalAltar().doCapture(team);
												
											}
				
											//Building
											else if((currentTile.beacon == null || 
													//currentTile.beacon.GetComponent<Beacon>().percBuildComplete < 100f &&
													currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.BuildingBasic) && 
													currentTile.buildable ())
											{
												Pulsate ();
												audioSourceInfluenceDone.volume = sRef.playerInfluenceDoneVolume;
												//audioSourceInfluenceDone.Play ();
												_currentState = PlayerState.building;
												
												float vpsBuildRate = sRef.vpsBaseBuild * getAltarBuildBoost ();
												addProgressToAction(vpsBuildRate);
				
												GameObject beaconBeingBuilt;
												
												if (currentTile.beacon == null) { 
													beaconBeingBuilt = (GameObject)GameObject.Instantiate(_prfbBeacon, new Vector3(0,0,0), Quaternion.identity);
													beaconBeingBuilt.name = "Beacon for" + team.teamNumber;
													
												}
												else {
													beaconBeingBuilt = currentTile.beacon;
												}
												
												beaconInProgress = beaconBeingBuilt.GetComponent<Beacon>();
												beaconInProgress.startBuilding(currentTile.gameObject, this.gameObject, vpsBuildRate);
												beaconInProgress.setDirection(facing);
												beaconInProgress.selfDestructing = false;
												_invalidAction = true;
												
											}
											else{
												if ((!currentTile.buildable() && currentTile.beacon == null) || 
													(currentTile.beacon != null && //If there's a beacon...
														(currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.Advanced && 
														currentTile.beacon.GetComponent<Beacon>().facing == facing)	//...and it's advanced and you're not gonna rotate it...
													))
													
												{
													_invalidAction = true;
												//												if(!audioSourceInvalid.isPlaying){ 
	//													playInvalid (0.3f);										
	//												}
												}
											}
										} else{
											_currentState = PlayerState.influencing;
										}
									}
									else {
										Pulsate ();
										currentActionProgress = 0f;
										float vpsInfluenceRate = sRef.vpsBasePlayerInfluence * getPlayerInfluenceBoost();
										addProgressToAction(vpsInfluenceRate);
										_currentState = PlayerState.influencing;
										currentTile.startInfluence(currentActionProgress, team);
									}
								}
							}
							
							//We chillin
							else {
								moveTowardCenterOfTile (currentTile);
							}
						}
					break;
					
					//IF stick holds the same direction keep doing move
					//IF it changes, remove all progress (PLACEHOLDER, feel free to nuke)
					//if it completes, move to next tile, set state to standing
					case PlayerState.moving:
						if(currentTile.owningTeam != null && currentTile.owningTeam.teamNumber != teamNumber){
							audioSourceMoveEnemyTerrain.clip = Resources.Load("SFX/Moving_EnemyTerrain") as AudioClip;
							audioLerp (audioSourceMoveEnemyTerrain, sRef.moveEnemyVolume, sRef.moveVolumeLerpRate);
							if(!audioSourceMoveEnemyTerrain.isPlaying){
								audioSourceMoveEnemyTerrain.Play();
							}
						} 
					if(currentTile.owningTeam == null || currentTile.owningTeam.teamNumber == teamNumber) {
						if(PlayerNumber == 1) audioSourceMove.clip = Resources.Load ("SFX/Player_Moving_Lo") as AudioClip;
						if(PlayerNumber == 2) audioSourceMove.clip = Resources.Load ("SFX/Player_Moving_Hi") as AudioClip;
						audioLerp (audioSourceMoveEnemyTerrain, 0.0f, sRef.moveVolumeLerpRate);
						audioLerp (audioSourceMove, sRef.moveVolume, sRef.moveVolumeLerpRate);
						if(!audioSourceMove.isPlaying){
							audioSourceMove.Play();
						}
					}
		
						if (audioSourceInfluenceStart.volume > 0.01f) { audioLerp (audioSourceInfluenceStart, 0.0f, sRef.playerInfluenceStartVolumeLerpRate);
							} else { audioSourceInfluenceStart.Stop (); }
						//audioLerp (audioSourceMove, sRef.moveVolume, sRef.moveVolumeLerpRate);
			
						qudProgessCircle.renderer.enabled = false;
						currentTile.Reveal (_vision);
					
						//This lets you hit build button while moving to start doing stuff
						//It also freezes you if you can't do anything (ex. are on genesis or upgraded beacon)
						if (buildButtonDown) {
							if (x.HasValue) {
								setDirection (x.Value);
							}
							_currentState = PlayerState.standing;
						}
						
						else { 
							if (x.HasValue) {
								setDirection(x.Value);	//Still need a 4-directional facing for building/rotating beacons
								
							#region movement
								
								//Add acceleration
								Vector2 vectorToAdd = new Vector2 (getPlayerXAxis(), getPlayerYAxis()) * sRef.playerAccelRate;
								moveVector += vectorToAdd;
								
								//Apply friction
								moveVector *= (1-sRef.playerFriction);
														
								//Make sure we're not going too fast
								if (Mathf.Abs (moveVector.x) > sRef.playerMaxSpeed) moveVector.x = sRef.playerMaxSpeed * Mathf.Sign (moveVector.x);
								if (Mathf.Abs (moveVector.y) > sRef.playerMaxSpeed) moveVector.y = sRef.playerMaxSpeed * Mathf.Sign (moveVector.y);
								
								//Go to new position				
								Vector3 posToCheck = new Vector3 (
								transform.parent.position.x + moveVector.x * sRef.vpsBaseFreeMoveSpeed * getTileSpeedBoost(currentTile) * getAltarSpeedBoost() * Time.deltaTime,
								transform.parent.position.y + moveVector.y * sRef.vpsBaseFreeMoveSpeed * getTileSpeedBoost(currentTile) * getAltarSpeedBoost() * Time.deltaTime, 
								transform.parent.position.z);
								
								/*
								Vector3 posToCheck = new Vector3 (
								transform.parent.position.x + getPlayerXAxis() * sRef.vpsBaseFreeMoveSpeed * getTileSpeedBoost(currentTile) * getAltarSpeedBoost() * Time.deltaTime, 
								transform.parent.position.y + getPlayerYAxis() * sRef.vpsBaseFreeMoveSpeed * getTileSpeedBoost(currentTile) * getAltarSpeedBoost() * Time.deltaTime, 
								transform.parent.position.z);
								*/
								
								//posToCheck is illegal, so instead, see how far you can move horizontally and vertically
								//Welcome to my nightmare
								if (outOfBounds (posToCheck) || 
									(onWater (posToCheck) && !gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) && currentTile.currentType != TileTypeEnum.water)) 
								{					
									//First, figure out where the posToCheck's tile is in relation to our own
									int xRel = (int) Mathf.Floor (transform.parent.position.x + 0.5f) - (int) Mathf.Floor (posToCheck.x + 0.5f);
									int yRel = (int) Mathf.Floor (transform.parent.position.y + 0.5f) - (int) Mathf.Floor (posToCheck.y + 0.5f);
									string dir = "";
									if (yRel == 1) 			dir += "south";
									else if (yRel == -1) 	dir += "north";
									if (xRel == 1) 			dir += "west";
									else if (xRel == -1)	dir += "east"; 
									//If posToCheck's tile is north, south, east, or west, just put us on the border and move horizontally/vertically as appropriate
									Vector2 safePos = new Vector2 (transform.parent.position.x, transform.parent.position.y);
									switch (dir) {
										case "north":
											safePos = new Vector2 (posToCheck.x, currentTile.transform.position.y + 0.499f);
											break;
										case "south":
											safePos = new Vector2 (posToCheck.x, currentTile.transform.position.y - 0.499f);
											break;
										case "east":
											safePos = new Vector2 (currentTile.transform.position.x + 0.499f, posToCheck.y);
											break;
										case "west":
											safePos = new Vector2 (currentTile.transform.position.x - 0.499f, posToCheck.y);
											break;
										case "northeast":
											if (Mathf.Abs (transform.position.x - currentTile.transform.position.x) > 
												Mathf.Abs (transform.position.y - currentTile.transform.position.y)) {
												
												//Try to move east, then north if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
												if (!outOfBounds(newPosToCheck) && 
										            (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
										         		safePos = newPosToCheck;
										    	}
										    	else {
										    		newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
													if (!outOfBounds(newPosToCheck) && 
														(!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x + 0.499f, currentTile.transform.position.y + 0.499f);
													}
										    	}
										    }
											else {
												//Try to move north, then east if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
												if (!outOfBounds(newPosToCheck) && 
												    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
												}
												else {
													newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
													if (!outOfBounds(newPosToCheck) && 
													    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
														safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x + 0.499f, currentTile.transform.position.y + 0.499f);
													}
												}						  
											}
											break;
										case "northwest":
											if (Mathf.Abs (transform.position.x - currentTile.transform.position.x) > 
											    Mathf.Abs (transform.position.y - currentTile.transform.position.y)) {
												
												//Try to move west, then north if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
												if (!outOfBounds(newPosToCheck) && 
												    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
												}
												else {
													newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
													if (!outOfBounds(newPosToCheck) && 
													    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
														safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x - 0.499f, currentTile.transform.position.y + 0.499f);
													}
												}
											}
											else {
												//Try to move north, then west if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
												if (!outOfBounds(newPosToCheck) && 
												    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
												}
												else {
													newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
													if (!outOfBounds(newPosToCheck) && 
													    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
														safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x - 0.499f, currentTile.transform.position.y + 0.499f);
													}
												}						  
											}
											break;
										case "southeast":
											if (Mathf.Abs (transform.position.x - currentTile.transform.position.x) > 
											    Mathf.Abs (transform.position.y - currentTile.transform.position.y)) {
												
												//Try to move east, then south if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
												if (!outOfBounds(newPosToCheck) && 
												    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
												}
												else {
													newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
													if (!outOfBounds(newPosToCheck) && 
													    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
														safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x + 0.499f, currentTile.transform.position.y - 0.499f);
													}
												}
											}
											else {
												//Try to move south, then east if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
												if (!outOfBounds(newPosToCheck) && 
												    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
												}
												else {
													newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
													if (!outOfBounds(newPosToCheck) && 
													    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
														safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x + 0.499f, currentTile.transform.position.y - 0.499f);
													}
												}						  
											}
											break;
										case "southwest":
											if (Mathf.Abs (transform.position.x - currentTile.transform.position.x) > 
											    Mathf.Abs (transform.position.y - currentTile.transform.position.y)) {
												
												//Try to move west, then south if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
												if (!outOfBounds(newPosToCheck) && 
												    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
												}
												else {
													newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
													if (!outOfBounds(newPosToCheck) && 
													    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
														safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x - 0.499f, currentTile.transform.position.y - 0.499f);
													}
												}
											}
											else {
												//Try to move south, then west if that fails, then put in corner if both fail
												Vector2 newPosToCheck = new Vector2 (transform.position.x, posToCheck.y);
												if (!outOfBounds(newPosToCheck) && 
												    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
													safePos = newPosToCheck;
												}
												else {
													newPosToCheck = new Vector2 (posToCheck.x, transform.position.y);
													if (!outOfBounds(newPosToCheck) && 
													    (!onWater(newPosToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) {
														safePos = newPosToCheck;
													}
													else {
														safePos = new Vector2 (currentTile.transform.position.x - 0.499f, currentTile.transform.position.y - 0.499f);
													}
												}						  
											}
											break; 
									}
									
									//PlaySFX(playerMove, 0.2f);
									transform.parent.position = new Vector3 (safePos.x, safePos.y, transform.parent.position.z);
									BaseTile thisTile = gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
									if (thisTile != currentTile) {
										//currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
										doNewTile(currentTile, thisTile);
									}
								} 
								
								if (!outOfBounds(posToCheck) && 
									//!tooCloseToOpponent(posToCheck) &&
									(!onWater(posToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) 
								{	//Valid move
		
									//PlaySFX(playerMove, 0.2f);
									transform.parent.position = posToCheck;
									BaseTile thisTile = gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
									if (thisTile != currentTile) {
									//	Debug.Log ("Getting New Tile moving");
										//currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
										doNewTile(currentTile, thisTile);
									}
								}
							}
							
							#endregion
							
							/**
							if(x.HasValue && x.Value == facing){
								gm.PlaySFX(playerMove, 0.8f);
								BaseTile MovingInto = currentTile.GetComponent<BaseTile>().GetDirection(x.Value);
								float vpsRate = MovingInto.GetRate(this) * sRef.vpsBaseMove *getAltarSpeedBoost();
								addProgressToAction(vpsRate);
								
								if(currentActionProgress > 100f){
									DoMove(MovingInto);
									currentActionProgress = 0f;
									_currentState = PlayerState.standing;	
								}
			
								else{ 
								
								//Move avatar according to how far along the action is
			
								float offset = currentActionProgress / 100f;
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
							}*/
							else{
								audioLerp (audioSourceMove, 0.0f, sRef.moveVolumeLerpRate);
								_currentState= PlayerState.standing; 
								currentActionProgress = 0f;
							}
						}
					break;	
					
					case PlayerState.building:
						if (audioSourceInfluenceStart.volume > 0.01f) { audioLerp (audioSourceInfluenceStart, 0.0f, sRef.playerInfluenceStartVolumeLerpRate);
							} else { audioSourceInfluenceStart.Stop (); }
						
						setProgressCircle(beaconInProgress.percBuildComplete/100);
						/*
						qudProgessCircle.renderer.enabled = true;
						qudProgessCircle.renderer.material.SetFloat("_Cutoff", 1-(beaconInProgress.percBuildComplete/100));
						*/
						
						if(buildButtonDown && currentTile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
						//	Jiggle ();	//Gotta jiggle
							Pulsate ();
							
							
						//Debug.Log ("In Build");
							if(currentTile.controllingTeam != null){
		//						Debug.Log ("current Team");
								if(currentTile.controllingTeam.teamNumber == team.teamNumber){
									moveTowardCenterOfTile (currentTile);
								//Check for a beacon in progress and start building!
									if(beaconInProgress != null){
										float vpsBuildRate = sRef.vpsBaseBuild * getAltarBuildBoost ();
										beaconInProgress.addBuildingProgress(vpsBuildRate);
										beaconInProgress.selfDestructing = false;
		
										if(x.HasValue){
											setDirection(x.Value);
											beaconInProgress.setDirection(x.Value);
										}
										if(beaconInProgress.percBuildComplete > 100f){
		
											beaconInProgress.Build();
											_currentState = PlayerState.standing;
											currentActionProgress = 0f;
										}
									}
								}
								else{
									_invalidAction = true;
									//Debug.Log ("743");
								
								}
							}
							else
							{
								float vpsInfluenceRate = sRef.vpsBasePlayerInfluence;
								addProgressToAction(vpsInfluenceRate);
								_currentState = PlayerState.influencing;
							}
							
						}
						else{
							currentActionProgress = 0;
							currentTile.beacon.GetComponent<Beacon>().AbortBuild();
							_currentState = PlayerState.standing;
							//StopSFX ();
							_invalidAction = true;
						//PlaySFX(invalid_Input, 1.0f);
							}
					break;
					
					case PlayerState.influencing:
			
					moveTowardCenterOfTile (currentTile);
					audioLerp (audioSourceMove, 0.0f, sRef.moveVolumeLerpRate);
					audioLerp (audioSourceMoveEnemyTerrain, 0.0f, sRef.moveVolumeLerpRate);
					if(currentTile.controllingTeam!= null && currentTile.controllingTeam.teamNumber != teamNumber){
							audioSourceInfluenceStart.clip = Resources.Load("SFX/Player_DeInfluencing_2") as AudioClip;
							if(!audioSourceInfluenceStart.isPlaying){
								audioSourceInfluenceStart.Play();
							}
						} else if(currentTile.owningTeam == null || currentTile.controllingTeam.teamNumber == teamNumber){
							audioSourceInfluenceStart.clip = Resources.Load("SFX/Player_Influencing") as AudioClip;
						}
						
						
						
						
		    			qudProgessCircle.renderer.enabled = true;
						if (currentTile.controllingTeam != null) {
							setProgressCircle( currentTile.percControlled/100 , currentTile.controllingTeam.teamColor);
						}
						else{
							setProgressCircle( currentTile.percControlled/100 );
							
						}	
						if(buildButtonDown && currentTile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
					//		Jiggle ();	//Gotta jiggle
							Pulsate ();
							if (!audioSourceInfluenceStart.isPlaying) {
								audioSourceInfluenceStart.Play ();
							}
							audioLerp (audioSourceInfluenceStart, sRef.playerInfluenceStartVolume, sRef.playerInfluenceStartVolumeLerpRate);
							
							if(currentTile.controllingTeam != null){
								if(currentTile.controllingTeam.teamNumber == teamNumber)
								{                                      
//									Debug.Log("Adding Influence");
//									
//									Debug.Log (sRef.vpsBasePlayerInfluence * getPlayerInfluenceBoost() * Time.deltaTime);
//									Debug.Log (currentTile.brdXPos);
//									Debug.Log(currentTile.percControlled);
								    float test = currentTile.addInfluenceReturnOverflow( sRef.vpsBasePlayerInfluence * getPlayerInfluenceBoost() * Time.deltaTime);
									currentActionProgress = currentTile.percControlled;
									
									float averageActionProgress = getAverageActionProgress();
									
//									Debug.Log (averageActionProgress*100 +" " +  currentTile.percControlled);
									if(averageActionProgress*100 > currentTile.percControlled  ){
										
										_invalidAction = true;		
									}
									if(Mathf.Abs(getAverageActionProgressDifference()) < .001 ){  
										
										
										_invalidAction = true;	
									}
								
								
								if(test > 0f || (currentTile.owningTeam != null && currentTile.owningTeam == team)){
									//Debug.Log ("here");
										if (currentTile.getLocalAltar () != null || currentTile.tooCloseToBeacon() || currentTile.gameObject.transform.FindChild ("Home(Clone)") != null) {
								
											//_invalidAction = true;
										
											_currentState = PlayerState.standing;
											
										}
										
										else {
										
											GameObject beaconBeingBuilt;
		
											if (currentTile.beacon == null) { 
												beaconBeingBuilt = (GameObject)GameObject.Instantiate(_prfbBeacon, new Vector3(0,0,0), Quaternion.identity);
												beaconBeingBuilt.name = "Beacon:  " + team.teamNumber;
												beaconBeingBuilt.GetComponent<Beacon>().PlayerNumber = PlayerNumber;
											}
											else {
												beaconBeingBuilt = currentTile.beacon;
											}
											
											beaconInProgress = beaconBeingBuilt.GetComponent<Beacon>();					
											
											if (currentTile.buildable () && 
												(beaconInProgress.currentState == null || beaconInProgress.currentState == BeaconState.BuildingBasic)) 
											{
												
												audioSourceInfluenceDone.volume = sRef.playerInfluenceDoneVolume;
												audioSourceInfluenceDone.Play ();
												_currentState = PlayerState.building;
												float vpsBuildRate = sRef.vpsBaseBuild * getAltarBuildBoost ();	
												addProgressToAction(vpsBuildRate);
												beaconInProgress.startBuilding(currentTile.gameObject, this.gameObject, vpsBuildRate);
												beaconInProgress.setDirection(facing);
												beaconInProgress.selfDestructing = false;
											}
										
											else if (beaconInProgress.facing != facing) {
												
												_currentState = PlayerState.rotating;
												beaconInProgress.startRotating (facing);
											}
											
											//Don't need to rotate, so either upgrade or return to standing
											else {
																			
												if (beaconInProgress.currentState == BeaconState.Basic || beaconInProgress.currentState == BeaconState.BuildingAdvanced) {
													
													_currentState = PlayerState.upgrading;
													beaconInProgress.startUpgrading ();
												}
											
												else if (beaconInProgress.currentState == BeaconState.Advanced) {
													_currentState = PlayerState.standing;
												}
											}
										}
									}
								}
								else{
									float test = currentTile.subTractInfluence(  sRef.vpsBasePlayerInfluence * getPlayerInfluenceBoost() * Time.deltaTime, team);
									currentActionProgress = currentTile.percControlled;
									if(test > 0f){
										currentTile.addInfluenceReturnOverflow(test);
										if (!audioSourceInvalid.isPlaying) {
											audioSourceInvalid.volume = sRef.invalidInputVolume;	//?
											//audioSourceInvalid.Play ();
											//Invoke("playInvalid", 1.0f);
										}
									}
									float averageActionProgress = getAverageActionProgress();
									
									if(_currentState == PlayerState.influencing){
										
//										Debug.Log (averageActionProgress*100 +" " +  currentTile.percControlled);
									if(averageActionProgress*100 < currentTile.percControlled ){ //&& isInfluenceNotFrameOne()){
										//Debug.Log("In total");
													_invalidAction = true;	
														
											}
									    if(Mathf.Abs(getAverageActionProgressDifference()) < .001){// && isInfluenceNotFrameOne()){  
											//Debug.Log(getAverageActionProgressDifference());
										       // Debug.Log("In average");
										
											_invalidAction = true;	
										}
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
		
								if(currentTile.percControlled == 100f) {
		//						Debug.Log ("INLFUENCE DONE");
									currentTile.tiltingFromPlayer = false;
									audioSourceInfluenceDone.volume = sRef.playerInfluenceDoneVolume;
									audioSourceInfluenceDone.Play ();
								}
								
								else { 
									currentTile.tiltingFromPlayer = true;
								}
								
								if (x.HasValue) { 
									setDirection (x.Value);
								}
							
								
							} else{
							///TODO catch fully influenced Tile!
		
							audioSourceInfluenceDone.volume = sRef.playerInfluenceDoneVolume;
							audioSourceInfluenceDone.Play ();
							}
						}
						else{
						///TODO: add reset to tile in case of change
							//need to reset currenttile to previousState
							//StopSFX();
							currentTile.tiltingFromPlayer = false;
							
							_currentState = PlayerState.standing;
						}	
						//Debug.Log (string.Format("Current {0}, previous {1}", Mathf.RoundToInt(currentActionProgress), (Mathf.RoundToInt(previousActionProgress))));
					    
					break;
					
					case PlayerState.rotating: 
		
						if (buildButtonDown) {
						
							Pulsate ();
							
							Beacon beacon = currentTile.beacon.GetComponent<Beacon>();
							
							setProgressCircle(beacon.percRotateComplete/100);
							
							/** This is to let players change facing mid-rotation 
							if (x.HasValue) { 
								setDirection (x.Value);
							}
							*/
												
							//This will only be called if players change facing mid-rotation - doesn't hurt anything so leaving it in for now
							if (beacon.dirRotatingToward != facing) {
								//StopSFX ();
								currentActionProgress = 0;
								beacon.Rotate (beacon.facing);
								beacon.percRotateComplete = 0f;
								_currentState = PlayerState.rotating;
								beacon.startRotating (facing);
							}
							
							else {
								
								float vpsRotateRate = sRef.vpsBaseRotate * getAltarRotateBoost ();
								addProgressToAction (vpsRotateRate);
								beacon.addRotateProgress (vpsRotateRate);
								beacon.dirRotatingToward = facing;
								
								if (beacon.percRotateComplete >= 100f) {
				
									currentActionProgress = 0;
									beacon.Rotate (facing);
									beacon.percRotateComplete = 0f;
									_currentState = PlayerState.standing;
									//StopSFX ();					
								}
							}
							moveTowardCenterOfTile (currentTile);
						}
							
						else {
						
							currentActionProgress = 0;
							currentTile.beacon.GetComponent<Beacon>().AbortRotate ();
							_currentState = PlayerState.standing;		
		
						}
					
					break;
					
					case PlayerState.upgrading:
		
						if (buildButtonDown) {
							
							Pulsate ();;
							float vpsUpgradeRate = sRef.vpsBaseUpgrade * getAltarUpgradeBoost ();
							addProgressToAction (vpsUpgradeRate);
							Beacon beacon = currentTile.beacon.GetComponent<Beacon>();
							beacon.addUpgradeProgress (vpsUpgradeRate);
							beacon.losingUpgradeProgress = false;
							
							setProgressCircle(beacon.percUpgradeComplete/100);
							
							if (beacon.percUpgradeComplete >= 100f) {
							
								currentActionProgress = 0;
								beacon.Upgrade ();
								beacon.percUpgradeComplete = 0f;
								_currentState = PlayerState.standing;
								
							}
							moveTowardCenterOfTile (currentTile);
						}
						
						else {
						
							currentActionProgress = 0;
							if (currentTile.beacon != null) currentTile.beacon.GetComponent<Beacon>().AbortUpgrade();
							_currentState = PlayerState.standing;
							//StopSFX ();
						
						}
					
					break;
				}
				
				
				//Set position based on offset
				/* Not with free movement!
				transform.parent.position = new Vector3 
					(GameManager.wrldPositionFromGrdPosition(grdLocation).x + _positionOffset.x / 2,
					 GameManager.wrldPositionFromGrdPosition(grdLocation).y + _positionOffset.y / 2, -1);
				*/			
				//If not pulsating, reset scale and _expanding
				if (!_pulsating && !Pause.paused) {
					transform.localScale = new Vector3 (_defaultScale.x, _defaultScale.y, _defaultScale.z) * getScaleBoost ();
					_expanding = true;
					pulsateProgress = 0f;
				}
				
				if (!Pause.paused) {		
					if(_invalidAction && _invalidActionFrames >= 2){
						qudActionableGlow.renderer.material.color = Color.red;	
						Jiggle();
						iTween.ShakeScale(qudActionableGlow,jiggleHash);
						
						if(!audioSourceInvalid.isPlaying){
							playInvalid(sRef.invalidInputVolume);
						}
					}
					else{
						qudActionableGlow.renderer.material.color = Color.white;	
					}
				}
			}
		}
		else {
			//Game over - pause de-influence sound
			audioSourceInfluenceStart.Stop ();
		}
	}
	
	//Not used with free movement.
	public void DoMove(BaseTile MoveTo){
		grdLocation = new Vector2(MoveTo.brdXPos, MoveTo.brdYPos);
		gm.tiles[(int)grdLocation.x,(int)grdLocation.y].GetComponent<BaseTile>().Reveal (_vision);
	}

	public void playInvalid(float vol){
		if (!Pause.paused) {
			//audioSourceInvalid.volume = vol;
			audioSourceInvalid.volume = sRef.invalidInputVolume;
			audioSourceInvalid.Play ();
		}
	}
	
	/// <summary>;
	/// Change teams, part of the setup process
	/// </summary>
	/// <param name="t">T.</param>
	public void SetTeam(TeamInfo t){
		team = t;
		MoveToTeamStart ();
	}
	
	public void SetColor (Color32 color) {
		GetComponent<MeshRenderer> ().material.color = color;
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
		return _controller.LeftStick.X;	
	}
				
	/// <summary>
	/// Gets the correct Y axis of the stick based on player number
	/// </summary>
	/// <returns>The player X axis.</returns>
	private float getPlayerYAxis(){
		//Debug.Log(Input.GetAxis("VerticalPlayer"+PlayerNumber));
		return _controller.LeftStick.Y;	
	}

	/// <summary>
	/// Gets the Build aixs based on player number
	/// </summary>
	/// <returns>The player X axis.</returns>
	public bool getPlayerBuild(){
		return _controller.Action1.IsPressed;
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
//		transform.RotateAround(transform.parent.position, new Vector3(0,0,1), currentRotAngle);
//		transform.RotateAround(transform.parent.position, new Vector3(0,0,-1), rotAngle);
		Vector2 normVec = new Vector2 (getPlayerXAxis(), getPlayerYAxis()).normalized;
		float angle;
		if (getPlayerXAxis() < 0) { 
			angle = Vector2.Angle (new Vector2 (0, 1), normVec);
		}
		else {
			angle = 360 - Vector2.Angle (new Vector2 (0, 1), normVec);
		}
		transform.eulerAngles = new Vector3(0f, 0f, angle);
	}
/// <summary>
/// 
/// </summary>
/// <param name="rate"></param>
	private void addProgressToAction(float rate){	//Do we actually need this for anything?
		currentActionProgress+= rate*Time.deltaTime;
		
	}
	
	/// <summary>
	/// Reveals nearby tiles.
	/// </summary>
	public void RevealTiles () {
		
		//BaseTile currentTile = GameManager.GameManagerInstance.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
		currentTile.Reveal(_vision);
		/**
		for (int i = _vision * -1; i <= _vision; i++) {
			for (int j = (_vision - Mathf.Abs (i)) * -1; j <= _vision - Mathf.Abs (i); j++) {
				GameObject tile = null;
				try { tile = gm.tiles[(int)_grdLocation.x + j, (int)_grdLocation.y + i]; }
					catch {  }
				if (tile != null) {
					tile.GetComponent<BaseTile>().IsRevealed = true;
				}		
			}
		}
		*/
	}
	
	/// <summary>
	/// Jiggle this instance.
	/// </summary>
	private void Jiggle () {
	
//		_positionOffset = new Vector2 (Random.Range (-1 * _jiggleRange, _jiggleRange), Random.Range(-1 * _jiggleRange, _jiggleRange));
		//pa = new Vector2 (Random.Range (-1 * _jiggleRange, _jiggleRange), 0);
		
		iTween.ShakePosition(gameObject, jiggleHash);
	}
	
	public void resetPos(){
		transform.localPosition = new Vector3(0,0,0);
	}
	
	/// <summary>
	/// Pulsate this instance.
	/// </summary>
	private void Pulsate () {
	
		_pulsating = true;
		float minScale = getMinScale();
		float maxScale = getMaxScale();
		
		if (_expanding) {
		
		
			float expandRate = 100f / _expandTime;
			pulsateProgress += expandRate * Time.deltaTime;
			
			
			float expandAmount = (maxScale - minScale) * (expandRate * Time.deltaTime / 100f);
			transform.localScale = new Vector3 (transform.localScale.x + expandAmount, transform.localScale.y + expandAmount, 1);

			if (pulsateProgress >= 100f) {
				transform.localScale = new Vector3 (maxScale, maxScale, 1);			
				_expanding = false;
				pulsateProgress = 0f;
			}						
		}
		
		else {
	
			float contractRate = 100f / _contractTime;
			pulsateProgress += contractRate * Time.deltaTime;
	
			float contractAmount = (maxScale - minScale) * (contractRate * Time.deltaTime / 100f);
			transform.localScale = new Vector3 (transform.localScale.x - contractAmount, transform.localScale.y - contractAmount, 1);			
			
			if (pulsateProgress >= 100f) {
				transform.localScale = new Vector3 (minScale, minScale, 1);
				_expanding = true;	
				pulsateProgress = 0f;
			}			
		}		
	}
	
	/// <summary>
	/// ONLY used for debug for player
	/// </summary>




		
	private float getTileSpeedBoost(BaseTile tile) {
		if (tile.owningTeam == null) {
			return sRef.coefMoveNeutral;
		}
		if (tile.owningTeam == team) {
			return sRef.coefMoveAllied;
		}
		return sRef.coefMoveEnemy;
	}
	
	private float getAltarSpeedBoost(){
		List<AltarType> a = gm.getCapturedAltars(team);
		if(a.Contains(AltarType.Choyutzol)){
			return 2f;
		}else{
			return 1f;
		}
	}
	
	private float getAltarBuildBoost(){
		List <AltarType> a = gm.getCapturedAltars(team);

		if(a.Contains(AltarType.Tikumose)){
			return 2f;
		}else{
			return 1f;
		}
	}
	
	private float getAltarUpgradeBoost (){
		List <AltarType> a = gm.getCapturedAltars(team);
		
		if(a.Contains(AltarType.Tikumose)){
			return 2f;
		}else{
			return 1f;
		}		
	}
	
	private float getAltarRotateBoost (){
		List <AltarType> a = gm.getCapturedAltars(team);
		
		if(a.Contains(AltarType.Tikumose)){
			return 2f;
		}else{
			return 1f;
		}		
	}
	
	private float getPlayerInfluenceBoost(){
		List <AltarType> a = gm.getCapturedAltars(team);

		if(a.Contains(AltarType.Choyutzol)){
			return 2f;
		}else{
			return 1f;
		}
	}
	
	private float getScaleBoost(){
		List <AltarType> a = gm.getCapturedAltars(team);
		
		if(a.Contains(AltarType.Munalwa)){
			return sRef.coefMunalwaScale;
		}else{
			return 1f;
		}
	}
	
	private float getMinScale () {
		return 0.5f * getScaleBoost();
	}
	
	private float getMaxScale () {
		return 0.9f * getScaleBoost();
	}
		
	private bool onWater (Vector3 pos) {
	
		return (gm.tiles[(int) Mathf.Floor (pos.x + 0.5f), (int) Mathf.Floor (pos.y + 0.5f)].GetComponent<BaseTile>().currentType == TileTypeEnum.water);
	
	}
	
	private bool outOfBounds (Vector3 pos) {
	
		//Assumes tile width and height of exactly 1; will screw up otherwise
	
		float minDistanceFromEdge = 0.0f; //Should probably be set in Settings; players cannot be closer to edge than this
	
		return (pos.x < -0.5f + minDistanceFromEdge ||
				pos.y < -0.5f + minDistanceFromEdge ||
		        pos.x > GameObject.Find ("TileCreator").GetComponent<TileCreation>().boardX - 0.5f - minDistanceFromEdge ||
		        pos.y > GameObject.Find ("TileCreator").GetComponent<TileCreation>().boardY - 0.5f - minDistanceFromEdge);
		       
		/**	return (Mathf.Floor(pos.x + 0.5f) < 0 || 
				Mathf.Floor(pos.y + 0.5f) < 0 ||
				Mathf.Floor(pos.x + 0.5f) > GameObject.Find ("TileCreator").GetComponent<TileCreation>().boardX ||
				Mathf.Floor(pos.y + 0.5f) > GameObject.Find ("TileCreator").GetComponent<TileCreation>().boardY);
		*/
	}
	
	private bool tooCloseToOpponent (Vector3 pos) {
	
		float minDistanceApart = 1.5f;	//Should probably be set in Settings; players cannot be closer than this
		
		foreach (GameObject o in GameObject.Find ("GameManager").GetComponent<GameManager>().players) {
			if (o.GetComponentInChildren<Player>().team != team) { //If it's an opponent, check its distance
				if (minDistanceApart > (o.GetComponentInChildren<Player>().gameObject.transform.parent.position - pos).magnitude) {
					return true;
				}
			} 
		}
		
		return false;	
	}
	
	public void OnCollisionEnter2D(Collision2D Collided){
		//Debug.Log("In Enter");
		GameObject go = Collided.gameObject;
//		if(go.tag == "Player" ){
//			Player p =  go.GetComponentInChildren<Player>();
//			if(p.team != team){
//				//Munalwa: Only teleport halfway home when teleporting
//				if (gm.getCapturedAltars (team).Contains (AltarType.Munalwa)) {
//					teleportTarget = (transform.parent.position + team.goGetHomeTile().transform.parent.position) / 2.0f;
//				}
//				else {
//					teleportTarget = team.goGetHomeTile().transform.parent.position;
//				}			
//				p._currentState = PlayerState.teleporting;
//			}
//		}
	}
	
	public bool closeEnoughToTarget (Vector3 newPos, Vector3 target, float closeEnoughDistance) {
	//Hoo boy
		return Mathf.Abs (newPos.x - target.x) + Mathf.Abs (newPos.y - target.y) <= closeEnoughDistance; 
	}
	
	public void moveTowardCenterOfTile (BaseTile tile) {
		
		Vector3 newPos;
		if (!Pause.paused) {
			if (closeEnoughToTarget (transform.parent.position, tile.transform.position, sRef.closeEnoughDistanceMoveToCenter)) {
				newPos = new Vector3 (tile.transform.position.x, tile.transform.position.y, transform.parent.position.z);
			}
			else {
				newPos = Vector2.Lerp(transform.parent.position, tile.transform.position, sRef.moveToCenterRate);
				newPos.z = transform.parent.position.z;
			}
			transform.parent.position = newPos;
		}	
	}

	public void doNewTile(BaseTile previousTile, BaseTile newTile){
		if (OpponentsOnTile (previousTile).Count == 0) {
			previousTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
		}
		
		if (OpponentsOnTile (newTile).Count == 0) {		
			newTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = true;
			newTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().material.color = team.highlightColor;
		}
		else {
			newTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = true;
			newTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().material.color = sRef.sharedTileColor;
		}

		//Debug.Log("In New Tile");
	//	qudActionableGlow.renderer.material.color = (newTile.getActionable(team, this.getPlayerBuild())) ?  Color.green : Color.red;
	}
	public void OnGUI(){
		if(gm.debugGUI == true){
			switch (gm.currentState){
			case GameState.playing:
				if(sRef.debugMode){
					GUI.Box (new Rect (10+200*(PlayerNumber-1),10,200,90), string.Format("Player {0}\r\nState: {1}\r\npercentcomplete{2}\r\nScore: {3}",PlayerNumber, currentState,currentActionProgress, team.score));	
				}
				break;
			}
		}
	}
	
	public void setProgressCircle(float progress){
		if (!Pause.paused) {
			qudProgessCircle.renderer.enabled = true;
			qudProgessCircle.renderer.material.color = team.beaconColor;
			float val = 1.001f-progress;
			if (val<= 0) {val =.001f;}
			qudProgessCircle.renderer.material.SetFloat("_Cutoff",val);
			actionProgressTicker = (++actionProgressTicker) % actionProgress.Length;
			actionProgress[actionProgressTicker] = progress;
		}
	}
	
	public void setProgressCircle(float progress, Color32 barColor){
		if (!Pause.paused) {
			qudProgessCircle.renderer.enabled = true;
			qudProgessCircle.renderer.material.color = barColor;
			float val = 1.001f-progress;
			if (val<= 0) { val =.001f; }
			
			qudProgessCircle.renderer.material.SetFloat("_Cutoff", val);
		
			actionProgressTicker = (++actionProgressTicker) % actionProgress.Length;
			actionProgress[actionProgressTicker] = progress;
		}
	}
	
	public void clearActionProgress(){
		for(int i = 0; i <actionProgress.Length; i++){
			actionProgress[i] = 0f;
		}
	}
	
	public float getAverageActionProgress(){
		float total=  0;
		int count = 0;
		for(int i = 0; i <actionProgress.Length; i++){
			if(actionProgress[i] != 0) {
				total+=actionProgress[i];
				count++;
			}
		}
		
		return total/(float)count;
	}
	
	public float getAverageActionProgressDifference(){
		float min=  float.MaxValue;
		float max = 0;
		int count = 0;
		for(int i = 0; i <actionProgress.Length; i++){
			if(actionProgress[i] != 0) {
				if(actionProgress[i]< min) min = actionProgress[i];
				if(actionProgress[i]> max) max = actionProgress[i];
				
				count++;
			}
		}
//		Debug.Log ("count: " + count);
		return max-min;
	}
	
	public bool isInfluenceNotFrameOne(){
		int zeroCount = 0;
		for(int i = 0; i <actionProgress.Length; i++){
			if(actionProgress[i] != 0) {
				if(actionProgress[i] ==0) { zeroCount++;}
				
			}
		}
		if(zeroCount>=2) return true; else return false;
	}
	
	
	public void audioLerp (AudioSource source, float target, float rate) {
		if (Mathf.Abs (source.volume - target) <= 0.01f) {
			source.volume = target;
		}
		else {
			source.volume = Mathf.Lerp (source.volume, target, rate);
		}
	}
	
	public List<Player> OpponentsOnTile () {
//		BaseTile currentTile = gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
		List<Player> opponentsOnSameTile = new List<Player>();
		foreach (GameObject o in gm.players) {
			Player p = o.transform.FindChild ("PlayerInner").GetComponent<Player>();
			if (p.team.teamNumber != team.teamNumber && 
				gm.tiles[(int) Mathf.Floor (p.transform.position.x + 0.5f), (int) Mathf.Floor (p.transform.position.y + 0.5f)].GetComponent<BaseTile>() == currentTile)
			{ 
				opponentsOnSameTile.Add (p);
			}
		}
		return opponentsOnSameTile;
	}
	
	public List<Player> OpponentsOnTile (BaseTile bt) {
		//		BaseTile currentTile = gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
		List<Player> opponentsOnSameTile = new List<Player>();
		foreach (GameObject o in gm.players) {
			Player p = o.transform.FindChild ("PlayerInner").GetComponent<Player>();
			if (p.team.teamNumber != team.teamNumber && 
			    gm.tiles[(int) Mathf.Floor (p.transform.position.x + 0.5f), (int) Mathf.Floor (p.transform.position.y + 0.5f)].GetComponent<BaseTile>() == bt)
			{ 
				opponentsOnSameTile.Add (p);
			}
		}
		return opponentsOnSameTile;
	}
	
	public bool IsActing () {
		return (currentState == PlayerState.building || currentState == PlayerState.influencing || currentState == PlayerState.rotating || 
				currentState == PlayerState.upgrading);
		}
}
