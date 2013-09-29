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
    class Monster:Sprite
    {
        Texture2D mSpriteTexture;

        int playerState = 0;
        Vector2[] states = new Vector2[20];

        private int elapsedFrameTime = 0;
        int frameRateInterval = 90;
        public int currentFrame;
        public int frameFirst;

        public virtual void LoadContent(ContentManager contentManager, string assetName)
        {
            mSpriteTexture = contentManager.Load<Texture2D>(assetName);
            frameFirst = 8;
            currentFrame = frameFirst;

            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
            states[] = new Vector2(,);
        }

        public virtual void Update(GameTime gameTime){
            KeyboardState keyboardState = Keyboard.GetState();



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

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mSpriteTexture, new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight), new Rectangle(currentFrame * frameWidth, frameIndex * frameHeight, frameWidth, frameHeight), Color.White);
        }
    }
}
