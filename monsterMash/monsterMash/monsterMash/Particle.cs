using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace monsterMash
{
    class Particle
    {
        private Texture2D mParticleTexture;

        public Vector2 position;
        
        private int frameWidth;
        private int frameHeight;

        private float fade;

        private bool flipX = true;

        public float lifeSpan
        {
            get;
            set;
        }

        public float fadeRate
        {
            get;
            set;
        }

        public Vector2 speed
        {
            get;
            set;
        }

        public bool active 
        { 
            get; 
            set; 
        }

        public void LoadContent(ContentManager contentManager, string assetName)
        {
            mParticleTexture = contentManager.Load<Texture2D>(assetName);
            frameHeight = mParticleTexture.Height;
            frameWidth = mParticleTexture.Width;
            fade = lifeSpan;
            active = false;
        }

        public void Update(GameTime gameTime)
        {
            if (fade > 0.0f)
            {
                fade -= fadeRate;
                position.Y -= speed.Y;

                position.X += speed.X;
            }
            else
            {
                fade = lifeSpan;
                active = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (active)
            {
                spriteBatch.Draw(mParticleTexture, new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight), Color.White * fade);
            }
        }
    }
}
