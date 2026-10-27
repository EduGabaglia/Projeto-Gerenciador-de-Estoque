# Sistema Gerenciador de Estoque

Sistema gerenciador de estoque desenvolvido para um supermercado, utilizando o padrão arquitetural **MVC (Model-View-Controller)** como base para a construção do software. A utilização do MVC proporciona uma maior organização do código por meio da separação de responsabilidades entre Models, Views e Controllers, facilitando também o desenvolvimento e a manutenção do sistema.

## Funcionalidades

O sistema conta atualmente com as seguintes funcionalidades:

* **Login:** autenticação utilizando as credenciais previamente definidas:

  * Usuário: `Admin`
  * Senha: `@123`
* **Configurações:** permite adicionar e gerenciar algumas informações da empresa.
* **Produtos:** permite cadastrar produtos e visualizar os produtos cadastrados recentemente.
* **Categorias:** permite cadastrar as categorias utilizadas no sistema.
* **Estoque:** permite gerenciar os produtos e suas informações relacionadas ao estoque.
* **Pedidos:** permite gerenciar os pedidos realizados.
* **Promoções:** permite cadastrar promoções com desconto percentual e acompanhar os preços promocionais.
* **Modo noturno:** permite alternar entre os modos de exibição. A preferência selecionada é armazenada para que seja mantida quando o sistema for aberto novamente.

## Estrutura da solução

| Projeto | Descrição |
| --- | --- |
| `projeto_estoque_web` | Painel administrativo ASP.NET Core MVC. |
| `APIs/projeto_estoque_api` | API REST ASP.NET Core com EF Core e SQL Server. |
| `projeto_estoque_mobile` | Aplicativo Expo/React Native do cliente. |

## Como executar

### API

Configure a conexão com o banco e a chave JWT com `dotnet user-secrets` na pasta da API:

```powershell
dotnet user-secrets set "ConnectionStrings:EstoqueDb" "<sua connection string>"
dotnet user-secrets set "Jwt:Key" "<uma chave com pelo menos 32 bytes>"
dotnet run --launch-profile http-rede
```

A API fica disponível em `http://localhost:5080` (ou no IP da máquina para acesso pelo celular). O health check é `GET /health`.

### Painel web

```powershell
cd projeto_estoque_web
dotnet run
```

O painel usa a API configurada em `Api:BaseUrl` (`appsettings.json`) e continua acessando os endpoints anônimos de catálogo e pedidos.

### Aplicativo mobile

Consulte [`projeto_estoque_mobile/README.md`](projeto_estoque_mobile/README.md) para configurar `EXPO_PUBLIC_API_URL`, executar o Expo e testar no celular, emulador ou navegador.

## Autenticação

O app mobile usa JWT com senhas protegidas por PBKDF2-SHA256. As rotas `/api/auth/*` e os endpoints de pedidos do cliente exigem autenticação; os endpoints usados pelo painel web permanecem anônimos para compatibilidade. A exclusão de conta é lógica, preservando o histórico de pedidos.

