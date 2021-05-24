using UnityEngine;

namespace RGame.Common
{
    public interface IInput
    {
        Vector2 GenerateInput(out bool fire);
    }

    public abstract class BaseInput : MonoBehaviour, IInput
    {
        public abstract Vector2 GenerateInput(out bool fire);
    }
}
