using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// Funções auxiliares para criar a interface por código (painéis, textos, botões...),
    /// além das cores, fontes e sprites gerados em tempo de execução.
    /// </summary>
    public static class UIFabrica
    {
        // ---------------- paleta (mesmos tons das artes) ----------------
        public static readonly Color Marrom = Hex("3B2A26");
        public static readonly Color MarromClaro = Hex("6B4A3A");
        public static readonly Color Creme = Hex("F4E4C6");
        public static readonly Color CremeClaro = Hex("FFF6E6");
        public static readonly Color Laranja = Hex("E8743B");
        public static readonly Color Vermelho = Hex("C8553D");
        public static readonly Color Verde = Hex("6E9F5A");
        public static readonly Color Azul = Hex("4F6C9E");
        public static readonly Color Amarelo = Hex("F2C14E");
        public static readonly Color Cinza = Hex("8A8F98");
        public static readonly Color Noite = Hex("1D1513");

        public static Font FonteTexto { get; private set; }
        public static Font FonteTitulo { get; private set; }

        private static Sprite arredondado, moldura, circulo, lanterna, vinheta;
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static void Inicializar()
        {
            Font padrao = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            FonteTexto = FonteDoSistema("Segoe UI Semibold", "Segoe UI", "Arial") ?? padrao;
            FonteTitulo = FonteDoSistema("Segoe UI Black", "Arial Black", "Segoe UI") ?? FonteTexto;
        }

        private static Font FonteDoSistema(params string[] nomes)
        {
            try
            {
                var instaladas = new HashSet<string>(Font.GetOSInstalledFontNames());
                foreach (string nome in nomes)
                    if (instaladas.Contains(nome)) return Font.CreateDynamicFontFromOSFont(nome, 32);
            }
            catch { /* plataforma sem acesso às fontes do sistema */ }
            return null;
        }

        /// <summary>Converte "RRGGBB" em cor (feito "na mão" para poder ser usado em campos estáticos).</summary>
        public static Color Hex(string hex, float alfa = 1f)
        {
            int v = System.Convert.ToInt32(hex, 16);
            return new Color(((v >> 16) & 0xFF) / 255f, ((v >> 8) & 0xFF) / 255f, (v & 0xFF) / 255f, alfa);
        }

        public static Color ComAlfa(Color c, float alfa)
        {
            c.a = alfa;
            return c;
        }

        // ---------------- sprites ----------------
        public static Sprite Carregar(string caminho)
        {
            if (cache.TryGetValue(caminho, out Sprite s)) return s;
            s = Resources.Load<Sprite>(caminho);
            if (s == null)
            {
                var tex = Resources.Load<Texture2D>(caminho);
                if (tex != null) s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                else Debug.LogWarning($"[UIFabrica] Sprite não encontrado em Resources/{caminho}");
            }
            cache[caminho] = s;
            return s;
        }

        public static Sprite Arredondado => arredondado != null ? arredondado : (arredondado = GerarRetangulo(96, 32, 0));
        public static Sprite Moldura => moldura != null ? moldura : (moldura = GerarRetangulo(96, 32, 7));
        public static Sprite Circulo => circulo != null ? circulo : (circulo = GerarCirculo(128));
        public static Sprite Lanterna => lanterna != null ? lanterna : (lanterna = GerarRadial(256, 0.07f, 0.16f, 1f));
        public static Sprite Vinheta => vinheta != null ? vinheta : (vinheta = GerarRadial(256, 0.25f, 0.75f, 1f));

        /// <summary>Retângulo de cantos arredondados (9-slice). espessura > 0 gera só a borda.</summary>
        private static Sprite GerarRetangulo(int tamanho, int raio, int espessura)
        {
            var tex = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[tamanho * tamanho];
            for (int y = 0; y < tamanho; y++)
            {
                for (int x = 0; x < tamanho; x++)
                {
                    float d = DistanciaBorda(x + 0.5f, y + 0.5f, tamanho, raio); // >0 dentro
                    float a = Mathf.Clamp01(d + 0.5f);
                    if (espessura > 0) a *= 1f - Mathf.Clamp01(d - espessura + 0.5f);
                    px[y * tamanho + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(raio + 2, raio + 2, raio + 2, raio + 2));
        }

        private static float DistanciaBorda(float x, float y, int tamanho, int raio)
        {
            float cx = Mathf.Clamp(x, raio, tamanho - raio);
            float cy = Mathf.Clamp(y, raio, tamanho - raio);
            float dc = raio - Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            float dl = Mathf.Min(Mathf.Min(x, tamanho - x), Mathf.Min(y, tamanho - y));
            return (x < raio || x > tamanho - raio) && (y < raio || y > tamanho - raio) ? dc : dl;
        }

        private static Sprite GerarCirculo(int tamanho)
        {
            var tex = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[tamanho * tamanho];
            float r = tamanho / 2f;
            for (int y = 0; y < tamanho; y++)
                for (int x = 0; x < tamanho; x++)
                {
                    float d = r - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    px[y * tamanho + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(d) * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>Gradiente radial: transparente no centro e opaco nas bordas.</summary>
        private static Sprite GerarRadial(int tamanho, float inicio, float fim, float alfaMax)
        {
            var tex = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[tamanho * tamanho];
            float r = tamanho / 2f;
            for (int y = 0; y < tamanho; y++)
                for (int x = 0; x < tamanho; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / tamanho;
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(inicio, fim, d)) * alfaMax;
                    px[y * tamanho + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100f);
        }

        // ---------------- layout ----------------
        public static RectTransform CriarRect(string nome, Transform pai)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            go.layer = 5; // UI
            go.transform.SetParent(pai, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Esticar(RectTransform rt, float margem = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margem, margem);
            rt.offsetMax = new Vector2(-margem, -margem);
            return rt;
        }

        /// <summary>Posiciona por âncora (0..1 no pai), pivô, deslocamento e tamanho.</summary>
        public static RectTransform Posicionar(RectTransform rt, Vector2 ancora, Vector2 pivo, Vector2 deslocamento, Vector2 tamanho)
        {
            rt.anchorMin = ancora;
            rt.anchorMax = ancora;
            rt.pivot = pivo;
            rt.anchoredPosition = deslocamento;
            rt.sizeDelta = tamanho;
            return rt;
        }

        public static RectTransform Area(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        // ---------------- elementos ----------------
        public static Image CriarImagem(string nome, Transform pai, Sprite sprite, Color cor, bool clicavel = false)
        {
            var img = CriarRect(nome, pai).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = cor;
            img.raycastTarget = clicavel;
            return img;
        }

        public static Image CriarPainel(string nome, Transform pai, Color cor, float arredondamento = 1f)
        {
            var img = CriarImagem(nome, pai, Arredondado, cor);
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = arredondamento;
            return img;
        }

        public static Text CriarTexto(string nome, Transform pai, string conteudo, int tamanho, Color cor,
            TextAnchor alinhamento = TextAnchor.MiddleCenter, bool titulo = false)
        {
            var txt = CriarRect(nome, pai).gameObject.AddComponent<Text>();
            txt.font = titulo ? FonteTitulo : FonteTexto;
            if (titulo) txt.fontStyle = FontStyle.Bold;
            txt.text = conteudo;
            txt.fontSize = tamanho;
            txt.color = cor;
            txt.alignment = alinhamento;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            return txt;
        }

        public static T Sombra<T>(T grafico, float distancia = 3f, float alfa = 0.5f) where T : Graphic
        {
            var s = grafico.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0f, 0f, 0f, alfa);
            s.effectDistance = new Vector2(distancia, -distancia);
            return grafico;
        }

        public static T Contorno<T>(T grafico, Color cor, float distancia = 2f) where T : Graphic
        {
            var o = grafico.gameObject.AddComponent<Outline>();
            o.effectColor = cor;
            o.effectDistance = new Vector2(distancia, -distancia);
            return grafico;
        }

        public static Button CriarBotao(string nome, Transform pai, string rotulo, Color corFundo, UnityAction aoClicar,
            int tamanhoFonte = 34, Sprite icone = null, Color? corTexto = null)
        {
            var fundo = CriarPainel(nome, pai, corFundo);
            fundo.raycastTarget = true;
            Sombra(fundo, 5f, 0.35f);

            var botao = fundo.gameObject.AddComponent<Button>();
            var cores = botao.colors;
            cores.normalColor = Color.white;
            cores.highlightedColor = new Color(1.08f, 1.08f, 1.08f);
            cores.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            cores.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            cores.colorMultiplier = 1f;
            botao.colors = cores;
            botao.targetGraphic = fundo;
            if (aoClicar != null) botao.onClick.AddListener(aoClicar);
            fundo.gameObject.AddComponent<EfeitoBotao>();

            // borda escura (estilo das artes)
            var borda = CriarImagem("Borda", fundo.transform, Moldura, ComAlfa(Marrom, 0.9f));
            borda.type = Image.Type.Sliced;
            Esticar(borda.rectTransform);

            var txt = CriarTexto("Rotulo", fundo.transform, rotulo, tamanhoFonte, corTexto ?? CremeClaro, TextAnchor.MiddleCenter, true);
            Sombra(txt, 2f, 0.45f);
            Esticar(txt.rectTransform);

            if (icone != null)
            {
                var img = CriarImagem("Icone", fundo.transform, icone, Color.white);
                img.preserveAspect = true;
                Posicionar(img.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(tamanhoFonte * 1.5f, tamanhoFonte * 1.5f));
                txt.rectTransform.offsetMin = new Vector2(tamanhoFonte * 1.5f + 18f, 0f);
            }
            return botao;
        }

        public static void DefinirTextoBotao(Button botao, string texto)
        {
            var txt = botao.GetComponentInChildren<Text>();
            if (txt != null) txt.text = texto;
        }

        public static InputField CriarCampoTexto(string nome, Transform pai, string valorInicial, string placeholder, int tamanhoFonte)
        {
            var fundo = CriarPainel(nome, pai, CremeClaro);
            fundo.raycastTarget = true;
            var borda = CriarImagem("Borda", fundo.transform, Moldura, Marrom);
            borda.type = Image.Type.Sliced;
            Esticar(borda.rectTransform);

            var dica = CriarTexto("Placeholder", fundo.transform, placeholder, tamanhoFonte, ComAlfa(MarromClaro, 0.5f), TextAnchor.MiddleCenter);
            dica.fontStyle = FontStyle.Italic;
            Esticar(dica.rectTransform, 10f);
            var txt = CriarTexto("Texto", fundo.transform, valorInicial, tamanhoFonte, Marrom, TextAnchor.MiddleCenter);
            txt.supportRichText = false;
            Esticar(txt.rectTransform, 10f);

            var campo = fundo.gameObject.AddComponent<InputField>();
            campo.textComponent = txt;
            campo.placeholder = dica;
            campo.targetGraphic = fundo;
            campo.characterLimit = 16;
            campo.text = valorInicial;
            return campo;
        }
    }
}
