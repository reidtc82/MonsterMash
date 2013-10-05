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
        int humanState;

        private int frameRateInterval = 90;
        int currentFrame;
        int frameFirst;
        public int maxFrames;
        public int frameWidth;
        public int frameHeight;
        private int elapsedFrameTime = 0;
        private int elapsedDuration = 0;
        public int frameIndex;
        //needs collision rect

        private Texture2D mSpriteTexture;

        Random rand = new Random();
        int direction = 0;
        private int duration;
        private bool hasState;

        public void LoadContent(ContentManager contentManager, string assetName)
        {
            mSpriteTexture = contentManager.Load<Texture2D>(assetName);
            frameFirst = 0;
            currentFrame = frameFirst;
            
            humanState = frameIndex;

            scared = false;

            states[0] = new Vector2(0, frameIndex);
            states[1] = new Vector2(4, frameIndex);
            states[2] = new Vector2(8, frameIndex);
            states[3] = new Vector2(12, frameIndex);
            states[4] = new Vector2(0, frameIndex + 1);
            states[5] = new Vector2(4, frameIndex + 1);
            states[6] = new Vector2(8, frameIndex + 1);
            states[7] = new Vector2(12, frameIndex + 1);
            states[8] = new Vector2(0, frameIndex + 2);
            states[9] = new Vector2(4, frameIndex + 2);
            states[10] = new Vector2(8, frameIndex + 2);
            states[11] = new Vector2(12, frameIndex + 2);

        }

        public void Update(GameTime gameTime)
        {
            elapsedDuration += (int)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedDuration >= duration)
            {
                hasState = false;
                elapsedDuration = 0;
            }

            if (hasState)
            {
                if (scared)
                {
                    if (direction == 0)
                    {
                        direction = 1;
                        duration = 2;
                        humanState = 9;
                        hasState = true;
                    }
                    else if (direction == 1)
                    {
                        direction = 0;
                        duration = 2;
                        humanState = 8;
                        hasState = true;
                    }
                    else if (direction == 2)
                    {
                        direction = 3;
                        duration = 2;
                        humanState = 11;
                        hasState = true;
                    }
                    else if (direction == 3)
                    {
                        direction = 2;
                        duration = 2;
                        humanState = 10;
                        hasState = true;
                    }
                }
                else
                {

                }
            }
            else
            {
                direction = rand.Next(4);
                duration = rand.Next(4);
                humanState = direction + rand.Next(4);
                hasState = true;
            }

            frameFirst = (int)states[humanState].X;
            frameIndex = (int)states[humanState].Y;

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
