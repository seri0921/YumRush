using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class TreasureKeyLink : MonoBehaviour
{
    [Header("ターゲット")]
    [SerializeField] private Transform _source;
    [SerializeField] private Transform _target;

    [Header("コンポーネント")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private ParticleSystem _sparkles;

    [Header("演出")]
    [SerializeField] private float _scrollSpeed = 1.5f;

    private Material _lineMaterial;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer != null) _lineMaterial = _lineRenderer.material;
    }

    public void SetTargets(Transform source, Transform target)
    {
        _source = source;
        _target = target;
    }

    private void LateUpdate()
    {
        if (_source == null || !_source.gameObject.activeInHierarchy ||
            _target == null || !_target.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 startPos = _source.position;
        Vector3 endPos = _target.position;

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);

        if (_lineMaterial != null)
        {
            Vector2 offset = _lineMaterial.GetTextureOffset(MainTex);
            offset.x -= _scrollSpeed * Time.deltaTime;
            _lineMaterial.SetTextureOffset(MainTex, offset);
        }

        if (_sparkles != null)
        {
            Vector3 dir = endPos - startPos;
            float distance = dir.magnitude;

            _sparkles.transform.position = startPos;
            if (distance > 0.001f)
            {
                _sparkles.transform.rotation = Quaternion.LookRotation(dir);
            }

            var shape = _sparkles.shape;
            shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
            shape.length = distance;
        }
    }
}
