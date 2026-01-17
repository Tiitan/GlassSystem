using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineRendererFollow : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    public Transform _targetTransform;
    public Vector3 _targetOffset;
    
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (_targetTransform)
            _lineRenderer.SetPositions(new []{transform.position, _targetTransform.position + _targetOffset});
    }
}
