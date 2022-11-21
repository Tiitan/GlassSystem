using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;
using static GlassSystem.Scripts.MathNetUtils;


namespace GlassSystem.Scripts
{
    public static class ShardPolygonBuilder
    {
        
        /// <summary>
        /// Algorithm inspired by https://stackoverflow.com/questions/35468830/efficient-algorithm-to-create-polygons-from-a-2d-mesh-verticesedges
        /// convert the list of segment into a graph, order that graph to form loops of polygons, then consume that graph to create a list of loop (polygons)
        /// </summary>
        public static List<Polygon2D> Build(List<LineSegment2D> lines, float tolerance)
        {
            // Map points to index
            var pointsList = new List<Point2D>(lines.Count / 2);
            var lineMap = new List<List<int>>(lines.Count / 2);
            int maxYIndex = 0;
            foreach (LineSegment2D line in lines)
            {
                int startIndex = GetIndexOrInsert(pointsList, lineMap, line.StartPoint, tolerance);
                int endIndex = GetIndexOrInsert(pointsList, lineMap, line.EndPoint, tolerance);
                if (startIndex == endIndex || lineMap[startIndex].Contains(endIndex) || lineMap[endIndex].Contains(startIndex)) // discard micro line
                    continue;
                lineMap[startIndex].Add(endIndex);
                lineMap[endIndex].Add(startIndex);
            
                // save lowest Y point in the graph to easily remove the outer loop
                if (line.StartPoint.Y > pointsList[maxYIndex].Y)
                    maxYIndex = startIndex;
                if (line.EndPoint.Y > pointsList[maxYIndex].Y)
                    maxYIndex = endIndex;
            }
        
            // Sort lineMap clockwise
            for (int i = 0; i < lineMap.Count; i++)
                lineMap[i].Sort((j, k) => CompareVectorAngle(pointsList[i], pointsList[j], pointsList[k]));
        
            // Deep copy linemap to create a consumable version that will be popped each time a line is used.
            // lineMap itself can't be consumed because looping require reading the inverse segments
            var consumableLineMap = lineMap.Select(line => line.ToList()).ToList();

            // Remove exterior loop
            PopLoop(lineMap, consumableLineMap, maxYIndex);
            
            // Build loops
            var loops = new List<List<int>>();
            for (int i = 0; i < lineMap.Count; i++)
            {
                while (consumableLineMap[i].Count != 0)
                    loops.Add(PopLoop(lineMap, consumableLineMap, i));
            }

            // Loops to polygons
            var shards = new List<Polygon2D>();
            foreach (var loop in loops) 
                shards.Add(Polygon2D.GetConvexHullFromPoints(loop.Select(x => pointsList[x])));

            return shards;
        }

        static int GetIndexOrInsert(List<Point2D> pointsList, List<List<int>> edgeMap, Point2D point, float tolerance)
        {
            int index = pointsList.FindIndex(p => p.Equals(point, tolerance));
            if (index == -1)
            {
                index = pointsList.Count;
                pointsList.Add(point);
                edgeMap.Add(new List<int>());
            }
    
            return index;
        }
        
        /// <summary>
        /// Build the next vertex loop (to make a polygon) out of the line map.
        /// </summary>
        /// <param name="lineMap">first list is the list of point, nested list is the list of point linked. sorted by angle</param>
        /// <param name="lines">same as linemap, but consumed on each created loop</param>
        /// <param name="start">starting vertex for this loop</param>
        /// <returns>a new loop of point index, used to create a glass shard</returns>
        static List<int> PopLoop(List<List<int>> lineMap, List<List<int>> lines, int start)
        {
            var loop = new List<int> { start };
            int current = start;
            int next = lines[start][0];
            while (next != start)
            {
                loop.Add(next);
                if (!lines[current].Remove(next))
                    throw new InternalGlassException("missing index from consumable list.");
                int prev = current;
                current = next;
                next = lineMap[current][(lineMap[current].FindIndex(x => x == prev) + 1) % lineMap[current].Count];

                if (loop.Count > 100)
                    throw new InternalGlassException("loop > 100, breaking infinite loop.");
            }
            lines[current].Remove(start);
            return loop;
        }
    }
}