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
	public float percInlfluenceComplete= 0;	//Countdown till another influence is popped
	public GameManager gm;	
	public GameObject tileBeingConverted;
	private InfluencePatternHolder patternConverting;
	public TowerState currentState{
		get{
			return _currentState;
		}
	}

	// Use this for initialization
	void Start () {
		///TODO ADD PATTERN STATICS
		gm = GameObject.Find ("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
		int brdX; int brdY;
		brdX = transform.parent.gameObject.GetComponent<BaseTile>().brdXPos;
		 brdY = transform.parent.gameObject.GetComponent<BaseTile>().brdYPos;
		
		setVisualDirection();
		
		if(_currentState == TowerState.Basic){
			//find nearest convertable block
			if(tileBeingConverted == null){
				 tileBeingConverted = null;
				//FIND The first convertable tile, list is ordeed by distance
				_pattern.ForEach(delegate (InfluencePatternHolder p){
					if(tileBeingConverted == null){
						int x = (int)brdX + (int)Mathf.RoundToInt(p.relCoordRotated.x);
						int y = (int)brdY + (int)Mathf.RoundToInt(p.relCoordRotated.y);
						GameObject Tile = gm.tiles[x, y];
						if(Tile != null){
							BaseTile Bt =  Tile.GetComponent<BaseTile>();
							if(Bt.controllingTeam == null){
								tileBeingConverted = Bt.gameObject;
								patternConverting = p;
								Debug.Log("found my dude");
							}
							else if(Bt.controllingTeam.teamNumber != controllingTeam.teamNumber || Bt.percControlled < 100f){
								tileBeingConverted = Bt.gameObject;
								patternConverting = p;
								Debug.Log("found my dude");
							}
						}
						
					}
				 });
			 }
			 else{
			 //TODO: Handle situations where other tiles are influencing.  
				Debug.Log("Tying to influence at rate " + patternConverting.vpsInfluence );
				if(tileBeingConverted.GetComponent<BaseTile>().addProgressToInfluence(patternConverting.vpsInfluence, controllingTeam)){
					tileBeingConverted = null;
					patternConverting= null;
				}
			 }
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
		controllingTeamColor.a = 255;
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
		//Debug.Log(percControlled);
	}
	
	public void addInfluenceProgress(float rate){
		percInlfluenceComplete += rate*Time.deltaTime;
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
		returanble.Add(new InfluencePatternHolder(new Vector2(0,1), 100f, degreeRotated));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,2), 50f, degreeRotated));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,3), 33.4f, degreeRotated));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,4), 25f, degreeRotated));
		
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
