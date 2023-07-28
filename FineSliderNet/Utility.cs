using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineSliderNet
{
    public static class Utility
    {
        public static string NominalToString(this double value, string nominalName, int digitsSoft = 0, int digitsHard = 0, bool overrangechar = false)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "INF";
            if (digitsSoft > 0 && digitsHard > 0) return "ARG";
            bool zero = Math.Abs(value) < double.Epsilon;
            string formater = "{0:F0}";
            int digits = 3;
            if (digitsSoft > 0 && digitsSoft <= 12) { formater = "{0:0." + new string('#', digitsSoft) + "}"; digits = digitsSoft; }
            int major = zero ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(value)));
            if (!zero && digitsHard > 0 && digitsHard <= 12)
            {
                major += (int)Math.Floor(Math.Log10(Math.Abs(Math.Round(value / Math.Pow(10, major), digitsHard))));

                if (major >= 0) digits = digitsHard - (Math.Abs(major) % 3);
                else digits = Math.Abs(major + 1) % 3 + (digitsHard > 2 ? digitsHard - 2 : 0);
                if (digits < 0) digits = 0;
                formater = "{0:F" + digits + "}";
            }
            if (major >= 12) return overrangechar ? "∞" + (string.IsNullOrEmpty(nominalName) ? "" : " " + nominalName) : string.Format(CultureInfo.InvariantCulture, "{0:E}" + (string.IsNullOrEmpty(nominalName) ? "" : " " + nominalName), value);
            else if (major >= 9 && major < 12) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "G" : " G" + nominalName), Math.Round(value / 1e9, digits));
            else if (major >= 6 && major < 9) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "M" : " M" + nominalName), Math.Round(value / 1e6, digits));
            else if (major >= 3 && major < 6) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "k" : " k" + nominalName), Math.Round(value / 1e3, digits));
            else if ((major >= 1 && major < 3) || major == 0) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "" : " " + nominalName), Math.Round(value, digits));
            else if (major < 0 && major >= -3) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "m" : " m" + nominalName), Math.Round(value * 1e3, digits));
            else if (major < -3 && major >= -6) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "u" : " u" + nominalName), Math.Round(value * 1e6, digits));
            else if (major < -6 && major >= -9) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "n" : " n" + nominalName), Math.Round(value * 1e9, digits));
            else if (major < -9 && major >= -12) return string.Format(CultureInfo.InvariantCulture, formater + (string.IsNullOrEmpty(nominalName) ? "p" : " p" + nominalName), Math.Round(value * 1e12, digits));
            else if (major < -12) return overrangechar ? "0" + (string.IsNullOrEmpty(nominalName) ? "" : " " + nominalName) : string.Format(CultureInfo.InvariantCulture, "{0:E}" + (string.IsNullOrEmpty(nominalName) ? "" : " " + nominalName), value);
            else return "ERR";
        }

        public static double Near125(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x)) return x;
            x = Math.Abs(x);
            int major = (int)Math.Floor(Math.Log10(x));
            double r1 = 1 * Math.Pow(10, major); double d1 = Math.Abs(r1 - x);
            double r2 = 2 * Math.Pow(10, major); double d2 = Math.Abs(r2 - x);
            double r5 = 5 * Math.Pow(10, major); double d5 = Math.Abs(r5 - x);
            double r10 = 10 * Math.Pow(10, major); double d10 = Math.Abs(r10 - x);
            if (d1 <= d2 && d1 <= d5 && d1 <= d10) return r1;
            else if (d2 <= d1 && d2 <= d5 && d2 <= d10) return r2;
            else if (d5 <= d1 && d5 <= d2 && d5 <= d10) return r5;
            else if (d10 <= d1 && d10 <= d2 && d10 <= d5) return r10;
            else throw new NotImplementedException();
        }
    }
}
