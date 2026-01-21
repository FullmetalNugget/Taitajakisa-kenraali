void FixedUpdate()
{
    Vector2 move = Vector2.zero;

    // GAMEPAD
    if (Gamepad.current != null)
    {
        move = Gamepad.current.leftStick.ReadValue();

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            BeginCharge();

        if (Gamepad.current.buttonSouth.isPressed && charging)
            ChargeFloat();

        if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
            ReleaseJump();
    }

    // KEYBOARD (WASD)
    if (Keyboard.current != null)
    {
        if (Keyboard.current.wKey.isPressed) move.y += 1;
        if (Keyboard.current.sKey.isPressed) move.y -= 1;
        if (Keyboard.current.aKey.isPressed) move.x -= 1;
        if (Keyboard.current.dKey.isPressed) move.x += 1;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            BeginCharge();

        if (Keyboard.current.spaceKey.isPressed && charging)
            ChargeFloat();

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            ReleaseJump();
    }

    move = Vector2.ClampMagnitude(move, 1f);

    Vector3 vel = rb.velocity;
    rb.velocity = new Vector3(move.x * moveSpeed, vel.y, move.y * moveSpeed);

    if (slamming && IsGrounded())
        slamming = false;
}

