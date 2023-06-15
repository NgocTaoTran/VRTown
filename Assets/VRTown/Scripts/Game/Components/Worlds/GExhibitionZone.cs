using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTown.Game.UI;
using VRTown.Model;
using Zenject;

namespace VRTown.Game.Component
{
    public class GExhibitionZone : MonoBehaviour
    {
        [SerializeField] string _nameZone;
        [SerializeField] opencloseDoor _opencloseDoor;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == C.TagConfig.PlayerTag)
            {
                Debug.Log("[GExhibitionZone] Player joined Sunloft");
                GHelper.Signal.Fire(new ForceVoiceSignal(_nameZone, true));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == C.TagConfig.PlayerTag)
            {
                Debug.Log("[GExhibitionZone] Player leave sunloft");
                GHelper.Signal.Fire(new ForceVoiceSignal(_nameZone, false));
                StartCoroutine(Close());
            }
        }

        IEnumerator Close()
        {
            yield return new WaitForSeconds(2.0f);
            _opencloseDoor.Close();
        }
    }
}