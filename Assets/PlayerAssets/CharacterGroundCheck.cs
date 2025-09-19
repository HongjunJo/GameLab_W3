using UnityEngine;

public class CharacterGroundCheck : MonoBehaviour
{

    private bool onGround;

    [Header("Ground Collider")]
    [SerializeField] private float groundLength = 1.0f;
    [SerializeField] private Vector3 colliderOffset;

    [Header("Layer Masks ")]
    [SerializeField] private LayerMask groundLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);
    }

    public bool GetOnGround() { return onGround; }
}
