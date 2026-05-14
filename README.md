# InovaGAB API

> **FIAP – Challenge 2025 | Grupo Águia Branca**

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=csharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Docker-4169E1?style=flat&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?style=flat&logo=docker&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-UI-85EA2D?style=flat&logo=swagger&logoColor=black)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?style=flat&logo=jsonwebtokens&logoColor=white)
![Build](https://github.com/lenonmerlo/InovaGAB.API/actions/workflows/build.yml/badge.svg)

API RESTful desenvolvida como solução para o desafio proposto pela FIAP, com foco em gestão de inovação corporativa. A plataforma permite que colaboradores submetam ideias, gestores as avaliem e promovam a projetos, líderes acompanhem indicadores estratégicos e toda a organização participe de desafios internos de inovação.

---

## Índice

- [Para Avaliadores](#para-avaliadores)
- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Modelos de Domínio](#modelos-de-domínio)
- [Perfis de Usuário (Roles)](#perfis-de-usuário-roles)
- [Endpoints da API](#endpoints-da-api)
  - [Auth](#auth)
  - [Ideias](#ideias)
  - [Projetos](#projetos)
  - [Desafios](#desafios)
  - [Diretrizes Estratégicas](#diretrizes-estratégicas)
  - [Dashboard](#dashboard)
- [Autenticação e Autorização](#autenticação-e-autorização)
- [Configuração do Ambiente](#configuração-do-ambiente)
- [Executando a Aplicação](#executando-a-aplicação)
- [Banco de Dados e Migrations](#banco-de-dados-e-migrations)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Swagger](#swagger--documentação-interativa)
- [Auditoria](#auditoria)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Equipe](#equipe)

---

## Para Avaliadores

O banco de dados **PostgreSQL** é executado localmente via container Docker.
As credenciais de acesso não são versionadas por segurança.

**Forma mais rápida de rodar:**

```bash
# 1. Clone o repositório
git clone https://github.com/lenonmerlo/InovaGAB.API.git
cd InovaGAB.API

# 2. Copie o arquivo de exemplo e preencha com as credenciais (solicitar ao autor)
cp .env.example .env

# 3. Suba a aplicação
docker-compose up --build
```

Acesse o Swagger em: **http://localhost:8080/swagger**

| Variável      | Descrição                                      |
| ------------- | ---------------------------------------------- |
| `DB_PASSWORD` | Senha do PostgreSQL local (solicitar ao autor) |
| `JWT_KEY`     | Chave secreta JWT (solicitar ao autor)         |

**Contato:** lenontm@gmail.com

> As migrations são aplicadas automaticamente na inicialização — não é necessário rodar `dotnet ef database update`.

---

## Visão Geral

O **InovaGAB** é um sistema de gestão de inovação interno para empresas. Seu ciclo principal é:

```
Operador submete ideia
        ↓
Gestor avalia e aprova/rejeita com Score de Impacto
        ↓
Ideia aprovada vira Projeto
        ↓
Projeto é monitorado (ROI, investimento, ganho de produtividade)
        ↓
Liderança acompanha métricas no Dashboard executivo
```

Além disso, a **Liderança** pode criar **Desafios** temáticos para incentivar a submissão de ideias em áreas específicas, e **Diretrizes Estratégicas** para alinhar as iniciativas com a estratégia da organização.

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

A aplicação segue o padrão **Controller → Service → EF Core**, com separação clara entre DTOs e entidades de domínio.

---

## Tecnologias

| Tecnologia                | Versão | Função                          |
| ------------------------- | ------ | ------------------------------- |
| **.NET**                  | 9.0    | Plataforma base                 |
| **ASP.NET Core**          | 9.0    | Framework Web / API             |
| **Entity Framework Core** | 9.0.4  | ORM / acesso a dados            |
| **Npgsql (EF Core)**      | 9.0.4  | Provider PostgreSQL             |
| **PostgreSQL**            | —      | Banco de dados local via Docker |
| **JWT Bearer**            | 9.0.4  | Autenticação via token          |
| **BCrypt.Net-Next**       | 4.1.0  | Hash seguro de senhas           |
| **Swashbuckle (Swagger)** | 6.9.0  | Documentação interativa da API  |
| **Docker**                | —      | Containerização (Linux)         |

---

## Modelos de Domínio

### `User` — Usuário

| Campo          | Tipo     | Descrição                               |
| -------------- | -------- | --------------------------------------- |
| `Id`           | int      | Identificador único                     |
| `Name`         | string   | Nome completo                           |
| `Email`        | string   | E-mail (usado no login)                 |
| `PasswordHash` | string   | Senha hasheada com BCrypt               |
| `Role`         | UserRole | Perfil: `Operator`, `Manager`, `Leader` |
| `Division`     | string   | Divisão/área da empresa                 |
| `Points`       | int      | Pontuação acumulada por contribuições   |
| `CreatedAt`    | DateTime | Data de criação                         |

### `Idea` — Ideia

| Campo              | Tipo       | Descrição                                          |
| ------------------ | ---------- | -------------------------------------------------- |
| `Id`               | int        | Identificador único                                |
| `Title`            | string     | Título da ideia                                    |
| `Description`      | string     | Descrição detalhada                                |
| `Division`         | string     | Divisão de origem                                  |
| `Status`           | IdeaStatus | `Submitted`, `UnderReview`, `Approved`, `Rejected` |
| `ImpactScore`      | int        | Nota de impacto (0–10)                             |
| `FeasibilityScore` | int        | Nota de viabilidade (0–10)                         |
| `AlignmentScore`   | int        | Nota de alinhamento estratégico (0–10)             |
| `TotalScore`       | int        | Média calculada automaticamente                    |
| `EvidenceUrl`      | string?    | URL de evidência/anexo                             |
| `ChallengeId`      | int?       | Desafio vinculado (opcional)                       |
| `UserId`           | int        | Autor da ideia                                     |

### `Project` — Projeto

| Campo              | Tipo          | Descrição                                                    |
| ------------------ | ------------- | ------------------------------------------------------------ |
| `Id`               | int           | Identificador único                                          |
| `Title`            | string        | Título do projeto                                            |
| `Division`         | string        | Divisão responsável                                          |
| `Status`           | ProjectStatus | `Planning`, `InProgress`, `Completed`, `OnHold`, `Cancelled` |
| `Stage`            | ProjectStage  | `Diagnosis`, `Implementation`, `Validation`, `Closure`       |
| `Investment`       | decimal       | Investimento total (R$)                                      |
| `FinancialReturn`  | decimal       | Retorno financeiro (R$)                                      |
| `Roi`              | decimal       | ROI calculado automaticamente (%)                            |
| `ProductivityGain` | int           | Ganho de produtividade (%)                                   |
| `ProgressPercent`  | int           | Progresso geral (0–100)                                      |
| `Deadline`         | DateTime      | Prazo de entrega                                             |
| `ManagerId`        | int           | Gestor responsável                                           |
| `IdeaId`           | int?          | Ideia de origem                                              |

### `Challenge` — Desafio

| Campo           | Tipo     | Descrição                                  |
| --------------- | -------- | ------------------------------------------ |
| `Id`            | int      | Identificador único                        |
| `Title`         | string   | Título do desafio                          |
| `Description`   | string   | Descrição e critérios                      |
| `Prize`         | decimal  | Prêmio oferecido (R$)                      |
| `StartDate`     | DateTime | Início das submissões                      |
| `EndDate`       | DateTime | Prazo final                                |
| `DaysRemaining` | int      | Dias restantes (calculado automaticamente) |
| `IsActive`      | bool     | Desafio aberto para submissões             |
| `CreatedById`   | int      | Usuário que criou o desafio                |

### `StrategicGuideline` — Diretriz Estratégica

| Campo         | Tipo              | Descrição               |
| ------------- | ----------------- | ----------------------- |
| `Id`          | int               | Identificador único     |
| `Title`       | string            | Título da diretriz      |
| `Description` | string            | Detalhamento            |
| `Priority`    | GuidelinePriority | `Low`, `Medium`, `High` |
| `Category`    | string            | Categoria temática      |
| `IsActive`    | bool              | Ativo (soft delete)     |
| `CreatedById` | int               | Usuário responsável     |

### `AuditLog` — Log de Auditoria

| Campo        | Tipo     | Descrição                     |
| ------------ | -------- | ----------------------------- |
| `Id`         | int      | Identificador único           |
| `Method`     | string   | Método HTTP (GET, POST...)    |
| `Endpoint`   | string   | Rota acessada                 |
| `UserEmail`  | string   | E-mail do usuário autenticado |
| `UserRole`   | string   | Role do usuário no momento    |
| `StatusCode` | int      | Código de resposta HTTP       |
| `DurationMs` | long     | Tempo de execução em ms       |
| `CreatedAt`  | DateTime | Momento do acesso             |

---

## Perfis de Usuário (Roles)

| Role       | Nome      | Permissões                                                                  |
| ---------- | --------- | --------------------------------------------------------------------------- |
| `Operator` | Operador  | Submeter ideias, visualizar próprias ideias, ver diretrizes e desafios      |
| `Manager`  | Gestor    | Aprovar/rejeitar ideias, criar e gerenciar projetos, ver todas as ideias    |
| `Leader`   | Liderança | Dashboard executivo, CRUD de diretrizes e desafios, visão total de projetos |

---

## Endpoints da API

> Todos os endpoints (exceto `/api/Auth/register` e `/api/Auth/login`) requerem o header:
>
> ```
> Authorization: Bearer {seu_token_jwt}
> ```

### Auth

| Método | Rota                 | Acesso  | Descrição                    |
| ------ | -------------------- | ------- | ---------------------------- |
| `POST` | `/api/Auth/register` | Público | Cadastro de novo usuário     |
| `POST` | `/api/Auth/login`    | Público | Login e geração do token JWT |

#### `POST /api/Auth/register`

```json
{
  "name": "João Silva",
  "email": "joao@empresa.com",
  "password": "Senha@123",
  "role": "0",
  "division": "Tecnologia"
}
```

> `role`: `"0"` = Operator, `"1"` = Manager, `"2"` = Leader

#### `POST /api/Auth/login`

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
  "email": "joao@empresa.com",
  "role": "Operator",
  "division": "Tecnologia"
}
```

---

### Ideias

| Método  | Rota                     | Role            | Descrição                                  |
| ------- | ------------------------ | --------------- | ------------------------------------------ |
| `POST`  | `/api/Idea`              | Operator        | Submeter nova ideia                        |
| `GET`   | `/api/Idea/my`           | Operator        | Listar próprias ideias                     |
| `GET`   | `/api/Idea`              | Manager, Leader | Listar todas as ideias ordenadas por score |
| `PATCH` | `/api/Idea/{id}/approve` | Manager         | Aprovar ideia com scores de impacto        |
| `PATCH` | `/api/Idea/{id}/reject`  | Manager         | Rejeitar ideia                             |

#### `POST /api/Idea`

```json
{
  "title": "Automação do processo X",
  "description": "Reduzir tempo manual usando RPA...",
  "division": "Operações",
  "evidenceUrl": "https://docs.empresa.com/evidencia",
  "challengeId": 1
}
```

#### `PATCH /api/Idea/{id}/approve`

```json
{
  "impactScore": 8,
  "feasibilityScore": 7,
  "alignmentScore": 9
}
```

---

### Projetos

| Método | Rota                | Role            | Descrição                  |
| ------ | ------------------- | --------------- | -------------------------- |
| `POST` | `/api/Project`      | Manager         | Criar projeto              |
| `GET`  | `/api/Project`      | Manager, Leader | Listar todos os projetos   |
| `GET`  | `/api/Project/{id}` | Manager, Leader | Detalhar um projeto        |
| `PUT`  | `/api/Project/{id}` | Manager         | Atualizar dados do projeto |

#### `POST /api/Project`

```json
{
  "title": "Projeto RPA Operações",
  "description": "Automação de processos manuais",
  "division": "Operações",
  "investment": 50000.0,
  "startDate": "2026-06-01T00:00:00Z",
  "deadline": "2026-09-01T00:00:00Z",
  "ideaId": 12
}
```

#### `PUT /api/Project/{id}`

```json
{
  "status": 1,
  "stage": 1,
  "progressPercent": 65,
  "financialReturn": 150000.0,
  "productivityGain": 30
}
```

> `status`: `0`=Planning, `1`=InProgress, `2`=Completed, `3`=OnHold, `4`=Cancelled
> `stage`: `0`=Diagnosis, `1`=Implementation, `2`=Validation, `3`=Closure

---

### Desafios

| Método | Rota                  | Role   | Descrição                      |
| ------ | --------------------- | ------ | ------------------------------ |
| `POST` | `/api/Challenge`      | Leader | Criar novo desafio             |
| `GET`  | `/api/Challenge`      | Todos  | Listar desafios ativos         |
| `GET`  | `/api/Challenge/{id}` | Todos  | Detalhar desafio e suas ideias |
| `PUT`  | `/api/Challenge/{id}` | Leader | Atualizar desafio              |

#### `POST /api/Challenge`

```json
{
  "title": "Sustentabilidade 2026",
  "description": "Ideias para redução de emissão de carbono",
  "prize": 5000.0,
  "startDate": "2026-06-01T00:00:00Z",
  "endDate": "2026-08-31T23:59:59Z"
}
```

---

### Diretrizes Estratégicas

| Método   | Rota                  | Role   | Descrição                      |
| -------- | --------------------- | ------ | ------------------------------ |
| `POST`   | `/api/Guideline`      | Leader | Criar diretriz                 |
| `GET`    | `/api/Guideline`      | Todos  | Listar diretrizes ativas       |
| `GET`    | `/api/Guideline/{id}` | Todos  | Detalhar diretriz              |
| `PUT`    | `/api/Guideline/{id}` | Leader | Atualizar diretriz             |
| `DELETE` | `/api/Guideline/{id}` | Leader | Remover diretriz (soft delete) |

#### `POST /api/Guideline`

```json
{
  "title": "Transformação Digital",
  "description": "Acelerar a digitalização dos processos internos",
  "priority": "High",
  "category": "Tecnologia"
}
```

> `priority`: `"Low"`, `"Medium"`, `"High"`

---

### Dashboard

| Método | Rota             | Role   | Descrição                        |
| ------ | ---------------- | ------ | -------------------------------- |
| `GET`  | `/api/Dashboard` | Leader | Métricas executivas consolidadas |

**Resposta:**

```json
{
  "totalRoi": 320000.00,
  "totalSavings": 180000.00,
  "productivityGainAverage": 24,
  "activeProjects": 8,
  "delayedProjects": 2,
  "ideaFunnel": {
    "totalSubmitted": 45,
    "underReview": 12,
    "approved": 20,
    "rejected": 13,
    "convertedToProjects": 8
  },
  "topProjects": [...],
  "topContributors": [...]
}
```

---

## Autenticação e Autorização

A API utiliza **JWT (JSON Web Token)** com as seguintes configurações:

- Algoritmo: **HMAC SHA-256**
- Claims incluídos no token: `nameidentifier` (userId), `email`, `name`, `role`
- Validade: **8 horas**
- Senhas armazenadas com **BCrypt**

Para autenticar no Swagger:

1. Faça login em `POST /api/Auth/login`
2. Copie o `token` retornado
3. Clique em **Authorize** (canto superior direito do Swagger UI)
4. Insira: `Bearer eyJhbGci...`

---

## Configuração do Ambiente

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/)

### Variáveis de ambiente

Copie o arquivo de exemplo e preencha com suas credenciais:

```bash
cp .env.example .env
```

| Variável      | Descrição                                      |
| ------------- | ---------------------------------------------- |
| `DB_PASSWORD` | Senha do PostgreSQL local (solicitar ao autor) |
| `JWT_KEY`     | Chave secreta JWT (solicitar ao autor)         |

---

## Executando a Aplicação

### Com Docker Compose _(recomendado)_

```bash
# Copie o arquivo de exemplo e preencha com suas credenciais
cp .env.example .env

# Suba a aplicação (banco PostgreSQL + API)
docker-compose up --build
```

Swagger em: **http://localhost:8080/swagger**

> As migrations são aplicadas automaticamente na inicialização.

### Localmente (.NET CLI)

```bash
# Restaurar pacotes
dotnet restore

# Configurar User Secrets (credenciais locais)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "sua_connection_string"
dotnet user-secrets set "Jwt:Key" "sua_chave_secreta"

# Executar a API
dotnet run --project InovaGAB.API
```

---

## Banco de Dados e Migrations

O projeto utiliza **EF Core Migrations** com aplicação automática no startup.

> O banco de dados PostgreSQL é criado automaticamente via container Docker. Não é necessário instalar ou configurar PostgreSQL localmente.

### Comandos úteis

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration --project InovaGAB.API

# Aplicar migrations manualmente
dotnet ef database update --project InovaGAB.API

# Listar migrations
dotnet ef migrations list --project InovaGAB.API
```

### Tabelas do banco

| Tabela                  | Descrição                                 |
| ----------------------- | ----------------------------------------- |
| `Users`                 | Usuários do sistema                       |
| `Ideas`                 | Ideias submetidas                         |
| `Projects`              | Projetos criados a partir de ideias       |
| `Challenges`            | Desafios de inovação                      |
| `StrategicGuidelines`   | Diretrizes estratégicas                   |
| `AuditLogs`             | Registro de auditoria das requisições     |
| `__EFMigrationsHistory` | Controle interno de migrations do EF Core |

---

## Swagger / Documentação Interativa

O Swagger UI está habilitado no ambiente de desenvolvimento:

- **URL**: `http://localhost:8080/swagger`
- **Especificação JSON**: `http://localhost:8080/swagger/v1/swagger.json`

---

## Auditoria

Todas as requisições são registradas automaticamente na tabela `AuditLogs` via `AuditMiddleware`:

- Método HTTP e rota acessada
- E-mail e role do usuário autenticado
- Código de resposta HTTP
- Tempo de execução em ms
- Data/hora da requisição

Rotas excluídas: `/swagger` e `/health`.

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
│   └── Response/
│
├── Models/
│   ├── User.cs          # + enum UserRole
│   ├── Idea.cs          # + enum IdeaStatus
│   ├── Project.cs       # + enum ProjectStatus, ProjectStage
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
├── Migrations/
├── Program.cs
└── InovaGAB.API.csproj
```

---

## Equipe

Projeto desenvolvido para o **Challenge FIAP 2025 — Grupo Águia Branca**.

| Membro                       | RM       |
| ---------------------------- | -------- |
| Guilherme Luccas da Costa    | RM561735 |
| Lenon Otmar Tonoli Merlo     | RM564471 |
| Matheus Henrique Silva Souza | RM561329 |

---

## Licença

Este projeto foi desenvolvido exclusivamente para fins acadêmicos no contexto do Challenge da FIAP.
