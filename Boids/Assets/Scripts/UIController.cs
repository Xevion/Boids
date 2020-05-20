using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
    public Button startButton;
    public Button settingsButton;

    public Text titleText;
    public BoidController boidController;

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
        // Add event functions
        startButton.onClick.AddListener(OnStartButton);
        settingsButton.onClick.AddListener(OnSettingsButton);

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
        // Move the Title Text up
        startButton.interactable = settingsButton.interactable = false;
        foreach (GameObject element in _titleElements) {
            LeanTween
                .move(element, element.transform.position + new Vector3(0, 500, 0), 1.2f)
                .setEase(LeanTweenType.easeInCubic);
        }
    }

    private void SwitchScreen(UIStance next) {
        
    }

    // Handle Settings Button Clicking
    private void OnSettingsButton() {
    }
}