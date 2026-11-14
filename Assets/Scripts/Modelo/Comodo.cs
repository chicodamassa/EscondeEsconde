using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscondeEsconde.Modelo
{
    /// <summary>
    /// Um cômodo da casa. É composto por vários <see cref="Esconderijo"/>s (composição).
    /// Guarda também as informações para desenhá-lo nas duas visões do jogo
    /// (imagem lateral e área na planta vista de cima).
    /// </summary>
    public class Comodo
    {
        // ------------------------- obrigatórios (construtor) -------------------------
        private readonly string id;
        private readonly string nome;
        private readonly string imagemLateral;   // caminho do sprite em Resources
        private readonly Rect areaPlanta;        // retângulo (pixels) do cômodo na planta
        private readonly Vector2 pontoPorta;     // porta do cômodo na planta
        private readonly Vector2 pontoCorredor;  // ponto do corredor em frente à porta

        // ------------------------- estado -------------------------
        private string descricao;
        private Color corDestaque;
        private readonly List<Esconderijo> listaEsconderijos = new List<Esconderijo>();
        private bool investigado;
        private int visitasDoJogador;
        private int visitasDoProcurador;

        public Comodo(string id, string nome, string imagemLateral, Rect areaPlanta, Vector2 pontoPorta, Vector2 pontoCorredor)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Cômodo sem id.", nameof(id));
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Cômodo sem nome.", nameof(nome));
            if (string.IsNullOrWhiteSpace(imagemLateral)) throw new ArgumentException("Cômodo sem imagem.", nameof(imagemLateral));

            this.id = id;
            this.nome = nome;
            this.imagemLateral = imagemLateral;
            this.areaPlanta = areaPlanta;
            this.pontoPorta = pontoPorta;
            this.pontoCorredor = pontoCorredor;
            descricao = string.Empty;
            corDestaque = new Color(1f, 0.85f, 0.35f);
        }

        // ------------------------- propriedades -------------------------
        public string Id => id;
        public string Nome => nome;
        public string ImagemLateral => imagemLateral;
        public Rect AreaPlanta => areaPlanta;
        public Vector2 PontoPorta => pontoPorta;
        public Vector2 PontoCorredor => pontoCorredor;
        public string Descricao => descricao;
        public Color CorDestaque => corDestaque;
        public bool Investigado => investigado;
        public int VisitasDoJogador => visitasDoJogador;
        public int VisitasDoProcurador => visitasDoProcurador;
        public int QuantidadeEsconderijos => listaEsconderijos.Count;

        // ------------------------- métodos -------------------------
        public void AdicionarEsconderijo(Esconderijo esconderijo)
        {
            if (esconderijo == null) throw new ArgumentNullException(nameof(esconderijo));
            if (listaEsconderijos.Contains(esconderijo)) return;
            listaEsconderijos.Add(esconderijo);
            esconderijo.DefinirComodo(this);
        }

        public bool RemoverEsconderijo(Esconderijo esconderijo)
        {
            if (esconderijo == null || !listaEsconderijos.Remove(esconderijo)) return false;
            esconderijo.DefinirComodo(null);
            return true;
        }

        /// <summary>Procura um esconderijo pelo nome ou pelo id.</summary>
        public Esconderijo ObterEsconderijo(string nomeOuId)
        {
            foreach (var e in listaEsconderijos)
            {
                if (string.Equals(e.Nome, nomeOuId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(e.Id, nomeOuId, StringComparison.OrdinalIgnoreCase))
                    return e;
            }
            return null;
        }

        /// <summary>Devolve uma CÓPIA da lista (ninguém de fora altera a lista interna).</summary>
        public List<Esconderijo> ObterEsconderijos()
        {
            return new List<Esconderijo>(listaEsconderijos);
        }

        public void DefinirInvestigado(bool estado)
        {
            investigado = estado;
        }

        public void DefinirDescricao(string novaDescricao)
        {
            descricao = novaDescricao ?? string.Empty;
        }

        public void DefinirCorDestaque(Color novaCor)
        {
            corDestaque = novaCor;
        }

        /// <summary>Conta a visita de um personagem. Usa o tipo do objeto para saber quem foi.</summary>
        public void RegistrarVisita(Personagem visitante)
        {
            if (visitante is Jogador) visitasDoJogador++;
            else if (visitante is Procurador) visitasDoProcurador++;
        }

        /// <summary>Diz se um ponto da planta está dentro deste cômodo.</summary>
        public bool ContemPonto(Vector2 pontoPlanta)
        {
            return areaPlanta.Contains(pontoPlanta);
        }

        public int ContarEsconderijosInvestigados()
        {
            int total = 0;
            foreach (var e in listaEsconderijos)
                if (e.FoiInvestigado) total++;
            return total;
        }

        public void ReiniciarEstado()
        {
            investigado = false;
            visitasDoJogador = 0;
            visitasDoProcurador = 0;
            foreach (var e in listaEsconderijos) e.ReiniciarEstado();
        }

        public override string ToString() => $"{nome} ({listaEsconderijos.Count} esconderijos)";
    }
}
