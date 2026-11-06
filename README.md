# AgendaBella — PIM IV (ADS / UNIP)

Sistema de agendamento para o estúdio de manicure da **Erika Santana**, montado sobre a
estrutura base do professor (5 projetos em camadas) e com um aplicativo Flutter para a cliente.

```
AgendaBella.sln
│
├── Banco/                      Scripts SQL Server: tabelas, triggers, procedures e dados
├── AgendaBella.Modelos         Biblioteca de classes: entidades e DTOs (não depende de ninguém)
├── AgendaBella.Repositorio     Biblioteca de classes: ÚNICA camada que acessa o banco (EF Core + Dapper)
├── AgendaBella.Negocio         Biblioteca de classes: TODAS as regras de negócio
├── AgendaBella.Web             ASP.NET Core MVC (Razor): site da cliente + painel da profissional
├── AgendaBella.Api             ASP.NET Core Web API: back-end do aplicativo (token JWT)
└── AgendaBella.App/            Aplicativo Flutter da cliente (abre no VS Code, não no Visual Studio)
```

Sentido das referências (igual ao manual):

```
Web ──┐
      ├──► Negocio ──► Repositorio ──► Modelos ──► (nada)
Api ──┘
App Flutter ──HTTP/JSON──► Api
```

## O que o sistema faz

**Cliente (site e aplicativo):** cadastro e login · agendamento com horários livres calculados
pelo banco · foto de inspiração e observação no agendamento · reagendar e cancelar (até 2 h antes) ·
histórico de cada agendamento · lista de espera com aviso de vaga liberada · preferências de unha ·
pontos de fidelidade.

**Profissional (site, painel):** painel com indicadores · agenda do dia com foto e preferências da
cliente · confirmar, concluir e cancelar · serviços (CRUD) · expediente e bloqueios de agenda ·
lista de espera · clientes e resgate de pontos · relatórios com previsão de demanda.

## Logins de demonstração (criados pelos scripts 04 e 05)

| Perfil | E-mail | Senha |
|---|---|---|
| Profissional (painel) | erika@agendabella.com.br | Erika@123 |
| Cliente | cliente@agendabella.com.br | Cliente@123 |

As senhas ficam no banco como **hash PBKDF2**, nunca em texto puro.

---

## Como rodar pela primeira vez

### 0. Programas necessários

- Visual Studio 2022 (17.12 ou mais novo) com a carga "ASP.NET e desenvolvimento Web" e o **.NET 9 SDK** (o projeto também roda com .NET 10).
- SQL Server (o Express serve) e o **SSMS**.
- Para o aplicativo: **Flutter SDK**, **VS Code** com as extensões *Flutter* e *Dart*, e um emulador Android (Android Studio) ou o Google Chrome.

### 1. Criar o banco (SSMS)

Abra cada arquivo da pasta `Banco` no SSMS e execute (F5), **nesta ordem**:

1. `01_criar_banco_e_tabelas.sql` — cria o banco `AgendaBella`, as 11 tabelas, chaves e índices
2. `02_triggers.sql` — as 3 triggers da tabela Agendamento
3. `03_procedures.sql` — as 4 stored procedures
4. `04_dados_iniciais.sql` — profissional, serviços, grade de horários e expediente
5. `05_dados_demonstracao.sql` — *(opcional)* clientes e 8 semanas de histórico para os relatórios

Diferente do template, aqui o banco **não** é criado por migration: os scripts são o
modelo físico pedido na Etapa 3 e deixam trigger e procedure visíveis para a banca.

### 2. Conferir a connection string

Nos **dois** `appsettings.json` (Web e Api), chave `Padrao`:

```
Server=localhost;Database=AgendaBella;Trusted_Connection=True;TrustServerCertificate=True
```

Se o seu SQL Server for o Express, troque por `Server=localhost\\SQLEXPRESS;` (barra dupla no JSON).
A explicação completa de cada parte está no README do professor, seção "Apontar para o seu SQL Server".

### 3. Rodar a Web e a Api

