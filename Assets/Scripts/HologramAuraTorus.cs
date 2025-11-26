using UnityEngine;

public class HologramAuraTorus : MonoBehaviour
{
    [Header("Torus Dimensions")]
    [SerializeField] private float torusRadius = 1.5f;
    [SerializeField] private float tubeRadius = 0.15f;
    [SerializeField] private float torusHeight = 1.5f;
    [SerializeField] private int radialSegments = 48;
    [SerializeField] private int tubularSegments = 24;
    
    [Header("Position Offset")]
    [SerializeField] private float offsetX = 0f;
    [SerializeField] private float offsetY = 0.75f;
    [SerializeField] private float offsetZ = 0f;
    
    [Header("Activation Control")]
    [SerializeField] private GameObject controlObject;
    
    [Header("Wave Effect")]
    [SerializeField] private float waveAmplitude = 0.2f;
    [SerializeField] private float waveFrequency = 4f;
    [SerializeField] private float waveSpeed = 2f;
    
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 30f;
    
    [Header("Material Settings")]
    [SerializeField] private Color baseColor = new Color(0f, 0.5f, 1f, 0.3f);
    [SerializeField] private Color emissionColor = new Color(0f, 0.8f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 2f;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minAlpha = 0.1f;
    [SerializeField] private float maxAlpha = 0.6f;
    
    private GameObject torusObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material hologramMaterial;
    private Mesh torusMesh;
    private Vector3[] baseVertices;
    private float timeOffset;
    private bool wasControlObjectActive;

    private void Start()
    {
        timeOffset = Random.Range(0f, 100f);
        
        if (controlObject != null)
        {
            wasControlObjectActive = controlObject.activeSelf;
            if (wasControlObjectActive)
            {
                CreateTorusAura();
                Debug.Log($"[HologramAura] Torus created - Control object '{controlObject.name}' is active");
            }
            else
            {
                Debug.Log($"[HologramAura] Torus not created - Control object '{controlObject.name}' is inactive");
            }
        }
        else
        {
            CreateTorusAura();
            Debug.Log("[HologramAura] Torus created - No control object assigned");
        }
    }

    private void Update()
    {
        if (controlObject != null)
        {
            bool isControlObjectActive = controlObject.activeSelf;
            
            if (isControlObjectActive != wasControlObjectActive)
            {
                if (isControlObjectActive)
                {
                    if (torusObject == null)
                    {
                        CreateTorusAura();
                        Debug.Log($"[HologramAura] Torus activated - Control object '{controlObject.name}' became active");
                    }
                }
                else
                {
                    if (torusObject != null)
                    {
                        DestroyTorus();
                        Debug.Log($"[HologramAura] Torus destroyed - Control object '{controlObject.name}' became inactive");
                    }
                }
                
                wasControlObjectActive = isControlObjectActive;
            }
        }
        
        if (torusObject != null)
        {
            torusObject.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.Self);
            
            UpdateWaveEffect();
            UpdateMaterialPulse();
        }
    }

    private void CreateTorusAura()
    {
        torusObject = new GameObject("HologramAura");
        torusObject.transform.SetParent(transform);
        torusObject.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
        torusObject.transform.localRotation = Quaternion.identity;
        torusObject.transform.localScale = Vector3.one;
        
        meshFilter = torusObject.AddComponent<MeshFilter>();
        meshRenderer = torusObject.AddComponent<MeshRenderer>();
        
        torusMesh = GenerateTorusMesh();
        meshFilter.mesh = torusMesh;
        
        baseVertices = torusMesh.vertices;
        
        CreateHologramMaterial();
        meshRenderer.material = hologramMaterial;
    }

    private Mesh GenerateTorusMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralTorus";
        
        int vertexCount = radialSegments * tubularSegments;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[radialSegments * tubularSegments * 6];
        
