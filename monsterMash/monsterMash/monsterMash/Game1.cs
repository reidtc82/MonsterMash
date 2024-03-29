using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace monsterMash
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        MouseState mState;
        KeyboardState lastKeyboardState;

        SoundEffect buttonClick;
        SoundEffectInstance buttonInstance;
        SoundEffect monsterScream;
        SoundEffectInstance monsterScreamInstance;

        Monster playerSprite;

        struct monsterProps
        {
            public string ID;
            public float range;
            public float spd;
            public int thisScore;
            public float stam;
            public float sReg;
            public float cost;
            public float maxStam;
            public float rateScare;
            public float visiblity;
        }

        struct stageProps
        {
            public int sizeMap;
            public int humanCount;
            public int fearTime;
        }

        monsterProps[] population = new monsterProps[20];

        private monsterProps tempProps;
        private stageProps mapBase;

        int maxHumans;// = 128;
        Human[] people;// = new Human[maxHumans];

        int maxTiles;// = 64;
        Tile[,] tiles;// = new Tile[maxTiles,maxTiles];

        int maxFOG;// = (maxTiles*2)+1;
        Tile[,] foggies;// = new Tile[maxFOG,maxFOG];

        const int maxFearticles = 128;
        Particle[] fearticle = new Particle[maxFearticles];

        private bool newGame;

        private Texture2D cursor;
        private Texture2D logo;
        private Texture2D startButton;
        private Texture2D instButton;
        private Texture2D exitButton;
        private Texture2D backButton;
        private Texture2D stamBarLeftEnd;
        private Texture2D stamBarCenterPart;
        private Texture2D stamBarRightEnd;
        private Texture2D stamBarFill;

        private Vector2 mousePOS;
        private Vector2 startBPOS;
        private Vector2 instBPOS;
        private Vector2 exitBPOS;
        private Vector2 backBPOS;
        private Vector2 stamBarPOS;

        private int screen;

        private Rectangle mouseRect;
        private Rectangle startButtonRect;
        private Rectangle startButtonBBox;
        private Rectangle instButtonRect;
        private Rectangle instButtonBBox;
        private Rectangle exitButtonRect;
        private Rectangle exitButtonBBox;
        private Rectangle backButtonRect;
        private Rectangle backButtonBBox;
        private Rectangle stamBarLeftBox;
        private Rectangle stamBarRightBox;
        private Rectangle stamBarCenterBox;
        private Rectangle stamBarFillBox;

        private double roundStartTime;
        private double currRoundTime;

        private SpriteFont timerFont;
        private SpriteFont yayFont;

        Random rand = new Random();

        int score;
        int highestScore;
        private bool selectedPop;
        private bool runGACycle;
        private bool scoreSet;
        private float randRad;
        private float randDis;

        private bool canGo;
        private bool drawMessage;
        private bool good;
        private int failure;
        private int milestone;

        String[] failures = new String[10];
        String[] milestones = new String[100];

        private int endRound;
        private bool setEndRound;
        private bool setStageProps;

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
            score = 0;
            highestScore = 0;
            scoreSet = false;
            drawMessage = false;
            setEndRound = false;
            setStageProps = false;

            playerSprite = new Monster();

            //Init map base properties
            mapBase.sizeMap = 16;
            mapBase.humanCount = 512;
            mapBase.fearTime = 50;

            mState = Mouse.GetState();
            lastKeyboardState = Keyboard.GetState();
            screen = 0;
            newGame = true;

            //Init population of monster properties
            for (int x = 0; x < population.Length; x++)
            {
                population[x] = new monsterProps();
            }

            //debug section
            #region
            population[0].ID = "A";
            population[1].ID = "B";
            population[2].ID = "C";
            population[3].ID = "D";
            population[4].ID = "E";
            population[5].ID = "F";
            population[6].ID = "G";
            population[7].ID = "H";
            population[8].ID = "I";
            population[9].ID = "J";
            population[10].ID = "K";
            population[11].ID = "L";
            population[12].ID = "M";
            population[13].ID = "N";
            population[14].ID = "O";
            population[15].ID = "P";
            population[16].ID = "Q";
            population[17].ID = "R";
            population[18].ID = "S";
            population[19].ID = "T";
            #endregion

            //Init fearticles
            for (int i = 0; i < maxFearticles; i++)
            {
                fearticle[i] = new Particle();
            }

            initYayStrings();

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

            //load player sprite
            playerSprite.LoadContent(this.Content, "textures/monsterBase");
            playerSprite.position = new Vector2((GraphicsDevice.Viewport.Width / 2) - 16, (GraphicsDevice.Viewport.Height / 2) - 16);
            playerSprite.frameHeight = 32;
            playerSprite.frameWidth = 32;
            playerSprite.maxFrames = 3;//like array index starts at 0
            playerSprite.frameIndex = 0;//like array index starts at 0
            playerSprite.maxHP = 100;//set up max hp for round. Will change eventually from algorithm.
            playerSprite.HP = playerSprite.maxHP;//set up hp for round

            loadStage(mapBase.sizeMap, mapBase.humanCount, mapBase.fearTime);

            //load fearticles
            for (int i = 0; i < maxFearticles; i++)
            {
                fearticle[i].LoadContent(this.Content, "textures/fearticle");
                fearticle[i].lifeSpan = 1f;
                fearticle[i].speed = new Vector2(0, 0.25f);
            }

            cursor = Content.Load<Texture2D>(@"textures/cursor");
            logo = Content.Load<Texture2D>(@"textures/logo");
            startButton = Content.Load<Texture2D>(@"textures/startButton");
            instButton = Content.Load<Texture2D>(@"textures/instButton");
            exitButton = Content.Load<Texture2D>(@"textures/exitButton");
            backButton = Content.Load<Texture2D>(@"textures/backButton");
            timerFont = Content.Load<SpriteFont>(@"Fonts/timerFont");
            yayFont = Content.Load<SpriteFont>(@"Fonts/yayFont");

            stamBarLeftEnd = Content.Load<Texture2D>(@"textures/stamBarLeft");
            stamBarCenterPart = Content.Load<Texture2D>(@"textures/stamBarCenter");
            stamBarRightEnd = Content.Load<Texture2D>(@"textures/stamBarRight");
            stamBarFill = Content.Load<Texture2D>(@"textures/stamBar");

            stamBarLeftBox = new Rectangle((int)(GraphicsDevice.Viewport.Width * 0.5f), (int)(GraphicsDevice.Viewport.Height * 0.08f), stamBarLeftEnd.Width, stamBarLeftEnd.Height);

            //Sounds
            buttonClick = Content.Load<SoundEffect>(@"Audio/Waves/button");
            buttonInstance = buttonClick.CreateInstance();
            buttonInstance.IsLooped = false;

            monsterScream = Content.Load<SoundEffect>(@"Audio/Waves/monster");
            monsterScreamInstance = monsterScream.CreateInstance();
            monsterScreamInstance.IsLooped = false;

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            mState = Mouse.GetState();
            KeyboardState keyBoardState = Keyboard.GetState();

            mousePOS.X = mState.X;
            mousePOS.Y = mState.Y;
            mouseRect = new Rectangle((int)mousePOS.X, (int)mousePOS.Y, cursor.Width, cursor.Height);
            
            // Allows the game to exit
            //this.Exit();
            
            if (screen == 0)
            {
                //main menu
                updateMainMenu();
            }
            else if (screen == 1)
            {
                //instructions
                updateInstructions();
            }
            else if (screen == 2)
            {
                //pre game, loading, processing generation
                updatePreGame(gameTime);
            }
            else if (screen == 3)
            {
                //in game
                updateInGame(gameTime, keyBoardState, lastKeyboardState);
            }
            // TODO: Add your update logic here

            lastKeyboardState = keyBoardState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            // TODO: Add your drawing code here
            
            if (screen == 0)
            {
                //main menu
                drawMainMenu();
            }
            else if (screen == 1)
            {
                //instructions
                drawInstructions();
            }
            else if (screen == 2)
            {
                //pre game, loading, processing generation
                drawPreGame();
            }
            else if (screen == 3)
            {
                //in game
                drawInGame();
            }

            spriteBatch.Draw(cursor, mousePOS, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void drawInGame()
        {
            //Draw map
            for (int x = 0; x < maxTiles; x++)
            {
                for (int y = 0; y < maxTiles; y++)
                {
                    if (tiles[x, y].position.X > -64 && tiles[x, y].position.X < GraphicsDevice.Viewport.Width + 64 && tiles[x, y].position.Y > -64 && tiles[x, y].position.Y < GraphicsDevice.Viewport.Height + 64)
                    {
                        tiles[x, y].Draw(this.spriteBatch);
                    }
                }
            }

            //Draw people
            for (int x = 0; x < maxHumans; x++)
            {
                people[x].Draw(this.spriteBatch);
            }

            //Draw playerSprite
            playerSprite.Draw(this.spriteBatch);

            //Draw fearticles
            for (int i = 0; i < maxFearticles; i++)
            {
                fearticle[i].Draw(this.spriteBatch);
            }

            //Draw fog tiles
            for (int x = 0; x < maxFOG; x++)
            {
                for (int y = 0; y < maxFOG; y++)
                {
                    if (foggies[x, y].position.X > -64 && foggies[x, y].position.X < GraphicsDevice.Viewport.Width + 64 && foggies[x, y].position.Y > -64 && foggies[x, y].position.Y < GraphicsDevice.Viewport.Height + 64)
                    {
                        float visRange = (float)Math.Sqrt(Math.Pow(Math.Abs((playerSprite.position.X + 16) - (foggies[x,y].position.X + 16)), 2) + Math.Pow(Math.Abs((playerSprite.position.Y + 32) - (foggies[x,y].position.Y + 64)), 2));//find the distance between playerSprite and foggies[x,y] 
                        if (visRange > playerSprite.fieldOfView)
                        {
                            foggies[x, y].Draw(this.spriteBatch);
                        }
                    }
                }
            }

            //Draw Stam bar
            if(playerSprite.stamina < playerSprite.scareCost+5)
            {
                spriteBatch.Draw(stamBarFill,stamBarFillBox,Color.Red);
            }
            else
            {
                spriteBatch.Draw(stamBarFill,stamBarFillBox,Color.White);
            }
            spriteBatch.Draw(stamBarLeftEnd,stamBarLeftBox,Color.White);
            spriteBatch.Draw(stamBarCenterPart,stamBarCenterBox,Color.White);
            spriteBatch.Draw(stamBarRightEnd,stamBarRightBox,Color.White);

            //Stam bar label
            spriteBatch.DrawString(timerFont, "Stamina", new Vector2(stamBarCenterBox.X, stamBarCenterBox.Y+32), Color.Yellow);


            //Draw Timer
            spriteBatch.DrawString(timerFont, "Time:", new Vector2((int)(GraphicsDevice.Viewport.Width / 10)-50, (int)GraphicsDevice.Viewport.Height / 12), Color.Yellow);
            if (currRoundTime >= 10)
            {
                spriteBatch.DrawString(timerFont, Math.Floor(currRoundTime).ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 10, (int)GraphicsDevice.Viewport.Height / 12), Color.Yellow);
            }
            else
            {
                spriteBatch.DrawString(timerFont, Math.Floor(currRoundTime).ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 10, (int)GraphicsDevice.Viewport.Height / 12), Color.Red);
            }

            //Draw scoring
            spriteBatch.DrawString(timerFont, "Score:", new Vector2((int)(GraphicsDevice.Viewport.Width / 10) + 64, (int)GraphicsDevice.Viewport.Height / 12), Color.Yellow);
            spriteBatch.DrawString(timerFont, score.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10) + 128, (int)(GraphicsDevice.Viewport.Height / 12)), Color.Yellow);
            
            spriteBatch.DrawString(timerFont, "High Score:", new Vector2((int)(GraphicsDevice.Viewport.Width / 10) + 160, (int)GraphicsDevice.Viewport.Height / 12), Color.Yellow);
            spriteBatch.DrawString(timerFont, highestScore.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10) + 275, (int)GraphicsDevice.Viewport.Height / 12), Color.Yellow);

            if (drawMessage)
            {
                if (good)
                {
                    spriteBatch.DrawString(yayFont, milestones[milestone], new Vector2(64, (int)(GraphicsDevice.Viewport.Height / 2)), Color.Aqua);
                }
                else
                {
                    spriteBatch.DrawString(yayFont, failures[failure], new Vector2(64, (int)(GraphicsDevice.Viewport.Height / 2)), Color.Red);
                }
            }

            //copyright info �
            spriteBatch.DrawString(timerFont, "� 2013 Reid Case", new Vector2(GraphicsDevice.Viewport.Width * 0.75f, GraphicsDevice.Viewport.Height * 0.9f), Color.Yellow);
        }

        private void drawPreGame()
        {
            spriteBatch.DrawString(timerFont, "Last Score: " + score.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10), (int)(GraphicsDevice.Viewport.Height / 12)), Color.Yellow);
            spriteBatch.DrawString(timerFont, "Highest Score: " + highestScore.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10), (int)(GraphicsDevice.Viewport.Height / 12) + 64), Color.Yellow);

            //spriteBatch.DrawString(timerFont, population[0].thisScore.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width/3, (int)GraphicsDevice.Viewport.Height/3), Color.White);
            spriteBatch.DrawString(timerFont, "Scare Range: " + population[0].range.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width * 0.25f, (int)(GraphicsDevice.Viewport.Height / 3)), Color.Yellow);
            spriteBatch.DrawString(timerFont, "Monster Speed: " + population[0].spd.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width * 0.25f, (int)(GraphicsDevice.Viewport.Height / 3) + 32), Color.Yellow);
            spriteBatch.DrawString(timerFont, "Max Stamina: " + population[0].maxStam.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width * 0.25f, (int)(GraphicsDevice.Viewport.Height / 3) + 64), Color.Yellow);
            spriteBatch.DrawString(timerFont, "Scare Cost: " + population[0].cost.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width * 0.25f, (int)(GraphicsDevice.Viewport.Height / 3) + 96), Color.Yellow);
            spriteBatch.DrawString(timerFont, "Stamina Regen Rate: " + population[0].sReg.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width * 0.25f, (int)(GraphicsDevice.Viewport.Height / 3) + 128), Color.Yellow);
            spriteBatch.DrawString(timerFont, "Rate of Scare: " + population[0].rateScare.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width * 0.25f, (int)(GraphicsDevice.Viewport.Height / 3) + 160), Color.Yellow);
            spriteBatch.DrawString(timerFont, "Visibility range: " + population[0].visiblity.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width * 0.25f, (int)(GraphicsDevice.Viewport.Height / 3) + 192), Color.Yellow);

            //debug - need to make sure it is actually working and not just randomly mutating population[0]
            //for (int i = 0; i < population.Length; i++)
            //{
            //    spriteBatch.DrawString(timerFont, population[i].thisScore.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60), (int)(GraphicsDevice.Viewport.Height * 0.1) + (i * 16)), Color.Yellow);
            //    spriteBatch.DrawString(timerFont, population[i].ID, new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60)+25, (int)(GraphicsDevice.Viewport.Height * 0.1) + (i * 16)), Color.Yellow);
            //    spriteBatch.DrawString(timerFont, population[i].range.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60) + 50, (int)(GraphicsDevice.Viewport.Height * 0.1)+(i*16)), Color.Yellow);
            //    spriteBatch.DrawString(timerFont, population[i].spd.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60) + 100, (int)(GraphicsDevice.Viewport.Height * 0.1) + (i * 16)), Color.Yellow);
            //    spriteBatch.DrawString(timerFont, population[i].maxStam.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60) + 150, (int)(GraphicsDevice.Viewport.Height * 0.1)+(i*16)), Color.Yellow);
            //    spriteBatch.DrawString(timerFont, population[i].cost.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60) + 200, (int)(GraphicsDevice.Viewport.Height * 0.1)+(i*16)), Color.Yellow);
            //    spriteBatch.DrawString(timerFont, population[i].sReg.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60) + 250, (int)(GraphicsDevice.Viewport.Height * 0.1)+(i*16)), Color.Yellow);
            //    spriteBatch.DrawString(timerFont, population[i].rateScare.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width * 0.60) + 300, (int)(GraphicsDevice.Viewport.Height * 0.1)+(i*16)), Color.Yellow);
            //}

            //copyright info �
            spriteBatch.DrawString(timerFont, "� 2013 Reid Case", new Vector2(GraphicsDevice.Viewport.Width * 0.75f, GraphicsDevice.Viewport.Height * 0.9f), Color.Yellow);

            spriteBatch.Draw(startButton, new Rectangle((int)startBPOS.X, (int)startBPOS.Y, startButton.Width / 2, startButton.Height), startButtonRect, Color.White);
            spriteBatch.Draw(backButton, new Rectangle((int)backBPOS.X, (int)backBPOS.Y, backButton.Width / 2, backButton.Height), backButtonRect, Color.White);
        }

        private void drawInstructions()
        {
            //Instructions
            spriteBatch.DrawString(timerFont, "Monster Mash Instructions: ", new Vector2((int)GraphicsDevice.Viewport.Width * 0.04f, (int)(GraphicsDevice.Viewport.Height / 3)), Color.Yellow);
            spriteBatch.DrawString(timerFont, "1. Boo! It's scaring time. Look at all the unsuspecting people.", new Vector2((int)GraphicsDevice.Viewport.Width * 0.04f, (int)(GraphicsDevice.Viewport.Height / 3) + 32), Color.Yellow);
            spriteBatch.DrawString(timerFont, "2. Use the space bar to scare as many as you can before the timer runs out.", new Vector2((int)GraphicsDevice.Viewport.Width * 0.04f, (int)(GraphicsDevice.Viewport.Height / 3) + 64), Color.Yellow);
            spriteBatch.DrawString(timerFont, "3. Get closer by using the W, A, S, D or the Arrow keys.", new Vector2((int)GraphicsDevice.Viewport.Width * 0.04f, (int)(GraphicsDevice.Viewport.Height / 3) + 96), Color.Yellow);
            spriteBatch.DrawString(timerFont, "4. Watch out your monster tot may mustate between rounds.", new Vector2((int)GraphicsDevice.Viewport.Width * 0.04f, (int)(GraphicsDevice.Viewport.Height / 3) + 128), Color.Yellow);
            spriteBatch.DrawString(timerFont, "5. Try to best your own personal score each round.", new Vector2((int)GraphicsDevice.Viewport.Width * 0.04f, (int)(GraphicsDevice.Viewport.Height / 3) + 160), Color.Yellow);

            //copyright info �
            spriteBatch.DrawString(timerFont, "� 2013 Reid Case", new Vector2(GraphicsDevice.Viewport.Width * 0.75f, GraphicsDevice.Viewport.Height * 0.9f), Color.Yellow);

            spriteBatch.Draw(backButton, new Rectangle((int)backBPOS.X, (int)backBPOS.Y, backButton.Width/2, backButton.Height), backButtonRect, Color.White);
        }

        private void drawMainMenu()
        {
            spriteBatch.Draw(logo, new Rectangle(GraphicsDevice.Viewport.Width/2-logo.Width/2, GraphicsDevice.Viewport.Height/4, logo.Width, logo.Height), Color.White);
            spriteBatch.Draw(startButton, new Rectangle((int)startBPOS.X, (int)startBPOS.Y, startButton.Width/2, startButton.Height), startButtonRect, Color.White);
            spriteBatch.Draw(instButton, new Rectangle((int)instBPOS.X, (int)instBPOS.Y, instButton.Width/2, instButton.Height), instButtonRect, Color.White);
            spriteBatch.Draw(exitButton, new Rectangle((int)exitBPOS.X, (int)exitBPOS.Y, exitButton.Width/2, exitButton.Height), exitButtonRect, Color.White);

            //copyright info �
            spriteBatch.DrawString(timerFont, "� 2013 Reid Case", new Vector2(GraphicsDevice.Viewport.Width * 0.75f, GraphicsDevice.Viewport.Height * 0.9f), Color.Yellow);
        }

        private void updateInGame(GameTime gameTime, KeyboardState kState, KeyboardState lKState)
        {
            newGame = false;
            if(gameTime.TotalGameTime.TotalSeconds <= roundStartTime+15)
            {


                currRoundTime = (roundStartTime+16)-(gameTime.TotalGameTime.TotalSeconds);
                if(kState.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }

                //update player sprite
                //trying to handle collision with boundaries - isCollide is not a good way to handle this
                if ((playerSprite.position.X + playerSprite.frameWidth >= tiles[maxTiles - 1, maxTiles - 1].position.X + tiles[maxTiles - 1, maxTiles - 1].frameWidth && (playerSprite.playerState == 7 || playerSprite.playerState == 6)) ||
                    (playerSprite.position.X <= tiles[0, 0].position.X && (playerSprite.playerState == 7 || playerSprite.playerState == 6)) ||
                    (playerSprite.position.Y + playerSprite.frameHeight >= tiles[maxTiles - 1, maxTiles - 1].position.Y + tiles[maxTiles - 1, maxTiles - 1].frameHeight && (playerSprite.playerState == 4 || playerSprite.playerState == 5)) ||
                    (playerSprite.position.Y <= tiles[0, 0].position.Y && (playerSprite.playerState == 4 || playerSprite.playerState == 5)))
                {
                    playerSprite.isCollide = true;
                }
                else
                {
                    playerSprite.isCollide = false;
                }

                playerSprite.Update(gameTime);

                stamBarFillBox = new Rectangle(stamBarCenterBox.X - 2, stamBarCenterBox.Y + 6, (int)playerSprite.stamina, stamBarFill.Height);

                //update tiles. Makes monster look like its moving.
                for (int x = 0; x < maxTiles; x++)
                {
                    for (int y = 0; y < maxTiles; y++)
                    {
                        
                            if ((kState.IsKeyDown(Keys.W)||kState.IsKeyDown(Keys.Up)) && tiles[x, 0].position.Y <= playerSprite.position.Y)
                            {
                                tiles[x, y].position.Y+=playerSprite.speed;
                            }
                            else if ((kState.IsKeyDown(Keys.S)||kState.IsKeyDown(Keys.Down)) && tiles[x, maxTiles - 1].position.Y + tiles[x, maxTiles - 1].frameHeight >= playerSprite.position.Y + playerSprite.frameHeight)
                            {
                                tiles[x, y].position.Y-=playerSprite.speed;
                            }
                            else if ((kState.IsKeyDown(Keys.A)||kState.IsKeyDown(Keys.Left)) && tiles[0, y].position.X <= playerSprite.position.X)
                            {
                                tiles[x, y].position.X += playerSprite.speed;
                            }
                            else if ((kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) && tiles[maxTiles - 1, y].position.X + tiles[maxTiles - 1, y].frameWidth >= playerSprite.position.X + playerSprite.frameWidth)
                            {
                                tiles[x, y].position.X -= playerSprite.speed;
                            }

                            tiles[x, y].Update(gameTime);                        
                    }
                }

                //update fog of war
                for (int x = 0; x < maxFOG; x++)
                {
                    for (int y = 0; y < maxFOG; y++)
                    {
                        if ((kState.IsKeyDown(Keys.W) || kState.IsKeyDown(Keys.Up)) && foggies[x, 0].position.Y <= playerSprite.position.Y)
                        {
                            foggies[x, y].position.Y += playerSprite.speed;
                        }
                        else if ((kState.IsKeyDown(Keys.S) || kState.IsKeyDown(Keys.Down)) && foggies[x, maxFOG - 1].position.Y + foggies[x, maxFOG - 1].frameHeight >= playerSprite.position.Y + playerSprite.frameHeight)
                        {
                            foggies[x, y].position.Y -= playerSprite.speed;
                        }
                        else if ((kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) && foggies[0, y].position.X <= playerSprite.position.X)
                        {
                            foggies[x, y].position.X += playerSprite.speed;
                        }
                        else if ((kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) && foggies[maxFOG - 1, y].position.X + foggies[maxFOG - 1, y].frameWidth >= playerSprite.position.X + playerSprite.frameWidth)
                        {
                            foggies[x, y].position.X -= playerSprite.speed;
                        }

                        foggies[x, y].Update(gameTime);
                    }
                }

                //update humans
                for (int x = 0; x < maxHumans; x++)
                {
                        //moves people with the game board
                    if ((kState.IsKeyDown(Keys.W) || kState.IsKeyDown(Keys.Up)) && !playerSprite.isCollide)
                    {
                        people[x].position.Y += playerSprite.speed;
                    }
                    else if ((kState.IsKeyDown(Keys.S) || kState.IsKeyDown(Keys.Down)) && !playerSprite.isCollide)
                    {    
                        people[x].position.Y -= playerSprite.speed;
                    }
                    else if ((kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) && !playerSprite.isCollide)
                    {
                        people[x].position.X += playerSprite.speed;
                    }
                    else if ((kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) && !playerSprite.isCollide)
                    {
                        people[x].position.X -= playerSprite.speed;
                    }

                        //this is the people brain. It controls AI lol. They are 3 lines smart.
                        if (!people[x].hasState)
                        {
                            people[x].humanState = rand.Next(8);
                            people[x].duration = rand.Next(2000);
                            people[x].hasState = true;
                        }

                        //following is collision with bounds of map
                        if (people[x].position.X > tiles[maxTiles-1,maxTiles-1].position.X)
                        {
                            people[x].position.X = tiles[maxTiles-1, maxTiles-1].position.X;
                            people[x].humanState = 7;
                        }
                        else if (people[x].position.X < tiles[0, 0].position.X)
                        {
                            people[x].position.X = tiles[0, 0].position.X;
                            people[x].humanState = 6;
                        }

                        if (people[x].position.Y > tiles[maxTiles-1, maxTiles-1].position.Y)
                        {
                            people[x].position.Y = tiles[maxTiles-1, maxTiles-1].position.Y;
                            people[x].humanState = 5;
                        }
                        else if (people[x].position.Y < tiles[0, 0].position.Y)
                        {
                            people[x].position.Y = tiles[0, 0].position.Y;
                            people[x].humanState = 4;
                        }

                        //scaring people
                        float agroRange = (float)Math.Sqrt(Math.Pow(Math.Abs((playerSprite.position.X+16)-(people[x].position.X+16)),2)+Math.Pow(Math.Abs((playerSprite.position.Y+32)-(people[x].position.Y+64)),2));//find the distance between playerSprite and people[x] 
                        if(!people[x].scared && playerSprite.isScary && playerSprite.scareRange >= agroRange){
                            score++;
                            if (people[x].humanState == 0 || people[x].humanState == 4)
                            {
                                people[x].fearDuration = mapBase.fearTime;
                                people[x].humanState = 9;
                            }
                            else if (people[x].humanState == 1 || people[x].humanState == 5)
                            {
                                people[x].fearDuration = mapBase.fearTime;
                                people[x].humanState = 8;
                            }
                            else if (people[x].humanState == 2 || people[x].humanState == 6)
                            {
                                people[x].fearDuration = mapBase.fearTime;
                                people[x].humanState = 11;
                            }
                            else if (people[x].humanState == 3 || people[x].humanState == 7)
                            {
                                people[x].fearDuration = mapBase.fearTime;
                                people[x].humanState = 10;
                            }
                            people[x].scared = true;
                               
                        }

                        people[x].Update(gameTime);                      
                }

                //Spawn fearticles
                for (int i = 0; i < maxFearticles; i++)
                {
                    if (!fearticle[i].active && playerSprite.isScary)
                    {
                        randRad = rand.Next(100);
                        randDis = rand.Next((int)playerSprite.scareRange)+1;
                        fearticle[i].fadeRate = ((float)rand.Next(10) / 1000)+0.01f;
                        fearticle[i].speed = new Vector2(0, ((float)rand.Next(10) * 0.1f)+0.01f);
                        //fearticle[i].position = new Vector2(playerSprite.position.X+(playerSprite.frameWidth/2), playerSprite.position.Y+playerSprite.frameHeight);
                        fearticle[i].position = new Vector2((float)(Math.Cos(randRad) * randDis) + (playerSprite.position.X + (playerSprite.frameWidth / 2)), (float)(Math.Sin(randRad) * randDis) + (playerSprite.position.Y + playerSprite.frameHeight));
                        fearticle[i].active = true;
                    }
                    else
                    {
                        if (kState.IsKeyDown(Keys.W) || kState.IsKeyDown(Keys.Up))
                        {
                            fearticle[i].position.Y += playerSprite.speed;
                        }
                        else if (kState.IsKeyDown(Keys.S) || kState.IsKeyDown(Keys.Down))
                        {
                            fearticle[i].position.Y -= playerSprite.speed;
                        }
                        else if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left))
                        {
                            fearticle[i].position.X += playerSprite.speed;
                        }
                        else if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right))
                        {
                            fearticle[i].position.X -= playerSprite.speed;
                        }
                    }


                    fearticle[i].Update(gameTime);
                }

                //Monster Sound
                if (playerSprite.isScary)
                {
                    monsterScreamInstance.Volume = 1f;
                    monsterScreamInstance.Play();
                    
                }

            }else{
                if(!setEndRound){
                    endRound = 0;
                    setEndRound = true;
                }
                //apply reward system here and difficulty 
                if (!canGo)
                {

                if (score >= highestScore)
                {
                    good = true;
                    if ((score - highestScore) >= 5)
                    {
                        //reward for milestone
                        if (score - highestScore >= 99)
                        {
                            milestone = 99;
                        }
                        else
                        {
                            milestone = score - highestScore;
                        }
                        canGo = false;
                        //difficulty up
                        if (!setStageProps)
                        {
                            setStageProps = true;
                            mapBase.sizeMap += 2;
                            mapBase.humanCount--;
                            mapBase.fearTime += 5;
                        }
                    }
                    else
                    {
                        milestone = 0;
                        canGo = false;
                    }
                }
                else
                {
                    good = false;
                    if (highestScore - score >= 9)
                    {
                        failure = 9;
                        canGo = false;
                    }
                    else
                    {
                        failure = highestScore - score;
                        canGo = false;
                    }
                }

                
                    drawMessage = true;
                    if (endRound >= 5000)
                    {
                        canGo = true;
                    }
                    endRound += gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    if (score > highestScore)
                    {
                        highestScore = score;
                    }
                    //dump back to pre game section
                    drawMessage = false;
                    runGACycle = false;
                    scoreSet = false;
                    screen = 2;
                    endRound = 0;
                }
            }
        }

        private void updatePreGame(GameTime gameTime)
        {
            //set up stage again
            loadStage(mapBase.sizeMap,mapBase.humanCount,mapBase.fearTime);
            setEndRound = false;
            setStageProps = false;
            canGo = false;

            if (!scoreSet)
            {
                population[0].thisScore = score;
                scoreSet = true;
            }
            //arrange all monsters by score
            bool hasFlipped = true;
            while (hasFlipped)
            {
                hasFlipped = false;
                for (int i = 0; i < population.Length - 1; i++)
                {
                    int score1 = population[i].thisScore;
                    int score2 = population[i + 1].thisScore;
                    if (score1 < score2)
                    {
                        //this works as long as you keep scoring higher each time but as soon as you let a 0 score sneak through it breaks
                        hasFlipped = true;
                        tempProps = population[i];
                        int tempScore = population[i].thisScore;
                        population[i] = population[i + 1];
                        population[i].thisScore = population[i + 1].thisScore;
                        population[i + 1] = tempProps;
                        population[i + 1].thisScore = tempScore;
                    }
                }
            }

            if (newGame)
            {
                //create random primer set of monsters to select from
                if (!selectedPop)
                {
                    for (int x = 0; x < population.Length; x++)
                    {
                        population[x].range = ((rand.Next(100)) / 10) + 20;
                        population[x].spd = ((rand.Next(20)) / 10) + 0.5f;
                        population[x].cost = (rand.Next(100)/10) + 20;
                        population[x].maxStam = (rand.Next(1000) / 10) + (population[x].cost * 2);
                        population[x].sReg = (rand.Next(20)/10);
                        population[x].rateScare = (rand.Next(100) / 10) + 50;
                        population[x].visiblity = (rand.Next(1000) / 10) + 100;
                    }
                    selectedPop = true;
                }

                //int rndInd = rand.Next(20);

                playerSprite.speed = population[0].spd;
                playerSprite.scareRange = population[0].range;
                playerSprite.maxStamina = population[0].maxStam;
                playerSprite.stamRegen = population[0].sReg;
                playerSprite.scareCost = population[0].cost;
                playerSprite.stamina = playerSprite.maxStamina;
                playerSprite.ROS = population[0].rateScare;
                playerSprite.fieldOfView = population[0].visiblity;

                //stamBarCenterBox = new Rectangle(stamBarLeftBox.X + stamBarLeftBox.Width, stamBarLeftBox.Y, (int)playerSprite.maxStamina - 4, stamBarCenterPart.Height);
                //stamBarRightBox = new Rectangle(stamBarCenterBox.X + stamBarCenterBox.Width, stamBarLeftBox.Y, stamBarRightEnd.Width, stamBarRightEnd.Height);

                score = 0;
                highestScore = 0;
            }
            else if(!newGame)
            {
                if (!runGACycle)
                {                    
                    //kill weakest
                    for (int i = 0; i < 4; i++)
                    {
                        //nulls out the lowest scoring 8 individuals
                        population[19-i].thisScore = 0;
                        population[19-i].range = 0;
                        population[19-i].spd = 0;
                        population[19-i].maxStam = 0;
                        population[19-i].cost = 0;
                        population[19-i].sReg = 0;
                        population[19-i].rateScare = 0;
                    }

                    //breed monsters
                    #region
                    for (int i = 0; i < 3; i = i + 2)
                    {
                        //offspring 1 - population[i] first half & population[i+1] second half
                        population[19 - i].thisScore = (population[i].thisScore + population[i + 1].thisScore)/2;
                        population[19 - i].range = (population[i].range+population[i+1].range)/2;
                        population[19 - i].spd = (population[i].spd+population[i+1].spd)/2;//i
                        population[19 - i].maxStam = (population[i].maxStam+population[i+1].maxStam)/2;//i
                        population[19 - i].cost = (population[i].cost+population[i+1].cost)/2;//i+1
                        population[19 - i].sReg = (population[i].sReg+population[i+1].sReg)/2;//i+1
                        population[19 - i].rateScare = (population[i].rateScare+population[i+1].rateScare)/2;//i+1
                        population[19 - i].visiblity = (population[i].visiblity+population[i+1].visiblity)/2;
                        //offspring 2 - population[i] second half & population[i+1] first half
                        population[19 - (i + 1)].thisScore = (population[i].thisScore + population[i + 1].thisScore)/2;
                        population[19 - (i + 1)].range = (population[i].range+population[i+1].range)/2;//i+1
                        population[19 - (i + 1)].spd = (population[i].spd+population[i+1].spd)/2;//i+1
                        population[19 - (i + 1)].maxStam = (population[i].maxStam+population[i+1].maxStam)/2;//i+1
                        population[19 - (i + 1)].cost = (population[i].cost+population[i+1].cost)/2;//i
                        population[19 - (i + 1)].sReg = (population[i].sReg+population[i+1].sReg)/2;//i
                        population[19 - (i + 1)].rateScare = (population[i].rateScare + population[i + 1].rateScare)/2;//i
                        population[19 - (i + 1)].visiblity = (population[i].visiblity+population[i+1].visiblity)/2;
                    }
                    #endregion
                    
                    //mutate
                    
                    //aiming for 10 iterations of mutations to occur. 
                    //each attribute has 20% chance to mutate, %50 after that to go up or 50% down by 0.1.
                    for (int i = 0; i < 10; i++)
                    {
                        //10 times do this
                        //grab random index for population
                        int rPopIndex = 0;//try this with 0 -- rand.Next(19);//I think thats correct. I may need to change this to 20 if its not getting #19

                        //population[rPopIndex] attributes - conditional on some rate will mutate randomly up or down a small fraction of a point
                        int rAttr;

                        rAttr = rand.Next(100);
                        if (rAttr <= 5)
                        {
                            population[rPopIndex].range += 10;
                        }
                        else if (rAttr > 5 && rAttr <= 10)
                        {
                            if (population[rPopIndex].range > 11)
                            {
                                population[rPopIndex].range -= 10;
                            }
                        }

                        rAttr = rand.Next(100);
                        if (rAttr <= 5)
                        {
                            population[rPopIndex].spd += 0.2f;
                        }
                        else if (rAttr > 5 && rAttr <= 10)
                        {
                            population[rPopIndex].spd -= 0.2f;
                        }

                        rAttr = rand.Next(100);
                        if (rAttr <= 5)
                        {
                            population[rPopIndex].maxStam += 0.8f;
                        }
                        else if (rAttr > 5 && rAttr <= 10)
                        {
                            if(population[rPopIndex].maxStam >= 0.9f)
                            {
                                population[rPopIndex].maxStam -= 0.8f;
                            }
                        }

                        rAttr = rand.Next(100);
                        if (rAttr <= 5)
                        {
                            population[rPopIndex].cost += 0.5f;
                        }
                        else if (rAttr > 5 && rAttr <= 10)
                        {
                            population[rPopIndex].cost -= 0.5f;
                        }

                        rAttr = rand.Next(100);
                        if (rAttr <= 5)
                        {
                            population[rPopIndex].sReg += 0.2f;
                        }
                        else if (rAttr > 5 && rAttr <= 10)
                        {
                            population[rPopIndex].sReg -= 0.2f;
                        }

                        rAttr = rand.Next(100);
                        if (rAttr <= 5)
                        {
                            population[rPopIndex].rateScare += 0.5f;
                        }
                        else if (rAttr > 5 && rAttr <= 10)
                        {
                            population[rPopIndex].rateScare -= 0.5f;
                        }

                        rAttr = rand.Next(100);
                        if (rAttr <= 5)
                        {
                            population[rPopIndex].visiblity += 32;
                        }
                        else if (rAttr > 5 && rAttr <= 10)
                        {
                            if (population[rPopIndex].visiblity >= 33)
                            {
                                population[rPopIndex].visiblity -= 32;
                            }
                        }
                    }
                    
                    //assign properties to playersprite
                    playerSprite.speed = population[0].spd;
                    playerSprite.scareRange = population[0].range;
                    playerSprite.maxStamina = population[0].maxStam;
                    playerSprite.stamRegen = population[0].sReg;
                    playerSprite.scareCost = population[0].cost;
                    playerSprite.stamina = playerSprite.maxStamina;
                    playerSprite.ROS = population[0].rateScare;
                    playerSprite.fieldOfView = population[0].visiblity;

                    //zero out score for next round
                    //score = 0;
                    //dont let it run GA again
                    runGACycle = true;
                }
            }

            stamBarCenterBox = new Rectangle(stamBarLeftBox.X + stamBarLeftBox.Width, stamBarLeftBox.Y, (int)playerSprite.maxStamina - 4, stamBarCenterPart.Height);
            stamBarRightBox = new Rectangle(stamBarCenterBox.X + stamBarCenterBox.Width, stamBarLeftBox.Y, stamBarRightEnd.Width, stamBarRightEnd.Height);

            backBPOS.X = ((GraphicsDevice.Viewport.Width / 6)*4) - backButton.Width / 4;
            startBPOS.X = backBPOS.X + 104;
            backBPOS.Y = (GraphicsDevice.Viewport.Height / 6)*5;
            startBPOS.Y = backBPOS.Y;

            backButtonBBox = new Rectangle((int)backBPOS.X, (int)backBPOS.Y, backButton.Width / 2, backButton.Height);
            startButtonBBox = new Rectangle((int)startBPOS.X, (int)startBPOS.Y, startButton.Width / 2, startButton.Height);

            if (mouseRect.Intersects(startButtonBBox))
            {

                startButtonRect = new Rectangle(startButton.Width / 2, 0, startButton.Width / 2, startButton.Height);
                if (mState.LeftButton == ButtonState.Pressed && !mouseRect.Intersects(backButtonBBox))
                {
                    buttonInstance.Volume = 0.75f;
                    buttonInstance.Play();
                    screen = 3;
                    roundStartTime = gameTime.TotalGameTime.TotalSeconds;
                    //zero out score for next round
                    score = 0;
                }
            }
            else
            {
                startButtonRect = new Rectangle(0, 0, startButton.Width / 2, startButton.Height);
            }

            if (mouseRect.Intersects(backButtonBBox))
            {

                backButtonRect = new Rectangle(backButton.Width / 2, 0, backButton.Width / 2, backButton.Height);
                if (mState.LeftButton == ButtonState.Pressed && !mouseRect.Intersects(startButtonBBox))
                {
                    buttonInstance.Volume = 0.75f;
                    buttonInstance.Play();
                    screen = 0;
                }
            }
            else
            {
                backButtonRect = new Rectangle(0, 0, backButton.Width / 2, backButton.Height);
            }

            // player selects monster to play
            //player presses start
            //start inGame with selected monster
        }

        private void updateInstructions()
        {
            backBPOS.X = GraphicsDevice.Viewport.Width / 2 - backButton.Width / 4;
            backBPOS.Y = GraphicsDevice.Viewport.Height/6;

            backButtonBBox = new Rectangle((int)backBPOS.X, (int)backBPOS.Y, backButton.Width/2, backButton.Height);

            if (mouseRect.Intersects(backButtonBBox))
            {

                backButtonRect = new Rectangle(backButton.Width / 2, 0, backButton.Width / 2, backButton.Height);
                if (mState.LeftButton == ButtonState.Pressed)
                {
                    buttonInstance.Volume = 0.75f;
                    buttonInstance.Play();
                    screen = 0;
                }
            }else{
                backButtonRect = new Rectangle(0, 0, backButton.Width/2, backButton.Height);
            }
        }

        private void updateMainMenu()
        {
            //new Rectangle(GraphicsDevice.Viewport.Width/2-startButton.Width/4,GraphicsDevice.Viewport.Height/4+80,startButton.Width/2,startButton.Height)
            
            startBPOS.X = GraphicsDevice.Viewport.Width / 2 - startButton.Width / 4;
            instBPOS.X = startBPOS.X;
            exitBPOS.X = startBPOS.X;
            startBPOS.Y = GraphicsDevice.Viewport.Height / 4 + 80;
            instBPOS.Y = startBPOS.Y + 40;
            exitBPOS.Y = instBPOS.Y + 40;
            
            startButtonBBox = new Rectangle((int)startBPOS.X, (int)startBPOS.Y, startButton.Width/2, startButton.Height);
            instButtonBBox = new Rectangle((int)instBPOS.X, (int)instBPOS.Y, instButton.Width/2, instButton.Height);
            exitButtonBBox = new Rectangle((int)exitBPOS.X, (int)exitBPOS.Y, exitButton.Width/2, exitButton.Height);
            
            if (mouseRect.Intersects(startButtonBBox))
            {
                
                startButtonRect = new Rectangle(startButton.Width / 2, 0, startButton.Width / 2, startButton.Height);
                if (mState.LeftButton == ButtonState.Pressed && !mouseRect.Intersects(instButtonBBox))
                {
                    buttonInstance.Volume = 0.75f;
                    buttonInstance.Play();
                    screen = 2;
                }
            }
            else
            {
                startButtonRect = new Rectangle(0, 0, startButton.Width/2, startButton.Height);
            }
            
            if (mouseRect.Intersects(instButtonBBox))
            {
                
                instButtonRect = new Rectangle(instButton.Width / 2, 0, instButton.Width / 2, instButton.Height);
                if (mState.LeftButton == ButtonState.Pressed && !mouseRect.Intersects(exitButtonBBox) && !mouseRect.Intersects(startButtonBBox))
                {
                    buttonInstance.Volume = 0.75f;
                    buttonInstance.Play();
                    screen = 1;
                }
            }
            else
            {
                instButtonRect = new Rectangle(0, 0, instButton.Width / 2, instButton.Height);
            }
            
            if (mouseRect.Intersects(exitButtonBBox))
            {
                
                exitButtonRect = new Rectangle(exitButton.Width / 2, 0, exitButton.Width / 2, exitButton.Height);
                if(mState.LeftButton == ButtonState.Pressed && !mouseRect.Intersects(instButtonBBox))
                {
                    buttonInstance.Volume = 0.75f;
                    buttonInstance.Play();
                    this.Exit();
                }
            }
            else
            {
                exitButtonRect = new Rectangle(0, 0, exitButton.Width / 2, exitButton.Height);
            }
        }

        private void loadStage(int mapSize, int humanCount, int fearTime)
        {
            maxTiles = mapBase.sizeMap;
            tiles = new Tile[maxTiles, maxTiles];

            maxFOG = (maxTiles * 2) + 1;
            foggies = new Tile[maxFOG, maxFOG];

            //Init Tiles
            for (int x = 0; x < maxTiles; x++)
            {
                for (int y = 0; y < maxTiles; y++)
                {
                    //Init ground tiles
                    tiles[x, y] = new Tile();
                }
            }

            //Init fog tiles
            for (int x = 0; x < maxFOG; x++)
            {
                for (int y = 0; y < maxFOG; y++)
                {
                    foggies[x, y] = new Tile();
                }
            }

            //Init people

            maxHumans = mapBase.humanCount;
            people = new Human[maxHumans];

            for (int x = 0; x < maxHumans; x++)
            {
                people[x] = new Human();
            }

            //loading map tiles
            for (int x = 0; x < maxTiles; x++)
            {
                for (int y = 0; y < maxTiles; y++)
                {
                    //Load ground tiles
                    tiles[x, y].LoadContent(this.Content, "textures/groundTile");
                    tiles[x, y].frameWidth = 32;//for now
                    tiles[x, y].frameHeight = 32;//for now
                    tiles[x, y].position.X = tiles[x, y].frameWidth * x;
                    tiles[x, y].position.Y = tiles[x, y].frameHeight * y;
                    tiles[x, y].maxFrames = 0;//hopefully no animation for now as a test but I think I may animate eventually
                    tiles[x, y].frameIndex = 0;//probably wont have more than this for this sprite sheet but maybe if I want different terrain types
                }
            }

            //load fog tiles
            for (int x = 0; x < maxFOG; x++)
            {
                for (int y = 0; y < maxFOG; y++)
                {
                    foggies[x, y].LoadContent(this.Content, "textures/fogOfWar");
                    foggies[x, y].frameWidth = 64;//for now
                    foggies[x, y].frameHeight = 64;//for now
                    foggies[x, y].position.X = ((foggies[x, y].frameWidth / 4) * x) - foggies[x, y].frameWidth / 4;
                    foggies[x, y].position.Y = ((foggies[x, y].frameHeight / 4) * y) - foggies[x, y].frameHeight / 4;
                    foggies[x, y].maxFrames = 0;//hopefully no animation for now as a test but I think I may animate eventually
                    foggies[x, y].frameIndex = 0;//probably wont have more than this for this sprite sheet but maybe if I want different terrain types
                }
            }

            //load humans
            for (int x = 0; x < maxHumans; x++)
            {
                int randX = rand.Next((int)tiles[maxTiles - 1, maxTiles - 1].position.X);
                int randY = rand.Next((int)tiles[maxTiles - 1, maxTiles - 1].position.Y);
                int randIndex = rand.Next(4);
                people[x].LoadContent(this.Content, "textures/humans");
                people[x].frameWidth = 32;
                people[x].frameHeight = 64;
                people[x].position.X = randX;
                people[x].position.Y = randY;
                people[x].maxFrames = 3;
                people[x].frameIndexOrigin = randIndex * 3;
                people[x].fearDuration = mapBase.fearTime;
            }
        }

        private void initYayStrings()
        {
            //init failures
            failures[0] = "meh...";
            failures[1] = "Not scary enough";
            failures[2] = "Deficient in fear excretion";
            failures[3] = "You'd do better as a fairy";
            failures[4] = "Turkeys scare rocks better than you scare people";
            failures[5] = "Your scaring career is sinking like a lead balloon";
            failures[6] = "Why are you still playing?";
            failures[7] = "Total lack of coordination";
            failures[8] = "Your children will lose all their Facebook friends because of you";
            failures[9] = "ULTIMATE FAIL!!!!!";
            
            //init milestones
            milestones[0] = "Time is up";
            milestones[1] = "standard";
            milestones[2] = "Acceptable";
            milestones[3] = "Decent";
            milestones[4] = "Better";
            milestones[5] = "Pretty Good";
            milestones[6] = "Good";
            milestones[7] = "Getting there";
            milestones[8] = "Way to go!";
            milestones[9] = "Worthy";
            milestones[10] = "Nice";
            milestones[11] = "Snake Eyes";
            milestones[12] = "Fearful Performance";
            milestones[13] = "Satisfactory";
            milestones[14] = "Kudos";
            milestones[15] = "Congratulations";
            milestones[16] = "Measurable Achievement";
            milestones[17] = "Stroke it!";
            milestones[18] = "Scare it up!";
            milestones[19] = "Do the deed";
            milestones[20] = "Wow!";
            milestones[21] = "AHHHHHHHH!!!!!!!";
            milestones[22] = "You brush your teeth with Harley Davidsons";
            milestones[23] = "No children will be born as you rein";
            milestones[24] = "Cast Fear!";
            milestones[25] = "Squirrels Dash from Your Presence";
            milestones[26] = "Wonderful";
            milestones[27] = "Host of Halloween Parties";
            milestones[28] = "Shivering souls";
            milestones[29] = "Skeletal Cracking";
            milestones[30] = "Better, Wow!";
            milestones[31] = "Island Sinker";
            milestones[32] = "Skull Crusher";
            milestones[33] = "Grrrreat";
            milestones[34] = "Woof!";
            milestones[35] = "The best!";
            milestones[36] = "Heart Eater";
            milestones[37] = "Intern Smasher";
            milestones[38] = "Monster";
            milestones[39] = "No Peace";
            milestones[40] = "Effortless Frightening";
            milestones[41] = "Extreme Shock";
            milestones[42] = "Faint";
            milestones[43] = "Like taking candy from a baby";
            milestones[44] = "How did you get so good?";
            milestones[45] = "You type on the keyboard";
            milestones[46] = "That's how to do it!";
            milestones[47] = "Sleeping Dragon Whips its Tail";
            milestones[48] = "Stop for me, please";
            milestones[49] = "Too much!";
            milestones[50] = "Masterpiece of fear";
            milestones[51] = "Paint their blood on the curtains";
            milestones[52] = "Their hopes are gone";
            milestones[53] = "Degauss their faces!";
            milestones[54] = "Infectious";
            milestones[55] = "Crows Mothers Cry at the Thought of You";
            milestones[56] = "Howl at the moon";
            milestones[57] = "The road to devastation";
            milestones[58] = "Zombie-like Consumption";
            milestones[59] = "Hatred";
            milestones[60] = "Reid is Awesome!";
            milestones[61] = "Your success brings value to us all";
            milestones[62] = "Play the music";
            milestones[63] = "Dig their graves";
            milestones[64] = "Loudly the dog whined";
            milestones[65] = "Startling gift";
            milestones[66] = "Impossible Manifestation of Surprise";
            milestones[67] = "Rise form the catacomb";
            milestones[68] = "Molten iron bleeds from the cold heart";
            milestones[69] = "Alarm";
            milestones[70] = "Prodigy";
            milestones[71] = "Vicious";
            milestones[72] = "Unify societies to bring total fear to all";
            milestones[73] = "Our children should run";
            milestones[74] = "Rats and Bats";
            milestones[75] = "Potent Panic Placement!";
            milestones[76] = "Poisonous Protrusion";
            milestones[77] = "Drain the lizard";
            milestones[78] = "Cup of tea";
            milestones[79] = "Drop the refrigerator!";
            milestones[80] = "Official Fear Progenitor";
            milestones[81] = "Flatten the world!";
            milestones[82] = "Gasoline breathing fire lizard!";
            milestones[83] = "King Reign";
            milestones[84] = "Spill the salt!";
            milestones[85] = "Measure the Cow";
            milestones[86] = "Black Cats";
            milestones[87] = "Big Dog!";
            milestones[88] = "Hammer Fright into Their Brains";
            milestones[89] = "Demonic Mannerism";
            milestones[90] = "Slap them with the glove";
            milestones[91] = "They sacrifice to you";
            milestones[92] = "Clad in the Tears of One Legged Infants";
            milestones[93] = "May your efforts cascade through time";
            milestones[94] = "Fright Night";
            milestones[95] = "Holy Panic";
            milestones[96] = "Infinite Progress";
            milestones[97] = "All hail our fearful master!";
            milestones[98] = "The Fear Prevails over Solace";
            milestones[99] = "Fear and Mortification Precede You, Your Victims Lathered in Terror!";
        }
    }
}
