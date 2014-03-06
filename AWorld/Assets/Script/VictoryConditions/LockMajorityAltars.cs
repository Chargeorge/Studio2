using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockMajorityAltars : VictoryCondition
{
		public LockMajorityAltars (int valueIn):base(valueIn)
		{
			victoryMessage = "team {0} has captured the most altars, and has undergone Apotheosis"; 
		}
		
		public override void CheckState(GameManager gm){
			//Debug.Log("Check State");
			int totalAltars = gm.altars.Count;
			IEnumerable<GameObject> Team1Altars = gm.altars.Where(x=>x.GetComponent<Altar>().owningTeamNetworkedAndLocked()==1);
			IEnumerable<GameObject> Team2Altars = gm.altars.Where(x=>x.GetComponent<Altar>().owningTeamNetworkedAndLocked()==2);
			//Debug.Log("Team 1 Altars: " + Team1Altars.Count());
			//Debug.Log("Team 2 Altars: " + Team2Altars.Count());
		
			if(Team1Altars.Count() > totalAltars/2){
				SetVictory(Team1Altars.First().GetComponent<Altar>().currentControllingTeam);
			}
			if(Team2Altars.Count()> totalAltars/2){
				SetVictory(Team2Altars.First().GetComponent<Altar>().currentControllingTeam);
			}
		}
}


