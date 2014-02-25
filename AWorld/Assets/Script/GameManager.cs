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
	public GameObject playerPrefab;
	public BaseTile debugMouse;
	public Tower debugTower;
	// Use this for initialization
	void Start () {
		sRef = GameObject.Find ("Settings").GetComponent<Settings>();
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

	void OnGUI(){
		if(sRef.debugMode){
			if(debugMouse!=null){
				GUI.Box (new Rect (10,100,200,90), string.Format("Mouse Over x:{0} y:{1}\r\nState: {2}\r\nPercentControlled: not yet ", debugMouse.brdXPos, debugMouse.brdYPos, debugMouse.currentState));
				
			}
			if(debugTower !=null){
				GUI.Box (new Rect (10,200,200,90), string.Format(" team {0} controlling\r\nstate: {1}", debugTower.controllingTeam.teamNumber, debugTower.currentState));
			}
		}
	}

	public void PlaySFX(AudioClip clip, float volume){
		audio.volume = volume;
		audio.PlayOneShot(clip);
	}

	public void StopSFX(){
		audio.Stop();
	}

}
