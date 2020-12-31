﻿// A copy from https://github.com/UnigramDev/Unigram/blob/develop/Unigram/Unigram/Controls/ImageCropper.cs

using PhotoEditorToolkit.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using static PhotoEditorToolkit.Helpers.Extensions;

namespace PhotoEditorToolkit.Controls
{
    public class CustomImageCropper : ContentControl
    {
        private readonly Dictionary<uint, Point> m_pointerPositions;

        private StorageFile m_imageSource;
        private Size m_imageSize;

        private Rect m_current;
        private Rect m_rectangle;

        private ImageCropperMask _mask = ImageCropperMask.Rectangle;

        private BitmapProportions _proportions = BitmapProportions.Custom;
        private BitmapRotation _rotation;
        private BitmapFlip _flip;

        private Grid m_layoutRoot;
        private Image m_imageViewer;

        private FrameworkElement m_imagePresenter;
        private CompositeTransform m_imagePresenterTransform;

        private Path m_clip;
        private RectangleGeometry m_outerClip;
        private Geometry m_innerClip;

        private Grid m_thumbsContainer;

        public CustomImageCropper()
        {
            DefaultStyleKey = typeof(CustomImageCropper);

            m_pointerPositions = new Dictionary<uint, Point>();

            m_current = new Rect(0, 0, 1, 1);
            m_rectangle = new Rect(0, 0, 1, 1);
        }
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_layoutRoot = (Grid)GetTemplateChild("LayoutRoot");
            m_imageViewer = (Image)GetTemplateChild("ImageViewer");
            m_imagePresenter = (FrameworkElement)GetTemplateChild("ImagePresenter");
            m_thumbsContainer = (Grid)GetTemplateChild("ThumbsContainer");

            m_clip = (Path)GetTemplateChild("Clip");

            if (m_clip != null)
            {
                SetMask(_mask);
            }

            m_imageViewer.SizeChanged += ImageViewer_SizeChanged;

            m_imagePresenter.RenderTransformOrigin = new Point(0.5, 0.5);
            m_imagePresenter.RenderTransform = m_imagePresenterTransform = new CompositeTransform();

            var leftThumb = (CustomImageCropperThumb)GetTemplateChild("LeftThumb");
            if (leftThumb != null)
            {
                leftThumb.PointerEntered += WEThumb_PointerEntered;
                leftThumb.PointerExited += Thumb_PointerExited;
                leftThumb.PointerPressed += Thumb_PointerPressed;
                leftThumb.PointerReleased += Thumb_PointerReleased;
                leftThumb.PointerMoved += LeftThumb_PointerMoved;
            }

            var topThumb = (CustomImageCropperThumb)GetTemplateChild("TopThumb");
            if (topThumb != null)
            {
                topThumb.PointerEntered += NSThumb_PointerEntered;
                topThumb.PointerExited += Thumb_PointerExited;
                topThumb.PointerPressed += Thumb_PointerPressed;
                topThumb.PointerReleased += Thumb_PointerReleased;
                topThumb.PointerMoved += TopThumb_PointerMoved;
            }

            var rightThumb = (CustomImageCropperThumb)GetTemplateChild("RightThumb");
            if (rightThumb != null)
            {
                rightThumb.PointerEntered += WEThumb_PointerEntered;
                rightThumb.PointerExited += Thumb_PointerExited;
                rightThumb.PointerPressed += Thumb_PointerPressed;
                rightThumb.PointerReleased += Thumb_PointerReleased;
                rightThumb.PointerMoved += RightThumb_PointerMoved;
            }

            var bottomThumb = (CustomImageCropperThumb)GetTemplateChild("BottomThumb");
            if (bottomThumb != null)
            {
                bottomThumb.PointerEntered += NSThumb_PointerEntered;
                bottomThumb.PointerExited += Thumb_PointerExited;
                bottomThumb.PointerPressed += Thumb_PointerPressed;
                bottomThumb.PointerReleased += Thumb_PointerReleased;
                bottomThumb.PointerMoved += BottomThumb_PointerMoved;
            }

            var topLeftThumb = (CustomImageCropperThumb)GetTemplateChild("TopLeftThumb");
            if (topLeftThumb != null)
            {
                topLeftThumb.PointerEntered += NWSEThumb_PointerEntered;
                topLeftThumb.PointerExited += Thumb_PointerExited;
                topLeftThumb.PointerPressed += Thumb_PointerPressed;
                topLeftThumb.PointerReleased += Thumb_PointerReleased;
                topLeftThumb.PointerMoved += TopLeftThumb_PointerMoved;
            }

