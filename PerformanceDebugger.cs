using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PerformanceDebugger : MonoBehaviour
{
    TextMeshProUGUI frameRate;
    [SerializeField] float textUpdateInterval = .1f;
    [SerializeField] int targetFrameRate = 60;
    [SerializeField] float fineThreshold = .8f;

    [SerializeField] bool showAverage = false;
    [SerializeField] float averagePeriodSec = 60;

    Queue<float> frameQueue;
    float sum1minutes;

    void Start()
    {
        frameRate = GetComponent<TextMeshProUGUI>();
        Application.targetFrameRate = targetFrameRate; 
        frameQueue = new Queue<float>();
        sum1minutes = 0;
        StartCoroutine(GetPerformanceText(textUpdateInterval));
    }

    string GetThisFrameRate(float acceptableFrames)
    {
        int frames = Mathf.RoundToInt(1f / Time.deltaTime);
        string color = frames > acceptableFrames ? "green" : "red";
        return "fps: " + ColorizeText($"{frames}", color);
    }

    string GetAverageFrameRate(float acceptableFrames)
    {
        sum1minutes += 1f / Time.deltaTime;
        frameQueue.Enqueue(1f/Time.deltaTime);
        if(frameQueue.Count > averagePeriodSec / textUpdateInterval) {
            float de = frameQueue.Dequeue();
            sum1minutes -= de;
        }
        float average = sum1minutes / frameQueue.Count;
        string averageColor = average > targetFrameRate * fineThreshold ? "green" : "red";
        string averageText = $"average: <color={averageColor}>{Mathf.RoundToInt(average)}</color>";
        return averageText;
    }

    string ColorizeText(string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }

    public IEnumerator GetPerformanceText(float debugIntervalSec)
    {
        string frameRateText = GetThisFrameRate(targetFrameRate * fineThreshold);
        if(showAverage) frameRateText += $"\n{GetAverageFrameRate(targetFrameRate * fineThreshold)}";
        frameRate.text = frameRateText;
        yield return new WaitForSeconds(debugIntervalSec);
        StartCoroutine(GetPerformanceText(debugIntervalSec));
    }
}
