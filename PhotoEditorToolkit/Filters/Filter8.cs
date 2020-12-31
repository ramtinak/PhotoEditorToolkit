using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Filters
{
    public class Filter8 : IFilter
    {
        public ICanvasImage ApplyFilter(ICanvasImage source)
        {
            var sepiaEffect = new SepiaEffect
            {
                Source = source,
                Intensity = 1
            };
            return sepiaEffect;
        }
    }
}
