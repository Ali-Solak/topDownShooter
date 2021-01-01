using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float speed = 5f;
    private PlayerController controller;
    private Camera viewCamera;
    private GunController gunController;
    
    protected override void  Start()
    {
        base.Start();
        
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;

    }

  
    void Update()
    {
        //Movement
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * speed;

        controller.move(moveVelocity);

        //Look
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (ground.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.lookAt(point);
        }
        
        //Weapon
        if (Input.GetMouseButton(0))
        {
            gunController.onTriggerHold();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            gunController.onTriggerRelease();
        }
        

    }
}
