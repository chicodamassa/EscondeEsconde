using System;
using EscondeEsconde.Modelo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// Região clicável sobre um móvel na visão lateral. Mostra um contorno e o nome
    /// do esconderijo quando o mouse passa por cima.
    /// </summary>
    public class AreaEsconderijo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private Esconderijo esconderijo;
        private Action<Esconderijo> aoClicar;
        private Image moldura;
        private RectTransform indicador;
        private Image indicadorFundo;
        private GameObject rotulo;
        private bool mouseEmCima;
        private bool selecionado;
        private bool depuracao;
        private float fase;

        public Esconderijo Esconderijo => esconderijo;

        public void Configurar(Esconderijo alvo, Action<Esconderijo> callback)
        {
            esconderijo = alvo;
            aoClicar = callback;
            fase = UnityEngine.Random.value * 6f;

            var rt = (RectTransform)transform;
            Rect a = alvo.AreaLateral; // 0..1 com origem no canto SUPERIOR esquerdo da imagem
            rt.anchorMin = new Vector2(a.xMin, 1f - a.yMax);
            rt.anchorMax = new Vector2(a.xMax, 1f - a.yMin);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var alvoClique = gameObject.AddComponent<Image>();
            alvoClique.color = new Color(1f, 1f, 1f, 0f); // invisível, mas recebe o clique
            alvoClique.raycastTarget = true;

            moldura = UIFabrica.CriarImagem("Moldura", transform, UIFabrica.Moldura, UIFabrica.ComAlfa(UIFabrica.Amarelo, 0f));
            moldura.type = Image.Type.Sliced;
            UIFabrica.Esticar(moldura.rectTransform, -6f);

            indicadorFundo = UIFabrica.CriarImagem("Indicador", transform, UIFabrica.Circulo, UIFabrica.ComAlfa(UIFabrica.CremeClaro, 0.85f));
            indicador = UIFabrica.Posicionar(indicadorFundo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(62f, 62f));
            UIFabrica.Contorno(indicadorFundo, UIFabrica.ComAlfa(UIFabrica.Marrom, 0.8f), 2f);
            var olho = UIFabrica.CriarImagem("Olho", indicador, UIFabrica.Carregar("Arte/Gerada/icone_olho"), Color.white);
            olho.preserveAspect = true;
            UIFabrica.Esticar(olho.rectTransform, 9f);

            var fundoRotulo = UIFabrica.CriarPainel("Rotulo", transform, UIFabrica.ComAlfa(UIFabrica.Marrom, 0.92f));
            UIFabrica.Posicionar(fundoRotulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(Mathf.Max(260f, alvo.Nome.Length * 15f + 40f), 52f));
            var txt = UIFabrica.CriarTexto("Texto", fundoRotulo.transform, alvo.Nome, 26, UIFabrica.CremeClaro, TextAnchor.MiddleCenter, true);
            UIFabrica.Esticar(txt.rectTransform);
            rotulo = fundoRotulo.gameObject;
            rotulo.SetActive(false);
        }

        public void DefinirSelecionado(bool valor)
        {
            selecionado = valor;
        }

        /// <summary>Mostra todos os contornos (usado para conferir as áreas na apresentação/testes).</summary>
        public void DefinirDepuracao(bool valor)
        {
            depuracao = valor;
        }

        private void Update()
        {
            bool destaque = mouseEmCima || selecionado || depuracao;
            float alvo = destaque ? 1f : 0f;
            var c = moldura.color;
            c.a = Mathf.MoveTowards(c.a, alvo, Time.unscaledDeltaTime * 6f);
            moldura.color = c;
            rotulo.SetActive(destaque);

            float pulso = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 3.2f + fase);
            indicador.localScale = Vector3.one * (destaque ? 1.2f : pulso);
            indicadorFundo.color = UIFabrica.ComAlfa(destaque ? UIFabrica.Amarelo : UIFabrica.CremeClaro, destaque ? 1f : 0.75f);
        }

        public void OnPointerEnter(PointerEventData dados)
        {
            mouseEmCima = true;
        }

        public void OnPointerExit(PointerEventData dados)
        {
            mouseEmCima = false;
        }

        public void OnPointerClick(PointerEventData dados)
        {
            SomJogo.Instancia?.Tocar(SomJogo.Efeito.Clique);
            aoClicar?.Invoke(esconderijo);
        }
    }
}
