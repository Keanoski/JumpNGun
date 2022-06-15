﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace JumpNGun
{
    public class HealthRegen : Ability
    {
        protected override void Create()
        {
            EventHandler.Instance.Subscribe("NextLevel", OnNextLevel);

            AbilityName = "Health Regeneration";
            AbilityDescription = "Regenerates some of your health after each stage";

            amount = 20.0f;

            IsStartAbility = true;
        }

        public override void Selected()
        {
            Console.WriteLine($"\nAbility {AbilityName} Selected!");
            Console.WriteLine(AbilityDescription);

        }

        protected override void LoadContent()
        {
            AbilitySprite = GameWorld.Instance.Content.Load<Texture2D>("icon_HealthRegn");
            AbilityIcon = GameWorld.Instance.Content.Load<Texture2D>("iconSmall_HealthRegn");
        }

        protected override void Execute(Player player)
        {
            if (!isNextLevel) return;

            Console.WriteLine($"Old Health {player.CurrentHealth}");
            player.CurrentHealth += amount;
            Console.WriteLine($"New Health {player.CurrentHealth}");

            isNextLevel = false;
        }

        private void OnNextLevel(Dictionary<string, object> obj)
        {
            isNextLevel = true;
        }
    }
}