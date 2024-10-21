using UnityEngine;

namespace FFH.HighDefinition.Extension
{
    [ExecuteAlways]
    public class FFHWindManager : MonoBehaviour
    {
        private Vector2 scrollDirection;
        private Material windMaterial;
        private float _targetRotation;
        private float _internalrotation;
        private float WindRotation;

        public float WindSpeed;
        public float WindAmplitude;

        // Update is called once per frame
        void Update()
        {
            WindRotation = transform.rotation.eulerAngles.y;

            _internalrotation = Mathf.Lerp(_internalrotation, WindRotation, Time.deltaTime * 2f);
            float difference = Mathf.Abs(WindRotation) - Mathf.Abs(_internalrotation);
            difference = Mathf.Clamp(Mathf.Abs(difference) * 25, 0, 1);

            //TO DO: WindAmplitude
            Shader.SetGlobalVector("_WindParameters", new Vector4(WindRotation, WindSpeed, difference, WindAmplitude));
        }
    }
}