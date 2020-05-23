using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A MonoBehaviour script dedicated to handling changes upon the Adjustments Panel.
/// This MonoBehaviour communicates with a <c>BoidController</c> in order add or remove boids,
/// control biases, enable or disable rules, and interact with many small GUI elements to allow complex User control. 
/// </summary>
public class AdjustmentsHandler : MonoBehaviour {
    public BoidController controller;
    
    // Bias Sliders
    public Slider separationSlider;
    public Slider alignmentSlider;
    public Slider cohesionSlider;
    public Slider boundarySlider;
    public Slider boidCountSlider;

    // Rule Toggles
    private Toggle _separationToggle;
    private Toggle _alignmentToggle;
    private Toggle _cohesionToggle;
    private Toggle _boundaryToggle;

    /// <summary>
    /// Used to specify which elements within the UI should be updated
    /// </summary>
    private enum SliderType {
        Separation,
        Alignment,
        Cohesion,
        Boundary,
        BoidCount
    }
    
    private void Start() {
        // Setup Sliders to Boid Controller's defaults
        separationSlider.value = controller.separationBias;
        alignmentSlider.value = controller.alignmentBias;
        cohesionSlider.value = controller.cohesionBias;
        boundarySlider.value = controller.boundaryBias;
        boidCountSlider.value = controller.boidCount;
        
        // Register Slider Callbacks
        separationSlider.onValueChanged.AddListener(_ => UpdateUI(SliderType.Separation));
        alignmentSlider.onValueChanged.AddListener(_ => UpdateUI(SliderType.Alignment));
        cohesionSlider.onValueChanged.AddListener(_ => UpdateUI(SliderType.Cohesion));
        boundarySlider.onValueChanged.AddListener(_ => UpdateUI(SliderType.Boundary));
        boidCountSlider.onValueChanged.AddListener(_ => UpdateUI(SliderType.BoidCount));
    }

    private void UpdateUI(SliderType update) {
        switch (update) {
            case SliderType.Separation:
                controller.separationBias = separationSlider.value;
                break;
            case SliderType.Alignment:
                controller.alignmentBias = alignmentSlider.value;
                break;
            case SliderType.Cohesion:
                controller.cohesionBias = cohesionSlider.value;
                break;
            case SliderType.Boundary:
                controller.boundaryBias = boundarySlider.value;
                break;
            case SliderType.BoidCount:
                controller.boidCount = (int) boidCountSlider.value;
                // Calculate diff, formally add/remove boids
                int diff = controller.boidCount - controller.boids.Count;
                if(diff > 0)
                    controller.AddBoids(diff);
                else if(diff < 0)
                    controller.RemoveBoids(diff * -1);
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(update), update, null);
        }
    }
}