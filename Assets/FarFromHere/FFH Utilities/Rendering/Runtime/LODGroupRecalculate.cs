using UnityEngine;

namespace FFH.Utilities.Rendering
{
    public class LODGroupRecalculate : MonoBehaviour
    {
        private LODGroup[] lodGroups;

        public void OnButtonClick()
        {
            lodGroups = gameObject.GetComponentsInChildren<LODGroup>();
            foreach (LODGroup lodGroup in lodGroups)
            {
                lodGroup.RecalculateBounds();
            }
        }
    }
}