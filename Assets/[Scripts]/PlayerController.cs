using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public delegate void OnMovementStateChanged(bool moving);
    public event OnMovementStateChanged onMovementStateChanged;

    public delegate void OnAimingStateChanged(bool aiming);
    public event OnAimingStateChanged onAimStateChanged;


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
    public Animator anim;

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
    [SerializeField] private RawImage healthBarParent;
    [SerializeField] private Image healthBar;
    [SerializeField] private RawImage armourBarParent;
    [SerializeField] private Image armourBar;

    private Quaternion originalRotation;

    [SerializeField] private Transform rightHandTransform;

    private Vector3 vel = Vector3.zero;

    public float grav = 0f;
    public float jumpForce = 0f;
    private bool isGrounded = true;
    public bool isReloading = false;

    private float health = 100f;
    private float armour = 40f; 
    
    public float Health { get { return health; } set { health = Mathf.Min(value, 100f); RefreshUI(); } }
    public float Armour { get { return armour; } set { armour = Mathf.Min(value, 100f); RefreshUI(); } }

    public CharacterController Controller => characterController;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = lookAtTransform.position;
        lookAtTransform.position = lookAtAiming.position;
        originalRotation = boneTransform.rotation;
        onAimStateChanged.Invoke(false);
        RefreshUI();
    }
    private void RefreshUI()
    {
        Vector2 parentHealthDelta = healthBarParent.rectTransform.sizeDelta;
        healthBar.rectTransform.sizeDelta = new Vector2(health / 100f * parentHealthDelta.x, healthBar.rectTransform.sizeDelta.y);

        Vector2 parentArmourDelta = healthBarParent.rectTransform.sizeDelta;
        armourBar.rectTransform.sizeDelta = new Vector2(armour / 100f * parentArmourDelta.x, armourBar.rectTransform.sizeDelta.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            aimCamera.Priority = 3;
            anim.SetBool("IsAiming", true);
            isAiming = true;
            onAimStateChanged.Invoke(true);

            targetPosition = lookAtIdle.position;

            lookAtTransform.position = lookAtIdle.position;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            aimCamera.Priority = 1;
            anim.SetBool("IsAiming", false);
            isAiming = false;
            onAimStateChanged.Invoke(false);
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            vel.y = jumpForce;
            anim.SetBool("IsJumping", true);
            isGrounded = false;
        }
        vel.y += grav * Time.deltaTime;

        characterController.Move(new Vector3(0f, vel.y * Time.deltaTime, 0f));

        Vector2 lateralSpeed = (new Vector2(horiz, vertical).normalized);
        anim.SetFloat("Blend", lateralSpeed.magnitude);

    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("IsJumping", false);
        }
        if (other.gameObject.CompareTag("Arm"))
        {
            ZombieController zController = null;
            if ((zController = other.transform.GetComponentInParent<ZombieController>()) != null)
            {
                if (zController.isAttacking)
                {
                    health -= 15f * (armour > 0f ? 0.25f : 1f);
                    armour -= 15f;
                    armour = Mathf.Max(armour, 0);

                    RefreshUI();

                    if (health <= 0f)
                    {
                        MenuController.Instance.OnActionByEnum(Action.Defeat);
                    }
                }
            }
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
        Vector3 targetDir = aimTransform.position - camTransform.position;       
        Quaternion aimTowards = Quaternion.LookRotation(targetDir);    
        Quaternion targetBoneRotation = Quaternion.Euler(baseRotation) * aimTowards * originalRotation;

        boneTransform.rotation = Quaternion.Euler(baseRotation)*aimTowards*originalRotation;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(camTransform.position, aimTransform.position);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!isReloading)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, WeaponController.Instance.EquippedWeapon.GetComponent<WeaponProperties>().gripIKTransform.position);
        
        }
        
    }

}
