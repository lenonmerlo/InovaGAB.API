# InovaGAB API

> **FIAP – Challenge 2025 | Grupo Águia Branca**

API RESTful desenvolvida como solução para o desafio proposto pela FIAP, com foco em gestão de inovação corporativa. A plataforma permite que colaboradores submetam ideias, gestores as avaliem e promovam a projetos, líderes acompanhem indicadores estratégicos e toda a organização participe de desafios internos de inovação.

---

## Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Modelos de Domínio](#-modelos-de-domínio)
- [Perfis de Usuário (Roles)](#-perfis-de-usuário-roles)
- [Endpoints da API](#-endpoints-da-api)
  - [Auth](#auth)
  - [Ideias](#ideias)
  - [Projetos](#projetos)
  - [Desafios](#desafios)
  - [Diretrizes Estratégicas](#diretrizes-estratégicas)
  - [Dashboard](#dashboard)
- [Autenticação e Autorização](#-autenticação-e-autorização)
- [Configuração do Ambiente](#-configuração-do-ambiente)
- [Executando a Aplicação](#-executando-a-aplicação)
  - [Localmente (Visual Studio / .NET CLI)](#localmente-visual-studio--net-cli)
  - [Com Docker](#com-docker)
- [Banco de Dados e Migrations](#-banco-de-dados-e-migrations)
- [Variáveis de Ambiente / appsettings](#-variáveis-de-ambiente--appsettings)
- [Swagger / Documentação Interativa](#-swagger--documentação-interativa)
- [Auditoria](#-auditoria)
- [Estrutura de Pastas](#-estrutura-de-pastas)
- [Grupo Águia Branca](#-grupo-águia-branca)

---

## Para Avaliadores

O banco de dados está hospedado no **Supabase (PostgreSQL em nuvem)** — região São Paulo.
As credenciais de acesso não são versionadas por segurança.

Para executar o projeto, configure as seguintes variáveis de ambiente antes de rodar o Docker:

| Variável | Descrição |
|---|---|
| `ConnectionStrings__DefaultConnection` | String de conexão Supabase (solicitar ao autor) |
| `Jwt__Key` | Chave secreta JWT (solicitar ao autor) |
| `Jwt__Issuer` | `InovaGAB.API` |
| `Jwt__Audience` | `InovaGAB.App` |

**Contato:** lenontm@gmail.com

> As migrations são aplicadas automaticamente na inicialização — não é necessário rodar `dotnet ef database update`.

## Visão Geral

O **InovaGAB** é um sistema de gestão de inovação interno para empresas. Seu ciclo principal é:

```
Operador submete ideia
        ↓
Gestor avalia e aprova/rejeita
        ↓
Ideia aprovada vira Projeto
        ↓
Projeto é monitorado (ROI, investimento, ganho de produtividade)
        ↓
Liderança acompanha métricas no Dashboard
```

Além disso, gestores e líderes podem criar **Desafios** temáticos para incentivar a submissão de ideias em áreas específicas, e **Diretrizes Estratégicas** para alinhar as iniciativas com a estratégia da organização.

---

## Arquitetura

```
InovaGAB.API/
├── Controllers/         # Camada de apresentação (endpoints HTTP)
├── Services/
│   ├── Interfaces/      # Contratos de serviço
│   └── Implementations/ # Lógica de negócio
├── DTOs/
│   ├── Request/         # Payloads de entrada
│   └── Response/        # Modelos de saída
├── Models/              # Entidades do domínio (EF Core)
├── Data/                # AppDbContext
├── Middleware/          # Auditoria de requisições
└── Migrations/          # Histórico de migrações EF Core
```

A aplicação segue o padrão **Controller → Service → Repository (EF Core)**, com separação clara entre DTOs e entidades de domínio.

---

## Tecnologias

| Tecnologia | Versão | Função |
|---|---|---|
| **.NET** | 9.0 | Plataforma base |
| **ASP.NET Core** | 9.0 | Framework Web / API |
| **Entity Framework Core** | 9.0.4 | ORM / acesso a dados |
| **Npgsql (EF Core)** | 9.0.4 | Provider PostgreSQL |
| **PostgreSQL** | — | Banco de dados relacional |
| **JWT Bearer** | 9.0.4 | Autenticação via token |
| **BCrypt.Net-Next** | 4.1.0 | Hash seguro de senhas |
| **Swashbuckle (Swagger)** | 6.9.0 | Documentação interativa da API |
| **Docker** | — | Containerização (Linux) |

---

## Modelos de Domínio

### `User` — Usuário
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Name` | string | Nome completo |
| `Email` | string | E-mail (usado no login) |
| `PasswordHash` | string | Senha hasheada com BCrypt |
| `Role` | UserRole | Perfil: `Operator`, `Manager`, `Leader` |
| `Division` | string | Divisão/área da empresa |
| `Points` | int | Pontuação acumulada por contribuições |
| `CreatedAt` | DateTime | Data de criação |

---

### `Idea` — Ideia
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Title` | string | Título da ideia |
| `Description` | string | Descrição detalhada |
| `Division` | string | Divisão de origem |
| `Status` | IdeaStatus | `Submitted`, `UnderReview`, `Approved`, `Rejected` |
| `ImpactScore` | int | Nota de impacto (0–10) |
| `FeasibilityScore` | int | Nota de viabilidade (0–10) |
| `AlignmentScore` | int | Nota de alinhamento estratégico (0–10) |
| `TotalScore` | int | Média calculada dos três scores |
| `EvidenceUrl` | string? | URL de evidência/anexo |
| `ChallengeId` | int? | Desafio vinculado (opcional) |
| `UserId` | int | Autor da ideia |

---

### `Project` — Projeto
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Title` | string | Título do projeto |
| `Division` | string | Divisão responsável |
| `Status` | ProjectStatus | `Planning`, `InProgress`, `Completed`, `Cancelled` |
| `Stage` | string | Etapa atual |
| `Investment` | decimal | Investimento total |
| `FinancialReturn` | decimal | Retorno financeiro esperado |
| `Roi` | decimal | ROI calculado automaticamente |
| `ProductivityGain` | decimal | Ganho de produtividade (%) |
| `ManagerId` | int | Gestor responsável |
| `IdeaId` | int? | Ideia de origem |

---

### `Challenge` — Desafio
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Title` | string | Título do desafio |
| `Description` | string | Descrição e critérios |
| `Prize` | decimal | Prêmio oferecido |
| `StartDate` | DateTime | Início das submissões |
| `EndDate` | DateTime | Prazo final |
| `IsActive` | bool | Desafio aberto para submissões |
| `CreatedById` | int | Usuário que criou o desafio |

---

### `StrategicGuideline` — Diretriz Estratégica
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Title` | string | Título da diretriz |
| `Description` | string | Detalhamento |
| `Priority` | string | Prioridade (ex.: Alta, Média, Baixa) |
| `Category` | string | Categoria temática |
| `CreatedById` | int | Usuário responsável |

---

### `AuditLog` — Log de Auditoria
| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | int | Identificador único |
| `Method` | string | Método HTTP (GET, POST...) |
| `Endpoint` | string | Rota acessada |
| `UserEmail` | string | E-mail do usuário autenticado |
| `UserRole` | string | Role do usuário no momento |
| `StatusCode` | int | Código de resposta HTTP |
| `DurationMs` | long | Tempo de execução em ms |
| `Timestamp` | DateTime | Momento do acesso |

---

## Perfis de Usuário (Roles)

| Role | Nome | Permissões |
|---|---|---|
| `Operator` | Operador | Submeter ideias, visualizar próprias ideias |
| `Manager` | Gestor | Aprovar/rejeitar ideias, criar e gerenciar projetos, criar desafios e diretrizes |
| `Leader` | Liderança | Acesso total de leitura, dashboard executivo, visualização de todos os projetos e ideias |

---

## Endpoints da API

> Todos os endpoints (exceto `/api/auth/register` e `/api/auth/login`) requerem o header:
> ```
> Authorization: Bearer {seu_token_jwt}
> ```

---

### Auth

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| `POST` | `/api/auth/register` | Público | Cadastro de novo usuário |
| `POST` | `/api/auth/login` | Público | Login e geração do token JWT |

#### `POST /api/auth/register`
```json
{
  "name": "João Silva",
  "email": "joao@empresa.com",
  "password": "Senha@123",
  "role": 0,
  "division": "Tecnologia"
}
```
> `role`: `0` = Operator, `1` = Manager, `2` = Leader

#### `POST /api/auth/login`
```json
{
  "email": "joao@empresa.com",
  "password": "Senha@123"
}
```
**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "name": "João Silva",
  "role": "Operator"
}
```

---

### Ideias

| Método | Rota | Role | Descrição |
|---|---|---|---|
| `POST` | `/api/idea` | Operator | Submeter nova ideia |
| `GET` | `/api/idea/my` | Operator | Listar próprias ideias |
| `GET` | `/api/idea` | Manager, Leader | Listar todas as ideias |
| `PATCH` | `/api/idea/{id}/approve` | Manager | Aprovar ideia com scores |
| `PATCH` | `/api/idea/{id}/reject` | Manager | Rejeitar ideia |

#### `POST /api/idea`
```json
{
  "title": "Automação do processo X",
  "description": "Reduzir tempo manual usando RPA...",
  "division": "Operações",
  "evidenceUrl": "https://docs.empresa.com/evidencia",
  "challengeId": 1
}
```

#### `PATCH /api/idea/{id}/approve`
```json
{
  "impactScore": 8,
  "feasibilityScore": 7,
  "alignmentScore": 9
}
```

---

### Projetos

| Método | Rota | Role | Descrição |
|---|---|---|---|
| `POST` | `/api/project` | Manager | Criar projeto a partir de uma ideia |
| `GET` | `/api/project` | Manager, Leader | Listar todos os projetos |
| `GET` | `/api/project/{id}` | Manager, Leader | Detalhar um projeto |
| `PUT` | `/api/project/{id}` | Manager | Atualizar dados do projeto |
| `DELETE` | `/api/project/{id}` | Manager | Remover projeto |

#### `POST /api/project`
```json
{
  "title": "Projeto RPA Operações",
  "division": "Operações",
  "stage": "Planejamento",
  "investment": 50000.00,
  "financialReturn": 150000.00,
  "productivityGain": 30.5,
  "ideaId": 12
}
```

---

### Desafios

| Método | Rota | Role | Descrição |
|---|---|---|---|
| `POST` | `/api/challenge` | Manager, Leader | Criar novo desafio |
| `GET` | `/api/challenge` | Todos | Listar desafios ativos |
| `GET` | `/api/challenge/{id}` | Todos | Detalhar desafio e suas ideias |
| `PUT` | `/api/challenge/{id}` | Manager, Leader | Atualizar desafio |
| `DELETE` | `/api/challenge/{id}` | Manager, Leader | Remover desafio |

#### `POST /api/challenge`
```json
{
  "title": "Sustentabilidade 2025",
  "description": "Ideias para redução de emissão de carbono",
  "prize": 5000.00,
  "startDate": "2025-06-01T00:00:00Z",
  "endDate": "2025-08-31T23:59:59Z"
}
```

---

### Diretrizes Estratégicas

| Método | Rota | Role | Descrição |
|---|---|---|---|
| `POST` | `/api/guideline` | Manager, Leader | Criar diretriz |
| `GET` | `/api/guideline` | Todos | Listar diretrizes |
| `GET` | `/api/guideline/{id}` | Todos | Detalhar diretriz |
| `PUT` | `/api/guideline/{id}` | Manager, Leader | Atualizar diretriz |
| `DELETE` | `/api/guideline/{id}` | Manager, Leader | Remover diretriz |

#### `POST /api/guideline`
```json
{
  "title": "Transformação Digital",
  "description": "Acelerar a digitalização dos processos internos",
  "priority": "Alta",
  "category": "Tecnologia"
}
```

---

### Dashboard

| Método | Rota | Role | Descrição |
|---|---|---|---|
| `GET` | `/api/dashboard` | Manager, Leader | Métricas executivas consolidadas |

**Resposta:**
```json
{
  "totalRoi": 320000.00,
  "totalSavings": 180000.00,
  "productivityGainAverage": 24.5,
  "activeProjects": 8,
  "delayedProjects": 2,
  "ideaFunnel": {
    "submitted": 45,
    "underReview": 12,
    "approved": 20,
    "rejected": 13
  },
  "topProjects": [...],
  "topContributors": [...]
}
```

---

## Autenticação e Autorização

A API utiliza **JWT (JSON Web Token)** com as seguintes configurações:

- Algoritmo: **HMAC SHA-256**
- Claims incluídos no token: `sub` (userId), `email`, `role`
- Validade configurável via `appsettings.json`
- Senhas armazenadas com **BCrypt** (cost factor padrão)

Para autenticar no Swagger:
1. Faça login em `POST /api/auth/login`
2. Copie o `token` retornado
3. Clique em **Authorize** (canto superior direito do Swagger UI)
4. Insira: `Bearer eyJhbGci...`

---

## Configuração do Ambiente

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/) ou conta no [Supabase](https://supabase.com/)
- [Docker](https://www.docker.com/) *(opcional)*
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)

### `appsettings.json`

Crie ou edite o arquivo `InovaGAB.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=inovagab;Username=postgres;Password=sua_senha"
  },
  "Jwt": {
    "Key": "sua_chave_secreta_com_pelo_menos_32_caracteres",
    "Issuer": "InovaGAB",
    "Audience": "InovaGAB",
    "ExpiresInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Atenção:** Nunca versione

#### Usando Supabase (Connection String)
```
Host=aws-0-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.XXXX;Password=sua_senha;SSL Mode=Require;Trust Server Certificate=true
```

---

## Executando a Aplicação

### Localmente (Visual Studio / .NET CLI)

```bash
# Restaurar pacotes
dotnet restore

# Aplicar migrations ao banco de dados
dotnet ef database update --project InovaGAB.API

# Executar a API
dotnet run --project InovaGAB.API
```

A API estará disponível em:
- `https://localhost:7XXX` (HTTPS)
- `http://localhost:5XXX` (HTTP)

Acesse o Swagger em: `https://localhost:7XXX/swagger`

---

### Com Docker

```bash
# Build da imagem
docker build -t inovagab-api ./InovaGAB.API

# Executar o container
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;..." \
  -e Jwt__Key="sua_chave_secreta" \
  -e Jwt__Issuer="InovaGAB" \
  -e Jwt__Audience="InovaGAB" \
  inovagab-api
```

Ou com **Docker Compose** (se disponível):
```bash
docker-compose up --build
```

Swagger em: `http://localhost:8080/swagger`

> As migrations são aplicadas automaticamente na inicialização via `db.Database.Migrate()`.

---

## Banco de Dados e Migrations

O projeto utiliza **EF Core Migrations** para versionamento do schema.

### Comandos úteis

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration --project InovaGAB.API

# Aplicar migrations pendentes
dotnet ef database update --project InovaGAB.API

# Reverter para migration específica
dotnet ef database update NomeDaMigration --project InovaGAB.API

# Listar migrations
dotnet ef migrations list --project InovaGAB.API
```

### Tabelas do banco

| Tabela | Descrição |
|---|---|
| `Users` | Usuários do sistema |
| `Ideas` | Ideias submetidas |
| `Projects` | Projetos criados a partir de ideias |
| `Challenges` | Desafios de inovação |
| `StrategicGuidelines` | Diretrizes estratégicas |
| `AuditLogs` | Registro de auditoria das requisições |

---

## Variáveis de Ambiente / appsettings

| Chave | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | String de conexão PostgreSQL | `Host=localhost;...` |
| `Jwt__Key` | Chave secreta para assinar tokens JWT (mín. 32 chars) | `minha_chave_super_secreta_2025` |
| `Jwt__Issuer` | Emissor do token | `InovaGAB` |
| `Jwt__Audience` | Audiência do token | `InovaGAB` |
| `Jwt__ExpiresInMinutes` | Tempo de expiração do token | `60` |

---

## Swagger / Documentação Interativa

O Swagger UI está habilitado no ambiente de desenvolvimento:

- **URL**: `/swagger` ou `/swagger/index.html`
- **Especificação JSON**: `/swagger/v1/swagger.json`

Recursos disponíveis no Swagger:
- Documentação de todos os endpoints
- Autenticação via Bearer Token integrada
- Teste de requisições diretamente pelo navegador

---

## Auditoria

Todas as requisições autenticadas são registradas automaticamente na tabela `AuditLogs` via `AuditMiddleware`. São capturados:

- Método HTTP e rota acessada
- E-mail e role do usuário autenticado
- Código de resposta HTTP
- Tempo de execução da requisição (em ms)
- Data/hora da requisição

Rotas excluídas da auditoria: `/swagger`, `/health`, e requisições de usuários não autenticados.

---

## Estrutura de Pastas

```
InovaGAB.API/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── IdeaController.cs
│   ├── ProjectController.cs
│   ├── ChallengeController.cs
│   ├── GuidelineController.cs
│   └── DashboardController.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IIdeaService.cs
│   │   ├── IProjectService.cs
│   │   ├── IChallengeService.cs
│   │   ├── IGuidelineService.cs
│   │   └── IDashboardService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── IdeaService.cs
│       ├── ProjectService.cs
│       ├── ChallengeService.cs
│       ├── GuidelineService.cs
│       └── DashboardService.cs
│
├── DTOs/
│   ├── Request/
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   ├── CreateIdeaRequest.cs
│   │   ├── ApproveIdeaRequest.cs
│   │   ├── CreateProjectRequest.cs
│   │   ├── UpdateProjectRequest.cs
│   │   ├── CreateChallengeRequest.cs
│   │   └── CreateGuidelineRequest.cs
│   └── Response/
│       ├── AuthResponse.cs
│       ├── IdeaResponse.cs
│       ├── ProjectResponse.cs
│       ├── ChallengeResponse.cs
│       ├── GuidelineResponse.cs
│       └── DashboardResponse.cs
│
├── Models/
│   ├── User.cs          # + enum UserRole
│   ├── Idea.cs          # + enum IdeaStatus
│   ├── Project.cs       # + enum ProjectStatus
│   ├── Challenge.cs
│   ├── StrategicGuideline.cs
│   └── AuditLog.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Middleware/
│   └── AuditMiddleware.cs
│
├── Migrations/          # Histórico de migrações EF Core
│
├── Program.cs           # Configuração e startup da aplicação
└── InovaGAB.API.csproj
```

---

## Grupo Águia Branca

Projeto desenvolvido para o **Challenge FIAP 2025**.

| Membro | RM |
|---|---|
| Guilherme Luccas da Costa | RM561735 |
| Lenon Otmar Tonoli Merlo | RM564471 |
| Matheus Henrique Silva Souza | RM561329 |

---

## Licença

Este projeto foi desenvolvido exclusivamente para fins acadêmicos no contexto do Challenge da FIAP.
