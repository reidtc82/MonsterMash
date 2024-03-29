﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monsterMash
{
    class Monster:Sprite
    {
        Texture2D mSpriteTexture;

        public int playerState = 0;
        Vector2[] states = new Vector2[20];
        int lastDirection;

        private int elapsedFrameTime = 0;
        int frameRateInterval = 90;
        public int currentFrame;
        public int frameFirst;
        private  int thisDirection;
        private  int oldDirection;

        public bool isScary;
        private double lastScare;
        private int regenDelay;

        public bool isCollide;
        
        public float scareRange 
        { 
            get; 
            set; 
        }

        public int HP
        {
            get;
            set;
        }

        public int maxHP
        {
            get;
            set;
        }

        public float stamina
        {
            get;
            set;
        }

        public float stamRegen
        {
            get;
            set;
        }

        public float scareCost
        {
            get;
            set;
        }

        public float maxStamina
        {
            get;
            set;
        }

        public float ROS 
        { 
            get; 
            set; 
        }

        public float fieldOfView
        {
            get;
            set;
        }

        public virtual void LoadContent(ContentManager contentManager, string assetName)
        {
            mSpriteTexture = contentManager.Load<Texture2D>(assetName);
            frameFirst = 0;
            currentFrame = frameFirst;
            lastDirection = frameFirst;
            //scareRange = 128;

            states[0] = new Vector2(4,0);
            states[1] = new Vector2(0,0);
            states[2] = new Vector2(12,0);
            states[3] = new Vector2(8,0);
            states[4] = new Vector2(4,1);
            states[5] = new Vector2(0,1);
            states[6] = new Vector2(12,1);
            states[7] = new Vector2(8,1);
            states[8] = new Vector2(4,2);
            states[9] = new Vector2(0,2);
            states[10] = new Vector2(12,2);
            states[11] = new Vector2(8,2);
            states[12] = new Vector2(4,3);
            states[13] = new Vector2(0,3);
            states[14] = new Vector2(12,3);
            states[15] = new Vector2(8,3);
            states[16] = new Vector2(4,4);
            states[17] = new Vector2(0,4);
            states[18] = new Vector2(12,4);
            states[19] = new Vector2(8,4);
        }

        public virtual void Update(GameTime gameTime){
            KeyboardState keyboardState = Keyboard.GetState();

            //on press events
                //direction
                    //w a s d - triggers walk in that directon until release
                //attack
                    //space bar? - triggers attack in stored direction, overrides walk, only plays once for 4 frames. 
            //on release events
                //w a s d - store direction return to standing
                //space bar - keep attack state for 4 frames then return to standing in stored direction.

            //takes damage - override walk, stand, attack. use stored direction retain direction.
            //die - override walk, stand, attack, take damage. Use stored direction retain direction. Play through once then stop and dont return. Cease all player input listeners.

            if (HP <= 0)
            {
                //die
                if(lastDirection == 4){
                    playerState = 16;
                }else if(lastDirection == 0){
                    playerState = 17;
                }else if(lastDirection == 12){
                    playerState = 18;
                }else if(lastDirection == 8){
                    playerState = 19;
                }
            }
            else
            {
                //add taking damage around this whole section.

                //resting states.
                if(lastDirection == 4)
                {
                    playerState = 0;
                }else if(lastDirection == 0)
                {
                    playerState = 1;
                }else if(lastDirection == 12)
                {
                    playerState = 2;
                }else if(lastDirection == 8)
                {
                    playerState = 3;
                }

                //listen for input
                if (keyboardState.IsKeyDown(Keys.Space))
                {

                    if (lastDirection == 4)
                    {
                        playerState = 8;
                    }
                    if (lastDirection == 0)
                    {
                        playerState = 9;
                    }
                    if (lastDirection == 12)
                    {
                        playerState = 10;
                    }
                    if (lastDirection == 8)
                    {
                        playerState = 11;
                    }
                }
                else
                {
                    if(keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                    {
                        playerState = 4;
                    }
                    else if(keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                    {
                        playerState = 6;
                    }
                    else if(keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                    {
                        playerState = 5;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                    {
                        playerState = 7;
                    }
                }

                lastDirection = frameFirst;
                frameFirst = (int)states[playerState].X;
                frameIndex = (int)states[playerState].Y;
            }

            maxFrames = 3 + frameFirst;
            elapsedFrameTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (elapsedFrameTime >= frameRateInterval)
            {
                if (currentFrame < maxFrames && thisDirection == oldDirection)
                {
                    thisDirection = frameFirst;
                    currentFrame++;
                }
                else if(currentFrame >= maxFrames || thisDirection != oldDirection)
                {
                    oldDirection = thisDirection;
                    currentFrame = frameFirst;
                }
                elapsedFrameTime = 0;
            }

            lastScare += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (lastScare >= ROS)//this doesnt work copy from above. needs delay for scare 
            {
                if (keyboardState.IsKeyDown(Keys.Space) && stamina >= scareCost)
                {
                    isScary = true;
                    stamina = stamina - scareCost;
                }
                else
                {
                    isScary = false;
                }

                lastScare = 0;
            }

            regenDelay += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (stamina < maxStamina && regenDelay >= 10)//need small delay or base this on time increment not update or make rate much smaller.
            {
                stamina += stamRegen;
                regenDelay = 0;
            }
            

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mSpriteTexture, new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight), new Rectangle(currentFrame * frameWidth, frameIndex * frameHeight, frameWidth, frameHeight), Color.White);
        }
    }
}
