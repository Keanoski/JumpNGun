﻿using JumpNGun;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpNGun
{



    // enums of buttons used in Button.cs
    // 
    public enum ButtonType {
                            Start,
                            Settings,
                            Highscores,
                            Quit,
                            Audio,
                            Controls,
                            Music,
                            Sfx,
                            Back,
                            QuitToMain,
                            Resume,
                            SfxPause,
                            MusicPause
                           }
    class ButtonFactory : Factory
    {
       

        private ButtonType _type; // determines which button to create.
        



        private static ButtonFactory _instance;

        public static ButtonFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ButtonFactory();
                }
                return _instance;
            }
        }

        // Uses factory pattern to create a gameObject with the corresponding button enum and sprite
        public override GameObject Create(Enum type)
        {
            GameObject gameObject = new GameObject();
            SpriteRenderer sr = (SpriteRenderer)gameObject.AddComponent(new SpriteRenderer());

            _type = (ButtonType)type;
            
            // adding sprites to buttons
            switch (_type)
            {
                case ButtonType.Start:
                    {
                        sr.SetSprite("start_button");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.Settings:
                    {
                        sr.SetSprite("settings_button");
                        
                        gameObject.AddComponent(new Button(_type));
                    }
                    break;
                case ButtonType.Highscores:
                    {
                        sr.SetSprite("highscore_button");
                        
                        gameObject.AddComponent(new Button(_type));


                    }
                    break;
                case ButtonType.Quit:
                    {
                        sr.SetSprite("quit_button");
                        
                        gameObject.AddComponent(new Button(_type));
                        
                    }
                    break;
                case ButtonType.Audio:
                    {
                        sr.SetSprite("audio_button");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.Controls:
                    {
                        sr.SetSprite("controls_button");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.Music:
                    {
                        sr.SetSprite("music");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.Sfx:
                    {
                        sr.SetSprite("sfx");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.Back:
                    {
                        sr.SetSprite("back_button");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.QuitToMain:
                    {
                        sr.SetSprite("quit_to_menu_button");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.Resume:
                    {
                        sr.SetSprite("resume_button");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.MusicPause:
                    {
                        sr.SetSprite("music");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;
                case ButtonType.SfxPause:
                    {
                        sr.SetSprite("sfx");

                        gameObject.AddComponent(new Button(_type));

                    }
                    break;

            }
            return gameObject;
        }

        public override GameObject Create(Enum type, Vector2 position)
        {
            throw new NotImplementedException();
        }

        
    }
}
