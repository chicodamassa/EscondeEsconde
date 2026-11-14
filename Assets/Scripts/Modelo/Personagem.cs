using System;
using UnityEngine;

namespace EscondeEsconde.Modelo
{
    /// <summary>
    /// CLASSE MÃE (abstrata). Reúne o que todo personagem do jogo tem em comum.
    /// Não pode ser instanciada diretamente: quem existe no jogo são as classes
    /// filhas <see cref="Jogador"/> e <see cref="Procurador"/>.
    /// </summary>
    public abstract class Personagem
    {
        // ------------------------------------------------------------------
        // Atributos protegidos (#): visíveis nas classes filhas.
        // ------------------------------------------------------------------
        protected string nome;          // obrigatório
        protected float velocidade;     // obrigatório
        protected Color cor;            // obrigatório
        protected bool ativo;
        protected Vector2 posicao;
        protected Comodo comodoAtual;

        /// <summary>Construtor chamado pelas classes filhas via <c>base(...)</c>.</summary>
        protected Personagem(string nome, float velocidade, Color cor)
        {
            Renomear(nome);
            DefinirVelocidade(velocidade);
            this.cor = cor;
            ativo = false;
            posicao = Vector2.zero;
        }

        // ------------------------------------------------------------------
        // Propriedades (somente leitura para quem está de fora)
        // ------------------------------------------------------------------
        public string Nome => nome;
        public float Velocidade => velocidade;
        public Color Cor => cor;
        public bool Ativo => ativo;
        public Vector2 Posicao => posicao;
        public Comodo ComodoAtual => comodoAtual;

        // ------------------------------------------------------------------
        // Métodos comuns a todos os personagens
        // ------------------------------------------------------------------
        public virtual void Ativar()
        {
            ativo = true;
        }

        public virtual void Desativar()
        {
            ativo = false;
        }

        public void Renomear(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new ArgumentException("O personagem precisa de um nome.", nameof(novoNome));
            nome = novoNome.Trim();
        }

        public void DefinirVelocidade(float novaVelocidade)
        {
            if (novaVelocidade <= 0f)
                throw new ArgumentOutOfRangeException(nameof(novaVelocidade), "A velocidade deve ser maior que zero.");
            velocidade = novaVelocidade;
        }

        public void DefinirPosicao(Vector2 novaPosicao)
        {
            posicao = novaPosicao;
        }

        /// <summary>
        /// Anda em linha reta até o destino, respeitando a velocidade do personagem.
        /// Retorna <c>true</c> quando chega.
        /// </summary>
        public bool MoverEmDirecao(Vector2 destino, float tempoDecorrido)
        {
            posicao = Vector2.MoveTowards(posicao, destino, velocidade * tempoDecorrido);
            return (posicao - destino).sqrMagnitude < 0.01f;
        }

        // ------------------------------------------------------------------
        // Métodos abstratos: cada classe filha é OBRIGADA a implementar
        // do seu jeito (polimorfismo).
        // ------------------------------------------------------------------
        public abstract string ObterFala(string contexto);

        public abstract string Descrever();

        public override string ToString() => Descrever();
    }
}
