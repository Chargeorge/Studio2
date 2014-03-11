using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlViaTime : VictoryCondition
{
	public ControlViaTime (int valueIn):base(valueIn)
	{
		victoryMessage = "team {0} Has gained power immesuable, congratulations."; 
	}
	
	public override void CheckState(GameManager gm){
		//Debug.Log("Check State");
		int totalTiles = gm.tiles.Length;
		
		
		TeamInfo t1 = gm.teams[0];
		TeamInfo t2 = gm.teams[1];
		
		if(t1.score > 100f && t2.score < 100f && t1.score > t2.score){
			SetVictory(t1);
		}
		else if(t2.score > 100f && t1.score < 100f && t2.score > t1.score){
			SetVictory(t2);
		}
		else  if(t2.score > 100f && t1.score > 100f){
			if(t1.score > t2.score){
				SetVictory(t2);
			}
			if(t2.score > t1.score){
				SetVictory(t2);
			}
		}
	}
}


