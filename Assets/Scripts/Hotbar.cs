using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Hotbar : MonoBehaviour
{
    public UIItemSlot[] slots;
    public RectTransform highlight;
    public Player player;
    public Text selectedBlockText;
    public Text selectedBlockTextShadow;
    public int slotIndex = 0;

    private MainGameControls gameControls;
    private InputAction mouseScrollAction;
    private InputAction keyboardScrollAction;

    World world;

    private void Awake()
    {
        gameControls = new MainGameControls();
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void OnEnable()
    {
        mouseScrollAction = gameControls.Player.MouseScroll;
        keyboardScrollAction = gameControls.Player.KeyboardScroll;
        mouseScrollAction.Enable();
        keyboardScrollAction.Enable();
    }

    private void OnDisable()
    {
        mouseScrollAction.Disable();
        keyboardScrollAction.Disable();
    }

    private void Start()
    {
        ushort index = 1;
        foreach (UIItemSlot s in slots)
        {
            ItemSlot slot = new ItemSlot(s, null);
            index++;
        }
    }

    private void Update()
    {
        float mouseScroll = mouseScrollAction.ReadValue<float>();
        float keyboardScroll = keyboardScrollAction.ReadValue<float>();

        if (mouseScrollAction.IsPressed() || keyboardScrollAction.WasPressedThisFrame())
        {
            if ((mouseScroll == 0 && keyboardScroll > 0) || (mouseScroll > 0 && keyboardScroll == 0) || (mouseScroll > 0 && keyboardScroll > 0))
                slotIndex++;
            else if ((mouseScroll == 0 && keyboardScroll < 0) || (mouseScroll < 0 && keyboardScroll == 0) || (mouseScroll < 0 && keyboardScroll < 0))
                slotIndex--;

            if (slotIndex > slots.Length - 1)
                slotIndex = 0;

            if (slotIndex < 0)
                slotIndex = slots.Length - 1;

            highlight.position = slots[slotIndex].slotIcon.transform.position;
        }

        if (slots[slotIndex].itemSlot.stack != null)
            selectedBlockText.text = world.blockTypes[slots[slotIndex].itemSlot.stack.id].blockName;
        else
            selectedBlockText.text = "";
        selectedBlockTextShadow.text = selectedBlockText.text;
    }
}