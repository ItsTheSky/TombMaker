using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

    public Slider slider;
    public float fillSpeed = 0.5f;
    public float targetProgress;
    
    // store code to execute when progress bar is full
    private System.Action _onComplete;
    
    public void SetProgress(float progress)
    {
        targetProgress = progress;
    }
    
    void Update()
    {
        if (slider.value < targetProgress)
        {
            slider.value += fillSpeed * Time.deltaTime;
        }
        else if (_onComplete != null && slider.value >= slider.maxValue)
        {
            _onComplete();
            _onComplete = null;
        }
    }
    
    public void ResetProgress()
    {
        slider.value = 0;
    }
    
    public void SetMaxProgress(float progress)
    {
        slider.maxValue = progress;
    }
    
    public void ExecuteOnComplete(System.Action onComplete)
    {
        _onComplete = onComplete;
    }

    public void SetWholeNumber(bool wholeNumber)
    {
        slider.wholeNumbers = wholeNumber;
    }
}