**Uma única vez por computador**, crie o certificado de desenvolvimento (o site usa https).
No Visual Studio: *Exibir › Terminal* e rode, um de cada vez (responda **Sim** nas janelas do Windows):

```
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Depois, na barra de cima do Visual Studio, escolha o perfil **Web + Api** (ele já vem no arquivo
`AgendaBella.slnLaunch`) e aperte **F5**. Abrem duas janelas pretas (o site e a Api); **deixe as duas abertas**.

Se o perfil **Web + Api** não aparecer: botão direito na solution › *Configurar Projetos de Inicialização* ›
*Vários projetos de inicialização* › **Iniciar** em `AgendaBella.Web` e `AgendaBella.Api` › **F5**.
Se o navegador não abrir sozinho, digite os endereços abaixo.

- Site: `https://localhost:7241` (ou `http://localhost:5157`)
- Api: `http://localhost:5218/scalar` (tela para testar os endpoints)

As portas estão fixadas no `Program.cs` de cada projeto (`builder.WebHost.UseUrls(...)`), para que a Web e a
Api nunca tentem usar a mesma porta.

Pelo terminal (na pasta do `.sln`): `dotnet run --project AgendaBella.Api` e, em outro terminal,
`dotnet run --project AgendaBella.Web`.

### 4. Rodar o aplicativo Flutter

Com a **Api rodando**, abra a pasta `AgendaBella.App` no VS Code e, no terminal:

```
flutter create . --project-name agendabella_app --org br.com.agendabella --platforms android,ios,web,windows
flutter pub get
flutter run
```

O `flutter create .` só gera as pastas de plataforma que faltam (android, ios, web, windows);
ele **não** apaga o código da pasta `lib`. Detalhes em `AgendaBella.App/README.md`.

---

## Endpoints da Api

| Verbo | Endereço | O que faz | Resposta |
|---|---|---|---|
| POST | `/api/auth/login` | login, devolve o token | 200 ou 401 |
| POST | `/api/auth/cadastro` | cria a conta e já devolve o token | 201 ou 400 |
| GET | `/api/auth/eu` | dados do dono do token | 200 ou 401 |
| GET | `/api/servicos` | catálogo (público) | 200 |
| GET | `/api/agendamentos` | agendamentos da cliente | 200 |
| GET | `/api/agendamentos/horarios?data=&idServico=` | horários livres (**stored procedure**) | 200 |
| POST | `/api/agendamentos` | novo agendamento | 201 + Location, ou 400 |
| POST | `/api/agendamentos/{id}/imagem` | envia a foto de inspiração | 200, 400 ou 404 |
| PATCH | `/api/agendamentos/{id}/reagendar` | muda dia/horário | 204, 400 ou 404 |
| PATCH | `/api/agendamentos/{id}/cancelar` | cancela | 204, 400 ou 404 |
| GET | `/api/agendamentos/{id}/historico` | histórico (gravado por **trigger**) | 200 ou 404 |
| GET / POST | `/api/lista-espera` | ver / entrar na lista de espera | 200 / 201 |
| DELETE | `/api/lista-espera/{id}` | sair da lista de espera | 204 ou 400 |
| GET / PUT | `/api/perfil/preferencias` | preferências de unha | 200 / 204 |
| GET | `/api/perfil/pontos` | saldo e extrato de pontos | 200 |

Os endpoints de agendamento, lista de espera e perfil exigem o cabeçalho
`Authorization: Bearer <token>`. O arquivo `AgendaBella.Api/AgendaBella.Api.http`
traz todas as chamadas prontas, na ordem.

## Onde cada exigência do manual está

