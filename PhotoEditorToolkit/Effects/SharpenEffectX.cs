using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Effects
{
    public sealed class SharpenEffectX : IEffect
    {
        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 100;
        public SharpenEffect Effect { get; private set; }
        public ICanvasImage ApplyEffect(ICanvasImage source, float value)
        {
            Effect = new SharpenEffect
            {
                Source = source,
                Amount = (float)(value * 0.1)
            };
            return Effect;
        }
    }
}
