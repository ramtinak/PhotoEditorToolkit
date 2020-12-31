using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Filters
{
    internal class Filter3 : IFilter
    {
        public ICanvasImage ApplyFilter(ICanvasImage source)
        {
            return new InvertEffect
            {
                Source = source
            };
        }
    }
}
