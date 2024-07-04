using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Petepi.TGA.Grid.Generators
{
    public abstract class PerlinGenerator : Generator
    {
        private Octave[] _layers;
        private const float MaxOffset = 20000;
        
        protected float SampleNoise(Vector2 point, float amplitude, float scale, float heightOffset, int layerCount)
        {
            var influence = 1f;
            var value = 0f;
            for (int i = 0; i < layerCount; i++)
            {
                var layerValue = _layers[i].Evaluate(point, (1f / influence)*(1f/scale));
                layerValue *= influence;
                value += layerValue;
                influence /= 2;
            }

            return (value * amplitude)+heightOffset;
        }

        public override void OnSeed()
        {
            for (int i = 0; i < 8; i++)
            {
                _layers[i].offset = new Vector2(Random.value * MaxOffset, Random.value * MaxOffset);
            }
        }

        public override void OnInit()
        {
            SetupLayers();
        }

        private void SetupLayers()
        {
            _layers = new Octave[8];
            for (int i = 0; i < 8; i++)
            {
                _layers[i] = new Octave();
            }
        }
        
        [Serializable]
        struct Octave
        {
            public Vector2 offset;
            public float Evaluate(Vector2 point, float scale)
            {
                var samplePosition = (point*scale) - offset;
                return Mathf.PerlinNoise(samplePosition.x, samplePosition.y);
            }
        }
    }
}