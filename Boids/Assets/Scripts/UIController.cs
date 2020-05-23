using System;
using JetBrains.Annotations;
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
    private int _activeTweens;
    private bool _UILock {
        get => _activeTweens > 0;
        set { }
    }

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

    private enum UIGroup {
        TitleScreen,
        AdjustmentsScreen,
        SettingsScreen,
        AboutScreen
    }

    private void Start() {
        // Basic variable setup
        _currentUI = UIStance.Title;
        _scaler = canvas.GetComponent<CanvasScaler>();
        _canvasRect = canvas.GetComponent<RectTransform>().rect;
        _scaleFactor = Screen.width / _scaler.referenceResolution.x;

        // Add stance change functionality to buttons
        startButton.onClick.AddListener(() => ChangeStance(UIStance.Play));
        aboutButton.onClick.AddListener(() => ChangeStance(UIStance.About));
        aboutCloseButton.onClick.AddListener(() => ChangeStance(UIStance.Title));
        settingsButton.onClick.AddListener(() => ChangeStance(UIStance.Settings));
        settingsCloseButton.onClick.AddListener(() => ChangeStance(UIStance.Title));

        // Calculate UI position deltas
        _aboutDiff = new Vector3(_canvasRect.size.x * _scaleFactor, 0, 0);
        _aboutButtonDiff = new Vector3(75 * _scaleFactor, 0, 0);
        _titleDiff = new Vector3(0, 450 * _scaleFactor, 0);
        _adjustmentsDiff = new Vector3(adjPanel.rect.size.x * _scaleFactor, 0, 0);
        _settingsDiff = new Vector3(_canvasRect.size.x * _scaleFactor * -1, 0, 0);

        // Move UI elements into position
        Vector3 center = canvas.transform.position;
        aboutPanel.transform.position = center + _aboutDiff;
        adjPanel.transform.position = center + new Vector3((_canvasRect.size.x + 10) / 2f * _scaleFactor, 0, 0) +
                                      _adjustmentsDiff / 2f;
        settingsPanel.transform.position = center + _settingsDiff;

        // Group Element Instantiation
        _titleElements = new[] {titleText.gameObject, startButton.gameObject, settingsButton.gameObject};
    }

    private void Update() {
        // Exit to the title screen with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
            ChangeStance(UIStance.Title);
    }
    
    
    private void TweenEnd() {
        _activeTweens -= 1;
        if(!_UILock)
            Debug.Log("Unlocking Stance Changes");
    }

    private Action StartTween() {
        if(!_UILock)
            Debug.Log("Locking Stance Changes");
        _activeTweens += 1;
        return TweenEnd;
    }


    private void ChangeStance(UIStance stance) {
        // Quit early if UI is currently "locked" due to an active tween
        if (_UILock || stance == _currentUI)
            return;

        Debug.Log($"UI Transition: {_currentUI} -> {stance}");

        switch (_currentUI) {
            // Title -> Settings/About/Play
            case UIStance.Title:
                if (stance == UIStance.Settings) {
                    MoveElements(UIGroup.TitleScreen, true);
                    MoveElements(UIGroup.SettingsScreen, false);
                }
                else if (stance == UIStance.About) {
                    MoveElements(UIGroup.TitleScreen, true);
                    MoveElements(UIGroup.AboutScreen, false);
                }
                else if (stance == UIStance.Play) {
                    MoveElements(UIGroup.TitleScreen, true);
                    MoveElements(UIGroup.AdjustmentsScreen, false);
                }

                break;
            // Play -> Title
            case UIStance.Play:
                if (stance == UIStance.Title) {
                    MoveElements(UIGroup.TitleScreen, false);
                    MoveElements(UIGroup.AdjustmentsScreen, true);
                }

                break;
            // Settings -> Title
            case UIStance.Settings:
                if (stance == UIStance.Title) {
                    MoveElements(UIGroup.TitleScreen, false);
                    MoveElements(UIGroup.SettingsScreen, true);
                }

                break;
            // About -> Title
            case UIStance.About:
                if (stance == UIStance.Title) {
                    MoveElements(UIGroup.TitleScreen, false);
                    MoveElements(UIGroup.AboutScreen, true);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _currentUI = stance;
    }

    private void MoveElements(UIGroup group, bool away) {
        switch (group) {
            case UIGroup.TitleScreen:
                // Main Title and Start/Settings Buttons
                foreach (GameObject element in _titleElements) {
                    LeanTween
                        .move(element, element.transform.position + _titleDiff * (away ? 1 : -1), away ? 0.65f : 1.15f)
                        .setDelay(away ? 0f : 0.35f)
                        .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic)
                        .setOnComplete(StartTween());
                }

                // Bottom Right About Button
                LeanTween
                    .move(aboutButton.gameObject, aboutButton.transform.position + _aboutButtonDiff * (away ? 1 : -1),
                        0.55f)
                    .setEase(LeanTweenType.easeInOutCubic);
                break;
            case UIGroup.AdjustmentsScreen:
                GameObject adjPanelGo;
                LeanTween
                    .move((adjPanelGo = adjPanel.gameObject),
                        adjPanelGo.transform.position + _adjustmentsDiff * (away ? 1 : -1), 1.15f)
                    .setDelay(away ? 0f : 0.15f)
                    .setEase(LeanTweenType.easeInOutCubic)
                    .setOnComplete(StartTween());
                break;
            case UIGroup.AboutScreen:
                LeanTween
                    .move(aboutPanel.gameObject, aboutPanel.transform.position + _aboutDiff * (away ? 1 : -1),
                        away ? 0.6f : 1f)
                    .setDelay(away ? 0f : 0.40f)
                    .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic)
                    .setOnComplete(StartTween());
                break;
            case UIGroup.SettingsScreen:
                LeanTween
                    .move(settingsPanel.gameObject, settingsPanel.transform.position + _settingsDiff * (away ? 1 : -1),
                        away ? 0.7f : 1.1f)
                    .setDelay(away ? 0.05f : 0.15f)
                    .setEase(away ? LeanTweenType.easeInCubic : LeanTweenType.easeOutCubic)
                    .setOnComplete(StartTween());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(@group), @group, null);
        }
    }
}