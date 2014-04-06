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
using System.Collections.Generic;
using UnityEngine;
public class TeamInfo
{
	public Color32 teamColor;
	public Color32 tileColor;
	public Color32 beaconColor;
	public Vector2 startingLocation;
	public int teamNumber;
	public float score;
	public List<Color32> marqueeColorList;
	
	public TeamInfo ()
	{
		score = 0;
		marqueeColorList = new List<Color32>();
	}

	public static TeamInfo GetTeamInfo(int teamNumber){
		TeamInfo returnable = new TeamInfo ();
		switch (teamNumber) {
			case 1: 
				returnable.teamColor = new Color32 (17, 75, 141, 255);
				returnable.tileColor = new Color32 (88, 151, 209,255);
				returnable.beaconColor = new Color32 (17, 75, 141, 255);
				returnable.startingLocation = Settings.SettingsInstance.team1Start;
				returnable.teamNumber = teamNumber;


				break;
			case 2:
				returnable.teamColor = new Color32 (247, 180, 40, 255);
				returnable.tileColor = new Color32 (247, 180, 40,255);
		//		returnable.beaconColor = new Color32 (0, 165, 80, 255);
				returnable.beaconColor = new Color32 (240, 139, 32, 255);
				returnable.startingLocation = Settings.SettingsInstance.team2Start;
				returnable.teamNumber = teamNumber;
				break;
		}

		byte colorOffset = 0;
		for(int i = 0;  i< Settings.SettingsInstance.marqueeCount; i++){
			Color32 c = new Color32((byte) (returnable.tileColor.r-colorOffset) ,
			                        (byte) (returnable.tileColor.g-colorOffset),
			                        (byte)(returnable.tileColor.b-colorOffset),
			                        (byte)255);
			returnable.marqueeColorList.Insert (0,c);
			colorOffset +=3;
		}
		return returnable;
	}

	
	public Color32 getHighLightColor(){ 
//		Color32 HighlightColor = teamColor;
//		HighlightColor.r+=100;
//		HighlightColor.b-=100;
//		HighlightColor.g-=100;

		Color32 HighlightColor = new Color32(7, 65, 131,255);
		return HighlightColor;
	}
	
	public GameObject goGetHomeTile(){
		return  GameManager.GameManagerInstance.tiles[(int)startingLocation.x, (int)startingLocation.y];
	}
	
	
}