            var topRightThumb = (CustomImageCropperThumb)GetTemplateChild("TopRightThumb");
            if (topLeftThumb != null)
            {
                topRightThumb.PointerEntered += NESWThumb_PointerEntered;
                topRightThumb.PointerExited += Thumb_PointerExited;
                topRightThumb.PointerPressed += Thumb_PointerPressed;
                topRightThumb.PointerReleased += Thumb_PointerReleased;
                topRightThumb.PointerMoved += TopRightThumb_PointerMoved;
            }

            var bottomLeftThumb = (CustomImageCropperThumb)GetTemplateChild("BottomLeftThumb");
            if (bottomLeftThumb != null)
            {
                bottomLeftThumb.PointerEntered += NESWThumb_PointerEntered;
                bottomLeftThumb.PointerExited += Thumb_PointerExited;
                bottomLeftThumb.PointerPressed += Thumb_PointerPressed;
                bottomLeftThumb.PointerReleased += Thumb_PointerReleased;
                bottomLeftThumb.PointerMoved += BottomLeftThumb_PointerMoved;
            }

            var bottomRightThumb = (CustomImageCropperThumb)GetTemplateChild("BottomRightThumb");
            if (bottomRightThumb != null)
            {
                bottomRightThumb.PointerEntered += NWSEThumb_PointerEntered;
                bottomRightThumb.PointerExited += Thumb_PointerExited;
                bottomRightThumb.PointerPressed += Thumb_PointerPressed;
                bottomRightThumb.PointerReleased += Thumb_PointerReleased;
                bottomRightThumb.PointerMoved += BottomRightThumb_PointerMoved;
            }

            var middleThumb = (Rectangle)GetTemplateChild("MiddleThumb");
            if (middleThumb != null)
            {
                middleThumb.PointerPressed += Thumb_PointerPressed;
                middleThumb.PointerReleased += Thumb_PointerReleased;
                middleThumb.PointerMoved += MiddleThumb_PointerMoved;
            }
        }

        private void UpdateTutteCose(Rect rect, bool animate = false)
        {
            m_current = rect;

            if (m_thumbsContainer == null)
            {
                return;
            }

            var w = m_layoutRoot.ActualWidth;
            var h = m_layoutRoot.ActualHeight;

            if (w == 0 || h == 0)
            {
                return;
            }

            m_thumbsContainer.Margin = new Thickness(
                rect.Left * w,
                rect.Top * h,
                w - rect.Right * w,
                h - rect.Bottom * h);
            m_outerClip.Rect = new Rect(0, 0, m_layoutRoot.ActualWidth, m_layoutRoot.ActualHeight);

            switch (m_innerClip)
            {
                case RectangleGeometry rectangle:
                    rectangle.Rect = new Rect(rect.Left * w, rect.Top * h, rect.Width * w, rect.Height * h);
                    break;
                case EllipseGeometry ellipse:
                    ellipse.Center = new Point((rect.Left + rect.Width / 2) * w, (rect.Top + rect.Height / 2) * h);
                    ellipse.RadiusX = rect.Width / 2 * w;
                    ellipse.RadiusY = rect.Height / 2 * h;
                    break;
            }
        }

        public void Reset()
        {
            m_current = new Rect(0, 0, 1, 1);
            m_rectangle = new Rect(0, 0, 1, 1);
        }
        #region Pointer events

        private void Thumb_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        private void Thumb_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ((UIElement)sender).CapturePointer(e.Pointer);

            var pointer = e.GetCurrentPoint(m_layoutRoot);
            m_pointerPositions[pointer.PointerId] = pointer.Position;

            m_current = m_rectangle;

