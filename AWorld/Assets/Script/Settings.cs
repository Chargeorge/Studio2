using System.Collections;
using UnityEngine;

public class Settings : MonoBehaviour {
	//VPS -- Value Per Second, represents the amount of value added per second for building or moving.
	//Coef -- The coefficient of that action, IE: moving through enemy tile or building different structure.

	//Usage:  Anytime we build every action takes 100 points.  Every frame (Update) we add vpsBase * coef * Time.deltaTime and add it to the current counter

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
	public float vpsScorePerBit;
	
	//Coefficients
	public float coefMoveNeutral;
	public float coefMoveAllied;
	public float coefMoveEnemy;
	public float coefConvert;
	public float coefBuildBeacon; 
	public float coefBaseBeaconInfluence;	//At 1.0f, takes 1 second to convert a neutral tile 1 space away
	public float coefOnixtal;				//Percentage strength at which non-facing influence beams operate with Onixtal
	public float coefTepwante;				//Percentage strength at which wider influence beams operate with Tepwante (currently 100% strength but we could change it)
	public float coefKhepru;				//Multiplier applied to all score coming in with Khepru
	public float coefYaxchay;				//Multiplier applied to range on altars with Yaxchay
	public float coefMunalwaScale;			//Multiplier applied to physical scale of player with Munalwa
	
	//Score stuff
	public float scoreOnCapture;
	public bool optLockTile;
	public bool optTilesGiveScore;
	public float valTileConvertScore;
	public float valPointsToWin;
	public float valScorePerAltar;
	public float valTimePerScoreShot;
	public float valScoreBaseCapture;
	public float scoreBitIntervalSlow;
	public float scoreBitIntervalNormal;
	public float scoreBitIntervalFast;
	
	
	//Beacon stuff
	public int beaconBasicRange;
	public int beaconAdvancedRange;
	public int beaconNoBuildRange;
	public float selfDestructDelay;	 		//How long you wait after player stops building before starting to destroy a beacon
	public float loseUpgradeProgressDelay;	//How long you wait after player stops upgrading before starting to undo upgrade progress
	public float loseRotateProgressDelay;	//How long you wait after player stops rotating before starting to undo rotate progress
	
	//Board setup stuff
	public int neutralBeaconCount;
	public int numAltars;
	public int numScoringAltars;

	//Mode switches
	public bool debugMode;
	public Mode gameMode;
	
	//Visual stuff
	public float secMarqueeUpgradeTime;
	public int marqueeCount;
	public float percMaxInfluenceColor;		//Percentage of color that tile fills in when just before 100% influence
	public int colorOffSet;
	public float buildCircleStartScale; 	//The scale at which the upgrade circle anim thing starts
	public float buildCircleFinishScale; 	//The scale the upgrade circle anim thing is at around 99% complete
	public float buildCircleStartAlpha; 	//The alpha at which the upgrade circle anim thing starts
	public float buildCircleFinishAlpha; 	//The alpha the upgrade circle anim thing is at around 99% complete 
	public float upgradeCircleStartScale; 	//The scale at which the upgrade circle anim thing starts
	public float upgradeCircleFinishScale; 	//The scale the upgrade circle anim thing is at around 99% complete
	public float upgradeCircleStartAlpha; 	//The alpha at which the upgrade circle anim thing starts
	public float upgradeCircleFinishAlpha; 	//The alpha the upgrade circle anim thing is at around 99% complete
	public float drainedAltarAlpha;
	public float scoreBitExplosionStartSize;
	public float scoreBitExplosionFinishSize;
	public float scoreBitExplosionStartSpeed;
	public float scoreBitExplosionFinishSpeed;
	public float arrowSpeed;
	public float arrowInterval;
	public Color32 sharedTileColor;
	
	//Player movement stuff
	public float playerAccelRate;
	public float playerFriction;
	public float playerMaxSpeed;
	public float playerMinSpeed;			//Below this, player speed automatically set to 0
	
	//Misc
	public float teleportRate;
	public float closeEnoughDistanceTeleport;
	public float moveToCenterRate;
	public float closeEnoughDistanceMoveToCenter;
	public float closeEnoughDistanceScoreBit;
	public int influenceRevealRange; 	 //Radius of fog reveal when tile is influenced
	public float secTillRestartable;

	
	//Audio stuff
	public float moveVolume;
	public float moveEnemyVolume;
	public float moveVolumeLerpRate;
	public float playerInfluenceStartVolume;
	public float playerInfluenceStartVolumeLerpRate;
	public float playerInfluenceDoneVolume;
	public float playerDeinfluencingVolume;
	public float invalidInputVolume;

