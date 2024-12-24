using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMapper;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
            opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));


//agregamos lso respositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//agregamos el automapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));

//Aqui se configura la autenticacion

builder.Services.AddAuthentication(
    x =>
    { 
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    }
    
    ).AddJwtBearer(x=>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken=true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
              ValidateIssuerSigningKey = true,
              IssuerSigningKey= new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
              ValidateIssuer=false,
              ValidateAudience=false,
        };
        
    }
    );


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//SOPORTE PARA CORS
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", builder =>
{
    builder.WithOrigins("http://localhost4200").AllowAnyMethod().AllowAnyHeader();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Soporte para CORS

app.UseCors("PoliticaCors");

//soporte para autenticacion
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
