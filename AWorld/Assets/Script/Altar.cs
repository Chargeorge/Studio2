using UnityEngine;
using System.Collections;

public class Altar : MonoBehaviour {

	public TeamInfo currentControllingTeam;
	public AltarType alterType;
	public TeamInfo firstControllingTeam;
	public TeamInfo touchControl;
	public int brdX;
	public int brdY;
	GameManager gm;

	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Am I controlled? 
		
		
	
	
	}
	
	public void setType(TeamInfo team){
		if(team!=null) {
			renderer.material.color = team.teamColor;
		}else{
			renderer.material.color = Color.gray;
		}
			
	}
	
	
	
	
}
