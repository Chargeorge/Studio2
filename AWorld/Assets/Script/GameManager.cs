using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

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
	public GameObject playerPrefab;
	
	// Use this for initialization
	void Start () {
		
		playerPrefab = (GameObject)Resources.Load("Prefabs/Player");
	}
	
	// Update is called once per frame
	void Update () {

		if (setup){
			switch (gameMode){
				case Mode.OneVOne:
					GameObject Player1 = (GameObject)Instantiate(playerPrefab, new Vector3(0,0,0), Quaternion.identity);
					GameObject Player2 = (GameObject)Instantiate(playerPrefab, new Vector3(0,0,0), Quaternion.identity);
					Player p1 = Player1.GetComponent<Player>();
					Player p2 = Player2.GetComponent<Player>();
					p1.SetTeam(TeamInfo.GetTeamInfo(1));
					p2.SetTeam(TeamInfo.GetTeamInfo(2));
					p1.PlayerNumber = 1;
					p2.PlayerNumber = 2;
					break;
				case Mode.TwoVTwo:
					
				break;

			}
			setup = false;
		}

	}


}
