using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public TeamInfo team;
	private Vector2 _grdLocation;
	private PlayerState _currentState;
	public int PlayerNumber;
	public GameManager gm; 
	private float currentActionProgress;
	public Settings sRef;
	public DirectionEnum facing;
	private GameObject _prfbTower;
	private Tower towerInProgress;
	

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
		_currentState = PlayerState.standing;
		_prfbTower = (GameObject)Resources.Load("Prefabs/Tower");
		sRef = GameObject.Find ("Settings").GetComponent<Settings>();
		gm = GameObject.Find ("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update (){ 
		DirectionEnum? x = getStickDirection();
		BaseTile currentTile = gm.tiles[(int)grdLocation.x,(int)grdLocation.y].GetComponent<BaseTile>();
		bool buildButtonDown = getPlayerBuild();
		
		switch( currentState){
			
			case PlayerState.standing:
			//If we are standinga nd we get an input, handle it.
				
				if(x.HasValue && !buildButtonDown){
					if(currentTile.GetDirection(x.Value) != null){
						BaseTile MovingInto = currentTile.GetDirection(x.Value);
						Debug.Log(string.Format("x:{0}, y: {1}", MovingInto.brdXPos, MovingInto.brdYPos));
						float vpsRate = MovingInto.GetRate(this) * sRef.vpsBaseMove;
						addProgressToAction(vpsRate);
						setDirection(x.Value);
						_currentState = PlayerState.moving;
						
					}
					else{
						setDirection(x.Value);
					}
					
					
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
					if(currentTile.tower == null){
		
						if(currentTile.controllingTeam != null){
							//Start building tower
							if(currentTile.controllingTeam.teamNumber == team.teamNumber){
								
								_currentState = PlayerState.building;
								float vpsBuildRate = sRef.vpsBaseBuild;
								addProgressToAction(vpsBuildRate);
								
								GameObject towerBeingBuild = (GameObject)GameObject.Instantiate(_prfbTower, new Vector3(0,0,0), Quaternion.identity);
								towerInProgress = towerBeingBuild.GetComponent<Tower>();
								towerInProgress.startBuilding(currentTile.gameObject, this.gameObject, vpsBuildRate);
								towerInProgress.setDirection(facing);
							}
						
							//Start removing influence
							else{
								
							}
						}
						//Start influencing Tower
						else
						{
							float vpsInfluenceRate = sRef.vpsBaseInfluence;
 							addProgressToAction(vpsInfluenceRate);
 							_currentState = PlayerState.influencing;
 							currentTile.startInfluence(currentActionProgress, team);
						}
					}
					else{
						//TOWER HERE KEEP BUILDING
						
						
					}
				}
				
			break;
			
			//IF stick holds the same direction keep doing move
			//IF it changes, remove all progress (PLACEHOLDER, feel free to nuke)
			//if it completes, move to next tile, set state to standing
			case PlayerState.moving:
				
			
				if(x.HasValue && x.Value == facing){
					BaseTile MovingInto = currentTile.GetComponent<BaseTile>().GetDirection(x.Value);
					float vpsRate = MovingInto.GetRate(this) * sRef.vpsBaseMove;
					addProgressToAction(vpsRate);
					
					if(currentActionProgress > sRef.baseRequired){
						DoMove(MovingInto);
						currentActionProgress = 0f;
						_currentState = PlayerState.standing;	
					}
				}
				else{
					_currentState= PlayerState.standing; 
					currentActionProgress = 0f;
				}
			break;	
			
			case PlayerState.building:
				if(buildButtonDown){
					Debug.Log ("In Build");
					if(currentTile.controllingTeam != null){
						Debug.Log ("curent Team");
						if(currentTile.controllingTeam.teamNumber == team.teamNumber){
							//Check for a tower in progress and start building!s
							if(towerInProgress != null){
								Debug.Log("attempting to build");
								float vpsBuildRate = sRef.vpsBaseBuild;
								towerInProgress.addBuildingProgress(vpsBuildRate);
								if(x.HasValue){
									setDirection(x.Value);
									towerInProgress.setDirection(x.Value);
								}
								if(towerInProgress.percActionComplete > 100f){
								
									Debug.Log("Finished?");
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
						float vpsInfluenceRate = sRef.vpsBaseInfluence;
						addProgressToAction(vpsInfluenceRate);
					}
					
				}
				else{
					_currentState =  PlayerState.standing;
					
					//TODO: figure out what happens when we abandon the tile
				}
			break;
			
			case PlayerState.influencing:
				if(buildButtonDown){
					if(currentTile.controllingTeam != null){
						float modifier = (currentTile.controllingTeam.teamNumber  == teamNumber) ? 1 : -1;
						float vpsInfluenceRate = sRef.vpsBaseInfluence * modifier;
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
						}
					
						
					} else{
					///TODOC catch fully influenced Tile!
					}
				}
				else{
				///TODO: add reset to tile in case of change
					//need to reset currenttile to previousState
				}	
			break;
		}	
	}
	
	public void DoMove(BaseTile MoveTo){
		grdLocation = new Vector2(MoveTo.brdXPos, MoveTo.brdYPos);
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

	private void addProgressToAction(float rate){
		currentActionProgress+= rate*Time.deltaTime;
		
	}	
	
	/// <summary>
	/// ONLY used for debug for player
	/// </summary>
	private void OnGUI(){
	
		if(sRef.debugMode){
			GUI.Box (new Rect (10+200*(PlayerNumber-1),10,200,90), string.Format("Player {0}\r\nState: {1}\r\npercentcomplete{2}",PlayerNumber, currentState,currentActionProgress));	
		}
	}
}
