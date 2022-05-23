using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    public Transform playerCamera;
    public Transform portal;
    public Transform tpTarget;
    public bool portalFinal;


    private void LateUpdate()
    {
        if (!portalFinal)
        {
            UpdatePortalPosition();
        }
        else if (tpTarget.gameObject.activeSelf)
        {
            UpdatePortalPosition();
        }
    }

    private void UpdatePortalPosition()
    {
        Vector3 playerOffsetFromPortal = playerCamera.position - tpTarget.position;
        transform.position = new Vector3(portal.position.x + playerOffsetFromPortal.x, portal.position.y + playerOffsetFromPortal.y, transform.position.z);

        float angularDifferenceBetweenPortalRotations = Quaternion.Angle(portal.rotation, tpTarget.rotation);

        Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenPortalRotations, Vector3.up);
        Vector3 newCameraRotation = portalRotationalDifference * playerCamera.forward;
        transform.rotation = Quaternion.LookRotation(newCameraRotation, Vector3.up);
    }
}
