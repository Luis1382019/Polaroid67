using System.Collections;
using UnityEngine;

public class BossCarController : MonoBehaviour
{
    [System.Serializable]
    public class BossCarForm
    {
        public Sprite sprite;
        public int bulletCount = 1;
        public float fireRate = 1f;
    }

    [Header("Forms")]
    [SerializeField] private BossCarForm[] forms;
    [SerializeField] private float formDuration = 5f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Shooting")]
    [SerializeField] private float spreadAngle = 45f;

    private int currentFormIndex = 0;
    private bool isActive = true;

    private void Start()
    {
        ApplyForm(0);
        StartCoroutine(FormLoop());
        StartCoroutine(ShootingLoop());
    }

    private IEnumerator FormLoop()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(formDuration);

            currentFormIndex++;

            if (currentFormIndex >= forms.Length)
            {
                currentFormIndex = 0;
            }

            ApplyForm(currentFormIndex);
        }
    }

    private void ApplyForm(int formIndex)
    {
        if (forms.Length == 0) return;

        BossCarForm form = forms[formIndex];
        spriteRenderer.sprite = form.sprite;
    }

    private IEnumerator ShootingLoop()
    {
        while (isActive)
        {
            if (forms.Length > 0)
            {
                BossCarForm form = forms[currentFormIndex];

                ShootPattern(form.bulletCount);

                yield return new WaitForSeconds(form.fireRate);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void ShootPattern(int bulletCount)
    {
        if (bulletPrefab == null || firePoint == null) return;

        if (bulletCount <= 1)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            return;
        }

        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            Instantiate(bulletPrefab, firePoint.position, rotation);
        }
    }

    public void StopBoss()
    {
        isActive = false;
    }
}