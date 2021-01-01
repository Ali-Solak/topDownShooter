using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    protected float health;
    public float startingHealth;
    protected bool dead;

    public event System.Action OnDeath;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public virtual void takeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        takeDamage(damage);
    }

    public void takeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            die();
        }
    }

    [ContextMenu("Self Destruct")]
    protected void die()
    {
        dead = true;
        if (OnDeath != null)
        {
            OnDeath();
        }

        GameObject.Destroy(gameObject);
    }
}