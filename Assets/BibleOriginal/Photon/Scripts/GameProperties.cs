using UnityEngine;
using System.Collections.Generic;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class GameProperties : MonoBehaviour { }
}
#else
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;

namespace RGame.Photon
{
    /// <summary>
    /// Playerのカスタムプロパティを管理
    /// </summary>
    public class GameProperties : MonoBehaviourPunCallbacks
    {
#region Debug
        [SerializeField] bool debugPowerUpTrigger = false;
        [SerializeField] bool debugPowerUpAll = false;
        [SerializeField] int debugPowerUpVolume = 200;
#endregion

        const int defaultPower = 0;

        [SerializeField] string powerCustomPropertyKey = "power";

        public Dictionary<Player, PlayerAvatar> players { get; private set; }
            = new Dictionary<Player, PlayerAvatar> ();

        void Update ()
        {
            if (debugPowerUpTrigger)
            {
                if (debugPowerUpAll)
                    foreach (var player in players.Keys)
                        PowerUp (player, debugPowerUpVolume);
                else
                    PowerUp (PhotonNetwork.LocalPlayer, debugPowerUpVolume);
                debugPowerUpTrigger = false;
            }
        }

        public override void OnPlayerPropertiesUpdate (Player target, Hashtable changedProps)
        {
            // 対象キャラがいないなら全検索
            PlayerAvatar player;
            if (!players.TryGetValue (target, out player))
                return;

            player.StatusUpdate ((int)changedProps[powerCustomPropertyKey]);
        }

        public void PowerUp (Player target, int volume = 1)
        {
            SetPower (target, GetPower (target) + volume);
        }

        public int GetPower (Player target)
        {
            if (target.CustomProperties[powerCustomPropertyKey] == null)
            {
                Reset (target);
                return defaultPower;
            }

            return (int)target.CustomProperties[powerCustomPropertyKey];
        }
        public void SetPower (Player target, int value)
        {
            var hashtable = new Hashtable ();
            hashtable[powerCustomPropertyKey] = value;
            target.SetCustomProperties (hashtable);
        }
        public void Reset (Player target)
        {
            SetPower (target, defaultPower);
        }
    }
}
#endif