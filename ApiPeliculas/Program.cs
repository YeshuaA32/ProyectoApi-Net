using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMapper;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using XAct;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
            opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

//Soporte para cache
builder.Services.AddResponseCaching(
);

//agregamos lso respositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//Soporte para mantenimiento

var apiVersioningBuilder = builder.Services.AddApiVersioning(opcion =>
{
opcion.AssumeDefaultVersionWhenUnspecified = true;
opcion.DefaultApiVersion = new ApiVersion(1, 0);
opcion.ReportApiVersions = true;
//opcion.ApiVersionReader = ApiVersionReader.Combine(
//            new QueryStringApiVersionReader("Api-version")
//            //?api-version=1.0
//            //new HeaderApiVersionReader(X-Version),
//            //new MedialTypeApiVersionReader("ver));
//            );
    }
);

apiVersioningBuilder.AddApiExplorer(
        opciones =>
        {
            opciones.GroupNameFormat = "'v'VVV";
            opciones.SubstituteApiVersionInUrl= true;

        }
     );

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


builder.Services.AddControllers(
    opcion=>
        {
            opcion.CacheProfiles.Add("PorDefecto30Segundos", new CacheProfile() { Duration =30});
        }
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options=>

    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description=
            "Autenticacion JWT usando el esquema Bearer. \r\n\r\n " +
            "Ingresa la palabra 'Bearer' seguido de un [espacio] y despues su token en el campo de abajo  \r\n\r\n " +
            "Ejemplo: \"Bearer tkmlfsdff\"",
            Name = "Authorization",
            In=ParameterLocation.Header,
            Scheme="Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference=new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id= "Bearer"
                    },
                    Scheme="oauth2",
                    Name="Bearer",
                    In=ParameterLocation.Header
                },
                new List<string>()
            }

        });
      }
    );

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
