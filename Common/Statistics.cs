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
            for (int i = 0; i < absoluteErrors.Count; i++)
            {
                if (isInsideCircle[i])
                {
                    // inside the track, so add a negative value
                    absoluteErrors[i] *= -1;
                }
            }

            return absoluteErrors;
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
