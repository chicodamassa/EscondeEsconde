# Esconde-Esconde — Documento de Entrega (POO)

> Texto pronto para copiar para o PDF da entrega. As imagens citadas estão nesta pasta
> (`diagrama_classes.png` e `capturas/`).

---

## 1. Briefing do Jogo

### Overview geral
Um jogo 2D de esconde-esconde onde o jogador tem um tempo limitado para se esconder
dentro de uma casa antes que o **Procurador** comece a procurá-lo.

### Funcionamento
O jogo tem duas fases.

**Fase de Preparação (visão 2D lateral)**
- A partida começa com um temporizador (45 s no Fácil, 30 s no Normal, 20 s no Difícil).
- O jogador começa na **Entrada** e vê o cômodo de lado. Pelas setas (ou ← →) ele anda pela
  casa: Entrada, Sala, Cozinha, Quarto e Banheiro. Um mini-mapa mostra onde ele está.
- Cada cômodo tem **3 esconderijos fixos** (15 no total). Ao clicar em um móvel aparece uma
  janela com o nome, uma descrição e o **nível de risco**, com os botões *Esconder* e *Cancelar*.
- Já escondido, aparecem os botões **Acelerar o tempo** (vai direto para a busca) e
  **Sair do esconderijo** (para trocar de lugar enquanto ainda há tempo).
- Se o tempo acabar e o jogador **não** estiver escondido, ele perde.

**Fase de Busca (visão 2D de cima — planta da casa)**
- O jogador não age mais; ele assiste o Procurador andando pela planta da casa.
- O Procurador começa pela Entrada e depois sorteia os outros cômodos. Em cada cômodo ele
  passa por todos os esconderijos, em ordem embaralhada.
- Em cada esconderijo ele **sorteia** se vai revistar. A chance depende de quão óbvio é o
  esconderijo (ex.: *debaixo da cama* 80%, *dentro do sofá* 25%), da dificuldade e da
  **memória** do Procurador (+15% onde ele já encontrou o jogador em partidas anteriores).
- O painel lateral mostra a chance, o sorteio e um diário da busca.
- Se ele revistar o esconderijo do jogador → **derrota**. Se passar direto e terminar a casa → **vitória**.

### Objetivo do jogador
Escolher um esconderijo pouco óbvio antes de o tempo acabar e torcer para o Procurador
não revistá-lo.

### Mecânicas principais
| Mecânica | Onde está no código |
|---|---|
| Temporizador da preparação | `GerenciadorJogo.AtualizarTempoPreparacao(float)` |
| Andar entre cômodos | `GerenciadorJogo.MoverJogadorPara(string)` → `Jogador.MoverParaComodo(Comodo)` |
| Selecionar / confirmar / sair do esconderijo | `Jogador.SelecionarEsconderijo`, `ConfirmarEsconderijo`, `SairDoEsconderijo` |
| Acelerar o tempo | `GerenciadorJogo.PularPreparacao()` |
| Sorteio da investigação | `Procurador.CalcularChance(Esconderijo)` + `InvestigarEsconderijo(Esconderijo, bool)` |
| Memória do procurador | `Procurador.Lembrar(string)` / `VezesQueEncontrouEm(string)` (Dictionary) |
| Dificuldade | `GerenciadorJogo.ObterParametros(Dificuldade)` |

---

## 2. Diagrama de Classes (UML)

Ver `diagrama_classes.png`.

O código foi dividido em duas camadas:

- **Modelo** (`Assets/Scripts/Modelo`): classes C# puras, com as regras do jogo. São as classes
  avaliadas no trabalho e as que aparecem no diagrama.
- **Interface** (`Assets/Scripts/Interface`): componentes da Unity (MonoBehaviour) que desenham
  as telas, tocam os sons e repassam os cliques para o modelo.

### Herança
- `Personagem` (**classe mãe, abstrata**) → `Jogador` e `Procurador` (**classes filhas**).
  - As filhas chamam o construtor da mãe com `base(nome, velocidade, cor)`.
  - Herdam `nome`, `velocidade`, `cor`, `ativo`, `posicao`, `comodoAtual` e os métodos
    `Ativar()`, `Desativar()`, `MoverEmDirecao(...)` etc.
  - Os métodos abstratos `ObterFala(string)` e `Descrever()` são sobrescritos (`override`) em
    cada filha → **polimorfismo**.
