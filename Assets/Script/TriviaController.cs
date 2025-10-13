using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class TelevisionaInteractionController : MonoBehaviour
{
    [Header("Text Display Settings")]
    [TextArea(3, 10)]
    public string displayText = "Hello world";
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueTextMesh;
    public float typingSpeed = 0.05f;

    [Header("Plane Color Settings")]
    public SkinnedMeshRenderer planeRenderer;
    public Color newColor = Color.red;

    [Header("State Management")]
    private bool isTextVisible = false;
    private bool isTyping = false;
    private Material planeMaterial;

    private void Start()
    {
        if (planeRenderer != null && planeRenderer.material != null)
        {
            planeMaterial = planeRenderer.material;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ShowText(String text)
    {
        if (!isTyping && text != null && text.Length > 0)
        {
            displayText = text;
        }
        StartCoroutine(DisplayTextSequence());
    }

    public void HideText()
    {
        if (isTextVisible && !isTyping)
        {
            StartCoroutine(HideTextSequence());
        }
    }

    public void ChangePlaneColor()
    {
        if (planeMaterial != null)
        {
            if (planeMaterial.HasProperty("_BaseColor"))
            {
                planeMaterial.SetColor("_BaseColor", newColor);
            }
            else if (planeMaterial.HasProperty("_Color"))
            {
                planeMaterial.SetColor("_Color", newColor);
            }
            else if (planeMaterial.HasProperty("_MainColor"))
            {
                planeMaterial.SetColor("_MainColor", newColor);
            }

            Debug.Log($"Changed plane color to {newColor}");
        }
        else
        {
            Debug.LogWarning("Plane material is null!");
        }
    }

    private IEnumerator DisplayTextSequence()
    {
        isTyping = true;
        isTextVisible = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        yield return StartCoroutine(TypeText(displayText));

        isTyping = false;
    }

    private IEnumerator HideTextSequence()
    {
        isTyping = true;

        yield return StartCoroutine(UntypeText());

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        isTextVisible = false;
        isTyping = false;
    }

    private IEnumerator TypeText(string textToType)
    {
        if (dialogueTextMesh != null)
        {
            dialogueTextMesh.text = "";

            foreach (char letter in textToType.ToCharArray())
            {
                dialogueTextMesh.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    private IEnumerator UntypeText()
    {
        if (dialogueTextMesh != null)
        {
            while (dialogueTextMesh.text.Length > 0)
            {
                dialogueTextMesh.text = dialogueTextMesh.text.Substring(0, dialogueTextMesh.text.Length - 1);
                yield return new WaitForSeconds(typingSpeed / 4);
            }
        }
    }
}
