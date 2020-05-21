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

    // About Screen Elements
    public RectTransform aboutPanel;
    public Button aboutCloseButton;

    // Element Groups
    private GameObject[] _titleElements;

    private UIStance _currentUI;
    private CanvasScaler _scaler;
    private Rect _canvasRect;
    private float _scaleFactor;

    // Element Displacements
    private Vector3 _aboutDiff;
    private Vector3 _aboutButtonDiff;
    private Vector3 _titleDiff;
    private Vector3 _settingsDiff;
    private Vector3 _adjustmentsDiff;

    private enum UIStance {
        Title,
        Play,
        Settings,
        About
    }

    private void Start() {
        // Basic variable setup
        _currentUI = UIStance.Title;
        _scaler = canvas.GetComponent<CanvasScaler>();
        _canvasRect = canvas.GetComponent<RectTransform>().rect;
        _scaleFactor = Screen.width / _scaler.referenceResolution.x;

        // Add event functions
        startButton.onClick.AddListener(OnStartButton);
        aboutButton.onClick.AddListener(OnAboutButton);
        aboutCloseButton.onClick.AddListener(OnAboutClose);
        // settingsButton.onClick.AddListener(OnSettingsButton);

        // Calculate UI position deltas
        _aboutDiff = new Vector3(_canvasRect.size.x * _scaleFactor, 0, 0);
        _aboutButtonDiff = new Vector3(75 * _scaleFactor, 0, 0);
        _titleDiff = new Vector3(0, 450 * _scaleFactor, 0);

        // Move UI elements into position
        aboutPanel.transform.position = canvas.transform.position + _aboutDiff;

        // Group Element Instantiation
        _titleElements = new[] {titleText.gameObject, startButton.gameObject, settingsButton.gameObject};
    }


    // Move Title Elements In/Out
    private void MoveTitleElements(bool away) {
        // Main Title and Start/Settings Buttons
        foreach (GameObject element in _titleElements) {
            LeanTween
                .move(element, element.transform.position + _titleDiff * (away ? 1 : -1), away ? 0.65f : 1.15f)
                .setDelay(away ? 0f : 0.35f)
                .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic);
        }

        // Bottom Right About Button
        LeanTween
            .move(aboutButton.gameObject, aboutButton.transform.position + _aboutButtonDiff * (away ? 1 : -1), 0.55f)
            .setEase(LeanTweenType.easeInOutCubic);
    }

    // Move About Elements In/Out
    private void MoveAboutElements(bool away) {
        LeanTween
            .move(aboutPanel.gameObject, aboutPanel.transform.position + _aboutDiff * (away ? 1 : -1), away ? 0.6f : 1f)
            .setDelay(away ? 0f : 0.40f)
            .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic);
    }

    // Handle switching to the Play Screen
    private void OnStartButton() {
        if (_currentUI == UIStance.Title) {
            Debug.Log($"UI Transition: {_currentUI} -> {UIStance.About}");
            MoveTitleElements(true);
        }
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
}