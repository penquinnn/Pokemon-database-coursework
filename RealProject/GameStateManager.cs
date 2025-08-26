using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealProject
{
    static class GameStateManager
    {
        public enum GameState
        {
            Overworld,
            Battle,
            Bag,
            Cutscene,
            Menu,
            TitleScreen
        }

        public static GameState gameState;

        public static void Initialize()
        {
            gameState = GameState.Overworld;
        }

        public static void ChangeState(GameState state)
        {
            gameState = state;
        }
    }
}
