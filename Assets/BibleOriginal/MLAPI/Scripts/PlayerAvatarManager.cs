using UnityEngine;
using Platformer.Mechanics;
using System.Collections.Generic;

namespace RGame.MLAPI
{
    /// <summary>
    /// PlayerAvatarの管理
    /// </summary>
    public class PlayerAvatarManager : MonoBehaviour
    {
        static PlayerAvatarManager _instance;
        static public PlayerAvatarManager Instance {
            get {
                if (_instance == null)
                    _instance = FindObjectOfType<PlayerAvatarManager> ();
                return _instance;
            }
        }

        // keyはClientId
        public Dictionary<ulong, PlayerAvatar> playerAvatars { get; private set; } = new Dictionary<ulong, PlayerAvatar> ();

        public void Add (PlayerAvatar playerAvatar)
        {
            playerAvatars[playerAvatar.OwnerClientId] = playerAvatar;
        }
        public ulong ClientId (PlayerController player)
        {
            foreach (var pair in playerAvatars)
                if( pair.Value.playerController == player)
                    return pair.Key;

            Debug.LogError ("ClientId not found.");
            return 0;
        }
    }
}