using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using UnityEngine;

namespace GlassSystem.Scripts
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
        
        /// <summary>
        /// Interpolate a point inside a triangle.
        /// </summary>
        /// <param name="p">point to interpolate</param>
        /// <param name="t">array of 3 point forming a triangle (unchecked, pass at least 3 points!)</param>
        /// <returns>interpolated weight vector: x=t[0], y=t[1], z=t[2]</returns>
        public static Vector3 BarycentricInterpolation(Point2D p, Point2D[] t)
        {
            double q = (t[1].Y - t[2].Y) * (t[0].X - t[2].X) + (t[2].X - t[1].X) * (t[0].Y - t[2].Y);
            double w0 = ((t[1].Y - t[2].Y) * (p.X - t[2].X) + (t[2].X - t[1].X) * (p.Y - t[2].Y)) / q;
            double w1 = ((t[2].Y - t[0].Y) * (p.X - t[2].X) + (t[0].X - t[2].X) * (p.Y - t[2].Y)) / q;
            return new Vector3((float)w0, (float)w1, (float)(1 - w0 - w1));
        }
        
        public class IndexedPoint
        {
            private Point2D _p;
            public int Index { get; }
            public double X => _p.X;
            public double Y => _p.Y;
            public float Z { get; }
            
            public Point2D Point2D => _p;

            public IndexedPoint(Vector3 v, int index)
            {
                Index = index;
                _p = new Point2D(v.x, v.y);
                Z = v.z;
            }

            public bool Equals(IndexedPoint other, double tolerance)
            {
                return _p.Equals(_p, tolerance);
            }

            public void TransformBy(DiagonalMatrix scalingMatrix)
            {
                _p = _p.TransformBy(scalingMatrix);
            }
            
            public static implicit operator Point2D(IndexedPoint p) => p._p;
        }
        
        public class Point2DComparer : IEqualityComparer<IndexedPoint>
        {
            private readonly double _tolerance;

            public Point2DComparer(double tolerance)
            {
                _tolerance = tolerance;
            }
        
            public bool Equals(IndexedPoint a, IndexedPoint b)
            {
                if (a is null)
                    return b is null;
                return a.Equals(b, _tolerance);
            }

            public int GetHashCode(IndexedPoint p)
            {
                return p.Point2D.GetHashCode();
            }
        }
    }
}
