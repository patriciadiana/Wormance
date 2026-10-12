using UnityEngine;

public class WormController : PlayerEntity
{
    [SerializeField] private PlayerMovement wormMovement;

    protected override void Update()
    {
        base.Update();

        if (wormMovement != null)
        {
            // movement logic here, or call a public method from PlayerMovement
        }
    }
}