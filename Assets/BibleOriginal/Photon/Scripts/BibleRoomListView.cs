using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if !PHOTON_UNITY_NETWORKING
namespace RGame.Photon
{
    public class BibleRoomListView : MonoBehaviour
    {
        [SerializeField]
        private BibleRoomListEntry roomListEntryPrefab = default;
    }
}
#else
using Photon.Pun;
using Photon.Realtime;

namespace RGame.Photon
{
    /// <summary>
    /// ルームに入るボタンのビュー
    /// </summary>
    public class BibleRoomListView : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private BibleRoomListEntry roomListEntryPrefab = default;

        private ScrollRect scrollRect;
        private Dictionary<string, BibleRoomListEntry> activeEntries = new Dictionary<string, BibleRoomListEntry> ();
        private Stack<BibleRoomListEntry> inactiveEntries = new Stack<BibleRoomListEntry> ();

        private void Awake ()
        {
            scrollRect = GetComponent<ScrollRect> ();
        }

        public override void OnRoomListUpdate (List<RoomInfo> roomList)
        {
            Debug.Log ($"OnRoomListUpdate length={roomList.Count}");
            foreach (var info in roomList)
            {
                BibleRoomListEntry entry;
                if (activeEntries.TryGetValue (info.Name, out entry))
                {
                    if (!info.RemovedFromList)
                    {
                        // リスト要素を更新する
                        entry.Activate (info);
                    }
                    else
                    {
                        // リスト要素を削除する
                        activeEntries.Remove (info.Name);
                        entry.Deactivate ();
                        inactiveEntries.Push (entry);
                    }
                }
                else if (!info.RemovedFromList)
                {
                    // リスト要素を追加する
                    entry = (inactiveEntries.Count > 0)
                        ? inactiveEntries.Pop ().SetAsLastSibling ()
                        : Instantiate (roomListEntryPrefab, scrollRect.content);
                    entry.Activate (info);
                    activeEntries.Add (info.Name, entry);
                }
            }
        }
    }
}
#endif