using UnityEngine;

/// <summary>
/// HP를 가진 모든 객체에 부착할 수 있는 범용 체력 컴포넌트
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;
    [SerializeField] private bool isInvulnerable = false;
    
    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private GameObject deathEffect;
    
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public float HealthPercentage => maxHP > 0 ? currentHP / maxHP : 0f;
    public bool IsAlive => currentHP > 0;
    public bool IsInvulnerable { get => isInvulnerable; set => isInvulnerable = value; }
    
    private void Awake()
    {
        currentHP = maxHP;
    }
    
    private void Start()
    {
        // 플레이어인 경우 초기 체력 이벤트 발생
        if (CompareTag("Player"))
        {
            GameEvents.HealthChanged(currentHP, maxHP);
        }
    }
    
    /// <summary>
    /// 데미지를 받습니다
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0 || !IsAlive || isInvulnerable) return;
        
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);
        
        // 플레이어인 경우 이벤트 발생
        if (CompareTag("Player"))
        {
            GameEvents.HealthChanged(currentHP, maxHP);
        }
        
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHP}/{maxHP}");
        
        if (currentHP <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 체력을 회복합니다
    /// </summary>
    public void Heal(float healAmount)
    {
        if (healAmount <= 0 || !IsAlive) return;
        
        currentHP += healAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
        
        // 플레이어인 경우 이벤트 발생
        if (CompareTag("Player"))
        {
            GameEvents.HealthChanged(currentHP, maxHP);
        }
        
        Debug.Log($"{gameObject.name} healed {healAmount}. HP: {currentHP}/{maxHP}");
    }
    
    /// <summary>
    /// 체력을 완전히 회복합니다
    /// </summary>
    public void FullHeal()
    {
        if (!IsAlive) return;
        
        currentHP = maxHP;
        
        // 플레이어인 경우 이벤트 발생
        if (CompareTag("Player"))
        {
            GameEvents.HealthChanged(currentHP, maxHP);
        }
        
        Debug.Log($"{gameObject.name} fully healed. HP: {currentHP}/{maxHP}");
    }
    
    /// <summary>
    /// 최대 체력을 설정합니다
    /// </summary>
    public void SetMaxHP(float newMaxHP)
    {
        if (newMaxHP <= 0) return;
        
        float hpRatio = HealthPercentage;
        maxHP = newMaxHP;
        currentHP = maxHP * hpRatio; // 비율 유지
        
        // 플레이어인 경우 이벤트 발생
        if (CompareTag("Player"))
        {
            GameEvents.HealthChanged(currentHP, maxHP);
        }
    }
    
    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // 플레이어인 경우 사망 이벤트 발생
        if (CompareTag("Player"))
        {
            GameEvents.PlayerDied();
        }
        
        // 사망 이펙트 생성
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        
        // 사망시 파괴 설정이 있다면 파괴
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 부활 (플레이어용)
    /// </summary>
    public void Revive()
    {
        currentHP = maxHP;
        
        if (CompareTag("Player"))
        {
            GameEvents.HealthChanged(currentHP, maxHP);
        }
        
        Debug.Log($"{gameObject.name} revived!");
    }
}