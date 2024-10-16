using UnityEngine;
using UnityEngine.UI;

namespace Valkyrie.EIR.Examples
{
    /// <summary>
    /// Button that communicates with FeelManager to play feelings
    /// </summary>
    public class FeelButton : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        private void Start()
        {
#if EIR_HAPTICS
            if(FeelManager.instance != null)
                FeelManager.instance.onFeelingStatusChange += OnFeelingStatusEvent;
#endif
        }
        #if EIR_HAPTICS
        private void OnDestroy()
        {
            if (FeelManager.instance != null && FeelManager.instance.onFeelingStatusChange != null)
                FeelManager.instance.onFeelingStatusChange -= OnFeelingStatusEvent;
        }
#endif

        void OnFeelingStatusEvent(bool status)
        {
            //Debug.Log("Feelstatus " + status.ToString());
            button.interactable = !status;
        }

        public void PlayFeeling(string feelID)
        {
            #if EIR_HAPTICS
            if (FeelManager.instance != null)
                FeelManager.instance.PlayFeeling(feelID);
#endif
        }
    }

    
}

