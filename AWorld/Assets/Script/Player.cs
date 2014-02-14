using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private TeamInfo team;
	private Vector2 _grdLocation;

	/// <summary>
	/// Gets or sets the grd location.  Sets transform to corresponding center square of tile.  
	/// </summary>
	/// <value>Int Vector 2 of grid coordinates.  The grd location.</value>
	public Vector2 grdLocation{
		get{
			return _grdLocation;
		}
		set{
			transform.position = GameManager.wrldPositionFromGrdPosition(value);
			_grdLocation = value;
		}
	}

	public int teamNumber{
		get{
			return team.teamNumber;
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetTeam(TeamInfo t){
		team = t;
		GetComponent<MeshRenderer> ().material.color = team.teamColor;
		MoveToTeamStart ();
	}

	public void MoveToTeamStart(){
		if (team != null) {
			grdLocation = team.startingLocation;		
		}
	}
}
