using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// Boid Counter UI
/// </summary>
public class BoidCounter : MonoBehaviour
{
    public static BoidCounter Instance;
    private Label _boidCounterLabel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _boidCounterLabel = root.Q<Label>("boid-counter");
    }

    public void SetCounter(uint value)
    {
        _boidCounterLabel.text = value.ToString();
        _boidCounterLabel.transform.scale = Vector3.one;
        _boidCounterLabel.experimental.animation.Scale(1.5f, 250).Ease(Easing.OutCubic)
            .OnCompleted(() => _boidCounterLabel.experimental.animation.Scale(1f, 125).Ease(Easing.OutCubic));
    }
}