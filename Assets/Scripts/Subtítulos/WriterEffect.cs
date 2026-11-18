
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

namespace Assets.SurpriseBox.Scripts
{
    public class WriterEffect : MonoBehaviour
    {
        [Header("Typewriter Effect")]
        [SerializeField] private GameObject pnlSubtitle;
        [SerializeField] private TMP_Text txtTypewriteEffect;
        [SerializeField] private TMP_Text txtTypewriteEffectSignature;

        [SerializeField] private bool useTypingSpeed = true;

        [SerializeField, Min(0f)]
        private float typingSpeed = 0.225f;

        [SerializeField, Min(0f)]
        private float initialDelay = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioPlay;

        // Control de los dialogos
        private bool advancePressed;

        private int nextTicket;
        private int currentTicket;

        // Estado de los subtitulos
        public bool EnableSub = false;

        public static WriterEffect Instance { get; private set; }

        // Informacion del dialogo activo
        private bool dialogueActive;
        private string activeText;
        private string activeSignature;

        // ------------------------------------------------
        // INICIALIZACION
        // ------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            EnableSub = PlayerPrefs.GetInt(
                "SubtitlesEnabled", 1
            ) == 1;

            if (audioPlay != null)
            {
                audioPlay.volume = Mathf.Clamp01(
                    PlayerPrefs.GetFloat(
                        "SubtitleVolume", 1f
                    )
                );
            }

            ClearText();

            if (pnlSubtitle == null)
            {
                Debug.LogError(
                    "El panel de subtitulos no esta asignado."
                );
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ------------------------------------------------
        // CONTROLES DEL TECLADO
        // ------------------------------------------------

        private void Update()
        {
            // NUEVO SISTEMA DE ENTRADA
            // No necesita PlayerInputManager ni PlayerInputController

            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    advancePressed = true;
                }
            }

            // Configuracion de subtitulos
            bool subtitlesEnabled = PlayerPrefs.GetInt(
                "SubtitlesEnabled", 1
            ) == 1;

            if (EnableSub != subtitlesEnabled)
            {
                EnableSub = subtitlesEnabled;

                if (!EnableSub)
                {
                    ClearText();
                }
                else if (dialogueActive)
                {
                    if (pnlSubtitle != null)
                    {
                        pnlSubtitle.SetActive(true);
                    }

                    if (txtTypewriteEffect != null)
                    {
                        txtTypewriteEffect.text = activeText;
                    }

                    if (txtTypewriteEffectSignature != null)
                    {
                        txtTypewriteEffectSignature.text =
                            activeSignature;
                    }
                }
            }