            e.Handled = true;
        }

        private void Thumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            m_pointerPositions.Remove(e.Pointer.PointerId);

            ((UIElement)sender).ReleasePointerCapture(e.Pointer);

            m_rectangle = m_current;

            e.Handled = true;
        }

        private void MiddleThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetX = (position.X - startPosition.X) / w;
                var offsetY = (position.Y - startPosition.Y) / h;

                var left = Clamp(m_rectangle.Left + offsetX, 0, 1 - m_rectangle.Width);
                var top = Clamp(m_rectangle.Top + offsetY, 0, 1 - m_rectangle.Height);
                var width = m_rectangle.Right - left;
                var height = m_rectangle.Bottom - top;

                var cropScale = (width * w) / (height * h);
                var proportionalCropScale = GetProportionsFactor(BitmapProportions.Custom, cropScale);

                if (cropScale < proportionalCropScale)
                {
                    var cropHeight = ((width * w) / proportionalCropScale) / h;

                    m_current.X = left;
                    m_current.Y = top + (height - cropHeight);
                }
                else
                {
                    var cropWidth = ((height * h) * proportionalCropScale) / w;

                    m_current.X = left + (width - cropWidth);
                    m_current.Y = top;
                }

                UpdateTutteCose(m_current);
            }
        }

        private void TopLeftThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetX = (position.X - startPosition.X) / w;
                var offsetY = (position.Y - startPosition.Y) / h;

                var left = Clamp(m_rectangle.Left + offsetX, 0, m_rectangle.Right);
                var top = Clamp(m_rectangle.Top + offsetY, 0, m_rectangle.Bottom);
                var width = m_rectangle.Right - left;
                var height = m_rectangle.Bottom - top;

                var cropScale = (width * w) / (height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                if (cropScale < proportionalCropScale)
                {
                    var cropHeight = ((width * w) / proportionalCropScale) / h;

                    m_current.X = left;
                    m_current.Y = top + (height - cropHeight);
                    m_current.Width = width;
                    m_current.Height = cropHeight;
                }
                else
                {
                    var cropWidth = ((height * h) * proportionalCropScale) / w;

                    m_current.X = left + (width - cropWidth);
                    m_current.Y = top;
                    m_current.Width = cropWidth;
                    m_current.Height = height;
                }

                UpdateTutteCose(m_current);
            }
        }

        private void TopThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetY = (position.Y - startPosition.Y) / h;

                var top = Clamp(m_rectangle.Top + offsetY, 0, m_rectangle.Bottom);
                var height = m_rectangle.Bottom - top;

                var cropScale = (m_current.Width * w) / (height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                var cropWidth = Math.Min(w, (height * h) * proportionalCropScale) / w;

                m_current.Y = top;
                m_current.X = Clamp(m_current.X + (m_current.Width - cropWidth) / 2.0, 0, 1 - cropWidth);
                m_current.Width = cropWidth;
                m_current.Height = ((cropWidth * w) / proportionalCropScale) / h;

                UpdateTutteCose(m_current);
            }
        }

        private void LeftThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetX = (position.X - startPosition.X) / w;

                var left = Clamp(m_rectangle.Left + offsetX, 0, m_rectangle.Right);
                var width = m_rectangle.Right - left;

                var cropScale = (width * w) / (m_current.Height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                var cropHeight = Math.Min(h, (width * w) / proportionalCropScale) / h;

                m_current.Y = Clamp(m_current.Y + (m_current.Height - cropHeight) / 2.0, 0, 1 - cropHeight);
                m_current.X = left;
                m_current.Width = ((cropHeight * h) * proportionalCropScale) / w;
                m_current.Height = cropHeight;

                UpdateTutteCose(m_current);
            }
        }

        private void BottomRightThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetX = (position.X - startPosition.X) / w;
                var offsetY = (position.Y - startPosition.Y) / h;

                var right = Clamp(m_rectangle.Right + offsetX, m_current.Left, 1);
                var bottom = Clamp(m_rectangle.Bottom + offsetY, m_current.Top, 1);
                var width = right - m_rectangle.Left;
                var height = bottom - m_rectangle.Top;

                var cropScale = (width * w) / (height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                if (cropScale < proportionalCropScale)
                {
                    var cropHeight = ((width * w) / proportionalCropScale) / h;

                    m_current.Width = width;
                    m_current.Height = cropHeight;
                }
                else
                {
                    var cropWidth = ((height * h) * proportionalCropScale) / w;

                    m_current.Width = cropWidth;
                    m_current.Height = height;
                }

                UpdateTutteCose(m_current);
                e.Handled = true;
            }
        }

        private void BottomThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetY = (position.Y - startPosition.Y) / h;

                var bottom = Clamp(m_rectangle.Bottom + offsetY, m_current.Top, 1);
                var height = bottom - m_rectangle.Top;

                var cropScale = (m_current.Width * w) / (height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                var cropWidth = Math.Min(w, (height * h) * proportionalCropScale) / w;

                m_current.X = Clamp(m_current.X + (m_current.Width - cropWidth) / 2.0, 0, 1 - cropWidth);
                m_current.Width = cropWidth;
                m_current.Height = ((cropWidth * w) / proportionalCropScale) / h;

                UpdateTutteCose(m_current);
                e.Handled = true;
            }
        }

        private void RightThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetX = (position.X - startPosition.X) / w;

                var right = Clamp(m_rectangle.Right + offsetX, m_current.Left, 1);
                var width = right - m_rectangle.Left;

                var cropScale = (width * w) / (m_current.Height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                var cropHeight = Math.Min(h, (width * w) / proportionalCropScale) / h;

                m_current.Y = Clamp(m_current.Y + (m_current.Height - cropHeight) / 2.0, 0, 1 - cropHeight);
                m_current.Width = ((cropHeight * h) * proportionalCropScale) / w;
                m_current.Height = cropHeight;

                UpdateTutteCose(m_current);
                e.Handled = true;
            }
        }

        private void BottomLeftThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetX = (position.X - startPosition.X) / w;
                var offsetY = (position.Y - startPosition.Y) / h;

                var left = Clamp(m_rectangle.Left + offsetX, 0, m_rectangle.Right);
                var bottom = Clamp(m_rectangle.Bottom + offsetY, m_current.Top, 1);
                var width = m_rectangle.Right - left;
                var height = bottom - m_rectangle.Top;

                var cropScale = (width * w) / (height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                if (cropScale < proportionalCropScale)
                {
                    var cropHeight = ((width * w) / proportionalCropScale) / h;

                    m_current.X = left;
                    m_current.Width = width;
                    m_current.Height = cropHeight;
                }
                else
                {
                    var cropWidth = ((height * h) * proportionalCropScale) / w;

                    m_current.X = left + (width - cropWidth);
                    m_current.Width = cropWidth;
                    m_current.Height = height;
                }

                UpdateTutteCose(m_current);
            }
        }

        private void TopRightThumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.IsInContact && m_pointerPositions.TryGetValue(e.Pointer.PointerId, out Point startPosition))
            {
                var w = m_layoutRoot.ActualWidth;
                var h = m_layoutRoot.ActualHeight;

                var position = e.GetCurrentPoint(m_layoutRoot).Position;
                var offsetX = (position.X - startPosition.X) / w;
                var offsetY = (position.Y - startPosition.Y) / h;

                var right = Clamp(m_rectangle.Right + offsetX, m_current.Left, 1);
                var top = Clamp(m_rectangle.Top + offsetY, 0, m_rectangle.Bottom);
                var width = right - m_rectangle.Left;
                var height = m_rectangle.Bottom - top;

                var cropScale = (width * w) / (height * h);
                var proportionalCropScale = GetProportionsFactor(_proportions, cropScale);

                if (cropScale < proportionalCropScale)
                {
                    var cropHeight = ((width * w) / proportionalCropScale) / h;

                    m_current.Y = top + (height - cropHeight);
                    m_current.Width = width;
                    m_current.Height = cropHeight;
                }
                else
                {
                    var cropWidth = ((height * h) * proportionalCropScale) / w;

                    m_current.Y = top;
                    m_current.Width = cropWidth;
                    m_current.Height = height;
                }

                UpdateTutteCose(m_current);
            }
        }

        private void NWSEThumb_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 1);
        }

        private void NESWThumb_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNortheastSouthwest, 1);
        }

        private void WEThumb_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 1);
        }

        private void NSThumb_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthSouth, 1);
        }

        #endregion

        #region Rectangle

        public void SetRectangle(Rect rectangle, bool animate = true)
        {
            if (rectangle.X < 0)
            {
                rectangle.X = 0;
            }
            if (rectangle.Y < 0)
            {
                rectangle.Y = 0;
            }
            if (rectangle.Right > 1)
            {
                rectangle.Width = 1 - rectangle.X;
            }
            if (rectangle.Bottom > 1)
            {
                rectangle.Height = 1 - rectangle.Y;
            }

            m_current = rectangle;
            m_rectangle = rectangle;

            UpdateTutteCose(rectangle, animate);
        }

        #endregion

        #region Proportions

        public void SetProportions(BitmapProportions value, bool animate = true)
        {
            _proportions = value;

            var cropScale = (m_rectangle.Width * m_imageSize.Width) / (m_rectangle.Height * m_imageSize.Height);
            var proportionalCropScale = GetProportionsFactor(value, cropScale);

            if (cropScale < proportionalCropScale)
            {
                var cropHeight = ((m_rectangle.Width * m_imageSize.Width) / proportionalCropScale) / m_imageSize.Height;

                m_rectangle.Y = Clamp(m_rectangle.Y + (m_rectangle.Height - cropHeight) / 2.0, 0.0, m_imageSize.Height - cropHeight);
                m_rectangle.Height = cropHeight;
            }
            else
            {
                var cropWidth = ((m_rectangle.Height * m_imageSize.Height) * proportionalCropScale) / m_imageSize.Width;

                m_rectangle.X = Clamp(m_rectangle.X + (m_rectangle.Width - cropWidth) / 2.0, 0.0, m_imageSize.Width - cropWidth);
                m_rectangle.Width = cropWidth;
            }

            UpdateTutteCose(m_rectangle, animate);
        }

        private double GetProportionsFactor(BitmapProportions proportions, double defaultValue)
        {
            switch (proportions)
            {
                case BitmapProportions.Original:
                    return m_imageSize.Width / m_imageSize.Height;
                case BitmapProportions.Square:
                    return 1.0;
                // Portrait
                case BitmapProportions.TwoOverThree:
                    return 2.0 / 3.0;
                case BitmapProportions.ThreeOverFive:
                    return 3.0 / 5.0;
                case BitmapProportions.ThreeOverFour:
                    return 3.0 / 4.0;
                case BitmapProportions.FourOverFive:
                    return 4.0 / 5.0;
                case BitmapProportions.FiveOverSeven:
                    return 5.0 / 7.0;
                case BitmapProportions.NineOverSixteen:
                    return 9.0 / 16.0;
                // Landscape
                case BitmapProportions.ThreeOverTwo:
                    return 3.0 / 2.0;
                case BitmapProportions.FiveOverThree:
                    return 5.0 / 3.0;
                case BitmapProportions.FourOverThree:
                    return 4.0 / 3.0;
                case BitmapProportions.FiveOverFour:
                    return 5.0 / 4.0;
                case BitmapProportions.SevenOverFive:
                    return 7.0 / 5.0;
                case BitmapProportions.SixteenOverNine:
                    return 16.0 / 9.0;
                default:
                    return defaultValue;
            }
        }

        public IReadOnlyList<BitmapProportions> GetProportions()
        {
            return GetProportionsFor(m_imageSize.Width, m_imageSize.Height);
        }

        public static IReadOnlyList<BitmapProportions> GetProportionsFor(double width, double height)
        {
            var items = new List<BitmapProportions>
            {
                BitmapProportions.Original,
                BitmapProportions.Square
            };

            if (width > height)
            {
                items.Add(BitmapProportions.ThreeOverTwo);
                items.Add(BitmapProportions.FiveOverThree);
                items.Add(BitmapProportions.FourOverThree);
                items.Add(BitmapProportions.FiveOverFour);
                items.Add(BitmapProportions.SevenOverFive);
                items.Add(BitmapProportions.SixteenOverNine);
            }
            else
            {
                items.Add(BitmapProportions.TwoOverThree);
                items.Add(BitmapProportions.ThreeOverFive);
                items.Add(BitmapProportions.ThreeOverFour);
                items.Add(BitmapProportions.FourOverFive);
                items.Add(BitmapProportions.FiveOverSeven);
                items.Add(BitmapProportions.NineOverSixteen);
            }

            return items;
        }

        #endregion

        #region Mask

        public void SetMask(ImageCropperMask mask)
        {
            _mask = mask;

            var clip = m_clip;
            if (clip == null)
            {
                return;
            }

            var group = new GeometryGroup();
            group.Children.Add(m_outerClip = new RectangleGeometry());
            group.Children.Add(m_innerClip = mask == ImageCropperMask.Rectangle
                ? (Geometry)new RectangleGeometry()
                : new EllipseGeometry());

            clip.Data = group;
            UpdateTutteCose(m_rectangle);
        }

        #endregion

        #region Properties

        public int PixelWidth => (int)m_imageSize.Width;
        public int PixelHeight => (int)m_imageSize.Height;

        public BitmapProportions Proportions => _proportions;

        public Rect CropRectangle => m_rectangle;

        private bool _isCropEnabled = true;
        public bool IsCropEnabled
        {
            get => _isCropEnabled;
            set
            {
                _isCropEnabled = value;
                VisualStateManager.GoToState(this, value ? "Normal" : "Disabled", true);
            }
        }

        public ImageCropperMask Mask => _mask;

        #endregion

        #region Source

        public SoftwareBitmap _software;
        public async Task SetSourceAsync(StorageFile file, BitmapRotation rotation = BitmapRotation.None, BitmapFlip flip = BitmapFlip.None, BitmapProportions proportions = BitmapProportions.Custom, Rect? cropRectangle = null)
        {
            _rotation = rotation;
            _flip = flip;

            SoftwareBitmapSource _source;
            using (var fileStream = await ImageHelper.OpenReadAsync(file))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);
                var transform = ImageHelper.ComputeScalingTransformForSourceImage(decoder);

                transform.Rotation = rotation;
                transform.Flip = flip;

                _software = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
                _source = new SoftwareBitmapSource();
                await _source.SetBitmapAsync(_software);

                SetSource(file, _source, _software.PixelWidth, _software.PixelHeight, proportions, cropRectangle);
            }

            UpdatePresenterTransform();
        }

        public void SetSource(StorageFile file, ImageSource source, double width, double height, BitmapProportions proportions = BitmapProportions.Custom, Rect? cropRectangle = null)
        {
            m_imageViewer.Source = source;

            m_imageSource = file;
            m_imageSize = new Size(width, height);

            if (cropRectangle is Rect rectangle)
            {
                SetProportions(proportions, false);
                SetRectangle(rectangle, false);
            }
            else
            {
                SetRectangle(new Rect(0, 0, 1, 1), false);
                SetProportions(proportions, false);
            }
        }

        public async Task<StorageFile> CropAsync(int min = 1280, int max = 0)
        {
            return await ImageHelper.CropAsync(m_imageSource, null, m_rectangle, min, max, rotation: _rotation, flip: _flip);
        }

        #endregion

        #region Content

        private void ImageViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var horizontal = -e.NewSize.Height;
            var vertical = -e.NewSize.Width;

            switch (_rotation)
            {
                case BitmapRotation.None:
                case BitmapRotation.Clockwise180Degrees:
                    m_imagePresenter.Margin = new Thickness(horizontal, vertical, horizontal, vertical);
                    m_imagePresenter.Width = e.NewSize.Width;
                    m_imagePresenter.Height = e.NewSize.Height;
                    m_imagePresenterTransform.Rotation = _rotation == BitmapRotation.None
                        ? 0
                        : 180;
                    break;
                case BitmapRotation.Clockwise90Degrees:
                case BitmapRotation.Clockwise270Degrees:
                    m_imagePresenter.Margin = new Thickness(horizontal, vertical, horizontal, vertical);
                    m_imagePresenter.Width = e.NewSize.Height;
                    m_imagePresenter.Height = e.NewSize.Width;
                    m_imagePresenterTransform.Rotation = _rotation == BitmapRotation.Clockwise90Degrees
                        ? 90
                        : 270;
                    break;
            }

            UpdatePresenterTransform();
            UpdateTutteCose(m_rectangle);
        }

        private void UpdatePresenterTransform()
        {
            m_imagePresenterTransform.ScaleX = _flip == BitmapFlip.Horizontal
                ? -1
                : 1;
            m_imagePresenterTransform.ScaleY = _flip == BitmapFlip.Vertical
                ? -1
                : 1;
        }

        #endregion
    }


    public enum BitmapProportions
    {
        Custom,
        Original,
        Square,
        TwoOverThree,
        ThreeOverFive,
        ThreeOverFour,
        FourOverFive,
        FiveOverSeven,
        NineOverSixteen,
        ThreeOverTwo,
        FiveOverThree,
        FourOverThree,
        FiveOverFour,
        SevenOverFive,
        SixteenOverNine,
    }

    public enum ImageCropperMask
    {
        Rectangle,
        Ellipse
    }

    public sealed class CustomImageCropperThumb : Control
    {
        #region constructors
        public CustomImageCropperThumb()
        {
            DefaultStyleKey = typeof(CustomImageCropperThumb);
        }
        #endregion
    }
}
