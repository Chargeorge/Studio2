using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Tower : MonoBehaviour {

	private List<InfluencePatternHolder> _pattern;
	public DirectionEnum facing;
	public TeamInfo controllingTeam;
	private TowerState _currentState;
	public float percActionComplete = 0;
	public float percInlfluenceComplete= 0;	//Countdown till another influence is popped
	
	public TowerState currentState{
		get{
			return _currentState;
		}
	}

	// Use this for initialization
	void Start () {
		///TODO ADD PATTERN STATICS
		
	}
	
	// Update is called once per frame
	void Update () {
	
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
			}
			if(_currentState == TowerState.BuildingAdvanced){
				_currentState = TowerState.Advanced;
			}			
		}
	}
}
