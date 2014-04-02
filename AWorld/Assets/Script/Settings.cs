using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour {
	//VPS -- Value Per Second, represents the amount of value added per second for building or moving.
	//Coef -- The coeficiant of that action, IE: movingt rhough enemy tile or building diffent structure.

	//USage:  Anytime we build every action takes 100 points.  Every frame (Update) we add vpsBase * coef * Time.deltaTime and add it to the current counter

	//Right now this is an object we can instantiate.  Left as is for future iterations that read from the file system.  
	
	//Char: starting with settings where everything takes 1 second.  

	/* NOTE: Changing these numbers does not actually update the Settings object currently in the scene. You must do this manually in the editor. */
	public float vpsBaseBuild;
	public float vpsBaseMove;
	public float vpsBaseFreeMoveSpeed;
	public float vpsBasePlayerInfluence;
	public float vpsBaseRotate;
	public float vpsBaseUpgrade;
	public float vpsScorePerAltarPerSecond;
	public float coefMoveNeutral;
	public float coefMoveAllied;
	public float coefMoveEnemy;
	public float coefConvert;
	public float coefBuildBeacon; 
	public float coefBaseBeaconInfluence;	//At 1.0f, takes 1 second to convert a neutral tile 1 space away
	public float coefOnixtal;	//Percentage strength at which non-facing influence beams operate with Onixtal
	public float coefTepwante;	//Percentage strength at which wider influence beams operate with Tepwante (currently 100% strength but we could change it)
	public float baseRequired;
	public float scoreOnCapture;
	public float vpsScorePerSecond;
	public float vpsBeaconBaseInfluence;
	public bool optLockTile;
	public bool optTilesGiveScore;
	public float valTileConvertScore;
	public Vector2 team1Start;
	public Vector2 team2Start;
	public int optPerlinLevel;
	public float valPointsToWin;
	public int beaconNoBuildRange;
	public int neutralBeaconCount;
	public bool debugMode;
	public Mode gameMode;
	public float percMaxInfluenceColor;	//Percentage of color that tile fills in when just before 100% influence
	public float selfDestructDelay;	//How long you wait after player stops building before destroying a beacon
	public float loseUpgradeProgressDelay;	//How long you wait after player stops upgrading before clearing upgrade progress
	
	// Use this for initialization
	void Start () {

		vpsBaseBuild = 25f;
		vpsBaseMove = 100f;
		vpsBaseFreeMoveSpeed = 2.0f;
		vpsBasePlayerInfluence =  25f;
		vpsBaseRotate = 50f;
		vpsBaseUpgrade = 25f;
		vpsScorePerAltarPerSecond = 1f;
		coefMoveNeutral = 1f;
		coefMoveAllied = 2f;
		coefMoveEnemy = .33f;
		coefConvert = .5f;
		coefBuildBeacon = .25f; 
		coefBaseBeaconInfluence = 0.5f;	//At 1.0f, takes 1 second to convert a neutral tile 1 space away
		coefOnixtal = 0.25f;	//Percentage strength at which non-facing influence beams operate with Onixtal
		coefTepwante = 1.0f;	//Percentage strength at which wider influence beams operate with Tepwante (currently 100% strength but we could change it)
		baseRequired = 100f;
		scoreOnCapture = 50f;
		vpsScorePerSecond = 3f;
		vpsBeaconBaseInfluence = 100f;
		optLockTile = false;
		optTilesGiveScore = true;
		valTileConvertScore = 1f;
		team1Start = new Vector2(5,7);
		team2Start = new Vector2(20,7);
		optPerlinLevel = 1800;
		valPointsToWin = 300;
		beaconNoBuildRange = 1;
		neutralBeaconCount = 12;
		debugMode = true;
		gameMode = Mode.OneVOne;
		percMaxInfluenceColor = 0.5f;	//Percentage of color that tile fills in when just before 100% influence
		selfDestructDelay = 0.5f;
		loseUpgradeProgressDelay = 0.5f;		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	private static Settings _instance;
	
	//This is the  reference that other classes will use
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
