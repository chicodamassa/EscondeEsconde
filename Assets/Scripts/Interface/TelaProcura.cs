using System.Collections.Generic;
using EscondeEsconde.Modelo;
using UnityEngine;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// FASE DE BUSCA (visão 2D de cima): o procurador anda pela planta da casa e
    /// passa por todos os esconderijos. O painel lateral mostra o sorteio de cada um.
    /// </summary>
    public class TelaProcura : Tela
    {
        private const float W = MontadorCasa.LARGURA_PLANTA;
        private const float H = MontadorCasa.ALTURA_PLANTA;

        /// <summary>Elementos visuais de um esconderijo na planta.</summary>
        private class Marcador
        {
            public RectTransform Raiz;
            public Image Fundo;
            public Image Icone;
            public bool DoJogador;
        }

        // planta
        private RectTransform planta;
        private RectTransform camadaPegadas, camadaMarcadores;
        private readonly Dictionary<string, Marcador> marcadores = new Dictionary<string, Marcador>();
        private readonly Dictionary<string, Image> sombrasComodos = new Dictionary<string, Image>();
        private readonly Dictionary<string, Image> destaquesComodos = new Dictionary<string, Image>();
        private RectTransform procuradorRT, lanternaRT, balaoRT, lupaRT;
        private Text balaoTexto;
        private float tempoBalao;
        private Text textoAlerta;
        private float tempoAlerta;
        private readonly List<(Image img, float nascimento)> pegadas = new List<(Image, float)>();
        private Vector2 ultimaPegada;
        private bool pegadaEsquerda;
        private float anguloAtual;
        private Marcador marcadorAtual;

        // painel lateral
        private Text textoOndeVoce, textoComodo, textoEsconderijo, textoChance, textoDecisao, textoContadores, textoDificuldade;
        private RectTransform barraPreenchida, agulha;
        private bool agulhaSorteando;
        private readonly List<Text> linhasDiario = new List<Text>();
        private readonly List<string> diario = new List<string>();
        private readonly Dictionary<float, Button> botoesVelocidade = new Dictionary<float, Button>();

        private Procurador procurador;
        private GerenciadorJogo G => controlador.Gerenciador;

        // ================================================================== construção
        protected override void Construir()
        {
            UIFabrica.Esticar(UIFabrica.CriarImagem("Fundo", raiz, null, UIFabrica.Hex("2A1F1C")).rectTransform);

            // ---- título e situação do jogador ----
            var titulo = UIFabrica.Sombra(UIFabrica.CriarTexto("Titulo", raiz, "FASE DE BUSCA", 56, UIFabrica.Amarelo, TextAnchor.MiddleLeft, true), 4f, 0.6f);
            UIFabrica.Posicionar(titulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -18f), new Vector2(560f, 80f));
            var miniRetrato = UIFabrica.CriarImagem("Retrato", raiz, UIFabrica.Carregar(ControladorJogo.ICONE_JOGADOR), Color.white);
            miniRetrato.preserveAspect = true;
            UIFabrica.Posicionar(miniRetrato.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(560f, -10f), new Vector2(92f, 92f));
            textoOndeVoce = UIFabrica.CriarTexto("OndeVoce", raiz, "", 30, UIFabrica.Creme, TextAnchor.MiddleLeft);
            UIFabrica.Posicionar(textoOndeVoce.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(660f, -18f), new Vector2(720f, 80f));

            // ---- área da planta ----
            var area = UIFabrica.Area(UIFabrica.CriarRect("AreaPlanta", raiz), new Vector2(0f, 0f), new Vector2(0.72f, 1f));
            area.offsetMin = new Vector2(34f, 30f);
            area.offsetMax = new Vector2(-14f, -110f);

            var imgPlanta = UIFabrica.CriarImagem("Planta", area, UIFabrica.Carregar("Arte/Planta/casatopdown"), Color.white);
            planta = UIFabrica.Esticar(imgPlanta.rectTransform);
            var ajuste = planta.gameObject.AddComponent<AspectRatioFitter>();
            ajuste.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            ajuste.aspectRatio = W / H;
            planta.gameObject.AddComponent<RectMask2D>();

            var camadaComodos = UIFabrica.Esticar(UIFabrica.CriarRect("Comodos", planta));
            foreach (var comodo in controlador.Gerenciador.TodosOsComodos) CriarCamadaComodo(camadaComodos, comodo);
            camadaPegadas = UIFabrica.Esticar(UIFabrica.CriarRect("Pegadas", planta));

            var lanterna = UIFabrica.CriarImagem("Lanterna", planta, UIFabrica.Lanterna, new Color(0.03f, 0.02f, 0.05f, 0.58f));
            lanternaRT = lanterna.rectTransform;
            camadaMarcadores = UIFabrica.Esticar(UIFabrica.CriarRect("Marcadores", planta));

            var imgProcurador = UIFabrica.CriarImagem("Procurador", planta, UIFabrica.Carregar("Arte/Gerada/procurador_topo"), Color.white);
            imgProcurador.preserveAspect = true;
            procuradorRT = UIFabrica.Posicionar(imgProcurador.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(124f, 124f));

            var lupa = UIFabrica.CriarImagem("Lupa", planta, UIFabrica.Carregar("Arte/Gerada/icone_lupa"), Color.white);
            lupa.preserveAspect = true;
            lupaRT = UIFabrica.Posicionar(lupa.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(64f, 64f));

            var balao = UIFabrica.CriarPainel("Balao", planta, UIFabrica.CremeClaro);
            UIFabrica.Contorno(balao, UIFabrica.Marrom, 2.5f);
            balaoRT = UIFabrica.Posicionar(balao.rectTransform, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(330f, 64f));
            balaoTexto = UIFabrica.CriarTexto("Texto", balaoRT, "", 26, UIFabrica.Marrom, TextAnchor.MiddleCenter, true);
            UIFabrica.Esticar(balaoTexto.rectTransform, 6f);

            textoAlerta = UIFabrica.CriarTexto("Alerta", planta, "", 130, UIFabrica.Vermelho, TextAnchor.MiddleCenter, true);
            UIFabrica.Contorno(textoAlerta, UIFabrica.CremeClaro, 5f);
            UIFabrica.Sombra(textoAlerta, 8f, 0.6f);
            UIFabrica.Posicionar(textoAlerta.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 200f));

            ConstruirPainelLateral();
        }

        private void CriarCamadaComodo(RectTransform pai, Comodo comodo)
        {
            Rect r = comodo.AreaPlanta;
            var ancoraMin = new Vector2(r.xMin / W, 1f - r.yMax / H);
            var ancoraMax = new Vector2(r.xMax / W, 1f - r.yMin / H);

            var sombra = UIFabrica.CriarImagem("Sombra_" + comodo.Id, pai, null, new Color(0.1f, 0.08f, 0.1f, 0f));
            UIFabrica.Area(sombra.rectTransform, ancoraMin, ancoraMax);
            sombrasComodos[comodo.Id] = sombra;

            var destaque = UIFabrica.CriarImagem("Destaque_" + comodo.Id, pai, UIFabrica.Moldura, UIFabrica.ComAlfa(UIFabrica.Amarelo, 0f));
            destaque.type = Image.Type.Sliced;
            destaque.pixelsPerUnitMultiplier = 1.5f;
            UIFabrica.Area(destaque.rectTransform, ancoraMin, ancoraMax);
            destaquesComodos[comodo.Id] = destaque;

            var fundoRotulo = UIFabrica.CriarPainel("Rotulo_" + comodo.Id, pai, UIFabrica.ComAlfa(UIFabrica.Marrom, 0.82f), 2f);
            var rr = UIFabrica.Posicionar(fundoRotulo.rectTransform, new Vector2(ancoraMin.x, ancoraMax.y), new Vector2(0f, 1f), new Vector2(10f, -10f), new Vector2(comodo.Nome.Length * 15f + 34f, 40f));
            var txt = UIFabrica.CriarTexto("Texto", rr, comodo.Nome, 24, UIFabrica.CremeClaro, TextAnchor.MiddleCenter, true);
            UIFabrica.Esticar(txt.rectTransform);
        }

        private void ConstruirPainelLateral()
        {
            var painel = UIFabrica.CriarPainel("PainelLateral", raiz, UIFabrica.CremeClaro);
            var p = UIFabrica.Area(painel.rectTransform, new Vector2(0.72f, 0f), new Vector2(1f, 1f));
            p.offsetMin = new Vector2(10f, 30f);
            p.offsetMax = new Vector2(-30f, -30f);
            var borda = UIFabrica.CriarImagem("Borda", p, UIFabrica.Moldura, UIFabrica.Marrom);
            borda.type = Image.Type.Sliced;
            UIFabrica.Esticar(borda.rectTransform);

            // cabeçalho com o retrato do procurador
            var retrato = UIFabrica.CriarImagem("Retrato", p, UIFabrica.Carregar(ControladorJogo.ICONE_PROCURADOR), Color.white);
            retrato.preserveAspect = true;
            UIFabrica.Posicionar(retrato.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -10f), new Vector2(150f, 150f));
            var nome = UIFabrica.CriarTexto("Nome", p, "Procurador", 40, UIFabrica.Vermelho, TextAnchor.MiddleLeft, true);
            UIFabrica.Posicionar(nome.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -34f), new Vector2(330f, 54f));
            textoDificuldade = UIFabrica.CriarTexto("Dificuldade", p, "", 22, UIFabrica.MarromClaro, TextAnchor.MiddleLeft);
            UIFabrica.Posicionar(textoDificuldade.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -88f), new Vector2(330f, 60f));

            float y = -170f;
            textoComodo = Linha(p, "Comodo", "", 28, UIFabrica.MarromClaro, ref y, 40f, false);
            textoEsconderijo = Linha(p, "Esconderijo", "", 34, UIFabrica.Marrom, ref y, 48f, true);

            var rotuloChance = UIFabrica.CriarTexto("RotuloChance", p, "Chance de revistar", 24, UIFabrica.MarromClaro, TextAnchor.MiddleLeft);
            UIFabrica.Posicionar(rotuloChance.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, y - 4f), new Vector2(300f, 34f));
            textoChance = UIFabrica.CriarTexto("Chance", p, "", 30, UIFabrica.Laranja, TextAnchor.MiddleRight, true);
            UIFabrica.Posicionar(textoChance.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, y - 4f), new Vector2(160f, 34f));
            y -= 44f;

            var barra = UIFabrica.CriarPainel("Barra", p, UIFabrica.ComAlfa(UIFabrica.MarromClaro, 0.25f), 3f);
            var rb = UIFabrica.Area(barra.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f));
            rb.offsetMin = new Vector2(26f, y - 38f);
            rb.offsetMax = new Vector2(-26f, y);
            var preenchida = UIFabrica.CriarPainel("Preenchida", rb, UIFabrica.Laranja, 3f);
            barraPreenchida = UIFabrica.Area(preenchida.rectTransform, Vector2.zero, new Vector2(0.5f, 1f));
            var imgAgulha = UIFabrica.CriarImagem("Agulha", rb, null, UIFabrica.Marrom);
            agulha = UIFabrica.Posicionar(imgAgulha.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8f, 58f));
            y -= 60f;

            var legenda = UIFabrica.CriarTexto("Legenda", p, "Se o sorteio (traço escuro) cair no laranja, ele revista.", 20, UIFabrica.MarromClaro, TextAnchor.MiddleLeft);
            UIFabrica.Posicionar(legenda.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, y), new Vector2(480f, 30f));
            y -= 36f;
            textoDecisao = Linha(p, "Decisao", "", 40, UIFabrica.Marrom, ref y, 58f, true);

            var separador = UIFabrica.CriarImagem("Separador", p, null, UIFabrica.ComAlfa(UIFabrica.Marrom, 0.25f));
            UIFabrica.Posicionar(separador.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y - 6f), new Vector2(460f, 3f));
            y -= 18f;
            Linha(p, "TituloDiario", "Diário da busca", 26, UIFabrica.Marrom, ref y, 40f, true);
            for (int i = 0; i < 8; i++)
            {
                var t = Linha(p, "Diario" + i, "", 21, UIFabrica.MarromClaro, ref y, 31f, false);
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                linhasDiario.Add(t);
            }

            textoContadores = UIFabrica.CriarTexto("Contadores", p, "", 22, UIFabrica.Marrom, TextAnchor.MiddleCenter);
            UIFabrica.Posicionar(textoContadores.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(480f, 34f));

            float[] velocidades = { 1f, 2f, 4f };
            for (int i = 0; i < velocidades.Length; i++)
            {
                float v = velocidades[i];
                var b = UIFabrica.CriarBotao("Velocidade" + v, p, v + "x", UIFabrica.Azul, () => DefinirVelocidade(v), 30, UIFabrica.Carregar("Arte/Gerada/icone_acelerar"));
                UIFabrica.Posicionar((RectTransform)b.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2((i - 1) * 150f, 26f), new Vector2(136f, 70f));
                botoesVelocidade[v] = b;
            }
        }

        private Text Linha(RectTransform pai, string nome, string conteudo, int tamanho, Color cor, ref float y, float altura, bool titulo)
        {
            var t = UIFabrica.CriarTexto(nome, pai, conteudo, tamanho, cor, TextAnchor.MiddleLeft, titulo);
            var rt = UIFabrica.Area(t.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f));
            rt.offsetMin = new Vector2(26f, y - altura);
            rt.offsetMax = new Vector2(-20f, y);
            y -= altura;
            return t;
        }

        // ================================================================== início da busca
        /// <summary>Chamado ANTES do procurador começar, para escutar os eventos dele.</summary>
        public void Preparar()
        {
            if (procurador != null) Desinscrever(procurador);
            procurador = controlador.Procurador;
            procurador.EntrouNoComodo += AoEntrarNoComodo;
            procurador.ChegouAoEsconderijo += AoChegarNoEsconderijo;
            procurador.DecidiuInvestigar += AoDecidir;
            procurador.Investigou += AoInvestigar;
            procurador.ComodoConcluido += AoConcluirComodo;
            procurador.BuscaConcluida += AoConcluirBusca;
            procurador.Falou += AoFalar;

            // limpa a partida anterior
            foreach (var m in marcadores.Values) Destroy(m.Raiz.gameObject);
            marcadores.Clear();
            foreach (var p in pegadas) if (p.img != null) Destroy(p.img.gameObject);
            pegadas.Clear();
            diario.Clear();
            foreach (var l in linhasDiario) l.text = "";
            foreach (var s in sombrasComodos.Values) s.color = UIFabrica.ComAlfa(s.color, 0f);
            foreach (var d in destaquesComodos.Values) d.color = UIFabrica.ComAlfa(d.color, 0f);
            marcadorAtual = null;
            tempoBalao = 0f;
            tempoAlerta = 0f;
            textoAlerta.text = "";
            textoComodo.text = "";
            textoEsconderijo.text = "Chegando na casa...";
            textoChance.text = "";
            textoDecisao.text = "";
            barraPreenchida.anchorMax = new Vector2(0f, 1f);
            agulha.anchorMin = agulha.anchorMax = new Vector2(0f, 0.5f);
            agulhaSorteando = false;
            ultimaPegada = procurador.PosicaoInicial;
            anguloAtual = 180f;

            var jogador = controlador.Jogador;
            var escolhido = jogador.ObterEsconderijoEscolhido();
            textoOndeVoce.text = escolhido != null
                ? $"{jogador.Nome} está escondido: <b>{escolhido.Nome}</b> ({escolhido.Comodo.Nome})"
                : "";
            var parametros = GerenciadorJogo.ObterParametros(G.Dificuldade);
            textoDificuldade.text = $"Dificuldade: {ControladorJogo.NomeDificuldade(G.Dificuldade)}\nPercepção: {(parametros.BonusPercepcao >= 0 ? "+" : "")}{parametros.BonusPercepcao * 100:0}%";

            foreach (var comodo in G.TodosOsComodos)
                foreach (var e in comodo.ObterEsconderijos())
                    marcadores[e.Id] = CriarMarcador(e, e == escolhido);

            DefinirVelocidade(1f);
            Posicionar(procuradorRT, procurador.Posicao);
            AtualizarContadores();
        }

        private void Desinscrever(Procurador p)
        {
            p.EntrouNoComodo -= AoEntrarNoComodo;
            p.ChegouAoEsconderijo -= AoChegarNoEsconderijo;
            p.DecidiuInvestigar -= AoDecidir;
            p.Investigou -= AoInvestigar;
            p.ComodoConcluido -= AoConcluirComodo;
            p.BuscaConcluida -= AoConcluirBusca;
            p.Falou -= AoFalar;
        }

        private Marcador CriarMarcador(Esconderijo e, bool doJogador)
        {
            var m = new Marcador { DoJogador = doJogador };
            float tam = doJogador ? 76f : 34f;
            m.Fundo = UIFabrica.CriarImagem("Marcador_" + e.Id, camadaMarcadores, UIFabrica.Circulo, doJogador ? UIFabrica.Azul : UIFabrica.CremeClaro);
            UIFabrica.Contorno(m.Fundo, UIFabrica.Marrom, 2.5f);
            m.Raiz = m.Fundo.rectTransform;
            Posicionar(m.Raiz, e.PosicaoPlanta);
            m.Raiz.sizeDelta = new Vector2(tam, tam);
            m.Icone = UIFabrica.CriarImagem("Icone", m.Raiz, doJogador ? UIFabrica.Carregar(ControladorJogo.ICONE_JOGADOR) : null, doJogador ? Color.white : UIFabrica.ComAlfa(UIFabrica.Marrom, 0f));
            m.Icone.preserveAspect = true;
            UIFabrica.Esticar(m.Icone.rectTransform, doJogador ? -6f : 4f);
            return m;
        }

        // ================================================================== eventos do procurador
        private void AoEntrarNoComodo(Comodo comodo)
        {
            foreach (var par in destaquesComodos)
                par.Value.color = UIFabrica.ComAlfa(UIFabrica.Amarelo, par.Key == comodo.Id ? 1f : 0f);
            textoComodo.text = $"Revistando: {comodo.Nome}";
            AdicionarDiario($"<b>Entrou: {comodo.Nome}</b>");
            controlador.Som.Tocar(SomJogo.Efeito.Porta, 0.6f);
        }

        private void AoChegarNoEsconderijo(Esconderijo e, float chance)
        {
            textoEsconderijo.text = e.Nome;
            textoChance.text = $"{chance * 100f:0}%";
            barraPreenchida.anchorMax = new Vector2(chance, 1f);
            textoDecisao.text = "<color=#6B4A3A>Pensando...</color>";
            agulhaSorteando = true;
            marcadorAtual = marcadores[e.Id];
            if (e.Ocupado)
            {
                controlador.Som.Tocar(SomJogo.Efeito.Coracao);
                MostrarAlerta("...", UIFabrica.Creme, 0.9f);
            }
        }

        private void AoDecidir(Esconderijo e, float chance, float sorteio, bool vaiRevistar)
        {
            agulhaSorteando = false;
            agulha.anchorMin = agulha.anchorMax = new Vector2(sorteio, 0.5f);
            textoDecisao.text = vaiRevistar ? "<color=#C8553D>Vai revistar!</color>" : "<color=#6E9F5A>Passou direto!</color>";
            if (vaiRevistar) controlador.Som.Tocar(SomJogo.Efeito.Lupa);
        }

        private void AoInvestigar(Esconderijo e, ResultadoInvestigacao resultado)
        {
            var m = marcadores[e.Id];
            m.Raiz.localScale = Vector3.one;
            switch (resultado)
            {
                case ResultadoInvestigacao.EncontrouJogador:
                    m.Fundo.color = UIFabrica.Vermelho;
                    AdicionarIcone(m, "Arte/Gerada/icone_xis");
                    AdicionarDiario($"<color=#C8553D><b>Revistou {e.Nome}: TE ACHOU!</b></color>");
                    MostrarAlerta("TE ACHEI!", UIFabrica.Vermelho, 3f);
                    controlador.Som.Tocar(SomJogo.Efeito.Encontrado);
                    break;

                case ResultadoInvestigacao.JogadorEscapou:
                    m.Fundo.color = UIFabrica.Verde;
                    AdicionarIcone(m, "Arte/Gerada/icone_check");
                    AdicionarDiario($"<color=#6E9F5A><b>Passou direto por {e.Nome}... ufa!</b></color>");
                    MostrarAlerta("UFA! Passou direto!", UIFabrica.Verde, 2f);
                    if (G.MultiplicadorVelocidade < 3f) DefinirVelocidade(4f); // o resto da busca vai mais rápido
                    break;

                default:
                    if (e.Investigado)
                    {
                        m.Fundo.color = UIFabrica.Laranja;
                        m.Icone.sprite = UIFabrica.Carregar("Arte/Gerada/icone_lupa");
                        m.Icone.color = Color.white;
                        AdicionarDiario($"Revistou {e.Nome}: vazio");
                    }
                    else
                    {
                        m.Fundo.color = UIFabrica.ComAlfa(UIFabrica.Cinza, 0.55f);
                        AdicionarDiario($"<color=#8A8F98>Ignorou {e.Nome}</color>");
                    }
                    break;
            }
            marcadorAtual = null;
            AtualizarContadores();
        }

        private void AoConcluirComodo(Comodo comodo)
        {
            var s = sombrasComodos[comodo.Id];
            s.color = UIFabrica.ComAlfa(s.color, 0.35f);
            destaquesComodos[comodo.Id].color = UIFabrica.ComAlfa(UIFabrica.Amarelo, 0f);
        }

        private void AoConcluirBusca()
        {
            MostrarAlerta("Ele desistiu!", UIFabrica.Verde, 3f);
            textoDecisao.text = "<color=#6E9F5A>Busca encerrada</color>";
            controlador.Som.Tocar(SomJogo.Efeito.Vitoria);
        }

        private void AoFalar(string texto)
        {
            balaoTexto.text = texto;
            balaoRT.sizeDelta = new Vector2(Mathf.Max(170f, texto.Length * 15f + 40f), 60f);
            tempoBalao = 1.9f;
        }

        // ================================================================== animação a cada quadro
        private void Update()
        {
            if (procurador == null) return;
            float dt = Time.deltaTime;
            Vector2 pos = procurador.Posicao;

            // procurador: posição, rotação e "balanço" ao andar
            Posicionar(procuradorRT, pos);
            Vector2 d = procurador.Direcao;
            float alvo = Mathf.Atan2(-d.y, d.x) * Mathf.Rad2Deg - 90f;
            anguloAtual = Mathf.LerpAngle(anguloAtual, alvo, dt * 10f);
            procuradorRT.localEulerAngles = new Vector3(0f, 0f, anguloAtual);
            bool andando = procurador.Estado == EstadoProcurador.Andando && procurador.Ativo;
            procuradorRT.localScale = Vector3.one * (andando ? 1f + 0.06f * Mathf.Sin(Time.time * 16f) : 1f);

            // lanterna (escuridão em volta do procurador)
            Posicionar(lanternaRT, pos);
            float lado = planta.rect.width * 2.6f;
            lanternaRT.sizeDelta = new Vector2(lado, lado);

            // pegadas
            if (andando && Vector2.Distance(pos, ultimaPegada) > 30f) CriarPegada(pos);
            for (int i = pegadas.Count - 1; i >= 0; i--)
            {
                float idade = Time.time - pegadas[i].nascimento;
                if (idade > 3f) { Destroy(pegadas[i].img.gameObject); pegadas.RemoveAt(i); continue; }
                pegadas[i].img.color = UIFabrica.ComAlfa(Color.white, 0.75f * (1f - idade / 3f));
            }

            // lupa enquanto revista
            bool revistando = procurador.Estado == EstadoProcurador.Investigando;
            lupaRT.gameObject.SetActive(revistando);
            if (revistando)
            {
                Posicionar(lupaRT, pos);
                float ang = Time.time * 6f;
                lupaRT.anchoredPosition = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 34f + new Vector2(0f, 20f);
            }

            // balão de fala
            tempoBalao -= dt;
            balaoRT.gameObject.SetActive(tempoBalao > 0f);
            if (tempoBalao > 0f)
            {
                // mantém o balão dentro da planta (não deixa ser cortado nas bordas)
                float meiaLargura = balaoRT.sizeDelta.x * 0.5f / Mathf.Max(1f, planta.rect.width) * W + 8f;
                Vector2 ancoraBalao = new Vector2(Mathf.Clamp(pos.x, meiaLargura, W - meiaLargura), Mathf.Max(pos.y, 90f));
                Posicionar(balaoRT, ancoraBalao);
                balaoRT.anchoredPosition = new Vector2(0f, 64f);
            }

            // agulha "sorteando"
            if (agulhaSorteando)
            {
                float x = Mathf.PerlinNoise(Time.time * 4f, 0.3f);
                agulha.anchorMin = agulha.anchorMax = new Vector2(Mathf.Clamp01(x * 1.3f - 0.15f), 0.5f);
            }

            // marcador do esconderijo atual pulsando
            if (marcadorAtual != null) marcadorAtual.Raiz.localScale = Vector3.one * (1.25f + 0.2f * Mathf.Sin(Time.time * 10f));

            // alerta grande
            if (tempoAlerta > 0f)
            {
                tempoAlerta -= dt;
                float k = Mathf.Clamp01(tempoAlerta * 2f);
                textoAlerta.color = UIFabrica.ComAlfa(textoAlerta.color, k);
                textoAlerta.rectTransform.localScale = Vector3.one * (1f + 0.05f * Mathf.Sin(Time.time * 12f));
            }
        }

        // ================================================================== auxiliares
        /// <summary>Converte um ponto da planta (pixels, origem em cima) para âncora na tela.</summary>
        private static void Posicionar(RectTransform rt, Vector2 pontoPlanta)
        {
            var ancora = new Vector2(pontoPlanta.x / W, 1f - pontoPlanta.y / H);
            rt.anchorMin = ancora;
            rt.anchorMax = ancora;
            rt.anchoredPosition = Vector2.zero;
        }

        private void CriarPegada(Vector2 pos)
        {
            Vector2 d = procurador.Direcao;
            Vector2 lateral = new Vector2(-d.y, d.x) * (pegadaEsquerda ? 9f : -9f);
            pegadaEsquerda = !pegadaEsquerda;
            ultimaPegada = pos;

            var img = UIFabrica.CriarImagem("Pegada", camadaPegadas, UIFabrica.Carregar("Arte/Gerada/pegada"), Color.white);
            Posicionar(img.rectTransform, pos + lateral);
            img.rectTransform.sizeDelta = new Vector2(24f, 24f);
            img.rectTransform.localEulerAngles = new Vector3(0f, 0f, anguloAtual);
            pegadas.Add((img, Time.time));
            if (pegadaEsquerda) controlador.Som.Tocar(SomJogo.Efeito.Passo, 0.5f);
        }

        private void AdicionarIcone(Marcador m, string sprite)
        {
            var icone = UIFabrica.CriarImagem("Resultado", m.Raiz, UIFabrica.Carregar(sprite), Color.white);
            UIFabrica.Posicionar(icone.rectTransform, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-6f, -6f), new Vector2(44f, 44f));
        }

        private void MostrarAlerta(string texto, Color cor, float duracao)
        {
            textoAlerta.text = texto;
            textoAlerta.color = cor;
            tempoAlerta = duracao;
        }

        private void AdicionarDiario(string linha)
        {
            diario.Add(linha);
            int inicio = Mathf.Max(0, diario.Count - linhasDiario.Count);
            for (int i = 0; i < linhasDiario.Count; i++)
            {
                int idx = inicio + i;
                linhasDiario[i].text = idx < diario.Count ? diario[idx] : "";
            }
        }

        private void AtualizarContadores()
        {
            int total = 0, vistos = 0;
            foreach (var c in G.TodosOsComodos)
            {
                total += c.QuantidadeEsconderijos;
                vistos += c.ContarEsconderijosInvestigados();
            }
            textoContadores.text = $"Revistados: {procurador.EsconderijosInvestigados}   •   Ignorados: {procurador.EsconderijosIgnorados}   •   Faltam: {total - vistos}";
        }

        private void DefinirVelocidade(float multiplicador)
        {
            G.DefinirMultiplicadorVelocidade(multiplicador);
            foreach (var par in botoesVelocidade)
            {
                var img = par.Value.GetComponent<Image>();
                img.color = par.Key == multiplicador ? UIFabrica.Laranja : UIFabrica.Azul;
            }
        }
    }
}
