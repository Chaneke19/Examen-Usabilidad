using UnityEngine;
using Jsgaona;

namespace Assets.SurpriseBox.Scripts
{
    public class ActiveSubtitle : MonoBehaviour
    {
        [Header("Configuracion del dialogo")]
        [SerializeField] private SODialogue dialogue;
        [SerializeField] private bool status = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (dialogue == null)
                return;

            if (WriterEffect.Instance == null)
                return;
        }


    }
}