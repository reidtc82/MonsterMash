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
    class Animation
    {
        Texture2D texture;
        Rectangle frame;
        Vector2 position;
        Vector2 origin;
        Vector2 velocity;

        int currentFrame;
        int frameHeight;
        int frameWidth;

        float timer;
        float interval = 75; //higher interval slower change

        public Animation(Texture2D newTexture, Vector2 newPosition, int newFrameHeight, int newFrameWidth)
        {
            texture = newTexture;
            position = newPosition;
            frameHeight = newFrameHeight;
            frameWidth = newFrameWidth;
        }

        public void Update(GameTime gameTime)
        {
            //frame = new Rectangle(currentFrame*frameWidth,0,frameWidth,frameHeight);
            origin = new Vector2(frame.Width/2, frame.Height/2);
            position = position + velocity;

            //add for up and down
            
        }

        //add more for up and down
        public void AnimateRest(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 2;
            if(timer > interval)
            {
                currentFrame++;
                timer = 0;
                if(currentFrame > 4)
                {
                    currentFrame = 0;
                }
            }
        }
        public void AnimateDown(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 2;
            if (timer > interval)
            {
                currentFrame++;
                timer = 0;
                if (currentFrame > 4)//change these values to accomodate new sprite sheet
                {
                    currentFrame = 0;
                }
            }
        }

        public void AnimateLeft(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 2;
            if (timer > interval)
            {
                currentFrame++;
                timer = 0;
                if (currentFrame > 16 || currentFrame < 13)//change these values to accomodate new sprite sheet
                {
                    currentFrame = 13;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, frame, Color.White, 0f, origin, 1.0f, SpriteEffects.None, 0);
        }

    }
}
