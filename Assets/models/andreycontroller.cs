using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class andreycontroller : MonoBehaviour {

[Header("Настройки преследования")]
    public float moveSpeed = 100f; // Скорость движения врага
    public float detectionRange = 30f; // Радиус обнаружения игрока
    public float stoppingDistance = 2f; // Дистанция остановки перед игроком
    public float rotationSpeed = 70f; // Скорость поворота
    
    [Header("Настройки видимости")]
    public float fieldOfView = 90f; // Угол обзора врага (в градусах)
    public LayerMask obstacleLayer; // Слой препятствий
    
    [Header("Ссылки")]
    public Transform player; // Ссылка на трансформ игрока
    
    private bool isChasing = false; // Флаг преследования
    private Rigidbody rb; // Компонент Rigidbody
    private Vector3 movement;
    private Animator animator; // Для анимаций (опционально)
    
    void Start()
    {
        // Автоматическое получение компонентов
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // Если игрок не назначен в инспекторе, ищем по тегу
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Проверяем расстояние до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Если игрок в радиусе обнаружения и в поле зрения
        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
            
            // Рассчитываем направление движения
            Vector3 direction = (player.position - transform.position).normalized;

			// Если враг достаточно близко к игроку - останавливаемся
			if (distanceToPlayer > stoppingDistance)
			{
				movement = direction;

				// Плавный поворот в сторону игрока
				Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
				transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
            }
			else
			{
				movement = Vector3.zero;
			}
            
            // Обновляем аниматор (если есть)
            if (animator != null)
            {
                animator.SetBool("IsChasing", isChasing);
                animator.SetFloat("Speed", movement.magnitude);
            }
        }
        else
        {
            isChasing = false;
            movement = Vector3.zero;
            
            // Обновляем аниматор (если есть)
            if (animator != null)
            {
                animator.SetBool("IsChasing", isChasing);
                animator.SetFloat("Speed", 0);
            }
        }
    }
    
    void FixedUpdate()
    {
        // Применяем движение в FixedUpdate для физики
        if (isChasing && movement != Vector3.zero)
        {
            // Двигаем врага (используем физику или трансформ в зависимости от нужного поведения)
            Vector3 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
            
            // Альтернативный вариант (если не используете физику):
            // transform.Translate(movement * moveSpeed * Time.fixedDeltaTime, Space.World);
        }
    }



	// Визуализация в редакторе
	void OnDrawGizmosSelected()
	{
		// Отображаем радиус обнаружения
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		// Отображаем поле зрения
		Gizmos.color = Color.yellow;
		Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward * detectionRange;
		Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward * detectionRange;

		Gizmos.DrawRay(transform.position, leftBoundary);
		Gizmos.DrawRay(transform.position, rightBoundary);
		Gizmos.DrawLine(transform.position + leftBoundary, transform.position + rightBoundary);

	}
    
    // Опционально: обработка столкновений
    void OnCollisionEnter(Collision collision)
    {
        // Если враг столкнулся с игроком
        if (collision.gameObject.CompareTag("Player"))
        {
            // Здесь может быть логика атаки или нанесения урона
            Debug.Log("Враг достиг игрока!");
            
            // Останавливаем преследование при столкновении
            movement = Vector3.zero;
        }
    }
}
