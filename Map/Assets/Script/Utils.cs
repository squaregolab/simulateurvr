using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public static class Utils
    {
        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
