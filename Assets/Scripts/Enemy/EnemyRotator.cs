using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float maxRotationAngle = 45f;
    [SerializeField] private float pauseDuration = 1f; // How long to pause at each end

    private float currentRotation = 0f;
    private bool rotatingRight = true;
    private float originalYRotation;
    private float pauseTimer = 0f;
    private bool isPaused = false;

    void Start()
    {
        originalYRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        // If paused, handle the pause timer
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
            }
            return;
        }

        float rotationThisFrame = rotationSpeed * Time.deltaTime;

        if (rotatingRight)
        {
            currentRotation += rotationThisFrame;
            if (currentRotation >= maxRotationAngle)
            {
                currentRotation = maxRotationAngle;
                rotatingRight = false;
                isPaused = true;
                pauseTimer = pauseDuration;
            }
        }
        else
        {
            currentRotation -= rotationThisFrame;
            if (currentRotation <= -maxRotationAngle)
            {
                currentRotation = -maxRotationAngle;
                rotatingRight = true;
                isPaused = true;
                pauseTimer = pauseDuration;
            }
        }

        Vector3 newRotation = transform.eulerAngles;
        newRotation.y = originalYRotation + currentRotation;
        transform.eulerAngles = newRotation;
    }
}