        int vertIndex = 0;
        for (int i = 0; i < radialSegments; i++)
        {
            float u = (float)i / radialSegments;
            float angle = u * Mathf.PI * 2f;
            
            Vector3 center = new Vector3(Mathf.Cos(angle) * torusRadius, 0f, Mathf.Sin(angle) * torusRadius);
            Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            Vector3 binormal = Vector3.up;
            Vector3 normal = Vector3.Cross(tangent, binormal);
            
            for (int j = 0; j < tubularSegments; j++)
            {
                float v = (float)j / tubularSegments;
                float tubeAngle = v * Mathf.PI * 2f;
                
                Vector3 offset = (normal * Mathf.Cos(tubeAngle) + binormal * Mathf.Sin(tubeAngle)) * tubeRadius;
                
                float verticalScale = Mathf.Abs(Mathf.Sin(tubeAngle));
                offset.y *= verticalScale;
                offset.y += Mathf.Sin(tubeAngle) * tubeRadius * (torusHeight / tubeRadius - 1f);
                
                vertices[vertIndex] = center + offset;
                normals[vertIndex] = offset.normalized;
                uvs[vertIndex] = new Vector2(u, v);
                
                vertIndex++;
            }
        }
        
        int triIndex = 0;
        for (int i = 0; i < radialSegments; i++)
        {
            int nextI = (i + 1) % radialSegments;
            
            for (int j = 0; j < tubularSegments; j++)
            {
                int nextJ = (j + 1) % tubularSegments;
                
                int v0 = i * tubularSegments + j;
                int v1 = nextI * tubularSegments + j;
                int v2 = i * tubularSegments + nextJ;
                int v3 = nextI * tubularSegments + nextJ;
                
                triangles[triIndex++] = v0;
                triangles[triIndex++] = v1;
                triangles[triIndex++] = v2;
                
                triangles[triIndex++] = v2;
                triangles[triIndex++] = v1;
                triangles[triIndex++] = v3;
            }
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        return mesh;
    }

    private void UpdateWaveEffect()
    {
        if (baseVertices == null || torusMesh == null) return;
        
        Vector3[] vertices = new Vector3[baseVertices.Length];
        
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 vertex = baseVertices[i];
            
            float angle = Mathf.Atan2(vertex.z, vertex.x);
            float wave = Mathf.Sin(angle * waveFrequency + Time.time * waveSpeed + timeOffset) * waveAmplitude;
            
            vertices[i] = vertex + Vector3.up * wave;
        }
        
        torusMesh.vertices = vertices;
        torusMesh.RecalculateNormals();
    }

    private void CreateHologramMaterial()
    {
        hologramMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        hologramMaterial.SetFloat("_Surface", 1);
        hologramMaterial.SetFloat("_Blend", 0);
        hologramMaterial.SetFloat("_AlphaClip", 0);
        hologramMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        hologramMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        hologramMaterial.SetFloat("_ZWrite", 0);
        hologramMaterial.SetFloat("_Cull", 0);
        hologramMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        hologramMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        hologramMaterial.renderQueue = 3000;
        
        hologramMaterial.SetColor("_BaseColor", baseColor);
        hologramMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        hologramMaterial.EnableKeyword("_EMISSION");
        hologramMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }

    private void UpdateMaterialPulse()
    {
        if (hologramMaterial == null) return;
        
        float pulse = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed + timeOffset) + 1f) * 0.5f);
        
        Color color = baseColor;
        color.a = pulse;
        hologramMaterial.SetColor("_BaseColor", color);
        
        float emissionPulse = Mathf.Lerp(0.5f, 1f, (Mathf.Sin(Time.time * pulseSpeed * 1.2f + timeOffset) + 1f) * 0.5f);
        hologramMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity * emissionPulse);
    }

    public void DestroyBase()
    {
        Debug.Log($"[HologramAura] Destroying base GameObject '{transform.gameObject.name}' and all children");
        Destroy(transform.gameObject);
    }
    
    private void DestroyTorus()
    {
        if (torusObject != null)
        {
            Destroy(torusObject);
            torusObject = null;
            meshFilter = null;
            meshRenderer = null;
        }
        
        if (hologramMaterial != null)
        {
            Destroy(hologramMaterial);
            hologramMaterial = null;
        }
        
        if (torusMesh != null)
        {
            Destroy(torusMesh);
            torusMesh = null;
        }
        
        baseVertices = null;
    }

    private void OnDestroy()
    {
        if (torusObject != null)
        {
            Debug.Log("[HologramAura] Component destroyed - Cleaning up torus resources");
        }
        
        if (hologramMaterial != null)
        {
            Destroy(hologramMaterial);
        }
        
        if (torusMesh != null)
        {
            Destroy(torusMesh);
        }
    }
}
