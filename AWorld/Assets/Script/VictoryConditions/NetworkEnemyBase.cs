using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEnemyBase: VictoryCondition
{
	public NetworkEnemyBase (int valueIn):base(valueIn)
	{
		victoryMessage = "team {0} has consumed all who oppose them \r\nand can exert their dark will upon this world."; 
	}
	
	public override void CheckState(GameManager gm){
		//Debug.Log("Check State");
		BaseTile team1Home;
		BaseTile team2Home;
		
		Vector2 team1Location = gm.teams[0].startingLocation;
		Vector2 team2Location = gm.teams[1].startingLocation;
		
		team1Home = gm.tiles[(int)team1Location.x, (int) team1Location.y].GetComponent<BaseTile>();
		team2Home = gm.tiles[(int)team2Location.x, (int) team2Location.y].GetComponent<BaseTile>();
		
		if(team1Home.controllingTeam != null  && team1Home.owningTeam ==  gm.teams[1] && team1Home.checkNetworkToHomeBase()){
			this.SetVictory(gm.teams[1]);
		}
		if(team2Home.controllingTeam != null  && team2Home.owningTeam ==  gm.teams[0] && team2Home.checkNetworkToHomeBase()){
			this.SetVictory(gm.teams[0]);
		}
	}
}
