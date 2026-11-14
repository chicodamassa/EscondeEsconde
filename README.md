# Esconde-Esconde

Jogo 2D de esconde-esconde feito na Unity 6 para o Projeto Final de Programação Orientada a Objetos.

- **Fase de esconder** (visão lateral): ande pelos 5 cômodos e escolha um dos 15 esconderijos antes do tempo acabar.
- **Fase de busca** (planta vista de cima): o Procurador passa por todos os esconderijos e sorteia se vai revistar cada um.

## Como rodar
- **Executável:** `Build/EscondeEsconde.exe` (copie a pasta `Build` inteira).
- **Unity:** abra a pasta no Unity Hub (Unity 6000.6.0f1), a cena `Assets/Scenes/Jogo.unity` abre sozinha → Play.
  Menu **Esconde-Esconde** → *Montar cena* / *Gerar executável*.

## Estrutura
```
Assets/
  Resources/Arte/        artes (cômodos, planta, personagens, ícones)
  Scripts/Modelo/        regras do jogo (classes POO puras)
    Personagem.cs        classe mãe abstrata
    Jogador.cs           classe filha
    Procurador.cs        classe filha
    Comodo.cs, Esconderijo.cs, GerenciadorJogo.cs, MontadorCasa.cs, Enumeracoes.cs
  Scripts/Interface/     telas Unity (MonoBehaviour), sons e helpers de UI
  Scripts/Editor/        importação das artes, montagem da cena e build
Documentacao/            briefing, UML, capturas e instruções (DOCUMENTACAO.md)
```

Documentação completa (briefing, UML, requisitos, instalação): [Documentacao/DOCUMENTACAO.md](Documentacao/DOCUMENTACAO.md)
