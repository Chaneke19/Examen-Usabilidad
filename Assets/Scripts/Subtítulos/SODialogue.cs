using UnityEngine;

namespace Assets.SurpriseBox.Scripts
{
    [CreateAssetMenu(
        fileName = "new dialogue",
        menuName = "Surprise Box/Dialogue"
    )]
    public class SODialogue : ScriptableObject
    {
        [Header("Identificador")]
        public int idDialogue;

        [Header("Textos del dialogo")]
        [TextArea(2, 5)]
        public string[] Dialogue;

        [Header("Duracion de cada texto")]
        public float[] Duration;

        [Header("Audios correspondientes")]
        public AudioClip[] Sound;

        [Header("Firma")]
        public string Signature;

        [Header("Configuracion de repeticion")]
        public bool CanRepeated = false;
    }
}