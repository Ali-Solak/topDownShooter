using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    };

    public ParticleSystem deathEffect;
    
    private Material skinMaterial;
    private Color originalColor;

    private State currentState;
    
    private NavMeshAgent pathfinder;
    private Transform target;
    private LivingEntity targetEntity;

    private float attackDistanceThreshold = .5f;
    private float timeBetweenAttack = 1f;
    private float damage = 1f;

    private float nextAttackTime;

    private float myCollisionRadius;
    private float targetCollisionRadius;

    private bool hasTarget;
    

    protected override void Start()
    {
        base.Start();

        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;
        
        pathfinder = GetComponent<NavMeshAgent>();


        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(updatePath());

            currentState = State.Chasing;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    public override void takeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (damage >= health)
        {
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)),deathEffect.startLifetime);
        }
        base.takeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    private void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float squarDistanceToTarget = (target.position - transform.position).sqrMagnitude;

                if (squarDistanceToTarget <
                    Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBetweenAttack;
                    StartCoroutine(attack());
                }
            }
        }

    }

    private IEnumerator attack()
    {
        skinMaterial.color = Color.red;
        currentState = State.Attacking;
        pathfinder.enabled = false;
        
        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition =
            target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius);

        float percent = 0;
        float attackSpeed = 3;

        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            if (percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.takeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            
            yield return null;
        }
        pathfinder.enabled = true;
        currentState = State.Chasing;
        skinMaterial.color = originalColor;
    }
    
    private IEnumerator updatePath()
    {
        float refreshRate = .25f;
        
        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition =
                    target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);

                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}