- Na interface também há herança: `Tela` (abstrata) → `TelaMenu`, `TelaEsconder`,
  `TelaProcura`, `TelaResultado`.

### Dicionários
| Dicionário | Para que serve |
|---|---|
| `GerenciadorJogo.comodos : Dictionary<string, Comodo>` | todos os cômodos da casa, acessados pela chave (`"entrada"`, `"sala"`...) |
| `Jogador.historicoEsconderijos : Dictionary<string, int>` | quantas vezes o jogador usou cada esconderijo |
| `Procurador.memoria : Dictionary<string, int>` | onde o procurador já encontrou o jogador (aumenta a chance) |
| `Procurador.registroInvestigacoes : Dictionary<string, ResultadoInvestigacao>` | o resultado de cada esconderijo na busca atual |

### Requisitos do professor × código

| Classe concreta | Atributos | Obrigatórios (construtor) | Métodos com parâmetros |
|---|---|---|---|
| **Jogador** | 11 próprios + 6 herdados = **17** | nome, velocidade, cor, comodoInicial, icone (**5**) | MoverParaComodo, SelecionarEsconderijo, ConfirmarEsconderijo, AtualizarTempoEscondido, MarcarCapturado, RegistrarUsoEsconderijo, VezesQueUsou, ReiniciarParaNovaPartida, ObterFala (**9**) |
| **Procurador** | 21 próprios + 6 herdados = **27** | nome, velocidade, cor, tempoInvestigacao, bonusPercepcao, posicaoInicial (**6**) | ConfigurarDificuldade, DefinirTempoInvestigacao, ReceberComodos, EmbaralharEsconderijos, CalcularChance, InvestigarEsconderijo, AtualizarTempoInvestigacao, Lembrar, VezesQueEncontrouEm, IniciarBusca, Atualizar, ObterFala (**12**) |
| **Comodo** | **12** | id, nome, imagemLateral, areaPlanta, pontoPorta, pontoCorredor (**6**) | AdicionarEsconderijo, RemoverEsconderijo, ObterEsconderijo, DefinirInvestigado, DefinirDescricao, DefinirCorDestaque, RegistrarVisita, ContemPonto (**8**) |
| **Esconderijo** | **12** | id, nome, chanceInvestigacao, areaLateral, posicaoPlanta (**5**) | DefinirChanceInvestigacao, DefinirDescricao, DefinirComodo, MarcarComoInvestigado, Ocupar, ObterNivelRisco (**6**) |
| **GerenciadorJogo** | **17** | comodos, ordemComodos, chaveComodoInicial, tempoPreparacao, dificuldade (**5**) | ConfigurarDificuldade, IniciarPartida, ObterComodo, ObterComodoVizinho, MoverJogadorPara, EsconderJogador, AtualizarTempoPreparacao, Atualizar, DefinirMultiplicadorVelocidade, FimDeJogoDerrota (**10**) |

Os construtores **validam** os obrigatórios (lançam `ArgumentException` se vierem vazios ou inválidos).

### Relacionamentos
- `GerenciadorJogo` → 1 `Jogador`, 1 `Procurador` (associação).
- `GerenciadorJogo` ◆→ vários `Comodo` (composição, guardados no dicionário).
- `Comodo` ◆→ vários `Esconderijo` (composição; 3 por cômodo).
- `Esconderijo.ocupante` → `Jogador` (0..1).
- `MontadorCasa` (classe estática / "fábrica") cria os 5 cômodos e 15 esconderijos.
- O `Procurador` avisa a interface por **eventos** (`event Action<...>`): o modelo não
  conhece a interface, só a interface conhece o modelo.

---

## 3. Elementos adicionais

- **Sketches / artes:** cômodos em visão lateral e planta da casa feitos pela equipe
  (`Assets/Resources/Arte/Comodos` e `Arte/Planta`). Personagens, ícones e logo gerados para o
  projeto (`Arte/Gerada`). Capturas de tela do jogo em `capturas/`.
- **Música e sons:** gerados por código (`SomJogo.cs`) — trilha de suspense em lá menor em loop,
  tique-taque nos últimos 10 s, batida de coração quando o procurador chega no seu esconderijo,
  jingles de vitória/derrota.

