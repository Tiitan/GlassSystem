using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using UnityEngine;
using static GlassSystem.MathNetUtils;

namespace GlassSystem
{
    public static class ClipPattern
    {
        public static List<LineSegment2D> Clip(Mesh pattern, Polygon2D mask, Vector3 offset)
        {
            var patternLines = MeshToSegments(pattern, offset);
            var (lines, intersections) = ClipEdges(mask, patternLines);
            var edges = BuildEdgeLines(mask.Vertices.Concat(intersections).ToList());
            return edges.Concat(lines).ToList();
        }
    
        static List<LineSegment2D> MeshToSegments(Mesh mesh, Vector3 offset)
        {
            var vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            var indices = mesh.GetIndices(0);
            var lines = new List<LineSegment2D>();
            for (int i = 0; i < indices.Length; i += 2)
            {
                Vector3 start = vertices[indices[i]] + offset;
                Vector3 end = vertices[indices[i + 1]] + offset;
                lines.Add(new LineSegment2D(new Point2D(start.x, start.y), new Point2D(end.x, end.y)));
            }

            return lines;
        }
    
        static (List<LineSegment2D>, List<Point2D>) ClipEdges(Polygon2D target, List<LineSegment2D> patternLines)
        {
            List<LineSegment2D> outputLines = new List<LineSegment2D>();
            List<Point2D> outputIntersections = new List<Point2D>();
            foreach (var e in patternLines)
            {
                bool startIn = target.EnclosesPoint(e.StartPoint);
                bool endInn = target.EnclosesPoint(e.EndPoint);
                if (startIn && endInn) // edge fully inside: added
                    outputLines.Add(e);
                else if (startIn ^ endInn) // edge cutting the limit
                {
                    Point2D? intersection = IntersectPolygon(target, e);
                    outputIntersections.Add(intersection!.Value);
                    outputLines.Add(new LineSegment2D(startIn ? e.StartPoint : e.EndPoint, intersection!.Value));
                }
            }
            return (outputLines, outputIntersections);
        }
    
        static Point2D? IntersectPolygon(Polygon2D polygon, LineSegment2D line)
        {
            foreach (var pLine in polygon.Edges)
            {
                if (pLine.TryIntersect(line, out Point2D intersection, new Angle()))
                    return intersection;
            }

            return null;
        }
    
        static List<LineSegment2D> BuildEdgeLines(List<Point2D> points)
        {
            points.Sort((a, b) => CompareVectorAngle(new Point2D(0, 0), a, b));
            List<LineSegment2D> edges = new List<LineSegment2D>();
            for (int i = 0; i < points.Count; i++)
                edges.Add(new LineSegment2D(points[i], points[(i + 1) % points.Count]));
            return edges;
        }
    }
}
