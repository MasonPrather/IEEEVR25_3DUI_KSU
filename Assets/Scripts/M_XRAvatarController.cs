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
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class M_XRAvatarController : MonoBehaviour
{
    public enum BodyMode { FullBody, HalfBody }
    [SerializeField] private BodyMode mode = BodyMode.HalfBody;

    [SerializeField] private MapTransforms head;
    [SerializeField] private MapTransforms leftHand;
    [SerializeField] private MapTransforms rightHand;

    [SerializeField] private float turnSmoothness = 5f;
    [SerializeField] private Transform ikHead;
    [SerializeField] private Vector3 headBodyOffset;

    //[SerializeField] private Animator avatarAnimator;
    [SerializeField] private float playerHeight = 1.8f;
    [SerializeField] private Transform vrCamera;

    [SerializeField] private bool useVrCamera = true;

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
            targetPosition.y = vrCamera.position.y;
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
    }

    private void UpdateTransform(MapTransforms map)
    {
        map.ikTarget.position = map.vrTarget.position;
        map.ikTarget.rotation = map.vrTarget.rotation;
    }

    private void AdjustAvatarScale()
    {
        //float scaleMultiplier = playerHeight / 1.8f;
        //transform.localScale = Vector3.one * scaleMultiplier;
    }
}