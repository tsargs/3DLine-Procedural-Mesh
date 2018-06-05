using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPen : MonoBehaviour
{
    // main container of all the drawings
    GameObject drawings;

    // script that reads data from pre-recorded oculus controller's motion
    // in future, 'Player' will be extended to include support for VR
    Player player;

    // boolean variables to control the pen
    bool drawing = false;
    bool newLine = false;
    bool addLine = false;
    bool clearLines = false;

    // array of gameobjects (each corresponding to a line)
    List<GameObject> lines;

    // gameobject corresponding to the current line being drawn
    GameObject line;

    // LineMesh instance corresponding to the current line being drawn
    LineMesh lineMesh;

    // prefabs to be loaded from Resources folder
    GameObject spherePrimitive;
    GameObject lineMeshPrefab;

    // gameobject for visual indication of the pen's nib
    GameObject penNib;
    
    // Vector3 to store previous position of the nib
    Vector3 previousPosition;

    // thickness of the line to be drawn
    public float lineThickness = 0.005f;

    // longitudes on the mesh
    public int meshDetail_X;

    // file name to save the drawings in Resources folder
    public string newPrefabName;

    // file name to load the drawings from Resources folder
    public string loadPrefabName;
    
    // constants
    const float INTERPOLATION_FACTOR = 0.75f;
    const float DISTANCE_THRESHOLD = 0.3f;
    const int FPS = 90;
    readonly Vector3 NULL_POSITION = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

    void Start()
    {
        player = GameObject.Find("RecordedActionPlayer").GetComponent<Player>();

        // create the main container for all drawings
        drawings = new GameObject();
        drawings.name = "Drawings";
        lines = new List<GameObject>();

        // instantiate sphere primitive for the nib
        spherePrimitive = Resources.Load("Prefabs/SpherePrimitive") as GameObject;
        penNib = (GameObject)Instantiate(spherePrimitive, Vector3.zero, Quaternion.identity);
        penNib.transform.localScale = new Vector3(lineThickness, lineThickness, lineThickness);
        penNib.name = "Nib";

        // load the LineMesh prefab
        lineMeshPrefab = Resources.Load("Prefabs/LineMesh") as GameObject;

        // initialize previous position of the nib
        previousPosition = NULL_POSITION;

        // set fixed deltatime based on fps
        Time.fixedDeltaTime = 1f / FPS;

        // Generate and store the mesh data for primitives (hemispheres & circle) to avoid duplicate calculations
        Repository.Initialize(meshDetail_X, lineThickness);
        Repository.GenerateUpperHemisphere();
        Repository.GenerateLowerHemisphere();
        Repository.GenerateCircle();
    }

    void FixedUpdate()
    {
        player.ReadNextFrame();
        ProcessInputs();
        UpdateNib();
        Draw();
    }

    void ProcessInputs()
    {
        if (player.pressed)     // index trigger of right touch controller
        {
            if (!drawing)
            {
                drawing = true;
                newLine = true;
            }
        }
        else
        {
            if (drawing)
            {
                addLine = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.C))      // button 'A' on the right touch controller
        {
            clearLines = true;
        }
    }

    void UpdateNib()
    {
        penNib.transform.position = GetRTouchPosition();
    }

    void Draw()
    {
        if (clearLines)     // user has decided to clear all the drawings
        {
            foreach (GameObject g in lines)
            {
                Destroy(g);
            }
            clearLines = false;

            if(drawing)
            {
                Destroy(line);
                drawing = false;
            }
        }

        if (drawing)       // user is still drawing a line (controller trigger is being held)
        {
            Vector3 currentPosition = GetRTouchPosition();
            Vector3 lineDirection = currentPosition - previousPosition;
            float distance = lineDirection.magnitude;   // distance between previous and current position of the controller
            lineDirection = lineDirection/distance;     // unit vector along the direction of controller movement

            if (previousPosition.Equals(NULL_POSITION))
            {   // valid previousPosition is unavailable
                previousPosition = currentPosition;
            }
            else
            {
                if (distance > lineThickness * DISTANCE_THRESHOLD)
                {
                    // draw only when the controller has moved sufficient distance
                    // this will avoid drawing less useful, closely spaced vertices

                    // compute rotation
                    Quaternion newRotation = Quaternion.FromToRotation(Vector3.down, lineDirection);

                    // matrix containing the translation, rotation and scale information of the mesh
                    Matrix4x4 trsMatrix;

                    if(!addLine)
                    {
                        if (newLine)    // new line is about to be drawn
                        {
                            line = (GameObject)Instantiate(lineMeshPrefab, Vector3.zero, Quaternion.identity);
                            line.transform.parent = drawings.transform;
                            lineMesh = line.AddComponent(typeof(LineMesh)) as LineMesh;
                            lineMesh.InitializeMesh();

                            trsMatrix = Matrix4x4.TRS(previousPosition, newRotation, Vector3.one);

                            lineMesh.AddUpperHemiSphere(ref trsMatrix, ref previousPosition);
                            newLine = false;
                        }

                        trsMatrix = Matrix4x4.TRS(currentPosition, newRotation, Vector3.one);
                        lineMesh.AddOpenCylinder(ref trsMatrix, ref currentPosition, ref lineDirection, ref distance);

                        previousPosition = currentPosition;
                    }
                    else
                    {
                        // user has finished drawing a line

                        trsMatrix = Matrix4x4.TRS(currentPosition, newRotation, Vector3.one);
                        lineMesh.AddLowerHemiSphere(ref trsMatrix, ref previousPosition);

                        lines.Add(line);
                        previousPosition = NULL_POSITION;
                        addLine = false;
                        drawing = false;
                        return;
                    }
                }
            }
        }
    }

    public void SaveLines()
    {
        if (string.IsNullOrEmpty(newPrefabName))
            Debug.LogWarning("File name not specified!");
        else
        {
            // save the drawings as prefabs to 'Resources' folder
            string savePath = "Assets/Resources/Drawings/" + newPrefabName + ".prefab";
            UnityEditor.PrefabUtility.CreatePrefab(savePath, drawings);
        }
    }

    public void LoadDrawingPrefab()
    {

        if (string.IsNullOrEmpty(loadPrefabName))
            Debug.LogWarning("File name not specified!");
        else
        {
            // load the prefab
            GameObject newGameObject = Resources.Load("Drawings/" + loadPrefabName) as GameObject;

            if (newGameObject == null)
            {
                Debug.LogWarning("Check whether file " + loadPrefabName + ".prefab exists");
                return;
            }

            // instantiate the prefab
            GameObject loadedDrawing = (GameObject)Instantiate(newGameObject);
            loadedDrawing.transform.parent = drawings.transform;

            // keep track of it in the 'lines' datastucture
            lines.Add(loadedDrawing);
        }
    }

    Vector3 GetRTouchPosition()
    {
        return player.GetPosition();
    }
}
