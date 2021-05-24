using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using RGame.Common;
using UnityEngine.Events;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public float baseSpeed = 3;

        public float addMoveRatio = 0;
        public float maxSpeed => baseSpeed * (1 + addMoveRatio);

        public float baseJumpTakeOffSpeed = 7;
        public float jumpTakeOffSpeed => baseJumpTakeOffSpeed * (1 + addMoveRatio);

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        public bool putMovingLog = false;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;
        public bool isLeftward => spriteRenderer.flipX;

        float oldPositionX;

        public bool isMine;

        PlayerInput playerInput;

        public UnityAction OnExitGame;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            playerInput = FindObjectOfType<PlayerInput> ();
        }

        protected override void Update()
        {
            if (controlEnabled && isMine)
            {
                var axis = playerInput.Axis;

                if (putMovingLog)
                {
                    if (axis.x != 0 || axis.y != 0)
                        Debug.Log ($"x={axis.x} j={axis.y}");
                }

                move.x = axis.x;
                if (jumpState == JumpState.Grounded && axis.y > 0f)
                    jumpState = JumpState.PrepareToJump;
                else if (axis.y <= 0f)
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>(this);
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>(this);
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>(this);
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            var diffX = transform.localPosition.x - oldPositionX;

            if (diffX > 0.001f)
                spriteRenderer.flipX = false;
            else if (diffX < -0.001f)
                spriteRenderer.flipX = true;

            oldPositionX = transform.localPosition.x;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(diffX) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}