using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class Player : MonoBehaviour
{

    public bool isGrounded;
    public bool isSprinting;
    public bool isSneaking;

    private Camera playerCamera;
    private World world;

    public float walkSpeed = 4.3f;
    public float sprintSpeed = 5.6f;
    public float sneakSpeed = 1.3f;
    public float jumpForce = 9f;
    public float gravity = -28.4f;
    public float baseMouseHorizontalSpeed = 4f;
    public float baseMouseVerticalSpeed = 1f;

    public float playerWidth = 0.3f;
    public float playerHeight = 1.8f;
    public float normalEyeLevel = 1.6f;
    public float sneakEyeLevel = 1.25f;
    public float eyeLevel = 1.6f;
    public float fieldOfView = 70f;
    public float sprintFieldOfView = 90f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private float verticalDirection;
    private Vector3 velocity;
    float verticalMomentum;
    private bool jumpRequest;

    private MainGameControls gameControls;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction breakAction;
    private InputAction placeAction;
    private InputAction sprintAction;
    private InputAction sneakAction;
    private InputAction mouseScrollAction;
    private InputAction keyboardScrollAction;
    private InputAction inventoryAction;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 4.5f;
    public LayerMask playerCollider;

    public Hotbar hotbar;

    private void Awake()
    {
        gameControls = new MainGameControls();
    }

    private void Start()
    {
        playerCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        world = GameObject.Find("World").GetComponent<World>();
        isSprinting = false;
        isSneaking = false;
        eyeLevel = normalEyeLevel;
        playerCamera.transform.position = transform.position + new Vector3(0f, eyeLevel, 0f);
    }

    private void OnEnable()
    {
        moveAction = gameControls.Player.Move;
        lookAction = gameControls.Player.Look;
        jumpAction = gameControls.Player.Jump;
        breakAction = gameControls.Player.AttackBreak;
        placeAction = gameControls.Player.PlaceUse;
        sprintAction = gameControls.Player.Sprint;
        sneakAction = gameControls.Player.Sneak;
        mouseScrollAction = gameControls.Player.MouseScroll;
        keyboardScrollAction = gameControls.Player.KeyboardScroll;
        inventoryAction = gameControls.Player.CreativeInventory;
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        breakAction.Enable();
        placeAction.Enable();
        sprintAction.Enable();
        sneakAction.Enable();
        mouseScrollAction.Enable();
        keyboardScrollAction.Enable();
        inventoryAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        breakAction.Disable();
        placeAction.Disable();
        sprintAction.Disable();
        sneakAction.Disable();
        mouseScrollAction.Disable();
        keyboardScrollAction.Disable();
        inventoryAction.Disable();
    }

    private void FixedUpdate()
    {
        if (!world.InUI)
        {
            CalculateVelocity();
            if (jumpRequest)
                Jump();

            transform.Rotate(Vector3.up * mouseHorizontal);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalDirection, 0, 0);
            transform.Translate(velocity, Space.World);
        }
    }

    private void Update()
    {
        if (inventoryAction.WasPressedThisFrame())
            world.InUI = !world.InUI;

        if (!world.InUI)
        {
            GetPlayerInputs();
            PlaceCursorBlocks();
        }
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier.
        if (isSneaking)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sneakSpeed;
        else if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && Front) || (velocity.z < 0 && Back))
            velocity.z = 0;
        if ((velocity.x > 0 && Right) || (velocity.x < 0 && Left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);
    }

    private void GetPlayerInputs()
    {
        horizontal = moveAction.ReadValue<Vector2>().x;
        vertical = moveAction.ReadValue<Vector2>().y;
        mouseHorizontal = lookAction.ReadValue<Vector2>().x * baseMouseHorizontalSpeed * world.settings.mouseSensitivity * Time.fixedDeltaTime;
        mouseVertical = lookAction.ReadValue<Vector2>().y * baseMouseVerticalSpeed * world.settings.mouseSensitivity * Time.fixedDeltaTime;
        verticalDirection -= mouseVertical;
        verticalDirection = Mathf.Clamp(verticalDirection, -89.999f, 89.999f);

        if (sprintAction.WasPressedThisFrame())
            isSprinting = !isSprinting;

        if (sneakAction.WasPressedThisFrame())
            isSneaking = !isSneaking;

        if (isGrounded && jumpAction.IsPressed())
            jumpRequest = true;

        playerCamera.transform.position = transform.position + new Vector3(0f, eyeLevel, 0f);

        if (isSneaking)
        {
            playerHeight = 1.5f;
            if (eyeLevel > sneakEyeLevel)
                eyeLevel -= 0.3f * Time.fixedDeltaTime;
            else
                eyeLevel = sneakEyeLevel;

            if (playerCamera.fieldOfView > fieldOfView)
                playerCamera.fieldOfView -= 20 * Time.fixedDeltaTime;
            else
                playerCamera.fieldOfView = fieldOfView;
        }
        else
        {
            playerHeight = 1.8f;
            if (eyeLevel < normalEyeLevel)
                eyeLevel += 0.3f * Time.fixedDeltaTime;
            else
                eyeLevel = normalEyeLevel;

            if (isSprinting)
            {
                if (playerCamera.fieldOfView < sprintFieldOfView)
                    playerCamera.fieldOfView += 20 * Time.fixedDeltaTime;
                else
                    playerCamera.fieldOfView = sprintFieldOfView;
            }
            else
            {
                if (playerCamera.fieldOfView > fieldOfView)
                    playerCamera.fieldOfView -= 20 * Time.fixedDeltaTime;
                else
                    playerCamera.fieldOfView = fieldOfView;
            }
        }

        if (highlightBlock.gameObject.activeSelf)
        {   // Destroy block.
            if (breakAction.WasPressedThisFrame())
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
        }

        if (placeBlock.gameObject.activeSelf)
        {   // Place block.
            if (placeAction.WasPressedThisFrame())
            {
                if (hotbar.slots[hotbar.slotIndex].HasItem)
                {
                    world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, hotbar.slots[hotbar.slotIndex].itemSlot.stack.id);
                    hotbar.slots[hotbar.slotIndex].itemSlot.Take(1);
                }
            }
        }
    }

    private void PlaceCursorBlocks()
    {
        float step = checkIncrement;

        while (step < reach)
        {
            Vector3 _position = playerCamera.transform.position + (playerCamera.transform.forward * step);

            if (world.CheckForVoxel(_position))
            {
                float xCheck;
                float yCheck;
                float zCheck;

                highlightBlock.position = new Vector3(Mathf.FloorToInt(_position.x), Mathf.FloorToInt(_position.y), Mathf.FloorToInt(_position.z));

                xCheck = _position.x % 1;
                if (xCheck > 0.5f)
                    xCheck = xCheck - 1;

                yCheck = _position.y % 1;
                if (yCheck > 0.5f)
                    yCheck = yCheck - 1;
                
                zCheck = _position.z % 1;
                if (zCheck > 0.5f)
                    zCheck = zCheck - 1;

                if (Mathf.Abs(xCheck) < Mathf.Abs(yCheck) && Mathf.Abs(xCheck) < Mathf.Abs(zCheck) && (!world.CheckForVoxel(highlightBlock.position + Vector3.right) || !world.CheckForVoxel(highlightBlock.position + Vector3.left)))
                {
                    // place block on x axis
                    if (xCheck < 0 && !world.CheckForVoxel(highlightBlock.position + Vector3.right))
                        placeBlock.position = highlightBlock.position + Vector3.right;
                    else if (xCheck > 0 && !world.CheckForVoxel(highlightBlock.position + Vector3.left))
                        placeBlock.position = highlightBlock.position + Vector3.left;
                }
                else if (Mathf.Abs(zCheck) < Mathf.Abs(yCheck) && Mathf.Abs(zCheck) < Mathf.Abs(xCheck) && (!world.CheckForVoxel(highlightBlock.position + Vector3.forward) || !world.CheckForVoxel(highlightBlock.position + Vector3.back)))
                {
                    // place block on z axis
                    if (zCheck < 0 && !world.CheckForVoxel(highlightBlock.position + Vector3.forward))
                        placeBlock.position = highlightBlock.position + Vector3.forward;
                    else if (zCheck > 0 && !world.CheckForVoxel(highlightBlock.position + Vector3.back))
                        placeBlock.position = highlightBlock.position + Vector3.back;
                }
                else if (!world.CheckForVoxel(highlightBlock.position + Vector3.up) || !world.CheckForVoxel(highlightBlock.position + Vector3.down))
                {
                    // place block on y axis
                    if (yCheck < 0 && !world.CheckForVoxel(highlightBlock.position + Vector3.up))
                        placeBlock.position = highlightBlock.position + Vector3.up;
                    else if (yCheck > 0 && !world.CheckForVoxel(highlightBlock.position + Vector3.down))
                        placeBlock.position = highlightBlock.position + Vector3.down;
                }

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);
                if (placeBlock.GetComponent<BoxCollider>().bounds.Intersects(GetComponent<BoxCollider>().bounds))
                    placeBlock.gameObject.SetActive(false);

                return;
            }

            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if ((world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))) && (
            world.CheckForVoxel(new Vector3(transform.position.x - (playerWidth * 0.8f), transform.position.y + downSpeed, transform.position.z - (playerWidth * 0.8f))) ||
            world.CheckForVoxel(new Vector3(transform.position.x + (playerWidth * 0.8f), transform.position.y + downSpeed, transform.position.z - (playerWidth * 0.8f))) ||
            world.CheckForVoxel(new Vector3(transform.position.x + (playerWidth * 0.8f), transform.position.y + downSpeed, transform.position.z + (playerWidth * 0.8f))) ||
            world.CheckForVoxel(new Vector3(transform.position.x - (playerWidth * 0.8f), transform.position.y + downSpeed, transform.position.z + (playerWidth * 0.8f)))))
        {
            isGrounded = true;
            verticalMomentum = 0;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }


    private float CheckUpSpeed(float upSpeed)
    {
        if ((world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + upSpeed + playerHeight, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + upSpeed + playerHeight, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + upSpeed + playerHeight, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + upSpeed + playerHeight, transform.position.z + playerWidth))) && (
            world.CheckForVoxel(new Vector3(transform.position.x - (playerWidth * 0.8f), transform.position.y + upSpeed + playerHeight, transform.position.z - (playerWidth * 0.8f))) ||
            world.CheckForVoxel(new Vector3(transform.position.x + (playerWidth * 0.8f), transform.position.y + upSpeed + playerHeight, transform.position.z - (playerWidth * 0.8f))) ||
            world.CheckForVoxel(new Vector3(transform.position.x + (playerWidth * 0.8f), transform.position.y + upSpeed + playerHeight, transform.position.z + (playerWidth * 0.8f))) ||
            world.CheckForVoxel(new Vector3(transform.position.x - (playerWidth * 0.8f), transform.position.y + upSpeed + playerHeight, transform.position.z + (playerWidth * 0.8f)))))
        {
            verticalMomentum = 0;
            return 0;
        }
        else
            return upSpeed;
    }

    public bool Front
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }
    }

    public bool Back
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }
    }

    public bool Left
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }

    public bool Right
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }
    }
}