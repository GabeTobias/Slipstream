using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEngine.InputSystem;

[CustomEditor(typeof(SmartTerrain))]
public class TerrainEditor : Editor
{
    public int BrushSize = 10;

    public override void OnInspectorGUI()
    {
        SmartTerrain terrain = (SmartTerrain)target;

        terrain.width = EditorGUILayout.IntField("Terrain Width", terrain.width);
        terrain.height = EditorGUILayout.IntField("Terrain Height", terrain.height);

        EditorGUILayout.Separator();

        terrain.vertexStretch = EditorGUILayout.IntField("Resolution", terrain.vertexStretch);

        EditorGUILayout.Separator();

        BrushSize = EditorGUILayout.IntSlider(BrushSize,2,25);

        if (GUILayout.Button("MatchTrack")) 
        {
            TrackPath path = FindObjectOfType<TrackPath>();
            int trackWidth = 10;

            for (int i = 0; i < path.getDuration()*2; i++) 
            {
                LocRot transform = path.GetTime(i / (path.getDuration()*2));
                int x = (int)(transform.position.x / terrain.vertexStretch) + (terrain.width / 2 / terrain.vertexStretch);
                float y = transform.position.y;
                int z = (int)(transform.position.z / terrain.vertexStretch) + (terrain.width / 2 / terrain.vertexStretch);

                for (int x0 = x - trackWidth; x0 < x + trackWidth; x0++)
                {
                    for (int y0 = z - trackWidth; y0 < z + trackWidth; y0++)
                    {
                        if (Vector2.Distance(new Vector2(x, z), new Vector2(x0, y0)) > trackWidth - 1) continue;

                        int index = x0 + y0 * (terrain.height / terrain.vertexStretch);

                        if (index < terrain.heightMap.Length && index > 0)
                        {
                            terrain.heightMap[index] = (byte)(y-1);
                        }
                    }
                }
            }

            terrain.GenerateMesh();
        }

        if (GUILayout.Button("Blur Heightmap"))
        {
            int resX = terrain.width / terrain.vertexStretch;
            int resZ = terrain.height / terrain.vertexStretch;

            int blurrSize = 4;
            for (int x0 = 0; x0 < resX; x0++)
            {
                for (int y0 = 0; y0 < resZ; y0++)
                {

                    //Average Neighboring Pixels
                    int total = 0;
                    int count = 0;
                    for (int x1 = x0-blurrSize; x1 < x0+blurrSize; x1++)
                    {
                        for (int y1 = y0-blurrSize; y1 < y0 + blurrSize; y1++)
                        {
                            //Get Index
                            int index = x1 + y1 * (terrain.height / terrain.vertexStretch);

                            //Add Value
                            if (index < terrain.heightMap.Length && index > 0)
                            {
                                total += terrain.heightMap[index];
                                count++;
                            }
                        }
                    }

                    //Divide value
                    total /= count;

                    //Set Final Value
                    int index2 = x0 + y0 * (terrain.height / terrain.vertexStretch);
                    terrain.heightMap[index2] = (byte)total;
                }
            }

            terrain.GenerateMesh();
        }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Generate Mesh"))
        {
            terrain.GenerateMesh();
        }

        if (GUILayout.Button("Clear"))
        {
            int resX = terrain.width / terrain.vertexStretch;
            int resZ = terrain.height / terrain.vertexStretch;

            terrain.heightMap = new byte[resX * resZ];
            terrain.GenerateMesh();
        }

    }

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (EditorWindow.focusedWindow != SceneView.currentDrawingSceneView) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        SmartTerrain terrain = (SmartTerrain)target;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Handles.color = new Color(1, 1, 1, 0.25f);
            Handles.DrawSphere(0, hit.point, Quaternion.identity, BrushSize*terrain.vertexStretch*2);

            if (Mouse.current.leftButton.isPressed) 
            {
                int x = (int)(hit.point.x/terrain.vertexStretch) + (terrain.width/2/terrain.vertexStretch);
                int y = (int)(hit.point.z / terrain.vertexStretch) + (terrain.height/2/terrain.vertexStretch);

                for (int x0 = x - BrushSize; x0 < x + BrushSize; x0++)
                {
                    for (int y0 = y - BrushSize; y0 < y + BrushSize; y0++)
                    {
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(x0, y0)) > BrushSize-1) continue;

                        int index = x0 + y0 * (terrain.height / terrain.vertexStretch);

                        if (index < terrain.heightMap.Length && index > 0)
                        {
                            if (Keyboard.current.ctrlKey.isPressed && terrain.heightMap[index] > 0) terrain.heightMap[index] -= 1;
                            if (!Keyboard.current.ctrlKey.isPressed && terrain.heightMap[index] < 250) terrain.heightMap[index] += 1;
                        }
                    }
                }

                terrain.GenerateMesh();

                EditorUtility.SetDirty((SmartTerrain)target);
            }
        }

        SceneView.RepaintAll();
    }
}
