using System.Globalization;
using System.Text;
using AgendaBella.Api.Seguranca;
using AgendaBella.Negocio;
using AgendaBella.Repositorio;
using AgendaBella.Repositorio.Contexto;
using AgendaBella.Repositorio.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

// Mesma cultura de leitura usada na Web (números com ponto decimal, datas ISO).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Endereço fixo: evita que o projeto caia na porta padrão 5000 e "brigue" com o outro projeto.
builder.WebHost.UseUrls("http://localhost:5218");

// A connection string vem do appsettings.json — nunca do código-fonte.
var connectionString = builder.Configuration.GetConnectionString("Padrao")
    ?? throw new InvalidOperationException("Connection string 'Padrao' não encontrada no appsettings.json.");

// ---------- Injeção de dependência ----------
// ATENÇÃO: este bloco é IGUAL no Program.cs da Web. Mudou aqui, mude lá também.

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
// --------------------------------------------

// ---------- Pasta das imagens (a MESMA da Web: "../uploads", ao lado dos projetos) ----------
var pastaUploads = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath, builder.Configuration["Uploads:Pasta"] ?? "../uploads"));
Directory.CreateDirectory(pastaUploads);
builder.Services.AddSingleton(new PastaUploadsApi(pastaUploads));

// ---------- Autenticação por token JWT (o app Flutter não usa cookie) ----------
var jwt = builder.Configuration.GetSection("Jwt").Get<ConfiguracaoJwt>()
    ?? throw new InvalidOperationException("Seção 'Jwt' não encontrada no appsettings.json.");
if (jwt.Chave.Length < 32)
    throw new InvalidOperationException("A chave Jwt:Chave precisa ter pelo menos 32 caracteres.");
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<GeradorDeToken>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;   // mantém os nomes curtos das claims: sub, name, email, role
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emissor,
            ValidateAudience = true,
            ValidAudience = jwt.Publico,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"    // é esta claim que o [Authorize(Roles = "Cliente")] confere
        };
    });
builder.Services.AddAuthorization();

// CORS: permite que o app Flutter rodando no NAVEGADOR (flutter run -d chrome) chame a Api.
// No celular/emulador o CORS não se aplica. Em produção, troque AllowAnyOrigin pelo endereço real.
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// O filtro transforma RegraDeNegocioException em 400 { "erro": "..." } em todos os endpoints.
builder.Services.AddControllers(o => o.Filters.Add<FiltroRegraDeNegocio>());

// OpenAPI: documento com todos os endpoints em /openapi/v1.json (lido pelo Scalar).
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();   // tela para testar os endpoints em /scalar
}
else
{
    // Fora do desenvolvimento, força HTTPS. Em desenvolvimento fica desligado para o emulador
    // Android conseguir chamar http://10.0.2.2:5218 sem certificado.
    app.UseHttpsRedirection();
}

// CORS antes dos arquivos estáticos: no navegador, o Flutter baixa as fotos por JavaScript
// e precisa do cabeçalho de CORS também nas imagens.
app.UseCors();

// Imagens de inspiração em /uploads/... (o app mostra a foto pelo mesmo caminho salvo no banco).
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(pastaUploads),
    RequestPath = "/uploads"
});

app.UseAuthentication();   // lê o token do cabeçalho Authorization ...
app.UseAuthorization();    // ... e confere o [Authorize]. A ordem importa.

app.MapControllers();

app.Run();
