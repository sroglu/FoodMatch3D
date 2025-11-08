using System;
using Game.Data;

/// <summary>
/// Events for the game
/// </summary>
namespace Game.Events
{
    public class GameActionEvent : EventArgs
    {
        public GameActionData GameActionData;
        public GameActionEvent(GameActionData gameActionData)
        {
            this.GameActionData = gameActionData;
        }
    }
}
