using System.Collections;
using UnityEngine;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// Classe base (abstrata) de todas as telas. Cada tela filha só precisa
    /// implementar <see cref="Construir"/>; mostrar/ocultar com fade vem pronto.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class Tela : MonoBehaviour
    {
        protected ControladorJogo controlador;
        protected RectTransform raiz;
        protected CanvasGroup grupo;

        public bool Visivel => gameObject.activeSelf;

        public void Inicializar(ControladorJogo dono)
        {
            controlador = dono;
            raiz = (RectTransform)transform;
            UIFabrica.Esticar(raiz);
            grupo = gameObject.AddComponent<CanvasGroup>();
            Construir();
            gameObject.SetActive(false);
        }

        /// <summary>Cria todos os elementos visuais da tela.</summary>
        protected abstract void Construir();

        public virtual void Mostrar()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StopAllCoroutines();
            StartCoroutine(Fade(0f, 1f, 0.25f));
        }

        public virtual void Ocultar()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        private IEnumerator Fade(float de, float para, float duracao)
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
