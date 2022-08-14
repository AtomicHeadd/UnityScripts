using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PerformanceDebugger : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI frameRate;
    [SerializeField] float debugIntervalSec = 1;
    [SerializeField] int targetFrameRate = 60;
    [SerializeField] float acceptableRate = .8f;

    [SerializeField] bool showAverage = false;
    [SerializeField] float averagePeriodSec = 60;

    Queue<float> frameQueue;
    float acceptableFrames;
    float sum1minutes;

    void Start()
    {
        Application.targetFrameRate = targetFrameRate; 
        frameQueue = new Queue<float>();
        sum1minutes = 0;
        StartCoroutine(GetPerformanceText(debugIntervalSec));
    }

    string GetThisFrameRate(float acceptableFrames)
    {
        int frames = Mathf.FloorToInt(1f / Time.deltaTime);
        string color = frames > acceptableFrames ? "green" : "red";
        return "fps: " + ColorizeText($"{frames}", color);
    }

    string GetAverageFrameRate(float acceptableFrames)
    {
        sum1minutes += 1f / Time.deltaTime;
        frameQueue.Enqueue(1f/Time.deltaTime);
        if(frameQueue.Count > averagePeriodSec / debugIntervalSec) {
            float de = frameQueue.Dequeue();
            sum1minutes -= de;
            //print($"Dequeue開始 count: {frameQueue.Count}");
        }
        float average = sum1minutes / frameQueue.Count;
        string averageColor = average > targetFrameRate * acceptableRate ? "green" : "red";
        string averageText = $"average: <color={averageColor}>{Mathf.FloorToInt(average)}</color>";
        return averageText;
    }

    string ColorizeText(string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }

    public IEnumerator GetPerformanceText(float debugIntervalSec)
    {
        string frameRateText = GetThisFrameRate(targetFrameRate * acceptableRate);
        if(showAverage) frameRateText += $"\n{GetAverageFrameRate(targetFrameRate * acceptableRate)}";
        frameRate.text = frameRateText;
        yield return new WaitForSeconds(debugIntervalSec);
        StartCoroutine(GetPerformanceText(debugIntervalSec));
    }
}
