using Platformer.Core;
using Platformer.Mechanics;
using static Platformer.Core.Simulation;

using RGame.Common;

namespace Platformer.Gameplay
{
    /// <summary>
    /// This event is triggered when the player character enters a trigger with a VictoryZone component.
    /// </summary>
    /// <typeparam name="PlayerEnteredVictoryZone"></typeparam>
    public class PlayerEnteredVictoryZone : Simulation.Event<PlayerEnteredVictoryZone>
    {
        const float leaveTime = 6f;

        public VictoryZone victoryZone;

        public override void Execute()
        {
            player.animator.SetTrigger("victory");
            player.controlEnabled = false;

            // ゴールした本人かどうかはおかまいなしに呼び出し
            Schedule<ExitGame> (player, leaveTime);
        }
    }
}