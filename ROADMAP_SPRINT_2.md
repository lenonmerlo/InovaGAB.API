# 🚀 INOVAGAB — Sprint 2 — Roadmap de Entrega

**Prazo de entrega:** 21/09/2026  
**Frontend:** Guilherme  
**Backend:** Lenon + Matheus  
**Status atualizado em:** 10/09/2026

---

## 📌 Situação atual

### ✅ Concluído

- Migração completa de PostgreSQL/EF Core para MongoDB.
- API e Docker adequados ao .NET 8.
- IDs convertidos de `int` para `string/ObjectId`.
- Relações convertidas para referências manuais entre collections.
- Serialização BSON configurada.
- PostgreSQL, Npgsql, `AppDbContext` e migrations removidos.
- Seeder reescrito para MongoDB.
- Índices MongoDB criados no startup.
- Autenticação JWT e autorização por roles validadas.
- Cadastro público protegido contra criação de `Manager` ou `Leader`.
- Tratamento global de erros e `ObjectId` inválido com resposta `400`.
- Auditoria persistida na collection `auditLogs`.
- Dashboard atual migrado e ROI consolidado corrigido.
- Collection Postman de regressão criada e executada com sucesso.
- README atualizado para .NET 8 e MongoDB.
- Pull Request da migração revisado e integrado à `main`.

### 🟢 Dependência liberada

Matheus já pode trabalhar nas funcionalidades que dependiam do novo modelo MongoDB:

- vínculo entre ideias, projetos e estratégias;
- CRUD completo e priorização de ideias;
- dashboard por estratégia e projeto;
- integração com IA.

### 🔴 Caminho crítico restante

1. Completar requisitos funcionais obrigatórios.
2. Integrar o aplicativo real e remover todos os mocks.
3. Implementar e demonstrar a funcionalidade de IA.
4. Criar testes automatizados.
5. Fechar documentação, apresentação e pacotes de entrega.

---

# 👨‍💻 Lenon — Fundação técnica, suporte e revisão

## ✅ Migração e infraestrutura MongoDB

- [x] Adequar projeto e Docker para .NET 8.
- [x] Adicionar `MongoDB.Driver`.
- [x] Configurar `MongoDbSettings`.
- [x] Registrar `IMongoClient`, `IMongoDatabase` e `MongoDbContext`.
- [x] Configurar convenções BSON:
  - [x] propriedades em `camelCase`;
  - [x] enums como `string`;
  - [x] valores monetários como `Decimal128`;
  - [x] navegações e campos calculados ignorados.
- [x] Converter IDs para `string/ObjectId`:
  - [x] `User`;
  - [x] `Idea`;
  - [x] `Project`;
  - [x] `Challenge`;
  - [x] `StrategicGuideline`;
  - [x] `AuditLog`.
- [x] Atualizar DTOs, Controllers, interfaces e claims JWT para IDs `string`.
- [x] Substituir `Include()` e relações do EF por consultas explícitas.
- [x] Migrar todos os Services para MongoDB.
- [x] Migrar `AuditMiddleware` para MongoDB.
- [x] Reescrever `DataSeeder`.
- [x] Criar health check MongoDB.
- [x] Criar índices para consultas e e-mail único.
- [x] Remover PostgreSQL do Docker Compose.
- [x] Remover EF Core, Npgsql, `AppDbContext` e migrations.
- [x] Atualizar `.env.example` e `appsettings.json`.
- [x] Executar regressão completa no Postman.
- [x] Atualizar README.
- [x] Integrar a migração à `main`.

## 🔲 Responsabilidades restantes do Lenon

- [ ] Apoiar Matheus na implementação sobre MongoDB.
- [ ] Revisar os novos Models, DTOs e índices adicionados.
- [ ] Evitar consultas N+1 nos novos relatórios e relacionamentos.
- [ ] Revisar validações de `ObjectId`, ownership e transições de status.
- [ ] Revisar segurança das chaves de IA e JWT.
- [ ] Revisar o contrato final usado pelo aplicativo.
- [ ] Executar regressão Postman após cada integração relevante.
- [ ] Revisar a documentação final e os artefatos da entrega.

---

# 👨‍💻 Matheus — Funcionalidades restantes do backend

## 🎯 1. Estratégias, histórico e vínculos

> Requisito obrigatório e prioridade máxima.

