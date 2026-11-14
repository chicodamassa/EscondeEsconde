using System;
using UnityEngine;

namespace EscondeEsconde.Modelo
{
    /// <summary>
    /// Um lugar da casa onde o jogador pode se esconder.
    /// Cada esconderijo tem uma chance fixa de ser revistado pelo procurador:
    /// quanto menos evidente, menor a chance.
    /// </summary>
    public class Esconderijo
    {
        // ------------------------- obrigatórios (construtor) -------------------------
        private readonly string id;
        private readonly string nome;
        private float chanceInvestigacao;      // 0..1
        private readonly Rect areaLateral;     // área clicável na imagem lateral (0..1, origem no canto superior esquerdo)
        private readonly Vector2 posicaoPlanta; // ponto (em pixels) onde o procurador para na planta da casa

        // ------------------------- estado -------------------------
        private string descricao;
        private Comodo comodo;          // cômodo dono deste esconderijo
        private bool investigado;       // o procurador revistou de fato
        private bool foiInvestigado;    // o procurador já passou por aqui (revistando ou não)
        private bool ocupado;
        private Jogador ocupante;
        private int vezesUsado;

        public Esconderijo(string id, string nome, float chanceInvestigacao, Rect areaLateral, Vector2 posicaoPlanta, string descricao = "")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Esconderijo sem id.", nameof(id));
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Esconderijo sem nome.", nameof(nome));
            if (areaLateral.width <= 0f || areaLateral.height <= 0f)
                throw new ArgumentException("A área lateral precisa ter tamanho.", nameof(areaLateral));

            this.id = id;
            this.nome = nome;
            this.areaLateral = areaLateral;
            this.posicaoPlanta = posicaoPlanta;
            DefinirChanceInvestigacao(chanceInvestigacao);
            DefinirDescricao(descricao);
            ReiniciarEstado();
        }

        // ------------------------- propriedades -------------------------
        public string Id => id;
        public string Nome => nome;
        public float ChanceInvestigacao => chanceInvestigacao;
        public Rect AreaLateral => areaLateral;
        public Vector2 PosicaoPlanta => posicaoPlanta;
        public string Descricao => descricao;
        public Comodo Comodo => comodo;
        public bool Investigado => investigado;
        public bool FoiInvestigado => foiInvestigado;
        public bool Ocupado => ocupado;
        public Jogador Ocupante => ocupante;
        public int VezesUsado => vezesUsado;

        // ------------------------- métodos -------------------------
        public void DefinirChanceInvestigacao(float chance)
        {
            chanceInvestigacao = Mathf.Clamp01(chance);
        }

        public void DefinirDescricao(string novaDescricao)
        {
            descricao = novaDescricao ?? string.Empty;
        }

        /// <summary>Chamado pelo <see cref="Comodo"/> quando o esconderijo é adicionado a ele.</summary>
        public void DefinirComodo(Comodo novoComodo)
        {
            comodo = novoComodo;
        }

        /// <summary>
        /// Marca que o procurador passou por aqui.
        /// <paramref name="resultado"/> = true se ele revistou, false se passou direto.
        /// </summary>
        public void MarcarComoInvestigado(bool resultado)
        {
            foiInvestigado = true;
            investigado = resultado;
        }

        public bool Ocupar(Jogador jogador)
        {
            if (jogador == null || ocupado) return false;
            ocupado = true;
            ocupante = jogador;
            return true;
        }

        public void Desocupar()
        {
            ocupado = false;
            ocupante = null;
        }

        public void RegistrarUso()
        {
            vezesUsado++;
        }

        /// <summary>Classifica o risco somando um bônus (ex.: percepção do procurador).</summary>
        public NivelRisco ObterNivelRisco(float bonus)
        {
            float chance = Mathf.Clamp01(chanceInvestigacao + bonus);
            if (chance >= 0.6f) return NivelRisco.Alto;
            if (chance >= 0.4f) return NivelRisco.Medio;
            return NivelRisco.Baixo;
        }

        /// <summary>Volta ao estado inicial de uma partida (não apaga o contador de usos).</summary>
        public void ReiniciarEstado()
        {
            investigado = false;
            foiInvestigado = false;
            Desocupar();
        }

        public override string ToString() => $"{nome} ({Mathf.RoundToInt(chanceInvestigacao * 100)}%)";
    }
}