	public float beaconBuildingVolume;
	public float beaconBuiltVolume;
	public float beaconUpgradingVolume;
	public float beaconUpgradedVolume;
	public float beaconRotatingVolume;
	public float beaconRotatedVolume;

	public float altarScoreVolume;

	public float soundtrackVolume;
	public float layersVolume;
	public float musicClimaxVolume;

	public float startGameVolume;
	public float victoryVolume;
	public float restartVolume;
	
	public AudioClip InfluenceDoneHi;
	public AudioClip InfluenceDoneLo;
	
	//Overridden by options - ignore
	public Vector2 team1Start;
	public Vector2 team2Start;
	public Vector2 boardSize;
	public int optPerlinLevel;
	public string ranjitRangeAltars;
	public bool fogOn;
	public float cameraSize;
	public int[] perlinLevels;
	public float[] coefSpeed;
	public SizeSetting[] sizes;
	public Vector3 cameraPosition;

	public Vector2 scorePos1;
	public Vector2 scorePos2;
	public float scaleY;
	public float bbLocalPosY;
	public float sbMoveUp;
	public bool useReadyUp;
	public bool tutorial;
	// Use this for initialization
	void Start () {

		//Base values per second
		//Overloaded by settings, use these vals if settings not set
		vpsBaseBuild = 50f;
		vpsBaseMove = 100f;
		vpsBaseFreeMoveSpeed = 2.0f;
		vpsBasePlayerInfluence =  50f;
		vpsBaseRotate = 50f;
		vpsBaseUpgrade = 50f;
		vpsScorePerAltarPerSecond = 1f;
		vpsBeaconBaseInfluence = 100f;
		vpsScorePerBit = 1.5f;
		
		//Coefficients
		coefMoveNeutral = 1f;
		coefMoveAllied = 1.5f;
		coefMoveEnemy = .66f;
		coefConvert = .5f;
		coefBuildBeacon = .25f; 
		coefBaseBeaconInfluence = 0.5f;	//At 1.0f, takes 1 second to convert a neutral tile 1 space away
		coefOnixtal = 0.25f;			//Percentage strength at which non-facing influence beams operate with Onixtal
		coefTepwante = 1.0f;			//Percentage strength at which wider influence beams operate with Tepwante (currently 100% strength but we could change it)
		coefKhepru = 2.0f;				//Multiplier applied to all score coming in with Khepru
		coefYaxchay = 2.0f;				//Multiplier for range of beacons with Yaxchay
		coefMunalwaScale = 2.0f;		//Multiplier applied to physical scale of player with Munalwa
	
		//Score stuff
		scoreOnCapture = 120f;
		optLockTile = false;
		optTilesGiveScore = true;
		valTileConvertScore = 1f;
		valPointsToWin = 250;
		valScorePerAltar = 100f;
		valTimePerScoreShot = 1f;
		valScoreBaseCapture = 250f;
		scoreBitIntervalSlow = 2.0f;
		scoreBitIntervalNormal = 1.0f;
		scoreBitIntervalFast = 0.75f;	//Feels like halving this is too much - test
		
		//Beacon stuff
		beaconBasicRange = 4;
		beaconAdvancedRange = 8;
		beaconNoBuildRange = 1;
		selfDestructDelay = 0.5f;			//How long you wait after player stops building before starting to destroy a beacon
		loseUpgradeProgressDelay = 0.5f;	//How long you wait after player stops upgrading before starting to undo upgrade progress
		loseRotateProgressDelay = 0.5f;		//How long you wait after player stops rotating before starting to undo rotate progress
		
		//Board setup stuff
		neutralBeaconCount = 12;
		numAltars = 0;
		numScoringAltars = 5;
		
		//Mode switches
		debugMode = true;
		
		//Visual stuff
		percMaxInfluenceColor = 0.5f;		//Percentage of color that tile fills in when just before 100% influence
		secMarqueeUpgradeTime = .08f;
		marqueeCount = 16;
		colorOffSet = 2;
		buildCircleStartScale = 5.0f; 		//The scale at which the upgrade circle anim thing starts
		buildCircleFinishScale = 1.0f; 		//The scale the upgrade circle anim thing is at around 99% complete
		buildCircleStartAlpha = 0.1f; 		//The alpha at which the upgrade circle anim thing starts
		buildCircleFinishAlpha = 0.8f; 		//The alpha the upgrade circle anim thing is at around 99% complete 
		upgradeCircleStartScale = 7.0f; 	//The scale at which the upgrade circle anim thing starts
		upgradeCircleFinishScale = 0.5f; 	//The scale the upgrade circle anim thing is at around 99% complete
		upgradeCircleStartAlpha = 0.1f; 	//The alpha at which the upgrade circle anim thing starts
		upgradeCircleFinishAlpha = 0.8f; 	//The alpha the upgrade circle anim thing is at around 99% complete
		drainedAltarAlpha = 0.5f;
		scoreBitExplosionStartSize = 0.1f;
		scoreBitExplosionFinishSize = 0.5f;
		scoreBitExplosionStartSpeed = 0.5f;
		scoreBitExplosionFinishSpeed = 1.0f;
		arrowSpeed = 5f;					//how fast beacon arrows shoot
		arrowInterval = 2f;					//how often beacon arrows shoot
		sharedTileColor = new Color32 (76, 153, 0, 150);
		
		//Player movement stuff
		playerAccelRate = 0.38f;
		playerFriction = 0.2f;
		playerMaxSpeed = 0.85f;
		playerMinSpeed = 0.1f;		//Below this, player speed automatically set to 0
		
		//Misc
		teleportRate = .2f;
		closeEnoughDistanceTeleport = 0.2f;
		moveToCenterRate = 0.014f;
		closeEnoughDistanceMoveToCenter = 0.012f;
		closeEnoughDistanceScoreBit = 0.1f;
		influenceRevealRange = 3; 			//Radius of fog reveal when tile is influenced
		secTillRestartable = 3f;

		//Audio stuff
		moveVolume = 0.6f;
		moveVolumeLerpRate = 0.2f;
		playerInfluenceStartVolume = 1.0f;
		playerInfluenceStartVolumeLerpRate = 0.1f;
		playerInfluenceDoneVolume = 0.1f;
		playerDeinfluencingVolume = 0.2f;
		invalidInputVolume = 0.5f;

		beaconBuildingVolume = 0.5f;
		beaconBuiltVolume = 0.8f;
		beaconUpgradingVolume = 0.9f;
		beaconUpgradedVolume = 0.8f;
		beaconRotatingVolume = 0.8f;
		beaconRotatedVolume = 1f;
		
		altarScoreVolume = 0.3f;
		
		soundtrackVolume = 0.4f;
		layersVolume = 0.6f;
		musicClimaxVolume = 1f;
		
		startGameVolume = 0.8f;
		victoryVolume = 1f;
		restartVolume = 0.5f;
		
		InfluenceDoneHi = Resources.Load("SFX/Influence_Done_Hi") as AudioClip;
		InfluenceDoneLo = Resources.Load("SFX/Influence_Done_Lo") as AudioClip;

		//THESE VALS ARE OVERIDDEN BY OPTIONS, THESE ARE ONLY IF PREFS NOT SET
		team1Start = new Vector2(2,7);
		team2Start = new Vector2(19,7);
		boardSize = new Vector2(22,14);
		optPerlinLevel = 1800;
		cameraSize = 7.08f;
		fogOn = true;
		gameMode = Mode.TwoVTwo;
		
		//Settings
		perlinLevels = new int[]{3000, 1800, 1400};
		sizes = new SizeSetting[]{
			new SizeSetting(new Vector2(16,12),new Vector2(2,6), new Vector2(14,6),6.7f, new Vector2(7.5f, 5.5f), new Vector2(-2.340571f,0), new Vector2 (17.55239f,0), 10.95f, 6f, 5.75f),
			new SizeSetting(new Vector2(22,14),new Vector2(2,7), new Vector2(19,7), 7.65f, new Vector2(10.5f, 6.49f), new Vector2(-1.904194f,0), new Vector2 (22.76141f,0), 13.1f, 7f, 6.4f),
			new SizeSetting(new Vector2(28,20),new Vector2(2,10), new Vector2(26,10),10.6f,new Vector2(13.4f, 9.5f), new Vector2(-3.085168f,0), new Vector2 (29.88822f,0), 19.14f, 10f, 9.2f)};
		
		setPrefs();
		useReadyUp = false;

	}