- [x] Adicionar `Campaign` em `StrategicGuideline`.
- [x] Garantir histórico das estratégias contendo, no mínimo:
  - [x] ID;
  - [x] data;
  - [x] categoria;
  - [x] campanha.
- [x] Definir a estratégia vigente de forma inequívoca.
- [x] Adicionar `GuidelineId` opcional em `Idea`.
- [x] Adicionar `GuidelineId` opcional em `Project`.
- [x] Usar `string/ObjectId` nos novos IDs e DTOs.
- [x] Atualizar `CreateIdeaRequest` para aceitar `GuidelineId`.
- [x] Atualizar `CreateProjectRequest` para aceitar `GuidelineId`.
- [x] Validar que a diretriz referenciada existe e está ativa.
- [x] Expor a diretriz vinculada em `IdeaResponse`.
- [x] Expor a diretriz vinculada em `ProjectResponse`.
- [x] Criar índices para `ideas.guidelineId` e `projects.guidelineId`.
- [x] Atualizar Seeder, Swagger, README e Collection Postman.

### Critérios de aceite

- [x] Ideia e projeto não aceitam uma diretriz inexistente ou inativa.
- [x] O vínculo é retornado ao aplicativo sem exigir consultas adicionais desnecessárias.
- [x] Atualizações de estratégia não apagam o histórico exigido pelo edital.

### Como foi implementado

`StrategicGuideline` agora é **versionado**: `PUT /api/Guideline/{id}` nunca sobrescreve o
documento existente: ele cria uma nova versão (`Version`, `RootId`, `PreviousVersionId`) e
marca a anterior como `IsCurrent = false`, preservando-a como histórico. Um índice único
parcial (`RootId` + `IsCurrent = true`) garante, no próprio banco, que exista no máximo uma
versão vigente por linha. O histórico completo fica disponível em
`GET /api/Guideline/{id}/history`. `Idea` e `Project` ganharam `GuidelineId` opcional,
validado contra diretrizes existentes e ativas na criação (e na atualização, para `Project`),
e as respostas embutem um resumo da diretriz (`GuidelineSummaryResponse`) carregado em lote
para evitar consultas N+1.

---

## 💡 2. CRUD completo de ideias e priorização

### Consultar ideia específica

- [x] Criar:

```http
GET /api/Idea/{id}
```

- [x] Permitir acesso conforme ownership e matriz de roles.
- [x] Retornar `400` para ID inválido e `404` para ideia inexistente.

### Editar ideia

- [x] Criar:

```http
PUT /api/Idea/{id}
```

- [x] Permitir que o `Operator` edite somente a própria ideia.
- [x] Permitir edição somente enquanto `Status = Submitted`.
- [x] Impedir alteração direta de autor, scores e status pelo operador.

### Excluir ideia

- [x] Criar:

```http
DELETE /api/Idea/{id}
```

- [x] Permitir que o `Operator` exclua somente a própria ideia.
- [x] Permitir exclusão somente antes da avaliação.
- [x] Definir se a exclusão será física ou lógica e documentar a decisão.

### Priorizar ideia

- [x] Adicionar `Priority` em `Idea`.
- [x] Definir enum ou faixa de prioridade documentada.
- [x] Criar:

```http
PATCH /api/Idea/{id}/prioritize
```

- [x] Permitir priorização somente ao `Manager`, conforme o requisito.
- [x] Registrar a operação no `AuditLog`.

### Regras adicionais

- [x] Impedir pontuações fora de `0-10`.
- [x] Impedir bônus duplicado ao aprovar novamente uma ideia.
- [x] Validar transições de status permitidas.
- [x] Atualizar Collection Postman com todos os novos fluxos.

### Como foi implementado

Exclusão lógica (`IsDeleted`), não física: preserva o documento para eventual
referência/auditoria e evita ponteiros quebrados. `GET/PUT/DELETE /api/Idea/{id}`
checam ownership no controller (`Forbid()`/`UnauthorizedAccessException` -> 403) e
delegam as regras de status ao service, que lança `InvalidOperationException`
(400) fora das janelas permitidas. `Priority` é um enum próprio (`IdeaPriority`),
sem relação com a prioridade da diretriz. O `AuditMiddleware` já registra toda
requisição autenticada (método, rota, usuário, status), então a priorização é
auditada automaticamente, sem código extra.

---

## 📁 3. CRUD completo de projetos

> O backend atual possui criação, listagem, detalhe e atualização. Falta fechar formalmente o CRUD exigido.

