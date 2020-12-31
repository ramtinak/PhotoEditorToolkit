using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;

namespace PhotoEditorToolkit.Helpers
{
    internal static class Extensions
    {
        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        public static uint GetHeight(this ImageProperties props) => props.Height;

        public static uint GetWidth(this ImageProperties props) => props.Width;

        public static uint GetHeight(this VideoProperties props)
        {
            return props.Orientation == VideoOrientation.Rotate180 || props.Orientation == VideoOrientation.Normal ? props.Height : props.Width;
        }

        public static uint GetWidth(this VideoProperties props)
        {
            return props.Orientation == VideoOrientation.Rotate180 || props.Orientation == VideoOrientation.Normal ? props.Width : props.Height;
        }
    }
}
