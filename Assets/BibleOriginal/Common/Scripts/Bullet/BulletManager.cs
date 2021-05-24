using System;
using System.Collections.Generic;
using UnityEngine;

namespace RGame.Common
{
    /// <summary>
    /// 弾管理。オブジェクトプールを有する
    /// </summary>
    public class BulletManager : MonoBehaviour
    {
        [SerializeField]
        private Bullet bulletPrefab = default;

        // アクティブな弾のリスト
        private List<Bullet> activeList = new List<Bullet> ();
        // 非アクティブな弾のオブジェクトプール
        private Stack<Bullet> inactivePool = new Stack<Bullet> ();

        private void Update ()
        {
            for (int i = activeList.Count - 1 ; i >= 0 ; i--)
            {
                var bullet = activeList[i];
                if (bullet.isActive)
                    bullet.OnUpdate ();
                else
                    Remove (bullet);
            }
        }

        public void Fire (int id, int actorNumber, Vector3 origin, bool toLeft, float speed, int timestamp, Func<int> serverTime)
        {
            // 非アクティブの弾があれば使い回す、なければ生成する
            var bullet = (inactivePool.Count > 0)
                ? inactivePool.Pop ()
                : Instantiate (bulletPrefab, transform);
            bullet.Activate (id, actorNumber, origin, toLeft, speed, timestamp, serverTime);
            activeList.Add (bullet);
        }

        public void Remove (Bullet bullet)
        {
            activeList.Remove (bullet);
            bullet.Diactivate ();
            inactivePool.Push (bullet);
        }

        public void Remove (int id, int ownerId)
        {
            foreach (var bullet in activeList)
            {
                if (bullet.equals (id, ownerId))
                {
                    Remove (bullet);
                    break;
                }
            }
        }
    }
}