- [x] Criar projeto.
- [x] Listar projetos.
- [x] Consultar projeto por ID.
- [x] Atualizar projeto.
- [x] Definir e implementar exclusão ou arquivamento:

```http
DELETE /api/Project/{id}
```

- [x] Restringir criação, alteração e exclusão ao `Manager`.
- [x] Garantir consulta do andamento pelo `Leader`.
- [x] Validar investimento não negativo.
- [x] Validar progresso entre `0-100`.
- [x] Validar que o prazo não seja anterior à data de início.
- [x] Validar transições de status e etapa.
- [x] Definir comportamento do ROI quando ainda não houver retorno financeiro.

### Como foi implementado

Arquivamento lógico (`IsArchived`), não exclusão física: o projeto carrega
investimento/retorno que compõem o ROI consolidado, e apagar o documento
distorceria os totais históricos do dashboard. Projetos arquivados somem da
listagem padrão mas continuam acessíveis por id. Transições de status e etapa
são validadas contra uma matriz fixa no `ProjectService` (`Completed` e
`Cancelled` são estados finais; etapas só avançam). ROI retorna `0` (em vez de
`-100%`) enquanto não houver retorno financeiro lançado.

---

## 📊 4. Dashboard por estratégia e projeto

- [x] Dashboard geral para `Leader`.
- [x] ROI consolidado calculado sobre investimento e retorno totais.
- [x] Indicadores atuais:
  - [x] retorno financeiro;
  - [x] produtividade média;
  - [x] projetos ativos e atrasados;
  - [x] funil de ideias;
  - [x] top projetos;
  - [x] top contribuidores.
- [x] Adicionar agrupamento por `GuidelineId`.
- [x] Exibir por estratégia:
  - [x] investimento;
  - [x] retorno financeiro;
  - [x] ROI;
  - [x] produtividade;
  - [x] prazo e atrasos;
  - [x] quantidade de projetos.
- [x] Incluir `GuidelineTitle` nos projetos do dashboard.
- [x] Permitir retorno específico por projeto.
- [x] Criar endpoint de drill-down recomendado:

```http
GET /api/Dashboard/guideline/{id}
```

- [x] Avaliar endpoint específico por projeto:

```http
GET /api/Dashboard/project/{id}
```

- [x] Garantir payload amigável para gráficos no aplicativo.
- [x] Evitar consultas N+1 e carregamento integral desnecessário das collections.

### Como foi implementado

O agrupamento usa `RootId` da diretriz (não o id de uma versão específica), então
projetos vinculados a versões antigas da mesma estratégia entram no mesmo grupo,
rotulado com título/categoria/campanha da versão vigente. Projetos sem diretriz
caem em um grupo `"Sem diretriz"`. Tudo é montado a partir de 4 consultas em lote
(projetos, ideias, usuários, diretrizes) e junções em memória por dicionário, sem
N+1. `GET /api/Dashboard/project/{id}` reaproveita `IProjectService.GetByIdAsync`.

---

## 🤖 5. IA — Pontuação automática de ideias

> Diferencial da Sprint e prioridade alta após os requisitos obrigatórios.

- [x] Escolher e documentar o provedor:
  - [x] Google Gemini API; ou
  - [ ] GitHub Models; ou
  - [ ] OpenRouter.
- [x] Guardar a chave somente em `.env` ou `user-secrets`.
- [x] Adicionar a variável sem segredo ao `.env.example`.
- [x] Criar:
  - [x] `IAiScoringService`;
  - [x] `AiScoringService`;
  - [x] Settings tipados do provedor;
  - [x] DTO estruturado da sugestão.
- [x] Criar prompt versionado com:
  - [x] título;
  - [x] descrição;
  - [x] diretriz estratégica vinculada;
  - [x] critérios e faixa de pontuação.
- [x] Exigir JSON contendo:
  - [x] `ImpactScore`;
  - [x] `FeasibilityScore`;
  - [x] `AlignmentScore`;
  - [x] justificativa curta.
- [x] Criar:

```http
POST /api/Idea/{id}/ai-score
```

- [x] Restringir ao `Manager`.
- [x] Retornar somente sugestão; não aprovar nem persistir automaticamente.
- [x] Configurar timeout e `CancellationToken`.
- [x] Validar JSON e scores retornados.
- [x] Tratar indisponibilidade sem bloquear avaliação manual.
- [x] Não registrar chaves, tokens ou prompts sensíveis.
- [x] Registrar no `AuditLog` que houve sugestão por IA.
- [ ] Testar sucesso, timeout, JSON inválido e falha do provedor (fica para o item 6, com xUnit).

