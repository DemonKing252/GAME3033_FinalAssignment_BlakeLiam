using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public delegate void OnAimingStateChanged(bool moving);
    public event OnAimingStateChanged onMovementStateChanged;


    [SerializeField]
    private float playerSpeed = 10f;

    [SerializeField]
    private Rigidbody playerRigidBody;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private float mouseSensitivity = 5f;

    [SerializeField]
    private Vector3 baseRotation;

    [SerializeField]
    private float turnSpeed = 3f;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    private Transform camTransform;

    private float currentAngle = 0f;
    private float turnSmoothVelocity;
    private bool isAiming = false;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform boneTransform;
    [SerializeField] private Transform drawGizmoTransform;

    private Vector3 targetPosition;
    [SerializeField] private float aimInterpSpeed = 5f;
    [SerializeField] private Transform lookAtTransform;
    [SerializeField] private Transform lookAtIdle;
    [SerializeField] private Transform lookAtAiming;

    [SerializeField] private Cinemachine.CinemachineFreeLook aimCamera;

    private Quaternion originalRotation;

    [SerializeField] private Transform rightHandTransform;

    private Vector3 vel = Vector3.zero;

    public float grav = 0f;
    public float jumpForce = 0f;
    private bool isGrounded = true;
    // Start is called before the first frame update
    void Start()
    {
        targetPosition = lookAtTransform.position;
        lookAtTransform.position = lookAtAiming.position;

        originalRotation = boneTransform.rotation;
        Cursor.lockState = CursorLockMode.Locked;

        //onMovementStateChanged.Invoke(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            aimCamera.Priority = 3;
            anim.SetBool("IsAiming", true);
            isAiming = true;

            targetPosition = lookAtIdle.position;

            lookAtTransform.position = lookAtIdle.position;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            aimCamera.Priority = 1;
            anim.SetBool("IsAiming", false);
            isAiming = false;

            targetPosition = lookAtAiming.position;

            lookAtTransform.position = lookAtAiming.position;
        }


        float horiz = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horiz, 0f, vertical).normalized;
        movement *= playerSpeed * Time.deltaTime;



        if (movement.magnitude > 0.0f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref turnSmoothVelocity, Time.deltaTime * turnSpeed);
            if (!isAiming)
            {
                transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
            }
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            characterController.Move(moveDir * playerSpeed * Time.deltaTime);

            onMovementStateChanged.Invoke(true);
        }
        else
        {
            onMovementStateChanged.Invoke(false);
        }

        if (Input.GetKeyDown(KeyCode.Space) /*&& isGrounded*/)
        {
            vel.y = jumpForce;
            anim.SetBool("IsJumping", true);
            isGrounded = false;
        }
        vel.y += grav * Time.deltaTime;

        characterController.Move(new Vector3(0f, vel.y * Time.deltaTime, 0f));



        Vector2 lateralSpeed = (new Vector2(horiz, vertical).normalized);
        anim.SetFloat("Blend", lateralSpeed.magnitude);

        //transform.rotation = Quaternion.Euler(mouseX * mouseSensitivity * Time.deltaTime, mouseY * mouseSensitivity * Time.deltaTime, 0f);

    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("Collision enter with the ground.");
            anim.SetBool("IsJumping", false);
        }
    }


    public void SetGripTransform(Transform trans)
    {
        rightHandTransform = trans;
    }


    void LateUpdate()
    {
        if (isAiming)
            UpdateAim();
    }

    public void UpdateAim()
    {
        Quaternion targetRot = Quaternion.Euler(new Vector3(0f, camTransform.rotation.eulerAngles.y, 0f));
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 12f);

        //transform.rotation = Quaternion.Euler(new Vector3(0f, camTransform.rotation.eulerAngles.y, 0f));

        //Vector3 aimDir = aimTransform.forward;
        Vector3 targetDir = aimTransform.position - camTransform.position;
        
        //Quaternion aimTowards = Quaternion.FromToRotation(aimDir, targetDir);
        Quaternion aimTowards = Quaternion.LookRotation(targetDir);
        
        //Quaternion targetBoneRotation = aimTowards;
        Quaternion targetBoneRotation = Quaternion.Euler(baseRotation) * aimTowards * originalRotation;

        boneTransform.rotation = Quaternion.Euler(baseRotation)*aimTowards*originalRotation;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(camTransform.position, aimTransform.position);
        //Gizmos.DrawLine(drawGizmoTransform.position, drawGizmoTransform.position + drawGizmoTransform.forward * 3f);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // TODO: Reloading we want to ignore this
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, rightHandTransform.position);

        }
        
    }

}
