using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace PhotoEditorToolkit.Filters
{
    public class Filter7 : IFilter
    {
        public ICanvasImage ApplyFilter(ICanvasImage source)
        {
            var embossEffect = new EmbossEffect
            {
                Source = source
            };
            embossEffect.Amount = 5;
            embossEffect.Angle = 0;
            return embossEffect;
        }
    }
}
