using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using EdmontonDrawingValidator.Model;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace EdmontonDrawingValidator
{
    public static class NumberExtensions
    {
        // <= C#6
        //public static bool IsNumeric(this string str)
        //{
        //    float f;
        //    return float.TryParse(str, out f);
        //}

        // C# 7+
        static double s = 0;
        public static bool IsNumeric(this string str) => double.TryParse(str, out s);
    }

    

}
