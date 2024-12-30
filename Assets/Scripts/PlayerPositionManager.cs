using System.Collections;
using UnityEngine;
/// <summary>
///
/// Project: IEEE VR 2025 - 3D UI Contest
/// Author: Mason Prather
/// Title: Player Position Manager
/// 
/// Description: This script's function is to modify player locations from the hosting session
///                 to two specified transforms. This can be used to transport both players to
///                 different locations, or to the same area, depending on how you use the
///                 functions.
/// 
/// </summary>
public class PlayerPositionManager : MonoBehaviour
{
    public bool IsHost;

    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    //[SerializeField] private CanvasGroup vignetteEffect;

    private float vignetteFadeDuration = 1.0f;

    /// <summary>
    /// Teleports both players to specified locations.
    /// </summary>
    /// <param name="targetTransform1">Target transform for player 1.</param>
    /// <param name="targetTransform2">Target transform for player 2.</param>
    public void TeleportPlayers(Transform targetTransform1, Transform targetTransform2)
    {
        if (!IsHost)
        {
            Debug.LogWarning("Teleportation can only be triggered by the host.");
            return;
        }

        if (targetTransform1 == null || targetTransform2 == null)
        {
            Debug.LogError("One or more target transforms are null.");
            return;
        }

        StartCoroutine(TeleportPlayersWithEffect(targetTransform1, targetTransform2));
    }

    private IEnumerator TeleportPlayersWithEffect(Transform targetTransform1, Transform targetTransform2)
    {
        // Trigger vignette fade-in
        if (vignetteEffect != null)
        {
            StartCoroutine(FadeVignette(1));
            yield return new WaitForSeconds(vignetteFadeDuration);
        }

        // Move players to target locations
        player1.transform.SetPositionAndRotation(targetTransform1.position, targetTransform1.rotation);
        player2.transform.SetPositionAndRotation(targetTransform2.position, targetTransform2.rotation);

        // Simulate a short delay for teleportation
        yield return new WaitForSeconds(0.5f);

        // Trigger vignette fade-out
        if (vignetteEffect != null)
        {
            StartCoroutine(FadeVignette(0));
        }
    }

    private IEnumerator FadeVignette(float targetAlpha)
    {
        if (vignetteEffect == null) yield break;

        float startAlpha = vignetteEffect.alpha;
        float timeElapsed = 0;

        while (timeElapsed < vignetteFadeDuration)
        {
            vignetteEffect.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / vignetteFadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        vignetteEffect.alpha = targetAlpha;
    }
}