using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscondeEsconde.Modelo
{
    /// <summary>
    /// CLASSE FILHA de <see cref="Personagem"/>: quem procura o jogador.
    /// Percorre a casa cômodo por cômodo e, em cada esconderijo, sorteia se vai
    /// revistar ou não (usando a chance do esconderijo + a sua percepção).
    /// Ele também tem MEMÓRIA: lembra onde já encontrou o jogador em partidas anteriores.
    /// </summary>
    public class Procurador : Personagem
    {
        public const float TEMPO_PENSANDO = 0.8f;   // segundos "decidindo" em cada esconderijo
        public const float BONUS_MEMORIA = 0.15f;   // +15% de chance onde já achou o jogador

        // ------------------------- obrigatórios (construtor) -------------------------
        // (+ nome, velocidade e cor, herdados de Personagem)
        private float tempoInvestigacao;        // segundos revistando cada esconderijo
        private float bonusPercepcao;           // soma na chance de todos os esconderijos (dificuldade)
        private readonly Vector2 posicaoInicial; // porta da frente da casa

        // ------------------------- estado -------------------------
        private EstadoProcurador estado;
        private readonly List<Comodo> comodosPendentes = new List<Comodo>();
        private readonly List<Esconderijo> esconderijosAtuais = new List<Esconderijo>();
        private Comodo primeiroComodo;
        private Esconderijo esconderijoAtual;
        private float temporizadorInvestigacao;
        private readonly Queue<Vector2> rota = new Queue<Vector2>();
        private bool indoParaEsconderijo;
        private Vector2 direcao;
        private int esconderijosInvestigados;
        private int esconderijosIgnorados;
        private bool encontrouJogador;
        private float ultimaChance;
        private float ultimoSorteio;
        private string falaAtual;
        private readonly System.Random sorteador;
        // Dicionário: id do esconderijo -> resultado nesta partida
        private readonly Dictionary<string, ResultadoInvestigacao> registroInvestigacoes = new Dictionary<string, ResultadoInvestigacao>();
        // Dicionário: id do esconderijo -> quantas vezes já encontrou o jogador ali (memória entre partidas)
        private readonly Dictionary<string, int> memoria = new Dictionary<string, int>();

        // ------------------------- eventos (a interface "escuta" o procurador) -------------------------
        public event Action<Comodo> EntrouNoComodo;
        public event Action<Esconderijo, float> ChegouAoEsconderijo;
        public event Action<Esconderijo, float, float, bool> DecidiuInvestigar;
        public event Action<Esconderijo, ResultadoInvestigacao> Investigou;
        public event Action<Comodo> ComodoConcluido;
        public event Action BuscaConcluida;
        public event Action<string> Falou;

        public Procurador(string nome, float velocidade, Color cor, float tempoInvestigacao, float bonusPercepcao, Vector2 posicaoInicial, int semente)
            : base(nome, velocidade, cor)
        {
            DefinirTempoInvestigacao(tempoInvestigacao);
            this.bonusPercepcao = Mathf.Clamp(bonusPercepcao, -0.5f, 0.5f);
            this.posicaoInicial = posicaoInicial;
            sorteador = new System.Random(semente);
            ReiniciarEstado();
        }

        // ------------------------- propriedades -------------------------
        public float TempoInvestigacao => tempoInvestigacao;
        public float BonusPercepcao => bonusPercepcao;
        public Vector2 PosicaoInicial => posicaoInicial;
        public EstadoProcurador Estado => estado;
        public Esconderijo EsconderijoAtual => esconderijoAtual;
        public Vector2 Direcao => direcao;
        public int EsconderijosInvestigados => esconderijosInvestigados;
        public int EsconderijosIgnorados => esconderijosIgnorados;
        public bool EncontrouJogador => encontrouJogador;
        public float UltimaChance => ultimaChance;
        public float UltimoSorteio => ultimoSorteio;
        public string FalaAtual => falaAtual;
        public int ComodosRestantes => comodosPendentes.Count;
        public IReadOnlyDictionary<string, ResultadoInvestigacao> RegistroInvestigacoes => registroInvestigacoes;

        /// <summary>0..1 enquanto está pensando ou revistando (para barras de progresso).</summary>
        public float ProgressoAcao
        {
            get
            {
                if (estado == EstadoProcurador.Pensando) return Mathf.Clamp01(temporizadorInvestigacao / TEMPO_PENSANDO);
                if (estado == EstadoProcurador.Investigando) return Mathf.Clamp01(temporizadorInvestigacao / tempoInvestigacao);
                return 0f;
            }
        }

        // ------------------------- configuração -------------------------
        public void DefinirTempoInvestigacao(float segundos)
        {
            if (segundos <= 0f) throw new ArgumentOutOfRangeException(nameof(segundos));
            tempoInvestigacao = segundos;
        }

        public void ConfigurarDificuldade(float novaVelocidade, float novoTempoInvestigacao, float novoBonusPercepcao)
        {
            DefinirVelocidade(novaVelocidade);
            DefinirTempoInvestigacao(novoTempoInvestigacao);
            bonusPercepcao = Mathf.Clamp(novoBonusPercepcao, -0.5f, 0.5f);
        }

        // ------------------------- métodos do diagrama -------------------------
        /// <summary>Recebe a lista de cômodos a revistar. O <paramref name="primeiro"/> é sempre revistado antes dos outros.</summary>
        public void ReceberComodos(List<Comodo> comodos, Comodo primeiro)
        {
            if (comodos == null) throw new ArgumentNullException(nameof(comodos));
            comodosPendentes.Clear();
            foreach (var c in comodos)
                if (c != null && !comodosPendentes.Contains(c)) comodosPendentes.Add(c);
            primeiroComodo = primeiro;
        }

        /// <summary>Escolhe o próximo cômodo: o primeiro definido ou, depois dele, um sorteado.</summary>
        public Comodo SortearComodo()
        {
            if (comodosPendentes.Count == 0) return null;

            Comodo escolhido = primeiroComodo != null && comodosPendentes.Contains(primeiroComodo)
                ? primeiroComodo
                : comodosPendentes[sorteador.Next(comodosPendentes.Count)];

            comodosPendentes.Remove(escolhido);
            return escolhido;
        }

        /// <summary>Embaralha (Fisher-Yates) a ordem em que os esconderijos do cômodo serão visitados.</summary>
        public void EmbaralharEsconderijos(List<Esconderijo> esconderijos)
        {
            esconderijosAtuais.Clear();
            if (esconderijos == null) return;
            esconderijosAtuais.AddRange(esconderijos);
            for (int i = esconderijosAtuais.Count - 1; i > 0; i--)
            {
                int j = sorteador.Next(i + 1);
                (esconderijosAtuais[i], esconderijosAtuais[j]) = (esconderijosAtuais[j], esconderijosAtuais[i]);
            }
        }

        public Esconderijo SelecionarEsconderijo()
        {
            if (esconderijosAtuais.Count == 0) return null;
            var proximo = esconderijosAtuais[0];
            esconderijosAtuais.RemoveAt(0);
            return proximo;
        }

        /// <summary>Chance final de revistar: chance do esconderijo + percepção + memória.</summary>
        public float CalcularChance(Esconderijo esconderijo)
        {
            if (esconderijo == null) return 0f;
            float chance = esconderijo.ChanceInvestigacao + bonusPercepcao + VezesQueEncontrouEm(esconderijo.Id) * BONUS_MEMORIA;
            return Mathf.Clamp(chance, 0.05f, 0.95f);
        }

        /// <summary>
        /// Resolve a passagem por um esconderijo.
        /// <paramref name="vaiRevistar"/> vem do sorteio feito em <see cref="Decidir"/>.
        /// </summary>
        public ResultadoInvestigacao InvestigarEsconderijo(Esconderijo esconderijo, bool vaiRevistar)
        {
            if (esconderijo == null) throw new ArgumentNullException(nameof(esconderijo));

            bool jogadorEstaAqui = esconderijo.Ocupado;
            ResultadoInvestigacao resultado;

            if (vaiRevistar)
            {
                esconderijosInvestigados++;
                resultado = jogadorEstaAqui ? ResultadoInvestigacao.EncontrouJogador : ResultadoInvestigacao.NaoEncontrouJogador;
            }
            else
            {
                esconderijosIgnorados++;
                resultado = jogadorEstaAqui ? ResultadoInvestigacao.JogadorEscapou : ResultadoInvestigacao.NaoEncontrouJogador;
            }

            esconderijo.MarcarComoInvestigado(vaiRevistar);
            registroInvestigacoes[esconderijo.Id] = resultado;

            if (resultado == ResultadoInvestigacao.EncontrouJogador)
            {
                encontrouJogador = true;
                Lembrar(esconderijo.Id);
            }
            return resultado;
        }

        /// <summary>Monta a rota pelo corredor até o próximo cômodo. Retorna false se não há mais cômodos.</summary>
        public bool AvancarParaProximoComodo()
        {
            Comodo anterior = comodoAtual;
            Comodo proximo = SortearComodo();
            if (proximo == null) return false;

            rota.Clear();
            if (anterior != null)
            {
                rota.Enqueue(anterior.PontoPorta);
                rota.Enqueue(anterior.PontoCorredor);
                rota.Enqueue(proximo.PontoCorredor);
                rota.Enqueue(proximo.PontoPorta);
            }
            comodoAtual = proximo;
            indoParaEsconderijo = false;
            estado = EstadoProcurador.Andando;
            return true;
        }

        public void IniciarTemporizadorInvestigacao()
        {
            temporizadorInvestigacao = 0f;
        }

        /// <summary>Avança o cronômetro da ação atual. Retorna true quando o tempo da ação acabou.</summary>
        public bool AtualizarTempoInvestigacao(float tempoDecorrido)
        {
            temporizadorInvestigacao += tempoDecorrido;
            float limite = estado == EstadoProcurador.Pensando ? TEMPO_PENSANDO : tempoInvestigacao;
            return temporizadorInvestigacao >= limite;
        }

        public void Lembrar(string idEsconderijo)
        {
            memoria.TryGetValue(idEsconderijo, out int vezes);
            memoria[idEsconderijo] = vezes + 1;
        }

        public int VezesQueEncontrouEm(string idEsconderijo)
        {
            return memoria.TryGetValue(idEsconderijo, out int vezes) ? vezes : 0;
        }

        // ------------------------- ciclo da busca -------------------------
        /// <summary>Começa a busca pela porta da frente.</summary>
        public void IniciarBusca(List<Comodo> comodos, Comodo primeiro)
        {
            ReiniciarEstado();
            ReceberComodos(comodos, primeiro);
            Ativar();
            if (AvancarParaProximoComodo())
                EntrarNoComodo(comodoAtual); // o primeiro cômodo começa logo na porta da frente
        }

        /// <summary>Chamado a cada quadro pelo GerenciadorJogo (máquina de estados).</summary>
        public void Atualizar(float tempoDecorrido)
        {
            if (!ativo || tempoDecorrido <= 0f) return;

            switch (estado)
            {
                case EstadoProcurador.Andando:
                    if (rota.Count == 0) { AoChegarNoDestino(); break; }
                    Vector2 alvo = rota.Peek();
                    Vector2 delta = alvo - posicao;
                    if (delta.sqrMagnitude > 0.0001f) direcao = delta.normalized;
                    if (MoverEmDirecao(alvo, tempoDecorrido))
                    {
                        rota.Dequeue();
                        if (rota.Count == 0) AoChegarNoDestino();
                    }
                    break;

                case EstadoProcurador.Pensando:
                    if (AtualizarTempoInvestigacao(tempoDecorrido)) Decidir();
                    break;

                case EstadoProcurador.Investigando:
                    if (AtualizarTempoInvestigacao(tempoDecorrido))
                        Registrar(InvestigarEsconderijo(esconderijoAtual, true));
                    break;
            }
        }

        private void AoChegarNoDestino()
        {
            if (indoParaEsconderijo)
            {
                estado = EstadoProcurador.Pensando;
                IniciarTemporizadorInvestigacao();
                ultimaChance = CalcularChance(esconderijoAtual);
                Falar(ObterFala("pensando"));
                ChegouAoEsconderijo?.Invoke(esconderijoAtual, ultimaChance);
            }
            else
            {
                EntrarNoComodo(comodoAtual);
            }
        }

        private void EntrarNoComodo(Comodo comodo)
        {
            comodo.RegistrarVisita(this);
            EmbaralharEsconderijos(comodo.ObterEsconderijos());
            Falar(ObterFala("entrou"));
            EntrouNoComodo?.Invoke(comodo);
            ProximoPasso();
        }

        private void Decidir()
        {
            ultimoSorteio = (float)sorteador.NextDouble();
            bool vaiRevistar = ultimoSorteio < ultimaChance;
            DecidiuInvestigar?.Invoke(esconderijoAtual, ultimaChance, ultimoSorteio, vaiRevistar);

            if (vaiRevistar)
            {
                estado = EstadoProcurador.Investigando;
                IniciarTemporizadorInvestigacao();
                Falar(ObterFala("revistando"));
            }
            else
            {
                Registrar(InvestigarEsconderijo(esconderijoAtual, false));
            }
        }

        private void Registrar(ResultadoInvestigacao resultado)
        {
            switch (resultado)
            {
                case ResultadoInvestigacao.EncontrouJogador: Falar(ObterFala("achou")); break;
                case ResultadoInvestigacao.JogadorEscapou: Falar(ObterFala("ignorou")); break;
                default: Falar(ObterFala(esconderijoAtual.Investigado ? "vazio" : "ignorou")); break;
            }

            Investigou?.Invoke(esconderijoAtual, resultado);

            if (resultado == ResultadoInvestigacao.EncontrouJogador)
            {
                estado = EstadoProcurador.Concluido;
                Desativar();
                return;
            }
            ProximoPasso();
        }

        private void ProximoPasso()
        {
            esconderijoAtual = SelecionarEsconderijo();
            if (esconderijoAtual != null)
            {
                rota.Clear();
                rota.Enqueue(esconderijoAtual.PosicaoPlanta);
                indoParaEsconderijo = true;
                estado = EstadoProcurador.Andando;
                return;
            }

            // acabaram os esconderijos deste cômodo
            if (comodoAtual != null)
            {
                comodoAtual.DefinirInvestigado(true);
                ComodoConcluido?.Invoke(comodoAtual);
            }

            if (!AvancarParaProximoComodo())
            {
                estado = EstadoProcurador.Concluido;
                Falar(ObterFala("desistiu"));
                Desativar();
                BuscaConcluida?.Invoke();
            }
        }

        private void Falar(string texto)
        {
            falaAtual = texto;
            Falou?.Invoke(texto);
        }

        /// <summary>Limpa a partida atual. A MEMÓRIA não é apagada.</summary>
        public void ReiniciarEstado()
        {
            estado = EstadoProcurador.Parado;
            comodosPendentes.Clear();
            esconderijosAtuais.Clear();
            rota.Clear();
            registroInvestigacoes.Clear();
            primeiroComodo = null;
            esconderijoAtual = null;
            comodoAtual = null;
            temporizadorInvestigacao = 0f;
            esconderijosInvestigados = 0;
            esconderijosIgnorados = 0;
            encontrouJogador = false;
            ultimaChance = 0f;
            ultimoSorteio = 0f;
            indoParaEsconderijo = false;
            falaAtual = string.Empty;
            direcao = Vector2.down;
            posicao = posicaoInicial;
            Desativar();
        }

        // ------------------------- polimorfismo -------------------------
        private static readonly Dictionary<string, string[]> falas = new Dictionary<string, string[]>
        {
            { "entrou",     new[] { "Vamos ver esse cômodo...", "Pronto ou não, lá vou eu!", "Sei que você tá por aqui..." } },
            { "pensando",   new[] { "Hmm...", "Será?", "Deixa eu pensar..." } },
            { "revistando", new[] { "Vou olhar aqui!", "Aha!", "Te peguei?" } },
            { "vazio",      new[] { "Nada aqui.", "Vazio...", "Não tá aqui." } },
            { "ignorou",    new[] { "Nah, aqui não.", "Muito óbvio...", "Ninguém caberia aí." } },
            { "achou",      new[] { "TE ACHEI!!", "ACHEI VOCÊ!" } },
            { "desistiu",   new[] { "Eu desisto! Onde você tá?!" } },
        };

        public override string ObterFala(string contexto)
        {
            if (!falas.TryGetValue(contexto, out string[] opcoes)) return "...";
            return opcoes[sorteador.Next(opcoes.Length)];
        }

        public override string Descrever()
        {
            string onde = comodoAtual != null ? comodoAtual.Nome : "porta da frente";
            return $"{nome} ({estado}) em {onde}: {esconderijosInvestigados} revistados, {esconderijosIgnorados} ignorados.";
        }
    }
}
