using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenerateTrees : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private float sizePrefab = 1.5f;
    [SerializeField] private int spawnSize = 5;
    [SerializeField] private int treeDistance = 2;

    private BoxCollider boxCollider;
    private List<GameObject> spawnedTrees = new List<GameObject>();

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        GenerateRandomObjects();
    }

    private void GenerateRandomObjects()
    {
        float boxSizeX = boxCollider.size.x;
        float boxSizeZ = boxCollider.size.z;
        for (int i = 0; i < spawnSize; i++)
        {
            int ri = Random.Range(0, prefabs.Length);
            spawnedTrees.Add(SpawnRandomPos(boxSizeX, boxSizeZ, ri));
        }
    }

    private GameObject SpawnRandomPos(float boxSizeX, float boxSizeZ, int ri)
    {
        Vector3 randomPos;
        float rot;
        float sizeFactor;
        do
        {
            randomPos = new Vector3(
                Random.Range(-boxSizeX / 2, boxSizeX / 2),
                0,
                Random.Range(-boxSizeZ / 2, boxSizeZ / 2)
            );
            rot = Random.Range(0, 360f);
            sizeFactor = Random.Range(0.75f, 1.25f);
        }
        while (spawnedTrees.Exists(t => Vector3.Distance(t.transform.position, randomPos) < treeDistance));

        GameObject tree = Instantiate(prefabs[ri], transform);
        tree.transform.localPosition = randomPos;
        tree.transform.eulerAngles = new Vector3(0, rot, 0);
        tree.transform.localScale = Vector3.one * (sizePrefab * sizeFactor);
        return tree;
    }

    public void UpdateTrees() // Add this method
    {
        ClearTrees(); // Add this method
        GenerateRandomObjects();
    }

    private void ClearTrees() // Add this method
    {
        foreach (GameObject tree in spawnedTrees)
        {
            Destroy(tree);
        }
        spawnedTrees.Clear();
    }
}
