using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CharacterSelector : MonoBehaviour
{
    [Header("Character Prefabs")]
    public GameObject lluviaPrefab;
    public GameObject aguitaPrefab;

    [Header("Spawn Position")]
    public Transform spawnPoint;

    private const float BELT_RADIUS = 2f;
    private const float BELT_HEIGHT = 2f;
    private const float BELT_THICKNESS = 0.1f;
    private const float BELT_ROTATION_SPEED = 10f;
    private static readonly Color BELT_GLOW_COLOR = Color.yellow;
    private const float BELT_EMISSION_INTENSITY = 5f;
    private const int TOTAL_TEXT_INSTANCES = 3;
    private const float TEXT_OFFSET_FROM_BAND = 0.15f;
    private const float TEXT_SIZE = 0.3f;
    private const float BAND_Y_OFFSET = -1.3f;
    private const float TEXT_Z_OFFSET = 0f;
    private const float TEXT_Y_OFFSET = 0.6f;
    private const float BAND_SEPARATION = 1.0f;
    private const float TORUS_CROSS_SECTION_RADIUS = 0.05f;
    private const int TORUS_SEGMENTS = 32;
    private const int TORUS_RING_SEGMENTS = 16;

    private persistanceData characterData;
    private persistanceData.Character selectedCharacter = persistanceData.Character.none;
    private GameObject currentCharacterInstance;
    private GameObject currentBeltInstance;

    private bool modelRotation = false;
    private float timeForRotation = 0f;

    private void Awake()
    {
        characterData = Resources.Load<persistanceData>("persistanceData");
    }

    private void Update()
    {
        if (modelRotation && currentCharacterInstance != null)
        {
            if (timeForRotation <= 2f)
            {
                RotateModel(currentCharacterInstance);
            }
            else
            {
                modelRotation = false;
            }
        }

        if (currentBeltInstance != null)
        {
            currentBeltInstance.transform.Rotate(Vector3.up, BELT_ROTATION_SPEED * Time.deltaTime);
        }
    }

    private void SelectCharacter()
    {
        if (currentCharacterInstance != null)
        {
            Destroy(currentCharacterInstance);
        }

        if (currentBeltInstance != null)
        {
            Destroy(currentBeltInstance);
        }

        GameObject prefabToInstantiate = null;
        if (selectedCharacter == persistanceData.Character.lluvia)
        {
            prefabToInstantiate = lluviaPrefab;
        }
        else if (selectedCharacter == persistanceData.Character.aguita)
        {
            prefabToInstantiate = aguitaPrefab;
        }

        if (prefabToInstantiate != null && spawnPoint != null)
        {

            currentCharacterInstance = Instantiate(prefabToInstantiate, spawnPoint.position + new Vector3(0f, 0.4f, 0f), Quaternion.Euler(0f, 200f, 0f));

            Renderer instanceRenderer = currentCharacterInstance.GetComponentInChildren<Renderer>();
            Material instanceMaterial = new Material(instanceRenderer.material);

            instanceMaterial.SetColor("_EmissionColor", new Color(0.55f, 0.55f, 0.55f));

            instanceRenderer.material = instanceMaterial;

            Animator anim = currentCharacterInstance.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("selected");
            }

            var interactable = currentCharacterInstance.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

            if (interactable != null)
            {
                interactable.activated = new ActivateEvent();
                interactable.activated.AddListener((args) => { GameStart(); });
            }

            CreateGlowingBelt();
            ActivateRotation();
        }
    }

    public void LluviaSelected()
    {
        selectedCharacter = persistanceData.Character.lluvia;
        SelectCharacter();
    }

    public void AguitaSelected()
    {
        selectedCharacter = persistanceData.Character.aguita;
        SelectCharacter();
    }

    public void GameStart()
    {
        if (characterData != null)
        {
            characterData.changeCharacter(selectedCharacter);
            if (selectedCharacter != persistanceData.Character.none)
            {
                GameObject sceneManager = GameObject.Find("SceneManager");
                if (sceneManager != null)
                {
                    LoadingScreen loadingScreen = sceneManager.GetComponent<LoadingScreen>();
                    if (loadingScreen != null)
                    {
                        loadingScreen.LoadScene(1);
                    }
                }
            }
        }
    }

    private void RotateModel(GameObject objectToRotate)
    {
        objectToRotate.transform.Rotate(Vector3.up, 100f * Time.deltaTime);
        timeForRotation += Time.deltaTime;
    }

    private void ActivateRotation()
    {
        timeForRotation = 0f;
        modelRotation = true;
    }

    private void CreateGlowingBelt()
    {
        currentBeltInstance = new GameObject("GlowingBelt");
        currentBeltInstance.transform.position = currentCharacterInstance.transform.position;
        currentBeltInstance.transform.SetParent(currentCharacterInstance.transform);

        CreateBeltBand(BAND_SEPARATION / 2f);
        CreateBeltBand(-BAND_SEPARATION / 2f);
        
        CreateTextInstances();
    }

    private void CreateBeltBand(float yOffset)
    {
        GameObject band = new GameObject(yOffset > 0 ? "TopBand" : "BottomBand");
        band.transform.SetParent(currentBeltInstance.transform);
        band.transform.localPosition = new Vector3(0, BELT_HEIGHT + yOffset + BAND_Y_OFFSET, 0);
        band.transform.localRotation = Quaternion.identity;

        Mesh torusMesh = CreateTorusMesh(BELT_RADIUS, TORUS_CROSS_SECTION_RADIUS, TORUS_SEGMENTS, TORUS_RING_SEGMENTS);
        
        MeshFilter meshFilter = band.AddComponent<MeshFilter>();
        meshFilter.mesh = torusMesh;

        Material bandMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bandMaterial.SetColor("_BaseColor", BELT_GLOW_COLOR);
        bandMaterial.SetColor("_EmissionColor", BELT_GLOW_COLOR * BELT_EMISSION_INTENSITY);
        bandMaterial.EnableKeyword("_EMISSION");
        bandMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        MeshRenderer renderer = band.AddComponent<MeshRenderer>();
        renderer.material = bandMaterial;
    }

    private Mesh CreateTorusMesh(float majorRadius, float minorRadius, int majorSegments, int minorSegments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Torus";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i <= majorSegments; i++)
        {
            float u = (float)i / majorSegments * 2f * Mathf.PI;
            float cosU = Mathf.Cos(u);
            float sinU = Mathf.Sin(u);

            for (int j = 0; j <= minorSegments; j++)
            {
                float v = (float)j / minorSegments * 2f * Mathf.PI;
                float cosV = Mathf.Cos(v);
                float sinV = Mathf.Sin(v);

                float x = (majorRadius + minorRadius * cosV) * cosU;
                float z = (majorRadius + minorRadius * cosV) * sinU;
                float y = minorRadius * sinV;

                vertices.Add(new Vector3(x, y, z));

                Vector3 normal = new Vector3(cosV * cosU, sinV, cosV * sinU);
                normals.Add(normal.normalized);
            }
        }

        for (int i = 0; i < majorSegments; i++)
        {
            for (int j = 0; j < minorSegments; j++)
            {
                int current = i * (minorSegments + 1) + j;
                int next = current + minorSegments + 1;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(next);
                triangles.Add(next + 1);
                triangles.Add(current + 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void CreateTextInstances()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/OTHorizontalUnlicensedTrial-Thin SDF");
        if (font == null)
        {
            font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Fonts/Omnitype/OTHorizontalUnlicensedTrial-Thin SDF");
        }
        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
            Debug.LogWarning("Thin font not found, using default font.");
        }
        
        if (font == null)
        {
            Debug.LogError("TextMeshPro font asset not found! Text will not render.");
            return;
        }

        float angleStep = 360f / TOTAL_TEXT_INSTANCES;
        float radius = BELT_RADIUS + TEXT_OFFSET_FROM_BAND + TEXT_Z_OFFSET;

        for (int i = 0; i < TOTAL_TEXT_INSTANCES; i++)
        {
            GameObject textObj = new GameObject($"Text_{i}");
            textObj.transform.SetParent(currentBeltInstance.transform);

            float angle = i * angleStep;
            float radians = angle * Mathf.Deg2Rad;
            
            float x = Mathf.Sin(radians) * radius;
            float z = Mathf.Cos(radians) * radius;
            float y = TEXT_Y_OFFSET;

            textObj.transform.localPosition = new Vector3(x, y, z);
            
            Vector3 outwardDirection = new Vector3(x, 0, z).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(outwardDirection);
            textObj.transform.localRotation = lookRotation * Quaternion.Euler(0f, 180f, 0f);
            textObj.transform.localScale = Vector3.one * TEXT_SIZE;

            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = "INICIAR";
            textMesh.font = font;
            textMesh.fontSize = 14;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = BELT_GLOW_COLOR;
            textMesh.fontStyle = FontStyles.Normal;
            textMesh.textWrappingMode = TextWrappingModes.NoWrap;

            textMesh.ForceMeshUpdate();

            if (textMesh.fontSharedMaterial != null)
            {
                Material textMaterial = new Material(textMesh.fontSharedMaterial);
                textMaterial.EnableKeyword("_EMISSION");
                textMaterial.SetColor("_EmissionColor", BELT_GLOW_COLOR * BELT_EMISSION_INTENSITY);
                textMaterial.EnableKeyword("UNDERLAY_ON");
                textMaterial.SetColor("_UnderlayColor", BELT_GLOW_COLOR * BELT_EMISSION_INTENSITY);
                textMaterial.SetFloat("_UnderlayDilate", 0.3f);
                textMaterial.SetFloat("_UnderlaySoftness", 0.3f);
                
                textMesh.fontSharedMaterial = textMaterial;
                textMesh.UpdateMeshPadding();
            }
        }
    }
}
