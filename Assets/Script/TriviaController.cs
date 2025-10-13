using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Trivia;

namespace Trivia
{
    [Serializable]
    public class Questions
    {
        public int position;
        public string question;
        public bool answer;
    }

}
public class TriviaController : MonoBehaviour
{

    [Header("Questions")]
    public Questions[] q;

    [Header("Animation controllers & buttons")]
    public Animator screenAnimator;
    public Animator keyboardAnimator;
    public GameObject buttonsPanel;

    [Header("Text Display Settings")]
    [TextArea(3, 10)]

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueTextMesh;
    public float typingSpeed = 0.05f;

    [Header("Plane Color Settings")]
    public SkinnedMeshRenderer planeRenderer;
    private Color newColor = Color.red;

    [Header("State Management")]
    private bool isTextVisible = false;
    private bool isTyping = false;
    private bool correctAnswer;
    private Material planeMaterial;
    private bool waitingForChoice = false;
    private bool choiceMade = false;
    private bool userChoice;

    private void Start()
    {
        buttonsPanel.SetActive(false);
        if (planeRenderer != null && planeRenderer.material != null)
        {
            planeMaterial = planeRenderer.material;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void InitiateTrivia()
    {
        StartCoroutine(InitiateTriviaCoroutine());
    }

    private IEnumerator InitiateTriviaCoroutine()
    {
        Debug.Log("Trivia started");
        if (screenAnimator && keyboardAnimator && buttonsPanel)
        {
            screenAnimator.SetTrigger("Appear");
            keyboardAnimator.SetTrigger("Appear");
            buttonsPanel.SetActive(true);
        }

        for (int i = 0; i < q.Length; i++)
        {
            Debug.Log("Question " + q[i] + " asked");
            correctAnswer = q[i].answer;
            ShowText(q[i].question);

            yield return new WaitUntil(() => !isTyping);

            waitingForChoice = true;
            choiceMade = false;

            yield return new WaitUntil(() => choiceMade);

            if (userChoice == correctAnswer)
            {
                Debug.Log("Correct answer");
            }
            else
            {
                Debug.Log("Wrong answer");
            }

            HideText();
            yield return new WaitUntil(() => !isTextVisible);

            yield return new WaitForSeconds(1f);
        }
        Debug.Log("Trivia completed");
    }

    private void ShowText(String text)
    {
        if (!isTyping && text != null && text.Length > 0) StartCoroutine(DisplayTextSequence(text));
    }

    public bool Choice(bool c)
    {
        if (waitingForChoice)
        {
            userChoice = c;
            choiceMade = true;
            waitingForChoice = false;
            Debug.Log($"User chose: {c}");
            return c;
        }
        else throw new Exception("Still waiting for an answer.");
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

    private IEnumerator DisplayTextSequence(String text)
    {
        isTyping = true;
        isTextVisible = true;
        dialoguePanel.SetActive(true);

        yield return StartCoroutine(TypeText(text));

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
