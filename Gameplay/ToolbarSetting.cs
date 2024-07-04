using UnityEngine;
using UnityEngine.Events;

namespace Petepi.TGA.Gameplay
{
    /// <summary>
    /// UI controller for a single setting in the in-game toolbar
    /// </summary>
    /// <typeparam name="T">Type of variable that the setting controls</typeparam>
    public abstract class ToolbarSetting<T> : MonoBehaviour
    {
        public UnityEvent<T> valueChanged = new UnityEvent<T>();

        /// <summary>
        /// Called after the widget this behaviour is attached to was instantiated in UI
        /// </summary>
        /// <param name="optionName">Name of the option</param>
        /// <param name="initialValue">Value this setting starts at</param>
        public abstract void Init(string optionName, T initialValue);
    }
}