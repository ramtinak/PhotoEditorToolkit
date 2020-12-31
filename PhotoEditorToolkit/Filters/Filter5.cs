using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Filters
{
    public class Filter5 : IFilter
    {
        public ICanvasImage ApplyFilter(ICanvasImage source)
        {
            var temperatureAndTintEffect = new TemperatureAndTintEffect
            {
                Source = source
            };
            temperatureAndTintEffect.Temperature = 0.7f;
            temperatureAndTintEffect.Tint = 0.7f;

            return temperatureAndTintEffect;
        }
    }
}
