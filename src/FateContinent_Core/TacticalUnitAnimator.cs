// [TACTICAL UNIT ANIMATOR v18.12.06]
// Оптимизированный менеджер анимаций для BattleScene (Бег, Атаки, Блоки, Получение урона, Смерть)
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TacticalUnitAnimator : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Скорость перемещения фигурки между ячейками.")]
    public float moveSpeed = 4.0f;
    
    [Tooltip("Скорость плавного разворота к цели.")]
    public float rotationSpeed = 10.0f;

    [Header("Unit Class Setup")]
    [Tooltip("0 = Воин, 1 = Стрелок, 2 = Маг (соответствует значению IdleType в Blend Tree)")]
    public int idleType = 0;

    private Animator animator;
    private Transform cachedTransform;
    private bool isMoving = false;

    // Оптимизированное кэширование параметров аниматора (строки переведены в хэши для работы без GC Alloc)
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IdleTypeHash = Animator.StringToHash("IdleType");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int SuperAttackTriggerHash = Animator.StringToHash("SuperAttack");
    
    // Новые хэши параметров для расширенной боевой системы
    private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
    private static readonly int BlockTriggerHash = Animator.StringToHash("Block");
    private static readonly int DeathTriggerHash = Animator.StringToHash("Death");
    private static readonly int SuperDeathTriggerHash = Animator.StringToHash("SuperDeath");
    private static readonly int IsDeadBoolHash = Animator.StringToHash("IsDead");

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cachedTransform = transform;
        targetPosition = cachedTransform.position;
        targetRotation = cachedTransform.rotation;
    }

    private void Start()
    {
        // Устанавливаем тип стойки покоя при старте в зависимости от класса юнита
        animator.SetInteger(IdleTypeHash, idleType);
    }

    private void Update()
    {
        if (isDead) return; // Если мертв — не двигаемся и не обновляем логику

        if (isMoving)
        {
            // Перемещение силами C# без тяжелой физики (экономит до 90% ресурсов процессора)
            cachedTransform.position = Vector3.MoveTowards(cachedTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Проверяем достижение цели
            if (Vector3.Distance(cachedTransform.position, targetPosition) < 0.01f)
            {
                cachedTransform.position = targetPosition;
                isMoving = false;
                animator.SetFloat(SpeedHash, 0f); // Плавный возврат в Idle стойку
            }
        }
    }

    /// <summary>
    /// Команда плавного перемещения в указанные координаты на тактической сетке
    /// </summary>
    public void MoveToCell(Vector3 destination)
    {
        if (isDead) return;

        targetPosition = destination;
        Vector3 direction = (destination - cachedTransform.position).normalized;
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
            isMoving = true;
            animator.SetFloat(SpeedHash, 1.0f); // Запускаем анимацию бега
        }
    }

    /// <summary>
    /// Воспроизведение стандартной атаки по направлению к цели
    /// </summary>
    public void PlayStandardAttack(Vector3 lookAtTarget)
    {
        if (isDead) return;

        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(AttackTriggerHash);
    }

    /// <summary>
    /// Воспроизведение суперспособности / специального удара
    /// </summary>
    public void PlaySuperAttack(Vector3 lookAtTarget)
    {
        if (isDead) return;

        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(SuperAttackTriggerHash);
    }

    /// <summary>
    /// Воспроизведение попадания (получение урона)
    /// </summary>
    public void PlayHit()
    {
        if (isDead) return;
        animator.SetTrigger(HitTriggerHash);
    }

    /// <summary>
    /// Воспроизведение блока удара щитом/оружием
    /// </summary>
    public void PlayBlock()
    {
        if (isDead) return;
        animator.SetTrigger(BlockTriggerHash);
    }

    /// <summary>
    /// Воспроизведение смерти персонажа
    /// </summary>
    /// <param name="isSuperAbility">Если true, воспроизводится смерть от мощного суперудара (падение/отлет)</param>
    public void PlayDeath(bool isSuperAbility)
    {
        if (isDead) return;
        isDead = true;
        isMoving = false;

        animator.SetFloat(SpeedHash, 0f);
        animator.SetBool(IsDeadBoolHash, true);

        if (isSuperAbility)
        {
            animator.SetTrigger(SuperDeathTriggerHash);
        }
        else
        {
            animator.SetTrigger(DeathTriggerHash);
        }
    }

    /// <summary>
    /// Полный сброс состояния (например, для переиспользования в пуле объектов)
    /// </summary>
    public void ResetUnit()
    {
        isDead = false;
        isMoving = false;
        animator.SetBool(IsDeadBoolHash, false);
        animator.SetFloat(SpeedHash, 0f);
    }

    private void LookAtTargetInstant(Vector3 target)
    {
        Vector3 direction = (target - cachedTransform.position).normalized;
        direction.y = 0; // Игнорируем высоту, чтобы модель не наклонялась вверх/вниз
        if (direction != Vector3.zero)
        {
            cachedTransform.rotation = Quaternion.LookRotation(direction);
            targetRotation = cachedTransform.rotation;
        }
    }
}
