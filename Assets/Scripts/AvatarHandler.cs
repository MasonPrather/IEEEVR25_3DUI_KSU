using ReadyPlayerMe.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer;

public class AvatarHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Set this to the URL or shortcode of the Ready Player Me Avatar you want to load.")]
    private string avatarUrl = "https://models.readyplayer.me/638df693d72bffc6fa17943c.glb";

    [SerializeField]
    [Tooltip("Reference to the XR Rig GameObject.")]
    private GameObject xrRig;

    private GameObject avatar;

    private void Start()
    {
        ApplicationData.Log();
        var avatarLoader = new AvatarObjectLoader();
        // use the OnCompleted event to set the avatar and setup animator
        avatarLoader.OnCompleted += (_, args) =>
        {
            avatar = args.Avatar;
            AvatarAnimationHelper.SetupAnimator(args.Metadata, args.Avatar);
            // Set the avatar as a child of the XR Rig
            avatar.transform.SetParent(xrRig.transform, false);
            // Reset the avatar's local position and rotation to match its parent
            avatar.transform.localPosition = Vector3.zero;
            avatar.transform.localRotation = Quaternion.identity;

            // Automatically add components to the avatar
            AddRequiredComponents(avatar);
        };
        avatarLoader.LoadAvatar(avatarUrl);
    }

    private void AddRequiredComponents(GameObject avatar)
    {
        // Add the XRAvatarIK component to the avatar
        //M_XRAvatarIK avatarIK = avatar.AddComponent<M_XRAvatarIK>();

        // Find and assign the skeleton joints from the avatar's hierarchy
        //avatarIK.m_HeadTransform = avatar.transform.Find("Armature/Hips/Spine/Spine1/Spine2/Neck/Head");
        //avatarIK.m_TorsoParentTransform = avatar.transform.Find("Armature/Hips/Spine/Spine1/Spine2");
        //avatarIK.m_HeadVisualsRoot = avatar.transform.Find("Armature/Hips/Spine/Spine1/Spine2/Neck/Head");
        //avatarIK.m_Neck = avatar.transform.Find("Armature/Hips/Spine/Spine1/Spine2/Neck");
    }

    private void OnDestroy()
    {
        if (avatar != null) Destroy(avatar);
    }
}
