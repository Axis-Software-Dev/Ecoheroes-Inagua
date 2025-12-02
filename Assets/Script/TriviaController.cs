using System;
using System.Collections;
using UnityEngine;
using TMPro;
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
        [TextArea(2, 5)]
        public string explanation;
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
    public AudioClip triviaStartAudio;
    public AudioClip questionAudio;
    public AudioClip correctAnswerAudio;
    public AudioClip wrongAnswerAudio;
    public AudioClip triviaCompleteAudio;

    [Header("Audio Clips - UI Sounds")]
    public AudioClip buttonClickAudio;
    public AudioClip clickAudio;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float triviaVolume = 1f;
    [Range(0f, 1f)]
    public float uiVolume = 0.8f;
    public bool enableTypingSounds = true;

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

    private const float SCREEN_APPEAR_DELAY = 4.5f;
    private const float KEYBOARD_APPEAR_DELAY = 1f;
    private const float COLOR_FLASH_DURATION = 0.1f;
    private const float AFTER_CHOICE_DELAY = 1f;
    private const float KEYBOARD_DISAPPEAR_DELAY = 1f;
    private const float SCREEN_DISAPPEAR_DELAY = 2f;
    private const float FINAL_DELAY = 6f;
    private const int MAX_MISTAKES = 2;
    private const int RESTART_SCENE_ID = 0;
    private const float AUDIO_SPATIAL_BLEND = 1f;
    private const float AUDIO_MAX_DISTANCE = 20f;
    private const float UNTYPE_SPEED_MULTIPLIER = 4f;

    private static readonly Color RIGHT_COLOR = new Color(0.52f, 0.717f, 0.615f, 0.2f);
    private static readonly Color WRONG_COLOR = new Color(0.639f, 0.2f, 0.239f, 0.2f);
    private static readonly Color EMISSION_RIGHT = new Color(0.486f, 0.616f, 0.49f);
    private static readonly Color EMISSION_WRONG = new Color(0.61f, 0.223f, 0.174f);
    private static readonly Color TRANSPARENT = new Color(1f, 1f, 1f, 0f);

    private void Start()
    {
        if (buttonsPanel != null)
        {
            buttonsPanel.SetActive(false);
        }

        if (planeRenderer != null && planeRenderer.material != null)
        {
            planeMaterial = planeRenderer.material;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        xrOrigin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            playerTransform = xrOrigin.transform;
        }

        triviaAudioSource = gameObject.AddComponent<AudioSource>();
        triviaAudioSource.spatialBlend = AUDIO_SPATIAL_BLEND;
        triviaAudioSource.rolloffMode = AudioRolloffMode.Linear;
        triviaAudioSource.maxDistance = AUDIO_MAX_DISTANCE;
        triviaAudioSource.volume = triviaVolume;

        if (buttonsPanel != null)
        {
            Button[] buttons = buttonsPanel.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.onClick.AddListener(() => PlayUISound(buttonClickAudio));
                }
            }
        }

        GameObject fluvioObj = GameObject.Find("Fluvi-o");
        if (fluvioObj != null)
        {
            fluvio = fluvioObj.GetComponent<FluvioController>();
        }
    }

    public void InitiateTrivia()
    {
        StartCoroutine(InitiateTriviaCoroutine());
    }

    private IEnumerator InitiateTriviaCoroutine()
    {
        Debug.Log("Trivia started");

        if (screenAnimator != null && keyboardAnimator != null && buttonsPanel != null)
        {
            screenAnimator.SetTrigger("Appear");
            yield return new WaitForSeconds(SCREEN_APPEAR_DELAY);
            keyboardAnimator.SetTrigger("Appear");
            yield return new WaitForSeconds(KEYBOARD_APPEAR_DELAY);
            buttonsPanel.SetActive(true);
        }

        if (q == null)
        {
            yield break;
        }

        for (int i = 0; i < q.Length; i++)
        {
            if (q[i] == null) continue;

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
                FlashPlaneColor(RIGHT_COLOR, EMISSION_RIGHT);
                Debug.Log("Correct answer");
            }
            else
            {
                PlayTriviaAudio(wrongAnswerAudio);
                FlashPlaneColor(WRONG_COLOR, EMISSION_WRONG);

                mistakeCount++;
                Debug.Log($"Wrong answer. Mistakes: {mistakeCount}/{MAX_MISTAKES}");

                HideText();
                yield return new WaitUntil(() => !isTextVisible);

                if (mistakeCount >= MAX_MISTAKES)
                {
                    ShowText("q[i].explanation");
                    yield return new WaitUntil(() => !isTyping);

                    HideText();
                    yield return new WaitUntil(() => !isTextVisible);

                    ShowText("Necesitas seguirte preparando para convertirte en un Ecohéroe. Empieza el juego nuevamente y presta más atención para el siguiente turno.");
                    yield return new WaitUntil(() => !isTyping);

                    HideText();
                    yield return new WaitUntil(() => !isTextVisible);

                    if (buttonsPanel != null)
                    {
                        buttonsPanel.SetActive(false);
                    }

                    yield return new WaitForSeconds(KEYBOARD_DISAPPEAR_DELAY);

                    if (keyboardAnimator != null)
                    {
                        keyboardAnimator.SetTrigger("Disappear");
                    }

                    yield return new WaitForSeconds(SCREEN_DISAPPEAR_DELAY);

                    if (screenAnimator != null)
                    {
                        screenAnimator.SetTrigger("Disappear");
                    }

                    yield return new WaitForSeconds(FINAL_DELAY);

                    GameObject sceneManagerObj = GameObject.Find("SceneManager");
                    if (sceneManagerObj != null)
                    {
                        LoadingScreen loadingScreen = sceneManagerObj.GetComponent<LoadingScreen>();
                        if (loadingScreen != null)
                        {
                            loadingScreen.LoadScene(RESTART_SCENE_ID);
                        }
                    }
                }
                else
                {
                    ShowText(q[i].explanation);
                    yield return new WaitUntil(() => !isTyping);

                    HideText();
                    yield return new WaitUntil(() => !isTextVisible);

                    yield return new WaitForSeconds(AFTER_CHOICE_DELAY);
                }
            }

            HideText();
            yield return new WaitUntil(() => !isTextVisible);

            yield return new WaitForSeconds(AFTER_CHOICE_DELAY);
        }

        Debug.Log("Trivia completed");
        PlayTriviaAudio(triviaCompleteAudio);
        
        if (fluvio != null)
        {
            fluvio.PlayVictorySequence();
        }
    }

    private IEnumerator FlashPlaneColor(Color color, Color emissionColor)
    {
        ChangePlaneColor(color);
        if (planeMaterial != null)
        {
            planeMaterial.EnableKeyword("_EMISSION");
            planeMaterial.SetColor("_EmissionColor", emissionColor * 0f);
        }
        yield return new WaitForSeconds(COLOR_FLASH_DURATION);

        ChangePlaneColor(TRANSPARENT);
        if (planeMaterial != null)
        {
            planeMaterial.DisableKeyword("_EMISSION");
        }
        yield return new WaitForSeconds(COLOR_FLASH_DURATION);

        ChangePlaneColor(color);
        if (planeMaterial != null)
        {
            planeMaterial.EnableKeyword("_EMISSION");
            planeMaterial.SetColor("_EmissionColor", emissionColor * 0f);
        }
        yield return new WaitForSeconds(COLOR_FLASH_DURATION);

        ChangePlaneColor(TRANSPARENT);
        if (planeMaterial != null)
        {
            planeMaterial.DisableKeyword("_EMISSION");
        }
    }

    private void PlayTriviaAudio(AudioClip clip)
    {
        if (clip != null && triviaAudioSource != null)
        {
            triviaAudioSource.volume = triviaVolume;
            triviaAudioSource.PlayOneShot(clip);
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

    private void ShowText(string text)
    {
        if (!isTyping && !string.IsNullOrEmpty(text))
        {
            StartCoroutine(DisplayTextSequence(text));
        }
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
        }
        else
        {
            PlayUISound(buttonClickAudio);
        }
    }

    public void HideText()
    {
        if (isTextVisible && !isTyping)
        {
            StartCoroutine(HideTextSequence());
        }
    }

    private void ChangePlaneColor(Color color)
    {
        if (planeMaterial == null) return;

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
    }

    private IEnumerator DisplayTextSequence(string text)
    {
        isTyping = true;
        isTextVisible = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

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
        if (dialogueTextMesh == null || string.IsNullOrEmpty(textToType))
        {
            yield break;
        }

        dialogueTextMesh.text = "";

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueTextMesh.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator UntypeText()
    {
        if (dialogueTextMesh == null)
        {
            yield break;
        }

        while (!string.IsNullOrEmpty(dialogueTextMesh.text))
        {
            dialogueTextMesh.text = dialogueTextMesh.text.Substring(0, dialogueTextMesh.text.Length - 1);
            yield return new WaitForSeconds(typingSpeed / UNTYPE_SPEED_MULTIPLIER);
        }
    }
}
