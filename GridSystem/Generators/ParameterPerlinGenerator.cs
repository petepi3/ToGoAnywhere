using System;
using System.Collections.Generic;
using UnityEngine;

namespace Petepi.TGA.Grid.Generators
{
    public class ParameterPerlinGenerator : PerlinGenerator
    {
        public float scale;
        public float amplitude;
        public float heightOffset;
        [Range(1, 8)] public int layerCount;
        
        public float SampleNoise(Vector2 point)
        {
            return SampleNoise(point, amplitude, scale, heightOffset, layerCount);
        }

        public override void Generate(ref Tile tile)
        {
            var height = SampleNoise(tile.GridPosition);
            tile.HeightOffset = height;
        }
    }
}