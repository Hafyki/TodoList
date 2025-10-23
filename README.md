# TodoList — Desafio Técnico (C# + SAPUI5)

**Stack:** ASP.NET Core (C#, EF Core) + SAPUI5 (sap.m)  
**Projeto:** `TodoList.csproj`  
**SAPUI5 namespace (front):** `com.todolist`  
**Frontend porta:** `8080`  
**Backend URLs (launchSettings):** `https://localhost:7210` e `http://localhost:5128`  
**ConnectionString padrão:** `Server=localhost\\SQLEXPRESS01;Database=ToDoListDb;Trusted_Connection=True;TrustServerCertificate=True`

---

## Visão geral
Aplicação full-stack que lista tarefas (`todos`) com:

- busca (filtro por título, debounce no front),
- paginação,
- ordenação,
- visualizar detalhes,
- atualizar `completed` (PUT),
- sincronizar dados de `https://jsonplaceholder.typicode.com/todos` via `POST /api/sync`,
- regra de negócio: **cada usuário (`userId`) só pode ter no máximo 5 tarefas incompletas**.

Resposta paginada padrão:

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 57,
  "totalPages": 6,
  "items": [ { "id":1, "title":"...", "userId":1, "isCompleted": false, ... } ]
}
```

---

## Requisitos mínimos implementados

**Backend**

- Endpoints REST: `GET /api/todos`, `GET /api/todos/{id}`, `PUT /api/todos/{id}`, `POST /api/sync`
- Filtros/paginação/ordenação via query params
- Regra de negócio (max 5 tarefas incompletas por `userId`) retornando `400` com JSON `{ "message": "..." }`
- Persistência via EF Core (SQLite/SQL Server em dev; InMemory para testes)
- ⚠️IMPORTANTE: No README existem menções sobre os Testes, mas eles NÃO foram implementados dentro do prazo definido do desafio e por isso não foram adicionados ao repositório remoto.
- Testes de integração (xUnit) cobrindo: 
  - Filtros e paginação
  - Regra de negócio (5 incompletas)
  - Comportamento de endpoints `GET` e `PUT`

**Frontend (SAPUI5)**

- Lista de tarefas exibindo `title`, `userId`, `completed` (checkbox bound), botão de detalhes(FUNCIONALIDADE NÃO TERMINADA)
- Pesquisa com **debounce** e filtro em tempo real (faz query ao backend)
- Paginação (padrão 10 itens por página) com botões anterior/próximo e seletor de tamanho
- Detail view (rota) exibindo todos os campos de uma tarefa
- BusyIndicator durante carregamento; estilização visual para concluídas (ex.: cinza/tachado)
- Binding: `todoModel` (JSONModel) compartilhado, tabela binds a `/items` ou raiz dependendo da model shape

---

## Visão geral da API (Backend)

### Endpoints

**GET `/api/todos`**  
Suporta query params:

- `pageNumber` ou `page` — número da página (default `1`)  
- `pageSize` — tamanho da página (default `10`)  
- `title` — filtro por título (contains, case-insensitive)  
- `sortBy` ou `sort` — campo a ordenar (`title`, `id`, `userId`, `completed`)  
- `isDescending` ou `order` — direção: `true/false` ou `asc/desc`

Exemplos:
- `/api/todos?pageNumber=2&pageSize=10&title=expedita&sortBy=title&isDescending=false`
- `/api/todos?page=1&pageSize=10&sort=title&order=asc`

**Resposta (JSON paginado)**

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 57,
  "totalPages": 6,
  "items": [
    {
      "id": 1,
      "title": "delectus aut autem",
      "userId": 1,
      "isCompleted": false,
      "user": { "id": 1, "name": "João" }
    }
  ]
}
```

**GET `/api/todos/{id}`**  
Retorna 200 + objeto `ToDo` (com `User` via `Include`) ou 404 se não encontrado.

**PUT `/api/todos/{id}`**  
Alterna/atualiza `completed` do item.  
- Se a regra de negócio é violada (usuário já tem 5 incompletas e a ação tornaria mais uma incompleta), retorna `400` com JSON `{ "message": "..." }`.

