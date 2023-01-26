using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Texture2D map;
    [SerializeField] private ColorToPrefab[] colorMappings;
    void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        for (int x = 0; x < map.width; x++)
        {
            for (int y = 0; y < map.height; y++)
            {
                GenerateTile(x, y);
            }
        }
    }

    private void GenerateTile(int x, int y)
    {
        Color pixelColor = map.GetPixel(x, y);

        if (pixelColor.a == 0)
        {
            return; // Ignore transparent pixel
        }

        Debug.Log(pixelColor);

        foreach (ColorToPrefab colorMapping in colorMappings)
        {
            if (colorMapping.color.Equals(pixelColor))
            {
                Vector3 pos = new Vector3(x, y, 0);
                Instantiate(colorMapping.prefab, pos, Quaternion.identity, transform);
            }
        }
    }
}
