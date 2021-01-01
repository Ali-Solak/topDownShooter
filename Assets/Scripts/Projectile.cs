using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Projectile : MonoBehaviour
{
    private float speed;
    public LayerMask collisionMask;
    private float damage = 1;
    private float lifeTime = 3f;
    private float skinWidth = .1f;
    public Color trailColor;


    private void Start()
    {
        Destroy(gameObject, lifeTime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            onHitObject(initialCollisions[0], transform.position);
        }
        
        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;

    }
    
    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        checkCollisions(moveDistance);
        transform.Translate(Vector3.back * moveDistance);
    }

    private void checkCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, -transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            onHitObject(hit.collider, hit.point);
        }
    }



    private void onHitObject(Collider c, Vector3 hitPoint)
    {
        IDamagable damagableObject = c.GetComponent<IDamagable>();
        
        if (damagableObject != null)
        {
            damagableObject.takeHit(damage, hitPoint, -transform.forward);
        }
        
        GameObject.Destroy(gameObject);
    }
}