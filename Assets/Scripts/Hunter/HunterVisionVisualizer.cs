using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HunterVisionVisualizer : MonoBehaviour
{
    [Range(8, 96)] public int segments = 36;
    public float groundOffset = 0.04f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private HunterAI _hunter;
    private DetectionSystem _detection;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private Mesh _mesh;
    private float _lastAngle = -1f;
    private float _lastRange = -1f;

    private void Awake()
    {
        _hunter = GetComponentInParent<HunterAI>();
        _detection = GetComponentInParent<DetectionSystem>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
        transform.localPosition = new Vector3(0f, groundOffset, 0f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (_meshRenderer != null)
        {
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        RebuildMesh();
    }

    private void LateUpdate()
    {
        if (_hunter == null || _hunter.config == null)
        {
            return;
        }

        if (!Mathf.Approximately(_lastAngle, _hunter.config.viewAngle)
            || !Mathf.Approximately(_lastRange, _hunter.GetCurrentViewRange()))
        {
            RebuildMesh();
        }

        var seesPlayer = _detection != null && _hunter.Player != null && _detection.CanSeePlayer(_hunter.Player);
        var color = GetStateColor(_hunter.currentState, seesPlayer);
        ApplyColor(color);
    }

    private void OnDestroy()
    {
        if (_mesh != null)
        {
            Destroy(_mesh);
        }
    }

    private void RebuildMesh()
    {
        if (_meshFilter == null || _hunter == null || _hunter.config == null)
        {
            return;
        }

        if (_mesh == null)
        {
            _mesh = new Mesh { name = "HunterVisionCone" };
        }
        else
        {
            _mesh.Clear();
        }

        var range = Mathf.Max(0.5f, _hunter.GetCurrentViewRange());
        var angle = Mathf.Clamp(_hunter.config.viewAngle, 10f, 360f);
        var arcSegments = Mathf.Max(8, segments);
        var vertices = new Vector3[arcSegments + 2];
        var triangles = new int[arcSegments * 3];
        vertices[0] = Vector3.zero;

        if (angle >= 359.9f)
        {
            for (var i = 0; i <= arcSegments; i++)
            {
                var t = i / (float)arcSegments;
                var radians = t * Mathf.PI * 2f;
                vertices[i + 1] = new Vector3(Mathf.Sin(radians) * range, 0f, Mathf.Cos(radians) * range);
            }
        }
        else
        {
            var halfAngle = angle * 0.5f;
            for (var i = 0; i <= arcSegments; i++)
            {
                var t = i / (float)arcSegments;
                var radians = Mathf.Deg2Rad * Mathf.Lerp(-halfAngle, halfAngle, t);
                vertices[i + 1] = new Vector3(Mathf.Sin(radians) * range, 0f, Mathf.Cos(radians) * range);
            }
        }

        for (var i = 0; i < arcSegments; i++)
        {
            var triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _meshFilter.sharedMesh = _mesh;

        _lastAngle = angle;
        _lastRange = range;
    }

    private void ApplyColor(Color color)
    {
        if (_meshRenderer == null)
        {
            return;
        }

        _meshRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(BaseColorId, color);
        _propertyBlock.SetColor(ColorId, color);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    private static Color GetStateColor(HunterState state, bool seesPlayer)
    {
        var color = state switch
        {
            HunterState.Investigate => new Color(1f, 0.78f, 0.24f, 0.17f),
            HunterState.Chase => new Color(1f, 0.36f, 0.20f, 0.20f),
            HunterState.Lockdown => new Color(1f, 0.14f, 0.12f, 0.24f),
            _ => new Color(0.22f, 0.66f, 0.98f, 0.12f)
        };

        if (seesPlayer)
        {
            color.a = Mathf.Min(0.32f, color.a + 0.08f);
        }

        return color;
    }
}
