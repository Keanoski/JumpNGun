﻿using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace JumpNGun
{
    public class LevelManager
    {
        private static LevelManager _instance;

        public static LevelManager Instance
        {
            get { return _instance ??= new LevelManager(); }
        }


        private bool _isBossLevel = false;
        private int _level = 1; // used to change level
        private int _enemyStartAmount = 2;// initial amount of enemies at start of game
        private int _enemyCurrentAmount = 2; // current amount of enemies through game
        private int _platformAmount = 4; // determines amount of platform pr. level

        #region DEBUG BUTTONS
        private bool _canPress = true;
        private bool canPressL = true;
        #endregion

        private PlatformType _currentPlatformType; // current platform used in game
        private PlatformType _currentGroundPlatform; // currennt ground platform being used in game
        private EnemyType _currentEnemyType; // current enemy being used in gamge

        public List<Rectangle> UsedLocations { get; private set; } //List for storing rectangles that contain a platform

        private LevelManager()
        {
            EventManager.Instance.Subscribe("NextLevel", ChangeLevel);
            EventManager.Instance.Subscribe("OnEnemyDeath", OnEnemyDeath); //TODO Fix another way
        }

        
        public void ExecuteLevelGeneration()
        {
            new Thread(GenerateLevel) { IsBackground = true }.Start();
        }
        
        
        /// <summary>
        /// Creates platforms and relevant Enviroment objects
        /// </summary>
        private void GenerateLevel()
        {
            //Change all relevant enum types
            ChangeEnviroment();

            //Create first portal
            GameWorld.Instance.Instantiate(WorldObjectFactory.Instance.Create(WorldObjectType.portal, new Vector2(40, 705)));

            //Create all relevant platforms
            PlatformGenerator.Instance.GeneratePlatforms(_platformAmount, _currentPlatformType);

            //Get all rectangles that contain platforms 
            UsedLocations = PlatformGenerator.Instance.GetLocations();

            //Create relevant boss or all relevant enemies 
            if (_isBossLevel) EnemyGenerator.Instance.GenerateBoss(_currentEnemyType);
            else EnemyGenerator.Instance.GenerateEnemies(_enemyStartAmount, _currentEnemyType, UsedLocations);

            //Create ground
            GameWorld.Instance.Instantiate(PlatformFactory.Instance.Create(_currentGroundPlatform));
        }

        /// <summary>
        /// Instantiates portal when level has been cleared of enemies
        /// </summary>
        /// <param name="ctx"></param>
        private void OnEnemyDeath(Dictionary<string, object> ctx)
        {
            _enemyCurrentAmount -= (int) ctx["enemyDeath"];

            if (_enemyCurrentAmount <= 0)
            {
                GameWorld.Instance.Instantiate(WorldObjectFactory.Instance.Create(WorldObjectType.portal, new Vector2(1210, 700)));
            }
        }

        /// <summary>
        /// Change enemies, platforms and bosses
        /// </summary>
        private void ChangeEnviroment()
        {
            switch (_level)
            {
                case 1:
                    {
                        _currentPlatformType = PlatformType.grass;
                        _currentGroundPlatform = PlatformType.grassGround;
                        _currentEnemyType = EnemyType.Mushroom;
                    }
                    break;
                case 7:
                    {
                        _currentPlatformType = PlatformType.dessert;
                        _currentGroundPlatform = PlatformType.dessertGround;
                        _currentEnemyType = EnemyType.Worm;
                    }
                    break;
                case 2:
                    {
                        _currentPlatformType = PlatformType.dessert;
                        _currentGroundPlatform = PlatformType.dessertGround;
                        _currentEnemyType = EnemyType.Reaper;
                        _isBossLevel = true;

                    }
                    break;
                case 13:
                    {
                        _isBossLevel = false;
                        _currentEnemyType = EnemyType.Skeleton;
                        _currentPlatformType = PlatformType.graveyard;
                        _currentGroundPlatform = PlatformType.graveGround;
                    }
                    break;
                case 18:
                    {
                        _currentEnemyType = EnemyType.Skeleton;
                        _currentPlatformType = PlatformType.graveyard;
                        _isBossLevel = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Change level when event event message is recieved by calling relevant level change methods
        /// </summary>
        /// <param name="message"></param>
        private void ChangeLevel(Dictionary<string, object> message)
        {
            if (message.ContainsKey("NewLevel"))
            {
                IncrementLevel();
                CleanLevel();
                ExecuteLevelGeneration();
            }
        }

        /// <summary>
        /// Removes all current objects from game and resets player position
        /// </summary>
        private void CleanLevel()
        {
            //Destroy all objects besides player
            foreach (GameObject go in GameWorld.Instance.GameObjects)
            {
                if (go.Tag != "player")
                {
                    GameWorld.Instance.Destroy(go);
                }
            }

            //Clear lists containing rectangles with platforms
            UsedLocations.Clear();

            //Set position of player to left corner of screen
            (GameWorld.Instance.FindObjectOfType<Player>() as Player).GameObject.Transform.Position = new Vector2(40, 705);
        }

        /// <summary>
        /// Increments level and amount of platforms. 
        /// </summary>
        private void IncrementLevel()
        {
            _level++;
            _platformAmount++;

            //if level is odd increment amount of enemies
            if (_level % 2 != 0)
            {
                _enemyStartAmount++;
            }

            _enemyCurrentAmount = _enemyStartAmount;

            //amount of platforms capped at 19, to avoid overcrowding screen and errors
            if (_platformAmount > 19)
            {
                _platformAmount = 19;
            }
        }
        
        /// <summary>
        /// Reset current level and enviroment to level 1
        /// </summary>
        public void ResetLevel()
        {
            _level = 1;
            _platformAmount = 4;
        }

        #region Test Methods

        /// <summary>
        /// Check for cleared level debugging
        /// </summary>
        public void CheckForClearedLevelDebug()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.L) && canPressL)
            {
                GameWorld.Instance.Instantiate(WorldObjectFactory.Instance.Create(WorldObjectType.portal, new Vector2(1210, 700)));
                canPressL = false;
            }

            if (Keyboard.GetState().IsKeyUp(Keys.L))
            {
                canPressL = true;
            }
        }

        /// <summary>
        /// Level change for debugging
        /// </summary>
        public void ChangeLevelDebug()
        {
            //TODO FIX THIS CALLING
            //CheckForClearedLevel();

            if (Keyboard.GetState().IsKeyDown(Keys.K) && _canPress)
            {
                IncrementLevel();
                CleanLevel();
                _canPress = false;
                ExecuteLevelGeneration();
            }

            if (Keyboard.GetState().IsKeyUp(Keys.K))
            {
                _canPress = true;
            }
        }

        //TEST LEVEL FOR GENERATE LEVEL METHOD

        //private Rectangle[] testLevel = new Rectangle[]
        //{
        //    new Rectangle(0, 500, 222, 125),
        //    new Rectangle(222, 500, 222, 125),
        //    new Rectangle(444, 500, 222, 125),
        //    new Rectangle(666, 500, 222, 125),
        //    new Rectangle(888, 500, 222, 125),
        //    new Rectangle(1110, 500, 222, 125),
        //};

        //for (int i = 0; i < 3; i++)
        //{
        //    GameWorld.Instance.Instantiate(EnemyFactory.Instance.Create(EnemyType.Mushroom, new Vector2(600, 0)));
        //    GameWorld.Instance.Instantiate(PlatformFactory.Instance.Create(PlatformType.grass, new Vector2(testLevel[i].Center.X, testLevel[i].Center.Y)));
        //    LevelGenerator.Instance.InvalidLocations.Add(testLevel[i]);
        //}
        //GameWorld.Instance.Instantiate(PlatformFactory.Instance.Create(PlatformType.grassGround));

        #endregion
    }
}