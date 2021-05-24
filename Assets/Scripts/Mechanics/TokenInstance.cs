using System;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class contains the data required for implementing token collection mechanics.
    /// It does not perform animation of the token, this is handled in a batch by the 
    /// TokenController in the scene.
    /// </summary>
    [RequireComponent (typeof (Collider2D))]
    public class TokenInstance : MonoBehaviour
    {
        public AudioClip tokenCollectAudio;
        [Tooltip ("If true, animation will start at a random position in the sequence.")]
        public bool randomAnimationStartTime = false;
        [Tooltip ("List of frames that make up the animation.")]
        public Sprite[] idleAnimation, collectedAnimation;

        internal Sprite[] sprites = new Sprite[0];

        internal SpriteRenderer _renderer;

        //unique index which is assigned by the TokenController in a scene.
        internal int tokenIndex = -1;
        TokenController controller;
        //active frame in animation, updated by the controller.
        internal int frame = 0;
        internal bool collected = false;

        public Action<PlayerController> tryGetToken = null;

        void Awake ()
        {
            _renderer = GetComponent<SpriteRenderer> ();
            if (randomAnimationStartTime)
                frame = UnityEngine.Random.Range (0, sprites.Length);
            sprites = idleAnimation;

            controller = FindObjectOfType<TokenController> ();
            controller.tokens.Add(this);
        }

        void OnTriggerEnter2D (Collider2D other)
        {
            //only exectue OnPlayerEnter if the player collides with this token.
            var player = other.gameObject.GetComponent<PlayerController> ();
            if (player != null) OnPlayerEnter (player);
        }

        void OnPlayerEnter (PlayerController player)
        {
            if (!player.isMine)
                return;
            if (collected) return;

            if (tryGetToken != null)
                tryGetToken (player);
            else
                OnFinish ();
        }

        public void OnFinish ()
        {
            Debug.Log ("OnFinish");
            //disable the gameObject and remove it from the controller update list.
            frame = 0;
            sprites = collectedAnimation;
            collected = true;

            AudioSource.PlayClipAtPoint (tokenCollectAudio, transform.position);
        }
    }
}