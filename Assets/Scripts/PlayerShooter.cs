using UnityEngine;


public class PlayerShooter : MonoBehaviour
{
    public enum AimState
    {
        Idle,
        HipFire
    }

    public AimState aimState { get; private set; }

    public Gun gun;
    public LayerMask excludeTarget;
    
    private PlayerInput playerInput;
    private Animator playerAnimator;
    private Camera playerCamera;

    private Vector3 aimPoint;
    private float waitingForReleasingAim = 2.5f;
    private float lastFireInputTime;
    private bool linedUp => !(Mathf.Abs( playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f);
    private bool hasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y,gun.fireTransform.position, ~excludeTarget);
    
    void Awake()
    {
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
        {
            excludeTarget |= 1 << gameObject.layer;
        }
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
        playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(true);
        gun.Setup(this);
    }

    private void OnDisable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (playerInput.fire)
        {
            lastFireInputTime = Time.time;
            Shoot();
        }
        else if (playerInput.reload)
        {
            Reload();
        }
    }

    private void Update()
    {
        UpdateAimTarget();

        var angle = playerCamera.transform.eulerAngles.x;
        if(angle > 270f)
        {
            angle -= 360f;
        }
        angle = angle / -180f + 0.5f;
        playerAnimator.SetFloat("Angle", angle);

        if(playerInput.fire == false && Time.time >= lastFireInputTime + waitingForReleasingAim)
        {
            aimState = AimState.Idle;
        }

        UpdateUI();
    }

    public void Shoot()
    {
        if(aimState == AimState.Idle)
        {
            if(linedUp == true)
            {
                aimState = AimState.HipFire;
            }
        }
        else if (aimState == AimState.HipFire)
        {
            if(hasEnoughDistance == true)
            {
                if(gun.Fire(aimPoint) == true)
                {
                    playerAnimator.SetTrigger("Shoot");
                }
            }
            else
            {
                aimState = AimState.Idle;
            }
        }
    }

    public void Reload()
    {
        if(gun.Reload() == true)
        {
            playerAnimator.SetTrigger("Reload");
        }
    }

    private void UpdateAimTarget()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out hit, gun.fireDistance, ~excludeTarget) == true)
        {
            aimPoint = hit.point;
            Debug.Log("1" + aimPoint);

            if (Physics.Linecast(gun.fireTransform.position, hit.point, out hit, ~excludeTarget) == true)
            {
                aimPoint = hit.point;
                Debug.Log("2" + aimPoint);
            }
        }
        else
        {
            //aimPoint = gun.fireTransform.position + playerCamera.transform.forward * gun.fireDistance;
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
        }
    }

    private void UpdateUI()
    {
        if (gun == null || UIManager.Instance == null) return;
        
        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);
        
        UIManager.Instance.SetActiveCrosshair(hasEnoughDistance);
        UIManager.Instance.UpdateCrossHairPosition(aimPoint);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (gun == null || gun.state == Gun.State.Reloading)
        {
            return;
        }
        else
        {
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

            playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, gun.leftHandMount.position);
            playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, gun.leftHandMount.rotation);
        }
    }
}


//using UnityEngine;


//public class PlayerShooter : MonoBehaviour
//{
//    public enum AimState
//    {
//        Idle,
//        HipFire
//    }

//    public AimState aimState { get; private set; }

//    public Gun gun;
//    public LayerMask excludeTarget;

//    private PlayerInput playerInput;
//    private Animator playerAnimator;
//    private Camera playerCamera;

//    private Vector3 aimPoint;
//    private Vector3 cameraAimPoint;
//    private float waitingForReleasingAim = 2.5f;
//    private float lastFireInputTime;
//    private bool linedUp => !(Mathf.Abs(playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f);
//    private bool hasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y, gun.fireTransform.position, ~excludeTarget);

//    void Awake()
//    {
//        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
//        {
//            excludeTarget |= 1 << gameObject.layer;
//        }
//    }

//    private void Start()
//    {
//        playerInput = GetComponent<PlayerInput>();
//        playerAnimator = GetComponent<Animator>();
//        playerCamera = Camera.main;
//    }

//    private void OnEnable()
//    {
//        aimState = AimState.Idle;
//        gun.gameObject.SetActive(true);
//        gun.Setup(this);
//    }

//    private void OnDisable()
//    {
//        aimState = AimState.Idle;
//        gun.gameObject.SetActive(false);
//    }

//    private void FixedUpdate()
//    {
//        if (playerInput.fire)
//        {
//            lastFireInputTime = Time.time;
//            Shoot();
//        }
//        else if (playerInput.reload)
//        {
//            Reload();
//        }
//    }

//    private void Update()
//    {
//        UpdateAimTarget();

//        var angle = playerCamera.transform.eulerAngles.x;
//        if (angle > 270f)
//        {
//            angle -= 360f;
//        }
//        angle = angle / -180f + 0.5f;
//        playerAnimator.SetFloat("Angle", angle);

//        if (playerInput.fire == false && Time.time >= lastFireInputTime + waitingForReleasingAim)
//        {
//            aimState = AimState.Idle;
//        }

//        UpdateUI();
//    }

//    public void Shoot()
//    {
//        if (aimState == AimState.Idle)
//        {
//            if (linedUp == true)
//            {
//                aimState = AimState.HipFire;
//            }
//        }
//        else if (aimState == AimState.HipFire)
//        {
//            if (hasEnoughDistance == true)
//            {
//                if (gun.Fire(aimPoint) == true)
//                {
//                    playerAnimator.SetTrigger("Shoot");
//                }
//            }
//            else
//            {
//                aimState = AimState.Idle;
//            }
//        }
//    }

//    public void Reload()
//    {
//        if (gun.Reload() == true)
//        {
//            playerAnimator.SetTrigger("Reload");
//        }
//    }

//    private void UpdateAimTarget()
//    {
//        RaycastHit hit;
//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

//        if (Physics.Raycast(ray, out hit, gun.fireDistance, ~excludeTarget) == true)
//        {
//            cameraAimPoint = hit.point;
//            aimPoint = hit.point;

//            if (Physics.Linecast(gun.fireTransform.position, hit.point, out hit, ~excludeTarget) == true)
//            {
//                aimPoint = hit.point;
//            }
//        }
//        else
//        {
//            //aimPoint = gun.fireTransform.position + playerCamera.transform.forward * gun.fireDistance;
//            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
//            cameraAimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
//        }
//    }

//    private void UpdateUI()
//    {
//        if (gun == null || UIManager.Instance == null) return;

//        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);

//        UIManager.Instance.SetActiveCrosshair(hasEnoughDistance);

//        if (Physics.Raycast(gun.fireTransform.position, aimPoint, 5f, ~excludeTarget) == true)
//        {
//            UIManager.Instance.UpdateCrossHairPosition(aimPoint);
//        }
//        else
//        {
//            UIManager.Instance.UpdateCrossHairPosition(cameraAimPoint);
//        }

//    }

//    private void OnAnimatorIK(int layerIndex)
//    {
//        if (gun == null || gun.state == Gun.State.Reloading)
//        {
//            return;
//        }
//        else
//        {
//            playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
//            playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

//            playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, gun.leftHandMount.position);
//            playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, gun.leftHandMount.rotation);
//        }
//    }
//}