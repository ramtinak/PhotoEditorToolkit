using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Filters
{
    internal class Filter4 : IFilter
    {
        public ICanvasImage ApplyFilter(ICanvasImage source)
        {
            return new HueRotationEffect
            {
                Source = source,
                Angle = 0.5f
            };
        }
    }
}
