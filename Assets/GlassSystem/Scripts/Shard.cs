using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;
using UnityEngine;

namespace GlassSystem.Scripts
{
    public class Shard : BaseGlass
    {
        public void InitializeShard(GlassPanel parentPanel, Polygon2D polygon, Vector2[] uvs, float thickness, Vector2 center)
        {
            _parentPanel = parentPanel;
            _polygon = polygon;
            _thickness = thickness;
            _uvs = uvs;
            _centerOffset = center;
        }

        public IEnumerable<LineSegment2D> GetEdgesInPanelSpace()
        {
            return _polygon.Edges.Select(e => new LineSegment2D(
                e.StartPoint + new Vector2D(_centerOffset.x, _centerOffset.y),
                e.EndPoint + new Vector2D(_centerOffset.x, _centerOffset.y)));
        }

        public override void Break(Vector3 breakPosition, Vector3 originVector, int patternIndex = -1, float rotation = Single.NaN)
        {
            base.Break(breakPosition, originVector, patternIndex, rotation);
            _parentPanel.OnShardDestroyed(this);
            Destroy(gameObject);
        }
    }
}
