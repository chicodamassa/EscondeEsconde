using System;
using System.Collections;
using EscondeEsconde.Modelo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscondeEsconde.Interface
{
    /// <summary>
    /// Ponto de entrada do jogo na Unity (o único componente que precisa estar na cena).
    /// Cria o <see cref="GerenciadorJogo"/>, os personagens e as telas, e faz a ponte
    /// entre o modelo (regras) e a interface.
    /// </summary>
    public class ControladorJogo : MonoBehaviour
    {
        [Header("Personagens")]
        [SerializeField] private string nomeProcurador = "Procurador";
        [Tooltip("Rapidez do jogador ao trocar de cômodo (transição = 1 / velocidade segundos).")]
        [SerializeField] private float velocidadeJogador = 3f;
        [Tooltip("0 = sorteio diferente a cada execução.")]
        [SerializeField] private int sementeAleatoria = 0;

        public GerenciadorJogo Gerenciador { get; private set; }
        public Jogador Jogador { get; private set; }
        public Procurador Procurador { get; private set; }
        public SomJogo Som { get; private set; }
        public Dificuldade DificuldadeEscolhida { get; private set; } = Dificuldade.Normal;
        public string NomeJogador { get; private set; } = "Jogador";
        public RectTransform RaizCanvas { get; private set; }

        public TelaMenu TelaMenu { get; private set; }
        public TelaEsconder TelaEsconder { get; private set; }
        public TelaProcura TelaProcura { get; private set; }
        public TelaResultado TelaResultado { get; private set; }
        public Transicao Transicao { get; private set; }

        public static readonly Color CorJogador = UIFabrica.Hex("4F6C9E");
        public static readonly Color CorProcurador = UIFabrica.Hex("C8553D");
        public const string ICONE_JOGADOR = "Arte/Gerada/jogador_retrato";
        public const string ICONE_PROCURADOR = "Arte/Gerada/procurador_retrato";

        public static string NomeDificuldade(Dificuldade d)
        {
            switch (d)
            {
                case Dificuldade.Facil: return "Fácil";
                case Dificuldade.Dificil: return "Difícil";
                default: return "Normal";
            }
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            UIFabrica.Inicializar();
            Som = gameObject.AddComponent<SomJogo>();
            GarantirEventSystem();
            CriarCanvas();

            var params0 = GerenciadorJogo.ObterParametros(Dificuldade.Normal);
            Gerenciador = new GerenciadorJogo(MontadorCasa.CriarComodos(), MontadorCasa.OrdemComodos(),
                MontadorCasa.COMODO_INICIAL, params0.TempoPreparacao, Dificuldade.Normal);
            Gerenciador.FaseAlterada += AoMudarFase;

            TelaEsconder = CriarTela<TelaEsconder>("TelaEsconder");
            TelaProcura = CriarTela<TelaProcura>("TelaProcura");
            TelaResultado = CriarTela<TelaResultado>("TelaResultado");
            TelaMenu = CriarTela<TelaMenu>("TelaMenu");

            Transicao = UIFabrica.CriarRect("Transicao", RaizCanvas).gameObject.AddComponent<Transicao>();
            Transicao.Construir();

            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-autoteste") >= 0)
                gameObject.AddComponent<AutoTeste>();
        }

        private void Start()
        {
            TelaMenu.Mostrar();
        }

        private void Update()
        {
            // Durante as transições o jogo fica "pausado" (o tempo não corre).
            if (!Transicao.EmAndamento) Gerenciador.Atualizar(Time.deltaTime);
        }

        // ------------------------------------------------------------------ ações chamadas pelas telas
        public void IniciarJogo(string nome, Dificuldade dificuldade)
        {
            NomeJogador = string.IsNullOrWhiteSpace(nome) ? "Jogador" : nome.Trim();
            DificuldadeEscolhida = dificuldade;
            var p = GerenciadorJogo.ObterParametros(dificuldade);
            Gerenciador.ConfigurarDificuldade(dificuldade, p.TempoPreparacao);

            // Os objetos são criados uma vez só e reaproveitados: assim o histórico do jogador
            // e a memória do procurador continuam valendo entre as partidas.
            if (Jogador == null)
                Jogador = new Jogador(NomeJogador, velocidadeJogador, CorJogador, Gerenciador.ObterComodo(MontadorCasa.COMODO_INICIAL), ICONE_JOGADOR);
            else
                Jogador.Renomear(NomeJogador);

            if (Procurador == null)
            {
                int semente = sementeAleatoria != 0 ? sementeAleatoria : Environment.TickCount;
                Procurador = new Procurador(nomeProcurador, p.VelocidadeProcurador, CorProcurador, p.TempoInvestigacao,
                    p.BonusPercepcao, MontadorCasa.PortaDaFrente, semente);
            }
            else
            {
                Procurador.ConfigurarDificuldade(p.VelocidadeProcurador, p.TempoInvestigacao, p.BonusPercepcao);
            }

            Som.PararMusica();
            Transicao.Executar("Esconda-se!", $"Você tem {p.TempoPreparacao:0} segundos antes de {nomeProcurador} começar a procurar.",
                1.4f, () => Gerenciador.IniciarPartida(Jogador, Procurador));
        }

        public void JogarNovamente()
        {
            IniciarJogo(NomeJogador, DificuldadeEscolhida);
        }

        public void VoltarAoMenu()
        {
            Som.PararMusica();
            Transicao.Executar("", "", 0.1f, () =>
            {
                OcultarTelas();
                TelaMenu.Mostrar();
            });
        }

        public void Sair()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ------------------------------------------------------------------ reação às fases do modelo
        private void AoMudarFase(FaseJogo fase)
        {
            switch (fase)
            {
                case FaseJogo.Preparacao:
                    OcultarTelas();
                    TelaEsconder.Mostrar();
                    Som.TocarMusica(1f);
                    break;

                case FaseJogo.Investigacao:
                    TelaProcura.Preparar(); // precisa acontecer antes do procurador começar a andar
                    Som.TocarMusica(0.85f);
                    Transicao.Executar("Pronto ou não...", $"...lá vou eu! — {Procurador.Nome}", 1.6f, () =>
                    {
                        OcultarTelas();
                        TelaProcura.Mostrar();
                    });
                    break;

                case FaseJogo.Encerrado:
                    Som.PararMusica();
                    StartCoroutine(MostrarResultado());
                    break;
            }
        }

        private IEnumerator MostrarResultado()
        {
            bool foiPego = Jogador.Capturado;
            bool tempoAcabou = Gerenciador.Derrotado && !foiPego;

            if (foiPego) yield return new WaitForSeconds(2.6f);       // deixa ver o "TE ACHEI!"
            else if (Gerenciador.Venceu) yield return new WaitForSeconds(1.8f);
            else if (tempoAcabou) Som.Tocar(SomJogo.Efeito.Derrota);

            string titulo = Gerenciador.Venceu ? "Você venceu!" : (tempoAcabou ? "Tempo esgotado!" : "Encontrado!");
            Transicao.Executar(titulo, "", 0.9f, () =>
            {
                OcultarTelas();
                TelaResultado.Exibir(Gerenciador);
            });
        }

        // ------------------------------------------------------------------ montagem
        private T CriarTela<T>(string nome) where T : Tela
        {
            var tela = UIFabrica.CriarRect(nome, RaizCanvas).gameObject.AddComponent<T>();
            tela.Inicializar(this);
            return tela;
        }

        private void OcultarTelas()
        {
            TelaMenu.Ocultar();
            TelaEsconder.Ocultar();
            TelaProcura.Ocultar();
            TelaResultado.Ocultar();
        }

        private void CriarCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = 5;
            go.transform.SetParent(transform, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var escala = go.GetComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escala.matchWidthOrHeight = 0.5f;

            RaizCanvas = (RectTransform)go.transform;
        }

        private static void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }
}