| Esconderijo | Cômodo | Chance base de ser revistado |
|---|---|---|
| Dentro do armário de sapatos | Entrada | 50% |
| Debaixo do banco | Entrada | 70% |
| Atrás da planta grande | Entrada | 35% |
| Atrás da cortina | Sala | 60% |
| Dentro do móvel da televisão | Sala | 40% |
| Dentro do sofá | Sala | 25% |
| Dentro do armário inferior | Cozinha | 45% |
| Debaixo da mesa | Cozinha | 75% |
| Dentro da despensa | Cozinha | 35% |
| Debaixo da cama | Quarto | 80% |
| Dentro do guarda-roupa | Quarto | 55% |
| Dentro do baú | Quarto | 30% |
| Dentro do armário da pia | Banheiro | 50% |
| Atrás da porta | Banheiro | 40% |
| Atrás da cortina do box | Banheiro | 65% |

Dificuldade: Fácil = −10% em todas as chances, Normal = +0%, Difícil = +12%.

---

## 4. Versionamento

*(Preencher com o print do histórico de commits — `git log --oneline` ou a página de commits do GitHub —
com pelo menos 10 ações de add/commit/push e um texto explicando cada etapa.)*

Sugestão de etapas de commit:
1. Criação do projeto Unity e `.gitignore`
2. Artes dos cômodos e da planta da casa
3. Enumerações e classe abstrata `Personagem`
4. Classes `Esconderijo` e `Comodo`
5. Classe `Jogador`
6. Classe `Procurador` (máquina de estados e sorteio)
7. `GerenciadorJogo` e `MontadorCasa`
8. Interface: menu e fase de esconder
9. Interface: fase de busca e tela de resultado
10. Sons, música e polimento visual
11. Documentação, UML e executável

---

## 5. Instalação e execução

### Opção A — Jogar pelo executável (Windows)
1. Copie a pasta `Build` inteira (não só o `.exe`).
2. Dê dois cliques em `EscondeEsconde.exe`.
3. O jogo abre em tela cheia. **Alt+Enter** alterna para janela. Para fechar: botão *Sair* no menu.

### Opção B — Abrir o projeto na Unity
1. Instale o **Unity Hub** e o editor **Unity 6 (6000.6.0f1)** com *Windows Build Support*.
2. No Unity Hub: **Add → Add project from disk** e selecione a pasta `EscondeEsconde`.
3. Abra o projeto (na primeira vez a Unity importa tudo, leva alguns minutos).
4. A cena `Assets/Scenes/Jogo.unity` abre sozinha. Clique em **Play** ▶.
5. Para gerar um novo executável: menu **Esconde-Esconde → 2. Gerar executável (Windows)**.
   Se a cena sumir, use **Esconde-Esconde → 1. Montar cena do jogo**.

### Controles
| Ação | Mouse | Teclado |
|---|---|---|
| Trocar de cômodo | setas nas laterais | ← → (ou A / D) |
| Ver esconderijo | clicar no móvel | — |
| Confirmar / cancelar | botões | Enter / Esc |
| Mostrar todas as áreas clicáveis | — | F1 |
| Velocidade da busca | botões 1x / 2x / 4x | — |

---

## 6. Roteiro sugerido para a apresentação

1. **Jogar uma partida rápida** (Normal): mostrar as setas, o mini-mapa, a janela de risco e o
   botão *Acelerar o tempo*.
2. Na busca, mostrar o **painel do sorteio**: chance em laranja, traço escuro = número sorteado.
3. Abrir o **diagrama** e explicar: `Personagem` abstrata → `Jogador` / `Procurador`;
   `GerenciadorJogo` com o `Dictionary<string, Comodo>`; `Comodo` composto por `Esconderijo`.
4. Abrir `Procurador.cs` e mostrar `CalcularChance` e `InvestigarEsconderijo` (parâmetros e dicionário `memoria`).
5. Mostrar a separação **Modelo × Interface** e que o modelo avisa a tela por eventos.
6. Jogar de novo escondido no mesmo lugar onde foi pego, para mostrar a **memória** do procurador
   (aviso "ele já te encontrou aqui").
