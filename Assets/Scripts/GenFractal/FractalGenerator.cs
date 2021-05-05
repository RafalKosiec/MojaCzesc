using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FractalGenerator : MonoBehaviour
{
    public float lowLimitRandom = 0.01f;
    public float highLimitRandom = 1f;
    HashSet<Vector3> setOfVertices = new HashSet<Vector3>();
    Dictionary<Vector3, Triangle> verticesDict = new Dictionary<Vector3, Triangle>();
    List<Triangle> trianglesStructList = new List<Triangle>();

    public float baseSideLength;

    [Range(0, 9)]
    public int levelsOfRecursion;

    const int numOfFloorsForZerothLevelOfRecursion = 2;
    int numOfFloors;
    int numOfVertices;
    int numOfTriangles;
    
    
    Mesh mesh;

    int verticesIterator = 3;
    int triangleIterator = 0;

    Vector3[] vertices;
    int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        numOfFloors = CalculateNumOfFloors(levelsOfRecursion);
        numOfVertices = CalculateNumOfVertices(numOfFloors);
        numOfTriangles = CalculateNumOfTriangles(levelsOfRecursion) * 3;

       // trianglesStructArray = new Triangle[numOfTriangles];

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape(numOfVertices, numOfTriangles);
        UpdateMesh();
        //for(int i = 0; i < vertices.Length;i++)
        //{
        //    if (vertices[i] == null) continue;
        //    else
        //    {
        //        Debug.Log(vertices[i]);
        //    }
        //}
        Debug.Log("Levels Of Recursion: " + levelsOfRecursion);
        Debug.Log("number of floors : " + numOfFloors);
        Debug.Log("vertices count : " + numOfVertices);
        Debug.Log("triangle count: " + numOfTriangles);
        for (int i = 0; i < numOfTriangles; i += 3)
        {
            Debug.Log(triangles[i] + " " + triangles[i + 1] + " " + triangles[i + 2]);
        }
        UpdateMesh();
    }

    private void Update()
    {
        //CreateShape(numOfVertices, numOfTriangles);
        //UpdateMesh();
    }

    //What are floors? Floors is the number of vertices along the side of the biggest triangle. It helps calculating number of points that the mountain will conmtain
    int CalculateNumOfFloors(int levelsOfRecursion)
    {
        int result = numOfFloorsForZerothLevelOfRecursion;
        for(int i=0;i<levelsOfRecursion; i++)
        {
            result = 2*result -1;
        }
        return result;
    }

    //With num of floors, we can easily calculate how many vertices will the final product have
    int CalculateNumOfVertices(int numOfFloors)
    {
        if (levelsOfRecursion == 0) return 3;
        else return (1 + numOfFloors) / 2 * numOfFloors;
    }
    
    //Number of triangles that will spawn is directly mapped to levels of recursion and equals 4^(levelsOfRecursion)
    int CalculateNumOfTriangles(int levelsOfRecursion)
    {
        int result = 1;
        for (int i = 0; i < levelsOfRecursion; i++)
        {
            result *= 4;
        }
        return result;
    }

    void CreateShape(int numOfVertices, int numOfTriangles)
    {
        vertices = new Vector3[numOfVertices];
        triangles = new int[numOfTriangles];

        DrawFirstTriangle();
        Sierpinski(vertices[0], vertices[1], vertices[2], levelsOfRecursion, Vector3.zero, Vector3.zero, Vector3.zero, 0,1,2, 0, 0);
        
    }

    void Sierpinski(Vector3 point1, Vector3 point2, Vector3 point3, int limit, Vector3 p1S, Vector3 p2S, Vector3 p3S, int vI1, int vI2, int vI3, int whichIsImportant, int indexOfImportantVertice)
    {
        

        if (limit == 0)
        {
            AddATriangle(vI1, vI2, vI3);
            return;
        }
        else
        {
            Vector3 pA = (point1 + point2) / 2f;
            Vector3 pB = (point2 + point3) / 2f;
            Vector3 pC = (point3 + point1) / 2f;

            Vector3 pAShifted = new Vector3(pA.x, p1S.y + Random.Range(lowLimitRandom, highLimitRandom), pA.z);
            Vector3 pBShifted = new Vector3(pB.x, p2S.y + Random.Range(lowLimitRandom, highLimitRandom), pB.z);
            Vector3 pCShifted = new Vector3(pC.x, p3S.y + Random.Range(lowLimitRandom, highLimitRandom), pC.z);

            int verticeIteratorA = AddAVerticeIfNotAddedYet(pA, pAShifted, vI1, whichIsImportant, indexOfImportantVertice);
            int verticeIteratorB = AddAVerticeIfNotAddedYet(pB, pBShifted, vI2, whichIsImportant, indexOfImportantVertice);
            int verticeIteratorC = AddAVerticeIfNotAddedYet(pC, pCShifted, vI3, whichIsImportant, indexOfImportantVertice);

            if (whichIsImportant == 1) indexOfImportantVertice = verticeIteratorA;
            else if (whichIsImportant == 2) indexOfImportantVertice = verticeIteratorB;
            else if (whichIsImportant == 3) indexOfImportantVertice = verticeIteratorC;

            //if (verticeIteratorA == vI1) Sierpinski(point1, pA, pC, limit - 1, pAShifted, pBShifted, pCShifted, vI1+1, verticeIteratorA, verticeIteratorC);
            //else Sierpinski(point1, pA, pC, limit - 1, pAShifted, pBShifted, pCShifted, vI1, verticeIteratorA, verticeIteratorC);
            //if (verticeIteratorB == vI2) Sierpinski(pA, point2, pB, limit - 1, pAShifted, pBShifted, pCShifted, verticeIteratorA, vI2+1, verticeIteratorB);
            //else Sierpinski(pA, point2, pB, limit - 1, pAShifted, pBShifted, pCShifted, verticeIteratorA, vI2, verticeIteratorB);
            //if (verticeIteratorC == vI3) Sierpinski(pC, pB, point3, limit - 1, pAShifted, pBShifted, pCShifted, verticeIteratorC, verticeIteratorB, vI3+6);
            //else Sierpinski(pC, pB, point3, limit - 1, pAShifted, pBShifted, pCShifted, verticeIteratorC, verticeIteratorB, vI3);
            Sierpinski(point1, pA, pC, limit - 1, pAShifted, pBShifted, pCShifted, vI1, verticeIteratorA, verticeIteratorC, 2, indexOfImportantVertice);
            Sierpinski(pA, point2, pB, limit - 1, pAShifted, pBShifted, pCShifted, verticeIteratorA, vI2, verticeIteratorB, 3, indexOfImportantVertice);
            Sierpinski(pC, pB, point3, limit - 1, pAShifted, pBShifted, pCShifted, verticeIteratorC, verticeIteratorB, vI3, 1, indexOfImportantVertice);

            Sierpinski(pA, pB, pC, limit - 1, pAShifted, pBShifted, pCShifted, verticeIteratorA, verticeIteratorB, verticeIteratorC, 0, indexOfImportantVertice);
        }
    }
    int AddAVerticeIfNotAddedYet(Vector3 vertice, Vector3 shiftedVertice, int vIX, int whichIsImportant, int indexOfImportantVertice)
    {
        if (!setOfVertices.Contains(vertice))
        {
            setOfVertices.Add(vertice);
            vertices[verticesIterator] = shiftedVertice;
            
            int result = verticesIterator;
            verticesIterator++;

            return result;
        }
        else
        {
            return whichIsImportant;
        }
    }


    void AddATriangle(int verticeIteratorA, int verticeIteratorB, int verticeIteratorC)
    {
        triangles[triangleIterator] = verticeIteratorA;
        triangleIterator++;
        triangles[triangleIterator] = verticeIteratorB;
        triangleIterator++;
        triangles[triangleIterator] = verticeIteratorC;
        triangleIterator++;
    }

    private void DrawFirstTriangle()
    {
        vertices[0] = new Vector3(-baseSideLength, 0, -baseSideLength);
        setOfVertices.Add(vertices[0]);
        vertices[1] = new Vector3(-baseSideLength, 0, baseSideLength);
        setOfVertices.Add(vertices[1]);
        vertices[2] = new Vector3(baseSideLength * 1.7320508076f / 2, 0, 0);
        setOfVertices.Add(vertices[2]);
        trianglesStructList.Add(new Triangle(vertices[0], vertices[1], vertices[2]));
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null) return;
        float sphereSize;
        if (levelsOfRecursion < 6) sphereSize = baseSideLength / 50f;
        else if(levelsOfRecursion == 6) sphereSize = baseSideLength / 100f;
        else if (levelsOfRecursion == 7) sphereSize = baseSideLength / 150f;
        else if (levelsOfRecursion == 8) sphereSize = baseSideLength / 300f;
        else sphereSize = baseSideLength / 500f;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], sphereSize);
        }
    }

    public struct Triangle
    {
        public Vector3 verticeA;
        public Vector3 verticeB;
        public Vector3 verticeC;
        public Triangle(Vector3 verticeA, Vector3 verticeB, Vector3 verticeC)
        {
            this.verticeA = verticeA;
            this.verticeB = verticeB;
            this.verticeC = verticeC;
        }
    }
}
