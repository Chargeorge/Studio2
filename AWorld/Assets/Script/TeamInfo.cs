//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
public class TeamInfo
{
	public Color32 teamColor;
	public Vector2 startingLocation;
	public int teamNumber;
	public float score;
	
	public TeamInfo ()
	{
		score = 0;
	}

	public static TeamInfo GetTeamInfo(int teamNumber){
		TeamInfo returnable = new TeamInfo ();
		switch (teamNumber) {
				case 1: 
						returnable.teamColor = new Color32 (0, 138, 206, 255);
						returnable.startingLocation = new Vector2 (19, 0);
						returnable.teamNumber = teamNumber;
						break;
				case 2:
						returnable.teamColor = new Color32 (255, 209, 38, 255);
						returnable.startingLocation = new Vector2 (19, 19);
						returnable.teamNumber = teamNumber;
						break;
				}
		return returnable;
	}
	
	public Color32 getHighLightColor(){ 
		Color32 HighlightColor = teamColor;
		HighlightColor.r+=100;
		HighlightColor.b-=100;
		HighlightColor.g-=100;
		return HighlightColor;
	}
	
	
}

