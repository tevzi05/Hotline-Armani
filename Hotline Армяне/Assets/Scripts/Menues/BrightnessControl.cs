using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessSettings : MonoBehaviour
{
    public Volume globalVolume;
    private ColorAdjustments _colorAdjustments;

    void Start()
    {
        globalVolume.profile.TryGet(out _colorAdjustments);
    }

    public void UpdateBrightness(float stepValue)
    {
        float finalExposure = Mathf.Lerp(-3f, 3f, stepValue);
        _colorAdjustments.postExposure.value = finalExposure;

    }

}
