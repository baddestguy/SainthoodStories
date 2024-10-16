using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OverideManager))]
public class OverideManagerEditor : Editor
{
    private OverideManager overideManager;

    GUIStyle uIStyle;


    private void OnEnable()
    {
        overideManager = (OverideManager)target;
    }

    private void OnValidate()
    {
        
    }

    private void SetColor(bool open)
    {
        GUI.color = open? Color.white : Color.gray;
    }

    private void SetColorNormal()
    {
        GUI.color = Color.white;
    }

    public override void OnInspectorGUI()
    {
        SetColor(true);
        //base.OnInspectorGUI();
        overideManager = (OverideManager)target;
        bool canClick = Application.isPlaying;

        uIStyle = new GUIStyle(EditorStyles.popup)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        


        DrawTimeDay_Override(canClick);
        DrawFPCPEnergy_Override(canClick);
        DrawProvisionIventory(canClick);
        DrawMissionIdOverride(canClick);



        //bool skip = canClick && EventsManager.Instance != null && EventsManager.Instance.EventInProgress;
        //SetColor(skip);
        //if (GUILayout.Button("Skip >>>>") && skip)
        //{
        //    overideManager.SkipEvent();
        //}
        //SetColor(true);
        this.serializedObject.ApplyModifiedProperties();
    }

    

