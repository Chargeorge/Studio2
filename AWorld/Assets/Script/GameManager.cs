using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public Settings sRef;
	#region Statics
	/// <summary>
	/// Converts the grd position into the absolute Unity world position
	/// </summary>
	/// <returns>The world position from grid position.</returns>
	/// <param name="grdPos">Grd position.</param>
	public static Vector3 wrldPositionFromGrdPosition(Vector2 grdPos){
		//This method is probably incomplete, needs to handle tile sizing intelligently
		return new Vector3 (grdPos.x, grdPos.y, -1);
	}
	#endregion
	public GameObject[,] tiles;
	private bool setup = true;
	public static Mode gameMode = Mode.OneVOne;
	public List<GameObject> players = new List<GameObject>();
	public GameObject prfbPlayer;
	public BaseTile debugMouse;
	public Tower debugTower;
	public GameObject prfbAltar, prfbHome;
	public List<GameObject> altars;
	public int numAltars;
	public string debugString;
	// Use this for initialization
	void Start () {
		sRef = GameObject.Find ("Settings").GetComponent<Settings>();
		prfbPlayer = (GameObject)Resources.Load("Prefabs/Player");
		prfbAltar = (GameObject)Resources.Load("Prefabs/Altar");
		prfbHome = (GameObject)Resources.Load ("Prefabs/Home");
		
		
	}
	
	// Update is called once per frame
	void Update () {
		if (setup){
			switch (gameMode){
				case Mode.OneVOne:
					GameObject Player1 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
					GameObject Player2 = (GameObject)Instantiate(prfbPlayer, new Vector3(0,0,0), Quaternion.identity);
					Player p1 = Player1.GetComponent<Player>();
					Player p2 = Player2.GetComponent<Player>();
					p1.SetTeam(TeamInfo.GetTeamInfo(1));
					p2.SetTeam(TeamInfo.GetTeamInfo(2));
					p1.PlayerNumber = 1;
					p2.PlayerNumber = 2;
					players.Add(Player1);
					players.Add(Player2);
					
					GameObject team1Home, team2Home;
					
					team1Home = setUpTeamHome(p1);
					team2Home = setUpTeamHome(p2);
						
					break;
				case Mode.TwoVTwo:
					
				break;

			}
			
			
			for (int i=0; i<numAltars; i++){
				Debug.Log ("in Altar");		
				GameObject a = (GameObject)Instantiate(prfbAltar, Vector3.zero, Quaternion.identity);
				Altar aObj = a.GetComponent<Altar>();
				aObj.brdX = Random.Range(0, tiles.GetLength(0));
				aObj.brdY = Random.Range(0, tiles.GetLength(1));
				aObj.gm = this;
				aObj.sRef = sRef;
				aObj.setControl(null);
				aObj.transform.parent = tiles[aObj.brdX, aObj.brdY].transform;
				aObj.transform.localPosition = new Vector3(0,0,-1);
			}
			
			
			
			setup = false;
			
		}

		debugMouse = getHoveredTile().GetComponent<BaseTile>();
		if(Input.GetButtonDown("Fire1")){
			if(debugMouse.owningTeam != null){
				BaseTile finalDestination = tiles[(int)debugMouse.owningTeam.startingLocation.x,(int)debugMouse.owningTeam.startingLocation.y].GetComponent<BaseTile>();
				if(debugMouse.owningTeam != null){
				
					List<AStarholder> As = 	BaseTile.aStarSearch(debugMouse, finalDestination,int.MaxValue, BaseTile.getLocalSameTeamTiles, debugMouse.owningTeam);
					debugString = string.Format("A star len: {0}", As.Count);
				}
			}
		}
		if(Input.GetButtonDown("Fire2")){
			debugMouse.owningTeam = players[0].GetComponent<Player>().team;
			
			debugMouse.controllingTeam = players[0].GetComponent<Player>().team;
			
			debugMouse.percControlled =100f;
		}
	}

	public GameObject getHoveredTile(){
		Vector3 MousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//Debug.Log("Mouse" + MousePos.x + ", " + MousePos.y);
		try{
			return tiles[Mathf.RoundToInt(MousePos.x), Mathf.RoundToInt(MousePos.y)];
		}
		catch{
			return null;
		}
	}

	void OnGUI(){
		if(sRef.debugMode){
			if(debugMouse!=null){
				GUI.Box (new Rect (10,100,200,90), string.Format("Mouse Over x:{0} y:{1}\r\nState: {2}\r\nPercentControlled: not yet ", debugMouse.brdXPos, debugMouse.brdYPos, debugMouse.currentState));
				
			}
			if(debugTower !=null){
				GUI.Box (new Rect (10,200,200,90), string.Format(" team {0} controlling\r\nstate: {1}", debugTower.controllingTeam.teamNumber, debugTower.currentState));
			}
			if(debugString != ""){
				GUI.Box (new Rect (210,100,200,90), debugString);
				
			}
		}
	}

	private GameObject setUpTeamHome(Player example){
		GameObject team1Home = (GameObject)Instantiate(prfbHome, Vector3.zero, Quaternion.identity);
		team1Home.transform.parent= tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].transform;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().controllingTeam = example.team;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().owningTeam = example.team;
		tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>().percControlled = 100f;
		team1Home.transform.localPosition = new Vector3(0,0,-.5f);
		team1Home.GetComponent<Home>().team = example.team;
		//ToSet = tiles[(int)example.team.startingLocation.x, (int)example.team.startingLocation.y].GetComponent<BaseTile>();
		
		return team1Home;
	}
	
	public BaseTile getTeamBase(TeamInfo T){
		BaseTile finalDestination = tiles[(int)T.startingLocation.x,(int)T.startingLocation.y].GetComponent<BaseTile>();
		return finalDestination;
	}
}
