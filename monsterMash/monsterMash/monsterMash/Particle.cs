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

        private Vector2 position;
        
        private int frameWidth;
        private int frameHeight;

        private float fade;

        public Vector2 origin
        {
            get;
            set;
        }

        public float lifeSpan
        {
            get;
            set;
        }

        public void LoadContent(ContentManager contentManager, string assetName)
        {
            mParticleTexture = contentManager.Load<Texture2D>(assetName);
            frameHeight = mParticleTexture.Height;
            frameWidth = mParticleTexture.Width;
            position = origin;
            fade = lifeSpan;
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mParticleTexture, new Rectangle((int)position.X, (int)position.Y, frameWidth, frameHeight), Color.White * fade);
        }
    }
}
