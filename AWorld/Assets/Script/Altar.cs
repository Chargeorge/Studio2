using UnityEngine;
using System.Collections;

public class Altar : MonoBehaviour {

	private TeamInfo _currentControllingTeam;
	public Settings sRef;
	public AltarType alterType;
	private TeamInfo _firstControllingTeam;
	public TeamInfo touchControl;
	public int brdX;
	public int brdY;
	GameManager gm;


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
		}
	}
	
	public void setType(TeamInfo team){
		if(team!=null) {
			renderer.material.color = team.teamColor;
		}else{
			renderer.material.color = Color.gray;
		}
			
	}
	
	
	
	
}
