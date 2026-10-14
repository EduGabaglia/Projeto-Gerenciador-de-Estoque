# Projeto Estoque — Requisitos Funcionais e Regras de Negócio

> **Documento gerado a partir da análise estática do código-fonte.**
> Branch de referência: `promocoes` · Solução: `projeto_estoque.sln`
> Cada requisito/regra recebe um identificador estável (`RF-xx`, `RN-xx`, `RISK-xx`)
> para permitir rastreabilidade e virar base de testes.

---

## Índice

1. [Visão geral](#1-visão-geral)
2. [Módulos do sistema](#2-módulos-do-sistema)
3. [Requisitos funcionais](#3-requisitos-funcionais)
4. [Regras de negócio](#4-regras-de-negócio)
5. [Fluxos críticos de estoque](#5-fluxos-críticos-de-estoque)
6. [Matriz de rastreabilidade](#6-matriz-de-rastreabilidade)
7. [Lacunas e riscos identificados](#7-lacunas-e-riscos-identificados)
8. [Recomendações priorizadas](#8-recomendações-priorizadas)

---

## 1. Visão geral

Sistema administrativo web para gestão de estoque e pedidos de uma loja/mercado.

| Item | Valor |
| --- | --- |
| **Stack** | ASP.NET Core MVC, .NET 9 (`projeto_estoque_web.csproj`) |
| **Linguagem** | C# com `Nullable` e `ImplicitUsings` habilitados |
| **Front-end** | Bootstrap 5, Bootstrap Icons, jQuery + jQuery Validate, CSS próprio por módulo |
| **Idioma** | pt-BR **fixo** — cultura e mensagens de model binding customizadas (`Program.cs:24-31`, `Program.cs:9-19`) |
| **Roteamento** | Rota padrão `{controller=Login}/{action=Index}/{id?}` (`Program.cs:48-51`) |
| **Persistência** | **Nenhuma.** Listas `static` em memória nos controllers; todos os dados são perdidos ao reiniciar a aplicação |
| **Views** | Razor: uma view por ação + partials de formulário, navegação e validação |
| **Layout** | Sidebar com 7 menus + topbar (tema claro/escuro persistido em `localStorage`) |
| **Segurança** | Apenas anti-forgery e validação de model binding. **Sem autenticação, sessão ou autorização** (ver [RISK-01](#risks)) |
| **Testes** | Nenhum projeto de teste na solução |
| **Documentação** | Este documento é o primeiro artefato de documentação do repositório |

### 1.1 Mapa de pastas

```
projeto_estoque/
├── projeto_estoque.sln
├── docs/
│   └── requisitos-e-regras-negocio.md      ← este documento
└── projeto_estoque_web/
    ├── projeto_estoque_web.csproj          → net9.0, Web SDK
    ├── Program.cs                          → pipeline MVC, localização pt-BR, rota padrão
    ├── appsettings.json                    → logging + AllowedHosts
    ├── appsettings.Development.json
    ├── launchSettings.json (Properties/)
    ├── Models/                             → domínio
    │   ├── produto.cs                      → Produto
    │   ├── categoria.cs                    → Categoria
    │   ├── pedido.cs                       → Pedido + PedidoItem
    │   ├── dados_empresa.cs                → DadosEmpresa
    │   └── view_models/                    → 7 VMs de listagem + 2 VMs de formulário
    │       ├── login_view_model.cs         → LoginViewModel
    │       ├── dashboard_view_model.cs     → DashboardViewModel, PedidoResumoViewModel, ProdutoMaisVendidoViewModel
    │       ├── produtos_view_model.cs      → ProdutosViewModel
    │       ├── categorias_view_model.cs    → CategoriasViewModel, CategoriaResumoViewModel
    │       ├── estoque_view_model.cs       → EstoqueViewModel, MovimentoEstoqueViewModel, AjusteEstoqueViewModel
    │       ├── pedidos_view_model.cs       → PedidosViewModel
    │       └── configuracoes_view_model.cs → ConfiguracoesViewModel
    ├── Controllers/                        → camada de "negócio" + dados estáticos
    │   ├── login_controller.cs
    │   ├── dashboard_controller.cs
    │   ├── produtos_controller.cs          → ANEXA lista estática de 14 produtos
    │   ├── categorias_controller.cs        → ANEXA lista estática de 5 categorias
    │   ├── estoque_controller.cs
    │   ├── pedidos_controller.cs           → ANEXA lista estática de 6 pedidos
    │   ├── promocoes_controller.cs         → stub
    │   ├── configuracoes_controller.cs     → ANEXA registro único de empresa
    │   └── home_controller.cs
    ├── Views/
    │   ├── _ViewImports.cshtml, _ViewStart.cshtml
    │   ├── Shared/                         → _Layout, _Layout.cshtml.css, _ValidationScriptsPartial
    │   ├── Login/index.cshtml
    │   ├── Dashboard/index.cshtml
    │   ├── Produtos/                       → Index, Criar, Editar, _ProdutoForm
    │   ├── Categorias/                     → Index, Criar, Editar, _CategoriaForm
    │   ├── Estoque/                        → Index, Entrada, Saida, Ajuste, _MovimentoForm, _Navegacao
    │   ├── Pedidos/                        → Index, Criar, Editar, _PedidoForm
    │   ├── Promocoes/Index.cshtml          → placeholder "em breve"
    │   └── Configuracoes/Index.cshtml
    └── wwwroot/
        ├── js/  → site.js, validacao.js, login.js, pedidos.js, configuracoes.js
        ├── css/ → site.css, admin.css + 1 folha por módulo
        └── lib/ → vendor (Bootstrap, jQuery)
```

### 1.2 Dados estáticos semeados

| Coleção | Quantidade | IDs | Ref. |
| --- | --- | --- | --- |
| `Produto` | 14 | 1 a 14 | `produtos_controller.cs:12-28` |
| `Categoria` | 5 | 1 a 5 | `categorias_controller.cs:11-18` |
| `Pedido` | 6 | 1001 a 1006 | `pedidos_controller.cs:17-91` |
| `DadosEmpresa` | 1 (singleton) | 1 | `configuracoes_controller.cs:9-19` |

Próximos IDs: produto `15`, categoria `6`, pedido `1007`.
Acesso cross-controller via propriedades `internal static` (`TodosOsProdutos`, `TodasAsCategorias`, `TodosOsPedidos`).

---

## 2. Módulos do sistema

| Módulo | Controller | Views | Modelo de domínio | Papel |
| --- | --- | --- | --- | --- |
| **Login** | `login_controller.cs` | `Login/index.cshtml` | `LoginViewModel` | Portão de entrada; valida credenciais fixas |
| **Dashboard** | `dashboard_controller.cs` | `Dashboard/index.cshtml` | `DashboardViewModel` | KPIs consolidados + últimos pedidos + top sellers |
| **Produtos** | `produtos_controller.cs` | `Produtos/{Index,Criar,Editar}`, `_ProdutoForm` | `Produto` | CRUD de produtos; dono da lista de produtos |
| **Categorias** | `categorias_controller.cs` | `Categorias/{Index,Criar,Editar}`, `_CategoriaForm` | `Categoria` | CRUD de categorias; dono da lista de categorias |
| **Estoque** | `estoque_controller.cs` | `Estoque/{Index,Entrada,Saida,Ajuste}`, `_MovimentoForm`, `_Navegacao` | `MovimentoEstoqueViewModel`, `AjusteEstoqueViewModel` | Mutação de saldos (consome a lista de produtos) |
| **Pedidos** | `pedidos_controller.cs` | `Pedidos/{Index,Criar,Editar}`, `_PedidoForm` | `Pedido`, `PedidoItem` | CRUD de pedidos + transições de status com efeito no estoque |
| **Promoções** | `promocoes_controller.cs` | `Promocoes/Index.cshtml` | — | Placeholder, sem implementação |
| **Configurações** | `configuracoes_controller.cs` | `Configuracoes/Index.cshtml` | `DadosEmpresa` | Cadastro único dos dados da empresa |
| **Home** | `home_controller.cs` | — | — | Redireciona para o Dashboard |

**Acoplamento:** Estoque e Pedidos dependem diretamente da lista estática de `ProdutosController`. Não há camada de serviço, repositório ou `DbContext`.

---

## 3. Requisitos funcionais

### 3.1 Autenticação (RF-01 a RF-05)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-01** | O sistema deve exibir tela de login com campos **usuário** e **senha**. | `Views/Login/index.cshtml:19-41` |
| **RF-02** | Ao enviar o formulário, o sistema deve validar as credenciais e, em caso de sucesso, redirecionar para o Dashboard. | `login_controller.cs:15-18` |
| **RF-03** | Em caso de credencial inválida, o sistema deve exibir mensagem genérica "Usuário ou senha inválidos." e reexibir o formulário com os dados digitados. | `login_controller.cs:19-20` |
| **RF-04** | O sistema deve permitir alternar a visibilidade do campo senha (ícone olho). | `wwwroot/js/login.js:5-26` |
| **RF-05** | A rota raiz do sistema deve ser a tela de login; `/Home` deve redirecionar para o Dashboard. | `Program.cs:48-51`, `home_controller.cs:8` |

> **Nota:** a "sessão" de login não existe. Ver [RISK-01](#7-lacunas-e-riscos-identificados).

### 3.2 Dashboard (RF-06 a RF-11)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-06** | Exibir 4 indicadores: total de produtos, produtos com estoque baixo, pedidos pendentes e vendas do mês corrente. | `dashboard_controller.cs:28-37` |
| **RF-07** | Listar os **3 pedidos mais recentes**, ordenados por data decrescente e, em empate, por id decrescente. | `dashboard_controller.cs:39-50` |
| **RF-08** | Listar os **4 produtos mais vendidos** com a quantidade total de unidades. | `dashboard_controller.cs:15-26` |
| **RF-09** | Exibir gráfico de vendas dos últimos 7 dias. | `Views/Dashboard/index.cshtml:76-104` |
| **RF-10** | Oferecer link "Ver todos" para a listagem completa de pedidos. | `Views/Dashboard/index.cshtml:151-156` |
| **RF-11** | Exibir o status de cada pedido com badge colorido. | `Views/Dashboard/index.cshtml:188-190` |

### 3.3 Produtos (RF-12 a RF-24)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-12** | Listar produtos em tabela: código, nome, categoria, preço, estoque, status e ações. | `Views/Produtos/Index.cshtml:120-177` |
| **RF-13** | Exibir indicadores: total de produtos, estoque baixo, nº de categorias distintas e valor total em estoque. | `produtos_controller.cs:56-66` |
| **RF-14** | Permitir busca textual por **nome do produto ou nome da categoria**. | `produtos_controller.cs:38-44` |
| **RF-15** | Paginar a listagem de produtos. | `produtos_controller.cs:46-54` |
| **RF-16** | Cadastrar novo produto com atribuição automática de id. | `produtos_controller.cs:71-92` |
| **RF-17** | Editar produto existente, pré-selecionando a categoria atual no campo de seleção. | `produtos_controller.cs:94-131` |
| **RF-18** | Excluir produto mediante confirmação do usuário e POST dedicated. | `produtos_controller.cs:133-146`, `Views/Produtos/Index.cshtml:161-173` |
| **RF-19** | O campo **categoria deve ser um seletor** populado com as categorias cadastradas. | `produtos_controller.cs:148-151` |
| **RF-20** | Exibir badge de status do produto com classe visual derivada da quantidade. | `produto.cs:25-37`, `Views/Produtos/Index.cshtml:149-151` |
| **RF-21** | Formulário deve bloquear o envio quando houver erros de validação, preservando os dados digitados. | `produtos_controller.cs:83-84, 120-121` |
| **RF-22** | Após salvar/excluir, redirecionar para a listagem exibindo mensagem de feedback. | `produtos_controller.cs:89-91, 128-130` |
| **RF-23** | O acesso a produto inexistente na edição deve retornar **404 Not Found**. | `produtos_controller.cs:98-99, 115-116` |
| **RF-24** | Se o id da rota divergir do id do formulário na edição, retornar **404**. | `produtos_controller.cs:110-111` |

### 3.4 Categorias (RF-25 a RF-33)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-25** | Listar categorias com a quantidade de produtos vinculados a cada uma. | `categorias_controller.cs:50-56` |
| **RF-26** | Exibir indicadores: total de categorias, categorias em uso, categorias vazias e média de produtos por categoria em uso. | `categorias_controller.cs:60-72` |
| **RF-27** | Permitir busca textual por **nome ou descrição** da categoria. | `categorias_controller.cs:28-34` |
| **RF-28** | Cadastrar categoria com validação de nome único. | `categorias_controller.cs:77-98` |
| **RF-29** | Editar categoria; ao alterar o nome, o sistema deve **migrar automaticamente** os produtos vinculados. | `categorias_controller.cs:128-137` |
| **RF-30** | Excluir categoria; a exclusão deve ser **bloqueada** quando houver produtos vinculados, com mensagem informative. | `categorias_controller.cs:146-165` |
| **RF-31** | Paginar a listagem de categorias. | `categorias_controller.cs:36-44` |
| **RF-32** | Após falha de validação, o formulário deve ser reexibido preservando os dados digitados. | `categorias_controller.cs:89-90, 125-126` |
| **RF-33** | O acesso a categoria inexistente na edição deve retornar **404**. | `categorias_controller.cs:104-105, 119-120` |

### 3.5 Estoque (RF-35 a RF-43)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-34** | Exigir a **seleção de um produto válido** em qualquer movimentação; produto não selecionado ou inexistente gera o erro "Selecione um produto." | `estoque_controller.cs:65, 95, 132` |
| **RF-35** | Oferecer 4 abas de operação: **Visualizar**, **Entrada**, **Saída**, **Ajuste**, com a aba ativa destacada. | `Views/Estoque/_Navegacao.cshtml:5-27` |
| **RF-36** | Listar produtos com saldo e status de estoque, com busca e paginação. | `estoque_controller.cs:12-44`, `Views/Estoque/Index.cshtml` |
| **RF-37** | Permitir registrar **entrada** de unidades em um produto. | `estoque_controller.cs:46-74` |
| **RF-38** | Permitir registrar **saída** de unidades em um produto, bloqueando saldo insuficiente. | `estoque_controller.cs:76-111` |
| **RF-39** | Permitir registrar **ajuste** de saldo para um valor absoluto. | `estoque_controller.cs:113-141` |
| **RF-40** | Os seletores de produto devem exibir o **saldo atual** junto ao nome. | `estoque_controller.cs:146-155` |
| **RF-41** | Após registrar um movimento, redirecionar para a listagem exibindo mensagem com produto e quantidade. | `estoque_controller.cs:71, 108, 138` |
| **RF-42** | A listagem de estoque deve oferecer atalho de edição do produto. | `Views/Estoque/Index.cshtml:82-90` |
| **RF-43** | Movimentos de entrada e saída devem exigir quantidade **mínima de 1**; o ajuste aceita **0**. | `estoque_view_model.cs:16-36` |

### 3.6 Pedidos (RF-44 a RF-60)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-44** | Listar pedidos em tabela: código, data, cliente, nº de itens, valor e status, com busca e paginação. | `Views/Pedidos/Index.cshtml:120-208` |
| **RF-45** | Exibir indicadores: total de pedidos, pendentes, entregues e receita total. | `pedidos_controller.cs:138-148` |
| **RF-46** | Permitir busca por **nome do cliente ou código exato** do pedido. | `pedidos_controller.cs:103-111` |
| **RF-47** | Permitir **criação de pedido** com múltiplos itens, adicionando e removendo linhas dinamicamente na tela. | `Views/Pedidos/_PedidoForm.cshtml:27-81`, `wwwroot/js/pedidos.js:31-53` |
| **RF-48** | Permitir **edição de pedido** (cliente, data, status e itens). | `pedidos_controller.cs:288-405` |
| **RF-49** | Permitir **exclusão de pedido** mediante confirmação. | `pedidos_controller.cs:407-431`, `Views/Pedidos/Index.cshtml:195-205` |
| **RF-50** | Permitir **alterar o status inline** via seletor com envio automático, preservando busca e página atual. | `Views/Pedidos/Index.cshtml:158-184`, `pedidos_controller.cs:155-214` |
| **RF-51** | O valor total e o número de itens do pedido devem ser calculados automaticamente. | `pedido.cs:22-24` |
| **RF-52** | O sistema deve validar a disponibilidade de estoque na criação e na edição. | `pedidos_controller.cs:246-264, 331-360` |
| **RF-53** | Ao cancelar um pedido, as quantidades devem ser **devolvidas ao estoque**. | `pedidos_controller.cs:176-188` |
| **RF-54** | Ao reativar um pedido cancelado, o estoque deve ser **re-reservado**, com validação integral prévia. | `pedidos_controller.cs:189-207` |
| **RF-55** | Nome e preço unitário dos itens devem ser **capturados do catálogo** no momento do registro. | `pedidos_controller.cs:271-273, 377-379` |
| **RF-56** | A exclusão de pedido não cancelado deve **estornar** o estoque. | `pedidos_controller.cs:415-424` |
| **RF-57** | O status do pedido deve ser um seletor restrito à lista de status válidos. | `pedidos_controller.cs:12-15`, `Views/Pedidos/_PedidoForm.cshtml:21-25` |
| **RF-58** | O formulário deve manter a indexação correta dos itens após adicionar/remover linhas. | `wwwroot/js/pedidos.js:1-28` |
| **RF-59** | Ao remover todos os itens, o sistema deve exibir o estado vazio "Nenhum item adicionado ainda." | `Views/Pedidos/_PedidoForm.cshtml:74-76`, `wwwroot/js/pedidos.js:23-26` |
| **RF-60** | Após salvar/excluir/atualizar status, a listagem deve exibir mensagem de sucesso ou erro. | `pedidos_controller.cs:159, 167, 211, 283, 402, 427` |

### 3.7 Configurações (RF-61 a RF-66)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-61** | Manter **cadastro único** (singleton) dos dados da empresa, com id fixo 1. | `configuracoes_controller.cs:9-19` |
| **RF-62** | Exibir cards-resumo: percentual de preenchimento (x/7), nome da empresa, CNPJ e cidade/UF. | `Views/Configuracoes/Index.cshtml:20-52` |
| **RF-63** | Alertar o usuário enquanto os dados exibidos forem **valores de exemplo**. | `configuracoes_controller.cs:21, 54-60`, `Views/Configuracoes/Index.cshtml:54-60` |
| **RF-64** | Salvar os dados aplicando normalização de espaços e caixa da UF. | `configuracoes_controller.cs:38-44` |
| **RF-65** | Exibir card "Completo/Incompleto" conforme o preenchimento dos 7 campos. | `configuracoes_view_model.cs:14-27`, `Views/Configuracoes/Index.cshtml:20-26` |
| **RF-66** | Aplicar **máscara progressiva** de CNPJ e telefone enquanto o usuário digita. | `wwwroot/js/configuracoes.js:8-44` |

### 3.8 Requisitos transversais (RF-67 a RF-74)

| ID | Requisito | Evidência |
| --- | --- | --- |
| **RF-67** | Exibir mensagens de sucesso e de erro de forma uniforme em todas as listagens. | `TempData["Mensagem"]` / `TempData["Erro"]` nos 4 controllers |
| **RF-68** | Todas as telas e mensagens devem estar em **português do Brasil**. | `Program.cs:9-19, 24-31` |
| **RF-69** | Validar formulários no cliente (jQuery Validate) **e** no servidor (DataAnnotations). | `wwwroot/js/validacao.js`, `Models/*.cs` |
| **RF-70** | Aceitar vírgula decimal em campos numéricos (padrão pt-BR). | `wwwroot/js/validacao.js:6-29` |
| **RF-71** | Formatar valores monetários com o padrão de moeda pt-BR. | `ToString("C")` nas views |
| **RF-72** | Oferecer alternância de tema claro/escuro, persistida entre páginas e abas. | `Views/Shared/_Layout.cshtml:309-389` |
| **RF-73** | Menu lateral responsivo, com overlay e botão de fechar em telas pequenas. | `Views/Shared/_Layout.cshtml:267-307` |
| **RF-74** | Todos os POSTs de alteração de dados devem ser protegidos contra CSRF. | `[ValidateAntiForgeryToken]` — exceto o login ([RISK-04](#7-lacunas-e-riscos-identificados)) |

### 3.9 Fora de escopo (não implementado)

| Item | Situação |
| --- | --- |
| **Módulo Promoções** | Placeholder; view exibe "Esta seção ainda está em construção." (`Views/Promocoes/Index.cshtml:19-23`) |
| **Persistência em banco** | Inexistente — apenas listas estáticas em memória |
| **Autenticação real / perfis** | Inexistente — credencial fixa em código |
| **Histórico de movimentações** | Inexistente — apenas o saldo é mutado |
| **Relatórios e exportações** | Inexistente (o gráfico do Dashboard é fixo, ver [RISK-07](#7-lacunas-e-riscos-identificados)) |
| **Cadastro de clientes, fornecedores, usuários** | Inexistente — o cliente do pedido é apenas texto livre |
| **Notificações / e-mail / NFe** | Inexistentes (o sino da topbar não tem ação, `Views/Shared/_Layout.cshtml:211-217`) |

---

## 4. Regras de negócio

### 4.1 Produto — RN-01 a RN-08

| # | Regra | Evidência |
| --- | --- | --- |
| **RN-01** | O **nome** do produto é obrigatório e limitado a **80 caracteres**. | `produto.cs:9-11` |
| **RN-02** | A **categoria** é obrigatória e deve ser uma das categorias cadastradas. | `produto.cs:13`, `produtos_controller.cs:148-151` |
| **RN-03** | O **preço** é obrigatório, deve estar entre **0,01 e 1.000.000,00** e é tratado como moeda. | `produto.cs:16-19` |
| **RN-04** | A **quantidade em estoque** é obrigatória e deve estar entre **0 e 1.000.000** — nunca negativa. | `produto.cs:21-23` |
| **RN-05** | O **status é derivado da quantidade**, nunca informado pelo usuário: `≤ 0` → **Esgotado**; `≤ 5` → **Estoque baixo**; `> 5` → **Disponível**. | `produto.cs:25-30` |
| **RN-06** | A classe CSS do badge de status acompanha a mesma régua (`esgotado` / `baixo` / `disponivel`). | `produto.cs:32-37` |
| **RN-07** | O **valor total em estoque** é `Σ (preço × quantidade)` de todos os produtos. | `produtos_controller.cs:61` |
| **RN-08** | O indicador **"Estoque baixo"** conta produtos com `quantidade ≤ 5` (inclui esgotados). | `produtos_controller.cs:59` |

### 4.2 Categoria — RN-09 a RN-15

| # | Regra | Evidência |
| --- | --- | --- |
| **RN-09** | **Nome** obrigatório (máx. 60) e **descrição** obrigatória (máx. 200). | `categoria.cs:9-15` |
| **RN-10** | O **nome da categoria é único**, comparação sem distinção de maiúsculas após `Trim()`, **ignorando o próprio registro** na edição. | `categorias_controller.cs:167-175, 86-87, 122-123` |
| **RN-11** | **Renomear uma categoria propaga**: todos os produtos que apontam para o nome antigo passam a apontar para o novo nome. | `categorias_controller.cs:133-137` |
| **RN-12** | **Exclusão bloqueada** quando a categoria possui ≥ 1 produto vinculado; a mensagem inclui a quantidade de produtos. | `categorias_controller.cs:152-158` |
| **RN-13** | O vínculo **produto ↔ categoria é feito pelo NOME (string)**, não por chave estrangeira. | `produto.cs:14`, `produtos_controller.cs:150` |
| **RN-14** | Uma categoria está **"em uso"** quando ao menos 1 produto referencia seu nome; caso contrário é "vazia". | `categorias_controller.cs:46-58` |
| **RN-15** | A **média de produtos por categoria** = `total de produtos ÷ categorias em uso`, arredondada a 1 casa decimal; `0` se nenhuma categoria estiver em uso. | `categorias_controller.cs:65-67` |

### 4.3 Movimentações de estoque — RN-16 a RN-23

| # | Regra | Evidência |
| --- | --- | --- |
| **RN-16** | **Entrada** *soma* ao saldo existente; a quantidade deve estar entre **1 e 1.000.000**. | `estoque_controller.cs:69`, `estoque_view_model.cs:21-24` |
| **RN-17** | **Saída** * subtrai* do saldo; a quantidade deve estar entre **1 e 1.000.000**. | `estoque_controller.cs:106`, `estoque_view_model.cs:21-24` |
| **RN-18** | A **saída é bloqueada se a quantidade exceder o saldo disponível**, com mensagem informando o saldo atual. | `estoque_controller.cs:99-104` |
| **RN-19** | O **ajuste sobrescreve** o saldo com um valor absoluto entre **0 e 1.000.000** — pode zerar, mas não ficar negativo. | `estoque_controller.cs:136`, `estoque_view_model.cs:32-35` |
| **RN-20** | Produto inexistente ou não selecionado gera o erro **"Selecione um produto."** | `estoque_controller.cs:65, 95, 132` |
| **RN-21** | O saldo do produto **nunca pode ficar negativo** por movimentação manual (RN-18 e RN-19). | `estoque_controller.cs:99-106, 136` |
| **RN-22** | **Não existe histórico nem auditoria** de movimentações — só o saldo é mutado, sem rastro de quem/quando/quanto. | ausência de entidade de movimento |
| **RN-23** | **Editar a quantidade pela tela de Produtos altera o saldo sem gerar movimentação**, burlando a rastreabilidade. | `produtos_controller.cs:126` |

### 4.4 Pedidos — RN-24 a RN-43

| # | Regra | Evidência |
| --- | --- | --- |
| **RN-24** | O **status pertence a uma lista fechada**: `Pendente`, `Preparando`, `Entregue`, `Cancelado`. | `pedidos_controller.cs:12-15` |
| **RN-25** | O status é **normalizado sem distinção de caixa**; valor fora da lista ou vazio **cai para "Pendente"**. | `pedidos_controller.cs:446-450` |
| **RN-26** | **Cliente** obrigatório (máx. 80) e **Data** obrigatória, com default = data atual. | `pedido.cs:9-16`, `pedidos_controller.cs:221` |
| **RN-27** | O pedido **precisa conter ao menos 1 item** após a normalização. | `pedidos_controller.cs:238-242, 323-327` |
| **RN-28** | **Normalização de itens**: linhas com `ProdutoId ≤ 0` **ou** `Quantidade ≤ 0` são descartadas; itens do **mesmo produto são consolidados somando as quantidades**. | `pedidos_controller.cs:433-444` |
| **RN-29** | **Nome do produto e preço unitário são sempre sobrescritos pelo catálogo** — o cliente não tem como definir preço. | `pedidos_controller.cs:271-273, 377-379, 387-389` |
| **RN-30** | **O estoque é baixado na CRIAÇÃO do pedido** (reserva imediata), exceto quando o status é "Cancelado". | `pedidos_controller.cs:275-276` |
| **RN-31** | Na criação, é validado `quantidade solicitada ≤ saldo disponível` para **cada produto**, com mensagem nomeada por produto. | `pedidos_controller.cs:246-264` |
| **RN-32** | Na edição, o saldo reservado pelo pedido anterior é **estornado antes** de reservar o novo, e a disponibilidade é calculada **já com o estorno** (`saldo + qtde anterior`). | `pedidos_controller.cs:345-352, 362-371` |
| **RN-33** | **Cancelar devolve** todas as quantidades dos itens ao estoque. | `pedidos_controller.cs:176-188` |
| **RN-34** | **Reativar um pedido cancelado** revalida **todos** os itens; se **qualquer um** falhar, a operação é **abortada por inteiro** (mensagem "Status não alterado"). | `pedidos_controller.cs:189-207` |
| **RN-35** | **Excluir** um pedido **não cancelado devolve o estoque**; se já cancelado, **não devolve** (evita devolução duplicada). | `pedidos_controller.cs:415-424` |
| **RN-36** | Redefinir o status para o **mesmo valor** é uma operação silenciosa (no-op). | `pedidos_controller.cs:173-174` |
| **RN-37** | **Não existe matriz de transições**: qualquer status pode ir para qualquer outro. Só "Cancelado" (RN-33/RN-34) tem efeito no estoque — inclusive "Entregue" **não** altera o saldo, pois a baixa ocorreu na criação. | `pedidos_controller.cs:176-207` |
| **RN-38** | **Valor total** = `Σ (quantidade × preço unitário)`; **nº de itens** = quantidade de **linhas distintas**, não o total de unidades. | `pedido.cs:22-24` |
| **RN-39** | **Receita total e vendas do mês ignoram pedidos cancelados**. | `pedidos_controller.cs:136-143`, `dashboard_controller.cs:33-37` |
| **RN-40** | A **busca textual** de pedidos casa por nome do cliente (substring) **ou por código exato**; um termo numérico só funciona por igualdade de id. | `pedidos_controller.cs:103-111` |
| **RN-41** | Pedidos novos são inseridos **no topo** da lista; os ids são incrementais a partir de 1007. | `pedidos_controller.cs:93, 280-281` |
| **RN-42** | Os erros de model binding dos itens (`Itens[...]`) são **descartados** após a normalização, evitando mensagens espúrias do binder. | `pedidos_controller.cs:452-460` |
| **RN-43** | A **listagem de pedidos** é ordenada por `Data ↓` e depois `Id ↓`. | `pedidos_controller.cs:113-116` |

### 4.5 Dashboard e Configurações — RN-44 a RN-51

| # | Regra | Evidência |
| --- | --- | --- |
| **RN-44** | **Vendas do mês** = `Σ valor` dos pedidos **não cancelados** cujo ano e mês coincidem com o corrente. | `dashboard_controller.cs:33-37` |
| **RN-45** | **Mais vendidos** = agregação da quantidade por nome de produto, considerando **todos** os pedidos não cancelados (sem filtro de mês). | `dashboard_controller.cs:15-26` |
| **RN-46** | **Dados da empresa** — validações: nome (≤100), CNPJ mascarado `00.000.000/0000-00` **ou** 14 dígitos, endereço (≤120), cidade (≤60), **UF com exatamente 2 caracteres**, telefone mascarado `(00) 00000-0000` **ou** 10-11 dígitos (≤15), e-mail válido (≤100). Todos obrigatórios. | `dados_empresa.cs:9-37` |
| **RN-47** | Ao salvar, **todos** os campos são gravados com `Trim()` e a **UF é convertida para maiúsculas**. | `configuracoes_controller.cs:38-44` |
| **RN-48** | A flag **`_dadosDeExemplo`** governa a apresentação: enquanto `true`, o formulário abre **vazio** com alerta de dados fictícios; após o **primeiro salvamento** bem-sucedido vira `false` e o formulário é pré-preenchido. | `configuracoes_controller.cs:21, 46, 53-59` |
| **RN-49** | O card "Completo" considera **7 campos**; campos em branco não contam como preenchidos. | `configuracoes_view_model.cs:14-27` |
| **RN-50** | As **máscaras client-side** são progressivas e operam sobre **somente dígitos** (CNPJ limitado a 14, telefone a 11). | `wwwroot/js/configuracoes.js:8-29, 43-44` |
| **RN-51** | A propriedade `Resumo` do ViewModel é marcada com `[ValidateNever]`, impedindo que dados do resumo sejam sobrescritos pelo formulário. | `configuracoes_view_model.cs:9-10` |

### 4.6 Regras transversais — RN-52 a RN-61

| # | Regra | Evidência |
| --- | --- | --- |
| **RN-52** | **Paginação: 8 itens por página** em Produtos, Categorias, Estoque e Pedidos. | `ItensPorPagina = 8` nos 4 controllers |
| **RN-53** | O número da página é **limitado (`Math.Clamp`) ao intervalo 1..totalPaginas**; o total de páginas nunca é menor que 1. | `Math.Max(1, ...)`, `Math.Clamp(...)` nos 4 controllers |
| **RN-54** | A **busca é insensível a maiúsculas**, aplica `Trim()` e faz correspondência por *substring*; termo vazio não filtra. | 4 controllers |
| **RN-55** | A ordenação padrão de produtos, categorias e estoque é **alfabética por nome**; pedidos são por data/id decrescentes. | `OrderBy(p => p.Nome)` / `OrderByDescending(p => p.Data)` |
| **RN-56** | Todo `POST` de alteração de dados usa `[ValidateAntiForgeryToken]` — **exceto o login**. | 4 controllers vs. `login_controller.cs:12` |
| **RN-57** | O feedback ao usuário é sempre transitório, via `TempData["Mensagem"]` (sucesso) e `TempData["Erro"]` (falha). | 4 controllers |
| **RN-58** | Recursos não encontrados devem retornar **404** (`NotFound()`), não redirecionar. | 4 controllers |
| **RN-59** | A aplicação opera exclusivamente em **pt-BR**, sem fallback para outro idioma. | `Program.cs:24-31` |
| **RN-60** | Datas são formatadas como `dd/MM/yyyy` e valores monetários com `ToString("C")`. | views de Pedidos, Produtos e Dashboard |
| **RN-61** | Campos numéricos aceitam **vírgula como separador decimal** (padrão pt-BR); a validação client-side normaliza vírgula para ponto antes de avaliar. | `wwwroot/js/validacao.js:6-29` |

---

## 5. Fluxos críticos de estoque

Esta seção documenta os 3 fluxos com **efeito colateral no saldo**, por serem os pontos de maior risco de inconsistência.

### 5.1 Criação de pedido — `POST Pedidos/Criar`

```
1. Normaliza o status          → Status fora da lista vira "Pendente"          [RN-25]
2. Normaliza os itens          → descarta produto≤0 / qtd≤0; consolida duplicados [RN-28]
3. Limpa erros do binder       → remove chaves "Itens[...]" do ModelState       [RN-42]
4. Valida ModelState           → erro: reexibe o formulário
5. Exige ≥ 1 item              → erro: "Adicione pelo menos um item ao pedido."  [RN-27]
6. Se status ≠ Cancelado:
     6a. Para cada produto: existe?          → erro: "produto inexistente"
     6b. Para cada produto: qtd ≤ saldo?      → erro: "Estoque insuficiente (...)"  [RN-31]
7. Para cada item:
     7a. NomeProduto  ← nome do catálogo                                        [RN-29]
     7b. PrecoUnitario ← preço atual do catálogo                                [RN-29]
     7c. Se status ≠ Cancelado: saldo -= quantidade                             [RN-30]
8. Atribui id incremental e insere no TOPO da lista                             [RN-41]
9. Redirect para Index com mensagem de sucesso
```

**Ponto de atenção:** a validação (6) e a mutação (7c) são **separadas**, sem transação. Uma exceção no meio do passo 7 deixa o estoque parcialmente debitado.

### 5.2 Edição de pedido — `POST Pedidos/Editar`

```
1. Valida id da rota × id do model           → 404 se divergirem                 [RN-58]
2. Localiza o pedido existente               → 404 se não existir
3. Normaliza status, itens e ModelState       (mesmos passos 1-4 da criação)
4. Exige ≥ 1 item                                                        [RN-27]
5. Se NOVO status ≠ Cancelado:
     5a. disponível = saldo atual
         + soma das quantidades do pedido existente com o MESMO produtoId,
           desde que o status existente ≠ "Cancelado"                        [RN-32]
     5b. qtd solicitada ≤ disponível? → erro citando o valor "disponível"
6. ESTORNO: se status existente ≠ Cancelado,
     devolve ao estoque todas as quantidades do pedido anterior             [RN-32]
7. Se NOVO status ≠ Cancelado:
     para cada item: snapshot de nome/preço + saldo -= quantidade           [RN-30]
8. Se NOVO status = Cancelado:
     apenas snapshot de nome/preço (nenhuma reserva)                        [RN-33]
9. Copia Cliente, Data, Status e Itens para o pedido existente
10. Redirect com mensagem de sucesso
```

**Ordem crítica:** o estorno (passo 6) **precisa** vir antes da nova reserva (passo 7); e a validação (5) **precisa** usar o saldo já somado com o estorno, caso contrário um pedido que reduz a própria quantidade seria rejeitado indevidamente.

### 5.3 Alteração de status — `POST Pedidos/AtualizarStatus`

```
1. Status pertence à lista?   → não: erro "Status inválido." e volta à lista  [RN-24]
2. Pedido existe?             → não: erro "Pedido não encontrado."             [RN-58]
3. Normaliza o status recebido (case-insensitive)                            [RN-25]
4. Status igual ao atual?     → sim: no-op silencioso                          [RN-36]
5. Se NOVO status = "Cancelado" e status atual ≠ "Cancelado":
     para cada item: saldo += quantidade  (devolve)                           [RN-33]
6. Se status ATUAL = "Cancelado" e NOVO status ≠ "Cancelado":
     6a. VALIDAÇÃO de todos os itens: qtd ≤ saldo?
         qualquer falha → erro "Estoque insuficiente (...)", ABORTA TUDO,
         status permanece "Cancelado"                                         [RN-34]
     6b. Só então: para cada item: saldo -= quantidade (re-reserva)           [RN-34]
7. Atribui o novo status e exibe mensagem
```

**Observação:** a transição para "Entregue" **não** mexe no estoque — a baixa ocorreu na criação (RN-30). Consequentemente, um pedido entregue ocupa estoque permanentemente até ser excluído.

---

## 6. Matriz de rastreabilidade

| Requisito | Regra de negócio | Implementação | View |
| --- | --- | --- | --- |
| RF-01, RF-02, RF-03 | RN-25 | `login_controller.cs:13-21` | `Views/Login/index.cshtml` |
| RF-04 | — | `wwwroot/js/login.js` | — |
| RF-06 | RN-08, RN-44 | `dashboard_controller.cs:28-37` | `Views/Dashboard/index.cshtml:21-68` |
| RF-07 | RN-43 | `dashboard_controller.cs:39-50` | `Views/Dashboard/index.cshtml:143-199` |
| RF-08 | RN-45 | `dashboard_controller.cs:15-26` | `Views/Dashboard/index.cshtml:108-137` |
| RF-09 | — | — | `Views/Dashboard/index.cshtml:85-102` |
| RF-12, RF-13 | RN-07, RN-08 | `produtos_controller.cs:34-69` | `Views/Produtos/Index.cshtml` |
| RF-14 | RN-54 | `produtos_controller.cs:38-44` | `Views/Produtos/Index.cshtml:91-100` |
| RF-15, RF-68 | RN-52, RN-53 | `produtos_controller.cs:46-54` | `Views/Produtos/Index.cshtml:184-213` |
| RF-16 | RN-01, RN-02, RN-03, RN-04 | `produtos_controller.cs:71-92` | `Views/Produtos/_ProdutoForm.cshtml` |
| RF-17, RF-24 | RN-01 a RN-04 | `produtos_controller.cs:94-131` | `Views/Produtos/_ProdutoForm.cshtml` |
| RF-18 | RN-56, RN-57 | `produtos_controller.cs:133-146` | `Views/Produtos/Index.cshtml:161-173` |
| RF-19 | RN-02, RN-13 | `produtos_controller.cs:148-151` | `Views/Produtos/_ProdutoForm.cshtml` |
| RF-20 | RN-05, RN-06 | `produto.cs:25-37` | `Views/Produtos/Index.cshtml:149-151` |
| RF-25, RF-26 | RN-14, RN-15 | `categorias_controller.cs:46-72` | `Views/Categorias/Index.cshtml` |
| RF-27 | RN-54 | `categorias_controller.cs:28-34` | `Views/Categorias/Index.cshtml` |
| RF-28 | RN-09, RN-10 | `categorias_controller.cs:77-98` | `Views/Categorias/_CategoriaForm.cshtml` |
| RF-29 | RN-11 | `categorias_controller.cs:128-137` | `Views/Categorias/Editar.cshtml` |
| RF-30 | RN-12 | `categorias_controller.cs:152-158` | `Views/Categorias/Index.cshtml` |
| RF-35, RF-36 | RN-52, RN-53, RN-54 | `estoque_controller.cs:12-44` | `Views/Estoque/_Navegacao.cshtml` |
| RF-37 | RN-16, RN-20, RN-21 | `estoque_controller.cs:46-74` | `Views/Estoque/Entrada.cshtml`, `_MovimentoForm` |
| RF-38 | RN-17, RN-18, RN-20, RN-21 | `estoque_controller.cs:76-111` | `Views/Estoque/Saida.cshtml`, `_MovimentoForm` |
| RF-39 | RN-19, RN-20, RN-21 | `estoque_controller.cs:113-141` | `Views/Estoque/Ajuste.cshtml` |
| RF-40 | — | `estoque_controller.cs:146-155` | `Views/Estoque/_MovimentoForm.cshtml:5-11` |
| RF-44, RF-45 | RN-39, RN-40, RN-43 | `pedidos_controller.cs:97-151` | `Views/Pedidos/Index.cshtml` |
| RF-47 | RN-26, RN-27, RN-28, RN-29, RN-30, RN-31 | `pedidos_controller.cs:216-286` | `Views/Pedidos/Criar.cshtml`, `_PedidoForm` |
| RF-48 | RN-32, RN-33 | `pedidos_controller.cs:288-405` | `Views/Pedidos/Editar.cshtml`, `_PedidoForm` |
| RF-49 | RN-35 | `pedidos_controller.cs:407-431` | `Views/Pedidos/Index.cshtml:195-205` |
| RF-50 | RN-24, RN-25, RN-33, RN-34, RN-36, RN-37 | `pedidos_controller.cs:153-214` | `Views/Pedidos/Index.cshtml:158-184` |
| RF-51 | RN-38 | `pedido.cs:22-24` | `Views/Pedidos/_PedidoForm.cshtml` |
| RF-55 | RN-29 | `pedidos_controller.cs:271-273` | — |
| RF-56 | RN-35 | `pedidos_controller.cs:415-424` | — |
| RF-61 a RF-65 | RN-46, RN-47, RN-48, RN-49, RN-51 | `configuracoes_controller.cs:9-59` | `Views/Configuracoes/Index.cshtml` |
| RF-66 | RN-50 | `wwwroot/js/configuracoes.js` | `Views/Configuracoes/Index.cshtml:150-153` |
| RF-67 | RN-57 | 4 controllers | views de listagem |
| RF-68, RF-69 | RN-59 | `Program.cs:9-31`, `wwwroot/js/validacao.js` | `_ValidationScriptsPartial.cshtml` |
| RF-70 | RN-61 | `wwwroot/js/validacao.js:6-29` | — |
| RF-71 | RN-60 | `ToString("C")` | views |
| RF-72 | — | `Views/Shared/_Layout.cshtml:309-389` | — |
| RF-73 | — | `Views/Shared/_Layout.cshtml:267-307` | — |
| RF-74 | RN-56 | 4 controllers | — |

---

## 7. Lacunas e riscos identificados

Severidades: **Crítico** (bloqueia produção) · **Alto** (corrompe dados) · **Médio** (comportamento incorreto) · **Baixo** (dívida técnica/UX).

### 7.1 Segurança

| ID | Risco | Sev. | Evidência | Impacto | Correção sugerida |
| --- | --- | --- | --- | --- | --- |
| **RISK-01** | **Autenticação inexistente.** Não há `AddAuthentication`, cookie, sessão nem atributo `[Authorize]`. `UseAuthorization()` é chamado sem um esquema de autenticação configurado. | **Crítico** | `Program.cs:44`; ausência de `[Authorize]` em todos os controllers | **Todas** as áreas (produtos, pedidos, estoque, configurações) são acessíveis anonimamente. O login é apenas um redirecionamento visual. | Configurar `AddAuthentication`/`AddCookie`, decorar as actions com `[Authorize]`, adicionar logout e `LoginPath`. |
| **RISK-02** | **Credencial fixa em código-fonte** (`Admin` / `@123`). | **Crítico** | `login_controller.cs:15` | Credencial publicada no repositório; qualquer pessoa com acesso ao código acessa o sistema. | Migrar para tabela de usuários com hash (PBKDF2/bcrypt/Argon2) e seed controlled por ambiente. |
| **RISK-03** | **Não há controle de sessão nem expiração** — nenhum cookie é emitido após login. | **Alto** | `login_controller.cs:12-21` | Sem sessão é impossível auditar "quem" fez cada operação; a rastreabilidade de estoque é perdida. | Idem RISK-01 + registrar usuário autenticado nas movimentações. |
| **RISK-04** | O `POST` de login **não usa `[ValidateAntiForgeryToken]`**, e exibe `@Html.ValidationSummary()` sem filtro. | **Médio** | `login_controller.cs:12`, `Views/Login/index.cshtml:20` | Possibilidade de login CSRF; poluição de mensagens de erro. | Adicionar `[ValidateAntiForgeryToken]` e `@Html.ValidationSummary(false)` ou `ModelOnly`. |
| **RISK-05** | `app.UseHttpsRedirection()` está **comentado**, e o HSTS só é aplicado fora de Development. | **Médio** | `Program.cs:41, 34-39` | Credenciais trafegam em texto claro em desenvolvimento. | Reativar a redireção e configurar `UseHsts` por ambiente. |

### 7.2 Integridade de dados

| ID | Risco | Sev. | Evidência | Impacto | Correção sugerida |
| --- | --- | --- | --- | --- | --- |
| **RISK-06** | **Excluir produto com pedidos vinculados causa HTTP 500.** `PedidosController.Editar` usa `ProdutosController.TodosOsProdutos.First(...)`, que lança `InvalidOperationException` se o produto não existir. | **Alto** | `pedidos_controller.cs:377` (e `:204`); exclusão em `produtos_controller.cs:133-146` | tela de edição de pedido quebra; o pedido fica inconsistente. | Bloquear a exclusão de produto com pedidos vinculados (como já é feito com categorias, RN-12), ou usar `FirstOrDefault` com tratamento. |
| **RISK-07** | **Cancelamento silencioso com produto excluído:** o caminho de cancelamento usa `FirstOrDefault` e apenas pula o produto, **sem devolver o estoque**. | **Alto** | `pedidos_controller.cs:184-186` | Estoque perde unidades para sempre, sem erro visível. | Bloquear exclusão de produto (RISK-06) ou registrar pendências de estorno. |
| **RISK-08** | **Sem persistência:** todos os dados são listas `static` em memória. | **Crítico** | `produtos_controller.cs:12`, `categorias_controller.cs:11`, `pedidos_controller.cs:17` | Qualquer reinício/deploy **apaga** produtos, categorias, pedidos e configurações. Não há backup nem auditoria. | Introduzir `DbContext` + EF Core (SQLite para dev, PostgreSQL/SQL Server para prod) e registrar as dependências via `ID`/`DI`. |
| **RISK-09** | **Estruturas estáticas não são thread-safe** (`List<T>` compartilhada entre requisições). | **Alto** | `ProdutosController.TodosOsProdutos` usado em 3 controllers | Condições de corrida em `Add`/`Remove`/`Insert` sob concorrência; corrupção de lista. | Repositório com lock, `ConcurrentBag`/`ImmutableList`, ou resolver junto com RISK-08. |
| **RISK-10** | **Ausência de transação** entre validação de estoque e mutação do saldo (fluxos 5.1 e 5.2). | **Alto** | `pedidos_controller.cs:246-278`, `:331-381` | Exceção no meio do laço deixa parte do estoque debitado e o pedido não salvo. | Envolver as operações em transação/`Unit of Work`. |
| **RISK-11** | **Atualização manual de saldo pela tela de Produtos** burla a rastreabilidade. | **Médio** | `produtos_controller.cs:126` | Saldos divergem do histórico de movimentações; erros não detectáveis. | Remover o campo `Quantidade` do formulário de produto, ou exigir um ajuste registrado. |
| **RISK-12** | **Idempotência de ID** depende de `_proximoId` estático compartilhado, sem proteção. | **Médio** | `produtos_controller.cs:30`, `categorias_controller.cs:20`, `pedidos_controller.cs:93` | Dois "criar" simultâneos podem gerar id duplicado. | Resolvido pelo ID sequencial do banco (RISK-08). |

### 7.3 Regras implementadas incorretamente

| ID | Risco | Sev. | Evidência | Impacto | Correção sugerida |
| --- | --- | --- | --- | --- | --- |
| **RISK-13** | **"Produtos mais vendidos — Este mês"** não filtra o mês: agrega todo o histórico de pedidos não cancelados. | **Médio** | `dashboard_controller.cs:15-26` vs. `Views/Dashboard/index.cshtml:114` | Informação exibida está incorreta em relação ao rótulo. | Aplicar o mesmo filtro de ano/mês de RN-44. |
| **RISK-14** | **Gráfico "Últimos 7 dias" é falso:** barras com alturas hardcoded no CSS inline. | **Médio** | `Views/Dashboard/index.cshtml:85-102` | Usuário recebe dado fictício como se fosse real. | Calcular a série dos últimos 7 dias a partir de `PedidosController.TodosOsPedidos`. |
| **RISK-15** | **`TempData` não é exibido nas telas de Estoque.** As mensagens são gravadas e perdidas no redirect para `Index`. | **Médio** | `estoque_controller.cs:71, 108, 138` vs. `Views/Estoque/Index.cshtml` (sem bloco de alerta) | Usuário não recebe confirmação de que a movimentação foi realizada. | Adicionar o bloco de `TempData["Mensagem"]`/`["Erro"]` ao `Estoque/Index.cshtml`. |
| **RISK-16** | **Não existe matriz de transição de status**; é possível reverter um pedido "Entregue" para "Pendente" sem efeito no estoque. | **Médio** | `pedidos_controller.cs:176-207` | Fluxos de negócio não previstos pelo usuário podem produzir estoque inconsistente. | Definir e implementar a máquina de estados (ex.: `Entregue` e `Cancelado` são terminais). |
| **RISK-17** | **`Produto.Categoria` não é validada no servidor** — apenas popula o `<select>`; um POST manipulado aceita categoria inexistente. | **Médio** | `produtos_controller.cs:79-91` (sem verificação), `:148-151` | Cria categoria-fantasma que aparece nas listagens e quebra RN-14/RN-15. | Validar a existência da categoria no controller antes de gravar. |
| **RISK-18** | **Preço do item é re-snapshotado na edição do pedido**; o valor histórico muda se o preço do produto mudar. | **Baixo** | `pedidos_controller.cs:377-379` | Margem histórica do pedido não é preservada. | Preservar `PrecoUnitario` quando apenas a quantidade muda, ou versionar o preço. |
| **RISK-19** | **`MediaProdutosPorCategoria`** divide o total de produtos por categorias **em uso**, ignorando produtos em categorias vazias. | **Baixo** | `categorias_controller.cs:65-67` | Métrica não representa a média real. | Dividir pelo total de categorias, ou documentar que a métrica é "por categoria em uso". |
| **RISK-20** | **Dados de exemplo apresentados como reais.** Os cards mostram CNPJ/endereço fictícios com aparência de cadastro válido enquanto `_dadosDeExemplo` for `true`. | **Baixo** | `configuracoes_controller.cs:21, 54-59` | Risco de o usuário usar em documento fiscal dados fictícios. | Marcar visualmente como "exemplo" também nos cards (além do alerta). |
| **RISK-21** | **"Estoque baixo" (RN-08) inclui produtos esgotados**, enquanto o badge deles diz "Esgotado" — o mesmo produto aparece nas duas categorias. | **Baixo** | `produtos_controller.cs:59` vs. `produto.cs:26-27` | KPI superestima o alerta de reposição. | Definir se esgotados devem contar como "estoque baixo". |

### 7.4 Dívida técnica e manutenibilidade

| ID | Risco | Sev. | Evidência | Impacto | Correção sugerida |
| --- | --- | --- | --- | --- | --- |
| **RISK-22** | **Regras de negócio ficam nos controllers**, que também servem de repositório. Não há camada de serviço nem testes unitários. | **Médio** | `Controllers/*.cs` | Alterações de regra exigem mexer no controller e não são verificáveis automaticamente. | Extrair serviços (`ProdutoService`, `PedidoService`, `EstoqueService`) e adicionar testes. |
| **RISK-23** | **Ação `Promocoes` sem implementação** e acessível pelo menu, entregando uma tela vazia ao usuário. | **Baixo** | `promocoes_controller.cs`, `Views/Promocoes/Index.cshtml` | Perda de credibilidade na interface. | Implementar ou remover o item do menu. |
| **RISK-24** | **Busca por código numérico só funciona por igualdade exata** (RN-40); não há busca parcial nem por status. | **Baixo** | `pedidos_controller.cs:103-111` | Usuário não consegue localizar pedidos por prefixo nem filtrar por status. | Ampliar a busca e adicionar filtro de status. |
| **RISK-25** | **Sem ordenação configurável**, sem ordenação por coluna, sem filtros avançados nas listagens. | **Baixo** | 4 controllers | Limitação de usabilidade em listas grandes. | Adicionar parâmetros de ordenação/filtro aos ViewModels. |
| **RISK-26** | **Nenhum teste, README ou pipeline de CI**; a solução contém apenas o projeto web. | **Médio** | `projeto_estoque.sln` | Regressões não detectadas; ausência de documentação de setup. | Adicionar projeto de testes, README com instruções e CI. |
| **RISK-27** | **`ordenados`/`ItensPorPagina` duplicados** em 4 controllers com lógica idêntica de paginação e busca. | **Baixo** | `produtos_controller.cs:34-54`, `categorias_controller.cs:24-44`, `estoque_controller.cs:12-32`, `pedidos_controller.cs:97-124` | Correções precisam ser replicadas em 4 lugares. | Extrair um helper de paginação/busca genérico. |
| **RISK-28** | **CSS inline no markup do Dashboard** para o gráfico falso (RISK-14) e ausência de CSS para o layout mobile do gráfico. | **Baixo** | `Views/Dashboard/index.cshtml:87-93` | Manutenção difícil e inconsistência visual. | Mover o gráfico para `dashboard.css` com dados reais. |

---

## 8. Recomendações priorizadas

### P0 — Antes de qualquer uso real

| # | Ação | Resolve |
| --- | --- | --- |
| 1 | **Implementar autenticação real** com cookie/sessão, `[Authorize]` global e tela de logout. | RISK-01, RISK-03 |
| 2 | **Remover a credencial do código** e persistir usuários com hash de senha. | RISK-02 |
| 3 | **Introduzir banco de dados** (EF Core + `DbContext`) substituindo as listas estáticas. | RISK-08, RISK-09, RISK-12 |
| 4 | **Bloquear a exclusão de produtos com pedidos vinculados** (em espelho à regra RN-12 já existente para categorias). | RISK-06, RISK-07 |

### P1 — Integridade e correção de regras

| # | Ação | Resolve |
| --- | --- | --- |
| 5 | Envolver criação/edição de pedido em **transação** e usar `FirstOrDefault` com tratamento em todos os laços. | RISK-10 |
| 6 | **Validar a categoria no servidor** antes de gravar o produto. | RISK-17 |
| 7 | **Exibir `TempData`** nas telas de Estoque. | RISK-15 |
| 8 | Definir e implementar a **máquina de estados de status do pedido** (`Entregue`/`Cancelado` como terminais). | RISK-16 |
| 9 | **Remover a edição direta de `Quantidade`** na tela de Produtos, ou exigir um ajuste registrado. | RISK-11 |
| 10 | **Corrigir o Dashboard**: filtro de mês em "mais vendidos" e gráfico real dos últimos 7 dias. | RISK-13, RISK-14, RISK-28 |
| 11 | Adicionar `[ValidateAntiForgeryToken]` no login e **reativar o HTTPS**. | RISK-04, RISK-05 |

### P2 — Qualidade e evolução

| # | Ação | Resolve |
| --- | --- | --- |
| 12 | **Extrair camada de serviço** e criar **testes unitários** para RN-05, RN-10, RN-12, RN-18, RN-28, RN-30, RN-32, RN-33 e RN-34. | RISK-22, RISK-26 |
| 13 | **Modelar o histórico de movimentações** de estoque (entrada/saída/ajuste/pedido com usuário, data e quantidade). | RISK-11, RN-22 |
| 14 | **Migrar o vínculo produto↔categoria** de string para chave estrangeira (`CategoriaId`). | RN-13, RISK-17 |
| 15 | Implementar ou **remover o módulo Promoções** do menu. | RISK-23 |
| 16 | Ampliar **busca e filtros** (status de pedido, ordenação por coluna). | RISK-24, RISK-25 |
| 17 | **Extrair helper de paginação/busca** compartilhado entre os 4 controllers. | RISK-27 |
| 18 | Adicionar **README** com setup e um **pipeline de CI**. | RISK-26 |

---

### Observações finais

- Este documento descreve **o que o código faz hoje**, incluindo comportamento divergente do desejado. Onde há divergência, ela está registrada como risco na seção 7.
- As regras de negócio dos pedidos (seção 4.4) são as de maior complexidade e concentram **10 dos 28 riscos** catalogados. Priorizam-se testes unitários sobre elas.
- Os identificadores `RF-xx`, `RN-xx` e `RISK-xx` são estáveis e devem ser preservados em-PRs subsequentes para permitir auditoria cruzada.