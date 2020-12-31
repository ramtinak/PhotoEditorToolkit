// A modified version of https://github.com/UnigramDev/Unigram/blob/develop/Unigram/Unigram/Controls/PencilCanvas.cs

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using PhotoEditorToolkit.Filters;
using PhotoEditorToolkit.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PhotoEditorToolkit.Controls
{
    [TemplatePart(Name = "Canvas", Type = typeof(CanvasControl))]
    public partial class PhotoEditor : Control
    {
        public PhotoEditor()
        {
            DefaultStyleKey = typeof(PhotoEditor);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
