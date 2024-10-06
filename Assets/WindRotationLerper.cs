using UnityEngine;

[ExecuteAlways]
public class WindRotationLerper : MonoBehaviour
{
    private Vector2 scrollDirection;
    private Material windMaterial;
    private float _internalrotation;

    public float WindRotation;
    public float WindSpeed;
    // Update is called once per frame
    void Update()
    {

        _internalrotation = Mathf.Lerp(_internalrotation, WindRotation, Time.deltaTime * 10f);
        float difference = Mathf.Abs(WindRotation) - Mathf.Abs(_internalrotation);
        difference = Mathf.Clamp(Mathf.Abs(difference)*25, 0, 1);
        Debug.Log(difference);

        _internalrotation = Mathf.Lerp(_internalrotation, WindRotation, Time.deltaTime*25);

        Shader.SetGlobalFloat("_Rotate", _internalrotation);
        Shader.SetGlobalFloat("_WindSpeed", WindSpeed);
        Shader.SetGlobalFloat("_WindAttenuation", difference);


        Shader.SetGlobalVector("_WindParameters", new Vector4(_internalrotation, WindSpeed, difference, 0 ));

        // Convert WindRotation to direction
        scrollDirection = RotationToDirection(WindRotation);


        //TO DO : REPLACE WITH SHADER FUNCTION
        // Apply scrolling to shader
        Vector2 scrollOffset = scrollDirection;
        Shader.SetGlobalVector("_ScrollOffset", scrollOffset);
    }

    public static Vector2 RotationToDirection(float rotationDegrees)
    {
        // Convert degrees to radians
        float rotationRadians = (rotationDegrees - 90) * Mathf.Deg2Rad;

        // Calculate direction vector using trigonometry
        // Using negative sin for X to match Unity's coordinate system
        return new Vector2(
            Mathf.Sin(rotationRadians), // X component
            Mathf.Cos(rotationRadians)   // Z component (stored in Y of Vector2)
        ).normalized;
    }

}
