/* InstantVR Movements
 * author: Pascal Serrarnes
 * email: support@passervr.com
 * version: 3.2.3
 * date: December 29, 2015
 * 
 */

using UnityEngine;

public class IVR_Movements : MonoBehaviour {
    //[HideInInspector]
    public GameObject grabbedObject = null;

	public virtual void StartMovements(InstantVR ivr) {}

	public virtual void UpdateMovements() {}
}
