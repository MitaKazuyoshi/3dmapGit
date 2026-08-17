using UnityEngine;
using UnityEngine.AI;
using StarterAssets;
using UnityEngine.InputSystem;

public class NavControlBridge : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;
    private ThirdPersonController _tpController;
    private CharacterController _charController;
    private StarterAssetsInputs _input;
    private PlayerInput _playerInput;

    private bool _isAutoNavigating = false;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _tpController = GetComponent<ThirdPersonController>();
        _animator = _tpController.customAnimator != null ? _tpController.customAnimator : GetComponent<Animator>();
        _charController = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();
    }

    public void StartNav(Vector3 destination)
    {
        // 1. エージェントを有効にして、即座に「今の場所」に固定する（ワープ防止）
        transform.position = transform.position;
        _agent.enabled = true;
        _agent.Warp(transform.position); 

        _isAutoNavigating = true;

        // 2. 現在のカメラ角度をリセットせずに同期
        Vector3 currentRotation = _tpController.CinemachineCameraTarget.transform.rotation.eulerAngles;
        _cinemachineTargetYaw = currentRotation.y;
        _cinemachineTargetPitch = currentRotation.x;
        // Pitchが180度を超えて計算される場合があるための補正
        if (_cinemachineTargetPitch > 180) _cinemachineTargetPitch -= 360;

        // 3. 操作系をオフ（エラー回避のためTPController自体を止める）
        _tpController.enabled = false;
        _charController.enabled = false;

        _agent.isStopped = false;
        _agent.SetDestination(destination);
        
        Debug.Log("ナビゲーション開始：ワープを防止しました。");
    }

    void Update()
    {
        if (!_isAutoNavigating) return;

        // 4. 視点操作の代行（計算式をStarterAssetsと一致）
        UpdateCameraRotation();

        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            // 5. アニメーションの代行（MotionSpeedが重要）
            float speed = _agent.velocity.magnitude;
            _animator.SetFloat("Speed", speed);
            //_animator.SetBool("Grounded", true);
            
            // ここを1.0固定にしないと、足の動きの再生速度が0倍（停止）に
            _animator.SetFloat("MotionSpeed", 1.0f); 

            // 6. 到着判定
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                {
                    StopNav();
                }
            }
        }

        // 7. 移動キー入力があったらキャンセル
        if (_input.move != Vector2.zero)
        {
            StopNav();
        }
    }

    private void UpdateCameraRotation()
{
    if (_input.look.sqrMagnitude >= 0.01f)
    {
        // 1. スマホかどうかを「プラットフォーム」と「UIの表示状態」の両方で判定
        bool isMobile = Application.isMobilePlatform || (_tpController.mobileUI != null && _tpController.mobileUI.activeInHierarchy);
        
        // 2. スマホなら MobileSensitivity、PCなら MouseSensitivity を使う
        float sensitivity = isMobile ? _tpController.MobileSensitivity : _tpController.MouseSensitivity;
        
        // 3. DeltaTimeの適用
        // マウス操作以外、あるいはモバイルモードなら Time.deltaTime を掛ける
        bool isMouse = _playerInput.currentControlScheme == "KeyboardMouse";
        float deltaTimeMultiplier = isMobile ? Time.deltaTime : (isMouse ? 1.0f : Time.deltaTime);

        // 4. 計算に反映（MouseSensitivity固定を解除）
        _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * sensitivity;
        _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * sensitivity;
    }

    _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
    _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _tpController.BottomClamp, _tpController.TopClamp);

    _tpController.CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
        _cinemachineTargetPitch + _tpController.CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
}

    public void StopNav()
    {
        _isAutoNavigating = false;
        _agent.enabled = false;
        _charController.enabled = true;
        _tpController.enabled = true;
        
        // パラメータをリセット
        _animator.SetFloat("Speed", 0);
        _animator.SetFloat("MotionSpeed", 1.0f);
        BuildingListManager.HideNavStatusByTag();
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}