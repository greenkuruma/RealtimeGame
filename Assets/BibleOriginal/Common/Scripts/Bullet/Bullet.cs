using UnityEngine;
using System.Collections;
using System;

namespace RGame.Common
{
    /// <summary>
    /// 弾。移動量はローカルではなくサーバー時間に依存する
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float lifetime = 10;

        private Vector3 origin;
        private Vector3 velocity;
        private int timestamp;

        public int id { get; private set; }
        public int actorNumber { get; private set; }
        public bool equals (int id, int actorNumber) => id == this.id && actorNumber == this.actorNumber;
        public bool isActive => gameObject.activeSelf;

        Coroutine lifetimeCoroutine = null;

        Func<int> serverTime;

        /// <summary>
        /// 初期化を兼ねた活性化
        /// </summary>
        public void Activate (int id, int actorNumber, Vector3 origin, bool toLeft, float speed, int timestamp, Func<int> serverTime)
        {
            this.id = id;
            this.actorNumber = actorNumber;
            this.origin = origin;
            velocity = speed * (toLeft ? Vector3.left : Vector3.right);
            this.timestamp = timestamp;
            this.serverTime = serverTime;

            OnUpdate (); // 座標初期化

            gameObject.SetActive (true);
            lifetimeCoroutine = StartCoroutine (LifetimeProcess ());
        }

        /// <summary>
        /// 寿命処理
        /// </summary>
        IEnumerator LifetimeProcess ()
        {
            float correctedLifeTime = lifetime - ElapsedTime ();
            if (correctedLifeTime > 0f)
                yield return new WaitForSeconds (correctedLifeTime);
            Diactivate ();
        }

        public void OnUpdate ()
        {
            // velocityは固定なので、経過時間を基に位置を算出
            transform.position = origin + velocity * ElapsedTime ();
        }

        public void Diactivate ()
        {
            StopCoroutine (lifetimeCoroutine);
            gameObject.SetActive (false);
        }

        // 経過時間算出
        float ElapsedTime ()
        {
            return Mathf.Max (0f, unchecked(serverTime () - timestamp) / 1000f);
        }
    }
}