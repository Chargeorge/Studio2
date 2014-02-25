using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Altar : MonoBehaviour {

	private TeamInfo _currentControllingTeam;
	public bool networked;
	public Settings sRef;
	public AltarType alterType;
	private TeamInfo _firstControllingTeam;
	public TeamInfo touchControl;
	public int brdX;
	public int brdY;
	public List<AStarholder> networkToBase;
	public GameManager gm;


	#region accessors
	public TeamInfo currentControllingTeam {
		get {
			return _currentControllingTeam;
		}
		set{
			if(_firstControllingTeam == null) _firstControllingTeam = value;
			_currentControllingTeam = value;
		}
	}
	
	
	public TeamInfo firstControllingTeam {
		get {
			return _firstControllingTeam;
		}
	}
	#endregion
	
	// Use this for initialization
	void Start () {
		sRef = GameObject.Find ("Settings").GetComponent<Settings>();
	}
	
	// Update is called once per frame
	void Update () {
		//Am I controlled? 
		
		if(_currentControllingTeam != null){
			//check to see if I'm networked
			networked= checkNetwork();
			
			gm.debugString = string.Format("ControllingTeam Number: {0}, Networked: {1}", _currentControllingTeam.teamNumber, networked);
		}
		
	}
	
	public void setControl(TeamInfo team){
		if(team!=null) {
			renderer.material.color = team.teamColor;
			_currentControllingTeam = team;
			checkNetwork();
		}else{
			renderer.material.color = Color.gray;
		}
			
	}
	
	public bool checkNetwork(){
		if(_currentControllingTeam != null){
			List<AStarholder> As = 	BaseTile.aStarSearch(gameObject.transform.parent.gameObject.GetComponent<BaseTile>(),gm.getTeamBase(_currentControllingTeam),int.MaxValue, BaseTile.getLocalSameTeamTiles, _currentControllingTeam);
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
	
	
}
