using NUnit.Framework;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    Light light;
    float t;
    public AnimationCurve curve;
    public float maxTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = GetComponent<Light>();   
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        light.intensity = Mathf.Lerp(0,1,curve.Evaluate(t));
        if(t > maxTime)
        {
            t = 0;
        }




    }
}
