using System;
using Petepi.TGA.Grid;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Petepi.TGA.Gameplay.BuildingSystem
{
    public class TownHall : Building
    {
        public float range = 10f;
        // TODO: generic range decal was added to Toolbar, use that one instead.
        public DecalProjector rangeDecal;
        
        public override void Initialize(GridSystem grid, Vector2Int position, Town town)
        {
            base.Initialize(grid, position, town);
            Town = TownSystem.Instance.CreateTown(this);
        }

        public void SetShowRange(bool doShow)
        {
            // magic numbers, this is obsolete as there is a generic range decal in Toolbar class
            // TODO: replace this with generic decal.
            rangeDecal.gameObject.SetActive(doShow);
            rangeDecal.ResizeAroundPivot(new Vector3(range*3,(range * 3)/GridSystem.TileWidth, 10));
        }
    }
}