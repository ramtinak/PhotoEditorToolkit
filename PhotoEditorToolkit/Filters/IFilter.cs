using Microsoft.Graphics.Canvas;

namespace PhotoEditorToolkit.Filters
{
    public interface IFilter
    {
        ICanvasImage ApplyFilter(ICanvasImage source);
    }
}
