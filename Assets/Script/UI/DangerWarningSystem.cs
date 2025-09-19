using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 위험도 경고 시스템 - 높은 위험도일 때 시각적 경고 제공
/// </summary>
public class DangerWarningSystem : MonoBehaviour
{
    [Header("Warning UI")]
    [SerializeField] private Image warningOverlay;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private AudioSource warningAudioSource;
    
    [Header("Warning Settings")]
    [SerializeField] private float warningThreshold = 75f; // 경고 시작 위험도
    [SerializeField] private float criticalThreshold = 90f; // 치명적 경고 시작 위험도
    [SerializeField] private Color warningColor = new Color(1f, 1f, 0f, 0.1f); // 노란색 반투명
    [SerializeField] private Color criticalColor = new Color(1f, 0f, 0f, 0.2f); // 빨간색 반투명
    
    [Header("Animation Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float flashSpeed = 8f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip criticalSound;
    [SerializeField] private float soundCooldown = 3f;
    
    private bool isWarningActive = false;
    private bool isCriticalActive = false;
    private float lastSoundTime = 0f;
    private Coroutine warningCoroutine;
    
    private void OnEnable()
    {
        GameEvents.OnDangerChanged += OnDangerChanged;
    }
    
    private void OnDisable()
    {
        GameEvents.OnDangerChanged -= OnDangerChanged;
    }
    
    private void Awake()
    {
        // UI 컴포넌트 자동 설정
        if (warningOverlay == null)
        {
            warningOverlay = GetComponentInChildren<Image>();
        }
        
        if (warningText == null)
        {
            warningText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (warningAudioSource == null)
        {
            warningAudioSource = GetComponent<AudioSource>();
            if (warningAudioSource == null)
            {
                warningAudioSource = gameObject.AddComponent<AudioSource>();
                warningAudioSource.playOnAwake = false;
            }
        }
        
        // 초기 상태 설정
        SetWarningState(false, false);
    }
    
    /// <summary>
    /// 위험도 변화에 따른 경고 시스템 업데이트
    /// </summary>
    private void OnDangerChanged(float currentDanger, float maxDanger)
    {
        float dangerPercentage = (currentDanger / maxDanger) * 100f;
        
        bool shouldShowWarning = dangerPercentage >= warningThreshold;
        bool shouldShowCritical = dangerPercentage >= criticalThreshold;
        
        // 상태 변화 확인
        if (shouldShowCritical && !isCriticalActive)
        {
            // 치명적 경고 시작
            SetWarningState(true, true);
            PlayWarningSound(criticalSound);
        }
        else if (shouldShowWarning && !isWarningActive && !shouldShowCritical)
        {
            // 일반 경고 시작
            SetWarningState(true, false);
            PlayWarningSound(warningSound);
        }
        else if (!shouldShowWarning && isWarningActive)
        {
            // 경고 해제
            SetWarningState(false, false);
        }
        
        // 경고 텍스트 업데이트
        UpdateWarningText(dangerPercentage);
    }
    
    /// <summary>
    /// 경고 상태 설정
    /// </summary>
    private void SetWarningState(bool warning, bool critical)
    {
        isWarningActive = warning;
        isCriticalActive = critical;
        
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }
        
        if (warning)
        {
            if (critical)
            {
                warningCoroutine = StartCoroutine(CriticalWarningAnimation());
            }
            else
            {
                warningCoroutine = StartCoroutine(WarningAnimation());
            }
        }
        else
        {
            // 경고 해제
            if (warningOverlay != null)
            {
                warningOverlay.color = Color.clear;
            }
            
            if (warningText != null)
            {
                warningText.text = "";
            }
        }
    }
    
    /// <summary>
    /// 일반 경고 애니메이션
    /// </summary>
    private IEnumerator WarningAnimation()
    {
        while (isWarningActive && !isCriticalActive)
        {
            if (warningOverlay != null)
            {
                float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f * warningColor.a;
                Color currentColor = warningColor;
                currentColor.a = alpha;
                warningOverlay.color = currentColor;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 치명적 경고 애니메이션 (빠른 깜빡임)
    /// </summary>
    private IEnumerator CriticalWarningAnimation()
    {
        while (isCriticalActive)
        {
            if (warningOverlay != null)
            {
                float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f * criticalColor.a;
                Color currentColor = criticalColor;
                currentColor.a = alpha;
                warningOverlay.color = currentColor;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 경고 텍스트 업데이트
    /// </summary>
    private void UpdateWarningText(float dangerPercentage)
    {
        if (warningText == null) return;
        
        if (isCriticalActive)
        {
            warningText.text = "⚠️ 치명적 위험! ⚠️";
            warningText.color = Color.red;
        }
        else if (isWarningActive)
        {
            warningText.text = "⚠️ 위험 경고 ⚠️";
            warningText.color = Color.yellow;
        }
        else
        {
            warningText.text = "";
        }
    }
    
    /// <summary>
    /// 경고 사운드 재생
    /// </summary>
    private void PlayWarningSound(AudioClip sound)
    {
        if (warningAudioSource == null || sound == null) return;
        
        // 사운드 쿨다운 확인
        if (Time.time - lastSoundTime < soundCooldown) return;
        
        warningAudioSource.clip = sound;
        warningAudioSource.Play();
        lastSoundTime = Time.time;
    }
    
    /// <summary>
    /// 경고 임계값 설정
    /// </summary>
    public void SetWarningThresholds(float warning, float critical)
    {
        warningThreshold = warning;
        criticalThreshold = critical;
        Debug.Log($"Warning thresholds updated: Warning at {warning}%, Critical at {critical}%");
    }
    
    /// <summary>
    /// 강제 경고 테스트
    /// </summary>
    [ContextMenu("Test Warning")]
    public void TestWarning()
    {
        SetWarningState(true, false);
    }
    
    /// <summary>
    /// 강제 치명적 경고 테스트
    /// </summary>
    [ContextMenu("Test Critical Warning")]
    public void TestCriticalWarning()
    {
        SetWarningState(true, true);
    }
}