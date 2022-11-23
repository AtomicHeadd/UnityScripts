using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyLightV2 : MonoBehaviour
{
    [SerializeField] Transform targetTransform;
    Vector3 offset;
    public float baseIntensity = 2;
    public float animIntensity = 1;
    Light playerLight;

    private void Start()
    {
        playerLight = GetComponent<Light>();
        baseIntensity = 2;
    }
    private void Awake()
    {
        offset = transform.position - targetTransform.position;
        transform.parent = null;
    }

    private void Update()
    {
        transform.position = targetTransform.position + offset;
        float x = Mathf.LerpAngle(transform.eulerAngles.x, targetTransform.eulerAngles.x, 0.1f);
        float y = Mathf.LerpAngle(transform.eulerAngles.y, targetTransform.eulerAngles.y, 0.1f);
        float z = Mathf.LerpAngle(transform.eulerAngles.z, targetTransform.eulerAngles.z, 0.1f);
        transform.eulerAngles = new Vector3(x, y, z);
        playerLight.intensity = baseIntensity * animIntensity;
    }
}
