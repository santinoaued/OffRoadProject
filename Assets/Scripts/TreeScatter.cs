using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TreeScatter : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] treePrefabs;

    [Header("Scatter Settings")]
    public int treeCount = 200;
    public float minScale = 0.8f;
    public float maxScale = 1.4f;
    public bool randomRotation = true;

    [Header("Exclusion Zones")]
    public List<ExclusionZone> exclusionZones = new List<ExclusionZone>();

    [Header("Parent Object (opcional)")]
    public Transform treeParent;

    private Terrain terrain;

    [System.Serializable]
    public class ExclusionZone
    {
        public Vector3 center;
        public float radius = 10f;
        public string label = "Zona";
    }

    private void OnDrawGizmos()
    {
        foreach (var zone in exclusionZones)
        {
#if UNITY_EDITOR
            Handles.color = new Color(1f, 0.2f, 0.2f, 0.25f);
            Handles.DrawSolidDisc(zone.center, Vector3.up, zone.radius);
            Handles.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Handles.DrawWireDisc(zone.center, Vector3.up, zone.radius);
            Handles.Label(zone.center + Vector3.up * 2f, zone.label);
#endif
        }
    }

    public void GenerateTrees()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("TreeScatter: no hay Terrain en este GameObject.");
            return;
        }

        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogError("TreeScatter: asigná al menos un prefab.");
            return;
        }

        if (treeParent != null)
        {
            for (int i = treeParent.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                DestroyImmediate(treeParent.GetChild(i).gameObject);
#else
                Destroy(treeParent.GetChild(i).gameObject);
#endif
            }
        }

        TerrainData td = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int placed = 0;
        int attempts = 0;
        int maxAttempts = treeCount * 20;

        while (placed < treeCount && attempts < maxAttempts)
        {
            attempts++;

            float randX = Random.Range(0f, td.size.x);
            float randZ = Random.Range(0f, td.size.z);

            Vector3 worldPos = new Vector3(
                terrainPos.x + randX,
                0f,
                terrainPos.z + randZ
            );

            if (IsInExclusionZone(worldPos))
                continue;

            float height = terrain.SampleHeight(worldPos);
            worldPos.y = terrainPos.y + height;

            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

            Transform parent = treeParent != null ? treeParent : terrain.transform;
            GameObject tree = null;

#if UNITY_EDITOR
            tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
#else
            tree = Instantiate(prefab, parent);
#endif

            tree.transform.position = worldPos;

            float scale = Random.Range(minScale, maxScale);
            tree.transform.localScale = Vector3.one * scale;

            if (randomRotation)
                tree.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            placed++;
        }

        Debug.Log($"TreeScatter: {placed} árboles generados en {attempts} intentos.");
    }

    public void ClearTrees()
    {
        if (treeParent == null) return;

        for (int i = treeParent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(treeParent.GetChild(i).gameObject);
#else
            Destroy(treeParent.GetChild(i).gameObject);
#endif
        }

        Debug.Log("TreeScatter: árboles eliminados.");
    }

    private bool IsInExclusionZone(Vector3 pos)
    {
        foreach (var zone in exclusionZones)
        {
            float dx = pos.x - zone.center.x;
            float dz = pos.z - zone.center.z;
            if ((dx * dx + dz * dz) <= zone.radius * zone.radius)
                return true;
        }
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TreeScatter))]
public class TreeScatterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TreeScatter scatter = (TreeScatter)target;

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("GENERAR ÁRBOLES", GUILayout.Height(35)))
        {
            Undo.RegisterFullObjectHierarchyUndo(scatter.gameObject, "Generar Árboles");
            scatter.GenerateTrees();
        }

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("LIMPIAR ÁRBOLES", GUILayout.Height(25)))
        {
            Undo.RegisterFullObjectHierarchyUndo(scatter.gameObject, "Limpiar Árboles");
            scatter.ClearTrees();
        }

        GUI.backgroundColor = Color.white;
    }
}
#endif