using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class SpeechToTextManager : MonoBehaviour, ISpeechToTextListener
{
    [Header("Table And Prefab")] public GameObject TableContentGo;
    public GameObject DataEntryPrefab;
    public Dictionary<string, GameObject> ExistingEntries = new Dictionary<string, GameObject>();
    public Slider VoiceRecordDurationSlider;
    public bool OnStartRecording;
    [Header("AR stuff")]
    public GameObject tutorialPanel;
    [SerializeField] private ARSession arSession;

    [Space(10)] public TMP_Text SpeechText;

    public Button
        //StartSpeechToTextButton,
        //StopSpeechToTextButton,
        StartSpeechToTextButtonKeyWord;

    public Slider VoiceLevelSlider;
    public bool PreferOfflineRecognition;
    private float normalizedVoiceLevel;

    [Header("For voice recognition")]
    public string[] customKeywords = new string[] { };
    bool onPartialResultReceivedSuccessParse = false;


    void Start()
    {
        SpeechToText.Initialize("en-US");
        SpeechToText.RequestPermissionAsync(permission =>
        {
            if (permission == SpeechToText.Permission.Granted)
            {
                Debug.Log("Permission granted");
            }
            else
            {
                Debug.Log("Permission denied");
            }
        });
        SpeechToText.Initialize("en-US");

        CheckARState();
#if !UNITY_EDITOR&&UNITY_IOS
	   StartSpeechToTextButtonKeyWord.GetComponent<ButtonConfigScript>().onStartSpeech.AddListener(StartSpeechToTextKeyword);
      StartSpeechToTextButtonKeyWord.GetComponent<ButtonConfigScript>().onStopSpeech.AddListener(StopSpeechToText);
#endif
#if UNITY_EDITOR
        StartSpeechToTextButtonKeyWord.GetComponent<ButtonConfigScript>().onStartSpeech.AddListener(StartSpeechToText);
        StartSpeechToTextButtonKeyWord.GetComponent<ButtonConfigScript>().onStopSpeech.AddListener(StopSpeechToText);
#endif
    }

    void OnEnable()
    {
        ARSession.stateChanged += OnARSessionStateChanged;

    }

    void OnDisable()
    {
        ARSession.stateChanged -= OnARSessionStateChanged;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20, 200, 1000, 60), $"Position: {Camera.main.transform.position}",new GUIStyle(GUI.skin.label) { fontSize = 48, normal = { textColor = Color.white } });
        GUI.Label(new Rect(20, 260, 1000, 60),$"Rotation: {Camera.main.transform.eulerAngles}",new GUIStyle(GUI.skin.label) { fontSize = 48, normal = { textColor = Color.white } });
    }

    private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
    {
        Debug.Log($"AR Session 狀態變更：{args.state}");
        if (args.state == ARSessionState.SessionTracking)
        {
            Debug.Log("AR 已完全初始化並正常追蹤！");
            tutorialPanel.SetActive(false);
        }
    }

    private void Update()
    {
        //StartSpeechToTextButton.interactable = SpeechToText.IsServiceAvailable( PreferOfflineRecognition ) && !SpeechToText.IsBusy();
        //StartSpeechToTextButtonKeyWord.interactable = SpeechToText.IsServiceAvailable( PreferOfflineRecognition ) && !SpeechToText.IsBusy();
        //StopSpeechToTextButton.interactable = SpeechToText.IsBusy();

        // You may also apply some noise to the voice level for a more fluid animation (e.g. via Mathf.PerlinNoise)
        VoiceLevelSlider.value = Mathf.Lerp(VoiceLevelSlider.value, normalizedVoiceLevel, 15f * Time.unscaledDeltaTime);
        if (OnStartRecording)
        {
            VoiceRecordDurationSlider.value += 0.1f * Time.deltaTime;

        }
        else
        {
            VoiceRecordDurationSlider.value = 0;
        }

        if (VoiceRecordDurationSlider.value >= 1)
        {
            StopSpeechToText();
        }
    }

    private void CheckARState()
    {
        switch (ARSession.state)
        {
            case ARSessionState.None:
            case ARSessionState.Unsupported:
                Debug.Log("AR 未初始化或設備唔支援 AR");
                break;
            case ARSessionState.CheckingAvailability:
                Debug.Log("檢查 AR 可用性中...");
                break;
            case ARSessionState.NeedsInstall:
                Debug.Log("需要安裝 AR 軟件（例如 ARKit）");
                break;
            case ARSessionState.Installing:
                Debug.Log("安裝 AR 軟件中...");
                break;
            case ARSessionState.Ready:
                Debug.Log("AR 已準備好，但未啟動");
                break;
            case ARSessionState.SessionInitializing:
                Debug.Log("AR Session 正在初始化...");
                break;
            case ARSessionState.SessionTracking:
                Debug.Log("AR 已成功初始化並追蹤中！");
                break;
        }
    }

    public void ChangeLanguage(string preferredLanguage)
    {
        if (!SpeechToText.Initialize(preferredLanguage))
            SpeechText.text = "Couldn't initialize with language: " + preferredLanguage;
    }

    public void StartSpeechToText()
    {
        SpeechToText.RequestPermissionAsync((permission) =>
        {
            if (permission == SpeechToText.Permission.Granted)
            {
                if (SpeechToText.Start(this, false, preferOfflineRecognition: PreferOfflineRecognition))
                {
                    SpeechText.text = "";
                    OnStartRecording = true;
                }

                else
                    SpeechText.text = "Couldn't start speech recognition session!";
            }
            else
                SpeechText.text = "Permission is denied!";
        });
    }
