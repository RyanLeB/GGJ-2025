using UnityEngine;
using System.Collections.Generic;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab; // Prefab for the bubble
    public int maxBubbles = 10; // Maximum number of bubbles on screen
    public float bubbleSpeed = 5f; // Speed of the bubbles

    private List<GameObject> bubbles = new List<GameObject>();
    private Camera mainCamera;
    
    // Sound effects
    public AudioClip popSound;
    public 

    void Start()
    {
        mainCamera = Camera.main;

        // Spawn initial bubbles
        for (int i = 0; i < maxBubbles; i++)
        {
            SpawnBubble();
        }
    }

    void Update()
    {
        // Check for bubble popping
        if (Input.GetMouseButtonDown(0)) // Left-click to pop bubbles
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                GameObject bubble = bubbles[i];
                if (Vector2.Distance(mousePos, bubble.transform.position) < 0.5f) // Adjust pop radius if needed
                {
                    DestroyBubble(bubble);
                }
            }
        }
    }

    void SpawnBubble()
    {
        Vector2 spawnPosition = GetRandomPositionWithinCamera();
        GameObject newBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);

        // Assign random direction and velocity
        Rigidbody2D rb = newBubble.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.velocity = randomDirection * bubbleSpeed;
        }

        bubbles.Add(newBubble);
    }

    void DestroyBubble(GameObject bubble)
    {
        bubbles.Remove(bubble);
        Destroy(bubble);
        
        if (popSound != null)
        {
            AudioSource.PlayClipAtPoint(popSound, Camera.main.transform.position);
        }

        // Spawn a new bubble
        SpawnBubble();
    }

    Vector2 GetRandomPositionWithinCamera()
    {
        float x = Random.Range(0f, 1f);
        float y = Random.Range(0f, 1f);
        Vector3 viewportPosition = new Vector3(x, y, 0);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);
        worldPosition.z = 0; // Keep bubbles in 2D space
        return worldPosition;
    }

    private void FixedUpdate()
    {
        // Check for bubbles bouncing off screen edges
        foreach (GameObject bubble in bubbles)
        {
            Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector3 position = bubble.transform.position;
                Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);

                // Bounce off screen edges
                if (viewportPosition.x <= 0f || viewportPosition.x >= 1f)
                {
                    rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
                }

                if (viewportPosition.y <= 0f || viewportPosition.y >= 1f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y);
                }
            }
        }
        
        // Enable bubble collisions
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
                    float radius = 0.5f; // Assuming bubbles have a radius of 0.5

                    if (distance < radius * 2f) // If bubbles overlap
                    {
                        direction = direction.normalized;
                        Vector2 relativeVelocity = rbA.velocity - rbB.velocity;
                        float velocityAlongDirection = Vector2.Dot(relativeVelocity, direction);

                        //if (velocityAlongDirection < 0)
                        //{
                        //    Vector2 impulse = direction * velocityAlongDirection * -1f; 
                        //    rbA.velocity += impulse;
                        //    rbB.velocity -= impulse;
                        //}
                    }
                }
            }
        }
    }
}

