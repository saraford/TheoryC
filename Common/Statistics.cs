/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

using Microsoft.Kinect;
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

    public enum DesiredCoord
    {
        X,
        Y
    }

    public class Statistics
    {
        public static double ConstantError(List<double> absoluteErrors, List<bool> isInsideCircle)
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

        public static double VariableError(List<double> absoluteErrors, List<bool> isInsideCircle)
        {
            List<double> algebraicErrors = ConvertAbsoluteErrorToAlgebraicError(absoluteErrors, isInsideCircle);

            double variableError = PopulationStandardDeviation(algebraicErrors);

            return variableError;
        }

        private static List<double> ConvertAbsoluteErrorToAlgebraicError(List<double> absoluteErrors, List<bool> isInsideCircle)
        {
            List<double> algebraicErrors = new List<double>();

            for (int i = 0; i < absoluteErrors.Count; i++)
            {
                if (isInsideCircle[i])
                {
                    // inside the track, so negate value
                    algebraicErrors.Add(absoluteErrors[i] * -1);
                }
                else
                {
                    // outside the track, so just add value
                    algebraicErrors.Add(absoluteErrors[i]);
                }
            }

            return algebraicErrors;
        }

        // http://stackoverflow.com/questions/2253874/linq-equivalent-for-standard-deviation
        public static double PopulationStandardDeviation(List<double> values)
        {
            double answer = 0;
            int count = values.Count();
            if (count > 1)
            {
                //Compute the Average
                double avg = values.Average();

                //Perform the Sum of (value-avg)^2
                double sum = values.Sum(d => (d - avg) * (d - avg));

                //Put it all together
                answer = Math.Sqrt(sum / count);
            }
            return answer;
        }
       
        public static double PopulationStandardDeviation(List<PointF> pointfs, DesiredCoord desiredCoord)
        {
            if (desiredCoord == DesiredCoord.X)
            {
                List<double> xValues = new List<double>();
                foreach (var item in pointfs)
                {
                    xValues.Add(item.X);
                }
                return PopulationStandardDeviation(xValues);
            }
            else
            {
                List<double> yValues = new List<double>();
                foreach (var item in pointfs)
                {
                    yValues.Add(item.Y);
                }
                return PopulationStandardDeviation(yValues);
            }
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
