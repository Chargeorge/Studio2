using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class Tower : MonoBehaviour {

	private List<InfluencePatternHolder> _pattern;
	public DirectionEnum facing;
	public TeamInfo controllingTeam;
	private TowerState _currentState;
	public float percActionComplete = 0;
	public float percInfluenceComplete= 0;	//Countdown till another influence is popped
	public GameManager gm;	
	public GameObject tileBeingConverted;
	private InfluencePatternHolder patternConverting;
	public Settings sRef;
	public TowerState currentState{
		get{
			return _currentState;
		}
	}

	public AudioClip towerInfluencing;
	public AudioClip towerBuilt;

	// Use this for initialization
	void Start () {
		///TODO ADD PATTERN STATICS
		gm = GameObject.Find ("GameManager").GetComponent<GameManager>();
		sRef= GameObject.Find ("Settings").GetComponent<Settings>();
	}
	
	// Update is called once per frame
	void Update () {
		int brdX; int brdY;
		brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
	 	brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
		
		setVisualDirection();

		//influence work
		// Find list of all influencible tiles
		//Get closest tile in terms of distance and  begin influening it
		//Add Influence till tile is at full
		// move on to the next tile
		//repeat till all tiles are full or influence is expended
		
		//This solution isn't "Correct" AKA, it doesn't resolve perfectly every frame, but over the aggregrate it should be correct
		// We can move to fixed update to get closer to correct
		if(_currentState == TowerState.Basic){

			//find nearest convertable block
			//FIND The first convertable tile, list is ordered by distance
			//TODO setup for bases so multiple tiles can influence at once
			float influenceThisFrame = sRef.vpsBeaconBaseInfluence * Time.deltaTime;
			
			_pattern.ForEach(delegate (InfluencePatternHolder p){
				if(influenceThisFrame > 0f){
					int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
					int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
					GameObject tile;
					
					try { tile = gm.tiles[x, y]; }
						catch { return; }
	//						if(tile != null && tile.GetComponent<BaseTile>().currentType != TileTypeEnum.water){
					
					if(tile != null){
					BaseTile Bt =  tile.GetComponent<BaseTile>();
						if(Bt.controllingTeam == null){
							Bt.startInfluence(influenceThisFrame, controllingTeam);
							influenceThisFrame = 0;  //Assume a null controlled Tile will eat all influence.
							//Debug.Log("influencing Null tile");
						}
						else if(Bt.controllingTeam.teamNumber != controllingTeam.teamNumber){
							influenceThisFrame = Bt.subTractInfluence(influenceThisFrame * p.coefInfluenceFraction, controllingTeam);
							//Debug.Log("Removing other influence");
							if(influenceThisFrame > 0){
								influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
								//Debug.Log("Adding next frames influence");
							}
						}
						else if(Bt.controllingTeam.teamNumber == controllingTeam.teamNumber && Bt.percControlled < 100f){
							influenceThisFrame = Bt.addInfluenceReturnOverflow(influenceThisFrame * p.coefInfluenceFraction);
							//Debug.Log ("Adding my influence");
						}
//						|| Bt.percControlled < 100f
					}
					
				}
			
			 });

//			 
//			 else{
//				gm.PlaySFX(towerInfluencing, 0.8f);
//			 //TODO: Handle situations where other tiles are influencing.  
//				Debug.Log("Trying to influence at rate " + patternConverting.vpsInfluence );
//				if(tileBeingConverted.GetComponent<BaseTile>().addProgressToInfluence(patternConverting.vpsInfluence, controllingTeam)){
//					tileBeingConverted = null;
//					patternConverting= null;
//				}
//			 }
		}
		
	}
	
	//START BUILDING:
	//SET the parent tile
	// Set the facing
	// ser State to building
	/// <summary>
	/// Puts it in the building state and starts the building.  
		/// </summary>
	/// <param name="tileLocation">Tile location.</param>
	/// <param name="player">Player.</param>
	/// <param name="valInit">Value init.</param>
	public void startBuilding(GameObject tileLocation, GameObject player, float valInit){
		this.gameObject.transform.parent = tileLocation.transform;
		this.facing = player.GetComponent<Player>().facing;
		this._currentState	= TowerState.BuildingBasic;
		controllingTeam = player.GetComponent<Player>().team;
		this.setTeam();

		this.transform.localPosition = new Vector3(0f,0f,-.5f);
		tileLocation.GetComponent<BaseTile>().tower = this.gameObject;

	}
	
	public void setTeam(){
		Color32 controllingTeamColor = controllingTeam.teamColor;		
		//TODO: custom sprites and colors per team
		controllingTeamColor.a = 0;
		controllingTeamColor.r += 30;
		controllingTeamColor.g += 30;
		controllingTeamColor.b += 30;
		renderer.material.color = controllingTeamColor;
		
	}
	
	/// <summary>
	/// Adds progress to the current building action
	/// </summary>
	/// <param name="rate">Rate.</param>
	public void addBuildingProgress(float rate){
		percActionComplete += rate*Time.deltaTime;

				
		Color32 towerColor = renderer.material.color;
		towerColor.a = (byte) (255f * (percActionComplete/100f));
		if (towerColor.a > 255f) towerColor.a = (byte) 255f;
		Debug.Log (towerColor.a);
		renderer.material.color = towerColor;		
		

	}
	
	public void addInfluenceProgress(float rate){
		percInfluenceComplete += rate*Time.deltaTime;
		//Debug.Log(percControlled);
	}	
	
	/// <summary>
	/// Finishes the current building action.  USE ONLY FOR BUILDING, INFLUENCE HANDLED ELSEWHERE
	/// </summary>
	public void finishAction(){
		///TODO: add end semaphore stuff her
		if(percActionComplete > 100f){
			percActionComplete = 100f;
			
			if(_currentState == TowerState.BuildingBasic){

				_currentState = TowerState.Basic;
				_pattern = Tower.createBasicInfluenceList(getAngleForDir(facing));
				
			}
			if(_currentState == TowerState.BuildingAdvanced){
				_currentState = TowerState.Advanced;
				_pattern = Tower.createBasicInfluenceList(getAngleForDir(facing));
			}			
		}
	}
	
	public static List<InfluencePatternHolder> createBasicInfluenceList(float degreeRotated){
		List<InfluencePatternHolder> returanble = new List<InfluencePatternHolder>();
		returanble.Add(new InfluencePatternHolder(new Vector2(0,1), 1f, degreeRotated));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,2), .5f, degreeRotated));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,3), .33f, degreeRotated));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,4), .25f, degreeRotated));
		
		return returanble.OrderBy(o=>o.relCoordRotated.magnitude).ToList();
	}
	void OnMouseOver() {
		GameObject.Find("GameManager").GetComponent<GameManager>().debugTower = this;
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
	
	public void setDirection(DirectionEnum N){
		
		float rotAngle = getAngleForDir(N);
		float currentRotAngle = getAngleForDir(facing);
		
		facing = N;
		transform.RotateAround(transform.position, new Vector3(0,0,1), currentRotAngle);
		transform.RotateAround(transform.position, new Vector3(0,0,-1), rotAngle);
	}
	
	public void setVisualDirection(){
		transform.localEulerAngles = new Vector3(0,0,-1*getAngleForDir(facing));
	}
	
	
	
}
