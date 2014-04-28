using System;
using UnityEngine;

public struct SizeSetting
{
		public Vector2 team1Start;
		public Vector2 team2Start;
		public Vector2 mapSize;
		public float cameraSize;
		public Vector3 cameraPosition;
	public Vector2 scorePos1;
	public Vector2 scorePos2;
	public float scaleY;
	public float bbLocalPosY;
	public float sbMoveUp;
		
		public SizeSetting (Vector2 mapSize, Vector2 team1Start, Vector2 team2Start, float cameraSize, 
	                    Vector2 cameraPosition, Vector2 scorePos1, Vector2 scorePos2, float scaleY, float bbLocalPosY, float sbMoveUp)
		{
			this.mapSize = mapSize;
			this.team1Start = team1Start;
			this.team2Start = team2Start;
			this.cameraSize = cameraSize;
			this.cameraPosition = new Vector3 (cameraPosition.x, cameraPosition.y, -10);
		this.scorePos1 = scorePos1;
		this.scorePos2 = scorePos2;
		this.scaleY = scaleY;
		this.bbLocalPosY = bbLocalPosY;
		this.sbMoveUp = sbMoveUp;
		}
}


