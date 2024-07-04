using UnityEngine;

namespace Petepi.TGA.Grid.Generators
{
    public class LakeGenerator : Generator
    {
        public float cutoff = 0.5f;
        public Gradient waterColor = new Gradient();
        public float waterMetallic = 0;
        public float waterGlossiness = 0;
        public Gradient beachColor = new Gradient();
        public float beachMetallic = 0;
        public float beachGlossiness = 0;
        public float minHeight;
        public float beachHeight;
        
        public override void Generate(ref Tile tile)
        {
            var height = tile.HeightOffset;
            var max = cutoff + beachHeight;
            var isWater = height < cutoff;
            var isAffected = height < max;
            var depth = isWater ? Mathf.InverseLerp(minHeight, cutoff, height) : Mathf.InverseLerp(cutoff, max, height);
            var lakeColor = isWater ? waterColor.Evaluate(depth) : beachColor.Evaluate(depth);
            var color = isAffected ? lakeColor : tile.Color;
            var gloss = isWater ? waterGlossiness : beachGlossiness;
            var metal = isWater ? waterMetallic : beachMetallic;
            tile.Color = color;
            tile.SideColor = isWater ? color : tile.SideColor;
            tile.Glossiness = isWater ? gloss : tile.Glossiness;
            tile.Metallic = isWater ? metal : tile.Metallic;
            tile.WaterDepth = -Mathf.InverseLerp(cutoff, minHeight, height);
            tile.HeightOffset = Mathf.Max(height, cutoff);
            tile.Resource = isAffected ? Resource.None : tile.Resource;
            tile.ResourceAmount = isAffected ? 0 : tile.ResourceAmount;
        }
    }
}