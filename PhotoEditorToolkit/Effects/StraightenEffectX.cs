using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Effects
{
    public sealed class StraightenEffectX : IEffect
    {
        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 100;
        public StraightenEffect Effect { get; private set; }
        public ICanvasImage ApplyEffect(ICanvasImage source, float value)
        {
            Effect = new StraightenEffect
            {
                Source = source,
                Angle = (float)(value / 500 * 2),
                MaintainSize = true
            };
            return Effect;
        }
    }
}
