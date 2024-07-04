using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    public class PhysicalPointer : MonoBehaviour
    {
        private static PhysicalPointer _instance;
        public static PhysicalPointer Instance => _instance ??= FindAnyObjectByType<PhysicalPointer>();

        public void SetColor(Color color)
        {
            GetComponent<MeshRenderer>().material.SetColor("_EmissiveColor", color * 20);
        }
    }
}