    private void DrawTimeDay_Override(bool canClick)
    {
        SerializedProperty Season = serializedObject.FindProperty(nameof(overideManager.SeasonOverride));
        SerializedProperty Day = this.serializedObject.FindProperty(nameof(overideManager.DayOverride));
        SerializedProperty Time = serializedObject.FindProperty(nameof(overideManager.TimeOverride));
        //SerializedProperty futureStartTime = serializedObject.FindProperty(nameof(overideManager.FutureStartTime));
        //SerializedProperty futureEndTime = serializedObject.FindProperty(nameof(overideManager.FutureEndTime));


        if (EditorGUILayout.DropdownButton(new GUIContent("Override Day/Time/Weather"), FocusType.Keyboard, uIStyle))
            overideManager.showDayTimeUI = !overideManager.showDayTimeUI;
        if (overideManager.showDayTimeUI)
        {
            EditorGUILayout.PropertyField(Season, new GUIContent(nameof(overideManager.SeasonOverride)));
            EditorGUILayout.PropertyField(Day, new GUIContent(nameof(overideManager.DayOverride)));
            EditorGUILayout.PropertyField(Time, new GUIContent(nameof(overideManager.TimeOverride)));
            //EditorGUILayout.PropertyField(futureStartTime, new GUIContent(nameof(overideManager.FutureStartTime)));
            //EditorGUILayout.PropertyField(futureEndTime, new GUIContent(nameof(overideManager.FutureEndTime)));


            

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Weather", GUILayout.Width(90));
            overideManager.weatherType = (WeatherType)EditorGUILayout.EnumPopup(overideManager.weatherType);
            float min = (float)overideManager.FutureStartTime;
            float max = (float)overideManager.FutureEndTime;
            min = EditorGUILayout.IntField((int)min, GUILayout.Width(50));
            EditorGUILayout.MinMaxSlider(ref min, ref max, 0, 24);
            max = EditorGUILayout.IntField((int)max, GUILayout.Width(50));
            overideManager.FutureStartTime = (int)min;
            overideManager.FutureEndTime = (int)max;
            EditorGUILayout.EndHorizontal();
            

            SetColor(canClick);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Overide Season"))
            {
                if (canClick)
                    overideManager.OverrideSeason();
            }
            if (GUILayout.Button("Overide Time/Day"))
            {
                if (canClick)
                    overideManager.OverideTime();
            }

            if (GUILayout.Button("Overide Weather"))
            {
                if (canClick)
                    overideManager.OverrideWeather();
            }
            if (GUILayout.Button("Run Day/Night Cycle"))
            {
                if (canClick)
                    overideManager.RunDayNightCycle();
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
        }
        SetColor(true);

    }

    private void DrawMissionIdOverride(bool canClick)
    {
        SerializedProperty MissionId = serializedObject.FindProperty(nameof(overideManager.MissionId));
        SerializedProperty HouseType = serializedObject.FindProperty(nameof(overideManager.HouseType));
        SerializedProperty HouseMissionId = serializedObject.FindProperty(nameof(overideManager.HouseMissionId));

        if (EditorGUILayout.DropdownButton(new GUIContent("Override Mission IDs"), FocusType.Keyboard, uIStyle))
            overideManager.showMissionUI = !overideManager.showMissionUI;

        if (overideManager.showMissionUI)
        {

            SetColor(canClick);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.PropertyField(MissionId, new GUIContent(nameof(overideManager.MissionId)));
            if (GUILayout.Button("Overide Mission ID") && canClick)
            {
                overideManager.OverrideMission();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(HouseType, new GUIContent(nameof(overideManager.HouseType)));
            EditorGUILayout.PropertyField(HouseMissionId, new GUIContent(nameof(overideManager.HouseMissionId)));
            if (GUILayout.Button("Overide House Mission ID") && canClick)
            {
                overideManager.OverrideHouseMission();
            }

            GUILayout.Space(10);
        }
        SetColor(true);
    }

    private void DrawFPCPEnergy_Override(bool canClick)
    {
        SerializedProperty FP = serializedObject.FindProperty(nameof(overideManager.FP));
        SerializedProperty CP = serializedObject.FindProperty(nameof(overideManager.CP));
        SerializedProperty Energy = serializedObject.FindProperty(nameof(overideManager.energy));

        if (EditorGUILayout.DropdownButton(new GUIContent("Override CP/FP/Energy"), FocusType.Keyboard, uIStyle))
            overideManager.showCpFpEnUI = !overideManager.showCpFpEnUI;

        if (overideManager.showCpFpEnUI)
        {
            EditorGUILayout.PropertyField(FP, new GUIContent(nameof(overideManager.FP)));
            EditorGUILayout.PropertyField(CP, new GUIContent(nameof(overideManager.CP)));
            EditorGUILayout.PropertyField(Energy, new GUIContent(nameof(overideManager.energy)));



            SetColor(canClick);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Overide CP") && canClick)
            {
                  overideManager.Overide_CP_FP_ENG(true, false, false);
            }
            if (GUILayout.Button("Overide FP") && canClick)
            {
                  overideManager.Overide_CP_FP_ENG(false, true, false);
            }
            if (GUILayout.Button("Overide Energy") && canClick)
            {
                 overideManager.Overide_CP_FP_ENG(false, false, true);
            }
            if (GUILayout.Button("Overide All") && canClick)
            {
                 overideManager.Overide_CP_FP_ENG(true, true, true);
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
        }
        SetColor(true);
    }

    private void DrawProvisionIventory(bool canClick)
    {
        SerializedProperty Inventory = serializedObject.FindProperty(nameof(overideManager.Inventory));
        SerializedProperty Provision = serializedObject.FindProperty(nameof(overideManager.provisions));
        SerializedProperty Saint = serializedObject.FindProperty(nameof(overideManager.NewSaint));
        SerializedProperty RandomSaint = serializedObject.FindProperty(nameof(overideManager.NewSaint));
        SerializedProperty money = serializedObject.FindProperty("Money");


        if (EditorGUILayout.DropdownButton(new GUIContent("Override Provision/Inventory/Saints/Money"), FocusType.Keyboard, uIStyle))
        {
            overideManager.showInvProUI = !overideManager.showInvProUI;
        }

        
        if (overideManager.showInvProUI)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(Inventory, new GUIContent(nameof(overideManager.Inventory)));
            SetColor(canClick);
            if (GUILayout.Button("+") && canClick)
                overideManager.OverrideProvitionInventory(false, true, true);
            if (GUILayout.Button("-") && canClick)
                overideManager.OverrideProvitionInventory(false, true, false);
            SetColor(true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(Provision, new GUIContent(nameof(overideManager.provisions)));
            SetColor(canClick);
            if (GUILayout.Button("+") && canClick)
            {
                overideManager.OverrideProvitionInventory(true, false, true);
            }
            if (GUILayout.Button("-") && canClick)
            {
                overideManager.OverrideProvitionInventory(true, false, false);
            }
            SetColor(true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(Saint, new GUIContent(nameof(overideManager.NewSaint)));
            SetColor(canClick);
            if (GUILayout.Button("Unlock Saint") && canClick)
            {
                overideManager.OverideSaint();
            }

            GUILayout.EndHorizontal();

            SetColor(true); 
            
            GUILayout.BeginHorizontal();
            SetColor(canClick);
            if (GUILayout.Button("Unlock Random Saint") && canClick)
            {
                overideManager.OverideRandomSaint();
            }

            GUILayout.EndHorizontal();

            SetColor(true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(money, new GUIContent("Money"));
            SetColor(canClick);
            if (GUILayout.Button("Add Money") && canClick)
                overideManager.AddMoney();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        SetColor(true);

    }
}