**POST `/api/sync`**  
Busca dados de `https://jsonplaceholder.typicode.com/todos` e persiste no banco local (pode sobrescrever, mesclar, ou inserir conforme implementação). Endpoint retorna texto ou JSON com resumo da operação.

---

## Persistência e providers

- Projeto configurado para usar **SQL Server (padrão)** via connection string:

```
Server=localhost\\SQLEXPRESS01;Database=ToDoListDb;Trusted_Connection=True;TrustServerCertificate=True
```

- Para testes de integração a configuração usa **InMemory** para isolamento.  
- Migrations: use `dotnet ef migrations add <Name>` e `dotnet ef database update` quando usar providers baseados em arquivo/SQL.
- Nos testes de integração, a fábrica de testes (CustomWebApplicationFactory) cria banco InMemory e faz `EnsureDeleted()` + `EnsureCreated()` para isolamento.

---

## Frontend (SAPUI5)

**Funcionalidades implementadas**

- Tela principal: tabela `sap.m.Table` com colunas `Título`, `ID`, `Usuário`, `Status` (checkbox).
- Pesquisa `sap.m.SearchField` com debounce (500 ms) que chama backend `?title=...`.
- Paginação com `Select` de pageSize, botões `Anterior` / `Próxima` e indicador `Página X de Y`.
- Ordenação por campo + direção (`sortBy` + `isDescending`) integradas à query string do backend.
- Detail view (rota) que exibe todos os campos de uma tarefa.
- BusyIndicator e feedbacks (MessageToast / MessageBox).
- Binding: `todoModel` (JSONModel) compartilhado, tabela binds a `/items` ou raiz dependendo do formato retornado.

**Como rodar (resumo)**

- Instalar UI5 CLI (recomendado): `npm i -g @ui5/cli`  
- : `ui5 init`
- Para iniciar a aplicação: `npm start` ou `ui5 serve -o index.html`

---

## Executando localmente (passo a passo)

### Pré-requisitos

- .NET SDK (net8.0/9.0 conforme projeto) — `dotnet --version`  
- Node.js + npm (para UI5 tooling) — `node -v`, `npm -v`  
- SQL Server Express (ou ajuste `ConnectionStrings` para SQLite)  
- (Opcional) UI5 CLI: `npm i -g @ui5/cli`

### Backend

1. Abra terminal na pasta do projeto backend (onde está `TodoList.csproj`).
2. Restaurar e build:
   ```bash
   dotnet restore
   dotnet build
   ```

3. Configurar provider (se necessário):
   `appsettings.json` contém a connection string para SQL Server; para usar SQLite, ajuste `Program.cs` / `appsettings`.

4. Migrations (se usar SQLite/SQL Server):
   ```bash
   dotnet tool install --global dotnet-ef   # se necessário
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. Rodar a API:
   ```bash
   dotnet run --project TodoList.csproj
   ```
   A API ficará disponível em `https://localhost:7210` e `http://localhost:5128` (conforme `launchSettings.json`).

6. **Sincronizar dados** (opcional):
   ```bash
   curl -X POST https://localhost:7210/api/sync
   ```

### Frontend (SAPUI5)

1. Dentro da pasta do webapp:
   ```bash
   npm install
   npm i -g @ui5/cli   # apenas se desejar usar ui5 serve global
   ```

2. Rodar (ui5 serve):
   ```bash
   ui5 serve --open index.html --port 8080
   ```
3. Abra: `http://localhost:8080`

> Certifique-se que o frontend está apontando para o endpoint correto do backend (`http://localhost:5128` ou `https://localhost:7210`) ao construir URLs nas chamadas `fetch`.

---

## Testes de Integração (xUnit)

### Executar testes
Na raiz da solution (onde está `.sln`):
```bash
dotnet test
```

### Notas importantes para testes funcionar corretamente

- O `WebApplicationFactory` deve apontar para o `Program` do projeto SUT (API). Em projetos .NET minimal, `Program` é gerado implicitamente; exponha-o ao assembly de testes com `InternalsVisibleTo`:
  ```xml
  <ItemGroup>
    <InternalsVisibleTo Include="TodoList.IntegrationTests" />
  </ItemGroup>
  ```
  (adicione ao `.csproj` do backend se necessário)
