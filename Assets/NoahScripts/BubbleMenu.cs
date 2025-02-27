using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BubbleSpawner : MonoBehaviour
{
    public List<GameObject> bubblePrefabs;
    public int maxBubbles = 10;
    public float bubbleSpeed = 5f;
    public float popInDuration = 0.5f;
    public float scaleBubble = 0.5f;

    private List<GameObject> bubbles = new List<GameObject>();
    private Camera mainCamera;
    public List<Sprite> faceSprites;
    public AudioClip popSound;
    public GameObject popParticles;
    public GameObject spawnParticles;

    private ClickerScore scoreManager;

    void Start()
    {
        mainCamera = Camera.main;
        scoreManager = FindObjectOfType<ClickerScore>();

        for (int i = 0; i < maxBubbles; i++)
        {
            StartCoroutine(SpawnBubble());
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            bool bubblePopped = false;

            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                GameObject bubble = bubbles[i];
                if (Vector2.Distance(mousePos, bubble.transform.position) < 0.5f)
                {
                    DestroyBubble(bubble);
                    bubblePopped = true;
                }
            }

            if (!bubblePopped)
            {
                scoreManager.ResetCombo();
            }
        }
    }

    private IEnumerator SpawnBubble()
    {
        Vector2 spawnPosition = GetRandomPositionWithinCamera();
        GameObject newBubble = Instantiate(bubblePrefabs[Random.Range(0, bubblePrefabs.Count)], spawnPosition, Quaternion.identity);

        GameObject face = new GameObject("Face");
        face.transform.SetParent(newBubble.transform);
        face.transform.localPosition = Vector3.zero;
        SpriteRenderer faceRenderer = face.AddComponent<SpriteRenderer>();
        if (faceRenderer != null && faceSprites.Count > 0)
        {
            faceRenderer.sprite = faceSprites[Random.Range(0, faceSprites.Count)];
            faceRenderer.sortingOrder = faceRenderer.sortingOrder + 1;
        }

        newBubble.transform.localScale = Vector3.zero;
        face.transform.localScale = Vector3.zero;

        float timer = 0f;
        while (timer < popInDuration)
        {
            newBubble.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * scaleBubble, timer / popInDuration);
            face.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.5f, timer / popInDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        newBubble.transform.localScale = Vector3.one * scaleBubble;
        face.transform.localScale = Vector3.one * 1f;

        Rigidbody2D rb = newBubble.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.velocity = randomDirection * bubbleSpeed;
        }

        bubbles.Add(newBubble);
        spawnParticles.transform.position = spawnPosition;
    }

    void DestroyBubble(GameObject bubble)
    {
        Animator animator = bubble.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Pop");
        }

        bubbles.Remove(bubble);
        Destroy(bubble, 0.25f);

        if (popSound != null)
        {
            AudioSource.PlayClipAtPoint(popSound, Camera.main.transform.position);
        }

        scoreManager.AddScore(10);
        StartCoroutine(SpawnBubble());
    }

    Vector2 GetRandomPositionWithinCamera()
    {
        float x = Random.Range(0f, 1f);
        float y = Random.Range(0f, 1f);
        Vector3 viewportPosition = new Vector3(x, y, 0);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);
        worldPosition.z = 0;
        return worldPosition;
    }

    private void FixedUpdate()
    {
        foreach (GameObject bubble in bubbles)
        {
            Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector3 position = bubble.transform.position;
                Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);

                SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();
                SpriteRenderer faceRenderer = bubble.transform.Find("Face")?.GetComponent<SpriteRenderer>();

                if (rb.velocity.x > 0)
                {
                    spriteRenderer.flipX = false;
                    faceRenderer.flipX = false;
                }
                else
                {
                    spriteRenderer.flipX = true;
                    faceRenderer.flipX = true;
                }

                if (viewportPosition.x < 0f || viewportPosition.x > 1f)
                {
                    position.x = Mathf.Clamp(position.x, mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x, mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x);
                    rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
                }

                if (viewportPosition.y < 0f || viewportPosition.y > 1f)
                {
                    position.y = Mathf.Clamp(position.y, mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y, mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y);
                    rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y);
                }

                bubble.transform.position = position;
            }
        }

        for (int i = 0; i < bubbles.Count; i++)
        {
            for (int j = i + 1; j < bubbles.Count; j++)
            {
                GameObject bubbleA = bubbles[i];
                GameObject bubbleB = bubbles[j];

                Rigidbody2D rbA = bubbleA.GetComponent<Rigidbody2D>();
                Rigidbody2D rbB = bubbleB.GetComponent<Rigidbody2D>();

                if (rbA != null && rbB != null)
                {
                    Vector2 direction = bubbleA.transform.position - bubbleB.transform.position;
                    float distance = direction.magnitude;
                    float radius = 0.5f;

                    if (distance < radius * 2f)
                    {
                        direction = direction.normalized;
                        Vector2 relativeVelocity = rbA.velocity - rbB.velocity;
                        float velocityAlongDirection = Vector2.Dot(relativeVelocity, direction);

                        if (velocityAlongDirection < 0)
                        {
                            Vector2 impulse = direction * velocityAlongDirection * -1f;
                            rbA.velocity += impulse;
                            rbB.velocity -= impulse;
                        }
                    }
                }
            }
        }
    }
}