using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TheoryC.Common
{
    // http://www.martijnkooij.nl/2013/04/csharp-math-mean-variance-and-standard-deviation/

    public class Statistics
    {
        public static double DistanceBetween2Points(Point pt1, Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static double CalculateConstantError(List<double> absoluteErrors, List<bool> isInsideCircle)
        {
            double constantError = 0;

            for (int i = 0; i < absoluteErrors.Count; i++)
            {
                if (isInsideCircle[i])
                {
                    // inside the track, so add a negative value
                    constantError += absoluteErrors[i] * -1;
                }
                else
                {
                    // outside track, so just add positive value
                    constantError += absoluteErrors[i];
                }
            }

            // need to divide by the total number of ticks for the mean
            constantError /= absoluteErrors.Count;

            return constantError;
        }

        public static double StandardDeviation(List<double> valueList)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in valueList)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public static double Mean(List<double> valueList)
        {
            double s = 0;

            for (int i = 0; i < valueList.Count; i++)
            {
                s += valueList[i];
            }

            return s / valueList.Count;
        }
    }
}
