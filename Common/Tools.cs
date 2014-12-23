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

    }
}
