using UnityEngine;

public class Collisions : MonoBehaviour {
    [SerializeField] LayerMask groundLayers;
    static Bounds _bounds;
    const float VerticalRays = 3f;
    const float HorizontalRays = 5f;
    const float RaycastOffset = 0.05f;
    
    // public static void CheckForCollisions(Bounds bounds, Vector3 movement, bool onGround) {
    //     _bounds = bounds;
    //     bool movingHorizontally = movement.x != 0;
    //     bool movingDown = movement.y < 0;
    //     bool movingUp = movement.y > 0;
    //     bool movingOnGround = movement.y == 0 && movingHorizontally;
    //
    //     if (movingHorizontally || !onGround)
    //         WallCheck();
    //
    //     if (movingDown || movingOnGround)
    //         GroundCheck();
    //     else if (movingUp)
    //         CeilingCheck();
    // }
    //
    // void VerticalCollisionCheck(Vector3 direction, out bool collision, ref PlayerController player) {
    //     var startPoint = new Vector2(_bounds.min.x + RaycastOffset, _bounds.center.y);
    //     var endPoint = new Vector2(_bounds.max.x - RaycastOffset, _bounds.center.y);
    //     float rayLength = _bounds.extents.y + Mathf.Abs(player._movement.y);
    //     
    //     collision = DoCollisionCheck(startPoint, endPoint, direction, rayLength, VerticalRays, out RaycastHit2D hit);
    //     
    //     if (collision)
    //         player.AdjustPosition(direction, hit.distance, player.Axis.Vertical);
    // }
    //
    // bool DoCollisionCheck(Vector2 startPoint, Vector2 endPoint, Vector3 direction, float rayLength, float rayAmount, out RaycastHit2D hit) {
    //     hit = new RaycastHit2D();
    //     
    //     for (int i = 0; i < rayAmount; i++) {
    //         Vector2 origin = Vector2.Lerp(startPoint, endPoint, i / (rayAmount - 1));
    //         hit = Physics2D.Raycast(origin, direction, rayLength, groundLayers);
    //         Debug.DrawRay(origin, direction, Color.cyan, 1f);
    //
    //         if (hit.collider == null) continue;
    //         return true;
    //     }
    //     return false;
    // }
}