### Como foi implementado

Provedor: Gemini, modelo `gemini-3.5-flash-lite` (configurável via `Gemini:Model`,
sem precisar recompilar). A chave vem só de `.env`/`user-secrets` (`Gemini:ApiKey`);
sem ela, o serviço lança `AiScoringUnavailableException` antes de qualquer chamada
de rede. Timeout configurável (`Gemini:TimeoutSeconds`, padrão 20s) combinado com o
`CancellationToken` da requisição via `CancellationTokenSource` vinculado. Qualquer
falha de rede, timeout, JSON inválido ou score fora de `0-10` vira
`AiScoringUnavailableException`, mapeada pelo middleware global para `502 Bad
Gateway`; as rotas de aprovação/rejeição manual são independentes e não são
afetadas. A auditoria genérica do `AuditMiddleware` (que já registra toda
requisição autenticada) cobre o requisito de registrar que houve sugestão por IA,
sem código extra e sem logar prompt, resposta ou chave.

### Critério de demonstração

O gestor solicita uma sugestão, vê os três scores e a justificativa, podendo aceitar, ajustar ou ignorar a recomendação antes da decisão humana.

---

## 🧪 6. Testes automatizados

- [x] Criar projeto `InovaGAB.API.Tests` com xUnit.
- [x] Adicionar testes unitários para:
  - [x] aprovação de ideia;
  - [x] rejeição de ideia;
  - [x] priorização;
  - [x] regras de ownership;
  - [x] transições de status;
  - [x] cálculo de ROI individual;
  - [x] cálculo de ROI consolidado.
- [x] Adicionar testes de integração para:
  - [x] login válido e inválido;
  - [x] `401` sem token;
  - [x] `403` por role incorreta;
  - [x] `400` para `ObjectId` inválido;
  - [x] vínculo com estratégia ativa/inativa;
  - [x] CRUD de ideias;
  - [x] CRUD de projetos;
  - [x] fallback da IA.
- [x] Usar MongoDB isolado para testes de integração.
- [x] Executar testes no workflow `.github/workflows/build.yml`.
- [x] Manter a Collection Postman como regressão manual/demonstração.

### Como foi implementado

`RoiCalculator` foi extraído de `Project.Roi` e do `DashboardService` como
função pura (mesma fórmula nos dois lugares), permitindo testar ROI individual
e consolidado sem banco. As regras de negócio dos services (aprovação,
rejeição, priorização, ownership, transições) são testadas diretamente contra
um MongoDB isolado (`InovaGab_Test_<guid>`, criado e destruído por classe de
teste) em vez de mocks: isso já pegou de verdade o bug do índice único da
diretriz antes de qualquer revisão manual. O fallback da IA é testado com um
`HttpMessageHandler` falso no lugar da chamada real ao Gemini (sucesso, JSON
inválido, score fora da faixa, erro HTTP do provedor e timeout), sem depender
de rede nem de chave real. Os testes de integração usam
`WebApplicationFactory<Program>` contra a aplicação real, cada classe com seu
próprio banco isolado. Total: 55 testes, todos passando localmente contra
MongoDB real.

---

# 📱 Guilherme — Integração do aplicativo

> Esta etapa vale 15% da avaliação e não deve ser deixada para o final.

- [ ] Remover todos os mocks do aplicativo.
- [ ] Configurar URL da API por ambiente.
- [ ] Integrar login e armazenamento seguro do JWT.
- [ ] Adaptar todos os IDs para `string/ObjectId`.
- [ ] Enviar `Authorization: Bearer {token}`.
- [ ] Integrar consulta de diretrizes e desafios.
- [ ] Integrar CRUD de ideias conforme a role.
- [ ] Integrar priorização e aprovação para gestor.
- [ ] Integrar CRUD e acompanhamento de projetos.
- [ ] Integrar dashboard e gráficos para liderança.
- [ ] Integrar sugestão de score por IA.
- [ ] Tratar `400`, `401`, `403`, `404`, timeout e indisponibilidade.
- [ ] Testar loading, lista vazia, erro e retry.
- [ ] Testar contra a API real sem mocks.
- [ ] Gerar APK Android ou IPA iOS.
- [ ] Gerar ZIP completo do aplicativo.

