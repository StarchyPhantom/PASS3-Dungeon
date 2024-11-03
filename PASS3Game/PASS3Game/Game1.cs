//Author: Benjamin Huynh
//File Name: Game1.cs
//Project Name: PASS3
//Creation Date: May. 24, 2022
//Modified Date: June. 21, 2022
//Description: an arcade-like top-down shooter with two characters you can tag in/out, different enemy types
//Output - sound and visual indicators, enemies and bullets appearing and moving, interface with informantion
//Variables - random spawning and shooting, health, special ammo, score, speed, locations of everything
//Input - keyboard for movement, items, volume, tag in/out, alternate shooting, and mouse for menu navigation and shooting direction and upgrades
//Arrays - enemy animations of the same type, enemy vectors of the same type, enemy timers of the same type, rock layouts, player animations, bullets of the same type
//Subprograms - allow ease of deleting enemies, deleting bullets, movement patterns, rock generation, item activation, player shooting, enemy spawning
//Selection - switch characters, movement, item, shoot, spawn/delete enemies and allow them to move and shoot
//Loops - deletion, detection, drawing of bullets, enemies, and obstacles

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Animation2D;
using Helper;

namespace PASS3Game
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		//constants of gamestates
		const byte MENU = 0;
		const byte INSTRUCTIONS = 1;
		const byte PREGAME = 2;
		const byte GAME = 3;
		const byte UPGRADE = 4;
		const byte ENDGAME = 5;

		//constants of directions
		const byte DOWN = 0;
		const byte LEFT = 1;
		const byte RIGHT = 2;
		const byte UP = 3;

		//on/off constants
		const byte OFF = 0;
		const byte ON = 1;

		//constants of items
		const byte NO_ITEM = 0;
		const byte COFFEE = 1;
		const byte BUBBLE = 2;
		const byte STAR = 3;

		//constants of upgrades
		const byte BOOTS = 0;
		const byte CLOCK = 1;
		const byte THREE_BULLETS = 2;
		const byte SHIELD = 3;
		const byte HEART = 4;

		//constants of speeds of the player, bullets, enemies
		const float BASE_SPEED = 2;
		const float BASE_BULLET_SPEED = 3;
		const float BASE_MELEE_ENEMY_SPEED = 1.5f;

		//location to hide game objects when not in use
		const float HIDE_OBJECTS_LOCATION = -400;

		//the rate that images are faded at
		const float FADE_FACTOR = 0.06f;

		//base firerates of both characters
		const int BASE_FIRE_RATE_J = 575;
		const int BASE_FIRE_RATE_E = 1100;
		const int MAX_BULLETS = 100;

		//angle in degrees of the multishot bullet offset
		const int MULTISHOT_OFFSET = 15;

		//angle in degrees of the starshot bullet offset
		const int STARSHOT_OFFSET = 45;

		//the offset of the floor in the game
		const int BORDER_OFFSET = 50;

		//UI related offsets
		const int UI_OFFSET = 150;
		const int UI_ICON_OFFSET = 10;

		//the size of the upgrades's images
		const int UPGRADE_SIZE = 200;

		//the maximum number of enemy types
		const int MAX_HITTERS = 100;
		const int MAX_SHOOTERS = 50;

		//graphics
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;

		//height & width of the screen
		int screenWidth;
		int screenHeight;

		//use tghis variable to allow for random number generation
		static Random rng = new Random();

		//the keys pressed on the keyboard
		KeyboardState kb;

		//the state of the mouse
		MouseState mouse;
		MouseState prevMouse;

		//the font used in the game
		SpriteFont gameFont;

		//the game logo image
		Texture2D logoImg;

		//the instruction images
		Texture2D instructions1Img;
		Texture2D instructions2Img;

		//character spritesheets
		Texture2D[] mainCharJImg = new Texture2D[4];
		Texture2D[] mainCharEImg = new Texture2D[4];

		//enemy spritesheets
		Texture2D[] robotEnemyImg = new Texture2D[4];
		Texture2D[] headEnemyImg = new Texture2D[4];
		Texture2D[] rockEnemyImg = new Texture2D[4];

		//randomized rock images
		Texture2D[] randomRocksImg = new Texture2D[20];

		//heart images
		Texture2D heartImg;
		Texture2D brokenHeartImg;

		//images of the upgrades
		Texture2D[] upgradesImg = new Texture2D[5];

		//floor/wall images
		Texture2D floorBkImg;
		Texture2D wallBkImg;

		//arrow to indicate where the player needs to go
		Texture2D downArrowImg;

		//rock obstacle images
		Texture2D rock1Img;
		Texture2D rock2Img;
		Texture2D rock3Img;

		//bullet images
		Texture2D metalBulletImg;
		Texture2D woodBulletImg;
		Texture2D enemyBulletImg;

		//smoke spritesheet
		Texture2D smokeTagImg;

		//bat spritesheet
		Texture2D batEnemyImg;

		//coffee "spritesheet"
		Texture2D coffeeImg;

		//bubble image
		Texture2D bubbleImg;

		//star box image
		Texture2D starImg;

		//button images
		Texture2D[] startImg = new Texture2D[2];
		Texture2D[] intsImg = new Texture2D[2];
		Texture2D[] menuImg = new Texture2D[2];
		Texture2D[] retryImg = new Texture2D[2];

		//vectors of where the enemies spawn
		Vector2[] enemySpawnFloorsLoc = new Vector2[4];
		Vector2[] enemySpawnsLoc = new Vector2[4];

		//location of player bullets
		Vector2[] playerBulletsLoc = new Vector2[MAX_BULLETS];

		//enemy locations
		Vector2[] batEnemiesLoc = new Vector2[MAX_HITTERS];
		Vector2[] robotEnemiesLoc = new Vector2[MAX_SHOOTERS];
		Vector2[] headEnemiesLoc = new Vector2[MAX_SHOOTERS];
		Vector2[] rockEnemiesLoc = new Vector2[MAX_HITTERS];

		//location to hide everything
		Vector2 hideObjectsLoc;

		//location of the instructions text
		Vector2 instructionsTextLoc;

		//location of where the score should be in the game
		Vector2 gameScoreLoc;

		//character starting location
		Vector2 roomCharacterStartLoc;

		//loction of the arrow to indicate where the player needs to go
		Vector2 downArrowLoc;

		//ui heart and health locations
		Vector2 heartJLoc;
		Vector2 heartELoc;
		Vector2 healthJLoc;
		Vector2 healthELoc;

		//upgrade icon locations
		Vector2 upgrade1Loc;
		Vector2 upgrade2Loc;

		//upgrade description locations
		Vector2[] upgradeTextLoc = new Vector2[5];

		//main character location, and the location of the center
		Vector2 mainCharLoc;
		Vector2 mainCharCenterLoc;

		//ui item icon location
		Vector2 UIItemLoc;

		//ui multishot icon and text location
		Vector2 UIMultiShotLoc;
		Vector2 UIMultishotTextLoc;

		//button locations
		Vector2 startBtnLoc;
		Vector2 intsBtnLoc;
		Vector2 menuBtnLoc;
		Vector2 retryBtnLoc;

		//rectangles for where the enemies spawwn
		Rectangle[] enemySpawnFloorsRec = new Rectangle[4];

		//rectangles of the rock obstacles
		Rectangle[] rocksRec = new Rectangle[16];

		//rectangles for player and enemy bullets
		Rectangle[] playerBulletsRec = new Rectangle[MAX_BULLETS];
		Rectangle[] robotBulletsRec = new Rectangle[MAX_BULLETS];
		Rectangle[] headBulletsRec = new Rectangle[MAX_BULLETS];

		//rectangle for the logo
		Rectangle logoRec;

		//rectangles of the instructions
		Rectangle instructions1Rec;
		Rectangle instructions2Rec;

		//rectangle of the floor and the wall
		Rectangle floorBkRec;
		Rectangle wallBkRec;

		//rectangle for the arrow indicating where the player needs to go
		Rectangle downArrowRec;

		//rectangle for the immunity bubble surrounding the player
		Rectangle immunityBubbleRec;

		//rectangles of the ui item icons
		Rectangle UIMultishotRec;
		Rectangle UIHeartItemRec;
		Rectangle UIBubbleRec;
		Rectangle UIstarRec;

		//rectangles of the items spawned in the room
		Rectangle heartItemRec;
		Rectangle bubbleRec;
		Rectangle starRec;

		//rectangles of the possinble upgrades
		Rectangle[] upgradesRec = new Rectangle[5];

		//rectangles of the buttons
		Rectangle startBtnRec;
		Rectangle intsBtnRec;
		Rectangle menuBtnRec;
		Rectangle retryBtnRec;

		//main character animations
		Animation[] mainCharJAnim = new Animation[4];
		Animation[] mainCharEAnim = new Animation[4];

		//bat enemy animation
		Animation[] batEnemyAnim = new Animation[MAX_HITTERS];

		//other enemy directional animations
		Animation[] robotEnemyDownAnim = new Animation[MAX_SHOOTERS];
		Animation[] robotEnemyLeftAnim = new Animation[MAX_SHOOTERS];
		Animation[] robotEnemyRightAnim = new Animation[MAX_SHOOTERS];
		Animation[] robotEnemyUpAnim = new Animation[MAX_SHOOTERS];
		Animation[] headEnemyDownAnim = new Animation[MAX_SHOOTERS];
		Animation[] headEnemyLeftAnim = new Animation[MAX_SHOOTERS];
		Animation[] headEnemyRightAnim = new Animation[MAX_SHOOTERS];
		Animation[] headEnemyUpAnim = new Animation[MAX_SHOOTERS];
		Animation[] rockEnemyDownAnim = new Animation[MAX_HITTERS];
		Animation[] rockEnemyLeftAnim = new Animation[MAX_HITTERS];
		Animation[] rockEnemyRightAnim = new Animation[MAX_HITTERS];
		Animation[] rockEnemyUpAnim = new Animation[MAX_HITTERS];

		//smoke animation
		Animation smokeTagAnim;

		//ui coffee icon animation
		Animation UICoffeeAnim;

		//coffee item animation
		Animation coffeeAnim;

		//timer for the time between player shots
		Timer playerFireRateTimer;

		//timer for the time the player is invincible
		Timer playerImmunityTimer;

		//timer for limiting the player from switching characters too much
		Timer tagDelayTimer;

		//timer that ensures that there are no indexing issues by delaying the time before an enemy is deleted
		Timer charEPierceTimer;

		//timers for the spawning of enemies
		Timer batEnemiesSpawnTimer;
		Timer robotEnemiesSpawnTimer;
		Timer headEnemiesSpawnTimer;
		Timer rockEnemiesSpawnTimer;

		//timers to make each shooting enemy randomly shoot
		Timer[] robotRandomShootTimer = new Timer[MAX_SHOOTERS];
		Timer[] headRandomShootTimer = new Timer[MAX_SHOOTERS];

		//timers to make the items dissapear
		Timer coffeeHideTimer;
		Timer bubbleHideTimer;
		Timer heartHideTimer;
		Timer starHideTimer;

		//timers for the length of the item's effect
		Timer coffeeEffectTimer;
		Timer bubbleEffectTimer;

		//songs for the appropriate gamestates
		Song[] menuMusic = new Song[2];
		Song[] gameMusic = new Song[3];
		Song[] endMusic = new Song[3];

		//sound effects for shooting and enemies dying
		SoundEffect bowSnd;
		SoundEffect gunSnd;
		SoundEffect metalSnd;
		SoundEffect sandSnd;

		//current gamestate
		byte gameState = MENU;

		//number of rock obstacles
		byte numRocks;

		//the direction the player is facing
		byte charDirection = DOWN;

		//the current item held by the player
		byte currentPlayerItem = NO_ITEM;

		//the current score and the highest score
		int score = 0;
		int highscore = 0;

		//current round
		int currentRound = 1;

		//the random number determining the upgrades chosen
		int randomUpgradeLeft;
		int randomUpgradeRight;

		//player healths
		int playerJHealth = 5;
		int playerEHealth = 4;

		//the count of bullets from the player
		int playerBulletsCount = 0;

		//amount of alternate ammo types left
		int storedMultiShot = 0;
		int storedStarShot = 0;

		//the current offset that a bullet is shot at
		int currentMultiShotAngle = 0;

		//times that the invincibility has been upgraded
		int timesImmuneUpgraded = 0;

		//the total count of enemies
		int totalEnemyCount= 0;

		//the amount of bats, the amount needed to be spawned, the total spawned
		int batEnemiesCount = 0;
		int batEnemiesToSpawn = 20;
		int batEnemiesSpawned = 0;

		//the amount of robots, the amount needed to be spawned, the total spawned
		int robotEnemiesCount = 0;
		int robotEnemiesToSpawn = 0;
		int robotEnemiesSpawned = 0;

		//the amount of heads, the amount needed to be spawned, the total spawned
		int headEnemiesCount = 0;
		int headEnemiesToSpawn = 0;
		int headEnemiesSpawned = 0;

		//the amount of rock enemies, the amount needed to be spawned, the total spawned
		int rockEnemiesCount = 0;
		int rockEnemiesToSpawn = 0;
		int rockEnemiesSpawned = 0;

		//number of enemy bullets
		int numRobotBullets = 0;
		int numHeadBullets = 0;

		//directions the enemies are facing
		int[] robotDirections = new int[MAX_SHOOTERS];
		int[] headDirections = new int[MAX_SHOOTERS];
		int[] rockEnemyDirections = new int[MAX_HITTERS];

		//the healths of the enemies
		int[] robotHealths = new int[MAX_SHOOTERS];
		int[] headHealths = new int[MAX_SHOOTERS];
		int[] rockEnemyHealths = new int[MAX_HITTERS];

		//the speeds of the enemies
		int[] robotBulletSpeedX = new int[MAX_BULLETS];
		int[] robotBulletSpeedY = new int[MAX_BULLETS];
		int[] headBulletSpeedX = new int[MAX_BULLETS];
		int[] headBulletSpeedY = new int[MAX_BULLETS];
		
		//the rock layout locations
		float[] roomType1X;
		float[] roomType1Y;
		float[] roomType2X;
		float[] roomType2Y;
		float[] roomType3X;
		float[] roomType3Y;
		float[] roomType4X;
		float[] roomType4Y;
		float[] roomType5X;
		float[] roomType5Y;

		//the fade of the immunity bubble over the player
		float immunityBubbleFade = 1;

		//the fade of the items
		float coffeeFade = 1;
		float bubbleFade = 1;
		float heartFade = 1;
		float starFade = 1;

		//player speed
		float currentMaxSpeed = BASE_SPEED;
		float maxDiagonalSpeed = (float)Math.Sqrt(BASE_SPEED);

		//the number that the fire rate is multiplied by to shorten it once upgraded
		float fireRateMultiplier = 1;

		//the speeds of the player's bullets
		float[] playerBulletSpeedX = new float[MAX_BULLETS];
		float[] playerBulletSpeedY = new float[MAX_BULLETS];

		//the angle the player's bullet is shot at
		float playerBulletAngle;

		//the character's distance away from enemies
		float charDistFromRockX;
		float charDistFromRockY;
		float charDistFromRobotX;
		float charDistFromRobotY;
		float charDistFromHeadX;
		float charDistFromHeadY;

		//is the current upgrade already randomized yet
		bool[] isUpgradeActive = new bool []{ false, false, false, false, false };

		//has the second upgrade option been randomized yet
		bool upgradeOptionChosen = false;

		//does the bullet belong to j or e
		bool[] isBulletOfJ = new bool[100];

		//is character j active or e
		bool isJActive = true;

		//is tht item in the room
		bool isCoffeeInRoom = false;
		bool isBubbleInRoom = false;
		bool isHeartInRoom = false;
		bool isStarInRoom = false;

		//is the mouse over these buttons
		bool startBtn = false;
		bool intsBtn = false;
		bool menuBtn = false;
		bool retryBtn = false;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			//set the dimensions of the game
			graphics.PreferredBackBufferWidth = 768 + UI_OFFSET * 2;
			graphics.PreferredBackBufferHeight = 768;

			//apply the changes
			graphics.ApplyChanges();

			//make the mouse visible
			IsMouseVisible = true;

			//get the width and height of the window
			screenWidth = graphics.GraphicsDevice.Viewport.Width;
			screenHeight = graphics.GraphicsDevice.Viewport.Height;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here

			//load font used in the game
			gameFont = Content.Load<SpriteFont>("Fonts/GameFont");

			//load the logo image
			logoImg = Content.Load<Texture2D>("Sprites/Logo");

			//load the instruction images
			instructions1Img = Content.Load<Texture2D>("Sprites/Ints1");
			instructions2Img = Content.Load<Texture2D>("Sprites/Ints2");

			//load the floor and wall images
			floorBkImg = Content.Load<Texture2D>("Backgrounds/GreyWall");
			wallBkImg = Content.Load<Texture2D>("Backgrounds/BlueWall");

			//load the image of the arrow indicating where the player goes
			downArrowImg = Content.Load<Texture2D>("Sprites/DownArrow");

			//load the images of the rock obstacles
			rock1Img = Content.Load<Texture2D>("Sprites/Stone1");
			rock2Img = Content.Load<Texture2D>("Sprites/Stone2");
			rock3Img = Content.Load<Texture2D>("Sprites/Stone3");

			//load the heart images
			heartImg = Content.Load<Texture2D>("Sprites/Heart");
			brokenHeartImg = Content.Load<Texture2D>("Sprites/BrokenHeart");

			//load the upgrade images
			upgradesImg[BOOTS] = Content.Load<Texture2D>("Sprites/Boot");
			upgradesImg[CLOCK] = Content.Load<Texture2D>("Sprites/Clock");
			upgradesImg[THREE_BULLETS] = Content.Load<Texture2D>("Sprites/3Bullets");
			upgradesImg[SHIELD] = Content.Load<Texture2D>("Sprites/Shield");
			upgradesImg[HEART] = Content.Load<Texture2D>("Sprites/Heart");

			//load the character spritesheets
			mainCharJImg[DOWN] = Content.Load<Texture2D>("Animations/CharacterJDOWN");
			mainCharJImg[LEFT] = Content.Load<Texture2D>("Animations/CharacterJLEFT");
			mainCharJImg[RIGHT] = Content.Load<Texture2D>("Animations/CharacterJRIGHT");
			mainCharJImg[UP] = Content.Load<Texture2D>("Animations/CharacterJUP");
			mainCharEImg[DOWN] = Content.Load<Texture2D>("Animations/CharacterEDOWN");
			mainCharEImg[LEFT] = Content.Load<Texture2D>("Animations/CharacterELEFT");
			mainCharEImg[RIGHT] = Content.Load<Texture2D>("Animations/CharacterERIGHT");
			mainCharEImg[UP] = Content.Load<Texture2D>("Animations/CharacterEUP");

			//load the smoke spritesheet
			smokeTagImg = Content.Load<Texture2D>("Animations/Smoke");

			//load bullet images
			metalBulletImg = Content.Load<Texture2D>("Sprites/MetalBullet");
			woodBulletImg = Content.Load<Texture2D>("Sprites/WoodBullet");
			enemyBulletImg = Content.Load<Texture2D>("Sprites/RedBullet");

			//load enemy spritesheets
			batEnemyImg = Content.Load<Texture2D>("Animations/BatEnemy");
			robotEnemyImg[DOWN] = Content.Load<Texture2D>("Animations/RobotEnemyDOWN");
			robotEnemyImg[LEFT] = Content.Load<Texture2D>("Animations/RobotEnemyLEFT");
			robotEnemyImg[RIGHT] = Content.Load<Texture2D>("Animations/RobotEnemyRIGHT");
			robotEnemyImg[UP] = Content.Load<Texture2D>("Animations/RobotEnemyUP");
			headEnemyImg[DOWN] = Content.Load<Texture2D>("Animations/FloatingHeadEnemyDOWN");
			headEnemyImg[LEFT] = Content.Load<Texture2D>("Animations/FloatingHeadEnemyLEFT");
			headEnemyImg[RIGHT] = Content.Load<Texture2D>("Animations/FloatingHeadEnemyRIGHT");
			headEnemyImg[UP] = Content.Load<Texture2D>("Animations/FloatingHeadEnemyUP");
			rockEnemyImg[DOWN] = Content.Load<Texture2D>("Animations/RockEnemyDOWN");
			rockEnemyImg[LEFT] = Content.Load<Texture2D>("Animations/RockEnemyLEFT");
			rockEnemyImg[RIGHT] = Content.Load<Texture2D>("Animations/RockEnemyRIGHT");
			rockEnemyImg[UP] = Content.Load<Texture2D>("Animations/RockEnemyUP");

			//load coffee item "spritesheet"
			coffeeImg = Content.Load<Texture2D>("Animations/Coffee");

			//load item sprites
			bubbleImg = Content.Load<Texture2D>("Sprites/Bubble");
			starImg = Content.Load<Texture2D>("Sprites/Star");

			//load button on/off images
			startImg[OFF] = Content.Load<Texture2D>("Sprites/StartOFF");
			startImg[ON] = Content.Load<Texture2D>("Sprites/StartON");
			intsImg[OFF] = Content.Load<Texture2D>("Sprites/IntsOFF");
			intsImg[ON] = Content.Load<Texture2D>("Sprites/IntsON");
			menuImg[OFF] = Content.Load<Texture2D>("Sprites/MenuOFF");
			menuImg[ON] = Content.Load<Texture2D>("Sprites/MenuON");
			retryImg[OFF] = Content.Load<Texture2D>("Sprites/RetryOFF");
			retryImg[ON] = Content.Load<Texture2D>("Sprites/RetryON");

			//load the location of where things are hidden
			hideObjectsLoc = new Vector2(HIDE_OBJECTS_LOCATION, HIDE_OBJECTS_LOCATION);

			//load the location of where the instructions text is
			instructionsTextLoc = new Vector2(screenWidth / 4, screenHeight / 3 * 2);

			//load the location of the score
			gameScoreLoc = new Vector2(screenWidth / 2 - (gameFont.MeasureString("Score: ").X / 2), 0);

			//load the location of where the character starts in the room
			roomCharacterStartLoc = new Vector2(screenWidth / 2 - mainCharJImg[DOWN].Width / 2 / 2 / 4, screenHeight / 2 - mainCharJImg[DOWN].Height / 2 / 2);

			//load the location of where the arrow guiding the player is
			downArrowLoc = new Vector2(screenWidth / 2 - downArrowImg.Width / 2, screenHeight / 1.5f);

			//load the locations of hearts and healths of the player
			heartJLoc = new Vector2(UI_OFFSET / 2 - heartImg.Width/ 2, screenHeight / 4);
			heartELoc = new Vector2(screenWidth - (UI_OFFSET / 2 + heartImg.Width / 2), screenHeight / 4);
			healthJLoc = new Vector2(heartJLoc.X + heartImg.Width + UI_ICON_OFFSET, heartJLoc.Y);
			healthELoc = new Vector2(heartELoc.X - heartImg.Width + UI_ICON_OFFSET, heartELoc.Y);

			//load the location of where the upgrade sqaures are
			upgrade1Loc = new Vector2(screenWidth / 3 - UPGRADE_SIZE / 2 - UI_ICON_OFFSET, screenHeight / 2 - UPGRADE_SIZE / 2);
			upgrade2Loc = new Vector2(screenWidth / 3 * 2 - UPGRADE_SIZE / 2 + UI_ICON_OFFSET, screenHeight / 2 - UPGRADE_SIZE / 2);

			//load the locations of the upgrade desciptions
			for (int i = 0; i < 5; i++)
			{
				//load the location of the upgrade description
				upgradeTextLoc[i] = hideObjectsLoc;
			}

			//load the location of the character and it's center
			mainCharLoc = roomCharacterStartLoc;
			mainCharCenterLoc = new Vector2(BORDER_OFFSET, BORDER_OFFSET);

			//load the location of where the ui items are moved to
			UIItemLoc = new Vector2(UI_OFFSET / 2 - coffeeImg.Width / 2, screenHeight / 2);

			//load the location of the multishot icon and it's text
			UIMultiShotLoc = new Vector2(screenWidth - UI_OFFSET + UI_ICON_OFFSET, screenHeight / 2);
			UIMultishotTextLoc = new Vector2(UIMultiShotLoc.X + upgradesImg[THREE_BULLETS].Width, UIMultiShotLoc.Y);

			//load the locations of the buttons
			startBtnLoc = new Vector2(screenWidth / 2 - startImg[OFF].Width * 4 / 2, screenHeight / 2 - startImg[OFF].Height * 4 / 2);
			intsBtnLoc = new Vector2(screenWidth / 2 - intsImg[OFF].Width * 4 / 2, screenHeight / 1.25f - intsImg[OFF].Height * 4 / 2);
			menuBtnLoc = new Vector2(screenWidth / 2 - menuImg[OFF].Width * 4 / 2, screenHeight / 1.25f - menuImg[OFF].Height * 4 / 2);
			retryBtnLoc = new Vector2(screenWidth / 2 - retryImg[OFF].Width * 4 / 2, screenHeight / 2 - retryImg[OFF].Height * 4 / 2);

			//load the locations of the enemy spawn floors
			enemySpawnFloorsLoc[DOWN] = new Vector2(screenWidth / 2 - BORDER_OFFSET / 2 * 3, screenHeight - BORDER_OFFSET);
			enemySpawnFloorsLoc[LEFT] = new Vector2(UI_OFFSET, screenHeight / 2 - BORDER_OFFSET / 2 * 3);
			enemySpawnFloorsLoc[RIGHT] = new Vector2(screenWidth - UI_OFFSET - BORDER_OFFSET, screenHeight / 2 - BORDER_OFFSET / 2 * 3);
			enemySpawnFloorsLoc[UP] = new Vector2(screenWidth / 2 - BORDER_OFFSET / 2 * 3, 0);

			//load the locations of the enemy spawns
			enemySpawnsLoc[DOWN] = new Vector2(screenWidth / 2, screenHeight);
			enemySpawnsLoc[LEFT] = new Vector2(UI_OFFSET, screenHeight / 2);
			enemySpawnsLoc[RIGHT] = new Vector2(screenWidth - UI_OFFSET, screenHeight / 2);
			enemySpawnsLoc[UP] = new Vector2(screenWidth / 2, 0);

			//load the locations and rectangles of all the bullets and set their speeds
			for (int i = 0; i < MAX_BULLETS ; i++)
			{
				//load the location and rectangle of the player bullet and set it's speed
				playerBulletsLoc[i] = hideObjectsLoc;
				playerBulletsRec[i] = new Rectangle((int)playerBulletsLoc[i].X, (int)playerBulletsLoc[i].Y, (int)(metalBulletImg.Width * 0.5), (int)(metalBulletImg.Height * 0.5));
				playerBulletSpeedX[i] = 0;
				playerBulletSpeedY[i] = 0;

				//load the location and rectangle of the robot bullet and set it's speed
				robotBulletsRec[i] = new Rectangle((int)hideObjectsLoc.X, (int)hideObjectsLoc.Y, (int)(enemyBulletImg.Width * 0.5), (int)(enemyBulletImg.Height * 0.5));
				robotBulletSpeedX[i] = 0;
				robotBulletSpeedY[i] = 0;

				//load the location and rectangle of the rock bullet and set it's speed
				headBulletsRec[i] = new Rectangle((int)hideObjectsLoc.X, (int)hideObjectsLoc.Y, (int)(enemyBulletImg.Width * 0.5), (int)(enemyBulletImg.Height * 0.5));
				headBulletSpeedX[i] = 0;
				headBulletSpeedY[i] = 0;
			}

			//load the locations and animations of melee enemies
			for (int i = 0; i < MAX_HITTERS; i++)
			{
				//load the location and animation of the bat enemy
				batEnemiesLoc[i] = hideObjectsLoc;
				batEnemyAnim[i] = new Animation(batEnemyImg, 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, batEnemiesLoc[i], 1, true);

				//load the location and animations of the rock enemy
				rockEnemiesLoc[i] = hideObjectsLoc;
				rockEnemyDownAnim[i] = new Animation(rockEnemyImg[DOWN], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, rockEnemiesLoc[i], 1, true);
				rockEnemyLeftAnim[i] = new Animation(rockEnemyImg[LEFT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, rockEnemiesLoc[i], 1, true);
				rockEnemyRightAnim[i] = new Animation(rockEnemyImg[RIGHT], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, rockEnemiesLoc[i], 1, true);
				rockEnemyUpAnim[i] = new Animation(rockEnemyImg[UP], 4, 1, 4, 0, 0, Animation.ANIMATE_FOREVER, 10, rockEnemiesLoc[i], 1, true);
			}

			//load the locations and animations of shooting enemies
			for (int i = 0; i < MAX_SHOOTERS; i++)
			{
				//load the location, animations, and random shooting timer of the robot enemy
				robotEnemiesLoc[i] = hideObjectsLoc;
				robotEnemyDownAnim[i] = new Animation(robotEnemyImg[DOWN], 6, 1, 6, 0, 0, Animation.ANIMATE_FOREVER, 10, robotEnemiesLoc[i], 1, true);
				robotEnemyLeftAnim[i] = new Animation(robotEnemyImg[LEFT], 6, 1, 6, 0, 0, Animation.ANIMATE_FOREVER, 10, robotEnemiesLoc[i], 1, true);
				robotEnemyRightAnim[i] = new Animation(robotEnemyImg[RIGHT], 6, 1, 6, 0, 0, Animation.ANIMATE_FOREVER, 10, robotEnemiesLoc[i], 1, true);
				robotEnemyUpAnim[i] = new Animation(robotEnemyImg[UP], 6, 1, 6, 0, 0, Animation.ANIMATE_FOREVER, 10, robotEnemiesLoc[i], 1, true);
				robotRandomShootTimer[i] = new Timer(rng.Next(5000, 7501), false);

				//load the location, animations, and random shooting timer of the head enemy
				headEnemiesLoc[i] = hideObjectsLoc;
				headEnemyDownAnim[i] = new Animation(headEnemyImg[DOWN], 10, 1, 10, 0, 0, Animation.ANIMATE_FOREVER, 10, headEnemiesLoc[i], 1, true);
				headEnemyLeftAnim[i] = new Animation(headEnemyImg[LEFT], 10, 1, 10, 0, 0, Animation.ANIMATE_FOREVER, 10, headEnemiesLoc[i], 1, true);
				headEnemyRightAnim[i] = new Animation(headEnemyImg[RIGHT], 10, 1, 10, 0, 0, Animation.ANIMATE_FOREVER, 10, headEnemiesLoc[i], 1, true);
				headEnemyUpAnim[i] = new Animation(headEnemyImg[UP], 10, 1, 10, 0, 0, Animation.ANIMATE_FOREVER, 10, headEnemiesLoc[i], 1, true);
				headRandomShootTimer[i] = new Timer(rng.Next(4000, 6501), false);
			}

			//load the rectangles of the ground rocks
			for (int i = 0; i < 16; i++)
			{
				//load the rectangle of the rock
				rocksRec[i] = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, rock1Img.Width * 2, rock1Img.Height * 2);
			}

			//load the rectangle of the logo
			logoRec = new Rectangle(screenWidth / 2 - logoImg.Width, 0, logoImg.Width * 2, logoImg.Height * 2);

			//load the rectangles of the instructions
			instructions1Rec = new Rectangle(screenWidth / 3 - logoImg.Width / 2, screenHeight / 3, logoImg.Width, logoImg.Height);
			instructions2Rec = new Rectangle(screenWidth / 3 * 2 - logoImg.Width / 2, screenHeight / 3, logoImg.Width, logoImg.Height);

			//load the rectangles of the floor and wall
			floorBkRec = new Rectangle(BORDER_OFFSET + UI_OFFSET, BORDER_OFFSET, screenWidth - (BORDER_OFFSET * 2) - UI_OFFSET * 2, screenHeight - (BORDER_OFFSET * 2));
			wallBkRec = new Rectangle(UI_OFFSET, 0, screenWidth - UI_OFFSET * 2, screenHeight);

			//load the rectangle of the guiding arrow
			downArrowRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, downArrowImg.Width, downArrowImg.Height);

			//load the rectangles of the ui item icons
			UIMultishotRec = new Rectangle((int)UIMultiShotLoc.X, (int)UIMultiShotLoc.Y, upgradesImg[THREE_BULLETS].Width, upgradesImg[THREE_BULLETS].Height);
			UIHeartItemRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, heartImg.Width, heartImg.Height);
			UIBubbleRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, bubbleImg.Width / 2, bubbleImg.Height / 2);
			UIstarRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, starImg.Width / 2, starImg.Height / 2);

			//load the rectangles of the unpgrades
			for (int i = 0; i < 5; i++)
			{
				//load the rectangle of the upgrade
				upgradesRec[i] = new Rectangle((int)hideObjectsLoc.X, (int)hideObjectsLoc.Y, UPGRADE_SIZE, UPGRADE_SIZE);
			}

			//load the rectangles of the items
			bubbleRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, bubbleImg.Width, bubbleImg.Height);
			heartItemRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, heartImg.Width / 2, heartImg.Height / 2);
			starRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, starImg.Width / 2, starImg.Height / 2);

			//load the rectangles of the buttons
			startBtnRec = new Rectangle((int)startBtnLoc.X, (int)startBtnLoc.Y, startImg[OFF].Width * 4, startImg[OFF].Height * 4);
			intsBtnRec = new Rectangle((int)intsBtnLoc.X, (int)intsBtnLoc.Y, intsImg[OFF].Width * 4, intsImg[OFF].Height * 4);
			menuBtnRec = new Rectangle((int)menuBtnLoc.X, (int)menuBtnLoc.Y, menuImg[OFF].Width * 4, menuImg[OFF].Height * 4);
			retryBtnRec = new Rectangle((int)retryBtnLoc.X, (int)retryBtnLoc.Y, retryImg[OFF].Width * 4, retryImg[OFF].Height * 4);

			//load the rectangles of the spawn floors
			enemySpawnFloorsRec[DOWN] = new Rectangle((int)enemySpawnFloorsLoc[DOWN].X, (int)enemySpawnFloorsLoc[DOWN].Y, BORDER_OFFSET * 3, BORDER_OFFSET);
			enemySpawnFloorsRec[LEFT] = new Rectangle((int)enemySpawnFloorsLoc[LEFT].X, (int)enemySpawnFloorsLoc[LEFT].Y, BORDER_OFFSET, BORDER_OFFSET * 3);
			enemySpawnFloorsRec[RIGHT] = new Rectangle((int)enemySpawnFloorsLoc[RIGHT].X, (int)enemySpawnFloorsLoc[RIGHT].Y, BORDER_OFFSET, BORDER_OFFSET * 3);
			enemySpawnFloorsRec[UP] = new Rectangle((int)enemySpawnFloorsLoc[UP].X, (int)enemySpawnFloorsLoc[UP].Y, BORDER_OFFSET * 3, BORDER_OFFSET);

			//load the animations of the characters
			mainCharJAnim[DOWN] = new Animation(mainCharJImg[DOWN], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, mainCharLoc, 0.5f, true);
			mainCharJAnim[LEFT] = new Animation(mainCharJImg[LEFT], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, mainCharLoc, 0.5f, true);
			mainCharJAnim[RIGHT] = new Animation(mainCharJImg[RIGHT], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, mainCharLoc, 0.5f, true);
			mainCharJAnim[UP] = new Animation(mainCharJImg[UP], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, mainCharLoc, 0.5f, true);
			mainCharEAnim[DOWN] = new Animation(mainCharEImg[DOWN], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, hideObjectsLoc, 0.5f, true);
			mainCharEAnim[LEFT] = new Animation(mainCharEImg[LEFT], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, hideObjectsLoc, 0.5f, true);
			mainCharEAnim[RIGHT] = new Animation(mainCharEImg[RIGHT], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, hideObjectsLoc, 0.5f, true);
			mainCharEAnim[UP] = new Animation(mainCharEImg[UP], 4, 1, 4, 0, 1, Animation.ANIMATE_FOREVER, 10, hideObjectsLoc, 0.5f, true);

			//load the rectangle of the bubble that covers the player
			immunityBubbleRec = new Rectangle((int)HIDE_OBJECTS_LOCATION, (int)HIDE_OBJECTS_LOCATION, mainCharJAnim[DOWN].destRec.Height, mainCharJAnim[DOWN].destRec.Height);

			//load the animation of the smoke
			smokeTagAnim = new Animation(smokeTagImg, 3, 2, 6, 1, 0, Animation.ANIMATE_FOREVER, 15, hideObjectsLoc, 1, false);

			//load the animation of the ui coffee icon
			UICoffeeAnim = new Animation(coffeeImg, 2, 1, 2, 0, 0, Animation.ANIMATE_FOREVER, 10, hideObjectsLoc, 1, true);

			//load the animation of the coffee item
			coffeeAnim = new Animation(coffeeImg, 2, 1, 2, 0, 0, Animation.ANIMATE_FOREVER, 10, hideObjectsLoc, 1, true);

			//load the timer for delaying the ability to tag in/out
			tagDelayTimer = new Timer(5000, true);

			//load the timer that ensures that there are no indexing issues by delaying the time before an enemy is deleted when intersecting a bullet
			charEPierceTimer = new Timer(50, true);

			//load the timers for spawning enemies
			batEnemiesSpawnTimer = new Timer(rng.Next(600, 1501), true);
			robotEnemiesSpawnTimer = new Timer(rng.Next(2000, 4001), true);
			headEnemiesSpawnTimer = new Timer(rng.Next(2500, 4501), true);
			rockEnemiesSpawnTimer = new Timer(rng.Next(600, 1501), true);

			//load the timers of the length the effect lasts
			coffeeEffectTimer = new Timer(10000, false);
			bubbleEffectTimer = new Timer(3000, false);

			//load the timers for making the items disappear if not collected
			coffeeHideTimer = new Timer(10000, false);
			bubbleHideTimer = new Timer(10000, false);
			heartHideTimer = new Timer(10000, false);
			starHideTimer = new Timer(10000, false);

			//load the songs for the menu
			menuMusic[0] = Content.Load<Song>("Music/Purrfect apawcalypse");
			menuMusic[1] = Content.Load<Song>("Music/Omori title");

			//load the songs for the game
			gameMusic[0] = Content.Load<Song>("Music/Journey of the prairie king");
			gameMusic[1] = Content.Load<Song>("Music/The score");
			gameMusic[2] = Content.Load<Song>("Music/Worlds end valentine");

			//load the songs for the end
			endMusic[0] = Content.Load<Song>("Music/See you tomorrow");
			endMusic[1] = Content.Load<Song>("Music/Whitespace");
			endMusic[2] = Content.Load<Song>("Music/Faint glow");

			//load the sounds of shooting and enemies dying
			bowSnd = Content.Load<SoundEffect>("Sounds/Bow");
			gunSnd = Content.Load<SoundEffect>("Sounds/Gun");
			metalSnd = Content.Load<SoundEffect>("Sounds/Metal");
			sandSnd = Content.Load<SoundEffect>("Sounds/Sand");

			//set the sound effect volume
			SoundEffect.MasterVolume = 0.25f;

			//set the music volume and play a random song for the menu
			MediaPlayer.Volume = 1f;
			MediaPlayer.Play(menuMusic[rng.Next(0, 2)]);

			//set the room layouts
			roomType1X = new float[] { UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6)};
			roomType1Y = new float[] { BORDER_OFFSET,
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET };
			roomType2X = new float[] { UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6)};
			roomType2Y = new float[] { BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4)};
			roomType3X = new float[] { UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4) };
			roomType3Y = new float[] { BORDER_OFFSET,
									   BORDER_OFFSET,
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET,
									   BORDER_OFFSET,
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4) };
			roomType4X = new float[] { UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET,
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 6),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4) };
			roomType4Y = new float[] { BORDER_OFFSET,
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET,
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 6),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4) };
			roomType5X = new float[] { UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 5),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 2),
									   UI_OFFSET + BORDER_OFFSET + (rock1Img.Width * 2 * 4) };
			roomType5Y = new float[] { BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 5),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 2),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4),
									   BORDER_OFFSET + (rock1Img.Height * 2 * 4) };
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			//get the state and previous state of the keyboard
			kb = Keyboard.GetState();
			prevMouse = mouse;

			//get the state of the mouse
			mouse = Mouse.GetState();

			//if the - or + buttons are pressed, lower and raise the volume respectively
			if (kb.IsKeyDown(Keys.OemPlus) && MediaPlayer.Volume < 1)
			{
				//increase the volume of the music
				MediaPlayer.Volume += 0.005f;
			}
			else if (kb.IsKeyDown(Keys.OemMinus) && MediaPlayer.Volume > 0)
			{
				//decrease the volume of the music
				MediaPlayer.Volume -= 0.005f;
			}

			//if the - or + buttons are pressed, lower and raise the volume respectively
			if (kb.IsKeyDown(Keys.P) && SoundEffect.MasterVolume < 1 - 0.005)
			{
				//increase the volume of the sound effects
				SoundEffect.MasterVolume += 0.005f;
			}
			else if (kb.IsKeyDown(Keys.O) && SoundEffect.MasterVolume > 0.005)
			{
				//decrease the volume of the sound effects
				SoundEffect.MasterVolume -= 0.005f;
			}

			// TODO: Add your update logic here
			switch (gameState)
			{
				case MENU:
					//update the menu
					UpdateMenu();
					break;
				case INSTRUCTIONS:
					//update the instructions
					UpdateInstructions();
					break;
				case PREGAME:
					//update the pregame
					UpdatePregame();
					break;
				case GAME:
					//update the game
					UpdateGame(gameTime);
					break;
				case UPGRADE:
					//update the upgrade screen
					UpdateUpgrade();
					break;
				case ENDGAME:
					//update the endgame
					UpdateEndGame();
					break;
			}

			base.Update(gameTime);
		}

		//Pre: nothing
		//Post: nothing
		//Desc: update the logic of the menu, what buttons are pressed
		private void UpdateMenu()
		{
			//make the buttons glow if the mouse is over them
			startBtn = startBtnRec.Contains(mouse.Position);
			intsBtn = intsBtnRec.Contains(mouse.Position);

			//play a song if not already playing
			if (MediaPlayer.State != MediaState.Playing)
			{
				//play a random song
				MediaPlayer.Play(menuMusic[rng.Next(0, 2)]);
			}

			//if the mouse is clicked and over a button, go to that gamestate
			if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
			{
				//if the mouse is in a button, go to that gamestate
				if (startBtnRec.Contains(mouse.Position))
				{
					//go to pregame and the game
					gameState = PREGAME;
				}
				else if (intsBtnRec.Contains(mouse.Position))
				{
					//go to the instructions
					gameState = INSTRUCTIONS;
				}
			}
		}

		//Pre: nothing
		//Post: nothing
		//Desc: allow the user to go back to the menu after reading the instructions
		private void UpdateInstructions()
		{
			if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
			{
				gameState = MENU;
			}
		}

		//Pre: nothing
		//Post: nothing
		//Desc: set up everything for the game
		private void UpdatePregame()
		{
			//set the current round to 1
			currentRound = 1;

			//reset the score
			score = 0;

			//set the healths according to the character
			playerJHealth = 5;
			playerEHealth = 4;

			//set the speeds to the base speed
			currentMaxSpeed = BASE_SPEED;
			maxDiagonalSpeed = (float)Math.Sqrt(currentMaxSpeed);

			//set that the user has no item 
			currentPlayerItem = NO_ITEM;

			//set no multishot stored
			storedMultiShot = 0;

			//set no starshot stored
			storedStarShot = 0;

			//set no modifier for the firerate
			fireRateMultiplier = 1;

			//hide the ui icons
			UICoffeeAnim.destRec.X = (int)HIDE_OBJECTS_LOCATION;
			UICoffeeAnim.destRec.Y = (int)HIDE_OBJECTS_LOCATION;
			UIBubbleRec.X = (int)HIDE_OBJECTS_LOCATION;
			UIBubbleRec.Y = (int)HIDE_OBJECTS_LOCATION;
			UIHeartItemRec.X = (int)HIDE_OBJECTS_LOCATION;
			UIHeartItemRec.Y = (int)HIDE_OBJECTS_LOCATION;
			UIstarRec.X = (int)HIDE_OBJECTS_LOCATION;
			UIstarRec.Y = (int)HIDE_OBJECTS_LOCATION;

			//reset the coffee effectm timer
			coffeeEffectTimer.ResetTimer(false);

			//setting the current counts of enemies
			batEnemiesCount = 0;
			batEnemiesToSpawn = 20;
			robotEnemiesCount = 0;
			robotEnemiesToSpawn = 0;
			headEnemiesCount = 0;
			headEnemiesToSpawn = 0;
			rockEnemiesCount = 0;
			rockEnemiesToSpawn = 3;

			//setting the immunity timer to 3 seconds of base immunity
			playerImmunityTimer = new Timer(3000, true);

			//set the firerate timer to the firerate of j
			playerFireRateTimer = new Timer(BASE_FIRE_RATE_J, true);
			
			//make the current character j
			isJActive = true;

			//setting things also set while transistioning in the game
			SoftReset();

			//hide all of character e's anims offscreen
			for (int i = 0; i < 4; i++)
			{
				//move the current animation offscreen
				mainCharEAnim[i].destRec.X = (int)HIDE_OBJECTS_LOCATION;
				mainCharEAnim[i].destRec.Y = (int)HIDE_OBJECTS_LOCATION;
			}

			//set the values for melee enemies
			for (int i = 0; i < MAX_HITTERS; i++)
			{
				//hide all bats
				batEnemiesLoc[i] = hideObjectsLoc;
				batEnemyAnim[i].destRec.X = (int)batEnemiesLoc[i].X;
				batEnemyAnim[i].destRec.Y = (int)batEnemiesLoc[i].Y;

				//hide all rock enemies
				rockEnemiesLoc[i] = hideObjectsLoc;
				rockEnemyDownAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyDownAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;
				rockEnemyLeftAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyLeftAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;
				rockEnemyRightAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyRightAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;
				rockEnemyUpAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyUpAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;

				//reset the healths of the rock enemies
				rockEnemyHealths[i] = 2;

				//default all rock enemy directions to down
				rockEnemyDirections[i] = DOWN;
			}

			//set the values for all shooter enemies
			for (int i = 0; i < MAX_SHOOTERS; i++)
			{
				//hide all robots
				robotEnemiesLoc[i] = hideObjectsLoc;
				robotEnemyDownAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyDownAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;
				robotEnemyLeftAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyLeftAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;
				robotEnemyRightAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyRightAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;
				robotEnemyUpAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyUpAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;

				//set all the robot healths
				robotHealths[i] = 3;

				//default all robot directions down
				robotDirections[i] = DOWN;

				//hide all head enemies
				headEnemiesLoc[i] = hideObjectsLoc;
				headEnemyDownAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyDownAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;
				headEnemyLeftAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyLeftAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;
				headEnemyRightAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyRightAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;
				headEnemyUpAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyUpAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;

				//set all head healths
				headHealths[i] = 3;

				//default all head directions down
				headDirections[i] = DOWN;
			}

			//randomize the rock pattern
			for (int i = 0; i < 20; i++)
			{
				//randomize the current rock
				RandomizeRockTypes(i);
			}

			//Lower the music volume and play game music
			MediaPlayer.Volume = 0.5f;
			MediaPlayer.Play(gameMusic[rng.Next(0, 3)]);

			//go to the game
			gameState = GAME;
		}

		//Pre: time passed since last update
		//Post: nothing
		//Desc: preform the logic of the game, updating timers and animations, moving enemies/player/bullets, spawing and deleting things, etc.
		private void UpdateGame(GameTime gameTime)
		{
			//update timers and animations
			playerFireRateTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			playerImmunityTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			batEnemiesSpawnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			robotEnemiesSpawnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			headEnemiesSpawnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			rockEnemiesSpawnTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			coffeeEffectTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			bubbleEffectTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			coffeeHideTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			bubbleHideTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			heartHideTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			starHideTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			tagDelayTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			charEPierceTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
			mainCharJAnim[charDirection].Update(gameTime);
			mainCharEAnim[charDirection].Update(gameTime);
			smokeTagAnim.Update(gameTime);
			UICoffeeAnim.Update(gameTime);
			coffeeAnim.Update(gameTime);

			//play a new rondom song if not already playing
			if (MediaPlayer.State != MediaState.Playing)
			{
				//play a random songs
				MediaPlayer.Play(gameMusic[rng.Next(0, 3)]);
			}

			//update the current bat animations
			for (int i = 0; i < batEnemiesCount; i++)
			{
				//update the current animation
				batEnemyAnim[i].Update(gameTime);
			}

			//update the current robot animations and timers depending on direction
			for (int i = 0; i < robotEnemiesCount; i++)
			{
				//update random robot shooting timer
				robotRandomShootTimer[i].Update(gameTime.ElapsedGameTime.TotalMilliseconds);

				//update the current animation depending on direction
				if (robotDirections[i] == DOWN)
				{
					//update the current animation
					robotEnemyDownAnim[i].Update(gameTime);
				}
				else if (robotDirections[i] == LEFT)
				{
					//update the current animation
					robotEnemyLeftAnim[i].Update(gameTime);
				}
				else if (robotDirections[i] == RIGHT)
				{
					//update the current animation
					robotEnemyRightAnim[i].Update(gameTime);
				}
				else if (robotDirections[i] == UP)
				{
					//update the current animation
					robotEnemyUpAnim[i].Update(gameTime);
				}
			}

			//update the current head animations and timers depending on direction
			for (int i = 0; i < headEnemiesCount; i++)
			{
				//update random head shooting timer
				headRandomShootTimer[i].Update(gameTime.ElapsedGameTime.TotalMilliseconds);

				//update the current animation depending on direction
				if (headDirections[i] == DOWN)
				{
					//update the current animation
					headEnemyDownAnim[i].Update(gameTime);
				}
				else if (headDirections[i] == LEFT)
				{
					//update the current animation
					headEnemyLeftAnim[i].Update(gameTime);
				}
				else if (headDirections[i] == RIGHT)
				{
					//update the current animation
					headEnemyRightAnim[i].Update(gameTime);
				}
				else if (headDirections[i] == UP)
				{
					//update the current animation
					headEnemyUpAnim[i].Update(gameTime);
				}
			}

			//update the current rock enemy animations depending on direction
			for (int i = 0; i < rockEnemiesCount; i++)
			{
				//update the current animation depending on direction
				if (rockEnemyDirections[i] == DOWN)
				{
					//update the current animation
					rockEnemyDownAnim[i].Update(gameTime);
				}
				else if (rockEnemyDirections[i] == LEFT)
				{
					//update the current animation
					rockEnemyLeftAnim[i].Update(gameTime);
				}
				else if (rockEnemyDirections[i] == RIGHT)
				{
					//update the current animation
					rockEnemyRightAnim[i].Update(gameTime);
				}
				else if (rockEnemyDirections[i] == UP)
				{
					//update the current animation
					rockEnemyUpAnim[i].Update(gameTime);
				}
			}

			//Move and face a direction depending on the inputs, and adjust diagonal speed accordingly
			if ((kb.IsKeyDown(Keys.W)|| kb.IsKeyDown(Keys.S)) && (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.D)))
			{
				//move and face upwards if w is pressed
				if (kb.IsKeyDown(Keys.W))
				{
					//make the character direction up
					charDirection = UP;

					//move the character up
					mainCharLoc.Y -= maxDiagonalSpeed;
				}

				//move and face downwards if s is pressed
				if (kb.IsKeyDown(Keys.S))
				{
					//make the character direction down
					charDirection = DOWN;

					//move the character down
					mainCharLoc.Y += maxDiagonalSpeed;
				}

				//move and face left if a is pressed
				if (kb.IsKeyDown(Keys.A))
				{
					//make the character direction left
					charDirection = LEFT;

					//move the character left
					mainCharLoc.X -= maxDiagonalSpeed;
				}

				//move and face right if d is pressed
				if (kb.IsKeyDown(Keys.D))
				{
					//make the character direction right
					charDirection = RIGHT;

					//move the character right
					mainCharLoc.X += maxDiagonalSpeed;
				}

				//get center vector of character
				if (isJActive == true)
				{
					//get the center vector of j
					mainCharCenterLoc.X = mainCharJAnim[charDirection].destRec.Center.X;
					mainCharCenterLoc.Y = mainCharJAnim[charDirection].destRec.Center.Y;
				}
				else
				{
					//get the center vector of e
					mainCharCenterLoc.X = mainCharEAnim[charDirection].destRec.Center.X;
					mainCharCenterLoc.Y = mainCharEAnim[charDirection].destRec.Center.Y;
				}
			}
			else
			{
				//move and face upwards if w is pressed
				if (kb.IsKeyDown(Keys.W))
				{
					//make the character direction up
					charDirection = UP;

					//move the character up
					mainCharLoc.Y -= currentMaxSpeed;
				}

				//move and face downwards if s is pressed
				if (kb.IsKeyDown(Keys.S))
				{
					//make the character direction down
					charDirection = DOWN;

					//move the character down
					mainCharLoc.Y += currentMaxSpeed;
				}

				//move and face left if a is pressed
				if (kb.IsKeyDown(Keys.A))
				{
					//make the character direction left
					charDirection = LEFT;

					//move the character left
					mainCharLoc.X -= currentMaxSpeed;
				}

				//move and face right if d is pressed
				if (kb.IsKeyDown(Keys.D))
				{
					//make the character direction right
					charDirection = RIGHT;

					//move the character right
					mainCharLoc.X += currentMaxSpeed;
				}

				//get center vector of the current character
				if (isJActive == true)
				{
					//get the center vector of j
					mainCharCenterLoc.X = mainCharJAnim[charDirection].destRec.Center.X;
					mainCharCenterLoc.Y = mainCharJAnim[charDirection].destRec.Center.Y;
				}
				else
				{
					//get the center vector of e
					mainCharCenterLoc.X = mainCharEAnim[charDirection].destRec.Center.X;
					mainCharCenterLoc.Y = mainCharEAnim[charDirection].destRec.Center.Y;
				}
			}

			//move the animations to the character vector
			for (int i = 0; i < 4; i++)
			{
				//move the current charater animations to the character vector
				if (isJActive == true)
				{
					//move the current charater j's current animation to the character vector
					mainCharJAnim[i].destRec.X = (int)mainCharLoc.X;
					mainCharJAnim[i].destRec.Y = (int)mainCharLoc.Y;
				}
				else
				{
					//move the current charater e's current animation to the character vector
					mainCharEAnim[i].destRec.X = (int)mainCharLoc.X;
					mainCharEAnim[i].destRec.Y = (int)mainCharLoc.Y;
				}
			}

			//if the character is touching the border, move it back
			if ((mainCharJAnim[charDirection].destRec.X + mainCharJAnim[charDirection].destRec.Width >= screenWidth - BORDER_OFFSET - UI_OFFSET && isJActive == true) || (mainCharEAnim[charDirection].destRec.X + mainCharEAnim[charDirection].destRec.Width >= screenWidth - BORDER_OFFSET - UI_OFFSET && isJActive == false))
			{
				//move the character away from the border
				mainCharLoc.X -= currentMaxSpeed;
			}

			//if the character is touching the border, move it back
			if ((mainCharJAnim[charDirection].destRec.X <= BORDER_OFFSET + UI_OFFSET && isJActive == true) || (mainCharEAnim[charDirection].destRec.X <= BORDER_OFFSET + UI_OFFSET && isJActive == false))
			{
				//move the character away from the border
				mainCharLoc.X += currentMaxSpeed;
			}

			//if the character is touching the border, move it back
			if ((mainCharJAnim[charDirection].destRec.Y + mainCharJAnim[charDirection].destRec.Height >= screenHeight - BORDER_OFFSET + UI_ICON_OFFSET && isJActive == true) || (mainCharEAnim[charDirection].destRec.Y + mainCharEAnim[charDirection].destRec.Height >= screenHeight - BORDER_OFFSET + UI_ICON_OFFSET && isJActive == false))
			{
				//move the character away from the border
				mainCharLoc.Y -= currentMaxSpeed;
			}

			//if the character is touching the border, move it back
			if ((mainCharJAnim[charDirection].destRec.Y <= BORDER_OFFSET && isJActive == true) || (mainCharEAnim[charDirection].destRec.Y <= BORDER_OFFSET && isJActive == false))
			{
				//move the character away from the border
				mainCharLoc.Y += currentMaxSpeed;
			}

			//check for player collision with rocks and stop the player from moving into the rock with some leniency
			for (int i = 0; i < numRocks; i++)
			{
				//if the character is intersecting the rock, move them back accordingly
				if ((mainCharJAnim[charDirection].destRec.Intersects(rocksRec[i]) && isJActive == true) || (mainCharEAnim[charDirection].destRec.Intersects(rocksRec[i]) && isJActive == false))
				{
					//get the distance from the center of the character to the rock
					charDistFromRockX = mainCharCenterLoc.X - rocksRec[i].Center.X;
					charDistFromRockY = mainCharCenterLoc.Y - rocksRec[i].Center.Y;

					//if the player is close enough to the rock and the distance of the rock horizontally is more than the vertical, move it accordingly on the x-axis
					if (charDistFromRockX < rocksRec[i].Width / 2 && charDistFromRockX > 0 && Math.Abs(charDistFromRockX) > Math.Abs(charDistFromRockY))
					{
						//move the player away from the rock
						mainCharLoc.X+=currentMaxSpeed;
					}
					else if (charDistFromRockX > -rocksRec[i].Width / 2 && charDistFromRockX < 0 && Math.Abs(charDistFromRockX) > Math.Abs(charDistFromRockY))
					{
						//move the player away from the rock
						mainCharLoc.X-=currentMaxSpeed; 
					}

					//if the player is close enough to the rock and the distance of the rock vertically is more than the horizontal, move it accordingly on the y-axis
					if (charDistFromRockY < rocksRec[i].Height / 2 && charDistFromRockY > 0 && Math.Abs(charDistFromRockX) < Math.Abs(charDistFromRockY))
					{
						//move the player away from the rock
						mainCharLoc.Y+=currentMaxSpeed;
					}
					else if (charDistFromRockY > -rocksRec[i].Height / 2 && charDistFromRockY < 0 && Math.Abs(charDistFromRockX) < Math.Abs(charDistFromRockY))
					{
						//move the player away from the rock
						mainCharLoc.Y-=currentMaxSpeed;
					}
				}
			}

			//if the mouse is pressed and the firerate cooldown timer is over, fire a bullet
			if (mouse.LeftButton == ButtonState.Pressed && playerFireRateTimer.IsFinished() && storedStarShot <= 0)
			{
				//fire a bullet to the mouse position
				PlayerShooting();

				//play the shooting sound based on the character
				if (isJActive == true)
				{
					//play the gunshot sound
					gunSnd.CreateInstance().Play();
				}
				else
				{
					//play the bow sound
					bowSnd.CreateInstance().Play();
				}
			}

			//when e is pressed and the timer is over and healths are more than 0, tag in/out the other character and dsplay and hide the smoke animation
			if ((kb.IsKeyDown(Keys.E) && tagDelayTimer.IsFinished() && playerEHealth > 0 && playerJHealth > 0) || (playerJHealth <= 0 && isJActive == true) || (playerEHealth <= 0 && isJActive == false))
			{
				//make the smoke animate
				smokeTagAnim.isAnimating = true;

				//reset the timer for tagging in/out
				tagDelayTimer.ResetTimer(true);
				
				//switch to the other character
				if (isJActive == true)
				{
					//move the smoke animation to the player
					smokeTagAnim.destRec.X = mainCharJAnim[charDirection].destRec.X - (smokeTagImg.Width / 3 - mainCharJImg[charDirection].Width / 4) / 2;
					smokeTagAnim.destRec.Y = mainCharJAnim[charDirection].destRec.Y - (smokeTagImg.Height / 2 - mainCharJImg[charDirection].Height) / 2;

					//hide the character being tagged out
					for (int i = 0; i < 4; i++)
					{
						//hide the current charater animation
						mainCharJAnim[i].destRec.X = (int)HIDE_OBJECTS_LOCATION;
						mainCharJAnim[i].destRec.Y = (int)HIDE_OBJECTS_LOCATION;
					}

					//switch to the new charater's firerate timer
					playerFireRateTimer = new Timer(BASE_FIRE_RATE_E * fireRateMultiplier, true);

					//mark that character j is not active
					isJActive = false;
				}
				else
				{
					//move the smoke animation to the player
					smokeTagAnim.destRec.X = mainCharEAnim[charDirection].destRec.X - (smokeTagImg.Width / 3 - mainCharEImg[charDirection].Width / 4) / 2;
					smokeTagAnim.destRec.Y = mainCharEAnim[charDirection].destRec.Y - (smokeTagImg.Height / 2 - mainCharEImg[charDirection].Height) / 2;

					//hide the character being tagged out
					for (int i = 0; i < 4; i++)
					{
						//hide the current charater animation
						mainCharEAnim[i].destRec.X = (int)HIDE_OBJECTS_LOCATION;
						mainCharEAnim[i].destRec.Y = (int)HIDE_OBJECTS_LOCATION;
					}

					//switch to the new charater's firerate timer
					playerFireRateTimer = new Timer(BASE_FIRE_RATE_J * fireRateMultiplier, true);

					//mark that character j is active
					isJActive = true;
				}
			}
			else if (smokeTagAnim.curFrame == 5)
			{
				//stop animating the smoke
				smokeTagAnim.isAnimating = false;

				//set the smoke frame to the first frame
				smokeTagAnim.curFrame = 0;

				//hide the smoke
				smokeTagAnim.destRec.X = (int)HIDE_OBJECTS_LOCATION;
				smokeTagAnim.destRec.Y = (int)HIDE_OBJECTS_LOCATION;
			}

			//if q is pressed, firerate timer is finished, and multishot is stored, shoot extra bullets
			if (kb.IsKeyDown(Keys.Q) && playerFireRateTimer.IsFinished() && storedMultiShot > 0)
			{
				//shoot a bullet towards the mouse
				PlayerShooting();

				//shoot an offset bullet
				currentMultiShotAngle = MULTISHOT_OFFSET;
				PlayerShooting();

				//shoot an offset bullet
				currentMultiShotAngle = -MULTISHOT_OFFSET;
				PlayerShooting();

				//play the gun sound and shoot extra bullets if j is active, else, play the bow sound
				if (isJActive == true)
				{
					//play the gun sound effect
					gunSnd.CreateInstance().Play();

					//shoot an offset bullet
					currentMultiShotAngle = MULTISHOT_OFFSET / 2;
					PlayerShooting();

					//shoot an offset bullet
					currentMultiShotAngle = -MULTISHOT_OFFSET / 2;
					PlayerShooting();
				}
				else
				{
					//play the bow sound effect
					bowSnd.CreateInstance().Play();
				}

				//reset the angle the bullets are shot
				currentMultiShotAngle = 0;

				//subtract one multishot
				storedMultiShot--;
			}

			//move the bubble to the player while the immunity timer is active
			if (playerImmunityTimer.IsActive())
			{
				//move the bubble to the active character
				if (isJActive == true)
				{
					//move the bubble to the character
					immunityBubbleRec.X = mainCharJAnim[charDirection].destRec.Center.X - (immunityBubbleRec.Height / 2);
					immunityBubbleRec.Y = mainCharJAnim[charDirection].destRec.Center.Y - (immunityBubbleRec.Height / 2);
				}
				else
				{
					//move the bubble to the character
					immunityBubbleRec.X = mainCharEAnim[charDirection].destRec.Center.X - (immunityBubbleRec.Height / 2);
					immunityBubbleRec.Y = mainCharEAnim[charDirection].destRec.Center.Y - (immunityBubbleRec.Height / 2);
				}
			}

			//hide the bubble if the timer is finished and reset the fade, or fade the bubble if the timer is running out, or just reset the fade if it's not reset
			if (playerImmunityTimer.IsFinished() && immunityBubbleRec.X != (int)HIDE_OBJECTS_LOCATION)
			{
				//hide the bubble
				immunityBubbleRec.X = (int)HIDE_OBJECTS_LOCATION;
				immunityBubbleRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//reset the fade for the bubble
				immunityBubbleFade = 1;
			}
			else if (playerImmunityTimer.GetTimeRemaining() < 1000 && !playerImmunityTimer.IsFinished())
			{
				//fade the bubble
				immunityBubbleFade -= FADE_FACTOR;
			}
			else if (immunityBubbleFade != 1)
			{
				//reset the fade for the bubble
				immunityBubbleFade = 1;
			}

			//if the user touches an item, set that to the current item or do it's affect
			if (isCoffeeInRoom == true && (mainCharJAnim[charDirection].destRec.Intersects(coffeeAnim.destRec) || mainCharEAnim[charDirection].destRec.Intersects(coffeeAnim.destRec)))
			{
				//make the player's held item a coffee unless they already have an item, and activate the effect on the spot
				if (currentPlayerItem == NO_ITEM)
				{
					//make the held item a coffee
					currentPlayerItem = COFFEE;

					//move the coffee into position
					UICoffeeAnim.destRec.X = (int)UIItemLoc.X;
					UICoffeeAnim.destRec.Y = (int)UIItemLoc.Y;
				}
				else
				{
					//apply coffee speed affect
					CoffeeActivation();
				}

				//reset the timer that hides the coffee item
				coffeeHideTimer.ResetTimer(false);

				//hide the coffee
				coffeeAnim.destRec.X = (int)HIDE_OBJECTS_LOCATION;
				coffeeAnim.destRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//indicate that the item is no longer on the floor
				isCoffeeInRoom = false;
			}
			else if (isBubbleInRoom == true && (mainCharJAnim[charDirection].destRec.Intersects(bubbleRec) || mainCharEAnim[charDirection].destRec.Intersects(bubbleRec)))
			{
				//make the player's held item a bubble unless they already have an item, and activate the effect on the spot
				if (currentPlayerItem == NO_ITEM)
				{
					//make the held item a bubble
					currentPlayerItem = BUBBLE;

					//move the ui bubble into position
					UIBubbleRec.X = (int)UIItemLoc.X;
					UIBubbleRec.Y = (int)UIItemLoc.Y;
				}
				else
				{
					//activate the bubble item
					bubbleEffectTimer.ResetTimer(true);
				}

				//reset the timer that hides the bubble item
				bubbleHideTimer.ResetTimer(false);

				//hide the bubble
				bubbleRec.X = (int)HIDE_OBJECTS_LOCATION;
				bubbleRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//reset the fade on the bubble
				bubbleFade = 1;

				//indicate that the item is no longer on the floor
				isBubbleInRoom = false;
			}
			else if (isHeartInRoom == true && (mainCharJAnim[charDirection].destRec.Intersects(heartItemRec) || mainCharEAnim[charDirection].destRec.Intersects(heartItemRec)))
			{
				//make the player's held item a heart unless they already have an item, and activate the effect on the spot
				if (currentPlayerItem == NO_ITEM)
				{
					//make the held item a heart
					currentPlayerItem = HEART;

					//move the ui heart item into position
					UIHeartItemRec.X = (int)UIItemLoc.X;
					UIHeartItemRec.Y = (int)UIItemLoc.Y;
				}
				else
				{
					//give one heart to each character unless both are full, then apply invincibity
					HeartActivation();
				}

				//reset the timer that hides the heart item
				heartHideTimer.ResetTimer(false);

				//hide the heart item
				heartItemRec.X = (int)HIDE_OBJECTS_LOCATION;
				heartItemRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//indicate that the item is no longer on the floor
				isHeartInRoom = false;
			}
			else if (isStarInRoom == true && (mainCharJAnim[charDirection].destRec.Intersects(starRec) || mainCharEAnim[charDirection].destRec.Intersects(starRec)))
			{
				//make the player's held item a star unless they already have an item, and activate the effect on the spot
				if (currentPlayerItem == NO_ITEM)
				{
					//make the held item a star
					currentPlayerItem = STAR;

					//move the ui star into position
					UIstarRec.X = (int)UIItemLoc.X;
					UIstarRec.Y = (int)UIItemLoc.Y;
				}
				else
				{
					//add starshot
					storedStarShot += 5;
				}

				//reset the timer that hides the star item
				starHideTimer.ResetTimer(false);

				//hide the star
				starRec.X = (int)HIDE_OBJECTS_LOCATION;
				starRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//indicate that the item is no longer on the floor
				isStarInRoom = false;
			}

			//reset the coffee hide timer, fade, hide it, and unfade it if the timer is over, or fade it if the time is running out
			if (coffeeHideTimer.IsFinished())
			{
				//reset the timer that hides the coffee item after a timer
				coffeeHideTimer.ResetTimer(false);

				//unfade the coffee
				coffeeFade = 1;

				//hide the coffee item
				coffeeAnim.destRec.X = (int)HIDE_OBJECTS_LOCATION;
				coffeeAnim.destRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//indicate that the coffee is no longer in the room
				isCoffeeInRoom = false;
			}
			else if (coffeeHideTimer.GetTimeRemaining() < 1000 && coffeeHideTimer.IsActive())
			{
				//fade the coffee
				coffeeFade -= FADE_FACTOR;
			}

			//reset the bubble hide timer, fade, hide it, and unfade it if the timer is over, or fade it if the time is running out
			if (bubbleHideTimer.IsFinished())
			{
				//reset the timer that hides the bubble item after a timer
				bubbleHideTimer.ResetTimer(false);

				//unfade the bubble
				bubbleFade = 1;

				//hide the bubble item
				bubbleRec.X = (int)HIDE_OBJECTS_LOCATION;
				bubbleRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//indicate that the bubble is no longer in the room
				isBubbleInRoom = false;
			}
			else if (bubbleHideTimer.GetTimeRemaining() < 1000 && bubbleHideTimer.IsActive())
			{
				//fade the bubble
				bubbleFade -= FADE_FACTOR;
			}

			//reset the heart hide timer, fade, hide it, and unfade it if the timer is over, or fade it if the time is running out
			if (heartHideTimer.IsFinished())
			{
				//reset the timer that hides the heart item after a timer
				heartHideTimer.ResetTimer(false);

				//unfade the heart
				heartFade = 1;

				//hide the heart item
				heartItemRec.X = (int)HIDE_OBJECTS_LOCATION;
				heartItemRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//indicate that the heart is no longer in the room
				isHeartInRoom = false;
			}
			else if (heartHideTimer.GetTimeRemaining() < 1000 && heartHideTimer.IsActive())
			{
				//fade the heart
				heartFade -= FADE_FACTOR;
			}

			//reset the star hide timer, fade, hide it, and unfade it if the timer is over, or fade it if the time is running out
			if (starHideTimer.IsFinished())
			{
				//reset the timer that hides the star item after a timer
				starHideTimer.ResetTimer(false);

				//unfade the star
				starFade = 1;

				//hide the star item
				starRec.X = (int)HIDE_OBJECTS_LOCATION;
				starRec.Y = (int)HIDE_OBJECTS_LOCATION;

				//indicate that the star is no longer in the room
				isStarInRoom = false;
			}
			else if (starHideTimer.GetTimeRemaining() < 1000 && starHideTimer.IsActive())
			{
				//fade the star
				starFade -= FADE_FACTOR;
			}

			//when the effect timer is over, reduce the player speed, reset the timer
			if (coffeeEffectTimer.IsFinished())
			{
				//reset the coffee effect timer
				coffeeEffectTimer.ResetTimer(false);

				//reduce the player speed and update the diagonal speed
				currentMaxSpeed -= 1;
				maxDiagonalSpeed = (float)Math.Sqrt(currentMaxSpeed);
			}

			//reset the immunity timer if the bubble timer is active
			if (bubbleEffectTimer.IsActive())
			{
				//reset the immunity timer
				playerImmunityTimer.ResetTimer(true);
			}

			//if the player has starshot and the mouse is pressed and the firerate timer is finished, shoot a ring of bullets
			if (storedStarShot > 0 && mouse.LeftButton == ButtonState.Pressed && playerFireRateTimer.IsFinished())
			{
				//shoot offset bullets in a ring/star
				PlayerShooting();
				currentMultiShotAngle = STARSHOT_OFFSET;
				PlayerShooting();
				currentMultiShotAngle = STARSHOT_OFFSET * 2;
				PlayerShooting();
				currentMultiShotAngle = STARSHOT_OFFSET * 3;
				PlayerShooting();
				currentMultiShotAngle = STARSHOT_OFFSET * 4;
				PlayerShooting();
				currentMultiShotAngle = STARSHOT_OFFSET * 5;
				PlayerShooting();
				currentMultiShotAngle = STARSHOT_OFFSET * 6;
				PlayerShooting();
				currentMultiShotAngle = STARSHOT_OFFSET * 7;
				PlayerShooting();

				//if character j is active, shoot more bullets and play the gun sound, else, play the bow sound
				if (isJActive == true)
				{
					//play the gun sound effect
					gunSnd.CreateInstance().Play();

					//shoot more offset bullets in a ring/star
					currentMultiShotAngle = MULTISHOT_OFFSET;
					PlayerShooting();
					currentMultiShotAngle = MULTISHOT_OFFSET + STARSHOT_OFFSET;
					PlayerShooting();
					currentMultiShotAngle = MULTISHOT_OFFSET + STARSHOT_OFFSET * 2;
					PlayerShooting();
					currentMultiShotAngle = MULTISHOT_OFFSET + STARSHOT_OFFSET * 3;
					PlayerShooting();
					currentMultiShotAngle = MULTISHOT_OFFSET + STARSHOT_OFFSET * 4;
					PlayerShooting();
					currentMultiShotAngle = MULTISHOT_OFFSET + STARSHOT_OFFSET * 5;
					PlayerShooting();
					currentMultiShotAngle = MULTISHOT_OFFSET + STARSHOT_OFFSET * 6;
					PlayerShooting();
					currentMultiShotAngle = MULTISHOT_OFFSET + STARSHOT_OFFSET * 7;
					PlayerShooting();
				}
				else
				{
					//play the bow sound effect
					bowSnd.CreateInstance().Play();
				}

				//reset the shooting angle
				currentMultiShotAngle = 0;

				//reduce the amount of starshot left
				storedStarShot--;
			}

			//if the space bar is pressed, use the held item
			if (kb.IsKeyDown(Keys.Space))
			{
				//do the effect of the current item held
				switch (currentPlayerItem)
				{
					//if the item is a coffee, hide the ui image and apply the coffee speed effect
					case COFFEE:
						//apply the coffee speed effect
						CoffeeActivation();

						//hide the ui image
						UICoffeeAnim.destRec.X = (int)HIDE_OBJECTS_LOCATION;
						UICoffeeAnim.destRec.Y = (int)HIDE_OBJECTS_LOCATION;
						break;
					//if the item is a bubble, hide the ui image and apply the bubble immunity effect
					case BUBBLE:
						//apply the bubble immunity effect
						bubbleEffectTimer.ResetTimer(true);

						//hide the ui image
						UIBubbleRec.X = (int)HIDE_OBJECTS_LOCATION;
						UIBubbleRec.Y = (int)HIDE_OBJECTS_LOCATION;
						break;
					//if the item is a heart, hide the ui image and give health to the characters, or apply the bubble immunity effect
					case HEART:
						//give health to the characters if possible, if not, apply the bubble immunity effect
						HeartActivation();

						//hide the ui image
						UIHeartItemRec.X = (int)HIDE_OBJECTS_LOCATION;
						UIHeartItemRec.Y = (int)HIDE_OBJECTS_LOCATION;
						break;
					//if the item is a star, hide the ui image and apply the starshot effect
					case STAR:
						//increase the amount of starshot
						storedStarShot += 5;

						//hide the ui image
						UIstarRec.X = (int)HIDE_OBJECTS_LOCATION;
						UIstarRec.Y = (int)HIDE_OBJECTS_LOCATION;
						break;
				}

				//make the player hold no item
				currentPlayerItem = NO_ITEM;
			}
			
			//update the player bullet locations and check for collisions
			for (int i = 0; i < playerBulletsCount; i++)
			{
				//update the current player bullet location
				playerBulletsLoc[i].X += playerBulletSpeedX[i];
				playerBulletsLoc[i].Y += playerBulletSpeedY[i];
				playerBulletsRec[i].X = (int)playerBulletsLoc[i].X;
				playerBulletsRec[i].Y = (int)playerBulletsLoc[i].Y;

				//detect if any of the bullets and bats intersect
				for (int j = 0; j < batEnemiesCount; j++)
				{
					//detect if the bullet and enemy intersect and "kill" it
					if (batEnemyAnim[j].destRec.Intersects(playerBulletsRec[i]) && charEPierceTimer.IsFinished())
					{
						//play the sand sound
						sandSnd.CreateInstance().Play();

						//"kill" the enemy and do other logic surrounding it
						batEnemiesLoc[j] = EnemyDeath(batEnemiesLoc, i, j, batEnemiesCount);

						//decrease the enemy count
						batEnemiesCount--;

						//add score
						score += 50;
					}
				}

				//detect if any of the bullets and robots intersect
				for (int j = 0; j < robotEnemiesCount; j++)
				{
					//detect if the bullet and enemy intersect and deal damage to it and "kill" it if the health is 0
					if (robotEnemyDownAnim[j].destRec.Intersects(playerBulletsRec[i]) && charEPierceTimer.IsFinished())
					{
						//reduce the health of the robot
						robotHealths[j]--;

						//"kill" the robot if the health is 0
						if (robotHealths[j] <= 0)
						{
							//play the metal sound
							metalSnd.CreateInstance().Play();

							//"kill" the enemy and do other logic surrounding it
							robotEnemiesLoc[j] = EnemyDeath(robotEnemiesLoc, i, j, robotEnemiesCount);
							robotHealths[j] = robotHealths[j + 1];

							//decrease the enemy count
							robotEnemiesCount--;

							//add score
							score += 175;

							//delete the current bullet if it belongs to e
							if (isBulletOfJ[i] == false)
							{
								//delete the current bullet
								DeleteBullet(i);
							}
						}
						else
						{
							//delete the current bullet
							DeleteBullet(i);
						}
					}
				}

				//detect if any of the bullets and heads intersect
				for (int j = 0; j < headEnemiesCount; j++)
				{
					//detect if the bullet and enemy intersect and deal damage to it and "kill" it if the health is 0
					if (headEnemyDownAnim[j].destRec.Intersects(playerBulletsRec[i]) && charEPierceTimer.IsFinished())
					{
						//reduce the health of the head
						headHealths[j]--;

						//"kill" the head if the health is 0
						if (headHealths[j] <= 0)
						{
							//play the sand sound
							sandSnd.CreateInstance().Play();

							//"kill" the enemy and do other logic surrounding it
							headEnemiesLoc[j] = EnemyDeath(headEnemiesLoc, i, j, headEnemiesCount);
							headHealths[j] = headHealths[j + 1];

							//decrease the enemy count
							headEnemiesCount--;

							//add score
							score += 150;

							//delete the current bullet if it belongs to e
							if (isBulletOfJ[i] == false)
							{
								//delete the current bullet
								DeleteBullet(i);
							}
						}
						else
						{
							//delete the current bullet
							DeleteBullet(i);
						}
					}
				}

				//detect if any of the bullets and rock enemies intersect
				for (int j = 0; j < rockEnemiesCount; j++)
				{
					//detect if the bullet and enemy intersect and deal damage to it and "kill" it if the health is 0
					if (rockEnemyDownAnim[j].destRec.Intersects(playerBulletsRec[i]) && charEPierceTimer.IsFinished())
					{
						//reduce the health of the rock enemy
						rockEnemyHealths[j]--;

						//"kill" the rock if the health is 0
						if (rockEnemyHealths[j] <= 0)
						{
							//play the metal sound
							metalSnd.CreateInstance().Play();

							//"kill" the enemy and do other logic surrounding it
							rockEnemiesLoc[j] = EnemyDeath(rockEnemiesLoc, i, j, rockEnemiesCount);
							rockEnemyHealths[j] = rockEnemyHealths[j + 1];

							//decrease the enemy count
							rockEnemiesCount--;

							//add score
							score += 75;

							//delete the current bullet if it belongs to e
							if (isBulletOfJ[i] == false)
							{
								//delete the current bullet
								DeleteBullet(i);
							}
						}
						else
						{
							//delete the current bullet
							DeleteBullet(i);
						}
					}
				}

				//if the player isnt being hidden and is not toucking the floor, delete it
				if (playerBulletsRec[i].X != (int)HIDE_OBJECTS_LOCATION && !playerBulletsRec[i].Intersects(floorBkRec))
				{
					//delete the current bullet
					DeleteBullet(i);
				}
			}

			//spawn bats if not enough have been spawned and the timer is over
			if (batEnemiesSpawned < batEnemiesToSpawn && batEnemiesSpawnTimer.IsFinished())
			{
				//spawn the bat at one of the 4 doors
				batEnemiesLoc[batEnemiesCount] = EnemySpawning();
				batEnemyAnim[batEnemiesCount].destRec.X = (int)batEnemiesLoc[batEnemiesCount].X;
				batEnemyAnim[batEnemiesCount].destRec.Y = (int)batEnemiesLoc[batEnemiesCount].Y;

				//increase the count of bats and amount spawned
				batEnemiesCount++;
				batEnemiesSpawned++;

				//reset the spawn timer
				batEnemiesSpawnTimer.ResetTimer(true);
			}

			//if any bats intersect the player, lower the current character health and reset the immunity timer, and move the bats towards the player
			for (int i = 0; i < batEnemiesCount; i++)
			{
				//if any bats intersect the player, lower the current character health and reset the immunity timer
				if (batEnemyAnim[i].destRec.Intersects(mainCharJAnim[charDirection].destRec) && playerImmunityTimer.IsFinished())
				{
					//lower character j health
					playerJHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);
				}
				else if (batEnemyAnim[i].destRec.Intersects(mainCharEAnim[charDirection].destRec) && playerImmunityTimer.IsFinished())
				{
					//lower character e health
					playerEHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);
				}

				//move the bats towards the player
				batEnemiesLoc[i] = MovingToPlayer(batEnemiesLoc[i], batEnemyAnim[i]);
				batEnemyAnim[i].destRec.X = (int)batEnemiesLoc[i].X;
				batEnemyAnim[i].destRec.Y = (int)batEnemiesLoc[i].Y;
			}

			//spawn a robot once the spawn timer is over and reset it 
			if (robotEnemiesSpawned < robotEnemiesToSpawn && robotEnemiesSpawnTimer.IsFinished())
			{
				//spawn a robot at one of the four spawns
				robotEnemiesLoc[robotEnemiesCount] = EnemySpawning();

				//move the animation to the vector
				robotEnemyDownAnim[robotEnemiesCount].destRec.X = (int)robotEnemiesLoc[robotEnemiesCount].X;
				robotEnemyDownAnim[robotEnemiesCount].destRec.Y = (int)robotEnemiesLoc[robotEnemiesCount].Y;
				robotEnemyLeftAnim[robotEnemiesCount].destRec.X = (int)robotEnemiesLoc[robotEnemiesCount].X;
				robotEnemyLeftAnim[robotEnemiesCount].destRec.Y = (int)robotEnemiesLoc[robotEnemiesCount].Y;
				robotEnemyRightAnim[robotEnemiesCount].destRec.X = (int)robotEnemiesLoc[robotEnemiesCount].X;
				robotEnemyRightAnim[robotEnemiesCount].destRec.Y = (int)robotEnemiesLoc[robotEnemiesCount].Y;
				robotEnemyUpAnim[robotEnemiesCount].destRec.X = (int)robotEnemiesLoc[robotEnemiesCount].X;
				robotEnemyUpAnim[robotEnemiesCount].destRec.Y = (int)robotEnemiesLoc[robotEnemiesCount].Y;

				//give the robot a timer to randomly shoot
				robotRandomShootTimer[robotEnemiesCount] = new Timer(rng.Next(5000, 7501), true);

				//set the robot's health
				robotHealths[robotEnemiesCount] = 3;

				//increase the robot count and amount spawned
				robotEnemiesCount++;
				robotEnemiesSpawned++;

				//reset the spawn timer
				robotEnemiesSpawnTimer.ResetTimer(true);
			}

			//update the robot positions and spawn bullets from them once the timer is over 
			for (int i = 0; i < robotEnemiesCount; i++)
			{
				//calculate the distance of the center of the player to the robot
				charDistFromRobotX = mainCharCenterLoc.X - robotEnemyDownAnim[i].destRec.Center.X;
				charDistFromRobotY = mainCharCenterLoc.Y - robotEnemyDownAnim[i].destRec.Center.Y;

				//make the robot stay a certain distance away from the player, but move if necessary
				if ((charDistFromRobotX < -200 || charDistFromRobotX > 200) || charDistFromRobotY < -50 || charDistFromRobotY > 50)
				{
					//move along the x-axis and face the direction depending on the character's position to the robot
					if (charDistFromRobotX < 0)
					{
						//move the robot
						robotEnemiesLoc[i].X -= 1;

						//face the robot left
						robotDirections[i] = LEFT;
					}
					else if (charDistFromRobotX > 0)
					{
						//move the robot
						robotEnemiesLoc[i].X += 1;

						//face the robot right
						robotDirections[i] = RIGHT;
					}
				}

				//make the robot stay a certain distance away from the player, but move if necessary
				if ((charDistFromRobotY < -200 || charDistFromRobotY > 200) || charDistFromRobotX < -50 || charDistFromRobotX > 50)
				{
					//move along the y-axis and face the direction depending on the character's position to the robot
					if (charDistFromRobotY < 0)
					{
						//move the robot
						robotEnemiesLoc[i].Y -= 1;

						//face the robot up
						robotDirections[i] = UP;
					}
					else if (charDistFromRobotY > 0)
					{
						//move the robot
						robotEnemiesLoc[i].Y += 1;

						//face the robot down
						robotDirections[i] = DOWN;
					}
				}

				//move the animations to the vector
				robotEnemyDownAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyDownAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;
				robotEnemyLeftAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyLeftAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;
				robotEnemyRightAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyRightAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;
				robotEnemyUpAnim[i].destRec.X = (int)robotEnemiesLoc[i].X;
				robotEnemyUpAnim[i].destRec.Y = (int)robotEnemiesLoc[i].Y;

				//shoot once the timer is finished
				if (robotRandomShootTimer[i].IsFinished())
				{
					//move the bullets to the robot
					robotBulletsRec[numRobotBullets].X = robotEnemyDownAnim[i].destRec.Center.X;
					robotBulletsRec[numRobotBullets].Y = robotEnemyDownAnim[i].destRec.Center.Y;
					robotBulletsRec[numRobotBullets + 1].X = robotEnemyDownAnim[i].destRec.Center.X;
					robotBulletsRec[numRobotBullets + 1].Y = robotEnemyDownAnim[i].destRec.Center.Y;
					robotBulletsRec[numRobotBullets + 2].X = robotEnemyDownAnim[i].destRec.Center.X;
					robotBulletsRec[numRobotBullets + 2].Y = robotEnemyDownAnim[i].destRec.Center.Y;
					robotBulletsRec[numRobotBullets + 3].X = robotEnemyDownAnim[i].destRec.Center.X;
					robotBulletsRec[numRobotBullets + 3].Y = robotEnemyDownAnim[i].destRec.Center.Y;

					//set the speed and direction of the bullets
					robotBulletSpeedX[numRobotBullets] = 0;
					robotBulletSpeedY[numRobotBullets] = 1;
					robotBulletSpeedX[numRobotBullets + 1] = 0;
					robotBulletSpeedY[numRobotBullets + 1] = -1;
					robotBulletSpeedX[numRobotBullets + 2] = -1;
					robotBulletSpeedY[numRobotBullets + 2] = 0;
					robotBulletSpeedX[numRobotBullets + 3] = 1;
					robotBulletSpeedY[numRobotBullets + 3] = 0;

					//increase the number of robot bullets
					numRobotBullets += 4;

					//reset the random shooting timer
					robotRandomShootTimer[i] = new Timer(rng.Next(5000, 7501), true);
				}
			}

			//move the bullets and detect collision
			for (int i = 0; i < numRobotBullets; i++)
			{
				//move the bullet
				robotBulletsRec[i].X += robotBulletSpeedX[i];
				robotBulletsRec[i].Y += robotBulletSpeedY[i];

				//if the bullet intersects the player, reduce health and delete, and just delete if it touches the walls
				if (robotBulletsRec[i].Intersects(mainCharJAnim[charDirection].destRec) && isJActive == true && playerImmunityTimer.IsFinished())
				{
					//lower health of j
					playerJHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);

					//delete the current bullet of the robot
					DeleteRobotBullet(i);
				}
				else if (robotBulletsRec[i].Intersects(mainCharEAnim[charDirection].destRec) && isJActive == false && playerImmunityTimer.IsFinished())
				{
					//lower health of e
					playerEHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);

					//delete the current bullet of the robot
					DeleteRobotBullet(i);
				}
				else if (!robotBulletsRec[i].Intersects(floorBkRec) || robotBulletsRec[i].Intersects(mainCharJAnim[charDirection].destRec) || robotBulletsRec[i].Intersects(mainCharEAnim[charDirection].destRec))
				{
					//delete the current bullet of the robot
					DeleteRobotBullet(i);
				}
			}

			//spawning the floating head enemies when they can be spawned and the timer is over 
			if (headEnemiesSpawned < headEnemiesToSpawn && headEnemiesSpawnTimer.IsFinished())
			{
				//spawn a head at one of the four spawns
				headEnemiesLoc[headEnemiesCount] = EnemySpawning();

				//move the animations to the vector
				headEnemyDownAnim[headEnemiesCount].destRec.X = (int)headEnemiesLoc[headEnemiesCount].X;
				headEnemyDownAnim[headEnemiesCount].destRec.Y = (int)headEnemiesLoc[headEnemiesCount].Y;
				headEnemyLeftAnim[headEnemiesCount].destRec.X = (int)headEnemiesLoc[headEnemiesCount].X;
				headEnemyLeftAnim[headEnemiesCount].destRec.Y = (int)headEnemiesLoc[headEnemiesCount].Y;
				headEnemyRightAnim[headEnemiesCount].destRec.X = (int)headEnemiesLoc[headEnemiesCount].X;
				headEnemyRightAnim[headEnemiesCount].destRec.Y = (int)headEnemiesLoc[headEnemiesCount].Y;
				headEnemyUpAnim[headEnemiesCount].destRec.X = (int)headEnemiesLoc[headEnemiesCount].X;
				headEnemyUpAnim[headEnemiesCount].destRec.Y = (int)headEnemiesLoc[headEnemiesCount].Y;

				//give the head a timer to randomly shoot
				headRandomShootTimer[headEnemiesCount] = new Timer(rng.Next(5000, 7501), true);

				//set the head's health
				headHealths[headEnemiesCount] = 3;

				//increase the head count and amount spawned
				headEnemiesCount++;
				headEnemiesSpawned++;

				//reset the spawn timer
				headEnemiesSpawnTimer.ResetTimer(true);
			}

			//update the head positions and spawn bullets from them once the timer is over 
			for (int i = 0; i < headEnemiesCount; i++)
			{
				//calculate the distance of the center of the player to the head
				charDistFromHeadX = mainCharCenterLoc.X - headEnemyDownAnim[i].destRec.Center.X;
				charDistFromHeadY = mainCharCenterLoc.Y - headEnemyDownAnim[i].destRec.Center.Y;

				//make the head stay a certain distance away from the player, but move if necessary
				if (charDistFromHeadX < -300 || charDistFromHeadX > 300 || charDistFromHeadY < -50 || charDistFromHeadY > 50)
				{
					//move along the x-axis and face the direction depending on the character's position to the head
					if (charDistFromHeadX < 0)
					{
						//move the head
						headEnemiesLoc[i].X -= 1;

						//face the head left
						headDirections[i] = LEFT;
					}
					else if (charDistFromHeadX > 0)
					{
						//move the head
						headEnemiesLoc[i].X += 1;

						//face the head right
						headDirections[i] = RIGHT;
					}
				}

				//make the head stay a certain distance away from the player, but move if necessary
				if (charDistFromHeadY < -300 || charDistFromHeadY > 300 || charDistFromHeadX < -50 || charDistFromHeadX > 50)
				{
					//move along the y-axis and face the direction depending on the character's position to the head
					if (charDistFromHeadY < 0)
					{
						//move the head
						headEnemiesLoc[i].Y -= 1;

						//face the head up
						headDirections[i] = UP;
					}
					else if (charDistFromHeadY > 0)
					{
						//move the head
						headEnemiesLoc[i].Y += 1;

						//face the head down
						headDirections[i] = DOWN;
					}
				}

				//move the animations to the vector
				headEnemyDownAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyDownAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;
				headEnemyLeftAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyLeftAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;
				headEnemyRightAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyRightAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;
				headEnemyUpAnim[i].destRec.X = (int)headEnemiesLoc[i].X;
				headEnemyUpAnim[i].destRec.Y = (int)headEnemiesLoc[i].Y;

				//shoot once the timer is finished
				if (headRandomShootTimer[i].IsFinished())
				{
					//shoot on the correct axis
					if (Math.Abs(charDistFromHeadX) > Math.Abs(charDistFromHeadY))
					{
						//move the bullets to the head
						headBulletsRec[numHeadBullets].X = headEnemyDownAnim[i].destRec.Center.X;
						headBulletsRec[numHeadBullets].Y = headEnemyDownAnim[i].destRec.Center.Y + MULTISHOT_OFFSET;
						headBulletsRec[numHeadBullets + 1].X = headEnemyDownAnim[i].destRec.Center.X;
						headBulletsRec[numHeadBullets + 1].Y = headEnemyDownAnim[i].destRec.Center.Y;
						headBulletsRec[numHeadBullets + 2].X = headEnemyDownAnim[i].destRec.Center.X;
						headBulletsRec[numHeadBullets + 2].Y = headEnemyDownAnim[i].destRec.Center.Y - MULTISHOT_OFFSET;

						//shoot in the correct direction
						if (charDistFromHeadX > 0)
						{
							//set the speed and direction of the bullets
							headBulletSpeedX[numHeadBullets] = 1;
							headBulletSpeedY[numHeadBullets] = 0;
							headBulletSpeedX[numHeadBullets + 1] = 1;
							headBulletSpeedY[numHeadBullets + 1] = 0;
							headBulletSpeedX[numHeadBullets + 2] = 1;
							headBulletSpeedY[numHeadBullets + 2] = 0;
						}
						else
						{
							//set the speed and direction of the bullets
							headBulletSpeedX[numHeadBullets] = -1;
							headBulletSpeedY[numHeadBullets] = 0;
							headBulletSpeedX[numHeadBullets + 1] = -1;
							headBulletSpeedY[numHeadBullets + 1] = 0;
							headBulletSpeedX[numHeadBullets + 2] = -1;
							headBulletSpeedY[numHeadBullets + 2] = 0;
						}
					}
					else
					{
						//move the bullets to the head
						headBulletsRec[numHeadBullets].X = headEnemyDownAnim[i].destRec.Center.X + MULTISHOT_OFFSET;
						headBulletsRec[numHeadBullets].Y = headEnemyDownAnim[i].destRec.Center.Y;
						headBulletsRec[numHeadBullets + 1].X = headEnemyDownAnim[i].destRec.Center.X;
						headBulletsRec[numHeadBullets + 1].Y = headEnemyDownAnim[i].destRec.Center.Y;
						headBulletsRec[numHeadBullets + 2].X = headEnemyDownAnim[i].destRec.Center.X - MULTISHOT_OFFSET;
						headBulletsRec[numHeadBullets + 2].Y = headEnemyDownAnim[i].destRec.Center.Y;

						//shoot in the correct direction
						if (charDistFromHeadY > 0)
						{
							//set the speed and direction of the bullets
							headBulletSpeedX[numHeadBullets] = 0;
							headBulletSpeedY[numHeadBullets] = 1;
							headBulletSpeedX[numHeadBullets + 1] = 0;
							headBulletSpeedY[numHeadBullets + 1] = 1;
							headBulletSpeedX[numHeadBullets + 2] = 0;
							headBulletSpeedY[numHeadBullets + 2] = 1;
						}
						else
						{
							//set the speed and direction of the bullets
							headBulletSpeedX[numHeadBullets] = 0;
							headBulletSpeedY[numHeadBullets] = -1;
							headBulletSpeedX[numHeadBullets + 1] = 0;
							headBulletSpeedY[numHeadBullets + 1] = -1;
							headBulletSpeedX[numHeadBullets + 2] = 0;
							headBulletSpeedY[numHeadBullets + 2] = -1;
						}
					}

					//increase the head bullet count
					numHeadBullets += 3;

					//randomize the timer for when the head shoots
					headRandomShootTimer[i] = new Timer(rng.Next(5000, 7501), true);
				}
			}

			//check all the current head enemy bullets for intersections with the player and the edge of the screen and move them
			for (int i = 0; i < numHeadBullets; i++)
			{
				//move the bullet
				headBulletsRec[i].X += headBulletSpeedX[i];
				headBulletsRec[i].Y += headBulletSpeedY[i];

				//if the bullet intersects the player, reduce health and delete, and just delete if it touches the walls
				if (headBulletsRec[i].Intersects(mainCharJAnim[charDirection].destRec) && isJActive == true && playerImmunityTimer.IsFinished())
				{
					//lower health of j
					playerJHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);

					//delete the current bullet of the head 
					DeleteHeadBullet(i);
				}
				else if (headBulletsRec[i].Intersects(mainCharEAnim[charDirection].destRec) && isJActive == false && playerImmunityTimer.IsFinished())
				{
					//lower health of e
					playerEHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);

					//delete the current bullet of the head
					DeleteHeadBullet(i);
				}
				else if (!headBulletsRec[i].Intersects(floorBkRec) || headBulletsRec[i].Intersects(mainCharJAnim[charDirection].destRec) || headBulletsRec[i].Intersects(mainCharEAnim[charDirection].destRec))
				{
					//delete the current bullet of the head
					DeleteHeadBullet(i);
				}
			}

			//spawning the rock enemies when they can be spawned and the timer is over 
			if (rockEnemiesSpawned < rockEnemiesToSpawn && rockEnemiesSpawnTimer.IsFinished())
			{
				//spawn a rock enemy at one of the four spawns
				rockEnemiesLoc[rockEnemiesCount] = EnemySpawning();

				//move the animation to the vector
				rockEnemyDownAnim[rockEnemiesCount].destRec.X = (int)rockEnemiesLoc[rockEnemiesCount].X;
				rockEnemyDownAnim[rockEnemiesCount].destRec.Y = (int)rockEnemiesLoc[rockEnemiesCount].Y;
				rockEnemyLeftAnim[rockEnemiesCount].destRec.X = (int)rockEnemiesLoc[rockEnemiesCount].X;
				rockEnemyLeftAnim[rockEnemiesCount].destRec.Y = (int)rockEnemiesLoc[rockEnemiesCount].Y;
				rockEnemyRightAnim[rockEnemiesCount].destRec.X = (int)rockEnemiesLoc[rockEnemiesCount].X;
				rockEnemyRightAnim[rockEnemiesCount].destRec.Y = (int)rockEnemiesLoc[rockEnemiesCount].Y;
				rockEnemyUpAnim[rockEnemiesCount].destRec.X = (int)rockEnemiesLoc[rockEnemiesCount].X;
				rockEnemyUpAnim[rockEnemiesCount].destRec.Y = (int)rockEnemiesLoc[rockEnemiesCount].Y;

				//set the healths
				rockEnemyHealths[rockEnemiesCount] = 2;

				//increase the count and amount spawned
				rockEnemiesCount++;
				rockEnemiesSpawned++;

				//reset the spawn timer
				rockEnemiesSpawnTimer.ResetTimer(true);
			}

			//if any rock enemies intersect the player, lower the current character health and reset the immunity timer, and move the rock enemies towards the player, and deal with floor rock collision
			for (int i = 0; i < rockEnemiesCount; i++)
			{
				//if any bats intersect the player, lower the current character health and reset the immunity timer
				if (rockEnemyDownAnim[i].destRec.Intersects(mainCharJAnim[charDirection].destRec) && playerImmunityTimer.IsFinished())
				{
					//lower character j health
					playerJHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);
				}
				else if (rockEnemyDownAnim[i].destRec.Intersects(mainCharEAnim[charDirection].destRec) && playerImmunityTimer.IsFinished())
				{
					//lower character e health
					playerEHealth--;

					//reset immunity timer
					playerImmunityTimer.ResetTimer(true);
				}

				//get the distance betweent the center of the player and the rock enemy
				charDistFromRockX = mainCharCenterLoc.X - rockEnemyDownAnim[i].destRec.Center.X;
				charDistFromRockY = mainCharCenterLoc.Y - rockEnemyDownAnim[i].destRec.Center.Y;

				//face towards the player if the x is farther than the y
				if (charDistFromRockX < 0 && Math.Abs(charDistFromRockX) > Math.Abs(charDistFromRockY))
				{
					//face rock enemy left
					rockEnemyDirections[i] = LEFT;
				}
				else if (charDistFromRockX > 0 && Math.Abs(charDistFromRockX) > Math.Abs(charDistFromRockY))
				{
					//face rock enemy right
					rockEnemyDirections[i] = RIGHT;
				}

				//face towards the player if the y is farther than the x
				if (charDistFromRockY < 0 && Math.Abs(charDistFromRockX) < Math.Abs(charDistFromRockY))
				{
					//face rock enemy up
					rockEnemyDirections[i] = UP;
				}
				else if (charDistFromRockY > 0 && Math.Abs(charDistFromRockX) < Math.Abs(charDistFromRockY))
				{
					//face rock enemy down
					rockEnemyDirections[i] = DOWN;
				}

				//if the rock enemies touch a floor rock, push it back
				for (int j = 0; j < numRocks; j++)
				{
					//if the rock enemy touches a floor rock, push it back
					if (rockEnemyDownAnim[i].destRec.Intersects(rocksRec[j]))
					{
						//move the rock enemy back depending on the side it is on
						if (rockEnemyDownAnim[i].destRec.Center.X > rocksRec[j].Center.X)
						{
							//move the rock enemy back
							rockEnemiesLoc[i].X += BASE_MELEE_ENEMY_SPEED;
						}
						else
						{
							//move the rock enemy back
							rockEnemiesLoc[i].X -= BASE_MELEE_ENEMY_SPEED;
						}

						//move the rock enemy back depending on the side it is on
						if (rockEnemyDownAnim[i].destRec.Center.Y > rocksRec[j].Center.Y)
						{
							//move the rock enemy back
							rockEnemiesLoc[i].Y += BASE_MELEE_ENEMY_SPEED;
						}
						else
						{
							//move the rock enemy back
							rockEnemiesLoc[i].Y -= BASE_MELEE_ENEMY_SPEED;
						}
					}
				}

				//update the rock enemy vector and move enemies to it
				rockEnemiesLoc[i] = MovingToPlayer(rockEnemiesLoc[i], rockEnemyDownAnim[i]);
				rockEnemyDownAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyDownAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;
				rockEnemyRightAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyRightAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;
				rockEnemyLeftAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyLeftAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;
				rockEnemyUpAnim[i].destRec.X = (int)rockEnemiesLoc[i].X;
				rockEnemyUpAnim[i].destRec.Y = (int)rockEnemiesLoc[i].Y;
			}

			//calculate the amount of enemies left
			totalEnemyCount = batEnemiesToSpawn - batEnemiesSpawned + robotEnemiesToSpawn - robotEnemiesSpawned + headEnemiesToSpawn - headEnemiesSpawned + rockEnemiesToSpawn - rockEnemiesSpawned;

			//check if all enemies are eliminated and go to upgrade
			if (totalEnemyCount <= 0 && batEnemiesCount <= 0 && robotEnemiesCount <= 0 && headEnemiesCount <= 0 && rockEnemiesCount <= 0)
			{
				//make the down arrow go to the location
				downArrowRec.X = (int)downArrowLoc.X;
				downArrowRec.Y = (int)downArrowLoc.Y;

				//check if the player touches the bottom floor rectangle and go to upgrade
				if (mainCharJAnim[charDirection].destRec.Intersects(enemySpawnFloorsRec[DOWN]) || mainCharEAnim[charDirection].destRec.Intersects(enemySpawnFloorsRec[DOWN]))
				{
					//increase the round
					currentRound++;

					//reset a few things
					SoftReset();

					//add score
					score += 500;

					//go to upgrade
					gameState = UPGRADE;
				}
			}
			else if (downArrowRec.X != HIDE_OBJECTS_LOCATION)
			{
				//hide the arrow
				downArrowRec.X = (int)HIDE_OBJECTS_LOCATION;
				downArrowRec.Y = (int)HIDE_OBJECTS_LOCATION;
			}

			//if both character healths are 0, go to endgame
			if (playerJHealth <= 0 && playerEHealth <= 0)
			{
				//reset muisc volume and play a end song
				MediaPlayer.Volume = 1f;
				MediaPlayer.Play(endMusic[rng.Next(0, 3)]);

				//update the highscore if applicable
				if (score > highscore)
				{
					//set highscore to the current score
					highscore = score;
				}

				//go to endgame
				gameState = ENDGAME;
			}
		}

		//Pre: nothing
		//Post: nothing
		//Desc: select random upgrades for the player and let the player choose between two
		private void UpdateUpgrade()
		{
			//keep trying to select upgrades until finally chosen
			while (upgradeOptionChosen == false)
			{
				//present the player with one of these options
				if (randomUpgradeLeft < 25)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[BOOTS].X = (int)upgrade1Loc.X;
					upgradesRec[BOOTS].Y = (int)upgrade1Loc.Y;
					upgradeTextLoc[BOOTS].X = upgrade1Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[BOOTS].Y = upgrade1Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate that this has been already chosen as a possible option
					isUpgradeActive[BOOTS] = true;
				}
				else if (randomUpgradeLeft < 50)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[CLOCK].X = (int)upgrade1Loc.X;
					upgradesRec[CLOCK].Y = (int)upgrade1Loc.Y;
					upgradeTextLoc[CLOCK].X = upgrade1Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[CLOCK].Y = upgrade1Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate that this has been already chosen as a possible option
					isUpgradeActive[CLOCK] = true;
				}
				else if (randomUpgradeLeft < 75)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[THREE_BULLETS].X = (int)upgrade1Loc.X;
					upgradesRec[THREE_BULLETS].Y = (int)upgrade1Loc.Y;
					upgradeTextLoc[THREE_BULLETS].X = upgrade1Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[THREE_BULLETS].Y = upgrade1Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate that this has been already chosen as a possible option
					isUpgradeActive[THREE_BULLETS] = true;
				}
				else
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[SHIELD].X = (int)upgrade1Loc.X;
					upgradesRec[SHIELD].Y = (int)upgrade1Loc.Y;
					upgradeTextLoc[SHIELD].X = upgrade1Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[SHIELD].Y = upgrade1Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate that this has been already chosen as a possible option
					isUpgradeActive[SHIELD] = true;
				}

				//present the player with one of these options
				if (randomUpgradeRight < 15 && isUpgradeActive[BOOTS] == false)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[BOOTS].X = (int)upgrade2Loc.X;
					upgradesRec[BOOTS].Y = (int)upgrade2Loc.Y;
					upgradeTextLoc[BOOTS].X = upgrade2Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[BOOTS].Y = upgrade2Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate to end the loop
					upgradeOptionChosen = true;
				}
				else if (randomUpgradeRight < 30 && isUpgradeActive[CLOCK] == false)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[CLOCK].X = (int)upgrade2Loc.X;
					upgradesRec[CLOCK].Y = (int)upgrade2Loc.Y;
					upgradeTextLoc[CLOCK].X = upgrade2Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[CLOCK].Y = upgrade2Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate to end the loop
					upgradeOptionChosen = true;
				}
				else if (randomUpgradeRight < 45 && isUpgradeActive[THREE_BULLETS] == false)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[THREE_BULLETS].X = (int)upgrade2Loc.X;
					upgradesRec[THREE_BULLETS].Y = (int)upgrade2Loc.Y;
					upgradeTextLoc[THREE_BULLETS].X = upgrade2Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[THREE_BULLETS].Y = upgrade2Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate to end the loop
					upgradeOptionChosen = true;
				}
				else if (randomUpgradeRight < 60 && isUpgradeActive[SHIELD] == false)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[SHIELD].X = (int)upgrade2Loc.X;
					upgradesRec[SHIELD].Y = (int)upgrade2Loc.Y;
					upgradeTextLoc[SHIELD].X = upgrade2Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[SHIELD].Y = upgrade2Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate to end the loop
					upgradeOptionChosen = true;
				}
				else if (isUpgradeActive[HEART] == false && playerJHealth < 5 && playerEHealth < 4)
				{
					//move the upgrade and its description to the upgrade location
					upgradesRec[HEART].X = (int)upgrade2Loc.X;
					upgradesRec[HEART].Y = (int)upgrade2Loc.Y;
					upgradeTextLoc[HEART].X = upgrade2Loc.X - UI_ICON_OFFSET;
					upgradeTextLoc[HEART].Y = upgrade2Loc.Y + UPGRADE_SIZE + UI_ICON_OFFSET;

					//indicate to end the loop
					upgradeOptionChosen = true;
				}
				else
				{
					//re-randomize if not previously applicable
					randomUpgradeRight = rng.Next(0, 99);
				}
			}

			//if the player clicks an upgrade, apply it
			if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
			{
				//apply the corresponding upgrade
				if (upgradesRec[BOOTS].Contains(mouse.Position))
				{
					//increase the speed
					currentMaxSpeed++;
					maxDiagonalSpeed = (float)Math.Sqrt(currentMaxSpeed);

					//go back to game
					gameState = GAME;
				}
				else if (upgradesRec[CLOCK].Contains(mouse.Position))
				{
					//increase the firerate
					fireRateMultiplier *= 0.95f;

					//go back to game
					gameState = GAME;
				}
				else if (upgradesRec[THREE_BULLETS].Contains(mouse.Position))
				{
					//add multishot
					storedMultiShot += 10;

					//go back to game
					gameState = GAME;
				}
				else if (upgradesRec[SHIELD].Contains(mouse.Position))
				{
					//increase the immunity timer and activate it
					timesImmuneUpgraded++;
					playerImmunityTimer = new Timer(3000 + (timesImmuneUpgraded * 500), true);
					immunityBubbleRec.X = mainCharJAnim[charDirection].destRec.Center.X - (immunityBubbleRec.Height / 2);
					immunityBubbleRec.Y = mainCharJAnim[charDirection].destRec.Center.Y - (immunityBubbleRec.Height / 2);

					//go back to game
					gameState = GAME;
				}
				else if (upgradesRec[HEART].Contains(mouse.Position))
				{
					//increase player health
					playerJHealth++;
					playerEHealth++;

					//go back to game
					gameState = GAME;
				}
			}
		}

		//Pre: nothing
		//Post: nothing
		//Desc: allow the player to compare their score to the highscore and retry or go to the menu
		private void UpdateEndGame()
		{
			//glow the buttons if hovered over
			menuBtn = menuBtnRec.Contains(mouse.Position);
			retryBtn = retryBtnRec.Contains(mouse.Position);

			//play a song if not already playing
			if (MediaPlayer.State != MediaState.Playing)
			{
				//play a random song
				MediaPlayer.Play(endMusic[rng.Next(0, 3)]);
			}

			//when the mouse is pressed over a button, go to menu or game
			if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
			{
				//go to menu or game depending on the button
				if (retryBtnRec.Contains(mouse.Position))
				{
					//set the volume and play a random game song
					MediaPlayer.Volume = 0.5f;
					MediaPlayer.Play(gameMusic[rng.Next(0, 3)]);

					//go to pregame
					gameState = PREGAME;
				}
				else if (menuBtnRec.Contains(mouse.Position))
				{
					//play a random menu song
					MediaPlayer.Play(menuMusic[rng.Next(0, 2)]);

					//go to menu
					gameState = MENU;
				}
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			//paint the back black
			GraphicsDevice.Clear(Color.Black);

			// TODO: Add your drawing code here
			spriteBatch.Begin();
			switch (gameState)
			{
				case MENU:
					//draw the menu
					DrawMenu();
					break;
				case INSTRUCTIONS:
					//draw the instructions
					DrawInstructions();
					break;
				case GAME:
					//draw the game
					DrawGame();
					break;
				case UPGRADE:
					//draw the upgrades
					DrawUpgrade();
					break;
				case ENDGAME:
					//draw the endgame
					DrawEndGame();
					break;
			}
			spriteBatch.End();

			base.Draw(gameTime);
		}

		//Pre: nothing
		//Post: nothing
		//Desc: draw images and text in the menu
		private void DrawMenu()
		{
			//draw the buttons
			spriteBatch.Draw(startImg[Convert.ToByte(startBtn)], startBtnRec, Color.White);
			spriteBatch.Draw(intsImg[Convert.ToByte(intsBtn)], intsBtnRec, Color.White);

			//draw the logo
			spriteBatch.Draw(logoImg, logoRec, Color.White);
		}

		//Pre: nothing
		//Post: nothing
		//Desc: draw images and text in the instructions
		private void DrawInstructions()
		{
			//draw the instructions
			spriteBatch.Draw(instructions1Img, instructions1Rec, Color.White);
			spriteBatch.Draw(instructions2Img, instructions2Rec, Color.White);
			spriteBatch.DrawString(gameFont, "Change music volume with - & +\nChange sound volume with O & P\nClick anywhere to continue", instructionsTextLoc, Color.White);
		}

		//Pre: nothing
		//Post: nothing
		//Desc: draw images and text in the game
		private void DrawGame()
		{
			//draw the floor and walls
			spriteBatch.Draw(wallBkImg, wallBkRec, Color.White);
			spriteBatch.Draw(floorBkImg, floorBkRec, Color.White);

			//draw the floor spawn rectangles
			for (int i = 0; i < 4; i++)
			{
				spriteBatch.Draw(floorBkImg, enemySpawnFloorsRec[i], Color.Gray);
			}

			//draw the player over or under the rock depending on the distance form the centers
			if (charDistFromRockY < 0)
			{
				//draw the current player animation
				mainCharJAnim[charDirection].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				mainCharEAnim[charDirection].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);

				//draw the floor rocks
				for (int i = 0; i < 16; i++)
				{
					spriteBatch.Draw(randomRocksImg[i], rocksRec[i], Color.White);
				}
			}
			else
			{
				//draw the floor rocks
				for (int i = 0; i < 16; i++)
				{
					spriteBatch.Draw(randomRocksImg[i], rocksRec[i], Color.White);
				}

				//draw the current player animation
				mainCharJAnim[charDirection].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				mainCharEAnim[charDirection].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
			}

			//draw the immunity bubble
			spriteBatch.Draw(bubbleImg, immunityBubbleRec, Color.White * immunityBubbleFade);

			//draw the guiding arrow
			spriteBatch.Draw(downArrowImg, downArrowRec, Color.White);

			//draw the player bullets
			for (int i = 0; i < playerBulletsCount; i++)
			{
				//draw the player bullets depending on who they belong to
				if (isBulletOfJ[i] == true)
				{
					//draw the player bullet
					spriteBatch.Draw(metalBulletImg, playerBulletsRec[i], Color.White);
				}
				else
				{
					//draw the player bullet
					spriteBatch.Draw(woodBulletImg, playerBulletsRec[i], Color.White);
				}
			}

			//draw the bat enemies
			for (int i = 0; i < batEnemiesCount; i++)
			{
				//draw the current bat enemy
				batEnemyAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
			}

			//draw the robot enemies
			for (int i = 0; i < robotEnemiesCount; i++)
			{
				//draw the robot enemy depending on direction
				if (robotDirections[i] == DOWN)
				{
					//draw the robot animation
					robotEnemyDownAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (robotDirections[i] == LEFT)
				{
					//draw the robot animation
					robotEnemyLeftAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (robotDirections[i] == RIGHT)
				{
					//draw the robot animation
					robotEnemyRightAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (robotDirections[i] == UP)
				{
					//draw the robot animation
					robotEnemyUpAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
			}

			//draw the head enemies
			for (int i = 0; i < headEnemiesCount; i++)
			{
				//draw the head enemy depending on direction
				if (headDirections[i] == DOWN)
				{
					//draw the head animation
					headEnemyDownAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (headDirections[i] == LEFT)
				{
					//draw the head animation
					headEnemyLeftAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (headDirections[i] == RIGHT)
				{
					//draw the head animation
					headEnemyRightAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (headDirections[i] == UP)
				{
					//draw the head animation
					headEnemyUpAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
			}

			//draw the rock enemies
			for (int i = 0; i < rockEnemiesCount; i++)
			{
				//draw the rock enemy depending on direction
				if (rockEnemyDirections[i] == DOWN)
				{
					//draw the rock animation
					rockEnemyDownAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (rockEnemyDirections[i] == LEFT)
				{
					//draw the rock animation
					rockEnemyLeftAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (rockEnemyDirections[i] == RIGHT)
				{
					//draw the rock animation
					rockEnemyRightAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
				else if (rockEnemyDirections[i] == UP)
				{
					//draw the rock animation
					rockEnemyUpAnim[i].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
				}
			}

			//draw the robot enemy bullets
			for (int i = 0; i < numRobotBullets; i++)
			{
				//draw the robot enemy bullet
				spriteBatch.Draw(enemyBulletImg, robotBulletsRec[i], Color.White);
			}

			//draw the head enemy bullets
			for (int i = 0; i < numHeadBullets; i++)
			{
				//draw the head enemy bullet
				spriteBatch.Draw(enemyBulletImg, headBulletsRec[i], Color.White);
			}

			//draw the heart depending on the health
			if (playerJHealth <= 0)
			{
				//draw the broken heart
				spriteBatch.Draw(brokenHeartImg, heartJLoc, Color.White);
			}
			else
			{
				//draw the heart and health
				spriteBatch.Draw(heartImg, heartJLoc, Color.White);
				spriteBatch.DrawString(gameFont, Convert.ToString(playerJHealth), healthJLoc, Color.White);
			}

			//draw the heart depending on the health
			if (playerEHealth <= 0)
			{
				//draw the broken heart
				spriteBatch.Draw(brokenHeartImg, heartELoc, Color.White);
			}
			else
			{
				//draw the heart and health
				spriteBatch.Draw(heartImg, heartELoc, Color.White);
				spriteBatch.DrawString(gameFont, Convert.ToString(playerEHealth), healthELoc, Color.White);
			}

			//draw the multishot icon and text if there is multishot
			if (storedMultiShot > 0)
			{
				//draw the multishot icon and text
				spriteBatch.Draw(upgradesImg[THREE_BULLETS], UIMultishotRec, Color.White);
				spriteBatch.DrawString(gameFont, Convert.ToString(storedMultiShot), UIMultishotTextLoc, Color.White);
			}

			//draw the floor items
			coffeeAnim.Draw(spriteBatch, Color.White * coffeeFade, Animation.FLIP_NONE);
			spriteBatch.Draw(bubbleImg, bubbleRec, Color.White * bubbleFade);
			spriteBatch.Draw(heartImg, heartItemRec, Color.White * heartFade);
			spriteBatch.Draw(starImg, starRec, Color.White * starFade);

			//draw the ui items
			UICoffeeAnim.Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
			spriteBatch.Draw(bubbleImg, UIBubbleRec, Color.White);
			spriteBatch.Draw(heartImg, UIHeartItemRec, Color.White);
			spriteBatch.Draw(starImg, UIstarRec, Color.White);

			//draw the tag smoke
			smokeTagAnim.Draw(spriteBatch, Color.White, Animation.FLIP_NONE);

			//draw the score
			spriteBatch.DrawString(gameFont, "Score: " + score, gameScoreLoc, Color.White);
		}

		//Pre: nothing
		//Post: nothing
		//Desc: draw images and text in the upgrade
		private void DrawUpgrade()
		{
			//draw the upgrades
			for (int i = 0; i < 5; i++)
			{
				//draw the upgrade
				spriteBatch.Draw(upgradesImg[i], upgradesRec[i], Color.White);
			}

			//draw the upgrade descriptions
			spriteBatch.DrawString(gameFont, "Increase speed", upgradeTextLoc[BOOTS], Color.White);
			spriteBatch.DrawString(gameFont, "Increase firerate", upgradeTextLoc[CLOCK], Color.White);
			spriteBatch.DrawString(gameFont, "Get 10 multishot \n(Use with 'Q')", upgradeTextLoc[THREE_BULLETS], Color.White);
			spriteBatch.DrawString(gameFont, "Increase the \ninvincibility time", upgradeTextLoc[SHIELD], Color.White);
			spriteBatch.DrawString(gameFont, "Give health to \neach character", upgradeTextLoc[HEART], Color.White);
		}

		//Pre: nothing
		//Post: nothing
		//Desc: draw images and text in the endgame
		private void DrawEndGame()
		{
			//draw the buttons
			spriteBatch.Draw(retryImg[Convert.ToByte(retryBtn)], retryBtnRec, Color.White);
			spriteBatch.Draw(menuImg[Convert.ToByte(menuBtn)], menuBtnRec, Color.White);

			//draw the score and maybe highscore depending on the score
			if (score >= highscore)
			{
				//draw the new highscore
				spriteBatch.DrawString(gameFont, "New Highscore: " + score, gameScoreLoc, Color.White);
			}
			else
			{
				//draw the score and highscore
				spriteBatch.DrawString(gameFont, "Score: " + score + "\n Highscore: " + highscore, gameScoreLoc, Color.White);
			}
		}

		//Pre: nothing
		//Post: nothing
		//Desc: reset minor things in prep for a new round
		private void SoftReset()
		{
			//spawn more bats as long as it agrees with the limit
			if (batEnemiesToSpawn + 5 < MAX_HITTERS)
			{
				//increase the amount to be spawned
				batEnemiesToSpawn = 20 + (5 * (currentRound - 1));
			}

			//spawn more robots as long as it agrees with the limit and the round is even
			if (robotEnemiesToSpawn + 4 < MAX_SHOOTERS && currentRound % 2 == 0)
			{
				//increase the amount to be spawned
				robotEnemiesToSpawn = (4 * (currentRound - 1));
			}

			//spawn more heads as long as it agrees with the limit and the round a multiple of 3
			if (headEnemiesToSpawn + 2 < MAX_SHOOTERS && currentRound % 3 == 0)
			{
				//increase the amount to be spawned
				headEnemiesToSpawn = (2 * (currentRound - 1));
			}

			//spawn more rocks as long as it agrees with the limit
			if (rockEnemiesToSpawn + 3 < MAX_HITTERS)
			{
				//increase the amount to be spawned
				rockEnemiesToSpawn = 3 + (3 * (currentRound - 1));
			}

			//reset the player bullets
			for (int i = 0; i < MAX_BULLETS; i++)
			{
				//reset everything about the bullet
				isBulletOfJ[i] = true;
				playerBulletSpeedX[i] = 0;
				playerBulletSpeedY[i] = 0;
				playerBulletsLoc[i] = hideObjectsLoc;
				playerBulletsRec[i].X = (int)playerBulletsLoc[i].X;
				playerBulletsRec[i].Y = (int)playerBulletsLoc[i].Y;
			}

			//make the count of bullets 0
			playerBulletsCount = 0;

			//delete all robot bullets
			for (int i = 0; i < numRobotBullets; i++)
			{
				//delete the robot bullet
				DeleteRobotBullet(i);
			}

			//delete all head bullets
			for (int i = 0; i < numHeadBullets; i++)
			{
				//delete the head bullet
				DeleteHeadBullet(i);
			}

			//reset the firerate timer
			playerFireRateTimer.ResetTimer(true);

			//hide all the ground items
			coffeeAnim.destRec.X = (int)HIDE_OBJECTS_LOCATION;
			coffeeAnim.destRec.Y = (int)HIDE_OBJECTS_LOCATION;
			bubbleRec.X = (int)HIDE_OBJECTS_LOCATION;
			bubbleRec.Y = (int)HIDE_OBJECTS_LOCATION;
			heartItemRec.X = (int)HIDE_OBJECTS_LOCATION;
			heartItemRec.Y = (int)HIDE_OBJECTS_LOCATION;
			starRec.X = (int)HIDE_OBJECTS_LOCATION;
			starRec.Y = (int)HIDE_OBJECTS_LOCATION;
			isCoffeeInRoom = false;
			isBubbleInRoom = false;
			isHeartInRoom = false;
			isStarInRoom = false;

			//reset the hide timers
			coffeeHideTimer.ResetTimer(false);
			bubbleHideTimer.ResetTimer(false);
			heartHideTimer.ResetTimer(false);
			starHideTimer.ResetTimer(false);

			//randomize the rock layout
			RandomRoomRockLayout();

			//reset the amount spawned
			batEnemiesSpawned = 0;
			robotEnemiesSpawned = 0;
			headEnemiesSpawned = 0;
			rockEnemiesSpawned = 0;

			//reset the player position to the center
			mainCharLoc = roomCharacterStartLoc;

			//randomize the next upgrades
			randomUpgradeLeft = rng.Next(0, 99);
			randomUpgradeRight = rng.Next(0, 99);

			//make the upgrades not chosen yet
			upgradeOptionChosen = false;

			//hide the upgrades
			for (int i = 0; i < 5; i++)
			{
				//hide all upgrades
				isUpgradeActive[i] = false;
				upgradesRec[i].X = (int)hideObjectsLoc.X;
				upgradesRec[i].Y = (int)hideObjectsLoc.Y;
				upgradeTextLoc[i].X = (int)hideObjectsLoc.X;
				upgradeTextLoc[i].Y = (int)hideObjectsLoc.Y;
			}
		}

		//Pre: nothing
		//Post: nothing
		//Desc: fire a bullet depending on mouse angle
		private void PlayerShooting()
		{
			//calculate the angle from the player to the mouse
			playerBulletAngle = (float)Math.Atan((mouse.Y - mainCharCenterLoc.Y) / ((mouse.X - mainCharCenterLoc.X)));

			//indicate who the bullet belongs to
			if (isJActive == true)
			{
				//indicate the bullet belongs to j
				isBulletOfJ[playerBulletsCount] = true;
			}
			else
			{
				//indicate the bullet belongs to e
				isBulletOfJ[playerBulletsCount] = false;
			}

			//fire the bullet in the correct direction
			if (mouse.X < mainCharCenterLoc.X)
			{
				//fire the bullet in the direction
				playerBulletSpeedX[playerBulletsCount] = BASE_BULLET_SPEED * -(float)Math.Cos(playerBulletAngle + (currentMultiShotAngle * (Math.PI / 180)));
				playerBulletSpeedY[playerBulletsCount] = BASE_BULLET_SPEED * -(float)Math.Sin(playerBulletAngle + (currentMultiShotAngle * (Math.PI / 180)));
			}
			else
			{
				//fire the bullet in the direction
				playerBulletSpeedX[playerBulletsCount] = BASE_BULLET_SPEED * (float)Math.Cos(playerBulletAngle + (currentMultiShotAngle * (Math.PI / 180)));
				playerBulletSpeedY[playerBulletsCount] = BASE_BULLET_SPEED * (float)Math.Sin(playerBulletAngle + (currentMultiShotAngle * (Math.PI / 180)));
			}

			//move the bullet to the character
			playerBulletsLoc[playerBulletsCount] = mainCharCenterLoc;

			//increase the count
			playerBulletsCount++;

			//reset the firerate timer
			playerFireRateTimer.ResetTimer(true);
		}

		//Pre: vector is the enemy's vector, animation is the enemy's animation
		//Post: returns the adjusted vector
		//Desc: moves an enemy to the player
		private Vector2 MovingToPlayer(Vector2 vector, Animation animation)
		{
			//move the enemy along the x-axis depending on the side
			if (mainCharCenterLoc.X - animation.destRec.Center.X < -10)
			{
				//move the vector
				vector.X -= BASE_MELEE_ENEMY_SPEED;
			}
			else if (mainCharCenterLoc.X - animation.destRec.Center.X > 10)
			{
				//move the vector
				vector.X += BASE_MELEE_ENEMY_SPEED;
			}

			//move the enemy along the y-axis depending on the side
			if (mainCharCenterLoc.Y - animation.destRec.Center.Y < -10)
			{
				//move the vector
				vector.Y -= BASE_MELEE_ENEMY_SPEED;
			}
			else if (mainCharCenterLoc.Y - animation.destRec.Center.Y > 10)
			{
				//move the vector
				vector.Y += BASE_MELEE_ENEMY_SPEED;
			}

			//return the updated vector
			return vector;
		}

		//Pre: enemyLoc is the array of the enemy being deleted, bullet is the current bullet being deleted if shot by j, enemy is the current enemy being deleted, enemyCount is the amount of that enemy type
		//Post: return the vector of the shifted enemy loction
		//Desc: get rid of an enemy by shifting the array down and delete the bullet if applicable
		private Vector2 EnemyDeath(Vector2[] enemyLoc, int bullet, int enemy, int enemyCount)
		{
			//decide if a random item should be dropped
			if (rng.Next(0, 100) < 8)
			{
				//get a random number from 0-99 inclusive
				int randomItem = rng.Next(0, 100);

				//depending on the random number, drop the respective item
				if (randomItem < 25 && isCoffeeInRoom == false)
				{
					//activate the timer that makes it dissappear
					coffeeHideTimer.Activate();

					//move the item to the enemy location
					coffeeAnim.destRec.X = (int)enemyLoc[enemy].X;
					coffeeAnim.destRec.Y = (int)enemyLoc[enemy].Y;

					//reset the fade
					coffeeFade = 1;

					//indicate that the item is inh the room
					isCoffeeInRoom = true;
				}
				else if (randomItem < 50 && isBubbleInRoom == false)
				{
					//activate the timer that makes it dissappear
					bubbleHideTimer.Activate();

					//move the item to the enemy location
					bubbleRec.X = (int)enemyLoc[enemy].X;
					bubbleRec.Y = (int)enemyLoc[enemy].Y;

					//reset the fade
					bubbleFade = 1;

					//indicate that the item is inh the room
					isBubbleInRoom = true;
				}
				else if (randomItem < 75 && (playerJHealth < 5|| playerEHealth < 4) && isHeartInRoom == false)
				{
					//activate the timer that makes it dissappear
					heartHideTimer.Activate();

					//move the item to the enemy location
					heartItemRec.X = (int)enemyLoc[enemy].X;
					heartItemRec.Y = (int)enemyLoc[enemy].Y;

					//reset the fade
					heartFade = 1;

					//indicate that the item is inh the room
					isHeartInRoom = true;
				}
				else if (isStarInRoom == false)
				{
					//activate the timer that makes it dissappear
					starHideTimer.Activate();

					//move the item to the enemy location
					starRec.X = (int)enemyLoc[enemy].X;
					starRec.Y = (int)enemyLoc[enemy].Y;

					//reset the fade
					starFade = 1;

					//indicate that the item is inh the room
					isStarInRoom = true;
				}
			}

			//delete the enemy by moving everything down an element the array
			for (int i = enemy; i < enemyCount; i++)
			{
				//make the current vector the next one
				enemyLoc[i].X = enemyLoc[i + 1].X;
				enemyLoc[i].Y = enemyLoc[i + 1].Y;
			}

			//delete the bullet if shot by j
			if (isBulletOfJ[bullet] == true)
			{
				//delete the current bullet
				DeleteBullet(bullet);
			}

			//reset the timer that ensures no indexing errors if two enemies have a bullet intersecting them at the same time
			charEPierceTimer.ResetTimer(true);

			//update the vector of the score text
			gameScoreLoc.X = screenWidth / 2 - (gameFont.MeasureString("Score: " + score).X / 2);

			//return the enemy shifted down the array
			return enemyLoc[enemy];
		}

		//Pre: bullet is the current bullet being deleted
		//Post: nothing
		//Desc: delete the bullets shot by the player by shifting the array elements down
		private void DeleteBullet (int bullet)
		{
			//shift all bullet data down an element of the array
			for (int i = bullet; i < playerBulletsCount; i++)
			{
				//make the current bullet belong to whoever the next bullet belongs to
				isBulletOfJ[i] = isBulletOfJ[i + 1];

				//make the current speed the next speed
				playerBulletSpeedX[i] = playerBulletSpeedX[i + 1];
				playerBulletSpeedY[i] = playerBulletSpeedY[i + 1];

				//make the current vector the next vector
				playerBulletsLoc[i] = playerBulletsLoc[i + 1];

				//make the rectangle location the vector location
				playerBulletsRec[i].X = (int)playerBulletsLoc[i].X;
				playerBulletsRec[i].Y = (int)playerBulletsLoc[i].Y;
			}

			//decrease the number of bullets counted
			playerBulletsCount--;
		}

		//Pre: nothing
		//Post: return the vector of where the enemy should spawn
		//Desc: spawn an enemy at one of the 4 spawns
		private Vector2 EnemySpawning()
		{
			//get a random number
			int randomEnemySpawnDirection = rng.Next(0, 100);

			//choose a spawn depending on the random number
			if (randomEnemySpawnDirection <= 24)
			{
				//return down as the direction
				return enemySpawnsLoc[DOWN];
			}
			else if (randomEnemySpawnDirection <= 49)
			{
				//return left as the direction
				return enemySpawnsLoc[LEFT];
			}
			else if (randomEnemySpawnDirection <= 74)
			{
				//return right as the direction
				return enemySpawnsLoc[RIGHT];
			}
			else
			{
				//return up as the direction
				return enemySpawnsLoc[UP];
			}
		}

		//Pre: bullet is the current bullet being deleted
		//Post: nothing
		//Desc: delete the bullets spawned from the robot enemy by shifting the array elements down
		private void DeleteRobotBullet (int bullet)
		{
			//move everything down the array
			for (int j = bullet; j < numRobotBullets; j++)
			{
				//make the current speed the next speed
				robotBulletSpeedX[j] = robotBulletSpeedX[j + 1];
				robotBulletSpeedY[j] = robotBulletSpeedY[j + 1];

				//make the current vector the next vector
				robotBulletsRec[j].X = robotBulletsRec[j + 1].X;
				robotBulletsRec[j].Y = robotBulletsRec[j + 1].Y;
			}

			//decrease the count
			numRobotBullets--;
		}

		//Pre: bullet is the current bullet being deleted
		//Post: nothing
		//Desc: delete the bullets spawned from the floating head enemy by shifting the array elements down
		private void DeleteHeadBullet (int bullet)
		{
			//move everything down the array
			for (int j = bullet; j < numHeadBullets; j++)
			{
				//make the current speed the next speed
				headBulletSpeedX[j] = headBulletSpeedX[j + 1];
				headBulletSpeedY[j] = headBulletSpeedY[j + 1];

				//make the current vector the next vector
				headBulletsRec[j].X = headBulletsRec[j + 1].X;
				headBulletsRec[j].Y = headBulletsRec[j + 1].Y;
			}

			//decrease the count
			numHeadBullets--;
		}

		//Pre: nothing
		//Post: nothing
		//Desc: apply or extend the speed affect given from touching a coffee
		private void CoffeeActivation()
		{
			//activate or extend the coffee timer
			if (coffeeEffectTimer.IsInactive())
			{
				//activate the coffee timer
				coffeeEffectTimer.Activate();

				//increase the speed
				currentMaxSpeed += 1;
			}
			else
			{
				//reset the coffee timer
				coffeeEffectTimer.ResetTimer(true);
			}

			//update the diaginal speed
			maxDiagonalSpeed = (float)Math.Sqrt(currentMaxSpeed);
		}

		//Pre: nothing
		//Post: nothing
		//Desc: give a heart to each character if not full
		private void HeartActivation()
		{
			//apply the bubble effect if max health
			if (playerJHealth == 5 && playerEHealth == 4)
			{
				//reset the bubble timer
				bubbleEffectTimer.ResetTimer(true);
			}

			//give health unless health is max
			if (playerJHealth < 5)
			{
				//give health
				playerJHealth++;
			}

			//give health unless health is max
			if (playerEHealth < 4)
			{
				//give health
				playerEHealth++;
			}
		}

		//Pre: nothing
		//Post: nothing
		//Desc: reset the current rock layout and randomize the rock locations based on the premade layouts
		private void RandomRoomRockLayout()
		{
			//hide all rocks
			for (int i = 0; i < 16; i++)
			{
				//hide the current rock
				rocksRec[i].X = (int)HIDE_OBJECTS_LOCATION;
				rocksRec[i].Y = (int)HIDE_OBJECTS_LOCATION;
			}

			//get a random number
			int randomRoom = rng.Next(0, 100);

			//get a layout based on the random number
			if (randomRoom <= 19)
			{
				//place the rocks at their positions
				for (int i = 0; i < roomType1X.Length; i++)
				{
					//place the rock at its position
					rocksRec[i].X = (int)roomType1X[i];
					rocksRec[i].Y = (int)roomType1Y[i];
				}

				//get the number of rocks
				numRocks = (byte)roomType1X.Length;
			}
			else if (randomRoom <= 39)
			{
				//place the rocks at their positions
				for (int i = 0; i < roomType2X.Length; i++)
				{
					//place the rock at its position
					rocksRec[i].X = (int)roomType2X[i];
					rocksRec[i].Y = (int)roomType2Y[i];
				}

				//get the number of rocks
				numRocks = (byte)roomType2X.Length;
			}
			else if (randomRoom <= 59)
			{
				//place the rocks at their positions
				for (int i = 0; i < roomType3X.Length; i++)
				{
					//place the rock at its position
					rocksRec[i].X = (int)roomType3X[i];
					rocksRec[i].Y = (int)roomType3Y[i];
				}

				//get the number of rocks
				numRocks = (byte)roomType3X.Length;
			}
			else if (randomRoom <= 79)
			{
				//place the rocks at their positions
				for (int i = 0; i < roomType4X.Length; i++)
				{
					//place the rock at its position
					rocksRec[i].X = (int)roomType4X[i];
					rocksRec[i].Y = (int)roomType4Y[i];
				}

				//get the number of rocks
				numRocks = (byte)roomType4X.Length;
			}
			else
			{
				//place the rocks at their positions
				for (int i = 0; i < roomType5X.Length; i++)
				{
					//place the rock at its position
					rocksRec[i].X = (int)roomType5X[i];
					rocksRec[i].Y = (int)roomType5Y[i];
				}

				//get the number of rocks
				numRocks = (byte)roomType5X.Length;
			}
		}

		//Pre: currentRock is the current rock being assigned an image
		//Post: nothing
		//Desc: assign a random rock image to thr current rock
		private void RandomizeRockTypes(int currentRock)
		{
			//get a random number
			int randomRockType = rng.Next(0, 99);

			//get a image for the rock depending on the number
			if (randomRockType <= 32)
			{
				//make the current rock have this image
				randomRocksImg[currentRock] = rock1Img;
			}
			else if (randomRockType <= 65)
			{
				//make the current rock have this image
				randomRocksImg[currentRock] = rock2Img;
			}
			else
			{
				//make the current rock have this image
				randomRocksImg[currentRock] = rock3Img;
			}
		}
	}
}
