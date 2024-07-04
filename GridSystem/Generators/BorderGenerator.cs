using UnityEngine;
using UnityEngine.Serialization;

namespace Petepi.TGA.Grid.Generators
{
    public class BorderGenerator : ParameterPerlinGenerator
    {
        public float margin = 20;
        [FormerlySerializedAs("rate")] public float heightReductionRate = 20;
        
        public override void Generate(ref Tile tile)
        {
            var distanceToBorder = tile.DistanceToBorder;
            var falloff = Mathf.Pow(-Mathf.Min(distanceToBorder-margin, 0)/heightReductionRate, 2);
            tile.HeightOffset -= falloff;
        }
    }
}