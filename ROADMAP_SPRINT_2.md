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
documento existente — ele cria uma nova versão (`Version`, `RootId`, `PreviousVersionId`) e
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

- [ ] Criar:

```http
GET /api/Idea/{id}
```

- [ ] Permitir acesso conforme ownership e matriz de roles.
- [ ] Retornar `400` para ID inválido e `404` para ideia inexistente.

### Editar ideia

- [ ] Criar:

```http
PUT /api/Idea/{id}
```

- [ ] Permitir que o `Operator` edite somente a própria ideia.
- [ ] Permitir edição somente enquanto `Status = Submitted`.
- [ ] Impedir alteração direta de autor, scores e status pelo operador.

### Excluir ideia

- [ ] Criar:

```http
DELETE /api/Idea/{id}
```

- [ ] Permitir que o `Operator` exclua somente a própria ideia.
- [ ] Permitir exclusão somente antes da avaliação.
- [ ] Definir se a exclusão será física ou lógica e documentar a decisão.

### Priorizar ideia

- [ ] Adicionar `Priority` em `Idea`.
- [ ] Definir enum ou faixa de prioridade documentada.
- [ ] Criar:

```http
PATCH /api/Idea/{id}/prioritize
```

- [ ] Permitir priorização somente ao `Manager`, conforme o requisito.
- [ ] Registrar a operação no `AuditLog`.

### Regras adicionais

- [ ] Impedir pontuações fora de `0–10`.
- [ ] Impedir bônus duplicado ao aprovar novamente uma ideia.
- [ ] Validar transições de status permitidas.
- [ ] Atualizar Collection Postman com todos os novos fluxos.

---

## 📁 3. CRUD completo de projetos

> O backend atual possui criação, listagem, detalhe e atualização. Falta fechar formalmente o CRUD exigido.

- [x] Criar projeto.
- [x] Listar projetos.
- [x] Consultar projeto por ID.
- [x] Atualizar projeto.
- [ ] Definir e implementar exclusão ou arquivamento:

```http
DELETE /api/Project/{id}
```

- [ ] Restringir criação, alteração e exclusão ao `Manager`.
- [ ] Garantir consulta do andamento pelo `Leader`.
- [ ] Validar investimento não negativo.
- [ ] Validar progresso entre `0–100`.
- [ ] Validar que o prazo não seja anterior à data de início.
- [ ] Validar transições de status e etapa.
- [ ] Definir comportamento do ROI quando ainda não houver retorno financeiro.

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
- [ ] Adicionar agrupamento por `GuidelineId`.
- [ ] Exibir por estratégia:
  - [ ] investimento;
  - [ ] retorno financeiro;
  - [ ] ROI;
  - [ ] produtividade;
  - [ ] prazo e atrasos;
  - [ ] quantidade de projetos.
- [ ] Incluir `GuidelineTitle` nos projetos do dashboard.
- [ ] Permitir retorno específico por projeto.
- [ ] Criar endpoint de drill-down recomendado:

```http
GET /api/Dashboard/guideline/{id}
```

- [ ] Avaliar endpoint específico por projeto:

```http
GET /api/Dashboard/project/{id}
```

- [ ] Garantir payload amigável para gráficos no aplicativo.
- [ ] Evitar consultas N+1 e carregamento integral desnecessário das collections.

---

## 🤖 5. IA — Pontuação automática de ideias

> Diferencial da Sprint e prioridade alta após os requisitos obrigatórios.

- [ ] Escolher e documentar o provedor:
  - [ ] Google Gemini API; ou
  - [ ] GitHub Models; ou
  - [ ] OpenRouter.
- [ ] Guardar a chave somente em `.env` ou `user-secrets`.
- [ ] Adicionar a variável sem segredo ao `.env.example`.
- [ ] Criar:
  - [ ] `IAiScoringService`;
  - [ ] `AiScoringService`;
  - [ ] Settings tipados do provedor;
  - [ ] DTO estruturado da sugestão.
- [ ] Criar prompt versionado com:
  - [ ] título;
  - [ ] descrição;
  - [ ] diretriz estratégica vinculada;
  - [ ] critérios e faixa de pontuação.
- [ ] Exigir JSON contendo:
  - [ ] `ImpactScore`;
  - [ ] `FeasibilityScore`;
  - [ ] `AlignmentScore`;
  - [ ] justificativa curta.
- [ ] Criar:

```http
POST /api/Idea/{id}/ai-score
```

- [ ] Restringir ao `Manager`.
- [ ] Retornar somente sugestão; não aprovar nem persistir automaticamente.
- [ ] Configurar timeout e `CancellationToken`.
- [ ] Validar JSON e scores retornados.
- [ ] Tratar indisponibilidade sem bloquear avaliação manual.
- [ ] Não registrar chaves, tokens ou prompts sensíveis.
- [ ] Registrar no `AuditLog` que houve sugestão por IA.
- [ ] Testar sucesso, timeout, JSON inválido e falha do provedor.

### Critério de demonstração

O gestor solicita uma sugestão, vê os três scores e a justificativa, podendo aceitar, ajustar ou ignorar a recomendação antes da decisão humana.

---

## 🧪 6. Testes automatizados

- [ ] Criar projeto `InovaGAB.API.Tests` com xUnit.
- [ ] Adicionar testes unitários para:
  - [ ] aprovação de ideia;
  - [ ] rejeição de ideia;
  - [ ] priorização;
  - [ ] regras de ownership;
  - [ ] transições de status;
  - [ ] cálculo de ROI individual;
  - [ ] cálculo de ROI consolidado.
- [ ] Adicionar testes de integração para:
  - [ ] login válido e inválido;
  - [ ] `401` sem token;
  - [ ] `403` por role incorreta;
  - [ ] `400` para `ObjectId` inválido;
  - [ ] vínculo com estratégia ativa/inativa;
  - [ ] CRUD de ideias;
  - [ ] CRUD de projetos;
  - [ ] fallback da IA.
- [ ] Usar MongoDB isolado para testes de integração.
- [ ] Executar testes no workflow `.github/workflows/build.yml`.
- [ ] Manter a Collection Postman como regressão manual/demonstração.

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
