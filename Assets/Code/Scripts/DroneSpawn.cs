using UnityEngine;
using System.Collections.Generic;

public class DroneSpawn : MonoBehaviour
{
    public GameObject objectToInstantiate;
    public GameObject startingObject;
    public float minDistance = 0.1f;
    public int maxInstantiationCount = 1000;

    public static List<GameObject> drones = new List<GameObject>();

    void Start()
    {
        if (objectToInstantiate == null)
        {
            Debug.LogError("Please assign the 'objectToInstantiate' field in the inspector.");
        }
        else if (startingObject == null)
        {
            Debug.LogError("Please assign the 'targetObject' field in the inspector or through code.");
        }
        else
        {
            MeshFilter[] meshFilters = startingObject.GetComponentsInChildren<MeshFilter>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = startingObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            Renderer[] targetRenderers = startingObject.GetComponentsInChildren<Renderer>();

            int instantiationCount = 0;

            Material[] targetMaterials = startingObject.GetComponentInChildren<Renderer>()?.materials;

            if (meshFilters != null)
            {
                foreach (MeshFilter meshFilter in meshFilters)
                {
                    if (meshFilter == null)
                        continue;

                    Mesh mesh = meshFilter.sharedMesh;

                    if (mesh == null)
                        continue;

                    Vector3[] vertices = mesh.vertices;
                    Vector2[] uvs = mesh.uv;

                    foreach (Vector3 vertex in vertices)
                    {
                        if (instantiationCount >= maxInstantiationCount)
                            break;

                        Vector3 worldPosition = meshFilter.transform.TransformPoint(vertex);

                        GameObject instantiatedObject = Instantiate(objectToInstantiate, worldPosition, Quaternion.identity, startingObject.transform);
                        drones.Add(instantiatedObject);

                        MeshRenderer rendererComponent = instantiatedObject.GetComponentInChildren<MeshRenderer>();

                        Material material = new Material(rendererComponent.material);
                        rendererComponent.material = material;

                        Color sampledColor = SampleColorFromTexture(targetMaterials[0].mainTexture, worldPosition);

                        material.SetColor("_BaseColor", sampledColor);

                        instantiationCount++;
                    }
                }
            }

            if (skinnedMeshRenderers != null)
            {
                foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    if (skinnedMeshRenderer == null)
                        continue;

                    Mesh mesh = skinnedMeshRenderer.sharedMesh;

                    if (mesh == null)
                        continue;

                    Vector3[] vertices = mesh.vertices;
                    Vector2[] uvs = mesh.uv;

                    foreach (Vector3 vertex in vertices)
                    {
                        if (instantiationCount >= maxInstantiationCount)
                            break;

                        Vector3 worldPosition = skinnedMeshRenderer.transform.TransformPoint(vertex);

                        GameObject instantiatedObject = Instantiate(objectToInstantiate, worldPosition, Quaternion.identity, startingObject.transform);
                        drones.Add(instantiatedObject);

                        MeshRenderer rendererComponent = instantiatedObject.GetComponentInChildren<MeshRenderer>();

                        Material material = new Material(rendererComponent.material);
                        rendererComponent.material = material;

                        Color sampledColor = SampleColorFromTexture(targetMaterials[0].mainTexture, worldPosition);

                        material.SetColor("_BaseColor", sampledColor);

                        instantiationCount++;
                    }
                }
            }

            foreach (Renderer targetRenderer in targetRenderers)
            {
                if (targetRenderer != null && targetRenderer.gameObject != objectToInstantiate)
                {
                    if (targetRenderer.GetComponent<DroneSpawn>() == null)
                    {
                        targetRenderer.enabled = false;
                    }
                }
            }
        }
    }

    Color SampleColorFromTexture(Texture texture, Vector3 worldPosition)
    {
        if (texture is Texture2D texture2D)
        {
            Renderer targetRenderer = startingObject.GetComponentInChildren<Renderer>();
            Vector3 localPosition = targetRenderer.transform.InverseTransformPoint(worldPosition);

            Vector2 uv = new Vector2(
                Mathf.InverseLerp(targetRenderer.bounds.min.x, targetRenderer.bounds.max.x, localPosition.x),
                Mathf.InverseLerp(targetRenderer.bounds.min.y, targetRenderer.bounds.max.y, localPosition.y)
            );

            // Get the sampled color
            Color sampledColor = SampleNearestColor(texture2D, uv);
            // Convert it to HSV
            float h, s, v;
            Color.RGBToHSV(sampledColor, out h, out s, out v);
            // // Set the saturation to 100%
            // s = 1.0f;
            // v = 1.0f;
            // Convert it back to RGB
            Color saturatedColor = Color.HSVToRGB(h, s, v);

            return saturatedColor;
        }

        return Color.white;
    }

    Color SampleNearestColor(Texture2D texture, Vector2 uv)
    {
        int x = Mathf.FloorToInt(uv.x * (texture.width - 1));
        int y = Mathf.FloorToInt(uv.y * (texture.height - 1));

        Color sampledColor = texture.GetPixel(x, y);

        return sampledColor;
    }
}
