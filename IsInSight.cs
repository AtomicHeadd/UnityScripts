using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class VisionExt
{
    /// <summary>
    /// Returns if "to" object is in vision of "from", based on fov, distance and raycast method.
    /// </summary>
    /// <param name="from">object which "see"</param>
    /// <param name="to">object which is "seen"</param>
    /// <param name="subjectLayer">layers to be considered (e,g, subjects and wall which block line of sight)</param>
    /// <param name="fov">angle that "from" object can see</param>
    /// <param name="maxDistance">distance that "from" object can see</param>
    /// <returns>if "to" object can be seen from "from" object</returns>
    public static bool IsInVision(this GameObject from, GameObject to, LayerMask subjectLayer, float fov=360, float maxDistance=1000)
    {
        Vector3 sub = to.transform.position - from.transform.position;
        if (Vector3.Angle(from.transform.forward, sub) > fov) return false;
        Ray ray = new Ray(from.transform.position, sub);
        if (!Physics.Raycast(ray, out RaycastHit hitinfo, maxDistance, subjectLayer)) return false;
        return hitinfo.collider.gameObject == to.gameObject;
    }
}
