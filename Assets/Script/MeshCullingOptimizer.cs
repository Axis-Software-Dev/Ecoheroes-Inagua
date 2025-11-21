using UnityEngine;

public class MeshCullingOptimizer : MonoBehaviour
{
    private SkinnedMeshRenderer _renderer;
    private Animator _animator;
    private Transform _camera;

    private const float RENDER_DISTANCE = 30f;
    private const float ANIMATION_DISTANCE = 20f;

    private void Start()
    {
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _animator = GetComponent<Animator>();

        if (Camera.main != null)
        {
            _camera = Camera.main.transform;
        }

        if (_renderer != null)
        {
            _renderer.updateWhenOffscreen = false;
            _renderer.skinnedMotionVectors = false;
        }
    }

    private void Update()
    {
        if (_camera == null) return;

        float distance = Vector3.Distance(transform.position, _camera.position);

        if (_renderer != null)
        {
            _renderer.enabled = distance < RENDER_DISTANCE;
        }

        if (_animator != null)
        {
            _animator.enabled = distance < ANIMATION_DISTANCE;
        }
    }
}