| Exigência | Onde |
|---|---|
| 5 projetos, referências no sentido certo | `AgendaBella.sln` e os `.csproj` |
| Injeção de dependência no Program.cs da Web e da Api | `AgendaBella.Web/Program.cs`, `AgendaBella.Api/Program.cs` |
| SQL Server, EF Core e Dapper | `AgendaBella.Repositorio` |
| Ao menos 1 trigger | `Banco/02_triggers.sql` (3 triggers) + `HasTrigger` no `AppDbContext` |
| Ao menos 1 procedure executada pelo sistema | `Banco/03_procedures.sql` (4) — chamadas em `AgendamentoRepositorio` e `RelatorioRepositorio` |
| Consultas parametrizadas e connection string no appsettings | todos os repositórios / `appsettings.json` |
| Regras só no Negocio, controllers finos, DTOs e ViewModels | `AgendaBella.Negocio`, `Controllers`, `Modelos/Dtos`, `Web/ViewModels` |
| Herança, encapsulamento e polimorfismo | `Usuario` → `Cliente`/`Profissional`; `Agendamento.Status` com `private set` |
| REST com códigos HTTP e OpenAPI | `AgendaBella.Api/Controllers`, `/openapi/v1.json`, `/scalar` |
| App Flutter consumindo a Api, com função complementar | `AgendaBella.App` (lista de espera com aviso de vaga, foto pela câmera) |
| Web responsiva e acessível | Bootstrap + `wwwroot/css/site.css` (contraste AA, foco visível, VLibras) |
| Análise de dados / previsão | `RelatorioNegocio.CalcularPrevisao` (regressão linear) e tela Relatórios |

## Rodar em outro computador (ex.: notebook da apresentação)

O banco **não** viaja dentro do zip: cada computador precisa ter o próprio SQL Server com o banco criado.
No computador novo:

1. Instale o Visual Studio (carga "ASP.NET e desenvolvimento Web" + .NET 9), o SQL Server (Express ou Developer) e o SSMS.
2. No SSMS, conecte e anote o **Nome do servidor** que aparece na tela de conexão.
3. Execute os scripts `01` a `05` da pasta `Banco` (veja o passo 1).
4. Ajuste a chave `Padrao` nos **dois** `appsettings.json` (Web e Api):
   - SQL Server com login do Windows: `Server=localhost;Database=AgendaBella;Trusted_Connection=True;TrustServerCertificate=True`
   - SQL Server Express: `Server=localhost\\SQLEXPRESS;Database=AgendaBella;Trusted_Connection=True;TrustServerCertificate=True`
   - Login `sa` (Autenticação do SQL Server): `Server=localhost;Database=AgendaBella;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True`
5. Crie o certificado (`dotnet dev-certs https --trust`) e rode com **F5** (passo 3).

Tudo fica dentro do notebook, então **não precisa de internet** na apresentação (só o VLibras, que vem da
internet, não aparece sem rede). Faça um ensaio completo no notebook alguns dias antes.

## Problemas comuns

| Aparece | Causa e correção |
|---|---|
| `Failed to bind to address http://127.0.0.1:5000: address already in use` | A Web e a Api tentaram a mesma porta. Confira a linha `UseUrls` no `Program.cs` de cada projeto: Web = `https://localhost:7241;http://localhost:5157`, Api = `http://localhost:5218`. Se persistir, finalize os processos AgendaBella no Gerenciador de Tarefas. |
| `Unable to configure HTTPS endpoint. No server certificate was specified` | Falta o certificado de desenvolvimento: rode `dotnet dev-certs https --trust`. |
| Janela "Terminações de linha inconsistentes" | Só formatação. Escolha *Windows (CR LF)* › **Sim**. |
| Navegador avisa "conexão não é particular" | Certificado não confiável. Use `http://localhost:5157`. |
| `error: 26 - Error Locating Server/Instance Specified` | O `Server=` não é o seu SQL Server. Corrija nos **dois** `appsettings.json`. |
| `Cannot open database "AgendaBella"` | Os scripts da pasta `Banco` não foram executados. |
| `Invalid object name 'sp_Horarios_Disponiveis'` | Faltou rodar o `03_procedures.sql`. |
| Login de demonstração não funciona | Faltou rodar o `04` (profissional) ou o `05` (cliente). |
| No app: "Sem conexão com o servidor" | A Api não está rodando, ou o celular físico não enxerga `localhost` (veja `AgendaBella.App/README.md`). |
| `Cleartext HTTP traffic not permitted` (Android) | Use o `AndroidManifest.xml` que já vem na pasta do app. |
