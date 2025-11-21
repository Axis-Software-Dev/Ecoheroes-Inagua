using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    [Header("Character Prefabs")]
    public GameObject lluviaPrefab;
    public GameObject aguitaPrefab;

    [Header("Spawn Position")]
    public Transform spawnPoint;

    // Belt settings
    private const float BELT_RADIUS = 3f;
    private const float BELT_HEIGHT = 2f;
    private const float BELT_THICKNESS = 0.1f;
    private const float BELT_ROTATION_SPEED = 10f;
    private static readonly Color BELT_GLOW_COLOR = Color.yellow;
    private const float BELT_EMISSION_INTENSITY = 5f;
    private const int TOTAL_TEXT_INSTANCES = 3; // Total text instances, not per band
    private const float TEXT_OFFSET_FROM_BAND = 0.15f;
    private const float TEXT_SIZE = 0.15f; // Smaller text
    private const float BAND_Y_OFFSET = -1.3f; // Move both bands down by 1.3f
    private const float TEXT_Z_OFFSET = -2.3f; // Move text closer to cylinders in Z

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
            currentCharacterInstance = Instantiate(prefabToInstantiate, spawnPoint.position + new Vector3(0f, 0.78f, 0f), spawnPoint.rotation);

            Animator anim = currentCharacterInstance.GetComponent<Animator>();
            anim.SetTrigger("selected");

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
        characterData.changeCharacter(selectedCharacter);
        if (selectedCharacter != persistanceData.Character.none)
        {
            GameObject.Find("SceneManager").GetComponent<LoadingScreen>().LoadScene(1);
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

        CreateBeltBand(0.3f);
        CreateBeltBand(-0.3f);
        
        // Create 3 text instances total, separated from the cylinders
        CreateTextInstances();
    }

    private void CreateBeltBand(float yOffset)
    {
        GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = yOffset > 0 ? "TopBand" : "BottomBand";
        band.transform.SetParent(currentBeltInstance.transform);
        band.transform.localPosition = new Vector3(0, BELT_HEIGHT + yOffset + BAND_Y_OFFSET, 0);
        band.transform.localRotation = Quaternion.identity;
        band.transform.localScale = new Vector3(BELT_RADIUS * 2, BELT_THICKNESS, BELT_RADIUS * 2);

        Destroy(band.GetComponent<Collider>());

        // Create neon glowing material
        Material bandMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        bandMaterial.SetColor("_BaseColor", BELT_GLOW_COLOR);
        bandMaterial.SetColor("_EmissionColor", BELT_GLOW_COLOR * BELT_EMISSION_INTENSITY);
        bandMaterial.EnableKeyword("_EMISSION");
        bandMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        MeshRenderer renderer = band.GetComponent<MeshRenderer>();
        renderer.material = bandMaterial;
    }

    private void CreateTextInstances()
    {
        // Get default font asset for TextMeshPro
        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont == null)
        {
            Debug.LogError("TextMeshPro default font asset not found! Text will not render.");
            return;
        }

        // Calculate spacing for 3 text instances total
        float angleStep = 360f / TOTAL_TEXT_INSTANCES;
        // Move text 2.3 units closer to the cylinders (reduce radius)
        float radius = BELT_RADIUS + TEXT_OFFSET_FROM_BAND + TEXT_Z_OFFSET;

        for (int i = 0; i < TOTAL_TEXT_INSTANCES; i++)
        {
            GameObject textObj = new GameObject($"Text_{i}");
            // Parent to belt instance, not the cylinder bands
            textObj.transform.SetParent(currentBeltInstance.transform);

            // Calculate position around the cylinder (like a soup can label)
            float angle = i * angleStep;
            float radians = angle * Mathf.Deg2Rad;
            
            // Position text on the outer surface of the cylinder
            // Text is positioned closer to cylinders due to reduced radius
            float x = Mathf.Sin(radians) * radius;
            float z = Mathf.Cos(radians) * radius;

            textObj.transform.localPosition = new Vector3(x, 0, z);
            
            // Rotate text to face outward from the cylinder surface (soup can label effect)
            // Rotation in X is now 0, not 90
            Vector3 outwardDirection = new Vector3(x, 0, z).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(outwardDirection);
            textObj.transform.localRotation = lookRotation * Quaternion.Euler(0f, 0f, 0f);
            textObj.transform.localScale = Vector3.one * TEXT_SIZE;

            // Create TextMeshPro component
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = "INICIAR";
            textMesh.font = defaultFont;
            textMesh.fontSize = 10; // Smaller font size
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = BELT_GLOW_COLOR;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.textWrappingMode = TextWrappingModes.NoWrap;

            // Force text to update
            textMesh.ForceMeshUpdate();

            // Create glowing emissive material for text (neon effect)
            if (textMesh.fontSharedMaterial != null)
            {
                Material textMaterial = new Material(textMesh.fontSharedMaterial);
                textMaterial.EnableKeyword("_EMISSION");
                textMaterial.SetColor("_EmissionColor", BELT_GLOW_COLOR * BELT_EMISSION_INTENSITY);
                textMaterial.EnableKeyword("UNDERLAY_ON");
                textMaterial.SetColor("_UnderlayColor", BELT_GLOW_COLOR * BELT_EMISSION_INTENSITY);
                textMaterial.SetFloat("_UnderlayDilate", 0.8f);
                textMaterial.SetFloat("_UnderlaySoftness", 0.3f);
                
                textMesh.fontSharedMaterial = textMaterial;
                textMesh.UpdateMeshPadding();
            }
        }
    }
}
