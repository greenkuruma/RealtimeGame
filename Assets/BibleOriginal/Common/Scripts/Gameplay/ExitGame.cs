using Platformer.Core;

namespace RGame.Common
{
    /// <summary>
    /// ゲーム終了イベント
    /// </summary>
    public class ExitGame : Simulation.Event<ExitGame>
    {
        public override void Execute()
        {
            UnityEngine.Debug.Log ("ExitGame:Execute");
            player.OnExitGame.Invoke ();
        }
    }
}