using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AimStateManager : MonoBehaviour
{
    public AimBaseState currentState;

    public HipFireState HipFire = new HipFireState();
    public AimState Aim = new AimState();

    [SerializeField] private float sensitivity = 1f;

    private float xAxis, yAxis;

    [HideInInspector] public Animator anim;

    [HideInInspector] public CinemachineVirtualCamera vCam;

    public float adsFov = 40f;

    [HideInInspector] public float hipFov;

    [HideInInspector] public float currentFov;

    [SerializeField] private float fovSmoothSpeed = 10f;

    [SerializeField] private Transform cameraFollowPos;

    public Transform aimPos;

    [SerializeField] private float aimSmoothSpeed = 20f;

    [SerializeField] private LayerMask aimMask;

    private float xFollowPos;
    private float yFollowPos, ogYPos;

    [SerializeField] private float crochCamHeight = 0.6f;
    [SerializeField] private float sholderSwapSpeed = 10;

    private MovementStateManager moving;

    private void Awake()
    {
        anim = this.GetComponent<Animator>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        moving = GetComponent<MovementStateManager>();
        vCam = GetComponentInChildren<CinemachineVirtualCamera>();

        hipFov = vCam.m_Lens.FieldOfView;

        xFollowPos = cameraFollowPos.localPosition.x;

        ogYPos = cameraFollowPos.localPosition.y;

        yFollowPos = ogYPos;

        SwitchState(HipFire);
    }

    private void Update()
    {
        xAxis += Input.GetAxisRaw("Mouse X") * sensitivity;
        yAxis -= Input.GetAxisRaw("Mouse Y") * sensitivity;

        yAxis = Mathf.Clamp(yAxis, -80f, 80);

        vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, currentFov, fovSmoothSpeed * Time.deltaTime);

        Vector2 screenCentre = new Vector2(Screen.width / 2 , Screen.height / 2);

        Ray ray = Camera.main.ScreenPointToRay(screenCentre);

        if(Physics.Raycast(ray,out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
        }

        MoveCamera();

        currentState.UpdateState(this);
    }

    private void LateUpdate()
    {
        cameraFollowPos.localEulerAngles = new Vector3(yAxis, cameraFollowPos.localEulerAngles.y, cameraFollowPos.localEulerAngles.z);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
    }

    public void SwitchState(AimBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    public void MoveCamera()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt)) { xFollowPos = -xFollowPos; }
        if(moving.currentState == moving.Crouch) { yFollowPos = crochCamHeight; }
        else
        {
            yFollowPos = ogYPos;
        }

        Vector3 newFollowPos = new Vector3(xFollowPos, yFollowPos, cameraFollowPos.localPosition.z);

        cameraFollowPos.localPosition = Vector3.Lerp(cameraFollowPos.localPosition, newFollowPos, sholderSwapSpeed * Time.deltaTime);
    }
}