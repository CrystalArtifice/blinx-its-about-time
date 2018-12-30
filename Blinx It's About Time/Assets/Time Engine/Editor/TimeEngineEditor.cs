using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace TimeControl
{
    [CustomEditor(typeof(TimeEngine))]
    public class TimeEngineEditor : Editor
    {
        TimeState oldTS;
        TimeState newTS;

        TimeEngine.RecordingState oldRS;
        TimeEngine.RecordingState newRS;
        
        TimeEngine eng;

        int repaintTrigger = 10;
        int repaintCounter = 0;
        
        void OnEnable()
        {
            eng = target as TimeEngine;
            EditorApplication.update += Update;
        }

        void Update()
        {
            if (repaintCounter-- <= 0)
            {
                repaintCounter = repaintTrigger;
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            oldRS = eng.recordingState;

            newTS = (TimeState)EditorGUILayout.EnumPopup("Global Time State", oldTS);
            EditorGUILayout.Space();
            newRS = (TimeEngine.RecordingState)EditorGUILayout.EnumPopup("Record State", oldRS);
            EditorGUILayout.Space();
            Rect r = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.ProgressBar(r, (float)eng.CurrentSample / (float)eng.MaxSamples, "Buffer Capacity");
            r = GUILayoutUtility.GetRect(18, 18, "TextField");
            EditorGUI.LabelField(r, "Memory usage: " + eng.MemoryUsage.ToSize(ByteUnit.KB) + " KB");

            

            if (oldTS != newTS)
            {
                eng.SetTimeState(newTS);
                oldTS = newTS;
            }

            if(oldRS != newRS)
            {
                eng.recordingState = newRS;
                oldRS = newRS;
            }
        }
    }
}
#endif