using UnityEngine;
using Vuforia;

public class StormBehavioour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public MidAirPositionerBehaviour midAirPositionerBehaviour;

    public void PlaceInMidAir(Vector3 coords)
    {

        Debug.Log("PLACED IN MID AIR");
        if (midAirPositionerBehaviour != null)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(coords);
            float distance = Vector3.Distance(Camera.main.transform.position, coords);
            midAirPositionerBehaviour.DistanceToCamera = distance;
            midAirPositionerBehaviour.ConfirmAnchorPosition(new Vector2(viewportPos.x, viewportPos.y));
        }
    }

    public void OnAnchorPositionConfirmed(Transform anchorTransform)
    {
        Debug.Log($"Anchor transform: {anchorTransform.name} at {anchorTransform.position}");

        // Option A: Keep your current world position
        // this.transform.SetParent(anchorTransform, worldPositionStays: true);

        // Option B: Snap exactly to anchor's position
        this.transform.SetParent(anchorTransform, false);
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        this.gameObject.SetActive(true);

        // Log final position
        Debug.Log($"The name is: {this.gameObject.name}");
        Debug.Log($"Storm active? {this.gameObject.activeInHierarchy}, scale: {this.transform.localScale}");
        Debug.Log($"Storm object final pos: {this.transform.position}, final rot: {this.transform.rotation}");
    }
}
