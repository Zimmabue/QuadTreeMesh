using UnityEngine;
using System.Collections.Generic;

public class QuadTree_Test : MonoBehaviour
{

    public int size;
    public int depth;
    public float y;

    public Vector2 point;
    public Vector2[] target;
    //private List<QuadTree.Node> nodes;
    private List<Vector2> positions;

    private QuadTree tree;

    // Use this for initialization
    void Start()
    {

    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        tree = new QuadTree(Vector2Int.zero, size, depth);

        Vector2[] normTarget = new Vector2[target.Length];
        for (int i = 0; i < target.Length; i++)
        {
            normTarget[i] = target[i] / (float)size;
        }
        tree.Build(normTarget);
        GetComponent<MeshFilter>().sharedMesh = tree.GenerateMesh();
        //Debug.Log(positions.Count);
    }

    private Vector2[] around = new Vector2[]
    {
        new Vector2(-1,-1), new Vector2(0,-1), new Vector2(1,-1),
        new Vector2(-1,0), new Vector2(0,0), new Vector2(1,0),
        new Vector2(-1,1), new Vector2(0,1), new Vector2(1,1)
    };
    public void GenerateFromPoint()
    {
        tree = new QuadTree(Vector2Int.zero, size, depth);

        List<Vector2> normTarget = new List<Vector2>();
        for (int i = 0; i < around.Length; i++)
        {
            if ((point + around[i]).y > y) continue;
            normTarget.Add((point + around[i]) / (float)size);
        }
        tree.Build(normTarget.ToArray());
        GetComponent<MeshFilter>().sharedMesh = tree.GenerateMesh();
    }

    private Vector2Int currentPoint = Vector2Int.zero;
    private void Update()
    {
        if (currentPoint == point)
            return;

        GenerateFromPoint();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(point.x + 0.5f, 0, point.y + 0.5f), 0.1f);
    }

    /*
    private void OnDrawGizmos()
    {
        if (positions == null)
            return;

        if (nodes == null)
            return;

        Gizmos.color = Color.blue;
        foreach (var node in nodes)
        {
            float s = node.size * size;
            Vector3 pos3 = new Vector3(node.position.x * size + s * 0.5f, 0, node.position.y * size + s * 0.5f);
            Gizmos.DrawWireCube(pos3, Vector3.one * s);
        }
        
        foreach (var p in positions)
        {
            Vector3 pos3 = new Vector3(p.x, 0, p.y) * size;
            Gizmos.DrawSphere(pos3, 0.5f);
        }

        if (target.Length == 0)
            return;

        Gizmos.color = Color.red;
        foreach (var p in target)
        {
            Gizmos.DrawWireSphere(new Vector3(p.x, 0, p.y), 0.5f);
        }

    }
    */
    /*
    private void OnDrawGizmos()
    {
        if (nodes == null)
            return;

        Gizmos.color = Color.blue;
        foreach (var node in nodes)
        {
            float s = node.size * size;
            Vector3 pos3 = new Vector3(node.position.x * size + s * 0.5f, 0, node.position.y * size + s * 0.5f);
            Gizmos.DrawWireCube(pos3, Vector3.one * s);
        }

        if (target.Length == 0)
            return;

        Gizmos.color = Color.red;
        foreach (var p in target)
        {
            Gizmos.DrawWireSphere(new Vector3(p.x, 0, p.y), 0.5f);
        }
        
    }
    */


}
