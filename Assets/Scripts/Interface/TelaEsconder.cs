using System.Collections;
using System.Collections.Generic;
using EscondeEsconde.Modelo;
using UnityEngine;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// FASE DE ESCONDER (visão 2D lateral): mostra o cômodo atual, os esconderijos
    /// clicáveis, o relógio, as setas para trocar de cômodo e o mini-mapa.
    /// </summary>
    public class TelaEsconder : Tela
    {
        // palco (imagem do cômodo + áreas clicáveis)
        private RectTransform palco;
        private CanvasGroup grupoPalco;
        private Image imagemComodo;
        private RectTransform camadaAreas;
        private readonly List<AreaEsconderijo> areas = new List<AreaEsconderijo>();

        // HUD
        private Text textoNomeComodo, textoDescricaoComodo;
        private RectTransform caixaTempo;
        private Image fundoTempo;
        private Text textoTempo;
        private Button setaEsquerda, setaDireita;
        private Text rotuloEsquerda, rotuloDireita;
        private RectTransform destaqueMini;
        private GameObject dica;

        // diálogo de confirmação
        private GameObject dialogo;
        private Text dlgNome, dlgRisco, dlgDescricao, dlgAviso;
        private Image dlgFundoRisco;
        private Esconderijo emConfirmacao;

        // estado "escondido"
        private GameObject camadaEscondido;
        private Text textoEscondido, falaEscondido;

        private bool trocandoComodo;
        private int ultimoSegundo = -1;
        private bool depuracaoAreas;

        private GerenciadorJogo G => controlador.Gerenciador;

        // ================================================================== construção
        protected override void Construir()
        {
            UIFabrica.Esticar(UIFabrica.CriarImagem("Fundo", raiz, null, UIFabrica.Noite).rectTransform);

            // palco com a mesma proporção das artes (16:9) -> as áreas clicáveis sempre batem com o desenho
            palco = UIFabrica.Esticar(UIFabrica.CriarRect("Palco", raiz));
            var ajuste = palco.gameObject.AddComponent<AspectRatioFitter>();
            ajuste.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            ajuste.aspectRatio = 1671f / 941f;
            grupoPalco = palco.gameObject.AddComponent<CanvasGroup>();
            imagemComodo = UIFabrica.CriarImagem("ImagemComodo", palco, null, Color.white);
            UIFabrica.Esticar(imagemComodo.rectTransform);
            camadaAreas = UIFabrica.Esticar(UIFabrica.CriarRect("Esconderijos", palco));

            ConstruirCamadaEscondido();
            ConstruirHud();
            ConstruirDialogo();
        }

        private void ConstruirHud()
        {
            // ---- nome do cômodo (canto superior esquerdo) ----
            var painelComodo = UIFabrica.CriarPainel("PainelComodo", raiz, UIFabrica.ComAlfa(UIFabrica.CremeClaro, 0.93f));
            UIFabrica.Sombra(painelComodo, 5f, 0.4f);
            var pc = UIFabrica.Posicionar(painelComodo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -26f), new Vector2(620f, 150f));
            var casa = UIFabrica.CriarImagem("Icone", pc, UIFabrica.Carregar("Arte/Gerada/icone_casa"), Color.white);
            casa.preserveAspect = true;
            UIFabrica.Posicionar(casa.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(92f, 92f));
            textoNomeComodo = UIFabrica.CriarTexto("Nome", pc, "", 50, UIFabrica.Marrom, TextAnchor.LowerLeft, true);
            UIFabrica.Area(textoNomeComodo.rectTransform, new Vector2(0f, 0.48f), new Vector2(1f, 1f)).offsetMin = new Vector2(126f, 0f);
            textoDescricaoComodo = UIFabrica.CriarTexto("Descricao", pc, "", 23, UIFabrica.MarromClaro, TextAnchor.UpperLeft);
            var rd = UIFabrica.Area(textoDescricaoComodo.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.48f));
            rd.offsetMin = new Vector2(126f, 8f);
            rd.offsetMax = new Vector2(-16f, 0f);

            // ---- relógio (centro superior) ----
            fundoTempo = UIFabrica.CriarPainel("Relogio", raiz, UIFabrica.CremeClaro);
            UIFabrica.Sombra(fundoTempo, 5f, 0.4f);
            caixaTempo = UIFabrica.Posicionar(fundoTempo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(290f, 112f));
            var bordaTempo = UIFabrica.CriarImagem("Borda", caixaTempo, UIFabrica.Moldura, UIFabrica.Marrom);
            bordaTempo.type = Image.Type.Sliced;
            UIFabrica.Esticar(bordaTempo.rectTransform);
            var relogio = UIFabrica.CriarImagem("Icone", caixaTempo, UIFabrica.Carregar("Arte/Gerada/icone_relogio"), Color.white);
            relogio.preserveAspect = true;
            UIFabrica.Posicionar(relogio.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(80f, 80f));
            textoTempo = UIFabrica.CriarTexto("Tempo", caixaTempo, "0:30", 66, UIFabrica.Marrom, TextAnchor.MiddleCenter, true);
            UIFabrica.Esticar(textoTempo.rectTransform).offsetMin = new Vector2(96f, 0f);

            // ---- mini-mapa (canto superior direito) ----
            var painelMini = UIFabrica.CriarPainel("MiniMapa", raiz, UIFabrica.ComAlfa(UIFabrica.CremeClaro, 0.93f));
            UIFabrica.Sombra(painelMini, 5f, 0.4f);
            var pm = UIFabrica.Posicionar(painelMini.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -26f), new Vector2(360f, 268f));
            var tituloMini = UIFabrica.CriarTexto("Titulo", pm, "Você está aqui", 24, UIFabrica.Marrom, TextAnchor.MiddleCenter, true);
            UIFabrica.Posicionar(tituloMini.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(340f, 36f));
            var areaMini = UIFabrica.Area(UIFabrica.CriarRect("AreaPlanta", pm), Vector2.zero, Vector2.one);
            areaMini.offsetMin = new Vector2(12f, 12f);
            areaMini.offsetMax = new Vector2(-12f, -46f);
            var plantaMini = UIFabrica.CriarImagem("Planta", areaMini, UIFabrica.Carregar("Arte/Planta/casatopdown"), Color.white);
            UIFabrica.Esticar(plantaMini.rectTransform);
            var ajusteMini = plantaMini.gameObject.AddComponent<AspectRatioFitter>();
            ajusteMini.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            ajusteMini.aspectRatio = MontadorCasa.LARGURA_PLANTA / MontadorCasa.ALTURA_PLANTA;
            var destaque = UIFabrica.CriarImagem("Destaque", plantaMini.transform, null, UIFabrica.ComAlfa(UIFabrica.Amarelo, 0.4f));
            destaqueMini = destaque.rectTransform;
            var contornoDestaque = UIFabrica.CriarImagem("Contorno", destaqueMini, UIFabrica.Moldura, UIFabrica.Laranja);
            contornoDestaque.type = Image.Type.Sliced;
            contornoDestaque.pixelsPerUnitMultiplier = 2f;
            UIFabrica.Esticar(contornoDestaque.rectTransform);

            // ---- setas ----
            setaEsquerda = CriarSeta(-1, out rotuloEsquerda);
            setaDireita = CriarSeta(1, out rotuloDireita);

            // ---- dica ----
            var fundoDica = UIFabrica.CriarPainel("Dica", raiz, UIFabrica.ComAlfa(UIFabrica.Marrom, 0.82f));
            UIFabrica.Posicionar(fundoDica.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1000f, 62f));
            var textoDica = UIFabrica.CriarTexto("Texto", fundoDica.transform,
                "Clique em um móvel para se esconder   •   ← → trocam de cômodo", 27, UIFabrica.CremeClaro);
            UIFabrica.Esticar(textoDica.rectTransform);
            dica = fundoDica.gameObject;
        }

        private Button CriarSeta(int direcao, out Text rotulo)
        {
            float lado = direcao < 0 ? 0f : 1f;
            var fundo = UIFabrica.CriarImagem(direcao < 0 ? "SetaEsquerda" : "SetaDireita", raiz, UIFabrica.Circulo, UIFabrica.ComAlfa(UIFabrica.Marrom, 0.8f), true);
            var rt = UIFabrica.Posicionar(fundo.rectTransform, new Vector2(lado, 0.5f), new Vector2(lado, 0.5f), new Vector2(direcao < 0 ? 30f : -30f, 0f), new Vector2(132f, 132f));
            UIFabrica.Sombra(fundo, 4f, 0.4f);
            var icone = UIFabrica.CriarImagem("Icone", rt, UIFabrica.Carregar("Arte/Gerada/icone_seta"), Color.white);
            icone.preserveAspect = true;
            UIFabrica.Esticar(icone.rectTransform, 22f);
            if (direcao < 0) icone.rectTransform.localScale = new Vector3(-1f, 1f, 1f);

            var botao = fundo.gameObject.AddComponent<Button>();
            botao.targetGraphic = fundo;
            botao.onClick.AddListener(() => Mover(direcao));
            fundo.gameObject.AddComponent<EfeitoBotao>();

            // o rótulo fica alinhado à borda da tela para não ser cortado
            var fundoRotulo = UIFabrica.CriarPainel("Rotulo", rt, UIFabrica.ComAlfa(UIFabrica.CremeClaro, 0.93f));
            UIFabrica.Posicionar(fundoRotulo.rectTransform, new Vector2(lado, 0f), new Vector2(lado, 1f), new Vector2(0f, -8f), new Vector2(190f, 50f));
            rotulo = UIFabrica.CriarTexto("Texto", fundoRotulo.transform, "", 27, UIFabrica.Marrom, TextAnchor.MiddleCenter, true);
            UIFabrica.Esticar(rotulo.rectTransform);
            return botao;
        }

        private void ConstruirDialogo()
        {
            var bloqueio = UIFabrica.CriarImagem("Dialogo", raiz, null, new Color(0f, 0f, 0f, 0.35f), true);
            UIFabrica.Esticar(bloqueio.rectTransform);
            var fechar = bloqueio.gameObject.AddComponent<Button>();
            fechar.transition = Selectable.Transition.None;
            fechar.onClick.AddListener(Cancelar);
            dialogo = bloqueio.gameObject;

            var painel = UIFabrica.CriarPainel("Painel", bloqueio.transform, UIFabrica.CremeClaro);
            painel.raycastTarget = true; // clicar no painel não fecha
            UIFabrica.Sombra(painel, 8f, 0.5f);
            var p = UIFabrica.Posicionar(painel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1060f, 360f));
            var borda = UIFabrica.CriarImagem("Borda", p, UIFabrica.Moldura, UIFabrica.Marrom);
            borda.type = Image.Type.Sliced;
            UIFabrica.Esticar(borda.rectTransform);

            var pergunta = UIFabrica.CriarTexto("Pergunta", p, "Esconder-se aqui?", 28, UIFabrica.MarromClaro, TextAnchor.MiddleLeft);
            UIFabrica.Posicionar(pergunta.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -18f), new Vector2(600f, 40f));
            dlgNome = UIFabrica.CriarTexto("Nome", p, "", 48, UIFabrica.Marrom, TextAnchor.MiddleLeft, true);
            UIFabrica.Posicionar(dlgNome.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -58f), new Vector2(720f, 64f));

            dlgFundoRisco = UIFabrica.CriarPainel("Risco", p, UIFabrica.Verde);
            UIFabrica.Posicionar(dlgFundoRisco.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(250f, 64f));
            dlgRisco = UIFabrica.CriarTexto("Texto", dlgFundoRisco.transform, "", 28, UIFabrica.CremeClaro, TextAnchor.MiddleCenter, true);
            UIFabrica.Esticar(dlgRisco.rectTransform);

            dlgDescricao = UIFabrica.CriarTexto("Descricao", p, "", 30, UIFabrica.MarromClaro, TextAnchor.UpperLeft);
            dlgDescricao.fontStyle = FontStyle.Italic;
            UIFabrica.Posicionar(dlgDescricao.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -132f), new Vector2(970f, 46f));
            dlgAviso = UIFabrica.CriarTexto("Aviso", p, "", 26, UIFabrica.Vermelho, TextAnchor.UpperLeft, true);
            UIFabrica.Posicionar(dlgAviso.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -184f), new Vector2(970f, 40f));

            var sim = UIFabrica.CriarBotao("BotaoEsconder", p, "Esconder", UIFabrica.Verde, Confirmar, 38, UIFabrica.Carregar("Arte/Gerada/icone_check"));
            UIFabrica.Posicionar((RectTransform)sim.transform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-14f, 30f), new Vector2(380f, 92f));
            var nao = UIFabrica.CriarBotao("BotaoCancelar", p, "Cancelar", UIFabrica.Vermelho, Cancelar, 38, UIFabrica.Carregar("Arte/Gerada/icone_xis"));
            UIFabrica.Posicionar((RectTransform)nao.transform, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(14f, 30f), new Vector2(380f, 92f));
            dialogo.SetActive(false);
        }

        private void ConstruirCamadaEscondido()
        {
            var camada = UIFabrica.CriarImagem("Escondido", raiz, null, new Color(0.05f, 0.03f, 0.03f, 0.45f), true);
            UIFabrica.Esticar(camada.rectTransform);
            camadaEscondido = camada.gameObject;
            UIFabrica.Esticar(UIFabrica.CriarImagem("Vinheta", camada.transform, UIFabrica.Vinheta, new Color(0f, 0f, 0f, 0.95f)).rectTransform);

            var painel = UIFabrica.CriarPainel("Painel", camada.transform, UIFabrica.ComAlfa(UIFabrica.CremeClaro, 0.97f));
            UIFabrica.Sombra(painel, 8f, 0.5f);
            var p = UIFabrica.Posicionar(painel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1240f, 250f));
            var borda = UIFabrica.CriarImagem("Borda", p, UIFabrica.Moldura, UIFabrica.Marrom);
            borda.type = Image.Type.Sliced;
            UIFabrica.Esticar(borda.rectTransform);

            var retrato = UIFabrica.CriarImagem("Retrato", p, UIFabrica.Carregar(ControladorJogo.ICONE_JOGADOR), Color.white);
            retrato.preserveAspect = true;
            UIFabrica.Posicionar(retrato.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 8f), new Vector2(230f, 230f));

            textoEscondido = UIFabrica.CriarTexto("Texto", p, "", 40, UIFabrica.Marrom, TextAnchor.MiddleLeft, true);
            UIFabrica.Posicionar(textoEscondido.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -20f), new Vector2(940f, 60f));
            falaEscondido = UIFabrica.CriarTexto("Fala", p, "", 28, UIFabrica.MarromClaro, TextAnchor.MiddleLeft);
            falaEscondido.fontStyle = FontStyle.Italic;
            UIFabrica.Posicionar(falaEscondido.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -82f), new Vector2(940f, 44f));

            var acelerar = UIFabrica.CriarBotao("BotaoAcelerar", p, "Acelerar o tempo", UIFabrica.Laranja, Acelerar, 34, UIFabrica.Carregar("Arte/Gerada/icone_acelerar"));
            UIFabrica.Posicionar((RectTransform)acelerar.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(270f, 26f), new Vector2(470f, 88f));
            var sair = UIFabrica.CriarBotao("BotaoSair", p, "Sair do esconderijo", UIFabrica.MarromClaro, SairDoEsconderijo, 30);
            UIFabrica.Posicionar((RectTransform)sair.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(760f, 26f), new Vector2(420f, 88f));
            camadaEscondido.SetActive(false);
        }

        // ================================================================== ciclo
        public override void Mostrar()
        {
            base.Mostrar();
            trocandoComodo = false;
            grupoPalco.alpha = 1f;
            palco.anchoredPosition = Vector2.zero;
            ultimoSegundo = -1;
            dialogo.SetActive(false);
            ExibirComodo(controlador.Jogador.ComodoAtual);
            AtualizarEstadoEscondido();
        }

        private void Update()
        {
            if (G.FaseAtual != FaseJogo.Preparacao) return;

            // relógio
            float t = G.TempoRestante;
            int segundos = Mathf.CeilToInt(t);
            textoTempo.text = $"{segundos / 60}:{segundos % 60:00}";
            bool urgente = t <= 10f;
            fundoTempo.color = urgente ? UIFabrica.Vermelho : UIFabrica.CremeClaro;
            textoTempo.color = urgente ? UIFabrica.CremeClaro : UIFabrica.Marrom;
            caixaTempo.localScale = Vector3.one * (urgente ? 1f + 0.06f * Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI)) : 1f);
            if (urgente && segundos != ultimoSegundo && segundos > 0 && !controlador.Transicao.EmAndamento)
                controlador.Som.Tocar(SomJogo.Efeito.Tique);
            ultimoSegundo = segundos;

            if (controlador.Jogador.Escondido && urgente)
                falaEscondido.text = "\"" + controlador.Jogador.ObterFala("escondido") + "\"";

            // teclado
            if (controlador.Transicao.EmAndamento) return;
            if (Input.GetKeyDown(KeyCode.F1)) DefinirDepuracaoAreas(!depuracaoAreas); // mostra todas as áreas clicáveis
            if (dialogo.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) Cancelar();
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Confirmar();
            }
            else if (!controlador.Jogador.Escondido)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Mover(-1);
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Mover(1);
            }
        }

        // ================================================================== ações
        public void Mover(int direcao)
        {
            if (trocandoComodo || controlador.Jogador.Escondido || G.FaseAtual != FaseJogo.Preparacao) return;
            Comodo destino = G.ObterComodoVizinho(controlador.Jogador.ComodoAtual, direcao);
            if (destino == null) return;
            StartCoroutine(TrocarComodo(destino, direcao));
        }

        private IEnumerator TrocarComodo(Comodo destino, int direcao)
        {
            trocandoComodo = true;
            dialogo.SetActive(false);
            controlador.Som.Tocar(SomJogo.Efeito.Porta);
            float meio = 0.5f / controlador.Jogador.Velocidade; // velocidade do jogador define a rapidez da troca

            for (float t = 0f; t < meio; t += Time.deltaTime)
            {
                float k = t / meio;
                grupoPalco.alpha = 1f - k;
                palco.anchoredPosition = new Vector2(-direcao * 120f * k, 0f);
                yield return null;
            }

            if (G.MoverJogadorPara(destino.Id)) ExibirComodo(destino);

            for (float t = 0f; t < meio; t += Time.deltaTime)
            {
                float k = t / meio;
                grupoPalco.alpha = k;
                palco.anchoredPosition = new Vector2(direcao * 120f * (1f - k), 0f);
                yield return null;
            }
            grupoPalco.alpha = 1f;
            palco.anchoredPosition = Vector2.zero;
            trocandoComodo = false;
        }

        private void ExibirComodo(Comodo comodo)
        {
            imagemComodo.sprite = UIFabrica.Carregar(comodo.ImagemLateral);
            textoNomeComodo.text = comodo.Nome;
            textoDescricaoComodo.text = comodo.Descricao;

            // recria as áreas clicáveis (as maiores ficam atrás das menores)
            foreach (var a in areas) Destroy(a.gameObject);
            areas.Clear();
            var lista = comodo.ObterEsconderijos();
            lista.Sort((x, y) => (y.AreaLateral.width * y.AreaLateral.height).CompareTo(x.AreaLateral.width * x.AreaLateral.height));
            foreach (var e in lista)
            {
                var area = UIFabrica.CriarRect("Area_" + e.Id, camadaAreas).gameObject.AddComponent<AreaEsconderijo>();
                area.Configurar(e, AbrirDialogo);
                area.DefinirDepuracao(depuracaoAreas);
                area.DefinirSelecionado(e == controlador.Jogador.ObterEsconderijoEscolhido());
                areas.Add(area);
            }

            rotuloEsquerda.text = G.ObterComodoVizinho(comodo, -1).Nome;
            rotuloDireita.text = G.ObterComodoVizinho(comodo, 1).Nome;

            Rect r = comodo.AreaPlanta;
            destaqueMini.anchorMin = new Vector2(r.xMin / MontadorCasa.LARGURA_PLANTA, 1f - r.yMax / MontadorCasa.ALTURA_PLANTA);
            destaqueMini.anchorMax = new Vector2(r.xMax / MontadorCasa.LARGURA_PLANTA, 1f - r.yMin / MontadorCasa.ALTURA_PLANTA);
            destaqueMini.offsetMin = new Vector2(-3f, -3f);
            destaqueMini.offsetMax = new Vector2(3f, 3f);
        }

        public void AbrirDialogo(Esconderijo esconderijo)
        {
            if (controlador.Jogador.Escondido || trocandoComodo || G.FaseAtual != FaseJogo.Preparacao) return;
            if (!controlador.Jogador.SelecionarEsconderijo(esconderijo)) return;

            emConfirmacao = esconderijo;
            foreach (var a in areas) a.DefinirSelecionado(a.Esconderijo == esconderijo);

            var procurador = controlador.Procurador;
            float chanceFinal = procurador.CalcularChance(esconderijo);
            NivelRisco risco = esconderijo.ObterNivelRisco(chanceFinal - esconderijo.ChanceInvestigacao);

            dlgNome.text = esconderijo.Nome;
            dlgDescricao.text = esconderijo.Descricao;
            switch (risco)
            {
                case NivelRisco.Baixo: dlgRisco.text = "RISCO BAIXO"; dlgFundoRisco.color = UIFabrica.Verde; break;
                case NivelRisco.Medio: dlgRisco.text = "RISCO MÉDIO"; dlgFundoRisco.color = UIFabrica.Laranja; break;
                default: dlgRisco.text = "RISCO ALTO"; dlgFundoRisco.color = UIFabrica.Vermelho; break;
            }

            int achou = procurador.VezesQueEncontrouEm(esconderijo.Id);
            int usou = controlador.Jogador.VezesQueUsou(esconderijo.Id);
            if (achou > 0) dlgAviso.text = $"Cuidado! {procurador.Nome} já te encontrou aqui {achou}x e lembra disso.";
            else if (usou > 0) dlgAviso.text = $"Você já se escondeu aqui {usou}x.";
            else dlgAviso.text = "";

            dialogo.SetActive(true);
        }

        public void Confirmar()
        {
            if (emConfirmacao == null) return;
            if (G.EsconderJogador(emConfirmacao))
            {
                controlador.Som.Tocar(SomJogo.Efeito.Esconder);
                falaEscondido.text = "\"" + controlador.Jogador.ObterFala("escondido") + "\"";
            }
            emConfirmacao = null;
            dialogo.SetActive(false);
            AtualizarEstadoEscondido();
        }

        public void Cancelar()
        {
            controlador.Jogador.CancelarSelecaoEsconderijo();
            emConfirmacao = null;
            dialogo.SetActive(false);
            AtualizarEstadoEscondido();
        }

        private void SairDoEsconderijo()
        {
            if (controlador.Jogador.SairDoEsconderijo()) controlador.Som.Tocar(SomJogo.Efeito.Porta);
            AtualizarEstadoEscondido();
        }

        private void Acelerar()
        {
            G.PularPreparacao();
        }

        private void AtualizarEstadoEscondido()
        {
            var jogador = controlador.Jogador;
            bool escondido = jogador.Escondido;
            camadaEscondido.SetActive(escondido);
            dica.SetActive(!escondido);
            setaEsquerda.gameObject.SetActive(!escondido);
            setaDireita.gameObject.SetActive(!escondido);

            var escolhido = jogador.ObterEsconderijoEscolhido();
            foreach (var a in areas) a.DefinirSelecionado(a.Esconderijo == escolhido);
            if (escondido) textoEscondido.text = $"Escondido: {escolhido.Nome}";
        }

        /// <summary>Liga/desliga o contorno de todas as áreas (útil para conferir na apresentação).</summary>
        public void DefinirDepuracaoAreas(bool valor)
        {
            depuracaoAreas = valor;
            foreach (var a in areas) a.DefinirDepuracao(valor);
        }
    }
}
