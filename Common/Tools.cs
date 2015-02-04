using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TheoryC.Common
{
    public static class Tools
    {
        public static double DegreeToRadian(double angleInDegrees)
        {
            return Math.PI * angleInDegrees / 180.0;
        }

        public static void PointsOnACircle(double radius, double angleInDegrees, Point center, out double x, out double y)
        {
            double angle = DegreeToRadian(angleInDegrees);
            x = radius * Math.Cos(angle) + center.X;
            y = radius * Math.Sin(angle) + center.Y;
        }

        public static bool IsInsideCircle(Point pt, Point circleCenter, double circleRadius)
        {
            // inside the circle satisfies the equation: (x - center_x)^2 + (y - center_y)^2 < radius^2
            double leftHandSide = (Math.Pow((pt.X - circleCenter.X), 2) + Math.Pow((pt.Y - circleCenter.Y), 2));
            double rightHandSide = (Math.Pow(circleRadius, 2));

            if (leftHandSide < rightHandSide)
            {
                // inside the circle
                return true;
            }

            else
            {
                // outside the circle 
                return false;
            }           
        }


        public static Point GetPointForPlacingTargetInStartingPosition(Point trackCenter, double trackRadius, double targetRadius)
        {
            Point pt = new Point(); 
            double rightX = trackCenter.X + trackRadius;

            pt.X = rightX - targetRadius;
            pt.Y = trackCenter.Y - targetRadius;

            return pt;
        }

        public static double DistanceBetween2Points(Point pt1, Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
        }

        public static void BreakListIntoThirds(List<double> originalList, out List<double> list1, out List<double> list2, out List<double> list3 )
        {
            int indexMarker = originalList.Count / 3;
            int remainder = originalList.Count % 3;

            // if there's 1 extra, throw into list3
            if (remainder == 0 || remainder == 1)
            {
                list1 = originalList.GetRange(0, indexMarker);
                list2 = originalList.GetRange(indexMarker, indexMarker);
                list3 = originalList.GetRange(indexMarker * 2, originalList.Count - indexMarker * 2);
            }
            else // need to put the 2 extras into list2 and list3
            {
                list1 = originalList.GetRange(0, indexMarker);
                list2 = originalList.GetRange(indexMarker, indexMarker + 1);
                list3 = originalList.GetRange((indexMarker * 2) + 1, originalList.Count - (indexMarker * 2) - 1);
            }
        }

        public static void BreakListIntoThirds(List<bool> originalList, out List<bool> list1, out List<bool> list2, out List<bool> list3)
        {
            int indexMarker = originalList.Count / 3;
            int remainder = originalList.Count % 3;

            // if there's 1 extra, throw into list3
            if (remainder == 0 || remainder == 1)
            {
                list1 = originalList.GetRange(0, indexMarker);
                list2 = originalList.GetRange(indexMarker, indexMarker);
                list3 = originalList.GetRange(indexMarker * 2, originalList.Count - indexMarker * 2);
            }
            else // need to put the 2 extras into list2 and list3
            {
                list1 = originalList.GetRange(0, indexMarker);
                list2 = originalList.GetRange(indexMarker, indexMarker + 1);
                list3 = originalList.GetRange((indexMarker * 2) + 1, originalList.Count - (indexMarker * 2) - 1);
            }
        }

        public static void BreakListIntoThirds(List<PointF> originalList, out List<PointF> list1, out List<PointF> list2, out List<PointF> list3)
        {
            int indexMarker = originalList.Count / 3;
            int remainder = originalList.Count % 3;

            // if there's 1 extra, throw into list3
            if (remainder == 0 || remainder == 1)
            {
                list1 = originalList.GetRange(0, indexMarker);
                list2 = originalList.GetRange(indexMarker, indexMarker);
                list3 = originalList.GetRange(indexMarker * 2, originalList.Count - indexMarker * 2);
            }
            else // need to put the 2 extras into list2 and list3
            {
                list1 = originalList.GetRange(0, indexMarker);
                list2 = originalList.GetRange(indexMarker, indexMarker + 1);
                list3 = originalList.GetRange((indexMarker * 2) + 1, originalList.Count - (indexMarker * 2) - 1);
            }
        }

        public static void GetTimeThirdPercentages(int tickCount, out double firstTimePercentage, out double secondTimePercentage, out double thirdTimePercentage)
        {
            int remainder = tickCount % 3;
            int sizeOfList = tickCount / 3;

            // equal tick times
            if (remainder == 0)
            {
                firstTimePercentage = (double)sizeOfList / tickCount;
                secondTimePercentage = (double)sizeOfList / tickCount;
                thirdTimePercentage = (double)sizeOfList / tickCount;
            }
            // list3 has an extra tick, so compensate times
            else if (remainder == 1)
            {
                firstTimePercentage = (double)sizeOfList / tickCount;
                secondTimePercentage = (double)sizeOfList / tickCount;
                thirdTimePercentage = (double)(sizeOfList + 1) / tickCount;                
            }
            else // if remainder == 2, second and third lists will have extra ticks 
            {
                firstTimePercentage = (double)sizeOfList / tickCount;
                secondTimePercentage = (double)(sizeOfList + 1) / tickCount;
                thirdTimePercentage = (double)(sizeOfList + 1) / tickCount;                
            }
        }

        public static double CalculateTimeThirds(List<bool> OnTarget, double trialDurationSeconds, double timePercentage)
        {
            int countOnTarget = OnTarget.Where(c => c).Count(); // gets the count of true bools
            double percentageOnTarget = (double)countOnTarget / OnTarget.Count; // this count is the List<> size, not the sum
            double timeOnTargetForThird = percentageOnTarget * trialDurationSeconds * timePercentage;

            return Math.Round(timeOnTargetForThird, 3);
        }
    }
}
