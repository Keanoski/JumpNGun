﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace JumpNGun
{
    //HELE KLASSEN ER LAVET AF KRISTIAN J. FICH 

    public class Portal : Component
    {
        // position of portal
        private Vector2 _position;

        //Determines animation to play
        private bool _open;

        //Determines if its spawn portal or next lvl portal
        private bool _isStartPortal;

        //animator component to play animations
        private Animator _animator;

        public Portal(Vector2 position)
        {
            _position = position;

            //Sets isStartportal to true according to position and calls HandlePlayerRendering
            if (position == new Vector2(40, 705))
            {
                _isStartPortal = true;
            }
        }

        public override void Awake()
        {
            GameObject.Transform.Position = _position;
        }

        public override void Start()
        {
            //get animator component from gameobject
            _animator = GameObject.GetComponent<Animator>() as Animator;

            //play open animation
            _animator.PlayAnimation("open");

            //set open to true
            _open = true;
        }

        public override void Update(GameTime gameTime)
        {
            CheckCollision();
            HandleAnimations();
        }

        /// <summary>
        /// Changes between animations
        /// </summary>
        private void HandleAnimations()
        {
            //Changes from open animation to idle animation
            if (_animator.CurrentIndex == 7 && _open)
            {
                _animator.PlayAnimation("idle");
            }
            //when open is false plays close animation and calls ClosePortal()
            else if (!_open)
            {
                _animator.PlayAnimation("close");
                
                if (_animator.CurrentIndex >= 6)
                {
                    ClosePortal();
                }
            }
        }

        /// <summary>
        /// Triggers event or destorys portal when animation plays through
        /// </summary>
        private void ClosePortal()
        {
            //if portal isn't a start portal trigger next level event
            if (!_isStartPortal)
            {
                EventHandler.Instance.TriggerEvent("NextLevel", new Dictionary<string, object>
                    {
                        {"NewLevel", null}
                    });
            }
            //destroy portal if its a start portal
            else
            {
                GameWorld.Instance.Destroy(this.GameObject);
            }
        }

        /// <summary>
        /// Sets _open to false if portal collides with player
        /// </summary>
        private void CheckCollision()
        {
            //the portal collider
            Collider portalCollider = GameObject.GetComponent<Collider>() as Collider;

            //checks every collisionbox in game and see if they intersect with _portalCollider
            foreach (Collider otherCollider in GameWorld.Instance.Colliders)
            {
                //Terminate if the collision is the portal itself
                if (otherCollider == portalCollider) return;

                //if other collider is player set _open to false and stop rendering of player
                if(portalCollider.CollisionBox.Intersects(otherCollider.CollisionBox) && otherCollider.GameObject.Tag == "player")
                {
                    _open = false;
                }
            }
        }
    }
}