	void setPrefs(){

		gameMode = (PlayerPrefs.GetInt(PreferencesOptions.numberOfPlayers.ToString()) == 2) ? Mode.OneVOne : Mode.TwoVTwo;
		fogOn = (PlayerPrefs.GetInt(PreferencesOptions.fogOn.ToString()) == 1) ? true : false;
		
		//Double-check for invalid values
		if (PlayerPrefs.GetInt(PreferencesOptions.terrainIntensity.ToString()) < 1 || PlayerPrefs.GetInt(PreferencesOptions.terrainIntensity.ToString()) > 3) {
			PlayerPrefs.SetInt(PreferencesOptions.terrainIntensity.ToString(), 2);
		}

		if (PlayerPrefs.GetInt(PreferencesOptions.terrainSize.ToString()) < 1 || PlayerPrefs.GetInt(PreferencesOptions.terrainSize.ToString()) > 3) {
			PlayerPrefs.SetInt(PreferencesOptions.terrainSize.ToString(), 2);
		}
		
		if (PlayerPrefs.GetInt(PreferencesOptions.gameSpeed.ToString()) < 1 || PlayerPrefs.GetInt(PreferencesOptions.gameSpeed.ToString()) > 3) {
			PlayerPrefs.SetInt(PreferencesOptions.gameSpeed.ToString(), 2);
		}
		
		optPerlinLevel =  (perlinLevels[PlayerPrefs.GetInt(PreferencesOptions.terrainIntensity.ToString())-1]);
		SizeSetting set = sizes[PlayerPrefs.GetInt(PreferencesOptions.terrainSize.ToString())-1];

		boardSize = set.mapSize;
		team1Start = set.team1Start;
		team2Start = set.team2Start;
		cameraSize = set.cameraSize;
		cameraPosition = set.cameraPosition;
		scorePos1 = set.scorePos1;
		scorePos2 = set.scorePos2;
		scaleY = set.scaleY;
		bbLocalPosY = set.bbLocalPosY;
		sbMoveUp = set.sbMoveUp;

		/*	public float vpsBaseBuild;
	public float vpsBaseMove;
	public float vpsBaseFreeMoveSpeed;
	public float vpsBasePlayerInfluence;
	public float vpsBaseRotate;
	public float vpsBaseUpgrade;
	public float vpsScorePerAltarPerSecond;
	public float vpsBeaconBaseInfluence;
	public float vpsScorePerMinePerSecond;
		*/

		switch (PlayerPrefs.GetInt (PreferencesOptions.gameSpeed.ToString())) {
		case 1:
			//Base values per second
			vpsBaseBuild = 25f;
			vpsBaseMove = 50f;
			vpsBaseFreeMoveSpeed = 2.0f;
			vpsBasePlayerInfluence =  25f;
			vpsBaseRotate = 25f;
			vpsBaseUpgrade = 25f;
			vpsScorePerAltarPerSecond = 1f;
			vpsBeaconBaseInfluence = 50f;
			vpsScorePerBit = 2.5f;

			break;

		case 2: 
			//Base values per second
			vpsBaseBuild = 50f;
			vpsBaseMove = 100f;
			vpsBaseFreeMoveSpeed = 3.0f;
			vpsBasePlayerInfluence =  50f;
			vpsBaseRotate = 50f;
			vpsBaseUpgrade = 50f;
			vpsScorePerAltarPerSecond = 1f;
			vpsBeaconBaseInfluence = 100f;
			vpsScorePerBit = 2.5f;

			break;

		case 3: 
			vpsBaseBuild = 100f;
			vpsBaseMove = 200f;
			vpsBaseFreeMoveSpeed = 4.0f;
			vpsBasePlayerInfluence =  100f;
			vpsBaseRotate = 100f;
			vpsBaseUpgrade = 100f;
			vpsScorePerAltarPerSecond = 1f;
			vpsBeaconBaseInfluence = 200f;
			vpsScorePerBit = 2.5f;

			break;
		}
		
		tutorial = (PlayerPrefs.GetInt(PreferencesOptions.tutorial.ToString()) == 1) ? true : false;

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
