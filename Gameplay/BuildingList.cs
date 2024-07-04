using System.Collections.Generic;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    /// <summary>
    /// Holds a list of all buildings that's then passed build tool
    /// This is to have reference to all building as an asset instead of holding this info in a scene
    /// </summary>
    [CreateAssetMenu(menuName = "Gameplay/Building List")]
    public class BuildingList : ScriptableObject
    {
        public List<BuildingInfo> buildings;
    }
}