using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum AntState{
	moving,
	mining,
	comingHome,
	lost, 
	depositing
}

public class WorkerAnt : MonoBehaviour {
	private List<BaseTile> tilesToTarget;
	private int currentIndex;
	
	private Vector3 previousPosition;
	private float timeInPreviousState;
	private float? timeToExecuteCurrentState;
	private Vector3 facing;
	private bool isCarrying;
	private GameObject carriedObject;
	private GameObject sprite;
	private Altar targetAlter;	
	private AntState state;
	private TeamInfo team;
	private bool pastFirstFrame = false;
	private float speed = 2f;
	

	public TeamInfo Team {
		get {
			return team;
		}
	}

	public AntState State {
		get {
			return state;
			
		}
	}

	// Use this for initialization
	void Start () {
		sprite = transform.Find("Sprite").gameObject;
		setState(AntState.lost);
		tilesToTarget = new List<BaseTile>();
		//Test Code? 
			Debug.Log("I'm a motherfucking ANT!");
		
	}
		
	// Update is called once per frame
	void Update () {
		if(!pastFirstFrame){
			SetTeam(GameManager.GameManagerInstance.teams[0]);
			pastFirstFrame = true;
		}
		bool timeUp = false;
		if(timeToExecuteCurrentState.HasValue){
			if(timeToExecuteCurrentState.Value < Time.time - timeInPreviousState){
				timeUp = true;
			
			}
		}
		if(Input.GetKeyDown(KeyCode.A)){
 			setState(AntState.moving);
		}
		if(Input.GetKeyDown(KeyCode.A)){
			setState(AntState.lost);
		}	
		if(Input.GetKeyDown(KeyCode.A)){
			setState(AntState.moving);
		}
		switch (State){
			case(AntState.moving) :
			{
				moveToNextPosition();
				if(currentTile() == tilesToTarget[currentIndex]){
					currentIndex++;
				}
				if(tilesToTarget[currentIndex].owningTeam != team){
					setState(AntState.lost);
				}
				if(targetAlter.currenTile == currentTile()){
					setState(AntState.mining);
				}
				break;
			}
			case(AntState.lost):
			{
				if(timeUp){
					setState(AntState.comingHome);		
				}
				break;
			
			}
			
			case(AntState.comingHome):
			{
				moveToNextPosition();
				if(currentTile() == team.goGetHomeTile()){
					setState(AntState.moving);
				}
			break;
					
			}
			
			
		}	
	}
	
	private void moveToNextPosition(){
		Vector3 dirToNext = (tilesToTarget[currentIndex].transform.position - transform.position);
		updatePos(dirToNext);
	}
		
		
	
	
	
	
	Altar getTargetAlter(){
		List<int> altarAntsCount  = getAltarAntCounts();
		int lowestValIndex = int.MaxValue;
		int index = 0;
		int returnableIndex = 0;
		altarAntsCount.ForEach(delegate(int obj) {
			
			if(obj < lowestValIndex){ 
				returnableIndex = index;
				lowestValIndex = obj;
			}
			index++;
		}
		);
		return GameManager.GameManagerInstance.altars[returnableIndex].GetComponent<Altar>();
		
	}
	
	
	
	void pathToTargetAltar(){
	
		List<AStarholder> path = BaseTile.aStarSearch(currentTile(), targetAlter.currenTile, int.MaxValue,BaseTile.getLocalTraversableTiles,team);
		tilesToTarget.Clear();
		foreach (AStarholder h in path){
			tilesToTarget.Add(h.current);
		}
	
	}
	
	void pathToHome(){
		List<AStarholder> As = 	BaseTile.aStarSearch(currentTile(),team.goGetHomeTile().GetComponent<BaseTile>(),int.MaxValue, BaseTile.getLocalSameTeamTiles, team);
		tilesToTarget.Clear();
		foreach (AStarholder h in As){
			tilesToTarget.Add(h.current);
		}
	}
	
	void getPath(BaseTile target){
		
	}
	
	void setState(AntState stateIn){
		timeInPreviousState = Time.time;
		if(stateIn == AntState.moving){
			targetAlter = getTargetAlter();
			pathToTargetAltar();
		}
		if(stateIn == AntState.lost){
			timeToExecuteCurrentState = 3f;
		}
		if(stateIn == AntState.comingHome){
			pathToHome();
			
		}	
		
		state= stateIn;
	}
	
	private BaseTile currentTile(){
		return GameManager.GameManagerInstance.tiles[(int) Mathf.Floor (transform.position.x + 0.5f), (int) Mathf.Floor (transform.position.y + 0.5f)].GetComponent<BaseTile>();
		
	}
	
	public void SetTeam(TeamInfo teamIn){
		sprite.GetComponent<Renderer>().material.color = teamIn.teamColorAlt;
		team= teamIn;
		
	}
	
	private List<int> getAltarAntCounts(){
		List<int> altarAntCounts = new List<int>(GameManager.GameManagerInstance.altars.Count);
		for (int i =0 ; i<  GameManager.GameManagerInstance.altars.Count; i++){
			
			altarAntCounts.Add(0);
		}
		for (int i =0 ; i<  GameManager.GameManagerInstance.altars.Count; i++){
			Altar a = GameManager.GameManagerInstance.altars[i].GetComponent<Altar>();
			foreach (WorkerAnt worker in team.Ants){
				if(worker.targetAlter == a){
					altarAntCounts[i]++;
				}
			}
		}
		return altarAntCounts;
	}
	private void updatePos(Vector3 offsetRaw){
		
		previousPosition = transform.position;
		transform.position+= new Vector3(offsetRaw.x, offsetRaw.y, 0).normalized * speed * Time.deltaTime;
	}
}
