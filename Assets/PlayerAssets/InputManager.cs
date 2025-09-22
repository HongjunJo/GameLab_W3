using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public PlayerInput playerInput { get; private set; }
    public event Action OnFill;
    public event Action StopFill;
    public event Action PressA;
    public event Action PressD;
    public event Action ReleaseA;
    public event Action ReleaseD;
    public event Action TimingChecker;
    public event Action<float> MovementAD;
    public event Action<float> StopMovementAD;
    public event Action SpacePressed;
    public event Action SpaceReleased;
    public event Action HitClick;

    [SerializeField] public bool isDragging = false; // 드래그 여부
    [SerializeField] public bool isClick = false; // 클릭 여부
    public Vector2 moveInput; // 대각선 무시 이동 방향
    private Camera mainCamera; // 메인 카메라
    public GameObject draggedObject; // 현재 드래그 중인 오브젝트
    public GameObject clickableObject; // 클릭 가능한 오브젝트
    public Rect mouseClampArea = new Rect(0, 0, 1920, 1080); // 게임 화면
    public LayerMask blockingMask;


    private void Awake()
    {
        playerInput = new PlayerInput(); 
        //TO-DO : 정상 작동 시 삭제
        mainCamera = Camera.main;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Jump.started += SpaceStarted;
        playerInput.Player.Jump.performed += OnJmup;
        playerInput.Player.Jump.canceled += SpaceCanceled;
        playerInput.Player.Pause.performed += OnPause;
        playerInput.Player.PrimaryClick.started += StartDrag;
        playerInput.Player.PrimaryClick.canceled += EndDrag;
        playerInput.Player.PrimaryClick.performed += OnLeftClick;
        playerInput.Player.HorizontalLeft.performed += OnPressA;
        playerInput.Player.HorizontalRight.performed += OnPressD;
        playerInput.Player.HorizontalLeft.canceled += OffPressA;
        playerInput.Player.HorizontalRight.canceled += OffPressD;
        playerInput.Player.MovementHoriAxis.performed += OnHoriAxis;
        playerInput.Player.MovementHoriAxis.canceled += OnHoriAxis;

        /*
        // 대각선 무시
        playerInput.Player.VerticalUp.performed += OnMoveUp;
        playerInput.Player.VerticalUp.canceled += OnMoveUpCanceled;
        playerInput.Player.VerticalDown.performed += OnMoveDown;
        playerInput.Player.VerticalDown.canceled += OnMoveDownCanceled;
        playerInput.Player.HorizontalLeft.performed += OnMoveLeft;
        playerInput.Player.HorizontalLeft.canceled += OnMoveLeftCanceled;
        playerInput.Player.HorizontalRight.performed += OnMoveRight;
        playerInput.Player.HorizontalRight.canceled += OnMoveRightCanceled;
        */
    }

    private void OnDisable()
    {
        playerInput.Player.Jump.started -= SpaceStarted;
        playerInput.Player.Jump.performed -= OnJmup;
        playerInput.Player.Jump.canceled -= SpaceCanceled;
        playerInput.Player.Pause.performed -= OnPause; 
        playerInput.Player.PrimaryClick.started -= StartDrag;
        playerInput.Player.PrimaryClick.canceled -= EndDrag;
        playerInput.Player.PrimaryClick.performed -= OnLeftClick;
        playerInput.Player.HorizontalLeft.performed -= OnPressA;
        playerInput.Player.HorizontalRight.performed -= OnPressD;
        playerInput.Player.HorizontalLeft.canceled -= OffPressA;
        playerInput.Player.HorizontalRight.canceled -= OffPressD;
        playerInput.Player.MovementHoriAxis.performed -= OnHoriAxis;
        playerInput.Player.MovementHoriAxis.canceled -= OnHoriAxis;


        /*
        // 대각선 무시
        playerInput.Player.VerticalUp.performed -= OnMoveUp;
        playerInput.Player.VerticalUp.canceled -= OnMoveUpCanceled;
        playerInput.Player.VerticalDown.performed -= OnMoveDown;
        playerInput.Player.VerticalDown.canceled -= OnMoveDownCanceled;
        playerInput.Player.HorizontalLeft.performed -= OnMoveLeft;
        playerInput.Player.HorizontalLeft.canceled -= OnMoveLeftCanceled;
        playerInput.Player.HorizontalRight.performed -= OnMoveRight;
        playerInput.Player.HorizontalRight.canceled -= OnMoveRightCanceled;
        */

        playerInput.Player.Disable();
    }

    //TO- DO  : DISABLE 도 통일
    private void OnDestroy()
    {
        DeleteEvents();
    }

    private void DeleteEvents()
    {
        playerInput.Player.Jump.performed -= OnJmup;
        playerInput.Player.Pause.performed -= OnPause; 
        // playerInput.Player.PrimaryClick.started -= StartDrag;
        playerInput.Player.PrimaryClick.canceled -= EndDrag;
        playerInput.Player.PrimaryClick.performed -= OnLeftClick;
        playerInput.Player.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInput.Player.PrimaryClick.IsPressed())
            isDragging = true;
        else
            isDragging = false;
        // GetCurrentMousePosition();
        // GetMoveInput();
    }
    // 왼쪽 클릭 - 좌클릭 입력
    public void OnLeftClick(InputAction.CallbackContext context)
    {
       
        
    }

    // 이동 입력
    public Vector2 GetMoveInput()
    {
        Vector2 moveDirection = playerInput.Player.Move.ReadValue<Vector2>();

        return moveDirection;
    }

    // 점프 - 스페이스
    public void OnJmup(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnFill?.Invoke();
            TimingChecker?.Invoke();
        }
    }

    // 일시정지 - ESC
    public void OnPause(InputAction.CallbackContext context)
    {
        
    }


    // 마우스 드래그 시작
    private void StartDrag(InputAction.CallbackContext context)
    {
        isDragging = true;
        // Debug.Log("Start");
        Vector2 mouseScreenPos = playerInput.Player.PointerPosition.ReadValue<Vector2>();
        if (mainCamera == null)
            mainCamera = Camera.main;
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(mouseScreenPos), Vector2.zero);
        if (hit.collider != null)
        {
            
        } 
        
    }

    // 마우스 드래그 종료
    private void EndDrag(InputAction.CallbackContext context)
    {
        // Debug.Log("End");
        isDragging = false;
        // 드래그 중인 오브젝트 해제
         draggedObject = null;
    }


    // 마우스 
    public Vector2 GetMousePointerWorldPos()
    {
        /*Vector2 mouseScreenPos = playerInput.Player.PointerPosition.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        return mouseWorldPos;*/

        Vector2 mouseScreenPos = playerInput.Player.PointerPosition.ReadValue<Vector2>();
        mouseScreenPos.x = Mathf.Clamp(mouseScreenPos.x, mouseClampArea.xMin, mouseClampArea.xMax);
        mouseScreenPos.y = Mathf.Clamp(mouseScreenPos.y, mouseClampArea.yMin, mouseClampArea.yMax);
        Vector3 mouseWorldPosWithZ = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        return mouseWorldPosWithZ;
    }
    private void SpaceStarted(InputAction.CallbackContext context)
    {
        SpacePressed?.Invoke();
    }
    private void SpaceCanceled(InputAction.CallbackContext context)
    {
        StopFill?.Invoke();
        SpaceReleased?.Invoke();
    }

   private void OnPressA(InputAction.CallbackContext context)
   {
        if (context.performed)
        {
            PressA?.Invoke();
        }
   }

    private void OnPressD(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PressD?.Invoke();
        }
    }

    private void OffPressA(InputAction.CallbackContext context)
    {
        ReleaseA?.Invoke();
    }

    private void OffPressD(InputAction.CallbackContext context)
    {
        ReleaseD?.Invoke();
    }

    private void OnHoriAxis(InputAction.CallbackContext context)
    {
        float movement = context.ReadValue<float>();
        MovementAD?.Invoke(movement);
    }
}
