using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscondeEsconde.Modelo
{
    /// <summary>
    /// Regras da partida. Liga o <see cref="Jogador"/>, o <see cref="Procurador"/> e os
    /// <see cref="Comodo"/>s (guardados em um DICIONÁRIO, acessados pela chave/id).
    /// Não depende de nada visual: a interface apenas chama os métodos e escuta os eventos.
    /// </summary>
    public class GerenciadorJogo
    {
        /// <summary>Parâmetros de cada dificuldade.</summary>
        public readonly struct ParametrosDificuldade
        {
            public readonly float TempoPreparacao;
            public readonly float VelocidadeProcurador;
            public readonly float TempoInvestigacao;
            public readonly float BonusPercepcao;

            public ParametrosDificuldade(float tempoPreparacao, float velocidadeProcurador, float tempoInvestigacao, float bonusPercepcao)
            {
                TempoPreparacao = tempoPreparacao;
                VelocidadeProcurador = velocidadeProcurador;
                TempoInvestigacao = tempoInvestigacao;
                BonusPercepcao = bonusPercepcao;
            }
        }

        public static ParametrosDificuldade ObterParametros(Dificuldade dificuldade)
        {
            switch (dificuldade)
            {
                case Dificuldade.Facil: return new ParametrosDificuldade(45f, 230f, 1.6f, -0.10f);
                case Dificuldade.Dificil: return new ParametrosDificuldade(20f, 320f, 1.1f, 0.12f);
                default: return new ParametrosDificuldade(30f, 270f, 1.35f, 0f);
            }
        }

        // ------------------------- obrigatórios (construtor) -------------------------
        private readonly Dictionary<string, Comodo> comodos;   // chave = id do cômodo
        private readonly List<string> ordemComodos;            // ordem das setas na fase de esconder
        private readonly string chaveComodoInicial;
        private float tempoPreparacao;
        private Dificuldade dificuldade;

        // ------------------------- estado -------------------------
        private bool jogoIniciado;
        private FaseJogo faseAtual;
        private float tempoRestante;
        private bool venceu;
        private bool derrotado;
        private bool jogadorEscapou;
        private Jogador jogador;
        private Procurador procurador;
        private string motivoFim;
        private int partidasJogadas;
        private int vitorias;
        private float multiplicadorVelocidade;

        // ------------------------- eventos -------------------------
        public event Action<FaseJogo> FaseAlterada;
        public event Action<Comodo> JogadorMudouDeComodo;

        public GerenciadorJogo(Dictionary<string, Comodo> comodos, List<string> ordemComodos, string chaveComodoInicial, float tempoPreparacao, Dificuldade dificuldade)
        {
            if (comodos == null || comodos.Count == 0) throw new ArgumentException("A casa precisa de cômodos.", nameof(comodos));
            if (ordemComodos == null || ordemComodos.Count == 0) throw new ArgumentException("Informe a ordem dos cômodos.", nameof(ordemComodos));
            if (!comodos.ContainsKey(chaveComodoInicial)) throw new ArgumentException($"Cômodo inicial '{chaveComodoInicial}' não existe.", nameof(chaveComodoInicial));
            foreach (var chave in ordemComodos)
                if (!comodos.ContainsKey(chave)) throw new ArgumentException($"Cômodo '{chave}' não existe.", nameof(ordemComodos));

            this.comodos = comodos;
            this.ordemComodos = new List<string>(ordemComodos);
            this.chaveComodoInicial = chaveComodoInicial;
            ConfigurarDificuldade(dificuldade, tempoPreparacao);
            faseAtual = FaseJogo.Encerrado;
            multiplicadorVelocidade = 1f;
            motivoFim = string.Empty;
        }

        // ------------------------- propriedades -------------------------
        public bool JogoIniciado => jogoIniciado;
        public FaseJogo FaseAtual => faseAtual;
        public float TempoPreparacao => tempoPreparacao;
        public float TempoRestante => tempoRestante;
        public bool Venceu => venceu;
        public bool Derrotado => derrotado;
        public bool JogadorEscapou => jogadorEscapou;
        public Jogador Jogador => jogador;
        public Procurador Procurador => procurador;
        public string MotivoFim => motivoFim;
        public int PartidasJogadas => partidasJogadas;
        public int Vitorias => vitorias;
        public float MultiplicadorVelocidade => multiplicadorVelocidade;
        public Dificuldade Dificuldade => dificuldade;
        public IReadOnlyList<string> OrdemComodos => ordemComodos;
        public IEnumerable<Comodo> TodosOsComodos => comodos.Values;

        // ------------------------- métodos -------------------------
        public void ConfigurarDificuldade(Dificuldade novaDificuldade, float novoTempoPreparacao)
        {
            if (novoTempoPreparacao <= 0f) throw new ArgumentOutOfRangeException(nameof(novoTempoPreparacao));
            dificuldade = novaDificuldade;
            tempoPreparacao = novoTempoPreparacao;
        }

        public void IniciarPartida(Jogador novoJogador, Procurador novoProcurador)
        {
            if (novoJogador == null) throw new ArgumentNullException(nameof(novoJogador));
            if (novoProcurador == null) throw new ArgumentNullException(nameof(novoProcurador));

            if (procurador != novoProcurador)
            {
                if (procurador != null)
                {
                    procurador.Investigou -= AoInvestigar;
                    procurador.BuscaConcluida -= AoConcluirBusca;
                }
                novoProcurador.Investigou += AoInvestigar;
                novoProcurador.BuscaConcluida += AoConcluirBusca;
            }

            jogador = novoJogador;
            procurador = novoProcurador;

            foreach (var comodo in comodos.Values) comodo.ReiniciarEstado();
            jogador.ReiniciarParaNovaPartida(ObterComodo(chaveComodoInicial));
            procurador.ReiniciarEstado();

            tempoRestante = tempoPreparacao;
            venceu = false;
            derrotado = false;
            jogadorEscapou = false;
            motivoFim = string.Empty;
            multiplicadorVelocidade = 1f;
            jogoIniciado = true;
            partidasJogadas++;
            MudarFase(FaseJogo.Preparacao);
        }

        /// <summary>Busca um cômodo no dicionário pela chave (id).</summary>
        public Comodo ObterComodo(string nome)
        {
            if (string.IsNullOrEmpty(nome)) return null;
            return comodos.TryGetValue(nome, out Comodo comodo) ? comodo : null;
        }

        /// <summary>Cômodo ao lado na ordem das setas (direcao = -1 esquerda, +1 direita).</summary>
        public Comodo ObterComodoVizinho(Comodo atual, int direcao)
        {
            int indice = atual != null ? ordemComodos.IndexOf(atual.Id) : 0;
            if (indice < 0) indice = 0;
            int n = ordemComodos.Count;
            int vizinho = ((indice + Math.Sign(direcao)) % n + n) % n;
            return ObterComodo(ordemComodos[vizinho]);
        }

        public bool MoverJogadorPara(string chaveComodo)
        {
            if (faseAtual != FaseJogo.Preparacao || jogador == null) return false;
            Comodo destino = ObterComodo(chaveComodo);
            if (!jogador.MoverParaComodo(destino)) return false;
            JogadorMudouDeComodo?.Invoke(destino);
            return true;
        }

        public bool EsconderJogador(Esconderijo esconderijo)
        {
            if (faseAtual != FaseJogo.Preparacao || jogador == null) return false;
            return jogador.SelecionarEsconderijo(esconderijo) && jogador.ConfirmarEsconderijo(tempoRestante > 0f);
        }

        /// <summary>Desconta o tempo da fase de esconder. Quando zera, decide o que acontece.</summary>
        public void AtualizarTempoPreparacao(float tempoDecorrido)
        {
            if (faseAtual != FaseJogo.Preparacao) return;

            tempoRestante = Mathf.Max(0f, tempoRestante - tempoDecorrido);
            jogador.AtualizarTempoEscondido(tempoDecorrido);

            if (tempoRestante <= 0f)
            {
                if (jogador.Escondido) IniciarInvestigacao();
                else FimDeJogoDerrota("O tempo acabou e você não estava escondido!");
            }
        }

        /// <summary>Botão "acelerar o tempo": só funciona se o jogador já estiver escondido.</summary>
        public bool PularPreparacao()
        {
            if (faseAtual != FaseJogo.Preparacao || jogador == null || !jogador.Escondido) return false;
            tempoRestante = 0f;
            IniciarInvestigacao();
            return true;
        }

        public void IniciarInvestigacao()
        {
            if (faseAtual != FaseJogo.Preparacao) return;
            jogador.RegistrarUsoEsconderijo(jogador.ObterEsconderijoEscolhido());
            MudarFase(FaseJogo.Investigacao);

            var lista = new List<Comodo>();
            foreach (var chave in ordemComodos) lista.Add(comodos[chave]);
            procurador.IniciarBusca(lista, ObterComodo(chaveComodoInicial));
        }

        /// <summary>Chamado a cada quadro pela interface.</summary>
        public void Atualizar(float tempoDecorrido)
        {
            if (!jogoIniciado) return;
            if (faseAtual == FaseJogo.Preparacao) AtualizarTempoPreparacao(tempoDecorrido);
            else if (faseAtual == FaseJogo.Investigacao) procurador.Atualizar(tempoDecorrido * multiplicadorVelocidade);
        }

        public void DefinirMultiplicadorVelocidade(float multiplicador)
        {
            multiplicadorVelocidade = Mathf.Clamp(multiplicador, 0.25f, 8f);
        }

        public void FimDeJogoVitoria()
        {
            if (faseAtual == FaseJogo.Encerrado) return;
            venceu = true;
            derrotado = false;
            vitorias++;
            motivoFim = $"{procurador.Nome} revistou a casa inteira e não te encontrou!";
            jogoIniciado = false;
            MudarFase(FaseJogo.Encerrado);
        }

        public void FimDeJogoDerrota(string motivo)
        {
            if (faseAtual == FaseJogo.Encerrado) return;
            venceu = false;
            derrotado = true;
            motivoFim = motivo;
            jogoIniciado = false;
            MudarFase(FaseJogo.Encerrado);
        }

        public void ReiniciarPartida()
        {
            IniciarPartida(jogador, procurador);
        }

        public float CalcularTaxaVitoria()
        {
            return partidasJogadas == 0 ? 0f : (float)vitorias / partidasJogadas;
        }

        // ------------------------- eventos do procurador -------------------------
        private void AoInvestigar(Esconderijo esconderijo, ResultadoInvestigacao resultado)
        {
            if (resultado == ResultadoInvestigacao.EncontrouJogador)
            {
                jogador.MarcarCapturado(true);
                FimDeJogoDerrota($"{procurador.Nome} te encontrou: {esconderijo.Nome.ToLowerInvariant()} ({esconderijo.Comodo.Nome}).");
            }
            else if (resultado == ResultadoInvestigacao.JogadorEscapou)
            {
                jogadorEscapou = true;
            }
        }

        private void AoConcluirBusca()
        {
            if (faseAtual == FaseJogo.Investigacao) FimDeJogoVitoria();
        }

        private void MudarFase(FaseJogo novaFase)
        {
            faseAtual = novaFase;
            FaseAlterada?.Invoke(novaFase);
        }
    }
}
