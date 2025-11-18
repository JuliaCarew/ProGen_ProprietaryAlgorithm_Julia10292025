using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPopulatorUI : MonoBehaviour
{
    [Header("UI References - Water Settings")]
    [SerializeField] private TMP_InputField minWaterPointsInputField;
    [SerializeField] private TMP_InputField maxWaterPointsInputField;
    [SerializeField] private TMP_InputField minDistanceBetweenPointsInputField;
    [SerializeField] private TMP_InputField maxDistanceBetweenPointsInputField;

    [Header("UI References - Pond Settings")]
    [SerializeField] private Toggle enablePondsToggle;
    [SerializeField] private TMP_InputField minPondSizeInputField;
    [SerializeField] private TMP_InputField maxPondSizeInputField;
    [SerializeField] private TMP_InputField minFloorTilesForPondInputField;
    [SerializeField] private TMP_InputField pondRadiusInputField;

    [Header("Room Populator Reference")]
    [SerializeField] private RoomPopulator roomPopulator;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        // set up input fields
        if (roomPopulator != null)
        {
            #region Water Generation Input Fields
            // min water points
            if (minWaterPointsInputField != null)
            {
                minWaterPointsInputField.text = roomPopulator.MinWaterPoints.ToString();
                minWaterPointsInputField.onEndEdit.AddListener(OnMinWaterPointsInputChanged);
            }

            // max water points
            if (maxWaterPointsInputField != null)
            {
                maxWaterPointsInputField.text = roomPopulator.MaxWaterPoints.ToString();
                maxWaterPointsInputField.onEndEdit.AddListener(OnMaxWaterPointsInputChanged);
            }

            // min distance between points
            if (minDistanceBetweenPointsInputField != null)
            {
                minDistanceBetweenPointsInputField.text = roomPopulator.MinDistanceBetweenPoints.ToString();
                minDistanceBetweenPointsInputField.onEndEdit.AddListener(OnMinDistanceBetweenPointsInputChanged);
            }

            // max distance between points
            if (maxDistanceBetweenPointsInputField != null)
            {
                maxDistanceBetweenPointsInputField.text = roomPopulator.MaxDistanceBetweenPoints.ToString();
                maxDistanceBetweenPointsInputField.onEndEdit.AddListener(OnMaxDistanceBetweenPointsInputChanged);
            }
            #endregion

            #region Pond Settings Input Fields
            // enable ponds toggle
            if (enablePondsToggle != null)
            {
                enablePondsToggle.isOn = roomPopulator.EnablePonds;
                enablePondsToggle.onValueChanged.AddListener(OnEnablePondsToggleChanged);
            }

            // min pond size
            if (minPondSizeInputField != null)
            {
                minPondSizeInputField.text = roomPopulator.MinPondSize.ToString();
                minPondSizeInputField.onEndEdit.AddListener(OnMinPondSizeInputChanged);
            }

            // max pond size
            if (maxPondSizeInputField != null)
            {
                maxPondSizeInputField.text = roomPopulator.MaxPondSize.ToString();
                maxPondSizeInputField.onEndEdit.AddListener(OnMaxPondSizeInputChanged);
            }

            // min floor tiles for pond
            if (minFloorTilesForPondInputField != null)
            {
                minFloorTilesForPondInputField.text = roomPopulator.MinFloorTilesForPond.ToString();
                minFloorTilesForPondInputField.onEndEdit.AddListener(OnMinFloorTilesForPondInputChanged);
            }

            // pond radius
            if (pondRadiusInputField != null)
            {
                pondRadiusInputField.text = roomPopulator.PondRadius.ToString();
                pondRadiusInputField.onEndEdit.AddListener(OnPondRadiusInputChanged);
            }
            #endregion
        }
    }

    #region Water Generation Input Fields Event Handlers

    void OnMinWaterPointsInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomPopulator.MinWaterPoints = result;
        }
    }

    void OnMaxWaterPointsInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomPopulator.MaxWaterPoints = result;
        }
    }

    void OnMinDistanceBetweenPointsInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result >= 0)
        {
            roomPopulator.MinDistanceBetweenPoints = result;
        }
    }

    void OnMaxDistanceBetweenPointsInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomPopulator.MaxDistanceBetweenPoints = result;
        }
    }

    #endregion

    #region Pond Settings Event Handlers

    void OnEnablePondsToggleChanged(bool value)
    {
        if (roomPopulator != null)
        {
            roomPopulator.EnablePonds = value;
        }
    }

    void OnMinPondSizeInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomPopulator.MinPondSize = result;
        }
    }

    void OnMaxPondSizeInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomPopulator.MaxPondSize = result;
        }
    }

    void OnMinFloorTilesForPondInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result >= 0)
        {
            roomPopulator.MinFloorTilesForPond = result;
        }
    }

    void OnPondRadiusInputChanged(string value)
    {
        if (roomPopulator != null && int.TryParse(value, out int result) && result > 0)
        {
            roomPopulator.PondRadius = result;
        }
    }

    #endregion
}

