using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Trivia;
using Fluvio;

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

    [Header("Audio")]
    public AudioSource triviaAudioSource;

    [Header("Audio Clips")]
    [Tooltip("Audio played when trivia starts")]
    public AudioClip triviaStartAudio;
    [Tooltip("Audio played when asking questions")]
    public AudioClip questionAudio;
    public AudioClip correctAnswerAudio;
    public AudioClip wrongAnswerAudio;
    public AudioClip triviaCompleteAudio;

    [Header("Audio Clips - UI Sounds")]
    public AudioClip buttonClickAudio;
    public AudioClip clickAudio;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume for trivia audio")]
    public float triviaVolume = 1f;
    [Range(0f, 1f)]
    [Tooltip("Volume for UI sound effects")]
    public float uiVolume = 0.8f;
    [Tooltip("Play typing sound for each character")]
    public bool enableTypingSounds = true;

    [Header("State Management")]
    private bool isTextVisible = false;
    private bool isTyping = false;
    private bool correctAnswer;
    private Material planeMaterial;
    private bool waitingForChoice = false;
    private bool choiceMade = false;
    private bool userChoice;
    private Transform playerTransform;
    private Unity.XR.CoreUtils.XROrigin xrOrigin;
    private FluvioController fluvio;
    private int mistakeCount = 0;

    private void Start()
    {
        buttonsPanel.SetActive(false);
        if (planeRenderer != null && planeRenderer.material != null)
        {
            planeMaterial = planeRenderer.material;
            Debug.Log("Plane material set to " + planeMaterial);
        }
        else Debug.Log("Can't set plane material, material is empty");

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        xrOrigin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null) playerTransform = xrOrigin.transform;
        else Debug.LogWarning("TriviaController: Could not find XR Origin. Player transportation will not work.");

        triviaAudioSource = gameObject.AddComponent<AudioSource>();
        Debug.Log("TriviaController: Created AudioSource component");
        triviaAudioSource.spatialBlend = 1f;
        triviaAudioSource.rolloffMode = AudioRolloffMode.Linear;
        triviaAudioSource.maxDistance = 20f;
        triviaAudioSource.volume = triviaVolume;
        Button[] buttons = buttonsPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => PlayUISound(buttonClickAudio));
        }
        fluvio = GameObject.Find("Fluvi-o").GetComponent<FluvioController>();
    }

    public void InitiateTrivia()
    {
        StartCoroutine(InitiateTriviaCoroutine());
    }

    private IEnumerator InitiateTriviaCoroutine()
    {
        Color rightColor = new Color(.52f, .717f, .615f, .2f);
        Color wrongColor = new Color(.639f, .2f, .239f, .2f);
        Color emisionRight = new Color(.486f, .616f, .49f);
        Color emisionWrong = new Color(.61f, .223f, .174f);
        Color transparent = new Color(1f, 1f, 1f, 0f);

        Debug.Log("Trivia started");
        if (screenAnimator && keyboardAnimator && buttonsPanel)
        {
            screenAnimator.SetTrigger("Appear");
            yield return new WaitForSeconds(4.5f);
            keyboardAnimator.SetTrigger("Appear");
            yield return new WaitForSeconds(1f);
            buttonsPanel.SetActive(true);
        }

        for (int i = 0; i < q.Length; i++)
        {
            Debug.Log("Question " + q[i].position + " asked");
            correctAnswer = q[i].answer;
            PlayTriviaAudio(questionAudio);
            ShowText(q[i].question);

            yield return new WaitUntil(() => !isTyping);

            waitingForChoice = true;
            choiceMade = false;

            yield return new WaitUntil(() => choiceMade);

            if (userChoice == correctAnswer)
            {
                PlayTriviaAudio(correctAnswerAudio);
                ChangePlaneColor(rightColor);
                planeMaterial.EnableKeyword("_EMISSION");
                planeMaterial.SetColor("_EmissionColor", emisionRight * 0f);
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(transparent);
                planeMaterial.DisableKeyword("_EMISSION");
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(rightColor);
                planeMaterial.EnableKeyword("_EMISSION");
                planeMaterial.SetColor("_EmissionColor", emisionRight * 0f);
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(transparent);
                planeMaterial.DisableKeyword("_EMISSION");
                Debug.Log("Correct answer");
            }
            else
            {
                PlayTriviaAudio(wrongAnswerAudio);
                ChangePlaneColor(wrongColor);
                planeMaterial.EnableKeyword("_EMISSION");
                planeMaterial.SetColor("_EmissionColor", emisionWrong * 0f);
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(transparent);
                planeMaterial.DisableKeyword("_EMISSION");
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(wrongColor);
                planeMaterial.EnableKeyword("_EMISSION");
                planeMaterial.SetColor("_EmissionColor", emisionWrong * 0f);
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(transparent);
                planeMaterial.DisableKeyword("_EMISSION");

                mistakeCount++;
                Debug.Log($"Wrong answer. Mistakes: {mistakeCount}/2");

                HideText();
                yield return new WaitUntil(() => !isTextVisible);

                if (mistakeCount >= 2)
                {
                    ShowText("Necesitas seguirte preparando para convertirte en un Ecohéroe. Empieza el juego nuevamente y presta más atención para el siguiente turno.");
                    yield return new WaitUntil(() => !isTyping);

                    HideText();
                    yield return new WaitUntil(() => !isTextVisible);

                    buttonsPanel.SetActive(false);
                    yield return new WaitForSeconds(1f);
                    keyboardAnimator.SetTrigger("Disappear");
                    yield return new WaitForSeconds(2f);
                    screenAnimator.SetTrigger("Disappear");
                    yield return new WaitForSeconds(6f);

                    GameObject.Find("SceneManager").GetComponent<LoadingScreen>().LoadScene(0);
                }
                else
                {
                    ShowText("Esa respuesta no es correcta. ¡Cuidado! Tienes una oportunidad más.");
                    yield return new WaitUntil(() => !isTyping);

                    HideText();
                    yield return new WaitUntil(() => !isTextVisible);

                    yield return new WaitForSeconds(1f);
                }
            }

            HideText();
            yield return new WaitUntil(() => !isTextVisible);

            yield return new WaitForSeconds(1f);
        }
        Debug.Log("Trivia completed");
        PlayTriviaAudio(triviaCompleteAudio);
        fluvio.PlayVictorySequence();
    }

    private void PlayTriviaAudio(AudioClip clip)
    {
        if (clip != null && triviaAudioSource != null)
        {
            triviaAudioSource.volume = triviaVolume;
            triviaAudioSource.PlayOneShot(clip);
            Debug.Log($"Playing trivia audio: {clip.name}");
        }
    }

    private void PlayUISound(AudioClip clip)
    {
        if (clip != null && triviaAudioSource != null)
        {
            float originalVolume = triviaAudioSource.volume;
            triviaAudioSource.volume = uiVolume;
            triviaAudioSource.PlayOneShot(clip);
            triviaAudioSource.volume = originalVolume;
        }
    }

    private void ShowText(String text)
    {
        if (!isTyping && text != null && text.Length > 0) StartCoroutine(DisplayTextSequence(text));
    }

    public void Choice(bool c)
    {
        if (waitingForChoice)
        {
            userChoice = c;
            choiceMade = true;
            waitingForChoice = false;
            Debug.Log($"User chose: {c}");
            PlayUISound(buttonClickAudio);
            return;
        }
        else
        {
            PlayUISound(buttonClickAudio);
            throw new Exception("Question still onscreen.");
        }
    }

    public void HideText()
    {
        if (isTextVisible && !isTyping)
        {
            StartCoroutine(HideTextSequence());
        }
    }

    public void ChangePlaneColor(Color color)
    {
        if (planeMaterial != null)
        {
            if (planeMaterial.HasProperty("_BaseColor"))
            {
                planeMaterial.SetColor("_BaseColor", color);
            }
            else if (planeMaterial.HasProperty("_Color"))
            {
                planeMaterial.SetColor("_Color", color);
            }
            else if (planeMaterial.HasProperty("_MainColor"))
            {
                planeMaterial.SetColor("_MainColor", color);
            }
            Debug.Log($"Changed plane color to {color}");
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
