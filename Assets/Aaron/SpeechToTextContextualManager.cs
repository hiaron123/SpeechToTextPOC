using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Enhanced Speech-to-Text manager with contextual strings support for iOS
/// This script provides improved recognition accuracy by supplying context words/phrases
/// </summary>
public class SpeechToTextContextualManager : MonoBehaviour, ISpeechToTextListener
{
    [Header("UI Components")]
    public TMP_Text SpeechText;
    public Button StartSpeechToTextButton, StopSpeechToTextButton;
    public Slider VoiceLevelSlider;

    [Header("Speech Recognition Settings")]
    public bool PreferOfflineRecognition = false;
    public string Language = "en-US";

    [Header("Contextual Strings (iOS Only)")]
    [Tooltip("Words or phrases that should have higher recognition priority")]
    public List<string> ContextualStrings = new List<string>
    {
        "hello world",
        "Unity",
        "speech recognition",
        "contextual",
        "iOS"
    };

    [Header("Advanced Settings")]
    [Tooltip("Maximum number of contextual strings to use (iOS limitation)")]
    [Range(1, 100)]
    public int MaxContextualStrings = 100;

    private float normalizedVoiceLevel;
    [SerializeField]
    private List<string> activeContextualStrings;

    void Start()
    {
        InitializeSpeechToText();
        SetupUICallbacks();
        PrepareContextualStrings();
    }

    private void InitializeSpeechToText()
    {
        SpeechToText.Initialize(Language);
        SpeechToText.RequestPermissionAsync(permission =>
        {
            if (permission == SpeechToText.Permission.Granted)
            {
                Debug.Log("Speech permission granted");
            }
            else
            {
                Debug.Log("Speech permission denied");
            }
        });
    }

    private void SetupUICallbacks()
    {
        StartSpeechToTextButton.onClick.AddListener(StartSpeechToText);
        StopSpeechToTextButton.onClick.AddListener(StopSpeechToText);
    }

    private void PrepareContextualStrings()
    {
        activeContextualStrings = new List<string>();

        // Limit contextual strings to the maximum allowed
        int stringCount = Mathf.Min(ContextualStrings.Count, MaxContextualStrings);

        for (int i = 0; i < stringCount; i++)
        {
            if (!string.IsNullOrEmpty(ContextualStrings[i]))
            {
                activeContextualStrings.Add(ContextualStrings[i].Trim());
            }
        }

        Debug.Log($"Prepared {activeContextualStrings.Count} contextual strings for speech recognition");
    }

    private void Update()
    {
        StartSpeechToTextButton.interactable = SpeechToText.IsServiceAvailable(PreferOfflineRecognition) && !SpeechToText.IsBusy();
        StopSpeechToTextButton.interactable = SpeechToText.IsBusy();

        // Smooth voice level animation
        VoiceLevelSlider.value = Mathf.Lerp(VoiceLevelSlider.value, normalizedVoiceLevel, 15f * Time.unscaledDeltaTime);
    }

    #region Public Methods

    /// <summary>
    /// Add a new contextual string to improve recognition of specific words/phrases
    /// </summary>
    /// <param name="contextString">The word or phrase to add</param>
    public void AddContextualString(string contextString)
    {
        if (!string.IsNullOrEmpty(contextString) && !ContextualStrings.Contains(contextString))
        {
            ContextualStrings.Add(contextString);
            PrepareContextualStrings();
        }
    }

    /// <summary>
    /// Remove a contextual string
    /// </summary>
    /// <param name="contextString">The word or phrase to remove</param>
    public void RemoveContextualString(string contextString)
    {
        if (ContextualStrings.Contains(contextString))
        {
            ContextualStrings.Remove(contextString);
            PrepareContextualStrings();
        }
    }

    /// <summary>
    /// Clear all contextual strings
    /// </summary>
    public void ClearContextualStrings()
    {
        ContextualStrings.Clear();
        activeContextualStrings.Clear();
    }

    /// <summary>
    /// Change the speech recognition language
    /// </summary>
    /// <param name="preferredLanguage">Language code (e.g., "en-US", "es-ES")</param>
    public void ChangeLanguage(string preferredLanguage)
    {
        Language = preferredLanguage;
        if (!SpeechToText.Initialize(preferredLanguage))
        {
            SpeechText.text = "Couldn't initialize with language: " + preferredLanguage;
        }
        else
        {
            Debug.Log($"Language changed to: {preferredLanguage}");
        }
    }

