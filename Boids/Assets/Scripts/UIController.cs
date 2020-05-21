using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public Canvas canvas;
    public BoidController boidController;

    // Main Title Screen Elements
    public Button startButton;
    public Button settingsButton;
    public Button aboutButton;
    public Text titleText;

    public RectTransform aboutPanel;
    public Button aboutCloseButton;

    // Element Groups
    private GameObject[] _titleElements;
    private GameObject[] _settingsElements;
    private GameObject[] _aboutElements;
    private GameObject[] _adjustmentElements;

    private UIStance _currentUI;

    private enum UIStance {
        Title,
        Play,
        Settings,
        About
    }

    private void Start() {
        // Basic variable setup
        _currentUI = UIStance.Title;

        // Add event functions
        startButton.onClick.AddListener(OnStartButton);
        aboutButton.onClick.AddListener(OnAboutButton);
        aboutCloseButton.onClick.AddListener(OnAboutClose);
        
        // settingsButton.onClick.AddListener(OnSettingsButton);

        // Organize all UI Game Objects
        _titleElements = new GameObject[]
            {titleText.gameObject, startButton.gameObject, settingsButton.gameObject};
        _settingsElements = new GameObject[]
            { };
        _aboutElements = new GameObject[] { };
        _adjustmentElements = new GameObject[] { };
    }

    // Handle Start Button Clicking
    private void OnStartButton() {
        switch (_currentUI) {
            case UIStance.Title: {
                Debug.Log("Switching to Play Screen");
                MoveTitleElements(true);
                break;
            }
            // Shouldn't be processed
            case UIStance.Play:
            case UIStance.Settings:
            case UIStance.About:
                Debug.LogWarning($"User clicked Start Button out of Title Screen ({_currentUI})");
                break;
        }
    }

    // Move Title Elements In/Out
    private void MoveTitleElements(bool away) {
        foreach (GameObject element in _titleElements) {
            LeanTween
                .move(element, element.transform.position + new Vector3(0, 350 * (away ? 1 : -1), 0), away ? 0.75f : 1.15f)
                .setDelay(away ? 0f : 0.35f)
                .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic);
        }

        // Bottom Right About Button
        LeanTween
            .move(aboutButton.gameObject, aboutButton.transform.position + new Vector3(100 * (away ? 1 : -1), 0, 0), 0.75f)
            .setEase(LeanTweenType.easeInCubic);
    }

    // Move About Elements In/Out
    private void MoveAboutElements(bool away) {
        LeanTween
            .move(aboutPanel.gameObject, aboutPanel.transform.position + new Vector3(750 * (away ? 1 : -1), 0, 0), away ? 0.6f : 0.8f)
            .setEase(LeanTweenType.easeInCubic);
    }

    // Handle switching to the About Screen
    private void OnAboutButton() {
        if (_currentUI == UIStance.Title) {
            Debug.Log($"Screen Transition: {_currentUI} -> {UIStance.About}");
            MoveAboutElements(false);
            MoveTitleElements(true);
            _currentUI = UIStance.About;
        }
    }

    // Handle returning to the Title Screen, closing the About Screen
    private void OnAboutClose() {
        if (_currentUI == UIStance.About) {
            Debug.Log($"Screen Transition: {_currentUI} -> {UIStance.Title}");
            MoveAboutElements(true);
            MoveTitleElements(false);
            _currentUI = UIStance.Title;
        }
    }

    private void SwitchToTitle() {
        _currentUI = UIStance.Title;
    }

    private void SwitchToAbout() {
        _currentUI = UIStance.About;
    }

    private void SwitchToSettings() {
        _currentUI = UIStance.Settings;
    }

    private void SwitchToPlay() {
        _currentUI = UIStance.Play;
    }
}