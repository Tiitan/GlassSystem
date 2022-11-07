using System;
using System.Collections.Generic;
using MathNet.Spatial.Euclidean;

namespace GlassSystem
{
    public static class MathNetUtils
    {
        public static int CompareVectorAngle(Point2D origin, Point2D a, Point2D b)
        {
            Vector2D vecA = origin.VectorTo(a).Normalize();
            Vector2D vecB = origin.VectorTo(b).Normalize();
            double angle = Math.Atan2(vecB.Y, vecB.X) - Math.Atan2(vecA.Y, vecA.X);
            return angle > 0 ? 1 : angle < 0 ? -1 : 0;
        }
    
        public class Point2DComparer : IEqualityComparer<Point2D>
        {
            private readonly double _tolerance;

            public Point2DComparer(double tolerance)
            {
                _tolerance = tolerance;
            }
        
            public bool Equals(Point2D a, Point2D b) {
                return a.Equals(b, _tolerance);
            }

            public int GetHashCode(Point2D obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
