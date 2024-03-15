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

        public int health = 2;
        
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
             base.Break(breakPosition, originVector, patternIndex, rotation);
             
             Destroy(GetComponent<MeshFilter>());
             Destroy(GetComponent<MeshRenderer>());
             Destroy(GetComponent<MeshRenderer>());
         }
         
         public void OnShardDestroyed(Shard shard)
         {
             _shards.Remove(shard);
             if (health > 0)
             {
                 health -= 1;
                 if (health == 0)
                 {
                     foreach (Shard s in _shards)
                     {
                         s.Fall();
                     }
                 }
             }
         }

         public void OnNewSHard(Shard shard)
         {
             _shards.Add(shard);
         }
    }
}