---

# 📑 Documentação e apresentação

## ✅ Já disponível

- [x] README com execução via Docker Compose e MongoDB.
- [x] Diagrama lógico da arquitetura no README.
- [x] Lista dos endpoints atuais.
- [x] Collection Postman de regressão.
- [x] Usuários de demonstração documentados.

## 🔲 Entrega final

- [ ] Atualizar README após todas as novas funcionalidades.
- [ ] Atualizar Collection Postman com os endpoints novos.
- [ ] Criar tabela final contendo:
  - [ ] rota;
  - [ ] método HTTP;
  - [ ] acesso;
  - [ ] payload;
  - [ ] resposta;
  - [ ] códigos de erro.
- [ ] Criar diagrama final da arquitetura.
- [ ] Criar slide de capa com nomes e RMs.
- [ ] Criar slides do problema, solução e fluxo principal.
- [ ] Explicar MongoDB, collections, referências e índices.
- [ ] Explicar o modelo/provedor de IA.
- [ ] Mostrar o prompt e o fluxo de decisão humana.
- [ ] Mostrar integração real do aplicativo.
- [ ] Mostrar dashboard e indicadores.
- [ ] Gerar `.pptx` e `.pdf`.
- [ ] Ensaiar demonstração usando dados previsíveis.

---

# 📦 Empacotamento final

## Backend

- [ ] Executar `dotnet build` sem erros.
- [ ] Executar testes automatizados.
- [ ] Executar toda a Collection Postman.
- [ ] Confirmar que `.env` e segredos não estão versionados.
- [ ] Confirmar que Docker Compose sobe do zero.
- [ ] Confirmar health check saudável.
- [ ] Gerar ZIP do backend sem `bin`, `obj`, `.git`, `.env` e volumes.

## Aplicativo

- [ ] Gerar ZIP com todos os artefatos necessários.
- [ ] Incluir APK ou IPA.
- [ ] Confirmar integração com API real.

## Apresentação

- [ ] Gerar PPTX.
- [ ] Gerar PDF.
- [ ] Conferir nomes e RMs.
- [ ] Conferir legibilidade dos diagramas e tabelas.

---

# 🔄 Ordem recomendada a partir de agora

1. **Matheus:** estratégia vigente, histórico, `Campaign` e vínculos.
2. **Matheus:** CRUD completo e priorização de ideias.
3. **Matheus:** fechar CRUD e validações de projetos.
4. **Guilherme:** integrar cada contrato liberado no aplicativo, sem esperar o backend inteiro.
5. **Matheus:** dashboard por estratégia/projeto.
6. **Matheus:** integração de IA.
7. **Lenon:** revisão transversal de MongoDB, segurança e contratos.
8. **Backend:** testes automatizados e atualização da Collection Postman.
9. **Equipe:** teste ponta a ponta app → API → MongoDB → IA.
10. **Equipe:** documentação, apresentação, APK/IPA e ZIPs.

---

# 📅 Cronograma sugerido

| Data | Meta |
| --- | --- |
| 10–12/09 | Estratégias, histórico, vínculos e contrato com o app |
| 13–14/09 | CRUD/priorização de ideias e fechamento de projetos |
| 15–16/09 | Dashboard por estratégia/projeto |
| 16–17/09 | IA e fallback manual |
| 17–18/09 | Integração completa do aplicativo |
| 18–19/09 | Testes automatizados e correções |
| 19–20/09 | README final, slides, PDF, APK/IPA e ZIPs |
| 21/09 | Margem de segurança, ensaio e submissão |

---

# ✅ Definition of Done da Sprint 2

A Sprint só está pronta quando:

- [ ] todos os recursos obrigatórios funcionam no MongoDB;
- [ ] permissões por role estão validadas;
- [ ] aplicativo consome exclusivamente APIs reais;
- [ ] não existem mocks nos fluxos entregues;
- [ ] dashboard apresenta visão geral e filtros exigidos;
- [ ] IA funciona e possui fallback manual;
- [ ] testes automatizados e Postman passam;
- [ ] Swagger e README refletem o comportamento real;
- [ ] Docker Compose sobe em ambiente limpo;
- [ ] APK/IPA e os dois ZIPs foram gerados;
- [ ] apresentação existe em PPTX e PDF;
- [ ] equipe executou ao menos um ensaio completo da demonstração.

---

## 🎯 Deadline

**21/09/2026**
