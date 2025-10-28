using UnityEngine;
public class MeshCullingOptimizer : MonoBehaviour
{
    private SkinnedMeshRenderer _renderer;
    private Animator _animator;
    private Transform _camera;

    void Start()
    {
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _animator = GetComponent<Animator>();
        _camera = Camera.main.transform;

        // Critical setting
        if (_renderer != null)
        {
            _renderer.updateWhenOffscreen = false;
            _renderer.skinnedMotionVectors = false;
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, _camera.position);

        // Disable rendering beyond 30m
        if (_renderer != null)
            _renderer.enabled = distance < 30f;

        // Disable animation beyond 20m
        if (_animator != null)
            _animator.enabled = distance < 20f;
    }
}
