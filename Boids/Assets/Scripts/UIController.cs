using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public Canvas canvas;

    public BoidController boidController;

    // Main Title Screen Elements
    public Button startButton;
    public Button settingsButton;
    public Button aboutButton;
    public RectTransform titleText;

    // About Screen Elements
    public RectTransform aboutPanel;
    public Button aboutCloseButton;

    // Adjustment Panel Elements
    public RectTransform adjPanel;
    
    // Settings Panel Elements
    public RectTransform settingsPanel;
    public Button settingsCloseButton;
    
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
        aboutCloseButton.onClick.AddListener(OnAboutCloseButton);
        settingsButton.onClick.AddListener(OnSettingsButton);
        settingsCloseButton.onClick.AddListener(OnSettingsCloseButton);
        
        // Calculate UI position deltas
        _aboutDiff = new Vector3(_canvasRect.size.x * _scaleFactor, 0, 0);
        _aboutButtonDiff = new Vector3(75 * _scaleFactor, 0, 0);
        _titleDiff = new Vector3(0, 450 * _scaleFactor, 0);
        _adjustmentsDiff = new Vector3(adjPanel.rect.size.x * _scaleFactor, 0, 0);
        _settingsDiff = new Vector3(_canvasRect.size.x * _scaleFactor * -1,0, 0);

        // Move UI elements into position
        Vector3 center = canvas.transform.position;
        aboutPanel.transform.position = center + _aboutDiff;
        adjPanel.transform.position = center + new Vector3((_canvasRect.size.x + 10) / 2f *  _scaleFactor, 0, 0) + _adjustmentsDiff / 2f;
        settingsPanel.transform.position = center + _settingsDiff;
        
        // Group Element Instantiation
        _titleElements = new[] {titleText.gameObject, startButton.gameObject, settingsButton.gameObject};
    }

    private void Update() {
        // Exit to the title screen with Escape key
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Debug.Log($"Escape key pressed");
            switch (_currentUI) {
                case UIStance.About:
                    OnAboutCloseButton();
                    break;
                case UIStance.Play:
                    OnPlayCloseButton();
                    break;
                case UIStance.Settings:
                    OnSettingsCloseButton();
                    break;
                case UIStance.Title:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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

    private void MoveAdjustmentElements(bool away) {
        LeanTween
            .move(adjPanel.gameObject, adjPanel.gameObject.transform.position + _adjustmentsDiff * (away ? 1 : -1), 1.15f)
            .setDelay(away ? 0f : 0.15f)
            .setEase(LeanTweenType.easeInOutCubic);
    }

    // Move About Elements In/Out
    private void MoveAboutElements(bool away) {
        LeanTween
            .move(aboutPanel.gameObject, aboutPanel.transform.position + _aboutDiff * (away ? 1 : -1), away ? 0.6f : 1f)
            .setDelay(away ? 0f : 0.40f)
            .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic);
    }

    private void MoveSettingsElements(bool away) {
        LeanTween
            .move(settingsPanel.gameObject, settingsPanel.transform.position + _settingsDiff * (away ? 1 : -1),
                away ? 0.7f : 1.1f)
            .setDelay(away ? 0.05f : 0.15f)
            .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic);
    }

    // Handle switching to the Play Screen
    private void OnStartButton() {
        if (_currentUI == UIStance.Title) {
            Debug.Log($"UI Transition: {_currentUI} -> {UIStance.About}");
            MoveTitleElements(true);
            MoveAdjustmentElements(false);
            _currentUI = UIStance.Play;
        }
    }

    private void OnSettingsButton() {
        if (_currentUI == UIStance.Title) {
            Debug.Log($"UI Transition: {_currentUI} -> {UIStance.Settings}");
            MoveTitleElements(true);
            MoveSettingsElements(false);
            _currentUI = UIStance.Settings;
        }
    }

    private void OnSettingsCloseButton() {
        if (_currentUI == UIStance.Settings) {
            Debug.Log($"UI Transition: {_currentUI} - > {UIStance.Title}");
            MoveTitleElements(false);
            MoveSettingsElements(true);
            _currentUI = UIStance.Title;
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
    private void OnAboutCloseButton() {
        if (_currentUI == UIStance.About) {
            Debug.Log($"Screen Transition: {_currentUI} -> {UIStance.Title}");
            MoveAboutElements(true);
            MoveTitleElements(false);
            _currentUI = UIStance.Title;
        }
    }
    
    // Handle returning to the Title Screen, closing the Play Screen
    private void OnPlayCloseButton() {
        if (_currentUI == UIStance.Play) {
            Debug.Log($"Screen Transition: {_currentUI} -> {UIStance.Title}");
            MoveAdjustmentElements(true);
            MoveTitleElements(false);
            _currentUI = UIStance.Title;
        }
    }
}