using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DragAndDropHandler : MonoBehaviour
{

    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem = null;

    World world;

    private MainGameControls gameControls;
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction shiftAction;

    private void Awake()
    {
        gameControls = new MainGameControls();
    }

    private void OnEnable()
    {
        leftClickAction = gameControls.Player.AttackBreak;
        rightClickAction = gameControls.Player.PlaceUse;
        shiftAction = gameControls.Player.Sneak;
        leftClickAction.Enable();
        rightClickAction.Enable();
        shiftAction.Enable();
    }

    private void OnDisable()
    {
        leftClickAction.Disable();
        rightClickAction.Disable();
        shiftAction.Disable();
    }

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update()
    {
        if (!world.InUI)
            return;

        cursorSlot.transform.position = Mouse.current.position.ReadValue();

        if (leftClickAction.WasPressedThisFrame() || rightClickAction.WasPressedThisFrame())
            HandleSlotClick(CheckForSlot(), rightClickAction.WasPressedThisFrame());
    }

    private void HandleSlotClick(UIItemSlot clickedSlot, bool rightClicked)
    {
        if (clickedSlot == null)
            return;

        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
            return;

        if (clickedSlot.itemSlot.isCreative)
        {
            if (!cursorSlot.HasItem || (cursorSlot.HasItem && cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id))
            {
                cursorItemSlot.EmptySlot();
                if (shiftAction.IsPressed())
                    cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
                else
                    cursorItemSlot.InsertSingle(clickedSlot.itemSlot.stack);
            }
            else if (cursorSlot.HasItem && cursorSlot.itemSlot.stack.id == clickedSlot.itemSlot.stack.id)
            {
                if (shiftAction.IsPressed())
                {
                    cursorItemSlot.EmptySlot();
                    cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
                }
                else
                {
                    if (cursorSlot.itemSlot.stack.amount < world.blockTypes[cursorSlot.itemSlot.stack.id].maxStackSize && !rightClicked)
                        cursorItemSlot.InsertStack(new ItemStack(cursorSlot.itemSlot.stack.id, cursorSlot.itemSlot.stack.amount + 1));
                    else if (rightClicked)
                    {
                        if (cursorSlot.itemSlot.stack.amount > 1)
                            cursorItemSlot.InsertStack(new ItemStack(cursorSlot.itemSlot.stack.id, cursorSlot.itemSlot.stack.amount - 1));
                        else
                            cursorItemSlot.EmptySlot();
                    }
                }
            }
            return;
        }

        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            if (rightClicked && clickedSlot.itemSlot.stack.amount > 1)
                cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeHalf());
            else
                cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            if (rightClicked && cursorSlot.itemSlot.stack.amount > 1)
                clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeOne());
            else
                clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {   // if there is an item stack in the cursor AND in the clicked slot
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
            {   // items carried and in slot are differents
                ItemStack previousCursorSlotStack = cursorSlot.itemSlot.TakeAll();
                ItemStack previousClickedSlotStack = clickedSlot.itemSlot.TakeAll();

                // swap/exchange the cursor and clicked slot stacks
                clickedSlot.itemSlot.InsertStack(previousCursorSlotStack);
                cursorSlot.itemSlot.InsertStack(previousClickedSlotStack);
                return;
            }
            else
            {
                ushort clickedSlotItemID = clickedSlot.itemSlot.stack.id;  // gets the id of item
                int maxStackSize = world.blockTypes[clickedSlotItemID].maxStackSize;  // gets the maxStackSize for this blocktype/item ID

                if (!rightClicked)
                {
                    ItemStack previousCursorSlotStack = cursorSlot.itemSlot.TakeAll();
                    ItemStack previousClickedSlotStack = clickedSlot.itemSlot.TakeAll();
                    int combinedClickedSlotStackAmount = previousClickedSlotStack.amount + previousCursorSlotStack.amount;
                    if (combinedClickedSlotStackAmount > maxStackSize)
                    {
                        ItemStack combinedClickedSlotStackFull = new ItemStack(clickedSlotItemID, maxStackSize);
                        ItemStack combinedClickedSlotStackLeftover = new ItemStack(clickedSlotItemID, combinedClickedSlotStackAmount - maxStackSize);
                        clickedSlot.itemSlot.InsertStack(combinedClickedSlotStackFull);
                        cursorSlot.itemSlot.InsertStack(combinedClickedSlotStackLeftover);
                        return;
                    }
                    else
                    {
                        ItemStack combinedClickedSlotStack = new ItemStack(clickedSlotItemID, combinedClickedSlotStackAmount);
                        clickedSlot.itemSlot.InsertStack(combinedClickedSlotStack);
                        return;
                    }
                }
                else
                {
                    if (clickedSlot.itemSlot.stack.amount < world.blockTypes[clickedSlot.itemSlot.stack.id].maxStackSize)
                    {
                        clickedSlot.itemSlot.InsertStack(new ItemStack(clickedSlot.itemSlot.stack.id, clickedSlot.itemSlot.stack.amount + 1));
                        if (cursorSlot.itemSlot.stack.amount > 1)
                            cursorItemSlot.InsertStack(new ItemStack(cursorSlot.itemSlot.stack.id, cursorSlot.itemSlot.stack.amount - 1));
                        else
                            cursorItemSlot.EmptySlot();
                    }
                    return;
                }
            }
        }
    }

    private UIItemSlot CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.tag == "UIItemSlot")
                return result.gameObject.GetComponent<UIItemSlot>();
        }

        return null;
    }
}