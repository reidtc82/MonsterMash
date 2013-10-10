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

        Monster playerSprite;

        struct monsterProps
        {
            public float range;
            public float spd;
            public int thisScore;
            public int stam;
            public int sReg;
            public int cost;
            public int maxStam;
            public int rateScare;
        }

        monsterProps[] population = new monsterProps[20];

        const int maxHumans = 128;
        Human[] people = new Human[maxHumans];

        const int maxTiles = 64;
        Tile[,] tiles = new Tile[maxTiles,maxTiles];

        private bool newGame;

        private Texture2D cursor;
        private Texture2D logo;
        private Texture2D startButton;
        private Texture2D instButton;
        private Texture2D exitButton;
        private Texture2D backButton;

        private Vector2 mousePOS;
        private Vector2 startBPOS;
        private Vector2 instBPOS;
        private Vector2 exitBPOS;
        private Vector2 backBPOS;

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

        private double roundStartTime;
        private double currRoundTime;
        private SpriteFont timerFont;

        Random rand = new Random();

        int score;
        int highestScore;
        private bool selectedPop;
        private bool runGACycle;

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

            playerSprite = new Monster();

            for (int x = 0; x < maxTiles; x++)
            {
                for (int y = 0; y < maxTiles; y++)
                {
                    tiles[x, y] = new Tile();
                }
            }

            for (int x = 0; x < maxHumans; x++)
            {
                people[x] = new Human();
            }

            mState = Mouse.GetState();
            lastKeyboardState = Keyboard.GetState();
            screen = 0;
            newGame = true;

            for (int x = 0; x < population.Length; x++)
            {
                population[x] = new monsterProps();
            }

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

            //loading map tiles
            for (int x = 0; x < maxTiles; x++)
            {
                for (int y = 0; y < maxTiles; y++)
                {
                    tiles[x, y].LoadContent(this.Content, "textures/groundTile");
                    tiles[x, y].frameWidth = 32;//for now
                    tiles[x, y].frameHeight = 32;//for now
                    tiles[x, y].position.X = tiles[x, y].frameWidth * x;
                    tiles[x, y].position.Y = tiles[x, y].frameHeight * y;
                    tiles[x, y].maxFrames = 0;//hopefully no animation for now as a test but I think I may animate eventually
                    tiles[x, y].frameIndex = 0;//probably wont have more than this for this sprite sheet but maybe if I want different terrain types
                }
            }

            //load humans
            for (int x = 0; x < maxHumans; x++)
            {
                int randX = rand.Next((int)tiles[maxTiles-1,maxTiles-1].position.X);
                int randY = rand.Next((int)tiles[maxTiles-1,maxTiles-1].position.Y);
                int randIndex = rand.Next(4);
                people[x].LoadContent(this.Content, "textures/humans");
                people[x].frameWidth = 32;
                people[x].frameHeight = 64;
                people[x].position.X = randX;
                people[x].position.Y = randY;
                people[x].maxFrames = 3;
                people[x].frameIndexOrigin = randIndex * 3;
            }

            cursor = Content.Load<Texture2D>(@"textures/cursor");
            logo = Content.Load<Texture2D>(@"textures/logo");
            startButton = Content.Load<Texture2D>(@"textures/startButton");
            instButton = Content.Load<Texture2D>(@"textures/instButton");
            exitButton = Content.Load<Texture2D>(@"textures/exitButton");
            backButton = Content.Load<Texture2D>(@"textures/backButton");
            timerFont = Content.Load<SpriteFont>(@"Fonts/timerFont");
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

            for (int x = 0; x < maxHumans; x++)
            {
                people[x].Draw(this.spriteBatch);
            }

            playerSprite.Draw(this.spriteBatch);

            if (currRoundTime >= 10)
            {
                spriteBatch.DrawString(timerFont, Math.Floor(currRoundTime).ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 10, (int)GraphicsDevice.Viewport.Height / 12), Color.White);
            }
            else
            {
                spriteBatch.DrawString(timerFont, Math.Floor(currRoundTime).ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 10, (int)GraphicsDevice.Viewport.Height / 12), Color.Red);
            }
            spriteBatch.DrawString(timerFont, score.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10)+64, (int)(GraphicsDevice.Viewport.Height / 12)), Color.White);
            spriteBatch.DrawString(timerFont, highestScore.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10)+128, (int)GraphicsDevice.Viewport.Height / 12), Color.White);
            spriteBatch.DrawString(timerFont, playerSprite.stamina.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10) + 192, (int)GraphicsDevice.Viewport.Height / 12), Color.White);
        }

        private void drawPreGame()
        {
            spriteBatch.DrawString(timerFont, "Last Score: "+score.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10), (int)(GraphicsDevice.Viewport.Height / 12)), Color.White);
            spriteBatch.DrawString(timerFont, "Highest Score: "+highestScore.ToString(), new Vector2((int)(GraphicsDevice.Viewport.Width / 10), (int)(GraphicsDevice.Viewport.Height / 12)+64), Color.White);

            //spriteBatch.DrawString(timerFont, population[0].thisScore.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width/3, (int)GraphicsDevice.Viewport.Height/3), Color.White);
            spriteBatch.DrawString(timerFont, "Scare Range: "+population[0].range.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 3, (int)(GraphicsDevice.Viewport.Height / 3)), Color.White);
            spriteBatch.DrawString(timerFont, "Monster Speed: "+population[0].spd.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 3, (int)(GraphicsDevice.Viewport.Height / 3)+32), Color.White);
            spriteBatch.DrawString(timerFont, "Max Stamina: " + population[0].maxStam.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 3, (int)(GraphicsDevice.Viewport.Height / 3)+64), Color.White);
            spriteBatch.DrawString(timerFont, "Scare Cost: " + population[0].cost.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 3, (int)(GraphicsDevice.Viewport.Height / 3)+96), Color.White);
            spriteBatch.DrawString(timerFont, "Stamina Regen Rate: " + population[0].sReg.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 3, (int)(GraphicsDevice.Viewport.Height / 3)+128), Color.White);
            spriteBatch.DrawString(timerFont, "Rate of Scare: " + population[0].rateScare.ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 3, (int)(GraphicsDevice.Viewport.Height / 3) + 160), Color.White);

            spriteBatch.Draw(startButton, new Rectangle((int)startBPOS.X, (int)startBPOS.Y, startButton.Width / 2, startButton.Height), startButtonRect, Color.White);
            spriteBatch.Draw(backButton, new Rectangle((int)backBPOS.X, (int)backBPOS.Y, backButton.Width / 2, backButton.Height), backButtonRect, Color.White);
        }

        private void drawInstructions()
        {
            spriteBatch.Draw(backButton, new Rectangle((int)backBPOS.X, (int)backBPOS.Y, backButton.Width/2, backButton.Height), backButtonRect, Color.White);
        }

        private void drawMainMenu()
        {
            spriteBatch.Draw(logo, new Rectangle(GraphicsDevice.Viewport.Width/2-logo.Width/2, GraphicsDevice.Viewport.Height/4, logo.Width, logo.Height), Color.White);
            spriteBatch.Draw(startButton, new Rectangle((int)startBPOS.X, (int)startBPOS.Y, startButton.Width/2, startButton.Height), startButtonRect, Color.White);
            spriteBatch.Draw(instButton, new Rectangle((int)instBPOS.X, (int)instBPOS.Y, instButton.Width/2, instButton.Height), instButtonRect, Color.White);
            spriteBatch.Draw(exitButton, new Rectangle((int)exitBPOS.X, (int)exitBPOS.Y, exitButton.Width/2, exitButton.Height), exitButtonRect, Color.White);
        }

        private void updateInGame(GameTime gameTime, KeyboardState kState, KeyboardState lKState)
        {
            newGame = false;
            if(gameTime.TotalGameTime.TotalSeconds <= roundStartTime+30)
            {
                currRoundTime = (roundStartTime+31)-(gameTime.TotalGameTime.TotalSeconds);
                if(kState.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }

                //update player sprite
                playerSprite.Update(gameTime);

                //update tiles. Makes monster look like its moving.
                for (int x = 0; x < maxTiles; x++)
                {
                    for (int y = 0; y < maxTiles; y++)
                    {
                        
                            if (kState.IsKeyDown(Keys.W) && tiles[x, 0].position.Y <= playerSprite.position.Y)
                            {
                                tiles[x, y].position.Y+=playerSprite.speed;
                            }
                            else if (kState.IsKeyDown(Keys.S) && tiles[x, maxTiles - 1].position.Y + tiles[x, maxTiles - 1].frameHeight >= playerSprite.position.Y + playerSprite.frameHeight)
                            {
                                tiles[x, y].position.Y-=playerSprite.speed;
                            }
                            else if (kState.IsKeyDown(Keys.A) && tiles[0, y].position.X <= playerSprite.position.X)
                            {
                                tiles[x, y].position.X += playerSprite.speed;
                            }
                            else if (kState.IsKeyDown(Keys.D) && tiles[maxTiles - 1, y].position.X + tiles[maxTiles - 1, y].frameWidth >= playerSprite.position.X + playerSprite.frameWidth)
                            {
                                tiles[x, y].position.X -= playerSprite.speed;
                            }
                            tiles[x, y].Update(gameTime);
                        
                    }
                }

                //update humans
                for (int x = 0; x < maxHumans; x++)
                {
                        //moves people with the game board
                        if (kState.IsKeyDown(Keys.W))
                        {
                            people[x].position.Y += playerSprite.speed;
                        }
                        else if (kState.IsKeyDown(Keys.S))
                        {
                            people[x].position.Y -= playerSprite.speed;
                        }
                        else if (kState.IsKeyDown(Keys.A))
                        {
                            people[x].position.X += playerSprite.speed;
                        }
                        else if (kState.IsKeyDown(Keys.D))
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
                                people[x].duration = 200;
                                people[x].humanState = 9;
                            }
                            else if (people[x].humanState == 1 || people[x].humanState == 5)
                            {
                                people[x].duration = 200;
                                people[x].humanState = 8;
                            }
                            else if (people[x].humanState == 2 || people[x].humanState == 6)
                            {
                                people[x].duration = 200;
                                people[x].humanState = 11;
                            }
                            else if (people[x].humanState == 3 || people[x].humanState == 7)
                            {
                                people[x].duration = 200;
                                people[x].humanState = 10;
                            }
                            people[x].scared = true;
                               
                        }

                        people[x].Update(gameTime);
                    
                }

            }else{
                if (score > highestScore)
                {
                    highestScore = score;
                }
                //dump back to pre game section
                runGACycle = false;
                screen = 2;
            }
        }

        private void updatePreGame(GameTime gameTime)
        {
            if (newGame)
            {
                //create random primer set of monsters to select from
                if (!selectedPop)
                {
                    for (int x = 0; x < population.Length; x++)
                    {
                        population[x].range = ((rand.Next(100)) / 10) + 0.5f;
                        population[x].spd = ((rand.Next(20)) / 10) + 0.5f;
                        population[x].maxStam = (rand.Next(1000)/10) + 1;
                        population[x].cost = (rand.Next(100)/10) + 20;
                        population[x].sReg = (rand.Next(20)/10) + 1;
                        population[x].rateScare = (rand.Next(100) / 10) + 20;
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

                score = 0;
                highestScore = 0;
            }
            else
            {
                if (!runGACycle)
                {
                    //calculate and apply score to previously played monster
                    population[0].thisScore = score;
                    //arrange all monsters by score
                    
                    //kill weakest
                    for (int i = 0; i < 9; i++)
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
                    //rearrange by score
                    //mutate
                    population[0].spd++;//debug
                    population[0].range++;//debug
                    //assign properties to playersprite
                    playerSprite.speed = population[0].spd;
                    playerSprite.scareRange = population[0].range;
                    playerSprite.maxStamina = population[0].maxStam;
                    playerSprite.stamRegen = population[0].sReg;
                    playerSprite.scareCost = population[0].cost;
                    playerSprite.stamina = playerSprite.maxStamina;
                    playerSprite.ROS = population[0].rateScare;

                    score = 0;
                    runGACycle = true;
                }
            }

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
                    screen = 3;
                    roundStartTime = gameTime.TotalGameTime.TotalSeconds;
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
                    this.Exit();
                }
            }
            else
            {
                exitButtonRect = new Rectangle(0, 0, exitButton.Width / 2, exitButton.Height);
            }
        }
    }
}
