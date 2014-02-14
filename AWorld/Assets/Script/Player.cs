using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private TeamInfo team;
	private Vector2 _grdLocation;
	private PlayerState _currentState;
	public int PlayerNumber;

	public PlayerState currentState {
		get {
			return _currentState;
		}
	}

	/// <summary>
	/// Gets or sets the grd location.  Sets transform to corresponding center square of tile.  
	/// </summary>
	/// <value>Int Vector 2 of grid coordinates.  	The grd location.</value>
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
		currentState = PlayerState.standing;
	}
	
	// Update is called once per frame
	void Update () {
		switch( currentState){
			float xStick = 

			case PlayerState.standing:
				if(Input.GetAxis(
			break;
		}
	}
	/// <summary>
	/// Change teams, part of the setup process
	/// </summary>
	/// <param name="t">T.</param>
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

	private float getPlayerXAxis(){
		return Input.GetAxis("HorizontalPlayer"+PlayerNumber);	
	}
				
	private float getPlayerYAxis(){
		return Input.GetAxis("VerticalPlayer"+PlayerNumber);	
	}

	private float getPlayerBuild(){
		return Input.GetAxis("BuildPlayer"+PlayerNumber);	
	}
}
