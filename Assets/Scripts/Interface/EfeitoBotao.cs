using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>Pequena animação de "crescer" ao passar o mouse e som de clique.</summary>
    public class EfeitoBotao : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Vector3 escalaAlvo = Vector3.one;
        private Selectable selecionavel;

        private void Awake()
        {
            selecionavel = GetComponent<Selectable>();
        }

        private void OnDisable()
        {
            escalaAlvo = Vector3.one;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, escalaAlvo, Time.unscaledDeltaTime * 14f);
        }

        public void OnPointerEnter(PointerEventData dados)
        {
            if (selecionavel == null || selecionavel.IsInteractable()) escalaAlvo = Vector3.one * 1.06f;
        }

        public void OnPointerExit(PointerEventData dados)
        {
            escalaAlvo = Vector3.one;
        }

        public void OnPointerClick(PointerEventData dados)
        {
            if (selecionavel != null && !selecionavel.IsInteractable()) return;
            transform.localScale = Vector3.one * 0.94f;
            SomJogo.Instancia?.Tocar(SomJogo.Efeito.Clique);
        }
    }
}
