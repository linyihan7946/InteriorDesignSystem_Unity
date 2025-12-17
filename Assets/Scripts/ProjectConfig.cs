using UnityEngine;


public class ProjectConfig
{
    [Header("Wall Defaults")]
    public float wallDefaultHeight = 2800f;
    public float wallDefaultThickness = 120f;
    public Color wallDefaultColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    [Header("Door / Window Defaults")]
    public float doorDefaultWidth = 0.9f;
    public float doorDefaultHeight = 2.1f;
    public float windowDefaultWidth = 1.2f;
    public float windowDefaultHeight = 1.2f;

    [Header("Floor Defaults")]
    public Color floorDefaultColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Materials (optional)")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material defaultMaterial;

    public static ProjectConfig Instance = new ProjectConfig();
}

