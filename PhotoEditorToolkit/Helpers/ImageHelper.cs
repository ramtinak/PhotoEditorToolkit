using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace PhotoEditorToolkit.Helpers
{
    static class ImageHelper
    {
        public static async Task<StorageFile> CropAsync(StorageFile sourceFile, StorageFile file, Rect cropRectangle, int min = 1280, int max = 0, double quality = 0.77, BitmapRotation rotation = BitmapRotation.None, BitmapFlip flip = BitmapFlip.None)
        {
            if (file == null)
                file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("crop.jpg", CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await OpenReadAsync(sourceFile))
            using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var cropWidth = (double)decoder.PixelWidth;
                var cropHeight = (double)decoder.PixelHeight;

                if (decoder.PixelWidth > 1280 || decoder.PixelHeight > 1280)
                {
                    double ratioX = 1280 / cropWidth;
                    double ratioY = 1280 / cropHeight;
                    double ratio = Math.Min(ratioX, ratioY);

                    cropWidth *= ratio;
                    cropHeight *= ratio;
                }

                cropRectangle = new Rect(
                    cropRectangle.X * decoder.PixelWidth,
                    cropRectangle.Y * decoder.PixelHeight,
                    cropRectangle.Width * decoder.PixelWidth,
                    cropRectangle.Height * decoder.PixelHeight);

                var tupple = Scale(cropRectangle, new Size(decoder.PixelWidth, decoder.PixelHeight), new Size(cropWidth, cropHeight), min, max);
                var scaledCrop = tupple.Item1;
                var scaledSize = tupple.Item2;

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
                    Rotation = rotation,
                    Flip = flip
                };

                var pixelData = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);

                var propertySet = new BitmapPropertySet();
                var qualityValue = new BitmapTypedValue(quality, PropertyType.Single);
                propertySet.Add("ImageQuality", qualityValue);

                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);
                encoder.SetSoftwareBitmap(pixelData);
                await encoder.FlushAsync();
            }

            return file;
        }
        internal static Tuple<Rect, Size> Scale(Rect rect, Size start, Size size, int min, int max)
        {
            var width = rect.Width * size.Width / start.Width;
            var height = rect.Height * size.Height / start.Height;

            if (width > min || height > min)
            {
                double ratioX = min / width;
                double ratioY = min / height;
                double ratio = Math.Min(ratioX, ratioY);

                width *= ratio;
                height *= ratio;
            }

            if (width < max || height < max)
            {
                double ratioX = max / width;
                double ratioY = max / height;
                double ratio = Math.Min(ratioX, ratioY);

                width *= ratio;
                height *= ratio;
            }

            var ratioW = start.Width * width / rect.Width;
            var ratioH = start.Height * height / rect.Height;

            var x = rect.X * ratioW / start.Width;
            var y = rect.Y * ratioH / start.Height;
            var w = rect.Width * ratioW / start.Width;
            var h = rect.Height * ratioH / start.Height;

            return new Tuple<Rect, Size>(new Rect(x, y, w, h), new Size(ratioW, ratioH));
        }
        public static async Task<IRandomAccessStream> OpenReadAsync(StorageFile sourceFile)
        {
            if (sourceFile.ContentType.Equals("video/mp4"))
            {
                var props = await sourceFile.Properties.GetVideoPropertiesAsync();
                var composition = new MediaComposition();
                var clip = await MediaClip.CreateFromFileAsync(sourceFile);
                composition.Clips.Add(clip);
                return await composition.GetThumbnailAsync(TimeSpan.Zero, (int)props.GetWidth(), (int)props.GetHeight(), VideoFramePrecision.NearestKeyFrame);
            }

            return await sourceFile.OpenReadAsync();
        }
        public static BitmapTransform ComputeScalingTransformForSourceImage(BitmapDecoder sourceDecoder)
        {
            var transform = new BitmapTransform();

            if (sourceDecoder.PixelWidth > 1280 || sourceDecoder.PixelHeight > 1280)
            {
                double ratioX = (double)1280 / sourceDecoder.PixelWidth;
                double ratioY = (double)1280 / sourceDecoder.PixelHeight;
                double ratio = Math.Min(ratioX, ratioY);

                transform.ScaledWidth = (uint)(sourceDecoder.PixelWidth * ratio);
                transform.ScaledHeight = (uint)(sourceDecoder.PixelHeight * ratio);
                transform.InterpolationMode = BitmapInterpolationMode.Linear;
            }

            return transform;
        }
    }
}
