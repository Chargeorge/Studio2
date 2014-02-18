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
		
		if(_currentState == TowerState.Basic){
			//find nearest convertable block
			if(tileBeingConverted == null){
				 tileBeingConverted = null;
				//FIND The first convertable tile, list is ordeed by distance
				_pattern.ForEach(delegate (InfluencePatternHolder p){
					if(tileBeingConverted == null){
						GameObject Tile = gm.tiles[(int)brdX + (int)p.relCoord.x, brdY + (int)p.relCoord.y];
						if(Tile != null){
							BaseTile Bt =  Tile.GetComponent<BaseTile>();
							if(Bt.controllingTeam != null && Bt.controllingTeam.teamNumber != controllingTeam.teamNumber && Bt.percControlled < 100){
								tileBeingConverted = Bt.gameObject;
								patternConverting = p;
							}
						}
						
					}
				 });
			 }
			 else{
				tileBeingConverted.GetComponent<BaseTile>().addProgressToInfluence(patternConverting.vpsInfluence, controllingTeam);
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
				_pattern = Tower.createBasicInfluenceList();
			}
			if(_currentState == TowerState.BuildingAdvanced){
				_currentState = TowerState.Advanced;
				_pattern = Tower.createBasicInfluenceList();
			}			
		}
	}
	
	public static List<InfluencePatternHolder> createBasicInfluenceList(){
		List<InfluencePatternHolder> returanble = new List<InfluencePatternHolder>();
		returanble.Add(new InfluencePatternHolder(new Vector2(0,1), 100f));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,2), 50f));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,3), 33.4f));
		returanble.Add(new InfluencePatternHolder(new Vector2(0,3), 25f));
		
		return returanble.OrderBy(o=>o.relCoord.magnitude).ToList();
	}
	
	
}
