using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class GunController : MonoBehaviour
{
    private GameObject equippedGun;
    public Transform weaponHold;
    public GameObject startingGun;

    private void Start()
    {
        if (startingGun != null)
        {
            equipGun(startingGun);
        }
    }

    public void equipGun(GameObject gunToEquip)
    {
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = (GameObject)Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation);
        equippedGun.transform.parent = weaponHold;

    }

    public void onTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.GetComponent<Gun>().onTriggerHold();
        }
    }

    public void onTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.GetComponent<Gun>().onTriggerRelease();
        }
    }
}
