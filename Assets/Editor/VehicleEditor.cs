using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Assets.Editor
{
    class VehicleEditor  : EditorWindow
    {
        string vehicleID = "xxxx";
        string roadTrunkIdno = "xxxx";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;

        [MenuItem("Window/Vehicles")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(VehicleEditor));
        }

        void OnGUI()
        {
            GUILayout.Label("Vehcile id Settings", EditorStyles.boldLabel);
            vehicleID = EditorGUILayout.TextField("Vehicle id ", vehicleID);
            roadTrunkIdno = EditorGUILayout.TextField("roadTrunkIdno", roadTrunkIdno);

            groupEnabled = EditorGUILayout.BeginToggleGroup("Vehicle Position", groupEnabled);
            myBool = EditorGUILayout.Toggle("Hide Vehicle", myBool);
            myFloat = EditorGUILayout.Slider("Vehicle Absolute Position (m) Slider", myFloat, 0, 1000);
            EditorGUILayout.EndToggleGroup();

            //CreateVehicleGameObject
            if (GUI.Button(new Rect(5, 140, 140, 30), "CreateVehicle"))
            {
                GameObject vd = GameObject.Find("VehicleDisplayerController");

                AssemblyCSharp.VehicleOptima vopt = new AssemblyCSharp.VehicleOptima();
                vopt.position = myFloat;
                vopt.serialId = 1234;
                GameObject roadTrunk = GameObject.Find(roadTrunkIdno);
                vd.GetComponent<VehicleDisplayer>().Init();
                vd.GetComponent<VehicleDisplayer>().CreateVehicleGameObject(vopt, roadTrunk);
                
            }

        }
    }
}
