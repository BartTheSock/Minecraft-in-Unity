using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CreativeInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    World world;
    int pageLength = 45;
    int pageNumber = 1;
    int pageCount;
    int blocksInCreative;

    List<ItemSlot> slots = new List<ItemSlot>();
    private MainGameControls gameControls;
    private InputAction previousPageAction;
    private InputAction nextPageAction;

    private void Awake()
    {
        gameControls = new MainGameControls();
    }

    private void OnEnable()
    {
        previousPageAction = gameControls.Player.PreviousPage;
        nextPageAction = gameControls.Player.NextPage;
        previousPageAction.Enable();
        nextPageAction.Enable();
    }

    private void OnDisable()
    {
        previousPageAction.Disable();
        nextPageAction.Disable();
    }

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        blocksInCreative = world.blockTypes.Count(x => x.appearsInCreative);
        pageCount = Mathf.CeilToInt((float)blocksInCreative / (float)pageLength);
        UpdatePages();
    }

    private void Update()
    {
        if (world.InUI)
        {
            if (previousPageAction.WasPressedThisFrame() && pageNumber > 1)
            {
                pageNumber--;
                UpdatePages();
            }

            if (nextPageAction.WasPressedThisFrame() && pageNumber < pageCount)
            {
                pageNumber++;
                UpdatePages();
            }
        }
    }

    private void UpdatePages()
    {
        int blockIndex = 0;
        slots.Clear();
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < world.blockTypes.Length; i++)
        {
            if (world.blockTypes[i].appearsInCreative)
            {
                blockIndex++;
                if (blockIndex > ((pageNumber - 1) * pageLength) && (blockIndex <= pageNumber * pageLength))
                {
                    GameObject newSlot = Instantiate(slotPrefab, transform);
                    ItemStack stack = new ItemStack((ushort)i, 64);
                    ItemSlot slot = new ItemSlot(newSlot.GetComponent<UIItemSlot>(), stack);
                    slot.isCreative = true;
                    slot.UpdateUISlot();
                }
            }
        }
    }
}