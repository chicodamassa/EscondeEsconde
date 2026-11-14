using System.Collections.Generic;
using EscondeEsconde.Modelo;
using UnityEngine;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>Tela inicial: nome do jogador, dificuldade, jogar, como jogar e sair.</summary>
    public class TelaMenu : Tela
    {
        private InputField campoNome;
        private readonly Dictionary<Dificuldade, Button> botoesDificuldade = new Dictionary<Dificuldade, Button>();
        private Dificuldade escolhida = Dificuldade.Normal;
        private Text textoInfoDificuldade;
        private Text textoPlacar;
        private RectTransform logo, retratoJogador, retratoProcurador;
        private GameObject painelComoJogar;

        protected override void Construir()
        {
            // ---- fundo: planta da casa escurecida ----
            UIFabrica.Esticar(UIFabrica.CriarImagem("FundoCor", raiz, null, UIFabrica.Noite).rectTransform);
            var planta = UIFabrica.CriarImagem("Planta", raiz, UIFabrica.Carregar("Arte/Planta/casatopdown"), new Color(1f, 1f, 1f, 0.45f));
            UIFabrica.Esticar(planta.rectTransform);
            var ajuste = planta.gameObject.AddComponent<AspectRatioFitter>();
            ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            ajuste.aspectRatio = MontadorCasa.LARGURA_PLANTA / MontadorCasa.ALTURA_PLANTA;
            UIFabrica.Esticar(UIFabrica.CriarImagem("Escurecer", raiz, null, UIFabrica.ComAlfa(UIFabrica.Noite, 0.35f)).rectTransform);
            UIFabrica.Esticar(UIFabrica.CriarImagem("Vinheta", raiz, UIFabrica.Vinheta, UIFabrica.ComAlfa(UIFabrica.Noite, 0.95f)).rectTransform);

            // ---- logo ----
            var imgLogo = UIFabrica.CriarImagem("Logo", raiz, UIFabrica.Carregar("Arte/Gerada/titulo"), Color.white);
            imgLogo.preserveAspect = true;
            logo = UIFabrica.Posicionar(imgLogo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 224f));
            var sub = UIFabrica.Sombra(UIFabrica.CriarTexto("Subtitulo", raiz,
                "Esconda-se antes que o tempo acabe... e torça para não ser encontrado!", 34, UIFabrica.Creme));
            UIFabrica.Posicionar(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -262f), new Vector2(1400f, 60f));

            // ---- personagens decorando os lados ----
            retratoJogador = CriarRetrato("RetratoJogador", ControladorJogo.ICONE_JOGADOR, 0.17f, "Você", UIFabrica.Azul);
            retratoProcurador = CriarRetrato("RetratoProcurador", ControladorJogo.ICONE_PROCURADOR, 0.83f, "O Procurador", UIFabrica.Vermelho);

            // ---- painel central ----
            var painel = UIFabrica.CriarPainel("Painel", raiz, UIFabrica.ComAlfa(UIFabrica.CremeClaro, 0.96f));
            UIFabrica.Sombra(painel, 8f, 0.45f);
            var p = UIFabrica.Posicionar(painel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(740f, 600f));
            var borda = UIFabrica.CriarImagem("Borda", p, UIFabrica.Moldura, UIFabrica.Marrom);
            borda.type = Image.Type.Sliced;
            UIFabrica.Esticar(borda.rectTransform);

            Rotulo(p, "Seu nome", -30f);
            campoNome = UIFabrica.CriarCampoTexto("CampoNome", p, "Jogador", "Digite seu nome", 34);
            UIFabrica.Posicionar((RectTransform)campoNome.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(620f, 72f));

            Rotulo(p, "Dificuldade", -172f);
            CriarBotaoDificuldade(p, Dificuldade.Facil, "Fácil", UIFabrica.Verde, -215f);
            CriarBotaoDificuldade(p, Dificuldade.Normal, "Normal", UIFabrica.Laranja, 0f);
            CriarBotaoDificuldade(p, Dificuldade.Dificil, "Difícil", UIFabrica.Vermelho, 215f);
            textoInfoDificuldade = UIFabrica.CriarTexto("InfoDificuldade", p, "", 24, UIFabrica.MarromClaro);
            UIFabrica.Posicionar(textoInfoDificuldade.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -305f), new Vector2(680f, 40f));

            var jogar = UIFabrica.CriarBotao("BotaoJogar", p, "JOGAR", UIFabrica.Laranja, AoClicarJogar, 52);
            UIFabrica.Posicionar((RectTransform)jogar.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -360f), new Vector2(620f, 106f));

            var comoJogar = UIFabrica.CriarBotao("BotaoComoJogar", p, "Como jogar", UIFabrica.Azul, () => painelComoJogar.SetActive(true), 30);
            UIFabrica.Posicionar((RectTransform)comoJogar.transform, new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -495f), new Vector2(300f, 72f));
            var sair = UIFabrica.CriarBotao("BotaoSair", p, "Sair", UIFabrica.MarromClaro, () => controlador.Sair(), 30);
            UIFabrica.Posicionar((RectTransform)sair.transform, new Vector2(0.5f, 1f), new Vector2(0f, 1f), new Vector2(10f, -495f), new Vector2(300f, 72f));

            // ---- rodapé ----
            textoPlacar = UIFabrica.Sombra(UIFabrica.CriarTexto("Placar", raiz, "", 28, UIFabrica.Amarelo));
            UIFabrica.Posicionar(textoPlacar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1200f, 40f));
            var rodape = UIFabrica.CriarTexto("Rodape", raiz, "Projeto Final • Programação Orientada a Objetos • 2026", 22, UIFabrica.ComAlfa(UIFabrica.Creme, 0.6f));
            UIFabrica.Posicionar(rodape.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(1200f, 34f));

            ConstruirComoJogar();
            SelecionarDificuldade(Dificuldade.Normal);
        }

        private RectTransform CriarRetrato(string nome, string sprite, float x, string legenda, Color cor)
        {
            var img = UIFabrica.CriarImagem(nome, raiz, UIFabrica.Carregar(sprite), Color.white);
            img.preserveAspect = true;
            var rt = UIFabrica.Posicionar(img.rectTransform, new Vector2(x, 0.42f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(440f, 440f));
            var fundo = UIFabrica.CriarPainel("Legenda", rt, cor);
            UIFabrica.Posicionar(fundo.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(300f, 64f));
            var txt = UIFabrica.Sombra(UIFabrica.CriarTexto("Texto", fundo.transform, legenda, 32, UIFabrica.CremeClaro, TextAnchor.MiddleCenter, true));
            UIFabrica.Esticar(txt.rectTransform);
            return rt;
        }

        private void Rotulo(Transform pai, string texto, float y)
        {
            var t = UIFabrica.CriarTexto("Rotulo_" + texto, pai, texto, 30, UIFabrica.Marrom, TextAnchor.MiddleCenter, true);
            UIFabrica.Posicionar(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(600f, 44f));
        }

        private void CriarBotaoDificuldade(Transform pai, Dificuldade dificuldade, string rotulo, Color cor, float x)
        {
            var b = UIFabrica.CriarBotao("Dificuldade_" + rotulo, pai, rotulo, cor, () => SelecionarDificuldade(dificuldade), 30);
            UIFabrica.Posicionar((RectTransform)b.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, -222f), new Vector2(196f, 72f));
            botoesDificuldade[dificuldade] = b;
        }

        private void SelecionarDificuldade(Dificuldade dificuldade)
        {
            escolhida = dificuldade;
            foreach (var par in botoesDificuldade)
            {
                bool ativo = par.Key == dificuldade;
                var img = par.Value.GetComponent<Image>();
                var c = img.color;
                c.a = ativo ? 1f : 0.35f;
                img.color = c;
                par.Value.transform.Find("Borda").GetComponent<Image>().color = ativo ? UIFabrica.Marrom : UIFabrica.ComAlfa(UIFabrica.Marrom, 0.25f);
            }

            var p = GerenciadorJogo.ObterParametros(dificuldade);
            string percepcao = p.BonusPercepcao > 0f ? "atento" : (p.BonusPercepcao < 0f ? "distraído" : "normal");
            textoInfoDificuldade.text = $"{p.TempoPreparacao:0} s para se esconder • procurador {percepcao}";
        }

        private void AoClicarJogar()
        {
            if (controlador.Transicao.EmAndamento) return;
            controlador.IniciarJogo(campoNome.text, escolhida);
        }

        private void ConstruirComoJogar()
        {
            var fundo = UIFabrica.CriarImagem("ComoJogar", raiz, null, UIFabrica.ComAlfa(UIFabrica.Noite, 0.85f), true);
            UIFabrica.Esticar(fundo.rectTransform);
            painelComoJogar = fundo.gameObject;

            var painel = UIFabrica.CriarPainel("Painel", fundo.transform, UIFabrica.CremeClaro);
            var p = UIFabrica.Posicionar(painel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 860f));
            var titulo = UIFabrica.CriarTexto("Titulo", p, "Como jogar", 56, UIFabrica.Laranja, TextAnchor.MiddleCenter, true);
            UIFabrica.Contorno(titulo, UIFabrica.Marrom, 2f);
            UIFabrica.Posicionar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(1000f, 80f));

            const string texto =
                "<b><color=#4F6C9E>1. FASE DE ESCONDER</color></b>  (visão lateral dos cômodos)\n" +
                "•  Você começa na <b>Entrada</b> e tem um tempo limite (relógio no topo da tela).\n" +
                "•  Use as <b>setas</b> (ou as teclas ← →) para andar pelos cômodos da casa.\n" +
                "•  Clique em um móvel para ver o esconderijo e confirme com <b>Esconder</b>.\n" +
                "•  Já escondido? Use <b>Acelerar o tempo</b> para chamar o procurador mais cedo.\n" +
                "•  Se o tempo acabar e você <b>não</b> estiver escondido, você perde!\n\n" +
                "<b><color=#C8553D>2. FASE DE BUSCA</color></b>  (planta da casa vista de cima)\n" +
                "•  O procurador começa pela Entrada e passa por <b>todos</b> os esconderijos.\n" +
                "•  Em cada um ele sorteia se vai revistar: quanto mais óbvio, maior a chance.\n" +
                "•  Se ele revistar o <b>seu</b> esconderijo, você perde. Se passar direto, você vence!\n" +
                "•  Cuidado: ele tem <b>memória</b> e lembra onde já te encontrou antes.";
            var corpo = UIFabrica.CriarTexto("Texto", p, texto, 31, UIFabrica.Marrom, TextAnchor.UpperLeft);
            corpo.lineSpacing = 1.15f;
            UIFabrica.Area(corpo.rectTransform, Vector2.zero, Vector2.one);
            corpo.rectTransform.offsetMin = new Vector2(70f, 130f);
            corpo.rectTransform.offsetMax = new Vector2(-60f, -125f);

            var ok = UIFabrica.CriarBotao("BotaoEntendi", p, "Entendi!", UIFabrica.Verde, () => painelComoJogar.SetActive(false), 36);
            UIFabrica.Posicionar((RectTransform)ok.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 35f), new Vector2(340f, 84f));
            painelComoJogar.SetActive(false);
        }

        public override void Mostrar()
        {
            base.Mostrar();
            painelComoJogar.SetActive(false);
            campoNome.text = controlador.NomeJogador;
            SelecionarDificuldade(controlador.DificuldadeEscolhida);
            var g = controlador.Gerenciador;
            textoPlacar.text = g.PartidasJogadas > 0
                ? $"Placar: {g.Vitorias} vitória(s) em {g.PartidasJogadas} partida(s)"
                : "";
        }

        private void Update()
        {
            float t = Time.unscaledTime;
            logo.localScale = Vector3.one * (1f + 0.025f * Mathf.Sin(t * 2.2f));
            retratoJogador.anchoredPosition = new Vector2(0f, 12f * Mathf.Sin(t * 1.7f));
            retratoProcurador.anchoredPosition = new Vector2(0f, 12f * Mathf.Sin(t * 1.7f + 1.5f));
            retratoProcurador.localEulerAngles = new Vector3(0f, 0f, 3f * Mathf.Sin(t * 1.1f));

            if (painelComoJogar.activeSelf && Input.GetKeyDown(KeyCode.Escape)) painelComoJogar.SetActive(false);
            else if (!painelComoJogar.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !campoNome.isFocused)
                AoClicarJogar();
        }
    }
}
