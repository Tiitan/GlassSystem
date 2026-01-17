using System;
using MathNet.Spatial.Euclidean;
using UnityEngine;

namespace GlassSystem.Scripts
{
    public class Shard : BaseGlass
    {
        public void InitializeShard(GlassPanel parentPanel, Polygon2D polygon, Vector2[] uvs, float thickness)
        {
            _parentPanel = parentPanel;
            _polygon = polygon;
            _thickness = thickness;
            _uvs = uvs;
        }

        public override void Break(Vector3 breakPosition, Vector3 originVector, int patternIndex = -1, float rotation = Single.NaN)
        {
            base.Break(breakPosition, originVector, patternIndex, rotation);
            _parentPanel.OnShardDestroyed(this);
            Destroy(gameObject);
        }
    }
}
