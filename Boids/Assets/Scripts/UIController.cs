using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
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
    public Button adjCloseButton;

    // Settings Panel Elements
    public RectTransform settingsPanel;
    public Button settingsCloseButton;
    public Toggle showBoidsOnTitleToggle;

    // Element Groups (populated on Start)
    private GameObject[] _titleElements;

    // Misc Elements/Cached Objects
    public Canvas canvas;
    public BoidController boidController;
    private UIStance _currentUI;
    private CanvasScaler _scaler;
    private Rect _canvasRect;
    private float _scaleFactor;
    private int _activeTweens;

    /// <summary>
    /// returns <c>True</c> when the UI Stance is locked due to active tweening animations
    /// </summary>
    private bool UILock => _activeTweens > 0;

    // Element Displacements
    private Vector3 _aboutDiff;
    private Vector3 _aboutButtonDiff;
    private Vector3 _titleDiff;
    private Vector3 _settingsDiff;
    private Vector3 _adjustmentsDiff;

    /// <summary> <c>UIStance</c> describes difference 'stances', or UI configurations.
    /// <see cref="UIController.ChangeStance"/> allows easy change between different stances with the <see cref="UIController.MoveElements"/>
    /// method. Note, this enum is similar but different from the <see cref="UIGroup"/> Enum.
    /// </summary>
    /// <seealso cref="UIController.ChangeStance"/>
    /// <seealso cref="UIController.MoveElements"/>
    private enum UIStance {
        Title,
        PlayAdjust,
        PlayHelp,
        PlayHidden,
        Settings,
        About
    }

    /// <summary> <c>UIGroup</c> describes distinct groups of UI elements.
    /// This Enum is used within <see cref="UIController.MoveElements"/> in order to describe specific portions of the application
    /// compared to a 'UI Stance'.
    /// <remarks>This Enum may be removed and be functionally replaced with <see cref="UIStance"/> later on.</remarks>
    /// </summary>
    /// <seealso cref="UIController.MoveElements"/>
    private enum UIGroup {
        TitleScreen,
        AdjustmentsScreen,
        SettingsScreen,
        AboutScreen
    }

    private void Start() {
        // Set Target Application Framerate
        Application.targetFrameRate = 90;

        // Basic variable setup
        _currentUI = UIStance.Title;
        _scaler = canvas.GetComponent<CanvasScaler>();
        _canvasRect = canvas.GetComponent<RectTransform>().rect;
        _scaleFactor = Screen.width / _scaler.referenceResolution.x;

        // Add stance change functionality to buttons
        startButton.onClick.AddListener(() => ChangeStance(UIStance.PlayAdjust));
        aboutButton.onClick.AddListener(() => ChangeStance(UIStance.About));
        aboutCloseButton.onClick.AddListener(() => ChangeStance(UIStance.Title));
        settingsButton.onClick.AddListener(() => ChangeStance(UIStance.Settings));
        settingsCloseButton.onClick.AddListener(() => ChangeStance(UIStance.Title));
        adjCloseButton.onClick.AddListener(() => ChangeStance(UIStance.Title));

        // Settings Menu Callbacks
        showBoidsOnTitleToggle.onValueChanged.AddListener(ShowBoidsTitle);

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
        // on Escape key, attempts to change UI stance to the Title Screen
        if (Input.GetKeyDown(KeyCode.Escape))
            if (_currentUI == UIStance.PlayHidden)
                ChangeStance(UIStance.PlayAdjust);
            else
                ChangeStance(UIStance.Title);
        else if (Input.GetKeyDown(KeyCode.LeftAlt))
            if (_currentUI == UIStance.PlayAdjust)
                ChangeStance(UIStance.PlayHidden);
            else if (_currentUI == UIStance.PlayHidden)
                ChangeStance(UIStance.PlayAdjust);
    }

    /// <summary>
    /// Signals to the application that a Tween has ended.
    /// This will unlock UI stance changes (if it was the last Tween within the stack).
    /// </summary>
    /// <seealso cref="StartTween"/>
    private void TweenEnd() {
        _activeTweens -= 1;
        if (!UILock)
            Debug.Log("Unlocking Stance Changes");
    }

    /// <summary>
    /// Signals to the application that a Tween is in progress.
    /// This locks UI stance changes until all Tweens have completed.
    /// This method returns the <c>TweenEnd</c> callback, which must be used in conjunction with <c>StartTween</c>.
    /// </summary>
    /// <returns>returns the <see cref="TweenEnd"/> action for a deconstructing callback</returns>
    /// <seealso cref="TweenEnd"/>
    private Action StartTween() {
        if (!UILock)
            Debug.Log("Locking Stance Changes");
        _activeTweens += 1;
        return TweenEnd;
    }


    /// <summary>
    /// Switches the 'Stance' a UI is in by moving groups of elements around.
    /// </summary>
    /// <param name="stance">The stance that the UI should switch to next.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void ChangeStance(UIStance stance) {
        // Quit early if UI is currently "locked" due to an active tween
        if (UILock || stance == _currentUI)
            return;

        Debug.Log($"UI Transition: {_currentUI} -> {stance}");

        // Title -> Settings/About/Play
        if (_currentUI == UIStance.Title) {
            MoveElements(UIGroup.TitleScreen, true);
            if (stance == UIStance.Settings)
                MoveElements(UIGroup.SettingsScreen, false);
            else if (stance == UIStance.About)
                MoveElements(UIGroup.AboutScreen, false);
            else if (stance == UIStance.PlayAdjust) {
                MoveElements(UIGroup.AdjustmentsScreen, false);
                if (!showBoidsOnTitleToggle.isOn)
                    ShowBoidsTitle(true);
            }
        }
        // Settings/About/PlayAdjust -> Title
        else if (stance == UIStance.Title) {
            MoveElements(UIGroup.TitleScreen, false);
            if (_currentUI == UIStance.PlayAdjust) {
                MoveElements(UIGroup.AdjustmentsScreen, true);
                if (!showBoidsOnTitleToggle.isOn)
                    ShowBoidsTitle(false);
            }
            else if (_currentUI == UIStance.Settings)
                MoveElements(UIGroup.SettingsScreen, true);
            else if (_currentUI == UIStance.About)
                MoveElements(UIGroup.AboutScreen, true);
        }
        // PlayAdjust -> PlayHidden
        else if (stance == UIStance.PlayHidden && _currentUI == UIStance.PlayAdjust) {
            MoveElements(UIGroup.AdjustmentsScreen, true);
        }
        // PlayHidden -> PlayAdjust
        else if (stance == UIStance.PlayAdjust && _currentUI == UIStance.PlayHidden) {
            MoveElements(UIGroup.AdjustmentsScreen, false);
        }

        _currentUI = stance;
    }

    /// <summary> Moves groups of elements back and forth using the LeanTween framework.
    /// <c>MoveElements</c> is not aware of the current position of the elements at any time,
    /// thus measures must be implemented to track the current 'state' of any elements.
    /// Elements will be moved towards or away from the Canvas depending on the <paramref name="away"/> parameter.
    /// Calls to <c>MoveElements</c> with the same group but inverted <paramref name="away"/>s should cancel each other out.
    /// Used in conjunction with <see cref="ChangeStance"/> to manipulate the UI elements.
    /// </summary>
    /// <param name="group">used to indicate which elements should be moved</param>
    /// <param name="away">used to indicate which direction the elements should be moved in</param>
    /// <seealso cref="ChangeStance"/>
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
        }
    }

    private void ShowBoidsTitle(bool active) {
        ShowBoids(active);

        // Works somewhat close to what is needed, but needs a fix.
        // if (!active) {
        //     print("Fading out");
        //     LeanTween
        //         .alpha(boidController.gameObject, 0f, 0.5f)
        //         .setEase(LeanTweenType.easeInCubic);
        // }
        // else {
        //     print("Fading in");
        //     LeanTween
        //         .alpha(boidController.gameObject, 1f, 0.5f)
        //         .setEase(LeanTweenType.easeInCubic);
        // }
    }

    private void ShowBoids(bool show) {
        foreach (MeshRenderer meshRenderer in boidController.gameObject.GetComponentsInChildren<MeshRenderer>())
            meshRenderer.enabled = show;
    }
}