- Para evitar conflito de providers EF (SqlServer vs InMemory), condicione a configuração do DbContext por ambiente em `Program.cs`:
  ```csharp
  if (builder.Environment.IsEnvironment("IntegrationTests"))
      builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseInMemoryDatabase("InMemoryForTests"));
  else
      builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(connectionString));
  ```
  e no `CustomWebApplicationFactory` configure o ambiente:
  ```csharp
  builder.UseEnvironment("IntegrationTests");
  ```
- Geração do `testhost.deps.json`: se ocorrer erro, adicione ao `.csproj` do backend:
  ```xml
  <PropertyGroup>
    <PreserveCompilationContext>true</PreserveCompilationContext>
  </PropertyGroup>
  ```

---

## Especificação de endpoints (resumo)

### GET `/api/todos`
Query params:
- `pageNumber` (default 1)
- `pageSize` (default 10)
- `title` (filtro contains)
- `sortBy` (ex: `title`, `id`, `userId`, `completed`)
- `isDescending` (true/false)

Retorno: paginado (ver exemplo acima).

### GET `/api/todos/{id}`
Retorna objeto ToDo com `user` incluído (quando aplicável). 404 se não existir.

### PUT `/api/todos/{id}`
Atualiza/alternar `IsCompleted`.
Se ação violar regra de negócio (usuario já tem 5 incompletas), retorna:
```json
HTTP 400
{ "message": "O usuário já possui 5 tarefas incompletas. Não é possível marcar mais tarefas como pendentes." }
```

### POST `/api/sync`
Dispara sync dos dados de `https://jsonplaceholder.typicode.com/todos` para o banco local. Retorna texto ou JSON resumo.

---

## Exemplos (curl)

/api todos página 1:
```bash
curl "http://localhost:5128/api/todos?pageNumber=1&pageSize=10"
```

Filtro + ordenação:
```bash
curl "http://localhost:5128/api/todos?title=expedita&sortBy=title&isDescending=false"
```

Get by id:
```bash
curl "http://localhost:5128/api/todos/12"
```

Toggle completed:
```bash
curl -X PUT "http://localhost:5128/api/todos/12"
```

Sync:
```bash
curl -X POST "http://localhost:5128/api/sync"
```

---

## Boas práticas e debugging (dicas rápidas)

- **Erros de leitura do body**: leia `response.Text()` apenas **uma vez** e parse manualmente, porque o stream só pode ser lido uma vez.
- **Padronize erros**: sempre retorne JSON `{ "message": "..." }` para erros de regra de negócio — facilita frontend e testes.
- **Seed consistente nos testes**: use `factory.Services.CreateScope()` para obter `ApplicationDbContext` do host de teste e `EnsureDeleted()` + `EnsureCreated()` antes de inserir dados.
- **Isolamento InMemory**: use um banco em memória por ambiente de teste (ou limpar entre testes) para evitar flakiness.
- **CORS**: habilite para `http://localhost:8080` enquanto desenvolve o front separado do back.
- **Logs**: mirar logs do ASP.NET Core (Developer Exception Page) para rastrear 404/500 durante testes.

---

## Arquitetura front (SAPUI5)

- `Component.js`, `manifest.json` para config e routing.
- `view/` (XML) + `controller/` (JS) separação.
- `model/` (JSONModel) nomeado `todoModel`.
- Router para navegar para `Detail` view.
- Bindings: `items="{todoModel>/items}"` (ou `todoModel>/` dependendo do formato retornado).
- SearchField com debounce (500 ms) chamando backend via query param `title`.

---

## Entrega
- Repositório público no GitHub contendo:
  - `TodoList/` (backend)
  - `webapp/` (frontend SAPUI5)
  - `TodoList.IntegrationTests/`
  - `README.md` com instruções (este arquivo)
- Incluir exemplos de execução (comandos), como rodar testes e como popular/limpar DB.

---

## Referências
- SAPUI5 Docs: https://sapui5.hana.ondemand.com
- Vídeo SAPUI5: https://www.youtube.com/watch?v=sdnpmgfbYAk
- JSON base: https://jsonplaceholder.typicode.com/todos

