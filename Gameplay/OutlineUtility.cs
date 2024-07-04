using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    // Utility to control the outline shader and apply outline to specific game objects
    public static class OutlineUtility
    {
        private static readonly List<GameObject> Selected = new List<GameObject>();
        
        public static void AddOutline(GameObject gameObject, OutlineType type)
        {
            gameObject.layer = (int)type;
            foreach (var childGameObject in gameObject.GetComponentsInChildren<Renderer>().Select(renderer => renderer.gameObject))
            {
                if (!Selected.Contains(childGameObject))
                {
                    Selected.Add(childGameObject);
                }
            }
            Selected.Add(gameObject);
        }

        public static void ClearOutlines()
        {
            foreach (var gameObject in Selected)
            {
                // todo: Store information on which layer objects were before and restore properly
                gameObject.layer = 0;
            }
            
            Selected.Clear();
        }

        
    }
    
    public enum OutlineType
    {
        Good = 10,
        Bad = 11
    }
}