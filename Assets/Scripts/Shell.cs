using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Shell : MonoBehaviour
{
    private Rigidbody rb;
    public float forceMin;
    public float forceMax;
    private float lifeTime = 4;
    private float fadeTime = 2;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        float force = UnityEngine.Random.Range(forceMin, forceMax);
        rb.AddForce(-transform.right * force);
        rb.AddTorque((UnityEngine.Random.insideUnitSphere * force));

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifeTime);

        float percent = 0;
        float fadeSpeed = 1 / fadeTime;
        Material mat = GetComponent<Renderer>().material;
        Color initColor = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;

            mat.color = Color.Lerp(initColor, Color.clear, percent);

            yield return null;
        }
        
        Destroy(gameObject);
    }
}
