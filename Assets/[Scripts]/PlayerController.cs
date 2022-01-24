using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float playerSpeed = 10f;

    [SerializeField]
    private Rigidbody playerRigidBody;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private float mouseSensitivity = 5f;

    [SerializeField]
    private float turnSpeed = 3f;
    
    [SerializeField]
    private Animator anim;

    [SerializeField]
    private Transform camTransform;

    private float currentAngle = 0f;
    private float turnSmoothVelocity;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float horiz = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horiz, 0f, vertical).normalized;
        movement *= playerSpeed * Time.deltaTime;


        if (movement.magnitude > 0.0f)
        {

            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref turnSmoothVelocity, Time.deltaTime * turnSpeed);
            transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            characterController.Move(moveDir * playerSpeed * Time.deltaTime);
        }

        Vector2 lateralSpeed = (new Vector2(horiz, vertical).normalized);
        anim.SetFloat("Blend", lateralSpeed.magnitude);

        //transform.rotation = Quaternion.Euler(mouseX * mouseSensitivity * Time.deltaTime, mouseY * mouseSensitivity * Time.deltaTime, 0f);

    }
}
