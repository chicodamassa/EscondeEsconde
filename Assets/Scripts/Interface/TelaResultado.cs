using EscondeEsconde.Modelo;
using UnityEngine;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>Tela final com vitória/derrota, estatísticas da partida e placar.</summary>
    public class TelaResultado : Tela
    {
        private Image retrato, molduraRetrato;
        private RectTransform retratoRT;
        private Text titulo, motivo, fala, estatisticas, placar;

        protected override void Construir()
        {
            UIFabrica.Esticar(UIFabrica.CriarImagem("Fundo", raiz, null, UIFabrica.Noite).rectTransform);
            var planta = UIFabrica.CriarImagem("Planta", raiz, UIFabrica.Carregar("Arte/Planta/casatopdown"), new Color(1f, 1f, 1f, 0.22f));
            UIFabrica.Esticar(planta.rectTransform);
            var ajuste = planta.gameObject.AddComponent<AspectRatioFitter>();
            ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            ajuste.aspectRatio = MontadorCasa.LARGURA_PLANTA / MontadorCasa.ALTURA_PLANTA;
            UIFabrica.Esticar(UIFabrica.CriarImagem("Vinheta", raiz, UIFabrica.Vinheta, UIFabrica.ComAlfa(UIFabrica.Noite, 0.95f)).rectTransform);

            // retrato dentro de um círculo (a máscara esconde o corte da arte na parte de baixo)
            var aro = UIFabrica.CriarImagem("Aro", raiz, UIFabrica.Circulo, UIFabrica.Marrom);
            retratoRT = UIFabrica.Posicionar(aro.rectTransform, new Vector2(0.24f, 0.47f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(584f, 584f));
            UIFabrica.Sombra(aro, 10f, 0.5f);
            molduraRetrato = UIFabrica.CriarImagem("Moldura", retratoRT, UIFabrica.Circulo, UIFabrica.Azul);
            UIFabrica.Esticar(molduraRetrato.rectTransform, 12f);
            molduraRetrato.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            retrato = UIFabrica.CriarImagem("Retrato", molduraRetrato.transform, null, Color.white);
            retrato.preserveAspect = true;
            UIFabrica.Posicionar(retrato.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -36f), new Vector2(600f, 600f));

            var balao = UIFabrica.CriarPainel("Fala", raiz, UIFabrica.CremeClaro);
            UIFabrica.Contorno(balao, UIFabrica.Marrom, 3f);
            UIFabrica.Posicionar(balao.rectTransform, new Vector2(0.24f, 0.5f), new Vector2(0.5f, 0f), new Vector2(0f, 300f), new Vector2(620f, 90f));
            fala = UIFabrica.CriarTexto("Texto", balao.transform, "", 30, UIFabrica.Marrom, TextAnchor.MiddleCenter, true);
            UIFabrica.Esticar(fala.rectTransform, 10f);

            var painel = UIFabrica.CriarPainel("Painel", raiz, UIFabrica.ComAlfa(UIFabrica.CremeClaro, 0.97f));
            UIFabrica.Sombra(painel, 8f, 0.5f);
            var p = UIFabrica.Posicionar(painel.rectTransform, new Vector2(0.69f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 900f));
            var borda = UIFabrica.CriarImagem("Borda", p, UIFabrica.Moldura, UIFabrica.Marrom);
            borda.type = Image.Type.Sliced;
            UIFabrica.Esticar(borda.rectTransform);

            titulo = UIFabrica.CriarTexto("Titulo", p, "", 92, UIFabrica.Verde, TextAnchor.MiddleCenter, true);
            UIFabrica.Contorno(titulo, UIFabrica.Marrom, 3f);
            UIFabrica.Posicionar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(900f, 120f));
            motivo = UIFabrica.CriarTexto("Motivo", p, "", 32, UIFabrica.Marrom);
            UIFabrica.Posicionar(motivo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(860f, 90f));

            var tituloEst = UIFabrica.CriarTexto("TituloEstatisticas", p, "Resumo da partida", 32, UIFabrica.Laranja, TextAnchor.MiddleLeft, true);
            UIFabrica.Posicionar(tituloEst.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -268f), new Vector2(820f, 44f));
            estatisticas = UIFabrica.CriarTexto("Estatisticas", p, "", 29, UIFabrica.MarromClaro, TextAnchor.UpperLeft);
            estatisticas.lineSpacing = 1.2f;
            UIFabrica.Posicionar(estatisticas.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -320f), new Vector2(840f, 330f));

            placar = UIFabrica.CriarTexto("Placar", p, "", 30, UIFabrica.Marrom, TextAnchor.MiddleCenter, true);
            UIFabrica.Posicionar(placar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 160f), new Vector2(860f, 50f));

            var novamente = UIFabrica.CriarBotao("BotaoJogarNovamente", p, "Jogar de novo", UIFabrica.Laranja, () => controlador.JogarNovamente(), 38);
            UIFabrica.Posicionar((RectTransform)novamente.transform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-12f, 46f), new Vector2(400f, 96f));
            var menu = UIFabrica.CriarBotao("BotaoMenu", p, "Menu", UIFabrica.Azul, () => controlador.VoltarAoMenu(), 38, UIFabrica.Carregar("Arte/Gerada/icone_casa"));
            UIFabrica.Posicionar((RectTransform)menu.transform, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(12f, 46f), new Vector2(320f, 96f));
        }

        /// <summary>Preenche a tela com os dados do gerenciador e a exibe.</summary>
        public void Exibir(GerenciadorJogo g)
        {
            var jogador = g.Jogador;
            var procurador = g.Procurador;
            var esconderijo = jogador.ObterEsconderijoEscolhido();
            bool tempoAcabou = g.Derrotado && !jogador.Capturado;

            if (g.Venceu)
            {
                titulo.text = "VOCÊ VENCEU!";
                titulo.color = UIFabrica.Verde;
                retrato.sprite = UIFabrica.Carregar(ControladorJogo.ICONE_JOGADOR);
                molduraRetrato.color = UIFabrica.Hex("8FBF7A");
                fala.text = "\"" + jogador.ObterFala("vitoria") + "\"";
            }
            else
            {
                titulo.text = tempoAcabou ? "TEMPO ESGOTADO!" : "TE ACHARAM!";
                titulo.color = UIFabrica.Vermelho;
                retrato.sprite = UIFabrica.Carregar(ControladorJogo.ICONE_PROCURADOR);
                molduraRetrato.color = UIFabrica.Hex("E59A86");
                fala.text = "\"" + (tempoAcabou ? jogador.ObterFala("sem_esconderijo") : procurador.ObterFala("achou")) + "\"";
            }
            motivo.text = g.MotivoFim;

            string onde = esconderijo != null ? $"{esconderijo.Nome} ({esconderijo.Comodo.Nome})" : "nenhum";
            string chance = esconderijo != null ? $"{esconderijo.ChanceInvestigacao * 100f:0}%" : "-";
            estatisticas.text =
                $"•  Esconderijo: <b>{onde}</b>\n" +
                $"•  Chance base de ser revistado: <b>{chance}</b>\n" +
                $"•  Cômodos visitados: <b>{jogador.ComodosVisitados.Count} de {g.OrdemComodos.Count}</b>\n" +
                $"•  Trocas de esconderijo: <b>{jogador.TrocasDeEsconderijo}</b>   •   Tempo escondido: <b>{jogador.TempoEscondido:0} s</b>\n" +
                $"•  {procurador.Nome}: <b>{procurador.EsconderijosInvestigados}</b> revistados, <b>{procurador.EsconderijosIgnorados}</b> ignorados\n" +
                $"•  Dificuldade: <b>{ControladorJogo.NomeDificuldade(g.Dificuldade)}</b>";
            placar.text = $"Placar: {g.Vitorias} vitória(s) em {g.PartidasJogadas} partida(s) ({g.CalcularTaxaVitoria() * 100f:0}%)";

            Mostrar();
            controlador.Som.Tocar(g.Venceu ? SomJogo.Efeito.Vitoria : SomJogo.Efeito.Derrota);
        }

        private void Update()
        {
            float t = Time.unscaledTime;
            retratoRT.anchoredPosition = new Vector2(0f, 14f * Mathf.Sin(t * 2f));
            retratoRT.localEulerAngles = new Vector3(0f, 0f, 2.5f * Mathf.Sin(t * 1.3f));

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!controlador.Transicao.EmAndamento) controlador.JogarNovamente();
            }
        }
    }
}
