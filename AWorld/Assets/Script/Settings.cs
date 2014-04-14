using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour {
	//VPS -- Value Per Second, represents the amount of value added per second for building or moving.
	//Coef -- The coeficiant of that action, IE: movingt rhough enemy tile or building diffent structure.

	//USage:  Anytime we build every action takes 100 points.  Every frame (Update) we add vpsBase * coef * Time.deltaTime and add it to the current counter

	//Right now this is an object we can instantiate.  Left as is for future iterations that read from the file system.  
	
	//Char: starting with settings where everything takes 1 second.  

	//Base values per second
	public float vpsBaseBuild;
	public float vpsBaseMove;
	public float vpsBaseFreeMoveSpeed;
	public float vpsBasePlayerInfluence;
	public float vpsBaseRotate;
	public float vpsBaseUpgrade;
	public float vpsScorePerAltarPerSecond;
	public float vpsBeaconBaseInfluence;
	public float vpsScorePerMinePerSecond;
	
	//Coefficients
	public float coefMoveNeutral;
	public float coefMoveAllied;
	public float coefMoveEnemy;
	public float coefConvert;
	public float coefBuildBeacon; 
	public float coefBaseBeaconInfluence;	//At 1.0f, takes 1 second to convert a neutral tile 1 space away
	public float coefOnixtal;	//Percentage strength at which non-facing influence beams operate with Onixtal
	public float coefTepwante;	//Percentage strength at which wider influence beams operate with Tepwante (currently 100% strength but we could change it)
	public float coefKhepru;	//Multiplier applied to all score coming in with Khepru
	public float coefYaxchay;	//Multiplier applied to range on altars with Yaxchay
	public float coefMunalwaScale;	//Multiplier applied to physical scale of player with Munalwa
	
	//Score stuff
	public float scoreOnCapture;
	public bool optLockTile;
	public bool optTilesGiveScore;
	public float valTileConvertScore;
	public float valPointsToWin;
	public float valScorePerMine;
	
	
	//Beacon stuff
	public int beaconBasicRange = 4;
	public int beaconAdvancedRange = 8;
	public int beaconNoBuildRange;
	public float selfDestructDelay;	//How long you wait after player stops building before destroying a beacon
	public float loseUpgradeProgressDelay;	//How long you wait after player stops upgrading before clearing upgrade progress
	
	//Board setup stuff
	public Vector2 team1Start;
	public Vector2 team2Start;
	public int optPerlinLevel;
	public int neutralBeaconCount;
	public int numAltars;
	public int numScoringAltars;
	public string ranjitRangeAltars;

	//Mode switches
	public bool debugMode;
	public Mode gameMode;
	
	//Visual stuff
	public float secMarqueeUpgradeTime;
	public int marqueeCount;
	public float percMaxInfluenceColor;	//Percentage of color that tile fills in when just before 100% influence
	public int colorOffSet;
	public float upgradeCircleStartScale; //The scale at which the upgrade circle anim thing starts
	public float upgradeCircleFinishScale; //The scale the upgrade circle anim thing is at around 99% complete
	public float upgradeCircleStartAlpha; //The alpha at which the upgrade circle anim thing starts
	public float upgradeCircleFinishAlpha; //The alpha the upgrade circle anim thing is at around 99% complete 
	
	//Movement rate stuff
	public float teleportRate;
	public float closeEnoughDistanceTeleport;
	public float moveToCenterRate;
	public float closeEnoughDistanceMoveToCenter;
	
	// Use this for initialization
	void Start () {

		//Base values per second
		vpsBaseBuild = 25f;
		vpsBaseMove = 100f;
		vpsBaseFreeMoveSpeed = 2.0f;
		vpsBasePlayerInfluence =  25f;
		vpsBaseRotate = 50f;
		vpsBaseUpgrade = 25f;
		vpsScorePerAltarPerSecond = 1f;
		vpsBeaconBaseInfluence = 100f;
		vpsScorePerMinePerSecond = 3f;
		//Coefficients
		coefMoveNeutral = 1f;
		coefMoveAllied = 2f;
		coefMoveEnemy = .33f;
		coefConvert = .5f;
		coefBuildBeacon = .25f; 
		coefBaseBeaconInfluence = 0.5f;	//At 1.0f, takes 1 second to convert a neutral tile 1 space away
		coefOnixtal = 0.25f;	//Percentage strength at which non-facing influence beams operate with Onixtal
		coefTepwante = 1.0f;	//Percentage strength at which wider influence beams operate with Tepwante (currently 100% strength but we could change it)
		coefKhepru = 2.0f;		//Multiplier applied to all score coming in with Khepru
		coefYaxchay = 2.0f;		//Multiplier for range of beacons with Yaxchay
		coefMunalwaScale = 2.0f;	//Multiplier applied to physical scale of player with Munalwa
	
		//Score stuff
		scoreOnCapture = 50f;
		optLockTile = false;
		optTilesGiveScore = true;
		valTileConvertScore = 1f;
		valPointsToWin = 300;
		valScorePerMine = 100f;
		//Beacon stuff
		beaconBasicRange = 4;
		beaconAdvancedRange = 8;
		beaconNoBuildRange = 1;
		selfDestructDelay = 0.5f;
		loseUpgradeProgressDelay = 0.5f;	
		
		//Board setup stuff
		team1Start = new Vector2(4,6);
		team2Start = new Vector2(12,6);
		optPerlinLevel = 1800;
		neutralBeaconCount = 12;
		numAltars = 0;
		numScoringAltars = 5;
		//Mode switches
		debugMode = true;
		gameMode = Mode.OneVOne;
		
		//Visual stuff
		percMaxInfluenceColor = 0.5f;	//Percentage of color that tile fills in when just before 100% influence
		secMarqueeUpgradeTime = .08f;
		marqueeCount = 16;
		colorOffSet = 2;
		upgradeCircleStartScale = 5.0f; //The scale at which the upgrade circle anim thing starts
		upgradeCircleFinishScale = 0.5f; //The scale the upgrade circle anim thing is at around 99% complete
		upgradeCircleStartAlpha = 0.1f; //The alpha at which the upgrade circle anim thing starts
		upgradeCircleFinishAlpha = 0.8f; //The alpha the upgrade circle anim thing is at around 99% complete 
		
		//Misc?
		teleportRate = .2f;
		closeEnoughDistanceTeleport = 0.2f;
		moveToCenterRate = 0.014f;
		closeEnoughDistanceMoveToCenter = 0.012f;
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
