using UnityEngine;
using System.Collections.Generic;

public class QuadTree
{

    public int depth { get; set; }
    
    private readonly static Vector2[] NodeDirections = new Vector2[]
    {
        Vector2.zero, Vector2.right, Vector2.up, Vector2.one
    };

    private Node root;
    private List<Vector3> vertices;
    private List<int> triangles;
    private Vector2Int position;
    private int size;

    public QuadTree(Vector2Int position, int size, int depth)
    {
        this.depth = depth;
        this.size = size;
        this.position = position;
        root = new Node(Vector2.zero, 1);
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    public void Build(params Vector2[] positions)
    {
        foreach (var pos in positions)
        {
            Subdivide(root, pos, 0.5f, depth);
        }
    }
    
    /*
        Quad Index
        =========
        | 2 | 3 |
        =========
        | 0 | 1 |
        =========
    */
    private void Subdivide(Node node, Vector2 targetPosition, float size, int depth)
    {
        //trova il nodo che dovrà essere suddiviso, ovvero a cui appartiene la targetPosition
        //se x e y sono soddisfatte:        0 or 0 = 0 quadrante
        //se la x non è soddisfatta:        1 or 0 = 1 quadrante
        //se la y non è soddisfatta:        0 or 2 = 2 quadrante
        //se x e y non sono soddisfatte:    1 or 2 = 3 quadrante (01 or 10 = 11)
        int index = 0;
        index |= (targetPosition.x >= node.position.x && targetPosition.x < node.position.x + size) ? 0 : 1;
        index |= (targetPosition.y >= node.position.y && targetPosition.y < node.position.y + size) ? 0 : 2;

        //se è una foglia crea i 4 figli
        if (node.isLeaf)
        {
            node.isLeaf = false;

            for (int i = 0; i < node.children.Length; i++)
            {
                //trovare la posizione dei sottonodi
                Vector2 pos = node.position + NodeDirections[i] * size;

                node.children[i] = new Node(pos, size);
            }
        }

        //se non è arrivato alla profondità massima suddivide il nodo a cui appartiene la targetPosition
        if (depth > 0)
            Subdivide(node.children[index], targetPosition, size * 0.5f, depth - 1);
    }
    
    public Mesh GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();
        NodeProc(root);

        Mesh m = new Mesh();
        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();

        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();
        return m;
    }

    private void NodeProc(Node node)
    {
        if (node.isLeaf)
        {
            vertices.Add(new Vector3(node.position.x + node.size * 0.5f, 0, node.position.y + node.size * 0.5f) * size);
            node.vertexIndex = vertices.Count - 1;
            return;
        }
        
        NodeProc(node[0]);
        NodeProc(node[1]);
        NodeProc(node[2]);
        NodeProc(node[3]);
        
        EdgeProcH(node[0], node[1]);
        EdgeProcH(node[2], node[3]);

        EdgeProcV(node[0], node[2]);
        EdgeProcV(node[1], node[3]);

        CenterProc(node[0], node[1], node[2], node[3]);
        
    }

    private void EdgeProcH(Node a, Node b)
    {
        if (a.isLeaf && b.isLeaf)
            return;

        if (a.isLeaf && !b.isLeaf)
        {
            EdgeProcH(a, b[0]);
            EdgeProcH(a, b[2]);

            MakeTriangle(a.vertexIndex, b[2].bottomLeft.vertexIndex, b[0].upperLeft.vertexIndex);
        }

        if (!a.isLeaf && b.isLeaf)
        {
            EdgeProcH(a[1], b);
            EdgeProcH(a[3], b);

            MakeTriangle(a[1].upperRight.vertexIndex, a[3].bottomRight.vertexIndex, b.vertexIndex);
        }

        
        if (!a.isLeaf && !b.isLeaf)
        {
            EdgeProcH(a[1], b[0]);
            EdgeProcH(a[3], b[2]);

            CenterProc(a[1], b[0], a[3], b[2]);
        }
        
    }

    private void EdgeProcV(Node a, Node b)
    {
        if (a.isLeaf && b.isLeaf)
            return;

        if (a.isLeaf && !b.isLeaf)
        {
            EdgeProcV(a, b[0]);
            EdgeProcV(a, b[1]);

            MakeTriangle(a.vertexIndex, b[0].bottomRight.vertexIndex, b[1].bottomLeft.vertexIndex);
        }

        if (!a.isLeaf && b.isLeaf)
        {
            EdgeProcV(a[2], b);
            EdgeProcV(a[3], b);

            MakeTriangle(a[2].upperRight.vertexIndex, b.vertexIndex, a[3].upperLeft.vertexIndex);
        }

        
        if (!a.isLeaf && !b.isLeaf)
        {
            EdgeProcV(a[2], b[0]);
            EdgeProcV(a[3], b[1]);

            CenterProc(a[2], a[3], b[0], b[1]);
        }
        
    }
    
    private void CenterProc(Node a, Node b, Node c, Node d)
    {
        MakeQuad(
                a.upperRight.vertexIndex,
                b.upperLeft.vertexIndex,
                c.bottomRight.vertexIndex,
                d.bottomLeft.vertexIndex
                );
    }
    
    private void MakeQuad(int a, int b, int c, int d)
    {
        triangles.Add(a);
        triangles.Add(c);
        triangles.Add(b);

        triangles.Add(c);
        triangles.Add(d);
        triangles.Add(b);
    }

    private void MakeTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    private class Node
    {
        public Vector2 position { get; set; }
        public int vertexIndex { get; set; }
        public float size { get; private set; }
        public Node[] children { get; set; }
        public bool isLeaf { get; set; }

        public Node this[int i]
        {
            get
            {
                if (i < 0) i = 0;
                if (i > 3) i = 3;
                return children[i];
            }
        }

        public Node upperLeft
        {
            get
            {
                if (isLeaf)
                    return this;
                return children[2].upperLeft;
            }
        }

        public Node upperRight
        {
            get
            {
                if (isLeaf)
                    return this;
                return children[3].upperRight;
            }
        }

        public Node bottomLeft
        {
            get
            {
                if (isLeaf)
                    return this;
                return children[0].bottomLeft;
            }
        }

        public Node bottomRight
        {
            get
            {
                if (isLeaf)
                    return this;
                return children[1].bottomRight;
            }
        }

        public Node(Vector2 position, float size)
        {
            children = new Node[4];
            this.position = position;
            this.size = size;
            isLeaf = true;
        }
    }

}
