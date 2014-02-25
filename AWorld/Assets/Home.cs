using UnityEngine;
using System.Collections;

public class Home : MonoBehaviour {
	public TeamInfo  team;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Color32 copy = new Color32((byte)(team.teamColor.r +30), (byte)(team.teamColor.g-30), (byte)(team.teamColor.b+30), (byte)255);
		renderer.material.color = copy;
		
		
	}
}
