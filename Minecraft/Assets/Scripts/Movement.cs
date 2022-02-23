using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    private Vector2 MouseInput;
    [HideInInspector] public Vector2 PlayerInput;
    private float xRot;
    private float Velocity;

    public ChunkCoords CurrentCoord;

    [SerializeField] private LayerMask ChunkLayerMask;

    [SerializeField] private GameObject GroundCheck;
    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private Rigidbody Controller;
    [SerializeField] private float Jumpforce;
    [Space]
    [SerializeField] private float WalkSpeed = 2;
    [SerializeField] private float RunSpeed = 5;
    private float Speed = 10;
    [SerializeField] private float Sensitivity = 5;

    private void Awake()
    {
        CurrentCoord = new ChunkCoords();
    }

    public void SpawnPlayer()
    {
        Ray r = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(r, out RaycastHit hitinfo, Mathf.Infinity, ChunkLayerMask))
        {
            transform.position = hitinfo.point + Vector3.up * 1.5f;
            Controller.isKinematic = false;
        }
    }

    private void Update()
    {
        PlayerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        PlayerMovement();
        PlayerRotation();
        GetPlayerCoordPosition();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void PlayerMovement()
    {

        Speed = Input.GetKey(KeyCode.LeftShift) ? RunSpeed : WalkSpeed;

        Vector3 dir = transform.TransformDirection(PlayerInput.x, 0f, PlayerInput.y) * Speed;
        Controller.velocity = new Vector3(dir.x, Controller.velocity.y, dir.z);

        if (Input.GetKey(KeyCode.Space))
        {
            if (Physics.CheckSphere(GroundCheck.transform.position, .2f, ChunkLayerMask))
            {
                Controller.velocity = Vector3.up * Jumpforce;
            }
        }
    }

    private void PlayerRotation()
    {
        xRot -= MouseInput.y * Sensitivity;

        xRot = Mathf.Clamp(xRot, -90, 90);

        transform.Rotate(0f, MouseInput.x * Sensitivity, 0f);
        PlayerCamera.localRotation = Quaternion.Euler(xRot, 0f, 0f);
    }

    RaycastHit LocationRayHit;
    private void GetPlayerCoordPosition()
    {
        Ray r = new Ray(transform.position, Vector3.down);
        if(Physics.Raycast(r, out LocationRayHit, Mathf.Infinity, ChunkLayerMask))
        {
            CurrentCoord.x = (int)LocationRayHit.collider.transform.position.x / 16;
            CurrentCoord.y = (int)LocationRayHit.collider.transform.position.z / 16;
        }

    }
}
