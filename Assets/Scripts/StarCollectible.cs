using System.Collections;
using UnityEngine;

public class StarCollectible : MonoBehaviour
{
    [Header("Efectos al Recoger")]
    [Tooltip("¿Esta estrella cuenta para el objetivo de desbloquear el diamante?")]
    public bool countsAsStar = true;

    [Tooltip("Prefab del efecto de animación o partículas (ej. de Cartoon FX)")]
    public GameObject collectEffectPrefab;

    [Tooltip("Sonido opcional al recoger la estrella")]
    public AudioClip collectSound;

    [Header("Animación en Espera (Flotar y Girar)")]
    public bool animateIdle = true;
    public float rotateSpeed = 90f;
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.25f;

    [Header("Animación al Recoger")]
    [Tooltip("Hace que la estrella gire rápido y se encoja antes de desaparecer")]
    public bool shrinkOnCollect = true;
    public float shrinkDuration = 0.25f;

    private Vector3 startPos;
    private bool isCollected = false;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (isCollected) return;

        if (animateIdle)
        {
            // Girar sobre su eje vertical
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

            // Flotar suavemente arriba y abajo
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Detectar si el objeto que atravesó la estrella es el jugador
        if (other.CompareTag("Player") || 
            other.GetComponent<CharacterController>() != null || 
            other.GetComponentInParent<CharacterController>() != null)
        {
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;

        // Avisar al GameManager que se recogió una estrella
        if (countsAsStar && GameManager.Instance != null)
        {
            GameManager.Instance.AddStar();
        }

        // Desactivar el colisionador de inmediato para no detectar dos veces
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 1. Instanciar el efecto de partículas / animación (si fue asignado)
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Reproducir sonido (si fue asignado)
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // 3. Ejecutar animación de desaparición o destruir directamente
        if (shrinkOnCollect)
        {
            StartCoroutine(ShrinkAndDestroy());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ShrinkAndDestroy()
    {
        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            transform.Rotate(Vector3.up, rotateSpeed * 4f * Time.deltaTime, Space.World);
            yield return null;
        }

        Destroy(gameObject);
    }
}
