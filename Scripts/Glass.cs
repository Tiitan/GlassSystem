using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using UnityEngine;
using UnityEngine.Rendering;
using static GlassSystem.MathNetUtils;
using Random = UnityEngine.Random;

namespace GlassSystem
{
    public class Glass : MonoBehaviour
    {
        private const float MicroShardSurface = 0.1f;
        private const float MicroShardTimer = 4f;
        private const float SmallShardSurface = 0.2f;
        private const float SmallShardTimer = 8f;
        private const float Tolerance = 0.001f;
        
        public Mesh Pattern;

        private Transform _transform;


        public void Break(Vector3 breakPosition, Vector3 originVector)
        {
            _transform = transform;
            
            Vector3 localPosition = transform.InverseTransformPoint(breakPosition);
            var scale = _transform.lossyScale;
            localPosition.x *= scale.x;
            localPosition.y *= scale.y;
            
            Polygon2D polygon = GetPolygon(gameObject, localPosition.z);
            if (polygon is null)
                return;

            var lines = ClipPattern.Clip(Pattern, polygon, localPosition);
            var shardPolygons = BuildShardPolygons(lines);
        
            var materials = GetComponent<Renderer>().sharedMaterials;
            foreach (Polygon2D shardPolygon in shardPolygons)
            {
                var center = Point2D.Centroid(shardPolygon.Vertices);
                var shardMesh = CreateMesh(shardPolygon, center, 0.01f);
                SpawnShard(shardMesh, originVector, new Vector3((float)center.X, (float)center.Y, 0), materials);
            }
            Destroy(gameObject);
        }
    
        Polygon2D GetPolygon(GameObject target, float side)
        {
            var targetMeshFilter = target.GetComponent<MeshFilter>();
            if (targetMeshFilter == null)
                return null;

            var targetMesh = targetMeshFilter.sharedMesh;
            var targetVertices = new List<Vector3>();
            targetMesh.GetVertices(targetVertices);
            if (targetVertices.Count > 100)
            {
                Debug.LogWarning("Target too large");
                return null;
            }

            var scale = _transform.lossyScale;
            var scalingMatrix = new DiagonalMatrix(2, 2, new double[] { scale.x, scale.y });
            var targetPoints = targetVertices.Where(p => Mathf.Abs(p.z - side) < Tolerance)
                .Select(p => new Point2D(p.x, p.y)).ToList(); // Discard backface
            targetPoints = targetPoints.Distinct(new Point2DComparer(Tolerance)) // Discard side submesh vertex duplicates
                .Select(p => p.TransformBy(scalingMatrix)).ToList(); // Apply transform scaling 
            
            Polygon2D targetPolygon = BuildConvexPolygon(targetPoints);
            return targetPolygon;
        }
    
        Polygon2D BuildConvexPolygon(List<Point2D> points)
        {
            points.Sort((a, b) => CompareVectorAngle(new Point2D(0, 0), a, b));
            return new Polygon2D(points);
        }
    
        /// <summary>
        /// Algorithm inspired by https://stackoverflow.com/questions/35468830/efficient-algorithm-to-create-polygons-from-a-2d-mesh-verticesedges
        /// convert the list of segment into a graph, order that graph to form loops of polygons, then consume that graph
        /// </summary>
        List<Polygon2D> BuildShardPolygons(List<LineSegment2D> lines)
        {
            // Map points to index
            var pointsList = new List<Point2D>(lines.Count / 2);
            var lineMap = new List<List<int>>(lines.Count / 2);
            int maxYIndex = 0;
            foreach (LineSegment2D line in lines)
            {
                int startIndex = GetIndexOrInsert(pointsList, lineMap, line.StartPoint);
                int endIndex = GetIndexOrInsert(pointsList, lineMap, line.EndPoint);
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

        int GetIndexOrInsert(List<Point2D> pointsList, List<List<int>> edgeMap, Point2D point)
        {
            int index = pointsList.FindIndex(p => p.Equals(point, Tolerance));
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
        List<int> PopLoop(List<List<int>> lineMap, List<List<int>> lines, int start)
        {
            var loop = new List<int> { start };
            int current = start;
            int next = lines[start][0];
            while (next != start)
            {
                loop.Add(next);
                if (!lines[current].Remove(next))
                    Debug.LogError("missing index from consumable list.");
                //throw new Exception("missing index from consumable list.");
                int prev = current;
                current = next;
                next = lineMap[current][(lineMap[current].FindIndex(x => x == prev) + 1) % lineMap[current].Count];

                if (loop.Count > 100)
                    throw new Exception("loop > 100, breaking infinite loop.");
            }
            lines[current].Remove(start);
            return loop;
        }
    
        void SpawnShard(Mesh mesh, Vector3 OriginVector, Vector3 offset, Material[] materials)
        {
            float shardSurface = mesh.bounds.size.x * mesh.bounds.size.y;

            var go = new GameObject(mesh.name);

            go.tag = gameObject.tag;
            go.transform.position = _transform.position + offset;
            go.transform.rotation = _transform.rotation;
        
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        
            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
        
            var meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = mesh;

            if (shardSurface > SmallShardSurface)
            {
                var glass = go.AddComponent<Glass>();
                glass.Pattern = Pattern;
            }
            else
                Destroy(go, shardSurface > MicroShardSurface ? SmallShardTimer : MicroShardTimer); // destroy small shards after x seconds

            /*if (Random.Range(0, 2) == 1) // TODO: rigidbody rules
            {
                var rigidbody = go.AddComponent<Rigidbody>();
                rigidbody.mass = shardSurface;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbody.AddForce(OriginVector);
            }*/
        }
    
        Mesh CreateMesh(Polygon2D polygon, Point2D offset, float thickness)
        {
            var mesh = new Mesh { name = "Shard" };
            var indices = new List<int>();

            var sideSize = polygon.Vertices.Count();
            // Front
            var vertices = polygon.Vertices.Select(v => new Vector3((float)(v.X - offset.X), (float)(v.Y - offset.Y), 0)).ToList();
            for (int i = 1; i < sideSize - 1; i++)
                indices.AddRange(new[] {0, i + 1, i});

            // Back
            vertices.AddRange(polygon.Vertices.Select(v => new Vector3((float)(v.X - offset.X), (float)(v.Y - offset.Y), -thickness)));
            for (int i = 1; i < sideSize - 1; i++)
                indices.AddRange(new[] {sideSize, sideSize + i, sideSize + i + 1});

        
            // side
            var sideIndexStart = indices.Count;
            var sideVertexStart = vertices.Count;
            for (int i = 0; i < sideSize; i++)
            {
                indices.AddRange(new[]
                {
                    sideVertexStart + i,
                    sideVertexStart + sideSize + i,
                    sideVertexStart * 2 + (i + 1) % sideSize, 
                    sideVertexStart + sideSize + i,
                    sideVertexStart * 2 + sideSize + (i + 1) % sideSize,
                    sideVertexStart * 2 + (i + 1) % sideSize
                });
            
            }

            var faceVertices = vertices.ToList();
            vertices.AddRange(faceVertices);
            vertices.AddRange(faceVertices);
        
            var layout = new[] { new VertexAttributeDescriptor(VertexAttribute.Position) };
            mesh.SetVertexBufferParams(vertices.Count, layout);
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Count);
            mesh.SetIndexBufferParams(indices.Count, IndexFormat.UInt32);
            mesh.SetIndexBufferData(indices,0, 0, indices.Count);
            mesh.subMeshCount = 2;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, sideIndexStart));
            mesh.SetSubMesh(1, new SubMeshDescriptor(sideIndexStart, indices.Count - sideIndexStart));
        
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
