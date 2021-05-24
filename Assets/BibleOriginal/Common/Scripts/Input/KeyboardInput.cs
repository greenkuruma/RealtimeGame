using UnityEngine;

namespace RGame.Common {

    public class KeyboardInput : BaseInput
    {
        public string fireButtonName = "Fire1";

        public string horizontal = "Horizontal";
        public string vertical = "Vertical";

        public override Vector2 GenerateInput (out bool fire)
        {
            fire = Input.GetButtonDown (fireButtonName);

            return new Vector2 {
                x = Input.GetAxis(horizontal),
                y = Input.GetAxis(vertical)
            };
        }
    }
}
