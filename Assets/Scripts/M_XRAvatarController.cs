using UnityEngine;

[System.Serializable]
public class MapTransforms
{
    public Transform vrTarget;
    public Transform ikTarget;

    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void VRMapping()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class M_XRAvatarController : MonoBehaviour
{
    [SerializeField] private MapTransforms head;
    [SerializeField] private MapTransforms leftHand;
    [SerializeField] private MapTransforms rightHand;

    [SerializeField] private float turnSmoothness = 5f;
    [SerializeField] Transform ikHead;
    [SerializeField] Vector3 headBodyOffset;

    [SerializeField] private Animator avatarAnimator;
    [SerializeField] private float playerHeight = 1.8f;
    [SerializeField] private Transform vrCamera;  // Reference to the VR camera

    [SerializeField] private bool useVrCamera = true;  // Toggle to decide whether to use the VR camera

    private Quaternion lastBodyRotation;
    private Vector3 lastPosition;

    private void Start()
    {
        lastBodyRotation = transform.rotation;
        lastPosition = transform.position;

        AdjustAvatarScale();
    }

    private void LateUpdate()
    {
        // Update body position based on head IK position and adjust height
        transform.position = ikHead.position + headBodyOffset;

        if (useVrCamera && vrCamera != null)  // Only use vrCamera if the toggle is true and vrCamera is assigned
        {
            Vector3 adjustedPosition = transform.position;
            adjustedPosition.y = vrCamera.position.y;
            transform.position = adjustedPosition;
        }

        // Body rotation based on head direction
        Quaternion targetBodyRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(ikHead.forward, Vector3.up).normalized);
        lastBodyRotation = Quaternion.Slerp(lastBodyRotation, targetBodyRotation, Time.deltaTime * turnSmoothness);
        transform.rotation = lastBodyRotation;

        // Perform VR mappings
        head.VRMapping();
        leftHand.VRMapping();
        rightHand.VRMapping();

        // Calculate movement speed and update animator
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        avatarAnimator.SetFloat("Speed", speed);

        // Dynamically adjust the lower body animation layer weight
        //int lowerBodyLayerIndex = avatarAnimator.GetLayerIndex("LowerBodyLayer");
        //avatarAnimator.SetLayerWeight(lowerBodyLayerIndex, speed > 0.1f ? 1 : 0); // Activate lower body animations only if speed is significant

        lastPosition = transform.position;
    }

    private void AdjustAvatarScale()
    {
        float scaleMultiplier = playerHeight / 1.8f;
        transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
    }
}
