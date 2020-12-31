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
    public partial class CustomCanvas : Control
    {
        private static Color? ERASING_STROKE = null;
        private static readonly float ERASING_STROKE_THICKNESS = 20;

        private CanvasRenderTarget _renderTarget;

        private ConcurrentDictionary<uint, SmoothPathBuilder> _builders;
        private CanvasMode _mode;

        private bool _needToCreateSizeDependentResources;
        private bool _needToRedrawInkSurface;

        private CanvasControl _canvas;

        public CustomCanvas()
        {
            DefaultStyleKey = typeof(CustomCanvas);
            InitPathBuilderHistory();
        }

        protected override void OnApplyTemplate()
        {
            _builders = new ConcurrentDictionary<uint, SmoothPathBuilder>();

            _canvas = (CanvasControl)GetTemplateChild("Canvas");

            _canvas.SizeChanged += OnSizeChanged;

            _canvas.CreateResources += OnCreateResources;
            _canvas.Draw += OnDraw;

            _canvas.PointerPressed += OnPointerPressed;
            _canvas.PointerMoved += OnPointerMoved;
            _canvas.PointerReleased += OnPointerReleased;

            base.OnApplyTemplate();
        }

        public ICanvasResourceCreatorWithDpi Creator => _canvas;


        public CanvasMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public Rect CropRect;
        public void SetRect(Rect cropRectangle)
        {
            CropRect = cropRectangle;
        }

        public event EventHandler StrokesChanged;

        public Color Stroke { get; set; }

        public float StrokeThickness { get; set; }


        public void Invalidate()
        {
            _canvas?.Invalidate();
        }

        #region Resources

        private void OnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            CreateSizeDependentResources();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _needToCreateSizeDependentResources = true;
            _canvas.Invalidate();
        }

        private void CreateSizeDependentResources()
        {
            _renderTarget = new CanvasRenderTarget(_canvas, _canvas.Size);

            _needToCreateSizeDependentResources = false;
            _needToRedrawInkSurface = true;
        }

        #endregion

        #region Drawing

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _canvas.CapturePointer(e.Pointer);

            var point = e.GetCurrentPoint(_canvas);
            var erasing = _mode == CanvasMode.Eraser || point.Properties.IsEraser;

            _builders[point.PointerId] = new SmoothPathBuilder(point.Position.ToVector2() / _canvas.Size.ToVector2())
            {
                Stroke = erasing ? ERASING_STROKE : Stroke,
                StrokeThickness = erasing ? ERASING_STROKE_THICKNESS : StrokeThickness
            };
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_builders.TryGetValue(e.Pointer.PointerId, out SmoothPathBuilder _builder))
            {
                _builder.MoveTo(e.GetCurrentPoint(_canvas).Position.ToVector2() / _canvas.Size.ToVector2());
                _canvas.Invalidate();
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _canvas.ReleasePointerCapture(e.Pointer);

            if (_builders.TryRemove(e.Pointer.PointerId, out SmoothPathBuilder builder))
            {
                using (var session = _renderTarget.CreateDrawingSession())
                    DrawPath(session, builder, _renderTarget.Size.ToVector2());
                PathBuilderHistory.Add(builder);

                StrokesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_needToCreateSizeDependentResources)
                CreateSizeDependentResources();

            GetDrawings(args);
            //var filter = CurrentFilter.ApplyFilter(_image ?? _renderTarget);
            //args.DrawingSession.DrawImage(filter, GetImageDrawingRect(), _image != null ? _image.Bounds : new Rect(0,0,1,1));

            //if (_needToRedrawInkSurface)
            //{
            //    using (var canvas = _renderTarget.CreateDrawingSession())
            //    {
            //        canvas.Clear(Colors.Transparent);
            //        foreach (var builder in PathBuilderHistory.ReversedUndoItems())
            //            DrawPath(canvas, builder, _renderTarget.Size.ToVector2());
            //    }
            //}
            //args.DrawingSession.DrawImage( _renderTarget);
            foreach (var builder in _builders.Values)
                DrawPath(args.DrawingSession, builder, sender.Size.ToVector2());
        }

        private CanvasDrawingSession GetDrawings(CanvasDrawEventArgs args = null, bool always = false)
        {
            //CanvasDevice device = CanvasDevice.GetSharedDevice();
            //CanvasRenderTarget target = new CanvasRenderTarget(device, (float)_image.Size.Width, (float)_image.Size.Height, _image.Dpi);
            CanvasDrawingSession graphics = args.DrawingSession;
            //using (CanvasDrawingSession graphics = args.DrawingSessiontarget.CreateDrawingSession())
            {
                var filter = CurrentFilter.ApplyFilter(_image ?? _renderTarget);
                if (args != null)
                    args?.DrawingSession.DrawImage(filter, GetImageDrawingRect(), _image != null ? _image.Bounds : new Rect(0, 0, 1, 1));
                else
                    graphics.DrawImage(filter, GetImageDrawingRect(), _image != null ? _image.Bounds : new Rect(0, 0, 1, 1));
                if (_needToRedrawInkSurface || always)
                {
                    using (var canvas = _renderTarget.CreateDrawingSession())
                    {
                        canvas.Clear(Colors.Transparent);
                        foreach (var builder in PathBuilderHistory.ReversedUndoItems())
                            DrawPath(canvas, builder, _renderTarget.Size.ToVector2());
                    }
                }
                if (args != null)
                    args.DrawingSession.DrawImage(_renderTarget);
                else
                    graphics.DrawImage(_renderTarget);
            }
            return graphics;
        }
        private Rect GetImageDrawingRect()
        {
            Rect des;
            if(_image == null)
                return new Rect(0, 0, 1, 1);
            var image_w = _image.Size.Width;
            var image_h = _image.Size.Height;
            var w = ActualWidth;
            var h = ActualHeight;
            if (image_w / image_h > w / h)
            {
                var left = 0;

                var width = w;
                var height = (image_h / image_w) * width;

                var top = (h - height) / 2 + 0;

                des = new Rect(left, top, width, height);
            }
            else
            {
                var top = 0;
                var height = h;
                var width = (image_w / image_h) * height;
                var left = (w - width) / 2 + 0;
                des = new Rect(left, top, width, height);
            }
            return des;
        }
        public void RemoveImage()
        {
            _image = null;
            Invalidate();
        }

        public static void DrawPath(CanvasDrawingSession canvas, SmoothPathBuilder builder, Vector2 canvasSize)
        {
            var geometry = builder.ToGeometry(canvas, canvasSize);
            var style = new CanvasStrokeStyle
            {
                StartCap = CanvasCapStyle.Round,
                EndCap = CanvasCapStyle.Round,
                LineJoin = CanvasLineJoin.Round
            };

            canvas.Blend = builder.Stroke == null ? CanvasBlend.Copy : CanvasBlend.SourceOver;
            canvas.DrawGeometry(geometry, builder.Stroke ?? Colors.Transparent, builder.StrokeThickness, style);
        }

        #endregion

        public void Reset()
        {
            CurrentFilter = new Filter0();
            _image = null;
            PathBuilderHistory?.Reset();
        }
        #region Filters
        public List<IFilter> Filters { get; private set; } = new List<IFilter>
        {
            new Filter0(),
            new Filter1(),
            new Filter2(),
            new Filter3(),
            new Filter4(),
            new Filter5(),
            new Filter6(),
            new Filter7(),
            new Filter8()
        };

        public IFilter CurrentFilter
        {
            get
            {
                return (IFilter)GetValue(CurrentFilterProperty);
            }
            set
            {
                SetValue(CurrentFilterProperty, value);
                ApplyFilter(value);
            }
        }
        public static readonly DependencyProperty CurrentFilterProperty =
            DependencyProperty.Register("CurrentFilter",
                typeof(bool),
                typeof(CustomCanvas),
                new PropertyMetadata(new Filter0()));

        public void ApplyFilter(IFilter filter)
        {
            if (filter?.GetType() != CurrentFilter?.GetType())
            {
                CurrentFilter = filter;
                return;
            }
            Invalidate();
        }
        #endregion
        CanvasBitmap _image;

        public void SetSource(SoftwareBitmap file)
        {
            var device = CanvasDevice.GetSharedDevice();
            _image = CanvasBitmap.CreateFromSoftwareBitmap(device, file);
        }

        StorageFile _originalFile;
        public void SetFile(StorageFile file)
        {
            _originalFile = file;
        }
        public async Task<StorageFile> SaveAsync(StorageFile file)
        {
            using (var fileStream = await _originalFile.OpenAsync(FileAccessMode.Read))
            using(var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var cropRectangle = CropRect;
                var cropWidth = (double)decoder.PixelWidth;
                var cropHeight = (double)decoder.PixelHeight;

                if (decoder.PixelWidth > 1920 || decoder.PixelHeight > 1920)
                {
                    double ratioX = (double)1920 / cropWidth;
                    double ratioY = (double)1920 / cropHeight;
                    double ratio = Math.Min(ratioX, ratioY);

                    cropWidth = cropWidth * ratio;
                    cropHeight = cropHeight * ratio;
                }


                cropRectangle = new Rect(
                    cropRectangle.X * decoder.PixelWidth,
                    cropRectangle.Y * decoder.PixelHeight,
                    cropRectangle.Width * decoder.PixelWidth,
                    cropRectangle.Height * decoder.PixelHeight);

                var (scaledCrop, scaledSize) = ImageHelper.Scale(cropRectangle, new Size(decoder.PixelWidth, decoder.PixelHeight), new Size(cropWidth, cropHeight), 1280, 0);

                var bounds = new BitmapBounds
                {
                    X = (uint)scaledCrop.X,
                    Y = (uint)scaledCrop.Y,
                    Width = (uint)scaledCrop.Width,
                    Height = (uint)scaledCrop.Height
                };

                var transform = new BitmapTransform
                {
                    ScaledWidth = (uint)scaledSize.Width,
                    ScaledHeight = (uint)scaledSize.Height,
                    Bounds = bounds,
                    InterpolationMode = BitmapInterpolationMode.Linear,
                };
                var pixelData = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);
                encoder.SetSoftwareBitmap(pixelData);
                await encoder.FlushAsync();
            }
            return await Save2Async(file);
        }
        public async Task<StorageFile> Save2Async(StorageFile file)
        {
            var device = CanvasDevice.GetSharedDevice();
            using (var ouputStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = await CanvasBitmap.LoadAsync(device, ouputStream, 96);
                var rectangle = CropRect;

                var canvas1 = new CanvasRenderTarget(device, (float)bitmap.Size.Width, (float)bitmap.Size.Height, bitmap.Dpi);
                var canvas2 = new CanvasRenderTarget(device, (float)bitmap.Size.Width, (float)bitmap.Size.Height, bitmap.Dpi);
                var size = canvas1.Size.ToVector2();

                var scaleX = 1 / (float)rectangle.Width;
                var scaleY = 1 / (float)rectangle.Height;

                var offsetX = (float)rectangle.X * scaleX;
                var offsetY = (float)rectangle.Y * scaleY;
                using (var session = canvas1.CreateDrawingSession())
                {
                    session.Transform = Matrix3x2.Multiply(session.Transform, Matrix3x2.CreateScale(scaleX, scaleY));
                    session.Transform = Matrix3x2.Multiply(session.Transform, Matrix3x2.CreateTranslation(-(offsetX * size.X), -(offsetY * size.Y)));

                    foreach (var builder in PathBuilderHistory.ReversedUndoItems())
                        DrawPath(session, builder, size);
                }

                using (var session = canvas2.CreateDrawingSession())
                {
                    if (CurrentFilter != null)
                        session.DrawImage(CurrentFilter.ApplyFilter(bitmap));
                    else
                        session.DrawImage(bitmap);
                    session.DrawImage(canvas1);
                }
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    await canvas2.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg);
                canvas1.Dispose();
                canvas2.Dispose();
            }
            return file;
        }
    }

    public enum CanvasMode
    {
        Stroke,
        Eraser
    }

    public sealed class SmoothPathBuilder
    {
        private List<Vector2> _controlPoints;
        private List<Vector2> _path;

        private Vector2 _beginPoint;

        public SmoothPathBuilder(Vector2 beginPoint)
        {
            _beginPoint = beginPoint;

            _controlPoints = new List<Vector2>();
            _path = new List<Vector2>();
        }

        public Color? Stroke { get; set; }
        public float StrokeThickness { get; set; }

        public Vector2 BeginPoint
        {
            get => _beginPoint;
            set => _beginPoint = value;
        }

        public List<Vector2> Path
        {
            get => _path;
            set => _path = value;
        }

        public void MoveTo(Vector2 point)
        {
            if (_controlPoints.Count < 4)
            {
                _controlPoints.Add(point);
                return;
            }

            var endPoint = new Vector2(
                (_controlPoints[2].X + point.X) / 2,
                (_controlPoints[2].Y + point.Y) / 2);

            _path.Add(_controlPoints[1]);
            _path.Add(_controlPoints[2]);
            _path.Add(endPoint);

            _controlPoints = new List<Vector2> { endPoint, point };
        }

        public void EndFigure(Vector2 point)
        {
            if (_controlPoints.Count > 1)
            {
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    MoveTo(point);
                }
            }
        }

        public CanvasGeometry ToGeometry(ICanvasResourceCreator resourceCreator, Vector2 canvasSize)
        {
            var multiplier = canvasSize; //_imageSize / canvasSize;

            var builder = new CanvasPathBuilder(resourceCreator);
            builder.BeginFigure(_beginPoint * multiplier);

            for (int i = 0; i < _path.Count; i += 3)
            {
                builder.AddCubicBezier(
                    _path[i] * multiplier,
                    _path[i + 1] * multiplier,
                    _path[i + 2] * multiplier);
            }

            builder.EndFigure(CanvasFigureLoop.Open);

            return CanvasGeometry.CreatePath(builder);
        }
    }
}
