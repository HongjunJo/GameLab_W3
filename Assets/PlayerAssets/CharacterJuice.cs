using UnityEngine;
using System.Collections;

public class CharacterJuice : MonoBehaviour
{
    [Header("Components")]
    CharacterMove moveScript;
    CharacterJump jumpScript;
    [SerializeField] Animator myAnimator;
    [SerializeField] GameObject characterSprite;

    [Header("Settings - Squash and Stretch")]
    [SerializeField, Tooltip("Width Squeeze, Height Squeeze, Duration")] Vector3 jumpSquashSettings;
    [SerializeField, Tooltip("Width Squeeze, Height Squeeze, Duration")] Vector3 landSquashSettings;
    [SerializeField, Tooltip("How powerful should the effect be?")] public float landSqueezeMultiplier;
    [SerializeField, Tooltip("How powerful should the effect be?")] public float jumpSqueezeMultiplier;
    [SerializeField] float landDrop = 1;

    [Header("Tilting")]
    [SerializeField, Tooltip("How far should the character tilt?")] public float maxTilt;
    [SerializeField, Tooltip("How fast should the character tilt?")] public float tiltSpeed;

    [Header("Calculations")]
    public float runningSpeed;
    public float maxSpeed;

    [Header("Current State")]
    public bool squeezing;
    public bool jumpSqueezing;
    public bool landSqueezing;
    public bool playerGrounded;

    public bool cameraFalling = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveScript = GetComponent<CharacterMove>();
        jumpScript = GetComponent<CharacterJump>();
    }

    void Update()
    {
        tiltCharacter();

        // 애니메이션 속도 맞추기
        // runningSpeed = Mathf.Clamp(Mathf.Abs(moveScript.velocity.x), 0, maxSpeed);

        checkForLanding();

        checkForGoingPastJumpLine();
    }

    private void tiltCharacter()
    {
        float directionToTilt = 0;
        if (moveScript.velocity.x != 0)
        {
            directionToTilt = Mathf.Sign(moveScript.velocity.x);
        }

        Vector3 targetRotVector = new Vector3(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, directionToTilt)));
    }

    private void checkForLanding()
    {
        if (!playerGrounded && jumpScript.onGround)
        {
            playerGrounded = true;
            cameraFalling = false;

            //This is related to the "ignore jumps" option on the camera panel.
            // jumpLine.characterY = transform.position.y;

            if (!landSqueezing && landSqueezeMultiplier > 1)
            {
                StartCoroutine(JumpSqueeze(landSquashSettings.x * landSqueezeMultiplier, landSquashSettings.y / landSqueezeMultiplier, landSquashSettings.z, landDrop, false));
            }

        }
        else if (playerGrounded && !jumpScript.onGround)
        {
            // Player has left the ground, so stop playing the running particles
            playerGrounded = false;
        }
    }

    private void checkForGoingPastJumpLine()
    {
        // 카메라 이동 무시
        /* if (transform.position.y < jumpLine.transform.position.y - 3)
        {
            cameraFalling = true;
        }

        if (cameraFalling)
        {
            jumpLine.characterY = transform.position.y;
        }*/
    }

    IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds, float dropAmount, bool jumpSqueeze)
    {
        if (jumpSqueeze) { jumpSqueezing = true; }
        else { landSqueezing = true; }
        squeezing = true;

        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);

        Vector3 originalPosition = Vector3.zero;
        Vector3 newPosition = new Vector3(0, -dropAmount, 0);

        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            characterSprite.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            characterSprite.transform.localPosition = Vector3.Lerp(originalPosition, newPosition, t);
            yield return null;
        }

        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            characterSprite.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            characterSprite.transform.localPosition = Vector3.Lerp(newPosition, originalPosition, t);
            yield return null;
        }

        if (jumpSqueeze) { jumpSqueezing = false; }
        else { landSqueezing = false; }

        squeezing = false;
    }

    public void jumpEffects()
    {
        if (!jumpSqueezing && jumpSqueezeMultiplier > 1)
        {
            StartCoroutine(JumpSqueeze(jumpSquashSettings.x / jumpSqueezeMultiplier, jumpSquashSettings.y * jumpSqueezeMultiplier, jumpSquashSettings.z, 0, true));

        }
    }
}