            // Control independiente del volumen
            if (audioPlay != null)
            {
                float volume = PlayerPrefs.GetFloat(
                    "SubtitleVolume", 1f
                );

                audioPlay.volume = Mathf.Clamp01(volume);
            }
        }

        // ------------------------------------------------
        // AVANZAR DIALOGOS
        // ------------------------------------------------

        public void NotifyAdvance()
        {
            advancePressed = true;
        }

        private bool ConsumeAdvance()
        {
            if (!advancePressed)
                return false;

            advancePressed = false;
            return true;
        }

        // ------------------------------------------------
        // SISTEMA PRINCIPAL DE DIALOGOS
        // ------------------------------------------------

        public IEnumerator WriterEffectCoroutine(
            SODialogue dialogue,
            bool wait,
            Func<bool> puedeContinuar = null)
        {
            if (dialogue == null)
            {
                Debug.LogWarning(
                    "WriterEffectCoroutine recibio un dialogo nulo."
                );

                yield break;
            }

            if (dialogue.Dialogue == null ||
                dialogue.Dialogue.Length == 0)
            {
                Debug.LogWarning(
                    "El dialogo no contiene textos."
                );

                yield break;
            }

            string[] texts = dialogue.Dialogue;
            float[] durations = dialogue.Duration;
            string signature = dialogue.Signature;

            for (int i = 0; i < texts.Length; i++)
            {
                // Asignar turno en la cola
                int ticket = nextTicket++;

                yield return new WaitUntil(
                    () => ticket == currentTicket
                );

                if (!PuedeContinuar(puedeContinuar))
                {
                    currentTicket++;
                    yield break;
                }

                advancePressed = false;

                // Espera inicial
                if (i == 0 && initialDelay > 0f)
                {
                    yield return EsperarCancelable(
                        initialDelay,
                        puedeContinuar
                    );
                }

                if (!PuedeContinuar(puedeContinuar))
                {
                    CancelarDialogoActual();
                    currentTicket++;
                    yield break;
                }

                string currentText = texts[i];

                bool isLastText = i == texts.Length - 1;

                activeText = currentText;

                activeSignature =
                    isLastText ? signature : "";

                dialogueActive = true;

                // Mostrar subtitulos
                if (pnlSubtitle != null)
                {
                    pnlSubtitle.SetActive(EnableSub);
                }

                if (txtTypewriteEffect != null)
                {
                    txtTypewriteEffect.text = "";
                }

                if (txtTypewriteEffectSignature != null)
                {
                    txtTypewriteEffectSignature.text = "";
                }

                // ----------------------------------------
                // REPRODUCCION DEL AUDIO
                // ----------------------------------------

                if (audioPlay != null &&
                    dialogue.Sound != null &&
                    i < dialogue.Sound.Length &&
                    dialogue.Sound[i] != null)
                {
                    audioPlay.PlayOneShot(
                        dialogue.Sound[i]
                    );
                }

                // ----------------------------------------
                // EFECTO DE ESCRITURA
                // ----------------------------------------

                if (EnableSub)
                {
                    if (useTypingSpeed)
                    {
                        yield return StartCoroutine(
                            TypeText(
                                currentText,
                                txtTypewriteEffect,
                                puedeContinuar
                            )
                        );

                        if (!PuedeContinuar(puedeContinuar))
                        {
                            CancelarDialogoActual();
                            currentTicket++;
                            yield break;
                        }

                        if (isLastText &&
                            !string.IsNullOrEmpty(signature))
                        {
                            yield return StartCoroutine(
                                TypeText(
                                    signature,
                                    txtTypewriteEffectSignature,
                                    puedeContinuar
                                )
                            );
                        }
                    }
                    else
                    {
                        if (txtTypewriteEffect != null)
                        {
                            txtTypewriteEffect.text = currentText;
                        }

                        if (isLastText &&
                            !string.IsNullOrEmpty(signature))
                        {
                            if (txtTypewriteEffectSignature != null)
                            {
                                txtTypewriteEffectSignature.text =
                                    signature;
                            }
                        }
                    }
                }

                if (!PuedeContinuar(puedeContinuar))
                {
                    CancelarDialogoActual();
                    currentTicket++;
                    yield break;
                }

                // ----------------------------------------
                // TIEMPO DE DURACION
                // ----------------------------------------

                if (wait)
                {
                    advancePressed = false;

                    float duration = 0f;

                    if (durations != null &&
                        i < durations.Length)
                    {
                        duration = durations[i];
                    }

                    yield return EsperarCancelable(
                        duration,
                        puedeContinuar
                    );
                }

                if (!PuedeContinuar(puedeContinuar))
                {
                    CancelarDialogoActual();
                    currentTicket++;
                    yield break;
                }

                // ----------------------------------------
                // ESPERAR FINALIZACION DEL AUDIO
                // ----------------------------------------

                if (audioPlay != null)
                {
                    while (audioPlay.isPlaying &&
                           PuedeContinuar(puedeContinuar))
                    {
                        yield return null;
                    }
                }

                if (!PuedeContinuar(puedeContinuar))
                {
                    CancelarDialogoActual();
                    currentTicket++;
                    yield break;
                }

                dialogueActive = false;

                if (isLastText)
                {
                    ClearText();
                }

                // Liberar turno
                currentTicket++;
            }

            if (!wait)
            {
                yield return null;
            }
        }

        // ------------------------------------------------
        // EFECTO DE ESCRITURA DE TEXTO
        // ------------------------------------------------

        private IEnumerator TypeText(
            string text,
            TMP_Text targetText,
            Func<bool> puedeContinuar = null)
        {
            if (targetText == null)
                yield break;

            if (string.IsNullOrEmpty(text))
            {
                targetText.text = "";
                yield break;
            }

            targetText.text = "";

            advancePressed = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (!PuedeContinuar(puedeContinuar))
                {
                    yield break;
                }

                if (!EnableSub)
                {
                    yield break;
                }

                // Completar texto al presionar espacio o enter
                if (ConsumeAdvance())
                {
                    targetText.text = text;
                    break;
                }

                targetText.text += text[i];

                if (typingSpeed > 0f)
                {
                    yield return new WaitForSeconds(typingSpeed);
                }
                else
                {
                    yield return null;
                }
            }
        }

        // ------------------------------------------------
        // CONTROL DE CANCELACION
        // ------------------------------------------------

        private bool PuedeContinuar(
            Func<bool> condicion)
        {
            return condicion == null || condicion();
        }

        // ------------------------------------------------
        // ESPERA CANCELABLE
        // ------------------------------------------------

        private IEnumerator EsperarCancelable(
            float segundos,
            Func<bool> condicion)
        {
            float transcurrido = 0f;

            while (transcurrido < segundos &&
                   PuedeContinuar(condicion))
            {
                transcurrido += Time.deltaTime;
                yield return null;
            }
        }

        // ------------------------------------------------
        // DETENER DIALOGO
        // ------------------------------------------------

        private void CancelarDialogoActual()
        {
            if (audioPlay != null)
            {
                audioPlay.Stop();
            }

            dialogueActive = false;

            activeText = "";
            activeSignature = "";

            ClearText();
        }

        // ------------------------------------------------
        // LIMPIAR SUBTITULOS
        // ------------------------------------------------

        public void ClearText()
        {
            if (txtTypewriteEffect != null)
            {
                txtTypewriteEffect.text = "";
            }

            if (txtTypewriteEffectSignature != null)
            {
                txtTypewriteEffectSignature.text = "";
            }

            if (pnlSubtitle != null)
            {
                pnlSubtitle.SetActive(false);
            }
        }
    }
}