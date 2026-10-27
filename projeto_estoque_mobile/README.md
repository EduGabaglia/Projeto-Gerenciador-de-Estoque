# Super Atacadão — App Mobile

Aplicativo Expo/React Native do sistema de estoque. Consome a API ASP.NET Core
em tempo real para autenticação, catálogo, carrinho e pedidos.

## Funcionalidades

- Cadastro, login e sessão persistente com JWT.
- Perfil, alteração de senha e exclusão lógica da conta.
- Catálogo com busca, categorias, paginação e produtos em promoção.
- Início com uma faixa de produtos em destaque e, logo abaixo, todos os produtos.
- Carrinho com limite de estoque e preço promocional.
- Pedido online vinculado à conta autenticada.
- Histórico, acompanhamento e cancelamento de pedidos pendentes.
- Cache local do perfil e do histórico por usuário. Pedidos **não** são
  enfileirados offline: é necessário internet para finalizar uma compra.

## Pré-requisitos

- Node.js 22.13 ou superior.
- API `APIs/projeto_estoque_api` configurada e em execução.
- SQL Server com o banco do projeto.

## Configuração da API

Na pasta `APIs/projeto_estoque_api`, configure os segredos locais (eles não vão
para o Git):

```powershell
dotnet user-secrets set "ConnectionStrings:EstoqueDb" "<sua connection string>"
dotnet user-secrets set "Jwt:Key" "<uma chave com pelo menos 32 bytes>"
```

Depois, inicie a API aceitando conexões da rede local (necessário para testar
em um celular físico):

```powershell
dotnet run --launch-profile http-rede
```

A API fica em `http://0.0.0.0:5080`. Confirme com
`http://localhost:5080/health`.

> O profile padrão `http` usa `http://localhost:5080`. Use `http-rede` para
> acessar a API por outro dispositivo na mesma rede.

## Configuração do app

1. Instale as dependências:

   ```bash
   npm install
   ```

2. Crie o arquivo de ambiente local:

   ```bash
   cp .env.example .env.local
   ```

3. Ajuste `EXPO_PUBLIC_API_URL` em `.env.local`:

   | Ambiente | URL |
   | --- | --- |
   | Celular físico | `http://<IP-do-computador>:5080` |
   | Emulador Android | `http://10.0.2.2:5080` |
   | Simulador iOS / web | `http://localhost:5080` |

   Não inclua `/api` na URL; o app adiciona o prefixo automaticamente.
   O celular e o computador precisam estar na mesma rede, e a porta 5080 deve
   estar liberada no firewall.

4. Inicie o Expo:

   ```bash
   npm start
   ```

   Atalhos: `npm run android`, `npm run ios` ou `npm run web`.

Sem `EXPO_PUBLIC_API_URL`, o app tenta descobrir o IP do servidor Expo em
desenvolvimento e usa a porta 5080. Em builds de produção, configure a variável
explicitamente com uma URL HTTPS.

> `EXPO_PUBLIC_*` é embutido no bundle e visível no aplicativo. Nunca coloque
> segredos nessas variáveis. O arquivo `.env.local` está no `.gitignore`.

## Scripts

```bash
npm start          # servidor de desenvolvimento Expo
npm run android    # abre no Android
npm run ios        # abre no iOS
npm run web        # abre a versão web
npx tsc --noEmit   # verificação de tipos
npx eslint src --ext .ts,.tsx
```

## Estrutura

- `src/app/`: rotas do Expo Router.
  - `(auth)/`: login e cadastro.
  - `(tabs)/`: início, categorias, carrinho e perfil.
  - `categoria/[id].tsx`: produtos de uma categoria.
  - `pedido/[id].tsx` e `compras.tsx`: histórico e detalhe do pedido.
  - `conta.tsx`: dados pessoais, senha e exclusão da conta.
- `src/contexts/`: sessão, carrinho e pedidos.
- `src/services/`: cliente HTTP, armazenamento e contratos da API.
- `src/data/`: tipos e regras de apresentação.
- `src/components/`: componentes compartilhados.

## API utilizada

| Método | Rota | Autenticação |
| --- | --- | --- |
| `POST` | `/api/auth/cadastro` | pública |
| `POST` | `/api/auth/login` | pública |
| `GET` | `/api/auth/me` | JWT |
| `PUT` | `/api/auth/perfil` | JWT |
| `PUT` | `/api/auth/senha` | JWT |
| `DELETE` | `/api/auth/conta` | JWT |
| `GET` | `/api/produtos` | pública |
| `GET` | `/api/categorias` | pública |
| `POST` | `/api/pedidos` | JWT |
| `GET` | `/api/pedidos/meus` | JWT |
| `GET` | `/api/pedidos/{id}` | JWT (somente do próprio usuário) |
| `PATCH` | `/api/pedidos/{id}/status` | JWT (somente `CANCELADO` e pedido pendente) |

O painel web continua usando os endpoints anônimos existentes. As rotas de
pedido do app exigem JWT e limitam o acesso aos pedidos do usuário autenticado.
