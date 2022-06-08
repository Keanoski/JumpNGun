﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JumpNGun
{
    /// <summary>
    /// Klassen er lavet af alle
    /// </summary>
    public class SpriteRenderer : Component
    {
        public bool StopRendering { get; set; } //decides whether we should render a sprite

        public Texture2D Sprite { get; set; } //texture for sprite

        public Vector2 Origin { get; set; } //origin for sprite

        public Color Color { get; set; } = Color.White; //color for sprite

        public SpriteEffects SpriteEffects { get; set; }//spriteeffects for sprite. flips, etc
       


        public override void Start()
        {
            Origin = new Vector2(Sprite.Width / 2, Sprite.Height / 2);
        }


        /// <summary>
        /// Set sprite to a specific png
        /// </summary>
        /// <param name="spriteName"></param>
        public void SetSprite(string spriteName)
        {
            Sprite = GameWorld.Instance.Content.Load<Texture2D>(spriteName);
        }
        
        public void SetColor(Color newColor)
        {
            Color = newColor;
        }
        
        public void SetOrigin(Vector2 newOrigin)
        {
            Origin = newOrigin;
        }
        

        /// <summary>
        /// Draw sprite
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (StopRendering) return;
            
            spriteBatch.Draw(Sprite, GameObject.Transform.Position, null, Color, 0, Origin, 1, SpriteEffects, 1);
            
        }
    }
}
