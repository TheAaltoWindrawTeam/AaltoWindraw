using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gfoidl.StringSearching;
using AaltoWindraw.Properties;
using System.Globalization;

namespace AaltoWindraw.Utilities
{
    public class StringDistanceEvaluator
    {
        private double threshold;

        private static StringDistanceEvaluator INSTANCE;

        private StringDistanceEvaluator()
        {
            threshold = Double.Parse(Properties.Resources.string_distance_threshold, CultureInfo.InvariantCulture);
        }

        public static StringDistanceEvaluator GetInstance()
        {
            if (INSTANCE == null)
                INSTANCE = new StringDistanceEvaluator();
            return INSTANCE;
        }

        public static int ComputeDistance(string a, string b)
        {
            if (INSTANCE == null)
                INSTANCE = new StringDistanceEvaluator();
            return FuzzySearch.LevenshteinDistance(a, b);
        }

        public static double ComputeRelativeDistance(string a, string b)
        {
            return (double)ComputeDistance(a, b) / Math.Max(a.Length, b.Length);
        }

        public static bool Distant(string a, string b)
        {
            return ComputeRelativeDistance(a, b) > GetInstance().threshold;
        }

    }
}
