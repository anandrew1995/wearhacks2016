/* InstantVR Animator
 * author: Pascal Serrarens
 * email: support@passervr.com
 * version: 3.2.0
 * date: November 30, 2015
 * 
 * - Redesigned calibration
 */

using UnityEngine;

public class IVR_AnimatorHead : IVR_Controller {

    public override void StartController(InstantVR ivr) {
        base.StartController(ivr);
        present = true;
        tracking = true;
    }
}
