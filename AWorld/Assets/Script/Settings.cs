using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour {
	//VPS -- Value Per Second, represents the amount of value added per second for building or moving.
	//Coef -- The coeficiant of that action, IE: movingt rhough enemy tile or building diffent structure.

	//USage:  Anytime we build every action takes 100 points.  Every frame (Update) we add vpsBase * coef * Time.deltaTime and add it to the current counter

	//Right now this is an object we can instantiate.  Left as is for future iterations that read from the file system.  


	//Char: starting with settings where everything takes 1 second.  
	public float vpsBaseBuild = 25f;
	public float vpsBaseMove = 100f;
	public float vpsBaseInfluence =  50f;
	public float vpsBaseRotate = 50f;
	public float vpsBaseUpgrade = 25f;
	public float vpsScorePerAltarPerSecond = .1f;
	public float coefAlliedMove = 1f;
	public float coefMoveAllied = 2f;
	public float coefMoveEnemy = .33f;
	public float coefConvert = .5f;
	public float coefBuildTower = .25f; 
	public float coefOnixtal = 0.25f;	//Percentage strength at which non-facing influence beams operate with Onixtal
	public float coefTepwante = 1.0f;	//Percentage strength at which wider influence beams operate with Tepwante (currently 100% strength but we could change it)
	public float baseRequired = 100f;
	public float scoreOnCapture = 50f;
	public float vpsScorePerSecond = 1f;
	public float vpsBeaconBaseInfluence = 100f;
	public bool optLockTile = false;
	public bool optTilesGiveScore = true;
	public float valTileConvertScore = 1f;
	public Vector2 team1Start = new Vector2(3,3);
	public Vector2 team2Start = new Vector2(4,4);
	
	public bool debugMode = true;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	private static Settings _instance;
	
	//This is the public reference that other classes will use
	public static Settings SettingsInstance
	{
		get
		{
			//If _instance hasn't been set yet, we grab it from the scene!
			//This will only happen the first time this reference is used.
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<Settings>();
			return _instance;
		}
	}
}
