using Platformer.Mechanics;
using UnityEngine;
using MLAPI;

namespace RGame.MLAPI
{
    /// <summary>
    /// 既存のTokenInstanceをPowerUpTokenに置換。MLAPI版
    /// </summary>
    public class MLAPIToToken : MonoBehaviour
    {
        [SerializeField] NetworkObject powerUpTokenPrefab = default;

        void Start ()
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        }

        public void OnServerStarted ()
        {
            // トークンの位置にパワーアップアイテム配置
            var tokens = FindObjectsOfType<TokenInstance> ();
            foreach (var token in tokens)
            {
                if (! token.gameObject.activeInHierarchy)
                    continue;

                Instantiate (powerUpTokenPrefab, token.transform.localPosition, Quaternion.identity).Spawn ();
                token.gameObject.SetActive (false);
            }
        }

        public void OnClientConnectedCallback (ulong clientId)
        {
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