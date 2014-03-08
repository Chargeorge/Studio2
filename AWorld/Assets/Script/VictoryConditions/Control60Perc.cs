using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control60Perc : VictoryCondition
{
	public Control60Perc (int valueIn):base(valueIn)
	{
		victoryMessage = "team {0} controls this world, rejoice."; 
	}
	
	public override void CheckState(GameManager gm){
		//Debug.Log("Check State");
		int totalTiles = gm.tiles.Length;
		
		int team1Tiles  = 0;
		int team2Tiles = 0;
		TeamInfo t1 = null;
		TeamInfo t2 = null;
		for(int y = 0; y< gm.tiles.GetLength(0); y++){
			for (int x = 0; x< gm.tiles.GetLength(1); x++){
				if(gm.tiles[y, x].GetComponent<BaseTile>().controllingTeam != null){
					if(gm.tiles[y, x].GetComponent<BaseTile>().controllingTeam.teamNumber == 1) { team1Tiles ++; t1 = gm.tiles[y, x].GetComponent<BaseTile>().controllingTeam;}
					
					if(gm.tiles[y, x].GetComponent<BaseTile>().controllingTeam.teamNumber == 2) { team2Tiles ++; t2 = gm.tiles[y, x].GetComponent<BaseTile>().controllingTeam;}
				}
			}
		}
		//Debug.Log("Team 1 Altars: " + Team1Altars.Count());
		//Debug.Log("Team 2 Altars: " + Team2Altars.Count());
		
		if(team1Tiles > (float)totalTiles*.6f){
			SetVictory(t1);
		}
		if(team1Tiles > (float)totalTiles*.6f){
			SetVictory(t2);
		}
	}
}