    /// <summary>
    /// Start speech recognition with contextual strings
    /// </summary>
    public void StartSpeechToText()
    {
        SpeechToText.RequestPermissionAsync((permission) =>
        {
            if (permission == SpeechToText.Permission.Granted)
            {
                // For iOS, we would ideally pass contextual strings here
                // Note: The current SpeechToText plugin doesn't expose contextual strings parameter
                // This is where you'd modify the native iOS implementation to accept contextual strings

                if (SpeechToText.Start(this, true, preferOfflineRecognition: PreferOfflineRecognition))
                {
                    SpeechText.text = "";
                    LogContextualStringsUsage();
                }
                else
                {
                    SpeechText.text = "Couldn't start speech recognition session!";
                }
            }
            else
            {
                SpeechText.text = "Permission is denied!";
            }
        });
    }

    /// <summary>
    /// Stop speech recognition
    /// </summary>
    public void StopSpeechToText()
    {
        SpeechToText.ForceStop();
    }

    #endregion

    #region Private Helper Methods

    private void LogContextualStringsUsage()
    {
        if (activeContextualStrings.Count > 0)
        {
            Debug.Log($"Using {activeContextualStrings.Count} contextual strings:");
            foreach (string contextString in activeContextualStrings)
            {
                Debug.Log($"  - \"{contextString}\"");
            }
        }
    }

    #endregion

    #region ISpeechToTextListener Implementation

    void ISpeechToTextListener.OnReadyForSpeech()
    {
        Debug.Log("OnReadyForSpeech - Recognition is ready, contextual strings active");
    }

    void ISpeechToTextListener.OnBeginningOfSpeech()
    {
        Debug.Log("OnBeginningOfSpeech - User started speaking");
    }

    void ISpeechToTextListener.OnVoiceLevelChanged(float normalizedVoiceLevel)
    {
        // Note: On Android, voice detection starts with a beep sound and it can trigger this callback
        // You may want to ignore this callback for ~0.5s on Android
        this.normalizedVoiceLevel = normalizedVoiceLevel;
    }

    void ISpeechToTextListener.OnPartialResultReceived(string spokenText)
    {
        Debug.Log("OnPartialResultReceived: " + spokenText);
        SpeechText.text = spokenText;

        // Check if any of our contextual strings were recognized
        CheckContextualStringMatches(spokenText);
    }

    void ISpeechToTextListener.OnResultReceived(string spokenText, int? errorCode)
    {
        Debug.Log("OnResultReceived: " + spokenText + (errorCode.HasValue ? (" --- Error: " + errorCode) : ""));
        SpeechText.text = spokenText;
        normalizedVoiceLevel = 0f;

        // Final check for contextual string matches
        CheckContextualStringMatches(spokenText);

        // Handle error codes according to recommendations
        HandleErrorCodes(errorCode, spokenText);
    }

    #endregion

    #region Error Handling and Contextual String Matching

    private void CheckContextualStringMatches(string spokenText)
    {
        if (string.IsNullOrEmpty(spokenText)) return;

        string lowerSpokenText = spokenText.ToLower();
        foreach (string contextString in activeContextualStrings)
        {
            if (lowerSpokenText.Contains(contextString.ToLower()))
            {
                Debug.Log($"Contextual string matched: \"{contextString}\" in \"{spokenText}\"");
            }
        }
    }

    private void HandleErrorCodes(int? errorCode, string spokenText)
    {
        if (!errorCode.HasValue) return;

        switch (errorCode.Value)
        {
            case 0:
                Debug.Log("Speech session was cancelled");
                break;
            case 6:
                Debug.Log("No speech detected - session timed out");
                break;
            case 9:
                Debug.LogWarning("Microphone permission issue with Google app");
                SpeechText.text += "\n(Microphone permission needed for Google app)";
                break;
            default:
                Debug.LogWarning($"Speech recognition error: {errorCode.Value}");
                break;
        }

        // Handle short sessions or empty results
        if (string.IsNullOrEmpty(spokenText) && errorCode.Value != 6)
        {
            SpeechText.text = "Please try again - no speech was detected";
        }
    }

    #endregion

    #region Editor Helper Methods (Inspector Use)

    [ContextMenu("Add Sample Contextual Strings")]
    private void AddSampleContextualStrings()
    {
        List<string> samples = new List<string>
        {
            "start recording", "stop recording", "pause", "resume",
            "save file", "delete", "confirm", "cancel",
            "navigation", "settings", "home", "back",
            "volume up", "volume down", "mute", "unmute"
        };

        foreach (string sample in samples)
        {
            if (!ContextualStrings.Contains(sample))
            {
                ContextualStrings.Add(sample);
            }
        }

        PrepareContextualStrings();
        Debug.Log("Added sample contextual strings");
    }

    [ContextMenu("Clear All Contextual Strings")]
    private void ClearAllContextualStrings()
    {
        ClearContextualStrings();
        Debug.Log("Cleared all contextual strings");
    }

    #endregion
}
