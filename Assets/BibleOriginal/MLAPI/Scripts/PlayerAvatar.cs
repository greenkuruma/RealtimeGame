using UnityEngine;
using Platformer.Mechanics;
using TMPro;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using MLAPI;
using MLAPI.NetworkVariable;
using RGame.Common;
using MLAPI.Messaging;

namespace RGame.MLAPI
{
    /// <summary>
    /// プレイヤーの挙動で、既存PlayerControllerに備わってない機能を持つ
    /// 弾発射、被弾、強化など
    /// </summary>
    public class PlayerAvatar : NetworkBehaviour
    {
        [SerializeField] float moveRatio = 0.05f;
        [SerializeField] float defaultBulletSpeed = 5f;
        [SerializeField] float bulletSpeedRatio = 0.2f;

        [SerializeField] TMP_Text nameLabel = default;

        public PlayerController playerController { get; private set; }

        NetworkVariable<int> power =
            new NetworkVariable<int> (new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        NetworkVariable<string> avatarName =
            new NetworkVariable<string> (new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });

        BulletManager bulletManager;

        PlayerInput playerInput;

        int shotId = 0;

        public int actorNumber => (int)OwnerClientId; // Bulletでのみ使用

        void Start ()
        {
            PlayerAvatarManager.Instance.Add (this);

            playerController = GetComponent<PlayerController> ();
            playerController.isMine = IsOwner;
            playerController.OnExitGame += FindObjectOfType<Reception> ().OnExitGame;
            power.OnValueChanged += PowerChanged;
            avatarName.OnValueChanged += AvatarNameChanged;

            if (IsOwner)
            {
                FindObjectOfType<GameController> ().model.player = playerController;
                var vCamera = FindObjectOfType<Cinemachine.CinemachineVirtualCamera> ();
                vCamera.Follow = transform;
                vCamera.LookAt = transform;

                power.Value = 0;
                avatarName.Value = System.Guid.NewGuid ().ToString ().Substring (0, 13);
            }

            StatusUpdate ();

            bulletManager = FindObjectOfType<BulletManager> ();
            playerInput = FindObjectOfType<PlayerInput> ();
        }

        internal void StatusUpdate ()
        {
            playerController.addMoveRatio = power.Value * moveRatio;
            UpdateText ();
        }

        public void PowerUp (int volume = 1)
        {
            power.Value += volume;
        }

        void UpdateText ()
        {
            nameLabel.text = $"{avatarName.Value}({power.Value.ToString ()})";
        }

        void PowerChanged (int prevF, int newF)
        {
            Debug.Log ("PowerChanged");
            StatusUpdate ();
        }
        void AvatarNameChanged (string prevS, string newS)
        {
            Debug.Log ("AvatarNameChanged");
            UpdateText ();
        }

        void Update ()
        {
            if (playerController.controlEnabled && IsOwner)
            {
                if (playerInput.triggerShot)
                {
                    var timestamp = ServerTime ();
                    var speed = defaultBulletSpeed + power.Value * bulletSpeedRatio;

                    Debug.Log ($"host={IsHost}, server={IsServer}, client={IsClient}");

                    shotId++;

                    ShotServerRpc (
                        transform.position,
                        playerController.isLeftward,
                        speed,
                        shotId,
                        timestamp
                    );
                }
            }
        }


        [ServerRpc]
        private void ShotServerRpc (Vector3 origin, bool toLeft, float speed, int shotId, int timestamp)
        {
            ShotClientRpc (
                transform.position,
                playerController.isLeftward,
                speed,
                shotId,
                timestamp
            );
        }

        [ClientRpc]
        private void ShotClientRpc (Vector3 origin, bool toLeft, float speed, int shotId, int timestamp)
        {
            Debug.Log ("ShotC");
            bulletManager.Fire (timestamp, actorNumber, origin, toLeft, speed, timestamp, ServerTime);
        }

        int ServerTime ()
        {
            return (int)(NetworkManager.NetworkTime * 1000);
        }
        private void OnTriggerEnter2D (Collider2D collision)
        {
            if (IsOwner)
            {
                var bullet = collision.GetComponent<Bullet> ();
                if (bullet != null && bullet.actorNumber != actorNumber)
                {
                    // ローカル処理が適切に行われると信じて
                    Debug.Log ("HitA");
                    HitServerRpc (bullet.id, bullet.actorNumber);
                    Debug.Log ("HitD");
                }
            }
        }

        [ServerRpc]
        void HitServerRpc (int bulletId, int actorNumber)
        {
            HitClientRpc (
                bulletId,
                actorNumber
            );
        }

        [ClientRpc]
        private void HitClientRpc (int bulletId, int actorNumber)
        {
            bulletManager.Remove (bulletId, actorNumber);

            // 当たったやつは死ぬ
            Schedule<PlayerDeath> (playerController);
        }
    }
}