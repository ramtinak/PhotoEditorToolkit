using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Effects
{
    public sealed class ExposureEffectX : IEffect
    {
        public double Minimum { get; set; } = -50;
        public double Maximum { get; set; } = 50;
        public ExposureEffect Effect { get; private set; }
        public ICanvasImage ApplyEffect(ICanvasImage source, float value)
        {
            Effect = new ExposureEffect
            {
                Source = source,
                Exposure = (float)(value / 500 * 2)
            };
            return Effect;
        }
    }
}
