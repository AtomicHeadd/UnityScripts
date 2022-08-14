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
    float sum1minutes;

    void Start()
    {
        Application.targetFrameRate = targetFrameRate; 
        frameQueue = new Queue<float>();
        sum1minutes = 0;
        StartCoroutine(GetPerformanceText(debugIntervalSec));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator GetPerformanceText(float debugIntervalSec){
        int frames = Mathf.FloorToInt(1f / Time.deltaTime);
        string color = (float)frames > targetFrameRate * acceptableRate ? "green" : "red";
        string current = $"<color={color}>{frames}</color>";
        if(!showAverage){
            frameRate.text = current;
            yield return new WaitForSeconds(debugIntervalSec);
            StartCoroutine(GetPerformanceText(debugIntervalSec));
            yield break;
        }
        sum1minutes += 1f / Time.deltaTime;
        frameQueue.Enqueue(1f/Time.deltaTime);
        if(frameQueue.Count > averagePeriodSec / debugIntervalSec) {
            float de = frameQueue.Dequeue();
            sum1minutes -= de;
            print($"Dequeue開始 count: {frameQueue.Count}");
        }
        float average = sum1minutes / frameQueue.Count;
        string averageColor = average > targetFrameRate * acceptableRate ? "green" : "red";
        string averageText = $"\n average: <color={averageColor}>{Mathf.FloorToInt(average)}</color>";
        frameRate.text = current + averageText;
        yield return new WaitForSeconds(debugIntervalSec);
        StartCoroutine(GetPerformanceText(debugIntervalSec));
    }
}
