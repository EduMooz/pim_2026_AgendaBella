using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using AgendaBella.Negocio;
using AgendaBella.Repositorio;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;
using AgendaBella.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

// Cultura fixa para LER números e datas enviados pelos formulários.
// Sem isto, num Windows em português o valor "45.50" (enviado pelo navegador) seria
// lido como 4550. A exibição em reais (R$ 45,50) é feita nas views, com o helper Formatos.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Endereço fixo: evita que o projeto caia na porta padrão 5000 e "brigue" com o outro projeto.
builder.WebHost.UseUrls("https://localhost:7241;http://localhost:5157");

// A connection string vem do appsettings.json — nunca do código-fonte.
var connectionString = builder.Configuration.GetConnectionString("Padrao")
    ?? throw new InvalidOperationException("Connection string 'Padrao' não encontrada no appsettings.json.");

// ---------- Injeção de dependência ----------
// LISTA DE MONTAGEM do sistema: cada linha ensina o ASP.NET Core a criar uma peça.
// Quando um controller pede um AgendamentoNegocio no construtor, o ASP.NET monta
// a cadeia inteira sozinho (negócio -> repositórios -> AppDbContext).
// ATENÇÃO: este bloco é IGUAL no Program.cs da Api. Mudou aqui, mude lá também.

// 1) O banco.
builder.Services.AddDbContext<AppDbContext>(opcoes => opcoes.UseSqlServer(connectionString));

// 2) Repositórios: "quem pedir a interface recebe a classe".
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IProfissionalRepositorio, ProfissionalRepositorio>();
builder.Services.AddScoped<IServicoRepositorio, ServicoRepositorio>();
builder.Services.AddScoped<IAgendamentoRepositorio, AgendamentoRepositorio>();
builder.Services.AddScoped<IDisponibilidadeRepositorio, DisponibilidadeRepositorio>();
builder.Services.AddScoped<IListaEsperaRepositorio, ListaEsperaRepositorio>();
builder.Services.AddScoped<IPreferenciaRepositorio, PreferenciaRepositorio>();
builder.Services.AddScoped<IPontosRepositorio, PontosRepositorio>();
builder.Services.AddScoped<IRelatorioRepositorio, RelatorioRepositorio>();

// 3) Negócio: classes concretas.
builder.Services.AddScoped<AutenticacaoNegocio>();
builder.Services.AddScoped<ClienteNegocio>();
builder.Services.AddScoped<ProfissionalNegocio>();
builder.Services.AddScoped<ServicoNegocio>();
builder.Services.AddScoped<AgendamentoNegocio>();
builder.Services.AddScoped<DisponibilidadeNegocio>();
builder.Services.AddScoped<ListaEsperaNegocio>();
builder.Services.AddScoped<PontosNegocio>();
builder.Services.AddScoped<RelatorioNegocio>();
builder.Services.AddScoped<ImagemInspiracaoNegocio>();
// AddScoped = um objeto novo por requisição (cada clique), descartado quando a resposta sai.
// --------------------------------------------

// ---------- Pasta das imagens de inspiração ----------
// Relativa à pasta do projeto. O padrão "../uploads" fica ao lado das cinco pastas
// da solution, e é a MESMA da Api: uma foto enviada pelo app aparece no site.
var pastaUploads = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath, builder.Configuration["Uploads:Pasta"] ?? "../uploads"));
Directory.CreateDirectory(pastaUploads);
builder.Services.AddSingleton(new PastaUploads(pastaUploads));

// ---------- Autenticação por cookie ----------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Conta/Login";
        o.AccessDeniedPath = "/Conta/AcessoNegado";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.HttpOnly = true;               // o JavaScript da página não consegue ler o cookie
        o.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorization();

// Deixa acentos (ã, ç, é...) saírem como letras no HTML, em vez de &#xE3; etc.
builder.Services.AddWebEncoders(o => o.TextEncoderSettings =
    new TextEncoderSettings(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement));

// Proteção contra CSRF em TODO formulário POST (sem precisar escrever em cada action).
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve as imagens enviadas em /uploads/...
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(pastaUploads),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseAuthentication();   // descobre QUEM é (lê o cookie) ...
app.UseAuthorization();    // ... e depois confere se PODE (atributo [Authorize]). A ordem importa.

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
