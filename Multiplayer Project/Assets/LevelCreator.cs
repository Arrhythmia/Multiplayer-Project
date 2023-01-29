using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCreator : MonoBehaviour
{
    public GameObject[] prefabs;
    public GameObject selectedPrefab;
    public GameObject canvas;
    public Dropdown prefabDropdown;
    public InputField xScaleInput, yScaleInput;

    private GameObject currentObject;

    void Start()
    {
        // Fill the dropdown with the names of the prefabs
        prefabDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < prefabs.Length; i++)
        {
            options.Add(prefabs[i].name);
        }
        prefabDropdown.AddOptions(options);
        selectedPrefab = prefabs[0];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !canvas.activeSelf)
        {
            // When the player left clicks, instantiate the selected prefab at the mouse position
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            currentObject = Instantiate(selectedPrefab, new Vector3(worldPos.x, worldPos.y, 0), Quaternion.identity);
        }

        if (Input.GetMouseButtonDown(1))
        {
            // When the player right clicks, show the scale change UI

            canvas.SetActive(true);
        }
    }

    public void ChangeSelectedPrefab(int index)
    {
        // Update the selected prefab when the dropdown value changes
        selectedPrefab = prefabs[index];
    }

    public void ChangeScale()
    {
        // Change the scale of the current object using the values from the input fields
        currentObject.transform.localScale = new Vector3(float.Parse(xScaleInput.text), float.Parse(yScaleInput.text), 1);

        canvas.SetActive(false);
    }

    public void Save()
    {
        // Save the current level using PlayerPrefs or another method
        // code here
    }
}
