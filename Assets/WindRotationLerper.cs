using UnityEngine;

[ExecuteAlways]
public class WindRotationLerper : MonoBehaviour
{
    private Vector2 scrollDirection;
    private Material windMaterial;

    float _internalrotation;
    public float rotation = 0;

    // Update is called once per frame
    void Update()
    {

        _internalrotation = Mathf.Lerp(_internalrotation, rotation, Time.deltaTime * 10f);
        float difference = Mathf.Abs(rotation) - Mathf.Abs(_internalrotation);
        difference = Mathf.Clamp(Mathf.Abs(difference)*5, 0, 1);
        Debug.Log(difference);
        
        Shader.SetGlobalFloat("_WindAttenuation", difference);
        Shader.SetGlobalFloat("_Rotate", rotation);


        // Convert rotation to direction
        scrollDirection = RotationToDirection(rotation);

        // Apply scrolling to shader
        Vector2 scrollOffset = scrollDirection;
        Shader.SetGlobalVector("_ScrollOffset", scrollOffset);
    }

    public static Vector2 RotationToDirection(float rotationDegrees)
    {
        // Convert degrees to radians
        float rotationRadians = rotationDegrees * Mathf.Deg2Rad;

        // Calculate direction vector using trigonometry
        // Using negative sin for X to match Unity's coordinate system
        return new Vector2(
            Mathf.Sin(rotationRadians), // X component
            Mathf.Cos(rotationRadians)   // Z component (stored in Y of Vector2)
        ).normalized;
    }

}
