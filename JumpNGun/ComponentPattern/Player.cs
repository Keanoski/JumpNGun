﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JumpNGun
{
    public class Player : Component
    {
        private CharacterType _character;
        private Dictionary<Keys, bool> _movementKeys = new Dictionary<Keys, bool>();

        private float _maxHealth;
        private float _currentHealth;
        
        #region Component Fields

        private SpriteRenderer _sr; // Reference to the SpriteRenderer component
        private Animator _animator; // Reference to the Animator component
        private Input _input; // Reference to the Input Component
        private Collider _pCollider;

        #endregion
        
        #region Movement Fields

        private Vector2 _moveDirection;
        
        private float _speed; // Speed at which the player moves
        private float _jumpHeight; // The jump height of the player
        private float _dashStrength; // The strength of the player dash

        private int _gravity = 50; // The initial force of gravity
        private float _gravityPull; // How strong the force of gravity is
        private int _gravityMultiplier = 100; // Used to multiply the gravity over time making it stronger


        private bool _canJump; // Can the player jump
        private bool _isJumping; // Is the player jumping
        private int _jumpCount; // The current amount of player jumps
        private int _maxJumpCount; // The max allowed amount of player jumps

        private bool _shouldIgnorePlatform;
        
        private bool _canDash = true;
        private float _dashTimer;
        private float _dashCooldown;
        private float _footstepCooldown;
        #endregion

        #region Action Fields

        private bool _canShoot = true;
        private float _shootTime;
        private float _shootCooldown;

        private float _projectileSpeed;

        #endregion

        #region Collision Fields

        private Vector2 _position = new Vector2(40, 705);
        private GameObject _groundCollisionGameObject;
        private Rectangle _groundCollisionRectangle = Rectangle.Empty;
        private bool _isGrounded; // Is the player grounded
        private bool _hasCollidedWithGround = false;

        public Vector2 Position { get => _position; set => _position = value; }
        public float Speed { get => _speed; private set => _speed = value; }

        #endregion

        public Player(CharacterType character)
        {
            switch (character)
            {
                case CharacterType.Soldier:
                {
                    _speed = 100;
                    _jumpHeight = -100;
                    _dashStrength = 50;
                    _dashCooldown = 0.5f;
                    _shootCooldown = 1f;
                    _maxJumpCount = 2;
                    _maxHealth = 120;
                    _currentHealth = _maxHealth;

                    _projectileSpeed = 350;

                }break;
                case CharacterType.Ranger:
                {
                    _speed = 150;
                    _jumpHeight = -120;
                    _dashStrength = 75;
                    _dashCooldown = 0.25f;
                    _shootCooldown = 1.5f;
                    _maxJumpCount = 2;
                    _maxHealth = 80;
                    _currentHealth = _maxHealth;

                    _projectileSpeed = 250;


                } break;

            }
            _character = character;
        }

        #region Component Methods

        public override void Awake()
        {
            EventManager.Instance.Subscribe("OnKeyPress", OnKeyPressed);
            EventManager.Instance.Subscribe("OnCollision", OnCollision);
        }
        
        public override void Start()
        {
            _sr = GameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;

            _input = GameObject.GetComponent<Input>() as Input;

            _animator = GameObject.GetComponent<Animator>() as Animator;
            
            _pCollider = GameObject.GetComponent<Collider>() as Collider;


            GameObject.Transform.Position = _position;
            _gravityPull = _gravity;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }

        public override void Update(GameTime gameTime)
        {
            _input.Execute(this);

            UpdatePositionReference();
            HandleShootLogic();
            HandleDashLogic();

            HandleAnimations();

            CheckGrounded();

            HandleGravity();

            CheckDeath();

            if (Keyboard.GetState().IsKeyDown(Keys.O))
            {
                _currentHealth--;
                Console.WriteLine(_currentHealth);
            }

            WalkingSoundEffects();

        }

        /// <summary>
        /// Plays random footstep out of 4 diffent if player is moving with A or D and is on ground
        /// </summary>
        private void WalkingSoundEffects()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.A) && _isGrounded || Keyboard.GetState().IsKeyDown(Keys.D) && _isGrounded)
            {
                _footstepCooldown += GameWorld.DeltaTime;
                if (_footstepCooldown > 0.3f)
                {
                    SoundManager.Instance.PlayRandomFootstep();
                    _footstepCooldown = 0;
                }
            }
        }

        #endregion

        #region Movement Methods

        /// <summary>
        /// Called from MoveCommand.cs
        /// Moves the player
        /// </summary>
        /// <param name="velocity">The strength at which we want to move</param>
        public void Move(Vector2 velocity)
        {
            // Normalize velocity
            velocity.Normalize();

            // Multiply with speed
            velocity *= _speed;

            _moveDirection = velocity;


            // Translate our current position to the new one
            GameObject.Transform.Translate(_moveDirection * GameWorld.DeltaTime);

            FlipSprite(_moveDirection);
        }

        /// <summary>
        /// Handles jumping. Called from JumpCommand.cs
        /// </summary>
        public void Jump()
        {
            

            // Check if we are above or at our maxJumpCount
            _canJump = _jumpCount <= _maxJumpCount;

            // Guard clause
            if (!_canJump || _isJumping) return;

            // Sound effect
            SoundManager.Instance.PlayRandomJump();

            // Add to our jump count
            _jumpCount++;

            // Set our targetDirection using our jumpHeight
            Vector2 targetDirection = new Vector2(0, _jumpHeight);

            GameObject.Transform.Translate(targetDirection);
        }
        
        /// <summary>
        /// Called from DashCommand.cs
        /// Makes the player dash if he can
        /// </summary>
        public void Dash()
        {
            if (!_canDash) return; // Guard clause

            // Multiply with dashStrength
            Vector2 dashDirection = _moveDirection;
            dashDirection *= _dashStrength;

            // Translate our current position to the new one
            GameObject.Transform.Translate(dashDirection * GameWorld.DeltaTime);

            FlipSprite(dashDirection);
            _canDash = false;
        }

        /// <summary>
        /// Logic for handling when we can dash
        /// </summary>
        private void HandleDashLogic()
        {
            if (_canDash) return; // Guard clause


            _dashTimer += GameWorld.DeltaTime; // Add to our dashTimer

            // Check if its bigger than the cooldown
            if (_dashTimer > _dashCooldown)
            {
                _canDash = true;
                _dashTimer = 0;
            }
        }

        /// <summary>
        /// Creates gravity making sure the player falls unless he is grounded
        /// </summary>
        private void HandleGravity()
        {
            if (_isGrounded) return;

            // Makes the gravity stronger over time, creating a feeling of a pull
            _gravityPull += GameWorld.DeltaTime * _gravityMultiplier;

            Vector2 fallDirection = new Vector2(0, 1);

            // Multiply fallDirection with our gravityPull
            fallDirection *= _gravityPull;

            GameObject.Transform.Translate(fallDirection * GameWorld.DeltaTime);
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Called from ShootCommand.cs
        /// Shoots a projectile if we can shoot
        /// </summary>
        public void Shoot()
        {
            if (!_canShoot) return; // Guard clause

            GameObject projectile = ProjectileFactory.Instance.Create(_character);

            projectile.Transform.Position = GameObject.Transform.Position;

            Vector2 shootRight = new Vector2(1, 0);
            Vector2 shootLeft = new Vector2(-1, 0);

            ((Projectile) projectile.GetComponent<Projectile>()).Velocity = _sr.SpriteEffects == SpriteEffects.None ? shootRight : shootLeft;
            ((Projectile) projectile.GetComponent<Projectile>()).Speed = _projectileSpeed;
            GameWorld.Instance.Instantiate(projectile);

            _canShoot = false;
        }

        #endregion

        #region Class Methods
        
        /// <summary>
        /// Handles when to set Animations 
        /// </summary>
        private void HandleAnimations()
        {
            // If we are not grounded we are in the air and should play the jump animation
            if (!_isGrounded) _animator.PlayAnimation("Jump");
            
            // If there isn't any values that is true in movementKeys and we are grounded play idle
            if (!_movementKeys.ContainsValue(true) && _isGrounded) _animator.PlayAnimation("Idle");
            
            if(_movementKeys.ContainsValue(true) && _isGrounded) _animator.PlayAnimation("Run");
        }
        
        /// <summary>
        /// Check if we are grounded
        /// </summary>
        private void CheckGrounded()
        {
            foreach (Collider otherCollision in GameWorld.Instance.Colliders)
            {
                // If our CollisionBox collides with another CollisionBox and it's tag is ground and we haven't collided with ground yet
                if (_pCollider.CollisionBox.Intersects(otherCollision.CollisionBox) && !_hasCollidedWithGround)
                {
                    if (otherCollision.GameObject.Tag == "ground" || otherCollision.GameObject.Tag == "platform")
                    {
                        _isGrounded = CalculateCollisionLineIntersection( otherCollision);
                    }
                }
            }
            
            // If we are grounded but do not collide with our groundCollision, then we are not grounded!
            if (_isGrounded && !_pCollider.CollisionBox.Intersects(_groundCollisionRectangle))
            {
                _isGrounded = false;
                _hasCollidedWithGround = false;
            }
        }
        
        /// <summary>
        /// Checks if the player should die
        /// </summary>
        private void CheckDeath()
        {
            if(_currentHealth <= 0)
            {
                //TODO Play death animation - NICHLAS
                
                if(_animator.IsAnimationDone)
                {
                    GameWorld.Instance.Destroy(this.GameObject);
                }
            }
        }

        /// <summary>
        /// Updates _position to current position during gametime
        /// </summary>
        private void UpdatePositionReference()
        {
            _position = GameObject.Transform.Position;
        }
        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Checks if the sprite should be flipped by using the moveDirection
        /// </summary>
        private void FlipSprite(Vector2 moveDirection)
        {
            // If we are moving left, flip the sprite
            if (moveDirection.X < 0)
            {
                _sr.SpriteEffects = SpriteEffects.FlipHorizontally;
            }
            // If we are moving right, unflip the sprite
            else if (moveDirection.X > 0)
            {
                _sr.SpriteEffects = SpriteEffects.None;
            }
        }

        /// <summary>
        /// Logic for handling when we can shoot
        /// </summary>
        private void HandleShootLogic()
        {
            // Guard clause
            if (_canShoot) return;

            // Add to our shootTime
            _shootTime += GameWorld.DeltaTime;

            // Check if its bigger than the cooldown
            if (_shootTime > _shootCooldown)
            {
                _canShoot = true;
                _shootTime = 0;
            }
        }

        /// <summary>
        /// Calculates which part of the player's CollisionBox it has intersected with and applies logic according to it
        /// </summary>
        /// <param name="collisionCollider">The other-collision's collider</param>
        private bool CalculateCollisionLineIntersection(Collider collisionCollider)
        {
            // If the bottom line of the playerCollider intersects with the other-collision's CollisionBox and we haven't collided with the ground yet
            if (_pCollider.BottomLine.Intersects(collisionCollider.CollisionBox))
            {
                // Check if the player is inside the ground collision or if hes just a tiny bit above it
                if (_pCollider.CollisionBox.Bottom >= collisionCollider.CollisionBox.Top && _pCollider.CollisionBox.Bottom <= collisionCollider.CollisionBox.Top + 5)
                {
                    _jumpCount = 0; // Reset jump counter
                    _gravityPull = _gravity; // Reset gravity pull
                    _groundCollisionGameObject = collisionCollider.GameObject; // Reference our current collisionColliders GameObject
                    _groundCollisionRectangle = collisionCollider.CollisionBox; // Reference our current collisionColliders CollisionBox
                    _hasCollidedWithGround = true; // We have collided with ground now
                    return true;
                    
                }
            }
            else if (_pCollider.CollisionBox.Intersects(collisionCollider.BottomLine))
            {
                // Console.WriteLine("Push To top!");
                return false;
            }

            // If the player hits the ground CollisionBox from either the left or the right side
            else if (_pCollider.CollisionBox.Intersects(collisionCollider.LeftLine))
            {
                // _isGrounded = false; // We are not grounded then 
                // Console.WriteLine("Collided with LeftLine!");
                GameObject.Transform.Translate(new Vector2(-1, 0) * 10); // Create a small push back
                return false;
            }
            else if (_pCollider.CollisionBox.Intersects(collisionCollider.RightLine))
            {
                // _isGrounded = false;
                // Console.WriteLine("Collided with RightLine!");
                GameObject.Transform.Translate(new Vector2(1, 0) * 10);
                return false;
            }
            
            return false;
        }

        #endregion
        
        #region Event Methods

        /// <summary>
        /// Event that receives all key presses
        /// And handles logic depending on the key and if its pressed down
        /// </summary>
        /// <param name="ctx">The context from the trigger in InputHandler.cs</param>
        private void OnKeyPressed(Dictionary<string, object> ctx)
        {
            // Get the current pressed key
            Keys currentKey = (Keys) ctx["key"];
            
            // If the currentKey is either the same as  our MoveLeft or MoveRight KeyCode
            if(currentKey == _input.MoveLeft.KeyboardBinding  ||
               currentKey == _input.MoveRight.KeyboardBinding)
            {
                _movementKeys[currentKey] = (bool) ctx["isKeyDown"];
                
            }
            
            // If the currentKey is the same as our Jump KeyCode
            if(currentKey == _input.Jump.KeyboardBinding)
            {
                _isJumping = (bool)ctx["isKeyDown"];
            }
            
            if(currentKey == _input.Fall.KeyboardBinding)
            {
                _shouldIgnorePlatform = (bool) ctx["isKeyDown"];
            }

        }
        private void OnCollision(Dictionary<string, object> ctx)
        {
            Collider pCollider = GameObject.GetComponent<Collider>() as Collider;
            
            // Collider collision = (Collider) ctx["collider"];
            //
            // if (pCollider.CollisionBox.Intersects(collision.CollisionBox))
            // {
            //     switch (collision.GameObject.Tag)
            //     {
            //         case "ground":
            //             // Console.WriteLine("hugubuku");
            //             break;
            //     }
            // }
                
        }


 

        #endregion
    }
}