using UnityEngine;
using Vuforia;

public class AnchorHolder : MonoBehaviour
{
    public AnchorBehaviour anchorBehaviour;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {

    // }

    // // Update is called once per frame
    // void Update()
    // {

    // }

    public void AnchorToLocation()
    {
        anchorBehaviour.ConfigureAnchor("holder", transform.position, Quaternion.identity);
    }
}
