using System;
using Windows.UI;

namespace PhotoEditorToolkit.Helpers
{
    public struct RGB
    {
        private byte _r;
        private byte _g;
        private byte _b;

        public RGB(byte r, byte g, byte b)
        {
            _r = r;
            _g = g;
            _b = b;
        }

        public byte R
        {
            get { return _r; }
            set { _r = value; }
        }

        public byte G
        {
            get { return _g; }
            set { _g = value; }
        }

        public byte B
        {
            get { return _b; }
            set { _b = value; }
        }

        public bool Equals(RGB rgb)
        {
            return (R == rgb.R) && (G == rgb.G) && (B == rgb.B);
        }

        public static implicit operator Color(RGB rhs)
        {
            return Color.FromArgb(255, rhs.R, rhs.G, rhs.B);
        }

        public static implicit operator RGB(Color lhs)
        {
            return new RGB(lhs.R, lhs.G, lhs.B);
        }

        public HSV ToHSV()
        {
            RGB rgb = this;
            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(rgb.R, rgb.G), rgb.B);
            v = Math.Max(Math.Max(rgb.R, rgb.G), rgb.B);
            delta = v - min;

            if (v == 0.0)
                s = 0;
            else
                s = delta / v;

            if (s == 0)
                h = 0.0;

            else
            {
                if (rgb.R == v)
                    h = (rgb.G - rgb.B) / delta;
                else if (rgb.G == v)
                    h = 2 + (rgb.B - rgb.R) / delta;
                else if (rgb.B == v)
                    h = 4 + (rgb.R - rgb.G) / delta;

                h *= 60;

                if (h < 0.0)
                    h += 360;
            }

            return new HSV(h, s, (v / 255));
        }

    }

    public struct HSV
    {
        private double _h;
        private double _s;
        private double _v;

        public HSV(double h, double s, double v)
        {
            _h = h;
            _s = s;
            _v = v;
        }

        public double H
        {
            get { return _h; }
            set { _h = value; }
        }

        public double S
        {
            get { return _s; }
            set { _s = value; }
        }

        public double V
        {
            get { return _v; }
            set { _v = value; }
        }
        public RGB ToRGB()
        {
            HSV hsv = this;
            double r, g, b;

            if (hsv.S == 0)
            {
                r = hsv.V;
                g = hsv.V;
                b = hsv.V;
            }
            else
            {
                int i;
                double f, p, q, t;

                if (hsv.H == 360)
                    hsv.H = 0;
                else
                    hsv.H /= 60;

                i = (int)Math.Truncate(hsv.H);
                f = hsv.H - i;

                p = hsv.V * (1.0 - hsv.S);
                q = hsv.V * (1.0 - (hsv.S * f));
                t = hsv.V * (1.0 - (hsv.S * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = hsv.V;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = hsv.V;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = hsv.V;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = hsv.V;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = hsv.V;
                        break;

                    default:
                        r = hsv.V;
                        g = p;
                        b = q;
                        break;
                }

            }

            return new RGB((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
    }
}
