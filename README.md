# InovaGAB API

> **FIAP — Challenge 2026 | Grupo Águia Branca**

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=csharp&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-8-47A248?style=flat&logo=mongodb&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?style=flat&logo=docker&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-UI-85EA2D?style=flat&logo=swagger&logoColor=black)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?style=flat&logo=jsonwebtokens&logoColor=white)
![Build](https://github.com/lenonmerlo/InovaGAB.API/actions/workflows/build.yml/badge.svg)

API REST desenvolvida para gestão de inovação corporativa. Operadores submetem ideias, gestores avaliam propostas e acompanham projetos, e líderes administram diretrizes estratégicas e consultam indicadores executivos.

## Execução rápida para avaliação

### Pré-requisitos

- Docker Desktop com Docker Compose; ou
- .NET 8 SDK e uma instância MongoDB acessível.

### Executar com Docker Compose

Na raiz do repositório, crie o arquivo de ambiente a partir do exemplo.

PowerShell:

```powershell
Copy-Item .env.example .env
```

Bash:

```bash
cp .env.example .env
```

Edite o `.env` e defina uma chave JWT com pelo menos 32 caracteres:

```dotenv
JWT_KEY=substitua_por_uma_chave_segura_com_32_caracteres
```

Suba a aplicação:

```bash
docker compose up --build -d
```

Verifique os serviços:

```bash
docker compose ps
```

Acesse:

- Swagger: <http://localhost:8080/swagger>
- Health do MongoDB: <http://localhost:8080/api/health/mongo>
- OpenAPI JSON: <http://localhost:8080/swagger/v1/swagger.json>

Para encerrar sem excluir os dados:

```bash
docker compose down
```

> Não use `docker compose down -v` se quiser preservar o volume do MongoDB.

## Usuários de demonstração

O Seeder cria os seguintes usuários na primeira inicialização:

| Perfil | E-mail | Senha |
| --- | --- | --- |
| Operator | `joao.operador@aguiabranca.com.br` | `senha123` |
| Operator | `maria.operador@aguiabranca.com.br` | `senha123` |
| Manager | `ana.gestora@aguiabranca.com.br` | `senha123` |
| Leader | `carlos.lider@aguiabranca.com.br` | `senha123` |

Essas credenciais existem exclusivamente para demonstração e desenvolvimento.

## Visão geral

Fluxo principal da plataforma:

```text
Operador submete uma ideia
        ↓
Gestor avalia, pontua e aprova ou rejeita
        ↓
Gestor vincula a ideia a um projeto
        ↓
Gestor acompanha investimento, prazo, progresso e retorno
        ↓
Liderança consulta métricas no dashboard executivo
```

A liderança também administra diretrizes estratégicas e desafios internos de inovação.

## Arquitetura

```mermaid
flowchart TD
    A[Aplicativo / Swagger / Postman] --> B[Controllers ASP.NET Core]
    B --> C[JWT e autorização por roles]
    C --> D[Services]
    D --> E[MongoDbContext]
    E --> F[(MongoDB)]
    C --> G[AuditMiddleware]
    G --> F
```

Organização lógica:

```text
Controller
    ↓
Service
    ↓
MongoDbContext / IMongoCollection<T>
    ↓
MongoDB.Driver
    ↓
MongoDB
```

As relações entre collections são mantidas por referências manuais com `ObjectId`. Os Services carregam os documentos relacionados quando necessário; objetos completos de navegação não são duplicados nos documentos.

## Tecnologias

| Tecnologia | Versão | Responsabilidade |
| --- | --- | --- |
| .NET / ASP.NET Core | 8.0 | Plataforma da API |
| MongoDB | 8 | Banco NoSQL orientado a documentos |
| MongoDB.Driver | 3.11.1 | Acesso nativo a collections, filtros e updates |
| JWT Bearer | 8.0.20 | Autenticação e autorização |
| BCrypt.Net-Next | 4.1.0 | Hash e validação de senhas |
| Swashbuckle | 6.9.0 | Swagger e especificação OpenAPI |
| Docker Compose | — | Execução da API e do MongoDB |

O projeto não utiliza Entity Framework Core, Npgsql, PostgreSQL ou migrations relacionais.

## Persistência MongoDB

Banco padrão:

```text
InovaGab
```

Collections:

| Collection | Conteúdo |
| --- | --- |
| `users` | Usuários, roles, credenciais e pontos |
| `ideas` | Ideias submetidas e avaliações |
| `projects` | Projetos, progresso e resultados financeiros |
| `challenges` | Desafios de inovação |
| `strategicGuidelines` | Diretrizes estratégicas |
| `auditLogs` | Auditoria das requisições |

### Convenções BSON

- identificadores são `ObjectId`, representados como `string` na API;
- propriedades são armazenadas em `camelCase`;
- enums são armazenados como texto;
- valores monetários são armazenados como `Decimal128`;
- propriedades calculadas e navegações são ignoradas na serialização;
- campos adicionais são tolerados na leitura para facilitar evolução de schema.

### Índices

A aplicação cria os índices necessários durante a inicialização, incluindo:

- e-mail de usuário único;
- usuário, desafio e status de ideias;
- gestor, ideia e status de projetos;
- diretrizes ativas por data;
- diretrizes por linha de histórico (`rootId`), com índice único parcial garantindo uma única versão vigente (`isCurrent = true`) por linha;
- `guidelineId` de ideias e projetos, para consultas e agregações por estratégia;
- desafios ativos por prazo;
- logs de auditoria por data.

Não existem migrations de schema. Alterações de documentos e índices são administradas pelo código de inicialização e pela compatibilidade de leitura BSON.

## Modelos principais

Todos os campos `Id`, `UserId`, `ManagerId`, `IdeaId`, `ChallengeId` e `CreatedById` relacionados a documentos são strings no contrato HTTP e representam `ObjectId` do MongoDB.

### User

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `Id` | string/ObjectId | Identificador |
| `Name` | string | Nome completo |
| `Email` | string | E-mail normalizado usado no login |
| `PasswordHash` | string | Hash BCrypt da senha |
| `Role` | UserRole | `Operator`, `Manager` ou `Leader` |
| `Division` | string | Área da empresa |
| `Points` | int | Pontuação por contribuições |

### Idea

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `Id` | string/ObjectId | Identificador |
| `UserId` | string/ObjectId | Autor da ideia |
| `ChallengeId` | string/ObjectId? | Desafio opcional |
| `GuidelineId` | string/ObjectId? | Diretriz estratégica vinculada (opcional) |
| `Status` | IdeaStatus | `Submitted`, `UnderReview`, `Approved` ou `Rejected` |
| `ImpactScore` | int | Impacto de 0 a 10 |
| `FeasibilityScore` | int | Viabilidade de 0 a 10 |
| `AlignmentScore` | int | Alinhamento de 0 a 10 |
| `TotalScore` | int | Média calculada, não persistida |

### Project

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `Id` | string/ObjectId | Identificador |
| `ManagerId` | string/ObjectId | Gestor responsável |
| `IdeaId` | string/ObjectId? | Ideia de origem |
| `GuidelineId` | string/ObjectId? | Diretriz estratégica vinculada (opcional) |
| `Status` | ProjectStatus | Estado do projeto |
| `Stage` | ProjectStage | Etapa atual |
| `Investment` | Decimal128 | Investimento |
| `FinancialReturn` | Decimal128 | Retorno financeiro |
| `Roi` | decimal | ROI calculado, não persistido |
| `ProductivityGain` | int | Ganho de produtividade |
| `ProgressPercent` | int | Progresso de 0 a 100 |

### StrategicGuideline

Representa uma diretriz estratégica **versionada**: cada atualização gera um novo documento (nova versão), preservando o anterior como histórico. Nenhuma atualização apaga ou sobrescreve uma versão já existente.

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `Id` | string/ObjectId | Identificador desta versão |
| `RootId` | string/ObjectId | Identificador da primeira versão da linha de histórico; igual em todas as versões geradas a partir dela |
| `PreviousVersionId` | string/ObjectId? | Versão anterior desta linha, quando esta versão veio de uma atualização |
| `Version` | int | Número sequencial da versão dentro da linha, começando em 1 |
| `IsCurrent` | bool | `true` somente na versão vigente da linha; garantido único por `RootId` via índice |
| `IsActive` | bool | `false` quando a diretriz foi desativada (exclusão lógica) |
| `Category` | string | Categoria da diretriz |
| `Campaign` | string | Campanha institucional vinculada (ex.: "InovaGAB 2026") |
| `Priority` | GuidelinePriority | `Low`, `Medium` ou `High` |

Ao atualizar uma diretriz (`PUT /api/Guideline/{id}`), a API cria uma nova versão vigente e marca a versão anterior como `IsCurrent = false`, sem excluí-la — o histórico completo continua disponível em `GET /api/Guideline/{id}/history`. `GET /api/Guideline` lista apenas as versões vigentes (`IsActive && IsCurrent`), garantindo que a estratégia atual de cada linha seja sempre inequívoca.

## Autenticação e autorização

A API utiliza JWT assinado com HMAC SHA-256. O token contém:

- identificador MongoDB do usuário;
- e-mail;
- nome;
- role.

Validade padrão do token: 8 horas.

Para autenticar no Swagger:

1. execute `POST /api/Auth/login`;
2. copie apenas o campo `token`;
3. clique em **Authorize**;
4. informe `Bearer SEU_TOKEN`.

O cadastro público sempre cria um usuário `Operator`. Campos extras tentando definir `Manager` ou `Leader` são ignorados. Os perfis privilegiados de demonstração são criados pelo Seeder.

## Matriz de acesso

| Recurso | Operator | Manager | Leader |
| --- | ---: | ---: | ---: |
| Cadastrar ideia | Sim | Não | Não |
| Consultar próprias ideias | Sim | Não | Não |
| Consultar todas as ideias | Não | Sim | Sim |
| Aprovar ou rejeitar ideia | Não | Sim | Não |
| Criar e atualizar projeto | Não | Sim | Não |
| Consultar projetos | Não | Sim | Sim |
| Gerenciar diretrizes | Não | Não | Sim |
| Consultar diretrizes | Sim | Sim | Sim |
| Gerenciar desafios | Não | Não | Sim |
| Consultar desafios | Sim | Sim | Sim |
| Consultar dashboard | Não | Não | Sim |

Respostas esperadas:

- `401 Unauthorized`: token ausente ou inválido;
- `403 Forbidden`: usuário autenticado sem a role exigida;
- `400 Bad Request`: dados ou `ObjectId` inválidos;
- `404 Not Found`: recurso não encontrado.

## Endpoints

### Autenticação

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| POST | `/api/Auth/register` | Público | Cadastra um operador |
| POST | `/api/Auth/login` | Público | Autentica e gera JWT |

Cadastro:

```json
{
  "name": "João Silva",
  "email": "joao@empresa.com",
  "password": "Senha@123",
  "division": "Tecnologia"
}
```

Login:

```json
{
  "email": "joao@empresa.com",
  "password": "Senha@123"
}
```

### Ideias

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| POST | `/api/Idea` | Operator | Submete uma ideia |
| GET | `/api/Idea/my` | Operator | Lista as ideias do usuário |
| GET | `/api/Idea` | Manager, Leader | Lista todas as ideias por score |
| PATCH | `/api/Idea/{id}/approve` | Manager | Aprova e pontua uma ideia |
| PATCH | `/api/Idea/{id}/reject` | Manager | Rejeita uma ideia |

Criação:

```json
{
  "title": "Automação do processo X",
  "description": "Reduzir tempo manual usando automação.",
  "division": "Operações",
  "evidenceUrl": "https://example.com/evidencia",
  "challengeId": "66d1234567890abcdef12345",
  "guidelineId": "66d1234567890abcdef54321"
}
```

`challengeId` e `guidelineId` são opcionais e podem ser `null`. Quando informado, `guidelineId` deve apontar para uma diretriz existente e ativa (`400 Bad Request` caso contrário). A resposta traz `guidelineId` e um resumo em `guideline` (`id`, `title`, `category`, `campaign`, `isActive`), sem exigir uma consulta adicional a `GET /api/Guideline/{id}`.

Aprovação:

```json
{
  "impactScore": 8,
  "feasibilityScore": 7,
  "alignmentScore": 9
}
```

### Projetos

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| POST | `/api/Project` | Manager | Cria um projeto |
| GET | `/api/Project` | Manager, Leader | Lista projetos |
| GET | `/api/Project/{id}` | Manager, Leader | Detalha um projeto |
| PUT | `/api/Project/{id}` | Manager | Atualiza um projeto |

Criação:

```json
{
  "title": "Projeto RPA Operações",
  "description": "Automação de processos manuais",
  "division": "Operações",
  "investment": 50000,
  "startDate": "2026-09-10T00:00:00Z",
  "deadline": "2026-12-20T23:59:59Z",
  "ideaId": "66d1234567890abcdef12345",
  "guidelineId": "66d1234567890abcdef54321"
}
```

`ideaId` e `guidelineId` são opcionais e podem ser `null`. Quando informado, `guidelineId` deve apontar para uma diretriz existente e ativa (`400 Bad Request` caso contrário). A resposta traz `guidelineId` e um resumo em `guideline`, sem exigir uma consulta adicional.

Atualização:

```json
{
  "status": 1,
  "stage": 1,
  "financialReturn": 150000,
  "productivityGain": 30,
  "progressPercent": 65,
  "guidelineId": "66d1234567890abcdef54321"
}
```

`guidelineId` na atualização é opcional; quando enviado, substitui o vínculo atual do projeto (mesma validação de existência e diretriz ativa).

### Desafios

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| POST | `/api/Challenge` | Leader | Cria um desafio |
| GET | `/api/Challenge` | Autenticado | Lista desafios ativos |
| GET | `/api/Challenge/{id}` | Autenticado | Detalha um desafio e suas ideias |
| PUT | `/api/Challenge/{id}` | Leader | Atualiza um desafio |

### Diretrizes estratégicas

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| POST | `/api/Guideline` | Leader | Cria uma diretriz (nova linha de histórico) |
| GET | `/api/Guideline` | Autenticado | Lista apenas as versões vigentes das diretrizes ativas |
| GET | `/api/Guideline/{id}` | Autenticado | Detalha uma diretriz por Id (qualquer versão) |
| GET | `/api/Guideline/{id}/history` | Autenticado | Lista o histórico completo da linha da diretriz, da versão mais recente para a mais antiga |
| PUT | `/api/Guideline/{id}` | Leader | Cria uma nova versão vigente a partir da diretriz informada, preservando a anterior como histórico |
| DELETE | `/api/Guideline/{id}` | Leader | Desativa logicamente a diretriz (`IsActive = false`) |

Criação/atualização:

```json
{
  "title": "Redução de Custos Operacionais",
  "description": "Foco em iniciativas que gerem economia direta na operação.",
  "category": "Financeiro",
  "campaign": "InovaGAB 2026",
  "priority": "High"
}
```

Cada diretriz retornada traz `id`, `rootId`, `previousVersionId`, `version`, `isCurrent` e `isActive`, permitindo identificar de forma inequívoca a estratégia vigente de cada linha de histórico.

### Dashboard

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| GET | `/api/Dashboard` | Leader | Retorna métricas executivas |

O dashboard apresenta ROI consolidado, retorno financeiro, produtividade média, projetos ativos e atrasados, funil de ideias, projetos com maior ROI e principais contribuidores.

### Infraestrutura

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| GET | `/api/health/mongo` | Público | Verifica a conexão com MongoDB |

## Tratamento de erros

A API possui middleware global para impedir vazamento de stack trace e padronizar erros. Exemplo para um `ObjectId` inválido:

```json
{
  "title": "Identificador inválido.",
  "status": 400,
  "detail": "O identificador informado não possui um formato válido.",
  "instance": "/api/Project"
}
```

## Auditoria

O `AuditMiddleware` registra no MongoDB:

- método HTTP;
- endpoint;
- e-mail e role autenticados;
- status HTTP;
- duração em milissegundos;
- data e hora em UTC.

Swagger e health checks não são auditados. Falhas na gravação do log não interrompem a requisição principal.

Consulta de diagnóstico:

```bash
docker exec inovagab-mongo mongosh InovaGab --quiet --eval "printjson(db.auditLogs.find().sort({ createdAt: -1 }).limit(3).toArray())"
```

## Testes de regressão com Postman

Importe o arquivo:

```text
InovaGAB.API.postman_collection.json
```

A Collection executa os fluxos em ordem e armazena automaticamente tokens e IDs. Ela cobre:

- health check;
- autenticação dos três perfis;
- respostas `401` e `403`;
- criação e aprovação de ideia;
- criação, atualização e consulta de projeto;
- dashboard;
- CRUD de diretriz;
- resposta `400` para `ObjectId` inválido.

Para executar, abra a Collection no Postman e selecione **Run collection**. A API deve estar disponível em `http://localhost:8080`.

## Execução local sem Docker para a API

Com MongoDB disponível em `mongodb://localhost:27017`:

```powershell
dotnet restore
dotnet user-secrets set "Jwt:Key" "sua_chave_secreta_com_pelo_menos_32_caracteres" --project InovaGAB.API
dotnet run --project InovaGAB.API
```

Configuração padrão:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "InovaGab"
  }
}
```

## Estrutura do projeto

```text
InovaGAB.API/
├── Configuration/
│   ├── MongoDbConventions.cs
│   └── MongoDbSettings.cs
├── Controllers/
├── Data/
│   ├── DataSeeder.cs
│   ├── MongoDbContext.cs
│   └── MongoDbIndexes.cs
├── DTOs/
│   ├── Request/
│   └── Response/
├── Middleware/
│   ├── AuditMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
├── Models/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Dockerfile
├── Program.cs
└── InovaGAB.API.csproj
```

Arquivos de apoio na raiz:

```text
docker-compose.yml
.env.example
InovaGAB.API.postman_collection.json
README.md
```

## Verificação da aplicação

```powershell
dotnet build
docker compose up --build -d
docker compose ps
Invoke-RestMethod http://localhost:8080/api/health/mongo
```

Resultado esperado do health check:

```json
{
  "status": "healthy",
  "database": "InovaGab",
  "mongoPing": 1
}
```

## Equipe

| Integrante | RM |
| --- | --- |
| Guilherme Luccas da Costa | RM561735 |
| Lenon Otmar Tonoli Merlo | RM564471 |
| Matheus Henrique Silva Souza | RM561329 |

## Licença

Projeto acadêmico desenvolvido no contexto do Challenge FIAP para o Grupo Águia Branca.
