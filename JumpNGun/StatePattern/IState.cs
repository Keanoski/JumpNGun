﻿using Microsoft.Xna.Framework;

namespace JumpNGun
{
    public interface IState
    {
        /// <summary>
        /// Method called when entering new state
        /// </summary>
        /// <param name="parent"></param>
        public void Enter(Reaper parent);

        /// <summary>
        /// Logic to execute when in specific state
        /// </summary>
        /// <param name="gameTime"></param>
        public void Execute(GameTime gameTime);

        /// <summary>
        /// Logic to execute before/as leaving state
        /// </summary>
        public void Exit();

    }
}
