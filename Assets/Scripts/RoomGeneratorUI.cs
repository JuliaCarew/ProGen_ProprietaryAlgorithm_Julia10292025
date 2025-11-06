using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomGeneratorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button regenerateButton;
    [SerializeField] private TMP_InputField roomsInputField;
    [SerializeField] private TMP_InputField gridWidthInputField;
    [SerializeField] private TMP_InputField gridDepthInputField;
    [SerializeField] private TMP_InputField minRoomWidthInputField;
    [SerializeField] private TMP_InputField maxRoomWidthInputField;
    [SerializeField] private TMP_InputField minRoomDepthInputField;
    [SerializeField] private TMP_InputField maxRoomDepthInputField;
    [SerializeField] private TMP_InputField minDistanceInputField;

    [Header("Room Generator Reference")]
    [SerializeField] private RoomGenerator roomGenerator;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        // set up button 
        if (regenerateButton != null)
        {
            regenerateButton.onClick.AddListener(OnRegenerateButtonClicked);
        }

        // set up input fields
        if (roomGenerator != null)
        {
            #region Generation Input Fields
            // rooms to generate
            if (roomsInputField != null)
            {
                roomsInputField.text = roomGenerator.RoomsToGenerate.ToString();
                roomsInputField.onEndEdit.AddListener(OnRoomsInputChanged);
            }

            // grid width
            if (gridWidthInputField != null)
            {
                gridWidthInputField.text = roomGenerator.GridWidth.ToString();
                gridWidthInputField.onEndEdit.AddListener(OnGridWidthInputChanged);
            }

            // grid depth
            if (gridDepthInputField != null)
            {
                gridDepthInputField.text = roomGenerator.GridDepth.ToString();
                gridDepthInputField.onEndEdit.AddListener(OnGridDepthInputChanged);
            }

            // min room width
            if (minRoomWidthInputField != null)
            {
                minRoomWidthInputField.text = roomGenerator.MinRoomWidth.ToString();
                minRoomWidthInputField.onEndEdit.AddListener(OnMinRoomWidthInputChanged);
            }

            // max room width
            if (maxRoomWidthInputField != null)
            {
                maxRoomWidthInputField.text = roomGenerator.MaxRoomWidth.ToString();
                maxRoomWidthInputField.onEndEdit.AddListener(OnMaxRoomWidthInputChanged);
            }

            // min room depth
            if (minRoomDepthInputField != null)
            {
                minRoomDepthInputField.text = roomGenerator.MinRoomDepth.ToString();
                minRoomDepthInputField.onEndEdit.AddListener(OnMinRoomDepthInputChanged);
            }

            // max room depth
            if (maxRoomDepthInputField != null)
            {
                maxRoomDepthInputField.text = roomGenerator.MaxRoomDepth.ToString();
                maxRoomDepthInputField.onEndEdit.AddListener(OnMaxRoomDepthInputChanged);
            }

            // min distance between rooms
            if (minDistanceInputField != null)
            {
                minDistanceInputField.text = roomGenerator.MinDistanceBetweenRooms.ToString();
                minDistanceInputField.onEndEdit.AddListener(OnMinDistanceInputChanged);
            }

            #endregion
        }
    }

    #region Generation Input Fields Event Handlers

    void OnRegenerateButtonClicked()
    {
        if (roomGenerator != null)
        {
            roomGenerator.GenerateDungeon();
        }
    }

    void OnRoomsInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomGenerator.RoomsToGenerate = result;
        }
    }

    void OnGridWidthInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomGenerator.GridWidth = result;
        }
    }

    void OnGridDepthInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomGenerator.GridDepth = result;
        }
    }

    void OnMinRoomWidthInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomGenerator.MinRoomWidth = result;
        }
    }

    void OnMaxRoomWidthInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomGenerator.MaxRoomWidth = result;
        }
    }

    void OnMinRoomDepthInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomGenerator.MinRoomDepth = result;
        }
    }

    void OnMaxRoomDepthInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomGenerator.MaxRoomDepth = result;
        }
    }

    void OnMinDistanceInputChanged(string value)
    {
        if (roomGenerator != null && int.TryParse(value, out int result) && result >= 0)
        {
            roomGenerator.MinDistanceBetweenRooms = result;
        }
    }

    #endregion
}

