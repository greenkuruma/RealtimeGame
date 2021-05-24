using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGame.Common
{
    /// <summary>
    /// プレイヤーの入力を生成
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        public Vector2 Axis { get; private set; }
        public bool triggerShot { get; private set; }

        IInput[] inputs;

        // Start is called before the first frame update
        void Start ()
        {
            inputs = GetComponents<IInput> ();
        }

        // Update is called once per frame
        void Update ()
        {
            GatherInputs ();
        }

        void GatherInputs ()
        {
            Axis = Vector2.zero;
            triggerShot = false;

            for (int i = 0 ; i < inputs.Length ; i++)
            {
                var inputSource = inputs[i];
                bool fire;
                Vector2 current = inputSource.GenerateInput (out fire);
                if (current.sqrMagnitude > 0)
                    Axis = current;
                triggerShot |= fire;
            }
        }
    }
}