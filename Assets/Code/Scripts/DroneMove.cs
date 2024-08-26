using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class DroneMove : MonoBehaviour
{
    public GameObject[] targetObjects;
    [SerializeField] int currentTargetIndex = 0;
    int dronesReachedTargetCount = 0;
    Vector3[] targetVertices;  // Updated to an array for storing vertices
    Vector3 targetPosition;
    Quaternion targetRotation;
    public float moveSpeed = 2f;
    public float startDelay = 2f;
    public float moveDelay = 1f;
    public float timeToWaitOnTarget = 2f;
    public float moveRandomness = 0.1f;
    public float distanceThreshold = 12f;

    void Start()
    {
        StartCoroutine(StartDelay());
    }

    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(startDelay);
        MoveObjectsToTarget();
    }

    void MoveObjectsToTarget()
    {
        // Updated to get target vertices continuously
        targetVertices = GetTargetVertices(targetObjects[currentTargetIndex]);
        targetPosition = targetObjects[currentTargetIndex].transform.position;
        targetRotation = targetObjects[currentTargetIndex].transform.rotation;
        Debug.Log(targetPosition + " " + targetRotation);

        for (int i = 0; i < DroneSpawn.drones.Count; i++)
        {
            StartCoroutine(MoveObject(DroneSpawn.drones[i], targetVertices[i], i * moveDelay));
        }
    }

    IEnumerator MoveObject(GameObject obj, Vector3 localTargetPosition, float delay)
    {
        // Delay before starting the movement
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0;
        float perlinOffsetX = Random.Range(0f, 100f);
        float perlinOffsetY = Random.Range(0f, 100f);
        Vector3 startingPosition = obj.transform.position;
        

        while (elapsedTime < 1)
        {
            float currentRandomness = Mathf.Lerp(0f, moveRandomness, elapsedTime);
            float noiseX = Mathf.PerlinNoise(perlinOffsetX, Time.time * moveSpeed);
            float noiseY = Mathf.PerlinNoise(perlinOffsetY, Time.time * moveSpeed);
            float randomFactorX = Mathf.Lerp(-1f, 1f, noiseX) * currentRandomness;
            float randomFactorY = Mathf.Lerp(-1f, 1f, noiseY) * currentRandomness;

            Vector3 perlinOffsetVector = new Vector3(randomFactorX, randomFactorY, 0f);
            perlinOffsetVector = Vector3.ClampMagnitude(perlinOffsetVector, 5);

            // Update target position, rotation, and vertices continuously
            targetPosition = targetObjects[currentTargetIndex].transform.position;
            targetRotation = targetObjects[currentTargetIndex].transform.rotation;
            targetVertices = GetTargetVertices(targetObjects[currentTargetIndex]);

            // Calculate the final target position using the updated values
            Vector3 rotatedTargetPosition = targetRotation * localTargetPosition;
            Vector3 finalTargetPosition = targetPosition + rotatedTargetPosition;

            if (Vector3.Distance(obj.transform.position, finalTargetPosition) < distanceThreshold) // adjust the threshold as needed, the sooner you want them to remove, make it high, else make it low
            {
                perlinOffsetVector = Vector3.zero;
            }

            obj.transform.position = Vector3.Lerp(obj.transform.position, finalTargetPosition, elapsedTime) + perlinOffsetVector;
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }
        
        //Set to parent of the target object
        obj.transform.SetParent(targetObjects[currentTargetIndex].transform.parent.transform);

        // Delay before starting the next movement
        yield return new WaitForSeconds(moveDelay);

        // Increment the counter when the drone reaches its target
        dronesReachedTargetCount++;

        // Check if all drones have reached their targets
        if (dronesReachedTargetCount == DroneSpawn.drones.Count)
        {
            // Reset the counter
            dronesReachedTargetCount = 0;

            // Start the coroutine to wait on the target
            StartCoroutine(WaitOnTarget());
        }
    }

    IEnumerator WaitOnTarget()
    {
        yield return new WaitForSeconds(timeToWaitOnTarget);

        // Move to the next target object
        currentTargetIndex = (currentTargetIndex + 1) % targetObjects.Length;

        // Check if it's the last target
        if (currentTargetIndex == 0)
        {
            // You can choose to finish or reset the scenario here
            Debug.Log("Reached the last target. Finishing...");
        }
        else
        {
            // Start moving drones to the new target
            MoveObjectsToTarget();
        }
    }

    Vector3[] GetTargetVertices(GameObject target)
    {
        MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (meshFilters.Length > 0)
        {
            List<Vector3> vertices = new List<Vector3>();
            foreach (var mesh in meshFilters)
            {
                vertices.AddRange(mesh.sharedMesh.vertices);
            }
            return vertices.ToArray();
        }
        else if (skinnedMeshRenderers.Length > 0)
        {
            List<Vector3> vertices = new List<Vector3>();
            foreach (var mesh in skinnedMeshRenderers)
            {
                vertices.AddRange(mesh.sharedMesh.vertices);
            }
            return vertices.ToArray();
        }

        return new Vector3[0];
    }
}
