using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode
    {
        Auto,
        Burst,
        Single
    };

    private bool triggerReleasedSinceShot;

    public FireMode fireMode;
    public int busrstCount;
    
    public Transform[] projectileSpawn;
    public GameObject projectile;
    public float msBetweenShots = 100f;
    public float muzzleVelocity = 35f;
    private float nextShotTime;

    public Transform shell;
    public Transform shellEjector;
    private MuzzleFlash muzzleFlash;
    private int shotsRemainingInBurst;

    private void Start()
    {
        shotsRemainingInBurst = busrstCount;
        muzzleFlash = GetComponent<MuzzleFlash>();
    }

    private void shoot()
    {
        if (Time.time > nextShotTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }

                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceShot)
                {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                nextShotTime = Time.time + msBetweenShots / 1000;

                GameObject newProjectile = (GameObject) Instantiate(projectile, projectileSpawn[i].position,
                    projectileSpawn[i].rotation);
                newProjectile.GetComponent<Projectile>().setSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEjector.position, shellEjector.rotation);
            muzzleFlash.activate();
        }
    }

    public void onTriggerHold()
    {
        shoot();
        triggerReleasedSinceShot = false;
    }

    public void onTriggerRelease()
    {
        triggerReleasedSinceShot = true;
        shotsRemainingInBurst = busrstCount;
    }
}
