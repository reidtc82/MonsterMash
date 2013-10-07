using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monsterMash
{
    class Human:Sprite
    {

        public Vector2 position = new Vector2(0, 0);
        Vector2[] states = new Vector2[12];

        private int frameRateInterval = 120;
        int currentFrame;
        int frameFirst;
        public int maxFrames;
        public int frameWidth;
        public int frameHeight;
        private int elapsedFrameTime = 0;
        private int elapsedDuration = 0;
        public int frameIndexOrigin;
        private int frameIndex;
        //needs collision rect

        private Texture2D mSpriteTexture;

        Random rand = new Random();
        public int humanState
        {
            get;
            set;
        }
        public bool hasState
        {
            get;
            set;
        }
        private int direction
        {
            get;
            set;
        }
        public int duration
        {
            get;
            set;
        }

        public bool scared
        {
            get;
            set;
        }

        public void LoadContent(ContentManager contentManager, string assetName)
        {
            mSpriteTexture = contentManager.Load<Texture2D>(assetName);
            frameFirst = 0;
            currentFrame = frameFirst;
            hasState = false;
            speed = 1;
        }

        public void Update(GameTime gameTime)//they all switch state at the same time
        {
            if (scared)
            {
                speed = 2;
                if (duration > 0)
                {
                    frameIndex = frameIndexOrigin + 2;
                    if (humanState == 8)
                    {
                        position.Y-=speed;
                    }
                    else if (humanState == 9)
                    {
                        position.Y+=speed;
                    }
                    else if (humanState == 10)
                    {
                        position.X-=speed;
                    }
                    else if (humanState == 11)
                    {
                        position.X+=speed;
                    }

                    duration--;
                }
                else
                {
                    scared = false;
                    hasState = false;
                }
            }
            else
            {
                speed = 1;
                if (duration > 0)
                {
                    if (humanState <= 3)
                    {
                        frameIndex = frameIndexOrigin;
                        if (humanState == 0)
                        {
                            frameFirst = 0;
                        }
                        else if (humanState == 1)
                        {
                            frameFirst = 4;
                        }
                        else if (humanState == 2)
                        {
                            frameFirst = 8;
                        }
                        else if (humanState == 3)
                        {
                            frameFirst = 12;
                        }
                    }
                    else if (humanState > 3 && humanState <= 7)
                    {
                        frameIndex = frameIndexOrigin + 1;
                        if (humanState == 4)
                        {
                            frameFirst = 0;
                            position.Y+=speed;
                        }
                        else if (humanState == 5)
                        {
                            frameFirst = 4;
                            position.Y-=speed;
                        }
                        else if (humanState == 6)
                        {
                            frameFirst = 8;
                            position.X+=speed;
                        }
                        else if (humanState == 7)
                        {
                            frameFirst = 12;
                            position.X-=speed;
                        }
                    }
                    duration--;
                }
                else
                {
                    hasState = false;
                }
            }

            maxFrames = 3 + frameFirst;
            elapsedFrameTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (elapsedFrameTime >= frameRateInterval)
            {
                if (currentFrame < maxFrames)
                {
                    currentFrame++;
                }
                else
                {
                    currentFrame = frameFirst;
                }
                elapsedFrameTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mSpriteTexture, new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight), new Rectangle(currentFrame * frameWidth, frameIndex * frameHeight, frameWidth, frameHeight), Color.White);
        }
    }
}
