﻿using UnityEngine;
using NetMQ;
using System.Globalization;
public class RotateCameraOnHeadMove : MonoBehaviour
{
    private FaceDetector _faceDetector;
    private ScriptConnector _scriptConnector;
    public Transform Target;

    public int MIN_DETECTED_ROTATION = 8;
    public float ROTATION_SPEED = 1.5f;
    public float Y_ROTATION_DISTANCE = 1.5f;

    private float previousYRotation = 0;
    private float currentYRotation = 0;
    private float receivedYRotation = 0;
    private int currentNullRotationCount = 0;
    
    private void Awake()
    {
        _faceDetector = new FaceDetector(Application.dataPath);
        _scriptConnector = new ScriptConnector();
    }

    private void Update()
    {
        receiveAndParseMessage();

        if (receivedYRotation != 0)
        {
            currentNullRotationCount = 0;

            if (receivedYRotation >= previousYRotation + MIN_DETECTED_ROTATION) // rotate right
            {
                currentYRotation = currentYRotation + Y_ROTATION_DISTANCE;
                rotateCamera(currentYRotation);
            } 
            else if (receivedYRotation < previousYRotation - MIN_DETECTED_ROTATION) // rotate left
            {
                currentYRotation = currentYRotation - Y_ROTATION_DISTANCE;
                rotateCamera(currentYRotation);
            }
        } 
        else { 
            currentNullRotationCount++; 
            if (currentNullRotationCount > 50) // if cannot detect face for 50 frames then recenter
            {
                if (currentYRotation != 0)
                {
                    if (currentYRotation > 1) {
                        currentYRotation -= 1;
                    rotateCamera(currentYRotation);
                    } 
                    else if (currentYRotation < -1)
                    {
                        currentYRotation +=1;
                        rotateCamera(currentYRotation);
                    }
                    else 
                    {
                        rotateCamera(0);
                        currentYRotation = 0;
                        previousYRotation = 0;
                    }
                }
            }
        } 
    }

    private void rotateCamera(float yRotation)
    {
        this.transform.localRotation = Quaternion.Euler(0, yRotation * ROTATION_SPEED, 0);
        previousYRotation = currentYRotation;
        receivedYRotation = 0;
    }

    private void receiveAndParseMessage()
    {
        string message = _scriptConnector.ReceiveMessage();

        if (message != null)
        {
            try 
            {
                string[] receivedRotations = message.Split(null);
                this.receivedYRotation = float.Parse(receivedRotations[1], CultureInfo.InvariantCulture.NumberFormat);
            }
            catch {
                Debug.LogError(message);
            }
        }
    }

    private void OnDestroy()
    {
        _scriptConnector.Destroy();
        _faceDetector.Destroy();
    }
}