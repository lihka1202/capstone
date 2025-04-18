using JetBrains.Annotations;
using UnityEngine;
using Vuforia;

public class QRCodeLocationDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // public TMPro.TextMeshProUGUI barcodeAsText;
    public TMPro.TextMeshProUGUI positionText; // New UI text to display position

    private BarcodeBehaviour mQRcodeBehaviour;

    public Vector3 barcodePosition;

    public GameObject shieldPrefab;

    private GameObject activeShield;
    private RectTransform qrCodeRectTransform;
    void Start()
    {
        mQRcodeBehaviour = GetComponent<BarcodeBehaviour>();
        qrCodeRectTransform = GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {
        if (mQRcodeBehaviour != null && mQRcodeBehaviour.InstanceData != null)
        {
            // Display barcode text
            // barcodeAsText.text = mQRcodeBehaviour.InstanceData.Text;

            // Get the position of the barcode in world space
            Vector3 barcodePosition = mQRcodeBehaviour.transform.position;

            // Display the position in UI
            positionText.text = $"Position: {barcodePosition.ToString("F3")}";

            // Print the position to the console
            // Debug.Log("QR Code Position: " + barcodePosition.ToString("F3"));

            if (activeShield != null)
            {
                Debug.Log(activeShield.transform.position);
            }
            else
            {
                Debug.Log("Active Shield is null");
            }
        }
        else
        {
            // barcodeAsText.text = "";
            positionText.text = "Position: N/A";
        }
    }

    // public void DisplayLocation()
    // {
    //     if (mQRcodeBehaviour != null && mQRcodeBehaviour.InstanceData != null)
    //     {
    //         // Display barcode text
    //         // barcodeAsText.text = mQRcodeBehaviour.InstanceData.Text;

    //         // Get the position of the barcode in world space
    //         Vector3 barcodePosition = mQRcodeBehaviour.transform.position;

    //         // Display the position in UI
    //         positionText.text = $"Position: {barcodePosition.ToString("F3")}";

    //         // Print the position to the console
    //         // Debug.Log("QR Code Position: " + barcodePosition.ToString("F3"));
    //     }
    //     else
    //     {
    //         // barcodeAsText.text = "";
    //         positionText.text = "Position: N/A";
    //     }

    // }

    public void DisplayShield()
    {
        if (shieldPrefab == null)
        {
            Debug.LogError("Shield Prefab not assigned");
            return;
        }

        if (activeShield == null)
        {


            Vector3 worldPosition = qrCodeRectTransform != null
                ? qrCodeRectTransform.position // If it's UI, get its world position
                : transform.position;

            // Instantiate
            activeShield = Instantiate(shieldPrefab, worldPosition, Quaternion.identity);

            // Make sure the size and scale is the same
            activeShield.transform.localScale = transform.lossyScale;

            // Offset the shield slightly in front of the QR code (adjust as needed)
            Vector3 shieldOffset = transform.forward * 0.2f; // Moves the shield slightly forward
            activeShield.transform.position = transform.position + shieldOffset;

            // Make the shield a child of the QR code so it moves with it
            activeShield.transform.SetParent(transform, worldPositionStays: true);

            activeShield.SetActive(true);
            // Debug.Log("Shield activated in front of QR code.");
            Debug.Log("Shield position is: ACTIVATED " + activeShield.transform.position);
        }

    }
}
