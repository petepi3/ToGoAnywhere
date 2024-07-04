using Petepi.TGA.Gameplay;
using Petepi.TGA.Gameplay.BuildingSystem;
using UnityEngine;

namespace Petepi.TGA.Grid
{
    public struct Tile
    {
        public Vector2Int GridPosition;
        public Vector3 WorldPosition;
        public Color Color;
        public Color SideColor;
        public float Glossiness;
        public float Metallic;
        public float HeightOffset;
        public Resource Resource;
        public float ResourceAmount;
        public float WaterDepth;
        public Building Building;
        public Road Road;
        public RoadIntersection Intersection;
        public float DistanceToBorder;

        public bool IsPassable => !Building &&
                                  WaterDepth == 0 &&
                                  Resource == Resource.None;

        public bool TryGetInspector(out ITileInspector inspector)
        {
            if (Building && Building is ITileInspector building)
            {
                inspector = building;
                return true;
            }
            
            // TODO: Add inspector to intersection
            
            if (Road && Road is ITileInspector road)
            {
                inspector = road;
                return true;
            }

            inspector = null;
            return false;
        }

        public static void Aggregate(ref AggregatedTiles agr, ref Tile tile, int i)
        {
            agr.Color[i] = tile.Color;
            agr.SideColor[i] = tile.SideColor;
            agr.Glossiness[i] = tile.Glossiness;
            agr.Metallic[i] = tile.Metallic;
            agr.HeightOffset[i] = tile.HeightOffset;
            agr.WorldPosition[i] = tile.WorldPosition;
            agr.GridPosition[i] = tile.GridPosition;
            agr.Resource[i] = tile.Resource;
            agr.ResourceAmount[i] = tile.ResourceAmount;
            agr.WaterDepth[i] = tile.WaterDepth;
            agr.Building[i] = tile.Building;
            agr.Road[i] = tile.Road;
            agr.Intersection[i] = tile.Intersection;
        }
        
        public Tile(Tile original)
        {
            GridPosition = original.GridPosition;
            Color = original.Color;
            SideColor = original.SideColor;
            Glossiness = original.Glossiness;
            Metallic = original.Metallic;
            HeightOffset = original.HeightOffset;
            WorldPosition = original.WorldPosition;
            Resource = original.Resource;
            ResourceAmount = original.ResourceAmount;
            WaterDepth = original.WaterDepth;
            Building = original.Building;
            Road = original.Road;
            DistanceToBorder = original.DistanceToBorder;
            Intersection = original.Intersection;
        }
    }

    public struct AggregatedTiles
    {
        public Vector3[] WorldPosition;
        public Vector2Int[] GridPosition;
        public Vector4[] Color;
        public Vector4[] SideColor;
        public float[] Glossiness;
        public float[] Metallic;
        public float[] HeightOffset;
        public Resource[] Resource;
        public float[] ResourceAmount;
        public float[] WaterDepth;
        public Building[] Building;
        public Road[] Road;
        public float[] DistanceToBorder;
        public RoadIntersection[] Intersection;
        public int ChunkWidth;

        public Tile GetTile(int i)
        {
            return new Tile
            {
                GridPosition = GridPosition[i],
                WorldPosition = WorldPosition[i],
                Color = Color[i],
                SideColor = SideColor[i],
                Glossiness = Glossiness[i],
                Metallic = Metallic[i],
                HeightOffset = HeightOffset[i],
                Resource = Resource[i],
                ResourceAmount = ResourceAmount[i],
                WaterDepth = WaterDepth[i],
                Building = Building[i],
                Road = Road[i],
                DistanceToBorder = DistanceToBorder[i],
                Intersection = Intersection[i],
            };
        }
        
        public AggregatedTiles(int chunkWidth)
        {
            var tileCount = chunkWidth * chunkWidth;
            GridPosition = new Vector2Int[tileCount];
            WorldPosition = new Vector3[tileCount];
            Color = new Vector4[tileCount];
            SideColor = new Vector4[tileCount];
            Glossiness = new float[tileCount];
            Metallic = new float[tileCount];
            HeightOffset = new float[tileCount];
            Resource = new Resource[tileCount];
            ResourceAmount = new float[tileCount];
            WaterDepth = new float[tileCount];
            Building = new Building[tileCount];
            ChunkWidth = chunkWidth;
            Road = new Road[tileCount];
            DistanceToBorder = new float[tileCount];
            Intersection = new RoadIntersection[tileCount];
        }
    }
}