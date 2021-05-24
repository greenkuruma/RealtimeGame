using UnityEngine;
using Platformer.Mechanics;
using TMPro;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using RGame.Common;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class PlayerAvatar : MonoBehaviour
    {
    }
}
#else
using Photon.Pun;
using Photon.Realtime;

namespace RGame.Photon
{
    /// <summary>
    /// プレイヤーの挙動で、既存PlayerControllerに備わってない機能を持つ
    /// 弾発射、被弾、強化など
    /// </summary>
    public class PlayerAvatar : MonoBehaviourPunCallbacks
    {
        [SerializeField] float moveRatio = 0.05f;
        [SerializeField] float defaultBulletSpeed = 5f;
        [SerializeField] float bulletSpeedRatio = 0.2f;

        [SerializeField] TMP_Text nameLabel = default;

        public PlayerController playerController { get; private set; }
        public Player player => photonView.Owner;

        GameProperties gameProperties;

        BulletManager bulletManager;

        PlayerInput playerInput;

        int shotId = 0;

        void Start ()
        {
            playerController = GetComponent<PlayerController> ();
            playerController.isMine = photonView.IsMine;
            playerController.OnExitGame += OnExitGame;

            if (photonView.IsMine)
            {
                FindObjectOfType<GameController> ().model.player = playerController;
                var vCamera = FindObjectOfType<Cinemachine.CinemachineVirtualCamera> ();
                vCamera.Follow = transform;
                vCamera.LookAt = transform;
            }

            gameProperties = FindObjectOfType<GameProperties> ();
            gameProperties.players[player] = this;


            UpdateText ();

            bulletManager = FindObjectOfType<BulletManager> ();
            playerInput = FindObjectOfType<PlayerInput> ();
        }

        /// <summary>
        /// ゲーム終了呼び出し
        /// どのAvatarが呼んだかは関係ない
        /// </summary>
        void OnExitGame ()
        {
            PhotonNetwork.LeaveRoom ();
        }

        internal void StatusUpdate (int power)
        {
            playerController.addMoveRatio = power * moveRatio;
            UpdateText ();
        }

        void Update ()
        {
            if (playerController.controlEnabled && photonView.IsMine)
            {
                if (playerInput.triggerShot)
                {
                    shotId ++;

                    photonView.RPC (
                        nameof (Shot),
                        RpcTarget.All,
                        shotId,
                        transform.position,
                        playerController.isLeftward,
                        defaultBulletSpeed + gameProperties.GetPower (player) * bulletSpeedRatio
                    );
                }
            }
        }

        void UpdateText ()
        {
            var customProperties = player.CustomProperties;

            // プレイヤー名の横にスコアを表示する
            nameLabel.text = $"{player.NickName}({gameProperties.GetPower (player).ToString ()})";
        }

        [PunRPC]
        private void Shot (int id, Vector3 origin, bool toLeft, float speed, PhotonMessageInfo info)
        {
            bulletManager.Fire (
                id,
                player.ActorNumber,
                origin,
                toLeft,
                speed,
                info.SentServerTimestamp,
                ServerTime
            );
        }

        int ServerTime ()
        {
            return PhotonNetwork.ServerTimestamp;
        }

        private void OnTriggerEnter2D (Collider2D collision)
        {
            if (photonView.IsMine)
            {
                var bullet = collision.GetComponent<Bullet> ();
                if (bullet != null && bullet.actorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    photonView.RPC (nameof (Hit), RpcTarget.All, bullet.id, bullet.actorNumber);
                }
            }
        }

        [PunRPC]
        private void Hit (int bulletId, int actorNumber)
        {
            bulletManager.Remove (bulletId, actorNumber);

            // 当たったやつは死ぬ
            Schedule<PlayerDeath> (playerController);
        }
    }
}
#endif