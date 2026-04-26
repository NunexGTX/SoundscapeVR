using Unity.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "VrStateData", menuName = "Scriptable Objects/VrStateData")]
public class VrStateData : ScriptableObject
{
    [SerializeField, DontCreateProperty]
    bool vrEnabled = false;
    [SerializeField, DontCreateProperty]
    VisualType currentVisual = VisualType.ENCOMPASSING;

    [CreateProperty]
    public bool IsInVr
    {
        get => vrEnabled;
        set => vrEnabled = value;
    }

    [CreateProperty]
    public int CurrentVisualChoice
    {
        get => (int)currentVisual;
        set => currentVisual = (VisualType) value;
    }

    public VisualType GetCurrentVisuals()
    {
        return currentVisual;
    }
}
