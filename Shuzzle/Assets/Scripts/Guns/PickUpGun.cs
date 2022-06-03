using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PickUpGun : MonoBehaviour
{
    [Header("References")]
    public gunScript gun;
    public Rigidbody rb;
    public CapsuleCollider coll;
    public TextMeshProUGUI weaponEquipped;
    public TextMeshProUGUI playerInteraction;
    public Transform player, gunContainer, fpsCam;
    public Transform gunTransform;
    public Camera cam;
    private InputManager inputManager;

    [Header("Pickup Config")]
    public float automaticPickUpRange;
    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;
    public bool equipped;
    
    private static bool _slotFull;
    private float pickUpTimer;
    private bool canPickUp;
    private bool pause;

    private void Start()
    {
        pickUpTimer = 2f;
        canPickUp = true;
        inputManager = InputManager.instance();
        if (!equipped)
        {
            gun.enabled = false;
            rb.isKinematic = false;
            coll.isTrigger = false;
            gun.setEquipStatus(false);
        }
        if (equipped)
        {
            gun.enabled = true;
            rb.isKinematic = true;
            coll.isTrigger = true;
            _slotFull = true;
            gun.setEquipStatus(true);
        }
    }
    private void Update()
    {
        if (inputManager.pauseGame())
        {
            if (pause)
            {
                pause = false;
            }
            else
            {
                pause = true;
            }
        }else if (pause)
        {
            return;
        }
        
        

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); 
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        Vector3 distanceToPlayer = player.position - transform.position;
        if (!equipped && distanceToPlayer.magnitude <= automaticPickUpRange && !_slotFull && canPickUp)
        {
            canPickUp = false;
            pickUpTimer = 2f;
            PickUp();
            playerInteraction.text = "";
        }else if (!equipped && distanceToPlayer.magnitude <= pickUpRange && inputManager.PickupItem() && !_slotFull && hit.transform.name == transform.name)
        { // Picking up a weapon
            canPickUp = false;
            PickUp();
            playerInteraction.text = "";
        }
        else if(!equipped && distanceToPlayer.magnitude <= pickUpRange && !_slotFull && hit.transform.CompareTag("weapon"))
        {
            playerInteraction.text = "Press E to pickup " + hit.transform.name;
        }
        else
        {
            
            if(!canPickUp && !equipped)
            {
                pickUpTimer -= Time.deltaTime;
            }
            if (pickUpTimer <= 0)
            {
                canPickUp = true;
            }
            playerInteraction.text = "";
        }
        
        if (equipped && inputManager.DropItem())
        {
            Drop();
            weaponEquipped.text = "No Weapon Equipped";
        }
    }
    private void PickUp()
    {
        equipped = true;
        _slotFull = true;
        gun.setEquipStatus(equipped);

        transform.SetParent(gunContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 90,90);
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        rb.isKinematic = true;
        coll.isTrigger = true;

        gun.enabled = true;
    }
    private void Drop()
    {
        equipped = false;
        _slotFull = false;
        gun.setEquipStatus(equipped);

        transform.SetParent(null);

        rb.isKinematic = false;
        coll.isTrigger = false;
        
        rb.velocity = GameObject.Find("Player").GetComponent<CharacterController>().velocity;
        
        
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse); 
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);
        
        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random) * 10);


        gunTransform = gameObject.transform;
        gun.enabled = false;
    }
}