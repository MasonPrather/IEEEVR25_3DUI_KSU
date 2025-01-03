using UnityEngine;

/// <summary>
/// Project: IEEE VR 2025 - 3D UI Contest
/// Author: Mason Prather
/// Title: Multiplayer - XR Avatar Controller
/// 
/// Description: Handles XR inputs and manipulates the avatar
///              for either full-body or half-body tracking.
/// </summary>
[System.Serializable]
public class MapTransforms
{
    public Transform vrTarget;
    public Transform ikTarget;

    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void ApplyMapping()
    {
        if (vrTarget == null || ikTarget == null)
            return;

        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class M_XRAvatarController : MonoBehaviour
{
    public enum BodyMode { FullBody, HalfBody }

    [SerializeField, Tooltip("Determines whether the avatar uses full-body or half-body tracking.")]
    private BodyMode mode = BodyMode.HalfBody;

    [SerializeField, Tooltip("Mapping for the head's VR and IK targets.")]
    private MapTransforms head;
    [SerializeField, Tooltip("Mapping for the left hand's VR and IK targets.")]
    private MapTransforms leftHand;
    [SerializeField, Tooltip("Mapping for the right hand's VR and IK targets.")]
    private MapTransforms rightHand;

    [SerializeField, Tooltip("Smoothness of avatar rotation when in full-body mode.")]
    private float turnSmoothness = 5f;
    [SerializeField, Tooltip("Head IK target used for rotation and positioning.")]
    private Transform ikHead;
    [SerializeField, Tooltip("Offset for the body position relative to the head.")]
    private Vector3 headBodyOffset;

    //[SerializeField, Tooltip("Animator component for avatar animations.")]
    //private Animator avatarAnimator;

    [SerializeField, Tooltip("The player's height in meters.")]
    private float playerHeight = 1.8f;
    [SerializeField, Tooltip("Reference to the VR camera for tracking.")]
    private Transform vrCamera;

    [SerializeField, Tooltip("Toggle to use the VR camera for head height adjustment.")]
    private bool useVrCamera = true;

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
        if (mode == BodyMode.FullBody)
            UpdateFullBody();
        else
            UpdateHalfBody();
    }

    private void UpdateFullBody()
    {
        // Set body position and height
        Vector3 targetPosition = ikHead.position + headBodyOffset;
        if (useVrCamera && vrCamera != null)
        {
            targetPosition.y = vrCamera.position.y;
        }
        else
        {
            Debug.LogWarning("VR Camera is not assigned!");
        }

        transform.position = targetPosition;

        // Smooth body rotation based on head direction
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(ikHead.forward, Vector3.up).normalized);
        lastBodyRotation = Quaternion.Slerp(lastBodyRotation, targetRotation, Time.deltaTime * turnSmoothness);
        transform.rotation = lastBodyRotation;

        // Map VR inputs to IK targets
        head.ApplyMapping();
        leftHand.ApplyMapping();
        rightHand.ApplyMapping();

        // Update movement speed for animations
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        //avatarAnimator.SetFloat("Speed", speed);

        lastPosition = transform.position;
    }

    private void UpdateHalfBody()
    {
        // Map head and hand positions directly to VR targets
        UpdateTransform(head);
        UpdateTransform(leftHand);
        UpdateTransform(rightHand);

        // Adjust head height based on VR camera
        if (useVrCamera && vrCamera != null)
        {
            Vector3 adjustedPosition = head.ikTarget.position;
            adjustedPosition.y = vrCamera.position.y;
            head.ikTarget.position = adjustedPosition;
        }
        else
        {
            Debug.LogWarning("VR Camera is not assigned!");
        }
    }

    private void UpdateTransform(MapTransforms map)
    {
        if (map.vrTarget == null || map.ikTarget == null)
            return;

        map.ikTarget.position = map.vrTarget.position;
        map.ikTarget.rotation = map.vrTarget.rotation;
    }

    private void AdjustAvatarScale()
    {
        if (playerHeight <= 0)
            return;

        float scaleMultiplier = playerHeight / 1.8f; // 1.8f is the baseline height
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    private void OnDrawGizmos()
    {
        // Visualize the IK targets for debugging
        if (head != null && head.ikTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(head.ikTarget.position, 0.05f);
        }

        if (leftHand != null && leftHand.ikTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(leftHand.ikTarget.position, 0.05f);
        }

        if (rightHand != null && rightHand.ikTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rightHand.ikTarget.position, 0.05f);
        }
    }
}