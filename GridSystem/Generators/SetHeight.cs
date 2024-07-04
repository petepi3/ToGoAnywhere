namespace Petepi.TGA.Grid.Generators
{
    public class SetHeight : Generator
    {
        public float height = 0;
        
        public override void Generate(ref Tile tile)
        {
            tile.HeightOffset = height;
        }
    }
}