using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoEditorToolkit.Filters
{
    public class Filter0 : IFilter
    {
        public ICanvasImage ApplyFilter(ICanvasImage source)
        {
            return source;
        }
    }
}
