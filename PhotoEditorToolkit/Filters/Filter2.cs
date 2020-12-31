using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Filters
{
    internal class Filter2 : IFilter
    {
        public ICanvasImage ApplyFilter(ICanvasImage source)
        {
            return new GrayscaleEffect
            {
                Source = source
            };
        }
    }
}
