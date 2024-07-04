using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Petepi.TGA.Grid.Generators
{
    public class BiomeGenerator : ParameterPerlinGenerator
    {
        [FormerlySerializedAs("from")] public float smoothingMinimum = 4;
        [FormerlySerializedAs("to")] public float smoothingMaximum = 5;
        public GameObject generatorObject;
        public float resourceSpawnThreshold = 0.25f;

        private Generator[] _subGenerators;
        
        public override void Generate(ref Tile tile)
        {
            if (!generatorObject)
            {
                return;
            }

            var biomeTile = new Tile(tile);

            biomeTile.Resource = Resource.None;
            biomeTile.ResourceAmount = 0;

            var influence = CalculateInfluence(tile);

            foreach (var subGenerator in _subGenerators)
            {
                subGenerator.Generate(ref biomeTile);
            }
            
            tile.Color = Color.Lerp(tile.Color, biomeTile.Color, influence);
            tile.SideColor = Color.Lerp(tile.SideColor, biomeTile.SideColor, influence);
            tile.HeightOffset = Mathf.Lerp(tile.HeightOffset, biomeTile.HeightOffset, influence);
            tile.Glossiness = Mathf.Lerp(tile.Glossiness, biomeTile.Glossiness, influence);
            tile.Metallic = Mathf.Lerp(tile.Metallic, biomeTile.Metallic, influence);
            var spawnResources = influence > resourceSpawnThreshold;
            tile.Resource = spawnResources ? biomeTile.Resource : tile.Resource;
            tile.ResourceAmount = spawnResources ? biomeTile.ResourceAmount : tile.ResourceAmount;
        }

        protected virtual float CalculateInfluence(Tile tile)
        {
            var noiseValue = SampleNoise(tile.GridPosition);
            return Mathf.SmoothStep(smoothingMinimum, smoothingMaximum, noiseValue);
        }

        private void OnSubGeneratorPropertyChange()
        {
            editorPropertyChange.Invoke();
        }
        
        public override void OnInit()
        {
            base.OnInit();
            if (_subGenerators != null)
            {
                foreach (var generator in _subGenerators)
                {
                    generator.editorPropertyChange.RemoveListener(OnSubGeneratorPropertyChange);
                }
            }
            if (!generatorObject)
            {
                return;
            }
            _subGenerators = generatorObject.GetComponents<Generator>();
            foreach (var generator in _subGenerators)
            {
                generator.editorPropertyChange.AddListener(OnSubGeneratorPropertyChange);
                generator.OnInit();
            }
        }

        public override void OnSeed()
        {
            base.OnSeed();
            if (!generatorObject)
            {
                return;
            }
            foreach (var generator in _subGenerators)
            {
                generator.OnSeed();
            }
        }
    }
}