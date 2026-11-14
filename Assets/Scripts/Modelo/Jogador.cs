using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscondeEsconde.Modelo
{
    /// <summary>
    /// CLASSE FILHA de <see cref="Personagem"/>: a pessoa que se esconde.
    /// Herda nome, velocidade, cor, posição e cômodo atual, e acrescenta
    /// tudo o que é próprio de quem se esconde.
    /// </summary>
    public class Jogador : Personagem
    {
        // ------------------------- obrigatórios (construtor) -------------------------
        // (+ nome, velocidade e cor, herdados de Personagem)
        private readonly Comodo comodoInicial;
        private readonly string icone;          // caminho do retrato em Resources

        // ------------------------- estado -------------------------
        private Esconderijo esconderijoSelecionado; // clicado, aguardando confirmação
        private Esconderijo esconderijoEscolhido;   // onde está escondido
        private bool escondido;
        private bool capturado;
        private int totalMovimentos;
        private int trocasDeEsconderijo;
        private float tempoEscondido;
        private readonly List<string> comodosVisitados = new List<string>();
        // Dicionário: id do esconderijo -> quantas partidas o jogador terminou escondido nele
        private readonly Dictionary<string, int> historicoEsconderijos = new Dictionary<string, int>();

        /// <param name="velocidade">Rapidez para trocar de cômodo (quanto maior, mais rápida a transição).</param>
        public Jogador(string nome, float velocidade, Color cor, Comodo comodoInicial, string icone)
            : base(nome, velocidade, cor)
        {
            this.comodoInicial = comodoInicial ?? throw new ArgumentNullException(nameof(comodoInicial));
            if (string.IsNullOrWhiteSpace(icone)) throw new ArgumentException("O jogador precisa de um ícone.", nameof(icone));
            this.icone = icone;
            ReiniciarParaNovaPartida(comodoInicial);
        }

        // ------------------------- propriedades -------------------------
        public Comodo ComodoInicial => comodoInicial;
        public string Icone => icone;
        public Esconderijo EsconderijoSelecionado => esconderijoSelecionado;
        public bool Escondido => escondido;
        public bool Capturado => capturado;
        public int TotalMovimentos => totalMovimentos;
        public int TrocasDeEsconderijo => trocasDeEsconderijo;
        public float TempoEscondido => tempoEscondido;
        public IReadOnlyList<string> ComodosVisitados => comodosVisitados;
        public IReadOnlyDictionary<string, int> HistoricoEsconderijos => historicoEsconderijos;

        // ------------------------- métodos -------------------------
        public bool MoverParaComodo(Comodo destino)
        {
            if (destino == null || escondido || destino == comodoAtual) return false;

            CancelarSelecaoEsconderijo();
            comodoAtual = destino;
            totalMovimentos++;
            if (!comodosVisitados.Contains(destino.Id)) comodosVisitados.Add(destino.Id);
            destino.RegistrarVisita(this);
            return true;
        }

        public bool SelecionarEsconderijo(Esconderijo esconderijo)
        {
            if (escondido || esconderijo == null || esconderijo.Comodo != comodoAtual) return false;
            esconderijoSelecionado = esconderijo;
            return true;
        }

        /// <summary>Entra no esconderijo selecionado. Só funciona enquanto o tempo de preparação está correndo.</summary>
        public bool ConfirmarEsconderijo(bool tempoPreparacaoAtivo)
        {
            if (!tempoPreparacaoAtivo || escondido || esconderijoSelecionado == null) return false;
            if (!esconderijoSelecionado.Ocupar(this)) return false;

            esconderijoEscolhido = esconderijoSelecionado;
            esconderijoSelecionado = null;
            escondido = true;
            posicao = esconderijoEscolhido.PosicaoPlanta;
            return true;
        }

        public void CancelarSelecaoEsconderijo()
        {
            esconderijoSelecionado = null;
        }

        public bool SairDoEsconderijo()
        {
            if (!escondido) return false;
            esconderijoEscolhido.Desocupar();
            esconderijoEscolhido = null;
            escondido = false;
            trocasDeEsconderijo++;
            return true;
        }

        public Esconderijo ObterEsconderijoEscolhido()
        {
            return esconderijoEscolhido;
        }

        public void AtualizarTempoEscondido(float tempoDecorrido)
        {
            if (escondido && tempoDecorrido > 0f) tempoEscondido += tempoDecorrido;
        }

        public void MarcarCapturado(bool foiCapturado)
        {
            capturado = foiCapturado;
            if (foiCapturado) Desativar();
        }

        /// <summary>Guarda no histórico (dicionário) o esconderijo em que a busca começou.</summary>
        public void RegistrarUsoEsconderijo(Esconderijo esconderijo)
        {
            if (esconderijo == null) return;
            historicoEsconderijos.TryGetValue(esconderijo.Id, out int vezes);
            historicoEsconderijos[esconderijo.Id] = vezes + 1;
            esconderijo.RegistrarUso();
        }

        public int VezesQueUsou(string idEsconderijo)
        {
            return historicoEsconderijos.TryGetValue(idEsconderijo, out int vezes) ? vezes : 0;
        }

        /// <summary>Limpa o estado da partida (mantém o histórico entre partidas).</summary>
        public void ReiniciarParaNovaPartida(Comodo inicio)
        {
            esconderijoEscolhido?.Desocupar();
            esconderijoSelecionado = null;
            esconderijoEscolhido = null;
            escondido = false;
            capturado = false;
            totalMovimentos = 0;
            trocasDeEsconderijo = 0;
            tempoEscondido = 0f;

            comodoAtual = inicio ?? comodoInicial;
            comodosVisitados.Clear();
            comodosVisitados.Add(comodoAtual.Id);
            comodoAtual.RegistrarVisita(this);
            Ativar();
        }

        // ------------------------- polimorfismo -------------------------
        public override string ObterFala(string contexto)
        {
            switch (contexto)
            {
                case "escondido": return "Shhh... ninguém vai me achar aqui!";
                case "pouco_tempo": return "Rápido, rápido, rápido!";
                case "encontrado": return "Ah não! Me achou!";
                case "escapou": return "Ufa... passou direto!";
                case "vitoria": return "Ninguém me acha! Sou o mestre do esconde-esconde!";
                case "sem_esconderijo": return "Ainda não me escondi!";
                default: return "...";
            }
        }

        public override string Descrever()
        {
            string onde = escondido ? $"escondido em \"{esconderijoEscolhido.Nome}\"" : $"andando pelo cômodo {comodoAtual?.Nome}";
            return $"{nome} está {onde}.";
        }
    }
}
