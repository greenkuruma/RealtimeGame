using Platformer.Mechanics;
using UnityEngine;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class PunToToken : MonoBehaviour { }
}
#else
using Photon.Pun;

namespace RGame.Photon
{
    /// <summary>
    /// 既存のTokenInstanceをPowerUpTokenに置換。MLAPI版
    /// </summary>
    public class PunToToken : MonoBehaviourPunCallbacks
    {
        [SerializeField] string powerUpTokenName = "PowerUpToken";

        public override void OnCreatedRoom ()
        {
            Debug.Log ($"OnCreatedRoom");

            // １つもない時のみ配置処理
            bool make = FindObjectsOfType<PowerUpToken> ().Length <= 0;

            // トークンの位置にパワーアップアイテム配置
            var tokens = FindObjectsOfType<TokenInstance> ();
            foreach (var token in tokens)
            {
                if (make)
                    PhotonNetwork.Instantiate (powerUpTokenName, token.transform.localPosition, Quaternion.identity);

                token.gameObject.SetActive (false);
            }
        }

        public override void OnJoinedRoom ()
        {
            Debug.Log ($"OnJoinedRoom");

            // 既に作られているのでトークン非表示
            var tokens = FindObjectsOfType<TokenInstance> ();
            foreach (var token in tokens)
            {
                if (token.GetComponent<PowerUpToken> () == null)
                    token.gameObject.SetActive (false);
            }
        }
    }
}
#endif