using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player : MonoBehaviour 
{   
    [SerializeField]
    int fileID;

    public int maxLoops;

    // variables to control playback
    int currentFrameID = 0;
    int totalFrames = 0;
    int loop = 0;
    float loopXOffset = 0.0f;
    float loopYOffset = 0.0f;
    float loopZOffset = 0.0f;

    // JSONObject containing all the recorded frames
    JSONObject frameContainerObject;

    // variables to hold info. related to a recorded frame
    JSONObject frameObject;
    int frameID = -1;
    float positionX = 0.0f;
    float positionY = 0.0f;
    float positionZ = 0.0f;
    float forwardX = 0.0f;
    float forwardY = 0.0f;
    float forwardZ = 0.0f;
    [HideInInspector]
    public bool pressed = false;

    // constant values    
    const float LOOP_X_OFFSET = 0.3f;
    const float LOOP_Y_OFFSET = 0.25f;
    const float LOOP_Z_OFFSET = 0.15f;

	void Start()
	{
        ReadJSONFile();
	}

    // reads the next frame data
	public void ReadNextFrame()
    {
        if(currentFrameID < totalFrames)
        {
            ReadFrameData();
            ++currentFrameID;
        }
        else
        {
            if(loop < maxLoops-1)
            {
                currentFrameID = 0;
                loopXOffset += LOOP_X_OFFSET;
                loopYOffset += LOOP_Y_OFFSET;
                loopZOffset += LOOP_Z_OFFSET;
                ++loop;
            }
        }
    }

    // reads the json file and creates frameContainerObject
    void ReadJSONFile()
    {
        string path = null;
        string fileNameSuffix = null;

        if (fileID < 10)
            fileNameSuffix = "0" + fileID.ToString() + ".json";
        else
            fileNameSuffix = fileID.ToString() + ".json";

        path = "Assets/Resources/RecordedAction/RecordData_" + fileNameSuffix;

        string encodedString;

        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            using (StreamReader reader = new StreamReader(fs))
            {
                encodedString = reader.ReadToEnd();
            }
        }

        JSONObject rootObject = new JSONObject(encodedString);
        frameContainerObject = (JSONObject)rootObject.list[0];

        totalFrames = frameContainerObject.list.Count;
    }

    // stores the current frame related data in member variables
    void ReadFrameData()
    {
        frameObject = (JSONObject)frameContainerObject.list[currentFrameID];

        frameID = (int)frameObject.list[0].n;

        positionX = (float)frameObject.list[1].n;
        positionY = (float)frameObject.list[2].n;
        positionZ = (float)frameObject.list[3].n;

        forwardX = (float)frameObject.list[4].n;
        forwardY = (float)frameObject.list[5].n;
        forwardZ = (float)frameObject.list[6].n;

        pressed = (int)frameObject.list[7].n != 0;
    }

    // returns the current position data read from frameContainerObject
    public Vector3 GetPosition()
    {
        return new Vector3(positionX+loopXOffset, positionY+loopYOffset, positionZ+loopZOffset);
    }
}
