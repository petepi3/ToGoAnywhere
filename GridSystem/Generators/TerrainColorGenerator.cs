using UnityEngine;

namespace Petepi.TGA.Grid.Generators
{
    public class TerrainColorGenerator : Generator
    {
        public Gradient gradient = new Gradient();
        public Color sideColor = new Color(1, 1, 1, 1);
        public float metallic = 0;
        public float glossiness = 0;
        public float minElevation = 2;
        public float maxElevation = -2;
        public override void Generate(ref Tile tile)
        {
            var interpolatedHeight = Mathf.InverseLerp(maxElevation, minElevation, tile.HeightOffset);
            tile.Color = gradient.Evaluate(interpolatedHeight);
            tile.SideColor = sideColor;
            tile.Glossiness = glossiness;
            tile.Metallic = metallic;
        }
    }
}