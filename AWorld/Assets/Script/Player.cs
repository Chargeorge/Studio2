using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public TeamInfo team;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetTeam(TeamInfo t){
		team = t;
		GetComponent<MeshRenderer> ().material.color = team.teamColor;
	}
}
