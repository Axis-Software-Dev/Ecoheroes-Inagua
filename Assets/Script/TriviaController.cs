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

    [Header("Player Transportation")]
    [Tooltip("Distance from Televesona to position the player")]
    public float playerDistance = 2f;
    [Tooltip("Height offset for player positioning")]
    public float heightOffset = 0f;
    [Tooltip("Smooth transition duration in seconds")]
    public float transportDuration = 1.5f;
    [Tooltip("If true, makes player face Televesona after transport")]
    public bool faceTv = true;

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
        Debug.Log("Trivia started");
        if (screenAnimator && keyboardAnimator && buttonsPanel)
        {
            screenAnimator.SetTrigger("Appear");
            yield return new WaitForSeconds(4.5f);
            keyboardAnimator.SetTrigger("Appear");
            yield return new WaitForSeconds(1f);
            buttonsPanel.SetActive(true);
        }

        yield return StartCoroutine(TransportPlayer());

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
                ChangePlaneColor(new Color(.52f, .717f, .615f, .6f));
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(new Color(1f, 1f, 1f, 0f));
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(new Color(.52f, .717f, .615f, .6f));
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(new Color(1f, 1f, 1f, 0f));
                Debug.Log("Correct answer");
            }
            else
            {
                PlayTriviaAudio(wrongAnswerAudio);
                ChangePlaneColor(new Color(.639f, .2f, .239f, .6f));
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(new Color(1f, 1f, 1f, 0f));
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(new Color(.639f, .2f, .239f, .6f));
                yield return new WaitForSeconds(.1f);
                ChangePlaneColor(new Color(1f, 1f, 1f, 0f));

                Debug.Log("Wrong answer");

                HideText();
                yield return new WaitUntil(() => !isTextVisible);

                ShowText("Necesitas seguirte preparando para convertirte en un Ecohéroe. Empieza el juego nuevamente y presta más atención para el siguiente turno.");
                yield return new WaitUntil(() => isTextVisible);

                HideText();
                yield return new WaitUntil(() => !isTextVisible);

                buttonsPanel.SetActive(false);
                yield return new WaitForSeconds(1f);
                keyboardAnimator.SetTrigger("Disappear");
                yield return new WaitForSeconds(4.5f);
                screenAnimator.SetTrigger("Disappear");

                UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
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

    private IEnumerator TransportPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform not found. Attempting to find XR Origin again.");
            playerTransform = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>().transform;
            if (playerTransform == null)
            {
                Debug.LogError("Could not transport player: XR Origin not found!");
                yield break;
            }
        }
        Vector3 tvPosition = transform.position;
        Vector3 tvForward = transform.forward;

        Vector3 targetPosition = tvPosition - (tvForward * playerDistance);
        targetPosition.y = tvPosition.y + heightOffset;

        Vector3 startPosition = playerTransform.position;
        Quaternion startRotation = playerTransform.rotation;
        Quaternion targetRotation = startRotation;

        if (faceTv)
        {
            Vector3 directionToTv = (tvPosition - targetPosition).normalized;
            directionToTv.y = 0;
            if (directionToTv != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(directionToTv);
            }
        }

        Debug.Log($"Transporting player from {startPosition} to {targetPosition}");

        float elapsedTime = 0;
        while (elapsedTime < transportDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transportDuration;

            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            Quaternion currentRotation = Quaternion.Lerp(startRotation, targetRotation, smoothProgress);

            playerTransform.position = currentPosition;
            if (faceTv)
            {
                playerTransform.rotation = currentRotation;
            }

            yield return null;
        }

        playerTransform.position = targetPosition;
        if (faceTv)
        {
            playerTransform.rotation = targetRotation;
        }

        Debug.Log("Player transportation completed");
    }

    [ContextMenu("Transport Player to Televesona")]
    public void TransportPlayerManually()
    {
        StartCoroutine(TransportPlayer());
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
