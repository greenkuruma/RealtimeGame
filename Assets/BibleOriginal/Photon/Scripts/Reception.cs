using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class Reception : MonoBehaviour { }
}
#else
using Photon.Pun;
using Photon.Realtime;

namespace RGame.Photon
{
    /// <summary>
    /// 接続から入室まで色々やっちゃう
    /// </summary>
    public class Reception : MonoBehaviourPunCallbacks
    {
        const string roomName = "Room";

        [SerializeField] bool useLobby = false;
        [SerializeField] GameObject lobbyCanvas = null;
        [SerializeField] GameObject mainCanvas = null;

        bool isRestarting;

        public string ownerPlayerName { get; private set; }

        void Start ()
        {
            if (mainCanvas != null)
                mainCanvas.SetActive (false);

            if (!PhotonNetwork.IsConnected)
            {
                if (lobbyCanvas != null)
                    lobbyCanvas.SetActive (false);

                // 適当
                ownerPlayerName = System.Guid.NewGuid ().ToString ().Substring (0, 13);
                PhotonNetwork.LocalPlayer.NickName = ownerPlayerName;

                PhotonNetwork.ConnectUsingSettings ();
            }
            else if (PhotonNetwork.InLobby)
            {
                Debug.Log ($"Start:In Lobby");
                lobbyCanvas.SetActive (true);
            }
            else
                ProperJoin ();
        }

        /// <summary>
        /// 接続完了、及びRoomから抜けた時に呼ばれるコールバック
        /// </summary>
        public override void OnConnectedToMaster ()
        {
            Debug.Log ($"connected to master");
            if (isRestarting)
                return;

            ProperJoin ();
        }

        void ProperJoin ()
        {
            Debug.Log ($"proper join");

            // ロビーに入る
            if (useLobby && lobbyCanvas != null)
                PhotonNetwork.JoinLobby ();
            // ルームに入る、ルームが無ければ作る
            else
                PhotonNetwork.JoinOrCreateRoom (roomName, new RoomOptions (), TypedLobby.Default);
        }

        public override void OnJoinedLobby ()
        {
            Debug.Log ($"joined Lobby");
            lobbyCanvas.SetActive (true);
        }

        public override void OnJoinedRoom ()
        {
            Debug.Log ($"joined room");
            if (lobbyCanvas != null)
                lobbyCanvas.SetActive (false);
            if (mainCanvas != null)
                mainCanvas.SetActive (true);

            FindObjectOfType<GameProperties> ().Reset (PhotonNetwork.LocalPlayer);
            PhotonNetwork.Instantiate ("PlayerVersionA", Vector3.zero, Quaternion.identity);
        }

        public override void OnLeftRoom ()
        {
            Debug.Log ($"left room");

            StartCoroutine (RestartScene ());
        }

        IEnumerator RestartScene ()
        {
            isRestarting = true;

            // Editor終了時にも呼ばれるので、リスタートするかどうかは念入りに調べる
            // 終了判定よりワンテンポ前に呼び出されることがあるので、少し待ってから判定する
            yield return new WaitForSeconds (1);

            // シーンが読み込める時でない＝終了状態だと判断して諦める
            if (! SceneLoadable ())
                yield break;

            Debug.Log ($"RestartScene");
            SceneManager.LoadScene ("Main");
        }
        public bool SceneLoadable ()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return false;
#endif
            return Application.isPlaying;
        }

        public override void OnPlayerEnteredRoom (Player player)
        {
            Debug.Log ($"join {player.NickName}");
        }

        public override void OnPlayerLeftRoom (Player player)
        {
            Debug.Log ($"exit {player.NickName}");
        }

        public override void OnCreatedRoom ()
        {
            Debug.Log ($"created room");
        }

        public override void OnCreateRoomFailed (short returnCode, string message)
        {
            Debug.Log ($"create room failed: {message}");
        }
    }
}
#endif