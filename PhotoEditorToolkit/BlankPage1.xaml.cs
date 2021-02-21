using PhotoEditorToolkit.Classes;
using PhotoEditorToolkit.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PhotoEditorToolkit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {

        public BlankPage1()
        {
            this.InitializeComponent();
        }
        private void CanvasLoaded(object sender, RoutedEventArgs e)
        {

            Canvas.PathBuilderHistory.UndoHappened += OnPathBuilderHistoryUndoRedoHappened;
            Canvas.PathBuilderHistory.RedoHappened += OnPathBuilderHistoryUndoRedoHappened;
        }
        private void OnPathBuilderHistoryUndoRedoHappened(object sender, UndoRedoEventArgs<SmoothPathBuilder> e)
        {

            EnableToolbarUndo(Canvas.PathBuilderHistory.CanUndo);
            EnableToolbarRedo(Canvas.PathBuilderHistory.CanRedo);
            //try
            //{
            //}
            //catch { }
        }

        private void MySliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {

        }

        private void Canvas_StrokesChanged(object sender, EventArgs e)
        {

        }

        private void DrawSlider_StrokeChanged(object sender, EventArgs e)
        {
            Canvas.Stroke = DrawSlider.Stroke;
            Canvas.StrokeThickness = DrawSlider.StrokeThickness;
        }

        private void DrawButtonClick(object sender, RoutedEventArgs e)
        {
            FindName("DrawToolbar");
            FindName("DrawSlider");
            ShowHideDrawingMode(true);
            Canvas.Mode = CanvasMode.Stroke;
            CropMode(false, false);

            DrawMode(true);
        }
        #region Draw/Crop
        bool ShowHideDrawingMode(bool show, bool setDefaultAsWell =true)
        {
            if (DrawToolbar == null) return false;
            var visibility = DrawToolbar.Visibility == (show ? Visibility.Visible : Visibility.Collapsed);
            DrawToolbar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            if(setDefaultAsWell)
            ShowHideDefaultToolbar(!show);
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
                Cropper.AspectRatio = 1.62d;
            }
            if (Canvas != null)
                Canvas.IsEnabled = false;
            ShowHideDrawingMode(false, false);
            ShowHideDefaultToolbar(true);

            Canvas.PathBuilderHistory.Reset();
            EnableToolbarUndo(false);
            EnableToolbarRedo(false);
            //UndoButton.Background = RedoButton.Background = new SolidColorBrush(Colors.Transparent);
        }
        public bool ShowHideDefaultToolbar(bool show)
        {
            var visibility = DefaultToolbar.Visibility == Visibility.Visible;
            DefaultToolbar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            return visibility;
        }
        public void CropMode(bool show, bool canvasAsWell = true)
        {
            Cropper.IsCropEnabled = show;
            if (canvasAsWell)
                Canvas.IsEnabled = !show;

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
        #endregion Draw/Crop

        private async void AccepscdtButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fo = new FileOpenPicker();
            fo.FileTypeFilter.Add(".png");
            fo.FileTypeFilter.Add(".jpg");
            fo.SuggestedStartLocation = PickerLocationId.Desktop;

            var file = await fo.PickSingleFileAsync();
            if (file != null)
            {
                ResetUI();
                Cropper.Reset();
                Canvas.Reset();
                await Cropper.SetSourceAsync(file);
                Canvas.SetFile(file);

                Canvas.SetSource(Cropper._software);
                Canvas.SetRect(Cropper.CropRectangle);

                DisbaleCloseButton();
                CropMode(true, false);
            }
        }

        private async void AccepscdtBussstton_Click(object sender, RoutedEventArgs e)
        {

            FileSavePicker picker = new FileSavePicker();
            //picker.FileTypeChoices.Add("PNG File", new string[] { ".png" });
            picker.FileTypeChoices.Add("JPG File", new string[] { ".jpeg", ".jpg" });
            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                Canvas.SetRect(Cropper.CropRectangle);
                await Canvas.SaveAsync(file);
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (ShowHideDrawingMode(false))
            {
                //ShowHideDefaultToolbar(true);
                return;
            }
            DrawMode(false);
        }

        private void CropButtonClick(object sender, RoutedEventArgs e)
        {

            DrawMode(false);
            CropMode(true);
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
        private void AspectRatioSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (AspectRatioSlider?.Value != -1)
                    Cropper.AspectRatio = AspectRatioSlider.Value;
            }
            catch { }
        }
        public void DisbaleCloseButton()
        {
            CancelButton.IsEnabled = DefaultToolbar.IsTapEnabled = false;
            ShowHideDefaultToolbar(false);
        }

        private void AccepscdtBuxxxssstton_Click(object sender, RoutedEventArgs e)
        {
            DisbaleCloseButton();
        }

        private void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            if (!CancelButton.IsEnabled || Cropper.IsEnabled)
            {
                CancelButton.IsEnabled = DefaultToolbar.IsTapEnabled = true;

                ShowHideDefaultToolbar(true);
                DrawMode(true);
                CropMode(false, false);
                return;
            }
        }
    }
}
