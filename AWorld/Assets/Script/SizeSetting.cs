using System;
using UnityEngine;

public struct SizeSetting
{
		public Vector2 team1Start;
		public Vector2 team2Start;
		public Vector2 mapSize;
		public float cameraSize;
		
		public SizeSetting (Vector2 mapSize, Vector2 team1Start, Vector2 team2Start, float cameraSize)
		{
			this.mapSize = mapSize;
			this.team1Start = team1Start;
			this.team2Start = team2Start;
			this.cameraSize = cameraSize;
		}
}


