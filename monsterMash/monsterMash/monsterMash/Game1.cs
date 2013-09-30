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
            playerSprite = new Monster();
            
            mState = Mouse.GetState();
            lastKeyboardState = Keyboard.GetState();
            screen = 0;
            newGame = true;

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

            //player = new Animation(Content.Load<Texture2D>(@"textures/monsterBaseForward"), new Vector2((GraphicsDevice.Viewport.Width / 2) - 8, (GraphicsDevice.Viewport.Height / 2) - 8), 16, 16);//texture,position,frame height,frame width
            playerSprite.LoadContent(this.Content, "textures/monsterBase");
            playerSprite.position = new Vector2((GraphicsDevice.Viewport.Width / 2) - 16, (GraphicsDevice.Viewport.Height / 2) - 16);
            playerSprite.frameHeight = 32;
            playerSprite.frameWidth = 32;
            playerSprite.maxFrames = 3;//like array index starts at 0
            playerSprite.frameIndex = 0;//like array index starts at 0
            playerSprite.maxHP = 100;//set up max hp for round. Will change eventually from algorithm.
            playerSprite.HP = playerSprite.maxHP;//set up hp for round

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
            playerSprite.Draw(this.spriteBatch);

            spriteBatch.DrawString(timerFont, Math.Floor(currRoundTime).ToString(), new Vector2((int)GraphicsDevice.Viewport.Width / 10, (int)GraphicsDevice.Viewport.Height / 12), Color.White);
            
        }

        private void drawPreGame()
        {
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
            if(gameTime.TotalGameTime.TotalSeconds <= roundStartTime+120)
            {
                currRoundTime = (roundStartTime+120)-(gameTime.TotalGameTime.TotalSeconds);
                if(kState.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }

                playerSprite.Update(gameTime);
                

            }else{
                //dump back to pre game section
                screen = 2;
            }
        }

        private void updatePreGame(GameTime gameTime)
        {
            if (newGame)
            {
                //create random primer set of monsters to select from
            }
            else
            {
                //calculate and apply score to previously played monster
                //arrange all monsters by score
                //kill weakest
                //breed monsters
                //rearrange by score
                //mutate
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
