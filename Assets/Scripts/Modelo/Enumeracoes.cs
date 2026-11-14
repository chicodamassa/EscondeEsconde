namespace EscondeEsconde.Modelo
{
    /// <summary>Fases de uma partida.</summary>
    public enum FaseJogo
    {
        Preparacao,   // jogador procurando um esconderijo (visão lateral)
        Investigacao, // procurador revistando a casa (visão de cima)
        Encerrado     // partida terminou (vitória ou derrota)
    }

    /// <summary>O que aconteceu quando o procurador passou por um esconderijo.</summary>
    public enum ResultadoInvestigacao
    {
        EncontrouJogador,   // revistou o esconderijo do jogador -> derrota
        JogadorEscapou,     // passou direto pelo esconderijo do jogador
        NaoEncontrouJogador // esconderijo vazio
    }

    /// <summary>Estados da "máquina de estados" do procurador.</summary>
    public enum EstadoProcurador
    {
        Parado,
        Andando,
        Pensando,     // decidindo se vai revistar o esconderijo
        Investigando, // revistando o esconderijo
        Concluido
    }

    /// <summary>Classificação do esconderijo a partir da chance de ser revistado.</summary>
    public enum NivelRisco
    {
        Baixo,
        Medio,
        Alto
    }

    public enum Dificuldade
    {
        Facil,
        Normal,
        Dificil
    }
}
