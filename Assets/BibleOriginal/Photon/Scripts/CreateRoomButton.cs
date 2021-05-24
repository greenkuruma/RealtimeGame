using UnityEngine;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class CreateRoomButton : MonoBehaviour { }
}
#else
using Photon.Pun;
using Photon.Realtime;

namespace RGame.Photon
{
    /// <summary>
    /// ルーム作成ボタン
    /// </summary>
    public class CreateRoomButton : MonoBehaviour
    {
        [SerializeField] byte roomMaxPlayers = 4;

        public void OnCreateRoom ()
        {
            Debug.Log ("CreateRoom");
            PhotonNetwork.CreateRoom (
                null, // 自動的にユニークなルーム名を生成する
                new RoomOptions ()
                {
                    MaxPlayers = roomMaxPlayers,
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable () {
                        { "DisplayName", $"{PhotonNetwork.NickName}'s room" },
                        { "Comment", "welcome!" }
                    },
                    CustomRoomPropertiesForLobby = new[] {
                        "DisplayName",
                        "Comment"
                    }
                }
            );
        }
    }
}
#endif