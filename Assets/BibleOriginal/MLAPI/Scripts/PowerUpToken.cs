using UnityEngine;
using Platformer.Mechanics;
using MLAPI;
using MLAPI.Messaging;

namespace RGame.MLAPI
{
    /// <summary>
    /// トークン取得時にパワーアップ
    /// </summary>
    public class PowerUpToken : NetworkBehaviour
    {
        private bool isAvailable = true; // アイテムが取得可能かどうか
        private bool isPlayed = false; // 取得演出を行ったかどうか

        TokenInstance tokenInstance = default;

        private void Awake ()
        {
            transform.SetParent (FindObjectOfType<MLAPIToToken> ().transform);

            tokenInstance = GetComponent <TokenInstance> ();
            tokenInstance.tryGetToken = TryGetToken;
        }

        public void TryGetToken (PlayerController player)
        {
            // player自身のClientIdを取得
            // （PowerUpTokenのOwnerClientIdはホストなので用いてはいけない）
            ulong clientId = PlayerAvatarManager.Instance.ClientId (player);

            // 演出は被っても良いので先にやっとく
            PlayFinishClientRpc ();

            TryGetTokenServerRpc (clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryGetTokenServerRpc (ulong clientId)
        {
            Debug.Log ($"ServerTryGetToken! clientId={clientId}");

            // サーバー側の取得状況を正とする
            if (isAvailable)
            {
                isAvailable = false;
                PlayerAvatarManager.Instance.playerAvatars[clientId].PowerUp ();
            }

            PlayFinishClientRpc ();
        }
        [ClientRpc]
        private void PlayFinishClientRpc ()
        {
            if (isPlayed)
                return;

            tokenInstance.OnFinish ();
            isPlayed = false;
        }
    }
}