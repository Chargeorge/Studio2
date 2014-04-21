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

	public  Rect TeamRect;
	public  Rect ScoreRect;

	public float currentActionProgress;
	public Settings sRef;
	public DirectionEnum facing;
	private GameObject _prfbBeacon;
	private Beacon beaconInProgress;
	private int _vision = 3;

	private float _jiggleRange = 0.1f;			//Max distance from center of grid the player will jiggle
	
	private bool _pulsating;	//Set to false every Update function; Pulsate sets it to true; if false at end of update, resets scale and _expanding
	private bool _expanding;	//Used during pulsating
	private Vector3 _defaultScale = new Vector3 (0.5f, 0.5f, 1f);
//	private float _expandRate = 0.007f;
//	private float _contractRate = 0.04f;
	private float _expandTime = 0.8f;
	private float _contractTime = 0.2f;
	private float pulsateProgress;
	
	private List<AltarType> altars;

	bool isPlaying = false;
	public AudioClip playerMove;
	public AudioClip influenceStart;
	public AudioClip influenceDone;
	public AudioClip invalid_Input;

	public float scoreBarH = 30;
	public float scoreBarW = 75;
	public Texture scoreTexture;
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

	// Use this for initialization
	void Start () {
		altars = new List<AltarType>();
		_currentState = PlayerState.standing;
		_prfbBeacon = (GameObject)Resources.Load("Prefabs/Beacon");
		sRef = Settings.SettingsInstance;
		gm = GameManager.GameManagerInstance;
		qudProgessCircle = transform.parent.FindChild("ActionTimer").gameObject;
		qudProgessCircle.renderer.material.color = team.beaconColor;
		qudActionableGlow = transform.parent.FindChild("ActionableGlow").gameObject;

		if(PlayerNumber == 1){
			scoreTexture = gm.scoreTexture1;
			winTexture = gm.winTexture1;
		} else {
			scoreTexture = gm.scoreTexture2;
			winTexture = gm.winTexture2;
		}
		
		TeamRect = new Rect((Screen.width - scoreBarW)*(PlayerNumber-1)+ 2, 0, scoreBarW, Screen.height);
		ScoreRect  = new Rect((Screen.width - scoreBarW)*(PlayerNumber-1)+ 2, Screen.height, scoreBarW,0);
	}
	
	
	// Update is called once per frame
	void Update (){ 
		
		DirectionEnum? x = getStickDirection();
		//BaseTile currentTile = gm.tiles[(int)grdLocation.x,(int)grdLocation.y].GetComponent<BaseTile>();	//Not used with free movement

		BaseTile currentTile = gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();

		bool buildButtonDown = getPlayerBuild();
		//if(x.HasValue) Debug.Log(x.Value);
		_pulsating = false;	//Pulsate () sets this to true; if false at the end of this method, reset scale and _expanding

		//_positionOffset = new Vector2 (0,0);	//This can't possibly be the right way to do this - Josh
				
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
					Debug.Log ("Getting New Tile");
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
			//If we are standing and we get an input, handle it.
//			Debug.Log (string.Format("Player number {0}, buld button down: {1}", PlayerNumber, buildButtonDown));
				qudProgessCircle.renderer.enabled = false;
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
				if (buildButtonDown && 
			    	currentTile.owningTeam == team &&
			    	currentTile.beacon != null &&
					facing == currentTile.beacon.GetComponent<Beacon>().facing &&
					(currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.Basic ||
					 currentTile.beacon.GetComponent<Beacon>().currentState == BeaconState.BuildingAdvanced)) 
				{				
					float vpsRate = sRef.vpsBaseUpgrade;
					addProgressToAction (vpsRate);
					_currentState = PlayerState.upgrading;
					currentTile.beacon.GetComponent<Beacon>().startUpgrading();
					currentTile.beacon.GetComponent<Beacon>().losingUpgradeProgress = false;
				} else {
					//PlaySFX(invalid_Input, 1.0f);
				}
				
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
					moveTowardCenterOfTile (currentTile);
					//NO BEACON HERE, GOTTA DO STUFF.
					//Check influence first
					if(currentTile.controllingTeam !=null){
						
						if(currentTile.controllingTeam.teamNumber == team.teamNumber){
							if(currentTile.percControlled < 100f){
							Pulsate ();
								_currentState = PlayerState.influencing;
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
								audio.Stop ();
								audio.PlayOneShot(influenceDone, 0.7f);
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
							}
						} else if(currentTile.tooCloseToBeacon() && currentTile.beacon == null){
								audio.PlayOneShot(invalid_Input, 1.0f);
						} else{
							_currentState = PlayerState.influencing;
						}
					}
					else {
						Pulsate ();
						float vpsInfluenceRate = sRef.vpsBasePlayerInfluence * getPlayerInfluenceBoost();
						addProgressToAction(vpsInfluenceRate);
						_currentState = PlayerState.influencing;
						currentTile.startInfluence(currentActionProgress, team);
					}
				}
				
				//We chillin
				else {
					moveTowardCenterOfTile (currentTile);
				}
				
			break;
			
			//IF stick holds the same direction keep doing move
			//IF it changes, remove all progress (PLACEHOLDER, feel free to nuke)
			//if it completes, move to next tile, set state to standing
			case PlayerState.moving:
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
						
						//Add acceleration
						Vector2 vectorToAdd = new Vector2 (getPlayerXAxis(), getPlayerYAxis()) * sRef.playerAccelRate;
						Debug.Log (vectorToAdd.y);
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
						if (!outOfBounds(posToCheck) && 
							//!tooCloseToOpponent(posToCheck) &&
							(!onWater(posToCheck) || gm.getCapturedAltars(team).Contains (AltarType.Thotzeti) || currentTile.currentType == TileTypeEnum.water)) 
						{	//Valid move

							PlaySFX(playerMove, 0.2f);
							transform.parent.position = posToCheck;
							BaseTile thisTile = gm.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
							if (thisTile != currentTile) {
								Debug.Log ("Getting New Tile moving");
								//currentTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;
								doNewTile(currentTile, thisTile);
							}
						}
					}
					
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
						StopSFX();
						_currentState= PlayerState.standing; 
						currentActionProgress = 0f;
					}
				}
			break;	
			
			case PlayerState.building:

				qudProgessCircle.renderer.enabled = true;
				qudProgessCircle.renderer.material.SetFloat("_Cutoff", 1-(beaconInProgress.percBuildComplete/100));
				
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
							PlaySFX(invalid_Input, 1.0f);
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
					StopSFX ();
					//PlaySFX(invalid_Input, 1.0f);
					}
			break;
			
			case PlayerState.influencing:
    			qudProgessCircle.renderer.enabled = true;
				qudProgessCircle.renderer.material.SetFloat("_Cutoff", 1-(currentTile.percControlled /100f) );
				if(buildButtonDown && currentTile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
			//		Jiggle ();	//Gotta jiggle
					Pulsate ();
					PlaySFX(influenceStart, 0.8f);
					
					if(currentTile.controllingTeam != null){
						if(currentTile.controllingTeam.teamNumber == teamNumber)
						{                                      
							//Debug.Log("Adding Influence");
							float test = currentTile.addInfluenceReturnOverflow( sRef.vpsBasePlayerInfluence * getPlayerInfluenceBoost() * Time.deltaTime);
							moveTowardCenterOfTile (currentTile);
						//	Debug.Log("test: " + test);
							if(test > 0f || (currentTile.owningTeam != null && currentTile.owningTeam == team)){
//								_currentState = PlayerState.standing;
								
								if (currentTile.getLocalAltar () != null || currentTile.tooCloseToBeacon()) {
								audio.Stop ();
									_currentState = PlayerState.standing;
									//if(currentTile.tooCloseToBeacon()) audio.PlayOneShot(invalid_Input, 0.3f); //this also applies to the neutral beacon
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
										audio.Stop ();
										audio.PlayOneShot(influenceDone, 0.7f);
										_currentState = PlayerState.building;
										float vpsBuildRate = sRef.vpsBaseBuild * getAltarBuildBoost ();	
										addProgressToAction(vpsBuildRate);
										beaconInProgress.startBuilding(currentTile.gameObject, this.gameObject, vpsBuildRate);
										beaconInProgress.setDirection(facing);
										beaconInProgress.selfDestructing = false;
									}
								
									else if (beaconInProgress.facing != facing) {
										audio.Stop();
										_currentState = PlayerState.rotating;
										beaconInProgress.startRotating (facing);
									}
									
									//Don't need to rotate, so either upgrade or return to standing
									else {
																	
										if (beaconInProgress.currentState == BeaconState.Basic || beaconInProgress.currentState == BeaconState.BuildingAdvanced) {
											audio.Stop();
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
							if(test > 0f){
								currentTile.addInfluenceReturnOverflow(test);
								audio.PlayOneShot(invalid_Input, 1.0f);
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
							currentTile.jigglingFromPlayer = false;
							audio.PlayOneShot(influenceDone, 1.0f);
						}
						
						else { 
							currentTile.jigglingFromPlayer = true;
						}
						
						if (x.HasValue) { 
							setDirection (x.Value);
						}
					
						
					} else{
					///TODO catch fully influenced Tile!
					audio.Stop();
					audio.PlayOneShot(influenceDone, 1.0f);
					}
				}
				else{
				///TODO: add reset to tile in case of change
					//need to reset currenttile to previousState
					//StopSFX();
					currentTile.jigglingFromPlayer = false;
					audio.Stop();
					_currentState = PlayerState.standing;
				}	
			break;
			
			case PlayerState.rotating: 

				if (buildButtonDown) {
				
					Pulsate ();
					
					Beacon beacon = currentTile.beacon.GetComponent<Beacon>();
					qudProgessCircle.renderer.enabled = true;
					qudProgessCircle.renderer.material.SetFloat("_Cutoff", 1-(beacon.percRotateComplete/100f));
				
					/** This is to let players change facing mid-rotation 
					if (x.HasValue) { 
						setDirection (x.Value);
					}
					*/
										
					//This will only be called if players change facing mid-rotation - doesn't hurt anything so leaving it in for now
					if (beacon.dirRotatingToward != facing) {
						StopSFX ();
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
							StopSFX ();					
						}
					}
					moveTowardCenterOfTile (currentTile);
				}
					
				else {
				
					currentActionProgress = 0;
					currentTile.beacon.GetComponent<Beacon>().percRotateComplete = 0f;
					_currentState = PlayerState.standing;
					currentTile.beacon.audio.Stop();				

				}
			
			break;
			
			case PlayerState.upgrading:

				if (buildButtonDown) {
					audio.Stop ();
					Pulsate ();;
					float vpsUpgradeRate = sRef.vpsBaseUpgrade * getAltarUpgradeBoost ();
					addProgressToAction (vpsUpgradeRate);
					Beacon beacon = currentTile.beacon.GetComponent<Beacon>();
					beacon.addUpgradeProgress (vpsUpgradeRate);
					beacon.losingUpgradeProgress = false;
					
					qudProgessCircle.renderer.enabled = true;
					qudProgessCircle.renderer.material.SetFloat("_Cutoff", 1-(beacon.percUpgradeComplete/100));
				
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
					currentTile.beacon.GetComponent<Beacon>().AbortUpgrade();
					_currentState = PlayerState.standing;
					StopSFX ();
				
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
		if (!_pulsating) {
			transform.localScale = new Vector3 (_defaultScale.x, _defaultScale.y, _defaultScale.z) * getScaleBoost ();
			_expanding = true;
			pulsateProgress = 0f;
		}
						
	}
	
	//Not used with free movement.
	public void DoMove(BaseTile MoveTo){
		grdLocation = new Vector2(MoveTo.brdXPos, MoveTo.brdYPos);
		gm.tiles[(int)grdLocation.x,(int)grdLocation.y].GetComponent<BaseTile>().Reveal (_vision);
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
		
		BaseTile currentTile = GameManager.GameManagerInstance.tiles[(int) Mathf.Floor (transform.parent.position.x + 0.5f), (int) Mathf.Floor (transform.parent.position.y + 0.5f)].GetComponent<BaseTile>();
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
		_positionOffset = new Vector2 (Random.Range (-1 * _jiggleRange, _jiggleRange), 0);
		
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



	private void OnGUI(){

		float perScore = team.score / sRef.valPointsToWin;
		ScoreRect.height =-Screen.height *perScore;

		GUI.DrawTexture(TeamRect, gm.scoreBgTexture, ScaleMode.StretchToFill, true, 1.0f);
		GUI.DrawTexture(ScoreRect, scoreTexture, ScaleMode.StretchToFill, true, 1.0f);

	//	GUI.DrawTexture(new Rect(0,(Screen.height - scoreBarH)*(PlayerNumber-1), Screen.width, scoreBarH), gm.scoreBgTexture, ScaleMode.StretchToFill, true, 1.0f);
	//	GUI.DrawTexture(new Rect(0,(Screen.height - scoreBarH)*(PlayerNumber-1), Screen.width * perScore, scoreBarH), scoreTexture, ScaleMode.StretchToFill, true, 1.0f);

//		GUI.DrawTexture(new Rect((Screen.width - scoreBarW)*(PlayerNumber-1)+ 2, 0, scoreBarW, Screen.height), gm.scoreBgTexture, ScaleMode.StretchToFill,true, 1.0f);
//		GUI.DrawTexture(new Rect((Screen.width - scoreBarW)*(PlayerNumber-1)+ 2, Screen.height, scoreBarW, -Screen.height *perScore),scoreTexture, ScaleMode.StretchToFill, true, 1.0f);


		int boxWidth = 1600;
		int boxHeight = 900;

		switch (gm.currentState){
			case GameState.gameWon:
				if(gm.vIsForVendetta.completingTeam == this.team){
				GUI.BeginGroup(new Rect(Screen.width/2 - boxWidth/2, Screen.height/2 - boxHeight/2, boxWidth, boxHeight));
				GUI.DrawTexture(new Rect(0,0,boxWidth,boxHeight), winTexture, ScaleMode.StretchToFill, true, 1.0f);
				GUI.EndGroup();
				}
				break;
				
			case GameState.playing:
				break;
			}
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
	
		float minDistanceFromEdge = 0.3f; //Should probably be set in Settings; players cannot be closer to edge than this
	
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

	public void PlaySFX(AudioClip clip, float _volume){
		audio.PlayOneShot(clip);
		
		if(audio.volume <= _volume){
			//audio.volume += 0.2f;
			audio.volume = _volume;
		}
		if(audio.volume >= _volume){
			audio.volume = _volume;
		}		
	}
	
	public void StopSFX(){
		audio.Stop();
		//audio.volume -= 0.2f;
		//StartCoroutine(StopSFXCoroutine ());
	}
	
	public IEnumerator StopSFXCoroutine(){
		audio.volume -= 0.1f;
		yield return new WaitForSeconds(0.6f);
		audio.Stop();
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
		if (closeEnoughToTarget (transform.parent.position, tile.transform.position, sRef.closeEnoughDistanceMoveToCenter)) {
			newPos = new Vector3 (tile.transform.position.x, tile.transform.position.y, transform.parent.position.z);
		}
		else {
			newPos = Vector2.Lerp(transform.parent.position, tile.transform.position, sRef.moveToCenterRate);
			newPos.z = transform.parent.position.z;
		}
		transform.parent.position = newPos;
		
	}

	public void doNewTile(BaseTile previousTile, BaseTile newTile){
		previousTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = false;

		newTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().enabled = true;
		newTile.gameObject.transform.Find("OwnedLayer").GetComponent<MeshRenderer>().material.color = team.highlightColor;

		//Debug.Log("In New Tile");
	//	qudActionableGlow.renderer.material.color = (newTile.getActionable(team, this.getPlayerBuild())) ?  Color.green : Color.red;
	}
	
}
