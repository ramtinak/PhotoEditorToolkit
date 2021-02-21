using PhotoEditorToolkit;
using PhotoEditorToolkit.Controls;
using PhotoEditorToolkit.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace Example
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Cropper.SetMask(ImageCropperMask.Rectangle);
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DrawSlider_StrokeChanged(object sender, EventArgs e)
        {
            Canvas.Stroke = DrawSlider.Stroke;
            Canvas.StrokeThickness = DrawSlider.StrokeThickness;
            if (Canvas.Mode != CanvasMode.Stroke)
                Canvas.Mode = CanvasMode.Stroke;
        }

        private void Canvas_StrokesChanged(object sender, EventArgs e)
        {

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            FileOpenPicker fo = new FileOpenPicker();
            fo.FileTypeFilter.Add(".png");
            fo.FileTypeFilter.Add(".jpg");
            fo.SuggestedStartLocation = PickerLocationId.Desktop;

            var file = await fo.PickSingleFileAsync();
            if (file != null)
            {
                Cropper.Reset();
                Canvas.Reset();
                await Cropper.SetSourceAsync(file);
                 Canvas.SetFile(file);

                Canvas.SetSource(Cropper._software);
                Canvas.SetRect(Cropper.CropRectangle);
                //if (!_hasMessageContext) //TODO: Fix mask bug, remove hack (#2017 references mask position)
                //{
                //    await Cropper.SetSourceAsync(_media.File, _media.EditState.Rotation, _media.EditState.Flip, _media.EditState.Proportions, _media.EditState.Rectangle);
                //    Cropper.SetProportions(_media.EditState.Proportions);
                //}
                //Cropper.IsCropEnabled = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ResetUI();

            Canvas.IsEnabled = true;
            if (DrawSlider == null)
                FindName(nameof(DrawSlider));

            DrawSlider.Visibility = Visibility.Visible;
            Canvas.Mode = CanvasMode.Stroke;
            Canvas.Stroke = DrawSlider.Stroke;
            Canvas.StrokeThickness = DrawSlider.StrokeThickness;
            DrawSlider.SetDefault(new Vector2(1f, 0.22f));
            InvalidateToolbar();
        }
        void ResetUI()
        {
            Cropper.IsCropEnabled = false;
            Canvas.IsEnabled = false;

        }
        private void InvalidateToolbar()
        {
            Debug.WriteLine("CanUndo " + Canvas.PathBuilderHistory.CanUndo);
            Debug.WriteLine("CanRedo " + Canvas.PathBuilderHistory.CanRedo);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Canvas.PathBuilderHistory.Undo();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Canvas.PathBuilderHistory.Redo();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Index = (Index +1) % Canvas.Filters.Count;
            Canvas.ApplyFilter(Canvas.Filters[Index]);
        }
        int Index = 0;
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Canvas.ApplyFilter(new Filter0());
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Canvas.RemoveImage();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Cropper.IsCropEnabled = true;
            Canvas.IsEnabled = false;

        }

        private async void Button_Click_8(object sender, RoutedEventArgs e)
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

        private void DrawSlider_StrokeChanged_1(object sender, EventArgs e)
        {

        }
    }
}
