using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using UnityEngine;
using static GlassSystem.Scripts.MathNetUtils;

namespace GlassSystem.Scripts
{
    public class GlassPanel : BaseGlass
    {
        protected List<Shard> _shards;  // child of this glass panel once shattered
        private Dictionary<Shard, HashSet<Shard>> _neighborGraph;
        private HashSet<Shard> _anchoredShards;
        private List<LineSegment2D> _frameBoundaryEdges;
        
        protected void Start()
        {
            _parentPanel = this;
        }

        /// <summary>
        /// Build polygon initialize a glass panel about to be broken for the first time,
        /// it is not used by shards which receives their data from InitializeShard instead.
        /// </summary>
        /// <param name="side">z position of the impact, used to discard the back face before building the polygon</param>
        /// <returns>2D polygon representing the glass panel</returns>
        protected override Polygon2D BuildPolygon(float side)
        {
            var targetMeshFilter = GetComponent<MeshFilter>();
            if (targetMeshFilter == null)
                return null;

            var targetMesh = targetMeshFilter.sharedMesh;
            var targetVertices = targetMesh.vertices;
            if (targetVertices.Length is > 100 or < 3)
            {
                Debug.LogWarning($"Invalid mesh ({targetVertices.Length})");
                return null;
            }

            // Scale
            var scale = _transform.lossyScale;
            var scalingMatrix = new DiagonalMatrix(2, 2, new double[] { scale.x, scale.y });
            
            // Thickness
            var verticesZ = targetVertices.Select(p => p.z).ToList();
            _thickness = (verticesZ.Max() + Mathf.Abs(verticesZ.Min())) * scale.z;
            
            // Vertices to polygon
            var targetPoints = targetVertices.Select((p, i) =>  new IndexedPoint(p, i)).ToList();
            targetPoints.RemoveAll(p => Mathf.Abs(p.Z - side) > Tolerance); // Discard backface
            targetPoints = targetPoints.Distinct(new Point2DComparer(Tolerance)).ToList(); // Discard side submesh vertex duplicates
            foreach (var point in targetPoints)
                point.TransformBy(scalingMatrix);
            
            // Build convex polygon
            targetPoints.Sort((a, b) => CompareVectorAngle(new Point2D(0, 0), a, b));
            Polygon2D targetPolygon = new Polygon2D(targetPoints.Select(p => p.Point2D));

            // UVs
            var uvs = targetMesh.uv;
            if (uvs != null && uvs.Length > 0)
            {
                _uvs = new Vector2[targetPoints.Count];
                for (int i = 0; i < targetPoints.Count; i++)
                    _uvs[i] = uvs[targetPoints[i].Index];
            }

            // Store frame boundary for anchor detection (before shards are created)
            _frameBoundaryEdges = targetPolygon.Edges.ToList();

            return targetPolygon;
        }
         
         public override void Break(Vector3 breakPosition, Vector3 originVector, int patternIndex = -1, float rotation = float.NaN)
         {
             if (_shards is not null)
             {
                 Debug.LogError("GlassPanel broken twice");
                 return;
             }

             _shards = new();
             _neighborGraph = new Dictionary<Shard, HashSet<Shard>>();
             _anchoredShards = new HashSet<Shard>();

             base.Break(breakPosition, originVector, patternIndex, rotation);

             Destroy(GetComponent<MeshFilter>());
             Destroy(GetComponent<MeshRenderer>());
             Destroy(GetComponent<Collider>());
         }
         
         public void OnShardDestroyed(Shard shard)
         {
             RemoveFromGraph(shard);
             _shards.Remove(shard);

             // Find and drop disconnected shards
             var disconnected = FindDisconnectedShards();
             foreach (var s in disconnected)
             {
                 RemoveFromGraph(s);
                 _shards.Remove(s);
                 s.Fall();
             }
         }

         public void OnNewShard(Shard shard)
         {
             _shards.Add(shard);
             _neighborGraph[shard] = new HashSet<Shard>();

             // Check for neighbors among existing shards
             foreach (var other in _shards)
             {
                 if (other != shard && _neighborGraph.ContainsKey(other) && SharesEdge(shard, other))
                 {
                     _neighborGraph[shard].Add(other);
                     _neighborGraph[other].Add(shard);
                 }
             }

             // Check if anchored to frame
             if (TouchesFrameBoundary(shard))
                 _anchoredShards.Add(shard);
         }

         private bool SharesEdge(Shard a, Shard b)
         {
             foreach (var edgeA in a.GetEdgesInPanelSpace())
                 foreach (var edgeB in b.GetEdgesInPanelSpace())
                     if (EdgesMatch(edgeA, edgeB))
                         return true;
             return false;
         }

         private bool EdgesMatch(LineSegment2D a, LineSegment2D b)
         {
             // Match if endpoints are same (either direction)
             return (a.StartPoint.Equals(b.StartPoint, Tolerance) && a.EndPoint.Equals(b.EndPoint, Tolerance)) ||
                    (a.StartPoint.Equals(b.EndPoint, Tolerance) && a.EndPoint.Equals(b.StartPoint, Tolerance));
         }

         private bool TouchesFrameBoundary(Shard shard)
         {
             if (_frameBoundaryEdges == null) return false;
             foreach (var shardEdge in shard.GetEdgesInPanelSpace())
                 foreach (var frameEdge in _frameBoundaryEdges)
                     if (EdgeLiesOnFrameEdge(shardEdge, frameEdge))
                         return true;
             return false;
         }

         private bool EdgeLiesOnFrameEdge(LineSegment2D shardEdge, LineSegment2D frameEdge)
         {
             return PointLiesOnSegment(shardEdge.StartPoint, frameEdge) &&
                    PointLiesOnSegment(shardEdge.EndPoint, frameEdge);
         }

         private bool PointLiesOnSegment(Point2D point, LineSegment2D segment)
         {
             var closest = segment.ClosestPointTo(point);
             if (!point.Equals(closest, Tolerance))
                 return false;

             var dir = segment.EndPoint - segment.StartPoint;
             var toPoint = point - segment.StartPoint;
             double t = dir.DotProduct(toPoint) / dir.DotProduct(dir);
             return t >= -Tolerance && t <= 1.0 + Tolerance;
         }

         private void RemoveFromGraph(Shard shard)
         {
             if (_neighborGraph == null) return;
             if (_neighborGraph.TryGetValue(shard, out var neighbors))
                 foreach (var neighbor in neighbors)
                     _neighborGraph[neighbor].Remove(shard);
             _neighborGraph.Remove(shard);
             _anchoredShards.Remove(shard);
         }

         private List<Shard> FindDisconnectedShards()
         {
             if (_neighborGraph == null || _anchoredShards.Count == 0)
                 return _shards.ToList(); // All fall if no anchors

             // BFS from anchored shards
             var reachable = new HashSet<Shard>(_anchoredShards);
             var queue = new Queue<Shard>(_anchoredShards);

             while (queue.Count > 0)
             {
                 var current = queue.Dequeue();
                 if (_neighborGraph.TryGetValue(current, out var neighbors))
                     foreach (var neighbor in neighbors)
                         if (reachable.Add(neighbor))
                             queue.Enqueue(neighbor);
             }

             return _shards.Where(s => !reachable.Contains(s)).ToList();
         }
    }
}
