// A modified version of https://github.com/UnigramDev/Unigram/blob/develop/Unigram/Unigram/Controls/PencilCanvas.cs

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using PhotoEditorToolkit.Classes;
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
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace PhotoEditorToolkit.Controls
{
    [TemplatePart(Name = CropperPart, Type = typeof(CustomImageCropper))]
    [TemplatePart(Name = CanvasPart, Type = typeof(CustomCanvas))]
    [TemplatePart(Name = DrawSliderPart, Type = typeof(ColorSlider))]
    [TemplatePart(Name = AspectRatioSliderPart, Type = typeof(Slider))]
    [TemplatePart(Name = CancelButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = DrawButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = CropButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = UndoButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = RedoButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = BrushButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = EraseButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = AcceptButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = DefaultToolbarPart, Type = typeof(Grid))]
    [TemplatePart(Name = DrawToolbarPart, Type = typeof(Grid))]
    public partial class PhotoEditor : Control
    {
        private CustomImageCropper Cropper;
        private CustomCanvas Canvas;
        private ColorSlider DrawSlider;
        private Slider AspectRatioSlider;

        private Grid DefaultToolbar,
            DrawToolbar;

        private Button CancelButton,
            DrawButton,
            CropButton,
            UndoButton,
            RedoButton,
            BrushButton,
            EraseButton,
            AcceptButton;
        #region Properties

        public double? AspectRatio
        {
            get { return (double?)GetValue(AspectRatioProperty); }
            set { SetValue(AspectRatioProperty, value); }
        }
        public static readonly DependencyProperty AspectRatioProperty =
            DependencyProperty.Register(nameof(AspectRatio), typeof(double?), typeof(PhotoEditor), new PropertyMetadata(null, OnAspectRatioChanged));

        #endregion Properties

        public PhotoEditor()
        {
            DefaultStyleKey = typeof(PhotoEditor);
        }

        protected override void OnApplyTemplate()
        {
            Cropper = (CustomImageCropper)GetTemplateChild(CropperPart);
            Canvas = (CustomCanvas)GetTemplateChild(CanvasPart);
            DrawSlider = (ColorSlider)GetTemplateChild(DrawSliderPart);
            AspectRatioSlider = (Slider)GetTemplateChild(AspectRatioSliderPart);
            DefaultToolbar = (Grid)GetTemplateChild(DefaultToolbarPart);
            DrawToolbar = (Grid)GetTemplateChild(DrawToolbarPart);


            CancelButton = (Button)GetTemplateChild(CancelButtonPart);
            DrawButton = (Button)GetTemplateChild(DrawButtonPart);
            CropButton = (Button)GetTemplateChild(CropButtonPart);
            UndoButton = (Button)GetTemplateChild(UndoButtonPart);
            RedoButton = (Button)GetTemplateChild(RedoButtonPart);
            BrushButton = (Button)GetTemplateChild(BrushButtonPart);
            EraseButton = (Button)GetTemplateChild(EraseButtonPart);
            AcceptButton = (Button)GetTemplateChild(AcceptButtonPart);

            InitEvents();

            base.OnApplyTemplate();
        }
        void InitEvents()
        {
            Canvas.Loaded += (s, e) =>
            {
                Canvas.PathBuilderHistory.UndoHappened += OnPathBuilderHistoryUndoRedoHappened;
                Canvas.PathBuilderHistory.RedoHappened += OnPathBuilderHistoryUndoRedoHappened;
            };

            Canvas.StrokesChanged += CanvasStrokesChanged;
            DrawSlider.StrokeChanged += DrawSliderStrokeChanged;
            AspectRatioSlider.ValueChanged += AspectRatioSliderValueChanged;

            CancelButton.Click += CancelButtonClick;
            DrawButton.Click += DrawButtonClick;
            CropButton.Click += CropButtonClick;

            UndoButton.Click += UndoButtonClick;



            RedoButton.Click += RedoButtonClick;


            BrushButton.Click += BrushButtonClick;



            EraseButton.Click += EraseButtonClick;

            AcceptButton.Click += AcceptButtonClick;



        }

        public async Task SetFileAsync(StorageFile file)
        {
            try
            {
                ResetUI();
                Cropper.Reset();
                Canvas.Reset();
                await Cropper.SetSourceAsync(file);
                Canvas.SetFile(file);

                Canvas.SetSource(Cropper._software);
                Canvas.SetRect(Cropper.CropRectangle);

                DisbaleCloseButton();
                CropMode(true);
                DrawMode(false);
            }
            catch { }
        }
        public async Task SaveToFileAsync(StorageFile file)
        {
            try
            {
                Canvas.SetRect(Cropper.CropRectangle);
                await Canvas.SaveAsync(file);
            }
            catch { }
        }

        #region Draw/Crop
        bool ShowHideDrawingMode(bool show)
        {
            if (DrawToolbar == null) return false;
            var visibility = DrawToolbar.Visibility == (show ? Visibility.Visible : Visibility.Collapsed);
            DrawToolbar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            return visibility;
        }
        void EnableToolbarUndo(bool show)
        {
            if (UndoButton != null)
                UndoButton.IsEnabled = show;
        }
        void EnableToolbarRedo(bool show)
        {
            if (RedoButton != null)
                RedoButton.IsEnabled = show;
        }
        public void ResetUI()
        {
            if (Cropper != null)
            {
                Cropper.IsCropEnabled = false;
                AspectRatio = 1.62d;
            }
            if (Canvas != null)
                Canvas.IsEnabled = false;
            ShowHideDrawingMode(false);
            ShowHideDefaultToolbar(true);

            Canvas?.PathBuilderHistory.Reset();
            EnableToolbarUndo(false);
            EnableToolbarRedo(false);
        }
        public bool ShowHideDefaultToolbar(bool show)
        {
            var visibility = DefaultToolbar.Visibility == Visibility.Visible;
            DefaultToolbar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            return visibility;
        }
        public void CropMode(bool show)
        {
            Cropper.IsCropEnabled = show;

            if (AspectRatioSlider != null)
                AspectRatioSlider.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
        public void DrawMode(bool show)
        {
            Canvas.IsEnabled = show;
            Canvas.Mode = CanvasMode.Stroke;
            if (DrawSlider != null)
            {
                DrawSlider.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                Canvas.Stroke = DrawSlider.Stroke;
                Canvas.StrokeThickness = DrawSlider.StrokeThickness;
            }
        }
        public void DisbaleCloseButton()
        {
            CancelButton.IsEnabled = DefaultToolbar.IsTapEnabled = false;
            ShowHideDefaultToolbar(false);
        }
        #endregion Draw/Crop

        #region Value changes

        private void OnPathBuilderHistoryUndoRedoHappened(object sender, UndoRedoEventArgs<SmoothPathBuilder> e)
        {

            EnableToolbarUndo(Canvas.PathBuilderHistory.CanUndo);
            EnableToolbarRedo(Canvas.PathBuilderHistory.CanRedo);
        }

        private void AspectRatioSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (AspectRatioSlider?.Value != -1)
                    AspectRatio = AspectRatioSlider.Value;
            }
            catch { }
        }
        private void CanvasStrokesChanged(object sender, EventArgs e)
        {

        }

        private void DrawSliderStrokeChanged(object sender, EventArgs e)
        {
            Canvas.Stroke = DrawSlider.Stroke;
            Canvas.StrokeThickness = DrawSlider.StrokeThickness;
        }

        private static void OnAspectRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (PhotoEditor)d;
            if (editor?.Cropper != null)
                editor.Cropper.AspectRatio = (double?)e.NewValue;
        }
        #endregion

        #region Methods

        #endregion
        #region Buttons
        bool IsEditingMode = false;
        private void DrawButtonClick(object sender, RoutedEventArgs e)
        {
            IsEditingMode = true;
            ShowHideDrawingMode(true);
            Canvas.Mode = CanvasMode.Stroke;
            CropMode(false);
            DrawMode(true);
        }
        private void CropButtonClick(object sender, RoutedEventArgs e)
        {
            IsEditingMode = true;
            DrawMode(false);
            CropMode(true);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            //if (ShowHideDrawingMode(false))
            //    return;
            //DrawMode(false);
            AcceptButtonClick(sender, e);
        }

        private void UndoButtonClick(object sender, RoutedEventArgs e)
        {
            Canvas.PathBuilderHistory.Undo();
            EnableToolbarUndo(Canvas.PathBuilderHistory.CanUndo);
            EnableToolbarRedo(Canvas.PathBuilderHistory.CanRedo);
        }

        private void RedoButtonClick(object sender, RoutedEventArgs e)
        {
            Canvas.PathBuilderHistory.Redo();
            EnableToolbarUndo(Canvas.PathBuilderHistory.CanUndo);
            EnableToolbarRedo(Canvas.PathBuilderHistory.CanRedo);
        }

        private void BrushButtonClick(object sender, RoutedEventArgs e)
        {
            Canvas.Mode = CanvasMode.Stroke;
        }

        private void EraseButtonClick(object sender, RoutedEventArgs e)
        {
            Canvas.Mode = CanvasMode.Eraser;
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            //DefaultToolbar  DrawToolbar   DrawSlider  AspectRatioSlider
            if (!CancelButton.IsEnabled ||IsEditingMode/* Cropper.IsEnabled*/)
            {
                CancelButton.IsEnabled = DefaultToolbar.IsTapEnabled = true;
                //Cropper.IsEnabled = false;
                ShowHideDefaultToolbar(true);
                ShowHideDrawingMode(false);
                DrawMode(false);
                CropMode(false);
                IsEditingMode = false;
                return;
            }
            var a = "";
        }
        #endregion Buttons

    }

}
