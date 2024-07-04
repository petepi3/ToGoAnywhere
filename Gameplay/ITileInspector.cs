using System.Text;

namespace Petepi.TGA.Gameplay
{
    // Used by tile inspector tool to allow structures on the map to
    // show text in tooltip, fields in inspector and in-game gizmos
    public interface ITileInspector
    {
        public void StartInspector(StringBuilder tooltipText);
        public void EndInspector(ToolbarController toolbar);
    }
}