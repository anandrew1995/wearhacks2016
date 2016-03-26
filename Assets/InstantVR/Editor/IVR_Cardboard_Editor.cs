/* InstantVR Cardboard extension editor
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.1.0
 * date: July 18, 2015
 * 
 * - First version
 */

using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(IVR_Cardboard))]
public class IVR_Cardboard_Editor : IVR_Extension_Editor {

    private InstantVR ivr;
    private IVR_Cardboard ivrCardboard;

    private IVR_CardboardHead cardboardHead;

#if UNITY_ANDROID
    public override void OnInspectorGUI() {
        if (PlayerSettings.virtualRealitySupported == true)
            EditorGUILayout.HelpBox("VirtualRealitySupported needs to be DISabled in Player Settings for Cardboard support", MessageType.Warning, true);

        base.OnInspectorGUI();
    }
#endif

    void OnDestroy() {
        if (ivrCardboard == null && ivr != null) {
            cardboardHead = ivr.headTarget.GetComponent<IVR_CardboardHead>();
            if (cardboardHead != null)
                DestroyImmediate(cardboardHead, true);
        }
    }

    void OnEnable() {
        ivrCardboard = (IVR_Cardboard)target;
        ivr = ivrCardboard.GetComponent<InstantVR>();

        if (ivr != null) {
            cardboardHead = ivr.headTarget.GetComponent<IVR_CardboardHead>();
            if (cardboardHead == null) {
                cardboardHead = ivr.headTarget.gameObject.AddComponent<IVR_CardboardHead>();
                cardboardHead.extension = ivrCardboard;
            }

            IVR_Extension[] extensions = ivr.GetComponents<IVR_Extension>();
            if (ivrCardboard.priority == -1)
                ivrCardboard.priority = extensions.Length - 1;
            for (int i = 0; i < extensions.Length; i++) {
                if (ivrCardboard == extensions[i]) {
                    while (i < ivrCardboard.priority) {
                        MoveUp(cardboardHead);
                        ivrCardboard.priority--;
                        //Debug.Log ("Cardboard Move up to : " + i + " now: " + ivrRift.priority);
                    }
                    while (i > ivrCardboard.priority) {
                        MoveDown(cardboardHead);
                        ivrCardboard.priority++;
                        //Debug.Log ("Cardboard Move down to : " + i + " now: " + ivrRift.priority);
                    }
                }
            }
        }
    }
}
