using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>Cortina que escurece a tela, mostra uma mensagem grande e clareia de novo.</summary>
    public class Transicao : MonoBehaviour
    {
        private CanvasGroup grupo;
        private Text titulo;
        private Text subtitulo;

        public bool EmAndamento { get; private set; }

        public void Construir()
        {
            var raiz = UIFabrica.Esticar((RectTransform)transform);
            var fundo = UIFabrica.CriarImagem("Fundo", raiz, null, UIFabrica.Noite, true);
            UIFabrica.Esticar(fundo.rectTransform);

            titulo = UIFabrica.Sombra(UIFabrica.CriarTexto("Titulo", raiz, "", 96, UIFabrica.Amarelo, TextAnchor.MiddleCenter, true), 6f, 0.6f);
            UIFabrica.Posicionar(titulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(1700f, 220f));
            subtitulo = UIFabrica.CriarTexto("Subtitulo", raiz, "", 40, UIFabrica.Creme);
            UIFabrica.Posicionar(subtitulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(1500f, 120f));

            grupo = gameObject.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;
            gameObject.SetActive(false);
        }

        /// <summary>Escurece, executa <paramref name="noMeio"/> com a tela coberta e clareia.</summary>
        public void Executar(string textoTitulo, string textoSubtitulo, float tempoNaTela, Action noMeio, Action noFim = null)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StopAllCoroutines();
            StartCoroutine(Rotina(textoTitulo, textoSubtitulo, tempoNaTela, noMeio, noFim));
        }

        private IEnumerator Rotina(string t1, string t2, float tempoNaTela, Action noMeio, Action noFim)
        {
            EmAndamento = true;
            titulo.text = t1;
            subtitulo.text = t2;
            yield return Animar(grupo.alpha, 1f, 0.3f);
            noMeio?.Invoke();
            transform.SetAsLastSibling();

            for (float t = 0f; t < tempoNaTela; t += Time.unscaledDeltaTime)
            {
                titulo.rectTransform.localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(t * 6f));
                yield return null;
            }
            yield return Animar(1f, 0f, 0.35f);
            EmAndamento = false;
            gameObject.SetActive(false);
            noFim?.Invoke();
        }

        private IEnumerator Animar(float de, float para, float duracao)
        {
            for (float t = 0f; t < duracao; t += Time.unscaledDeltaTime)
            {
                grupo.alpha = Mathf.Lerp(de, para, t / duracao);
                yield return null;
            }
            grupo.alpha = para;
        }
    }
}
