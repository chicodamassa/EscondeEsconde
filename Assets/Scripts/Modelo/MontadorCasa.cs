using System.Collections.Generic;
using UnityEngine;

namespace EscondeEsconde.Modelo
{
    /// <summary>
    /// "Fábrica" que monta a casa: cria os 5 cômodos e os 15 esconderijos
    /// com as posições medidas nas artes da equipe.
    /// Para mudar uma chance ou posição, é só alterar aqui.
    /// </summary>
    public static class MontadorCasa
    {
        // Tamanho (em pixels) da imagem da planta vista de cima.
        public const float LARGURA_PLANTA = 1589f;
        public const float ALTURA_PLANTA = 990f;

        public const string COMODO_INICIAL = "entrada";
        public static readonly Vector2 PortaDaFrente = new Vector2(715f, 110f);

        /// <summary>Ordem dos cômodos nas setas (esquerda/direita) da fase de esconder.</summary>
        public static List<string> OrdemComodos()
        {
            return new List<string> { "entrada", "sala", "cozinha", "quarto", "banheiro" };
        }

        public static Dictionary<string, Comodo> CriarComodos()
        {
            var casa = new Dictionary<string, Comodo>();

            // ---------------- ENTRADA (imagem 1671 x 941) ----------------
            var entrada = NovoComodo(casa, "entrada", "Entrada", 485, 50, 925, 390, new Vector2(725, 405), new Vector2(725, 485),
                "Onde tudo começa. O procurador vai olhar aqui primeiro!");
            Adicionar(entrada, "entrada_armario", "Dentro do armário de sapatos", 0.50f, 1671, 941, 30, 455, 460, 885, 610, 270,
                "Apertado e com cheiro de tênis... mas tem porta.");
            Adicionar(entrada, "entrada_banco", "Debaixo do banco", 0.70f, 1671, 941, 950, 535, 1325, 780, 790, 235,
                "Dá pra ver seus pés de longe. Arriscado!");
            Adicionar(entrada, "entrada_planta", "Atrás da planta grande", 0.35f, 1671, 941, 1255, 285, 1645, 880, 820, 330,
                "As folhas são grandes. Fique bem paradinho.");

            // ---------------- SALA (imagem 1670 x 941) ----------------
            var sala = NovoComodo(casa, "sala", "Sala", 215, 580, 780, 935, new Vector2(512, 562), new Vector2(512, 485),
                "Sofá fofo, TV grande e cortinas compridas.");
            Adicionar(sala, "sala_tv", "Dentro do móvel da televisão", 0.40f, 1670, 941, 1185, 480, 1650, 870, 380, 735,
                "Escuro e cheio de fios, mas cabe uma pessoa.");
            Adicionar(sala, "sala_sofa", "Dentro do sofá", 0.25f, 1670, 941, 110, 400, 625, 850, 575, 750,
                "Ninguém pensa em abrir o sofá... ou pensa?");
            Adicionar(sala, "sala_cortina", "Atrás da cortina", 0.60f, 1670, 941, 500, 105, 700, 470, 420, 860,
                "Clássico! Só não deixe os pés aparecendo.");

            // ---------------- COZINHA (imagem 1672 x 941) ----------------
            var cozinha = NovoComodo(casa, "cozinha", "Cozinha", 965, 50, 1540, 390, new Vector2(1060, 405), new Vector2(1060, 485),
                "Cheirinho de comida... e uma despensa bem escura.");
            Adicionar(cozinha, "cozinha_mesa", "Debaixo da mesa", 0.75f, 1672, 941, 565, 530, 1175, 935, 1110, 300,
                "A toalha não cobre quase nada...");
            Adicionar(cozinha, "cozinha_armario", "Dentro do armário inferior", 0.45f, 1672, 941, 318, 478, 565, 725, 1150, 215,
                "Entre panelas e potes. Não espirre!");
            Adicionar(cozinha, "cozinha_despensa", "Dentro da despensa", 0.35f, 1672, 941, 1330, 120, 1590, 640, 1410, 320,
                "Bem no fundo, atrás dos potes de biscoito.");

            // ---------------- QUARTO (imagem 1671 x 941) ----------------
            var quarto = NovoComodo(casa, "quarto", "Quarto", 820, 580, 1400, 935, new Vector2(1020, 565), new Vector2(1020, 485),
                "Cama, guarda-roupa e um baú misterioso.");
            Adicionar(quarto, "quarto_cama", "Debaixo da cama", 0.80f, 1671, 941, 160, 440, 795, 765, 1220, 830,
                "O primeiro lugar onde todo mundo olha!");
            Adicionar(quarto, "quarto_guarda_roupa", "Dentro do guarda-roupa", 0.55f, 1671, 941, 990, 75, 1400, 675, 980, 850,
                "Entre os casacos dá pra se camuflar.");
            Adicionar(quarto, "quarto_bau", "Dentro do baú", 0.30f, 1671, 941, 797, 545, 1000, 750, 1050, 745,
                "Pequeno e fechado. Quem imaginaria?");

            // ---------------- BANHEIRO (imagem 1670 x 941) ----------------
            var banheiro = NovoComodo(casa, "banheiro", "Banheiro", 55, 200, 445, 525, new Vector2(410, 465), new Vector2(470, 485),
                "Pequeno, mas cheio de cantinhos.");
            Adicionar(banheiro, "banheiro_pia", "Dentro do armário da pia", 0.50f, 1670, 941, 25, 495, 580, 910, 190, 430,
                "Cuidado com o cano gelado nas costas.");
            Adicionar(banheiro, "banheiro_porta", "Atrás da porta", 0.40f, 1670, 941, 1275, 75, 1445, 880, 370, 470,
                "Se ninguém fechar a porta, você está salvo.");
            Adicionar(banheiro, "banheiro_box", "Atrás da cortina do box", 0.65f, 1670, 941, 830, 60, 1180, 720, 315, 410,
                "Todo filme de terror começa assim...");

            return casa;
        }

        private static Comodo NovoComodo(Dictionary<string, Comodo> casa, string id, string nome,
            float x1, float y1, float x2, float y2, Vector2 porta, Vector2 corredor, string descricao)
        {
            var comodo = new Comodo(id, nome, "Arte/Comodos/" + id, Rect.MinMaxRect(x1, y1, x2, y2), porta, corredor);
            comodo.DefinirDescricao(descricao);
            casa.Add(id, comodo);
            return comodo;
        }

        /// <summary>
        /// Cria um esconderijo. A área clicável é medida em pixels na imagem do cômodo
        /// (x1,y1)-(x2,y2) e convertida para 0..1, assim funciona em qualquer resolução.
        /// </summary>
        private static void Adicionar(Comodo comodo, string id, string nome, float chance,
            float larguraImagem, float alturaImagem, float x1, float y1, float x2, float y2,
            float plantaX, float plantaY, string descricao)
        {
            Rect area = Rect.MinMaxRect(x1 / larguraImagem, y1 / alturaImagem, x2 / larguraImagem, y2 / alturaImagem);
            comodo.AdicionarEsconderijo(new Esconderijo(id, nome, chance, area, new Vector2(plantaX, plantaY), descricao));
        }
    }
}
