using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Effects
{
    public sealed class BlurEffectX : IEffect
    {
        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 100;
        public GaussianBlurEffect Effect { get; private set; }
        public ICanvasImage ApplyEffect(ICanvasImage source, float value)
        {
            Effect = new GaussianBlurEffect
            {
                Source = source,
                BlurAmount = (float)(value / 100 * 12)
            };
            return Effect;
        }
    }
}