#if UNITY_IOS
    public void StartSpeechToTextKeyword()
    {
        SpeechToText.RequestPermissionAsync((permission) =>
        {
            if (permission == SpeechToText.Permission.Granted)
            {
#if !UNITY_EDITOR
				if( SpeechToText.StartWithKeywords( this,false ,preferOfflineRecognition: PreferOfflineRecognition , customKeywords) )
                {
                SpeechText.text = "";
		        OnStartRecording = true;
                }

				else
					SpeechText.text = "Couldn't start speech recognition session!";
#endif
            }
            else
                SpeechText.text = "Permission is denied!";
        });
    }
#endif

    public void StopSpeechToText()
    {
        SpeechToText.ForceStop();
        OnStartRecording = false;
    }

    public (string processedText, string replacedValue) ReformatTextAfterStop(string text)
    {
        var numberMap = new Dictionary<string, string>()
        {
            { "zero", "0" }, { "one", "1" }, { "two", "2" }, { "three", "3" },
            { "four", "4" }, { "five", "5" }, { "six", "6" }, { "seven", "7" },
            { "eight", "8" }, { "nine", "9" }, { "ten", "10" }
        };
        var oftenMisheardMap = new Dictionary<string, string>()
        {
            { "to", "2" }, { "too", "2" }, { "for", "4" },
            { "ate", "8" }, { "free", "3" }, { "tree", "3" }, { "sex", "6" },
            { "sick", "6" }, { "won", "1" }, { "freight", "3" }, { "fate", "8" },
            { "nite", "9" }, { "night", "9" }
        };
        var oftenMisheardMapForProductName = new Dictionary<string, string>()
        {
            { "parent", "baron" },
            { "barren", "baron" },
            { "barron", "baron" },
            { "ark", "arc" },
            { "arch", "arc" },
            { "art", "arc" },
            { "our", "arc" },
            { "par", "bar" },
            { "brown", "crown" },
            { "clown", "crown" },
            { "partop", "bartop" },
            { "bar tap", "bartop" },
            { "he licks", "helix" },
            { "healix", "helix" },
            { "plus", "+" },
            { "extee", "xt" },
            { "ecstee", "xt" },
            { "macs", "max" },
            { "macks", "max" },
            { "mar sex", "marsx" },
            { "mars ex", "marsx" },
            { "mar six", "marsx" },
            { "marsex", "marsx" },
            { "flecks", "flex" },
            { "flicks", "flex" },
            { "plant", "slant" },
            { "slent", "slant" }
        };

        string replacedValue = "";
        string cleanProductName = text; // Keep original casing for product names
        bool autoCorrectForMisheard = false;

        // First, try to find English number words
        foreach (var pair in numberMap)
        {
            // Use case-insensitive matching directly on original text
            var matches = Regex.Matches(cleanProductName, @"\b" + pair.Key + @"\b", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                // Store the digit value
                replacedValue = pair.Value;

                // Remove the number word from the text for clean product name
                cleanProductName = Regex.Replace(cleanProductName, @"\b" + pair.Key + @"\b", "", RegexOptions.IgnoreCase).Trim();
                // Avoid multiple spaces after removal
                cleanProductName = Regex.Replace(cleanProductName, @"\s+", " ");
                break;
            }
        }

        // Then try often misheard words for number (only if no number found yet)
        if (string.IsNullOrEmpty(replacedValue))
        {
            foreach (var pair in oftenMisheardMap)
            {
                var matches = Regex.Matches(cleanProductName, @"\b" + pair.Key + @"\b", RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    replacedValue = pair.Value;

                    // Remove the misheard word from the text for clean product name
                    cleanProductName = Regex.Replace(cleanProductName, @"\b" + Regex.Escape(pair.Key) + @"\b", "",
                        RegexOptions.IgnoreCase).Trim();
                    // Avoid multiple spaces after removal
                    cleanProductName = Regex.Replace(cleanProductName, @"\s+", " ");
                    autoCorrectForMisheard = true;
                    break;
                }
            }
        }

        // If no English number words found, look for actual digits
        if (string.IsNullOrEmpty(replacedValue))
        {
            // Look for standalone digits (0-9) or numbers (10, 11, etc.)
            var digitMatch = Regex.Match(cleanProductName, @"\b(\d+)\b");
            if (digitMatch.Success)
            {
                replacedValue = digitMatch.Value;

                // Remove the digit from the text for clean product name
                cleanProductName = Regex.Replace(cleanProductName, @"\b" + Regex.Escape(digitMatch.Value) + @"\b", "").Trim();
                // Avoid multiple spaces after removal
                cleanProductName = Regex.Replace(cleanProductName, @"\s+", " ");
            }
        }

        // Try often misheard word for product name (after number extraction)
        foreach (var pair in oftenMisheardMapForProductName)
        {
            var matches = Regex.Matches(cleanProductName, @"\b" + pair.Key + @"\b", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                cleanProductName = Regex.Replace(cleanProductName, @"\b" + pair.Key + @"\b", pair.Value, RegexOptions.IgnoreCase).Trim();
                cleanProductName = Regex.Replace(cleanProductName, @"\s+", " ");
                autoCorrectForMisheard = true;
                break; // Added missing break statement
            }
        }

        // Perform check on the product name if it is within the keywords list
        if (customKeywords.Length > 0 && !string.IsNullOrEmpty(replacedValue))
        {
            // Check if the clean product name matches any of the custom keywords (case-insensitive)
            bool isValidKeyword = customKeywords.Any(keyword => keyword.ToLower() == cleanProductName.ToLower());
            if (!isValidKeyword)
            {
                SpeechText.text = "Product name '" + cleanProductName + "' not recognized in keywords list.";
                return ("", "");
            }
        }

        // Show debug info
        if (!string.IsNullOrEmpty(replacedValue))
        {
            if (autoCorrectForMisheard)
            {
                SpeechText.text = cleanProductName + " " + replacedValue + " →  (auto-corrected for misheard word)";
            }
            else
            {
                SpeechText.text = cleanProductName + " " + replacedValue;
            }
        }
        else
        {
            SpeechText.text = text + " → No quantity found";
        }

        // Return clean product name (for table) and quantity separately
        return (cleanProductName, replacedValue);
    }

    void ISpeechToTextListener.OnReadyForSpeech()
    {
        Debug.Log("OnReadyForSpeech");
    }

    void ISpeechToTextListener.OnBeginningOfSpeech()
    {
        Debug.Log("OnBeginningOfSpeech");
    }

    void ISpeechToTextListener.OnVoiceLevelChanged(float normalizedVoiceLevel)
    {
        // Note that On Android, voice detection starts with a beep sound and it can trigger this callback. You may want to ignore this callback for ~0.5s on Android.
        this.normalizedVoiceLevel = normalizedVoiceLevel;
    }

    void ISpeechToTextListener.OnPartialResultReceived(string spokenText)
    {
        Debug.Log("OnPartialResultReceived: " + spokenText);
        SpeechText.text = spokenText;

        var (processedText, replacedValue) = ReformatTextAfterStop(spokenText);

        // Check if we found a valid number first
        if (string.IsNullOrEmpty(replacedValue))
        {
            // No quantity found, don't try to add to table
            return;
        }

        var successParse = int.TryParse(replacedValue, out int qty);
        if (successParse)
        {
            onPartialResultReceivedSuccessParse = true;
            AddDataEntryToTable(processedText, qty);
        }
        else
        {
            SpeechText.text = "Failed to parse quantity. Please try again.";
        }
    }

    void ISpeechToTextListener.OnResultReceived(string spokenText, int? errorCode)
    {
        Debug.Log("OnResultReceived: " + spokenText + (errorCode.HasValue ? (" --- Error: " + errorCode) : ""));
        normalizedVoiceLevel = 0f;

        if (onPartialResultReceivedSuccessParse)
        {
            onPartialResultReceivedSuccessParse = false;
            return;
        }

        var (processedText, replacedValue) = ReformatTextAfterStop(spokenText);

        // Check if we found a valid number first
        if (string.IsNullOrEmpty(replacedValue))
        {
            // No quantity found, don't try to add to table
            return;
        }

        var successParse = int.TryParse(replacedValue, out int qty);
        if (successParse)
        {
            AddDataEntryToTable(processedText, qty);
        }
        else
        {
            SpeechText.text = "Failed to parse quantity. Please try again.";
        }

        // Recommended approach:
        // - If errorCode is 0, session was aborted via SpeechToText.Cancel. Handle the case appropriately.
        // - If errorCode is 9, notify the user that they must grant Microphone permission to the Google app and call SpeechToText.OpenGoogleAppSettings.
        // - If the speech session took shorter than 1 seconds (should be an error) or a null/empty spokenText is returned, prompt the user to try again (note that if
        //   errorCode is 6, then the user hasn't spoken and the session has timed out as expected).
    }

    public void AddDataEntryToTable(string spokenText, int quantity)
    {
        if (!string.IsNullOrEmpty(spokenText))
        {
            // Check for existing entry using ContainsKey to avoid exceptions
            if (ExistingEntries.ContainsKey(spokenText))
            {
                var existingDataContent = ExistingEntries[spokenText].GetComponent<DataEntry>();
                existingDataContent.CountText.text =
                    (int.Parse(existingDataContent.CountText.text) + quantity).ToString();
                return;
            }

            // Adding new entry
            GameObject newEntry = Instantiate(DataEntryPrefab, TableContentGo.transform);
            newEntry.GetComponentInChildren<TMP_Text>().text = spokenText;

            // Set the quantity for the new entry
            var newDataContent = newEntry.GetComponent<DataEntry>();
            if (newDataContent != null)
            {
                newDataContent.CountText.text = quantity.ToString();
            }

            ExistingEntries.Add(spokenText, newEntry);
        }
    }
}

