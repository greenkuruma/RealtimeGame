using Platformer.Mechanics;
using UnityEngine;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class PowerUpToken : MonoBehaviour { }
}
#else
using Photon.Pun;
namespace RGame.Photon
{
    /// <summary>
    /// トークン取得時にパワーアップ
    /// </summary>
    public class PowerUpToken : MonoBehaviourPunCallbacks
    {
        bool isAvailable = true; // アイテムが取得可能かどうか

        GameProperties gameProperties = default;

        TokenInstance tokenInstance = default;

        void Awake ()
        {
            transform.SetParent (FindObjectOfType<PunToToken> ().transform);

            gameProperties = FindObjectOfType<GameProperties> ();
            tokenInstance = GetComponent<TokenInstance> ();
            tokenInstance.tryGetToken = TryGetToken;
        }

        public void TryGetToken (PlayerController player)
        {
            Debug.Log ("TryGetToken!");

            photonView.RPC (nameof (RPCTryGetToken), RpcTarget.AllViaServer);
        }

        [PunRPC]
        void RPCTryGetToken (PhotonMessageInfo info)
        {
            Debug.Log ("RPCTryGetToken!");
            if (isAvailable)
            {
                isAvailable = false;
                tokenInstance.OnFinish ();

                if (info.Sender.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                    gameProperties.PowerUp (PhotonNetwork.LocalPlayer);
            }
        }
    }
}
#endif