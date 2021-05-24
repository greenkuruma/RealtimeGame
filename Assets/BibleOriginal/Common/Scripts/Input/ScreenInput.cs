using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RGame.Common
{
    /// <summary>
    /// UGUIからの入力
    /// </summary>
    public class ScreenInput : BaseInput
    {
        Dictionary<string, bool> pressKeys = new Dictionary<string, bool> ();
        bool triggerFire;

        void Awake ()
        {
            pressKeys["Left"] = false;
            pressKeys["Right"] = false;
            pressKeys["Up"] = false;
            pressKeys["Down"] = false;
        }

        public void OnPress (BaseEventData data)
        {
            pressKeys[data.selectedObject.name] = true;
        }
        public void OnRelease (BaseEventData data)
        {
            pressKeys[data.selectedObject.name] = false;
        }
        public void OnTriggerFire ()
        {
            triggerFire = true;
        }
        public override Vector2 GenerateInput (out bool fire)
        {
            fire = triggerFire;
            triggerFire = false;

            return new Vector2
            {
                x = (pressKeys["Left"] ? -1 : 0) + (pressKeys["Right"] ? 1 : 0),
                y = (pressKeys["Down"] ? -1 : 0) + (pressKeys["Up"] ? 1 : 0),
            };
        }
    }
}