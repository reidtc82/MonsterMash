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
        bool scared;

        private int frameRateInterval = 90;
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
        private bool hasState;
        private int direction;
        public int duration;

        public void LoadContent(ContentManager contentManager, string assetName)
        {
            mSpriteTexture = contentManager.Load<Texture2D>(assetName);
            frameFirst = 0;
            currentFrame = frameFirst;
            scared = false;//debug
            hasState = false;//debug
        }

        public void Update(GameTime gameTime)//they all switch state at the same time
        {
            duration--;
            if (!scared)
            {
                if (hasState)
                {
                    if (duration == 0)
                    {
                        hasState = false;
                    }
                }
                else
                {
                    direction = (int)rand.Next(4);//up, down, left, or right
                    duration = (int)rand.Next(2000);//some amoutn of milliseconds
                    frameIndex = frameIndexOrigin + (int)rand.Next(2);//walk or stand
                    hasState = true;
                }
            }
            else
            {
                duration = 500;
                frameIndex = frameIndexOrigin + 2;
                if (duration == 0)
                {
                    scared = false;
                }
            }

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
