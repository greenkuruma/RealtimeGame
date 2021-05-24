using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class BibleRoomListEntry : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text nameLabel = default;
        [SerializeField]
        private TMP_Text messageLabel = default;
        [SerializeField]
        private TMP_Text playerCounter = default;
    }
}
#else
using Photon.Pun;
using Photon.Realtime;

namespace RGame.Photon
{
    /// <summary>
    /// ルーム情報を表示するボタン
    /// </summary>
    public class BibleRoomListEntry : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text nameLabel = default;
        [SerializeField]
        private TMP_Text messageLabel = default;
        [SerializeField]
        private TMP_Text playerCounter = default;

        private RectTransform rectTransform;
        private Button button;
        private string roomName;

        private void Awake ()
        {
            rectTransform = GetComponent<RectTransform> ();
            button = GetComponent<Button> ();
        }

        private void Start ()
        {
            // リスト要素がクリックされたら、対応したルーム名のルームに参加する
            button.onClick.AddListener (() => PhotonNetwork.JoinRoom (roomName));
        }

        public void Activate (RoomInfo info)
        {
            roomName = info.Name;

            nameLabel.text = (string)info.CustomProperties["DisplayName"];
            messageLabel.text = (string)info.CustomProperties["Comment"];
            playerCounter.SetText ("{0}/{1}", info.PlayerCount, info.MaxPlayers);
            // ルームの参加人数が満員でない時だけ、クリックできるようにする
            button.interactable = (info.PlayerCount < info.MaxPlayers);

            gameObject.SetActive (true);
        }

        public void Deactivate ()
        {
            gameObject.SetActive (false);
        }

        public BibleRoomListEntry SetAsLastSibling ()
        {
            rectTransform.SetAsLastSibling ();
            return this;
        }
    }
}
#endif