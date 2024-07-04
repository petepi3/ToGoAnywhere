using UnityEngine;

namespace Petepi.TGA.Grid.Generators
{
    public class ResourceGenerator : ParameterPerlinGenerator
    {
        public Resource resource;
        public float amountMultiplier = 1;
        public bool highLight = false;
        public bool markUnpassable;
        
        public override void Generate(ref Tile tile)
        {
            var noiseValue = SampleNoise(tile.GridPosition);
            var spawn = noiseValue > 0;
            tile.Color = spawn && highLight ? Color.red : tile.Color;
            tile.Resource = spawn ? resource : tile.Resource;
            tile.ResourceAmount = spawn ? noiseValue * amountMultiplier : tile.ResourceAmount;
        }
    }
}