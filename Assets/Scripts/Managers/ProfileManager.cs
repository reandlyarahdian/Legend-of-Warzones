using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    [Header("UI Components")]
    [SerializeField] private Image _profileImage;
    [SerializeField] private TMP_InputField _profileInputField;
    [SerializeField] private TMP_Dropdown _profileDropdown;
    [SerializeField] private TMP_Dropdown _profileImageDropdown;
    [SerializeField] private TMP_Text _creationMessageText;
    [SerializeField] private TMP_Text _statsText;
    [SerializeField] private TMP_Text _profileName;

    [SerializeField] private Image _profileImage2;

    [SerializeField] private TMP_Text _profileName2;
    [SerializeField] private GameObject _profile;

    [SerializeField] private TMP_Text _statsText2;

    public Sprite[] _profileSprites; 

    private string _currentProfileName;
    private int _currentProfileScore;
    private string _currentImagePath;

    public string playerName {  get { return _currentProfileName; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        LoadProfilesIntoDropdown();
        LoadProfilesImageDropdown();
        _currentProfileName = PlayerPrefs.GetString("profiles", "").Split(';')[0];
        _currentProfileScore = PlayerPrefs.GetInt(_currentProfileName);
        _currentImagePath = PlayerPrefs.GetString(_currentProfileName + "Image");
        _profileName.text = _currentProfileName;
        _statsText.text = $"{_currentProfileScore}";
        _statsText2.text = $"Token : {_currentProfileScore}";
        _profileImage.sprite = Resources.Load<Sprite>(_currentImagePath);
        _profileName2.text = _currentProfileName;
        _profileImage2.sprite = Resources.Load<Sprite>(_currentImagePath);
        if(_currentProfileName != null)
        {
            _profile.SetActive(false);
        }
        else
        {
            _profile.SetActive(true);
        }
    }

    public void PressProfile()
    {
        _profile.SetActive(true);

        Time.timeScale = 0;
        //Set the clip for effect audio
        AudioManager.PlayMenuSelect();
    }

    public void BackProfile()
    {
        _profile.SetActive(false);

        Time.timeScale = 1;
        //Set the clip for effect audio
        AudioManager.PlayMenuSelect();
    }

    public void CreateProfile()
    {
        string playerName = _profileInputField.text;
        if (string.IsNullOrEmpty(playerName)) return;

        // Save the player's profile
        List<string> profiles = GetProfilesList();
        if (!profiles.Contains(playerName))
        {
            profiles.Add(playerName);
            PlayerPrefs.SetString("profiles", string.Join(";", profiles));
        }

        StartCoroutine(ShowCreationMessage());

        // Reload the dropdown
        LoadProfilesIntoDropdown();

        Debug.Log($"Created and saved profile: {playerName}");
    }
    private List<string> GetProfilesList()
    {
        return new List<string>(PlayerPrefs.GetString("profiles", "").Split(';').Where(profile => !string.IsNullOrEmpty(profile)));
    }
    private IEnumerator ShowCreationMessage()
    {
        _creationMessageText.text = "Profile Created!";
        yield return new WaitForSeconds(3);  // Message stays for 3 seconds
        _creationMessageText.text = "";
    }
    public void LoadProfilesIntoDropdown()
    {
        _profileDropdown.ClearOptions();
        string[] profiles = PlayerPrefs.GetString("profiles", "").Split(';');
        _profileDropdown.AddOptions(new List<string>(profiles));
    }

    public void LoadProfilesImageDropdown()
    {
        _profileImageDropdown.ClearOptions();
        _profileImageDropdown.AddOptions(new List<string>(_profileSprites.Select(x => x.name)));
    }

    public void SelectProfileFromDropdown()
    {
        _currentProfileName = _profileDropdown.options[_profileDropdown.value].text;
        Debug.Log($"Selected profile: {_currentProfileName}");
        _currentProfileScore = PlayerPrefs.GetInt(_currentProfileName);
        _currentImagePath = PlayerPrefs.GetString(_currentProfileName+"Image");
        _profileName.text = _currentProfileName;
        _profileName2.text = _currentProfileName;

        UpdateStatsText();
        UpdateImage();
    }

    public void SelectProfileImageDropdown(TMP_Dropdown _dropdown)
    {
        _profileImage.sprite = _profileSprites[_dropdown.value];
        _profileImage2.sprite = _profileSprites[_dropdown.value];
        _currentImagePath = "Sprites/"+_profileImage.sprite.name;
        UpdateImage();
    }

    public void AddScoreToCurrentProfile(int scoreToAdd)
    {
        Debug.Log($"Adding {scoreToAdd} points to profile: {_currentProfileName}");

        if (!string.IsNullOrEmpty(_currentProfileName))
        {
            _currentProfileScore += scoreToAdd;
            SaveProfile();
        }
    }

    public void SaveProfile()
    {
        if (!string.IsNullOrEmpty(_currentProfileName))
        {
            PlayerPrefs.SetInt(_currentProfileName, _currentProfileScore);
            PlayerPrefs.SetString(_currentProfileName, _currentProfileName);
            PlayerPrefs.SetString(_currentProfileName + "Image", _currentImagePath);
        }
    }
    public void UpdateStatsText()
    {
        // Update statsText to only show total points
        _profileName.text = _currentProfileName;
        _profileName2.text = _currentProfileName;

        _statsText.text = $"{_currentProfileScore}";
        _statsText2.text = $"Token : {_currentProfileScore}";
    }

    public void UpdateImage()
    {
        // Update statsText to only show total points
        _profileImage.sprite = Resources.Load<Sprite>(_currentImagePath);
        _profileImage2.sprite = Resources.Load<Sprite>(_currentImagePath);
    }
    public void ClearProfiles()
    {
        PlayerPrefs.DeleteAll();
        _profileDropdown.ClearOptions();
    }
}