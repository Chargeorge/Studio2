//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;

public class InfluencePatternHolder
{
	public Vector2 relCoord; //Relative coordinate of tower, assuming facing up
	public float vpsInfluence; // Value Per second of influence.
	public float degRotation; //Degrees Rotated, will return the relative based on this value
	public InfluencePatternHolder (Vector2 relCoord,  float vpsInfluenceRate)
	{
		this.relCoord = relCoord;
		this.vpsInfluence = vpsInfluenceRate;
	}
}


