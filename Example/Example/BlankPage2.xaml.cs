using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Example
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage2 : Page
    {
        public BlankPage2()
        {
            this.InitializeComponent();
        }
        private async void AccepscdtButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fo = new FileOpenPicker();
            fo.FileTypeFilter.Add(".png");
            fo.FileTypeFilter.Add(".jpg");
            fo.SuggestedStartLocation = PickerLocationId.Desktop;

            var file = await fo.PickSingleFileAsync();
            if (file != null)
                await Editor.SetFileAsync(file);
        }

        private async void AccepscdtBussstton_Click(object sender, RoutedEventArgs e)
        {

            FileSavePicker picker = new FileSavePicker();
            //picker.FileTypeChoices.Add("PNG File", new string[] { ".png" });
            picker.FileTypeChoices.Add("JPG File", new string[] { ".jpeg", ".jpg" });
            var file = await picker.PickSaveFileAsync();
            if (file != null)
                await Editor.SaveToFileAsync(file);
        }

    }
}
