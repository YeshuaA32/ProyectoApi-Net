using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using XSystem.Security.Cryptography;

namespace ApiPeliculas.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _bd;
        private string claveSecreta;
        private readonly UserManager<AppUsuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        public UsuarioRepositorio(ApplicationDbContext bd, IConfiguration config, UserManager<AppUsuario> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _bd = bd;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public AppUsuario GetUsuario(string usuarioId)
        {
            return _bd.AppUsuario.FirstOrDefault(c=> c.Id == usuarioId);
        }

        public ICollection<AppUsuario> GetUsuarios()
        {
            return _bd.AppUsuario.OrderBy(c => c.UserName).ToList();
        }

        public bool IsUniqueUser(string usuario)
        {
            var usuarioBd = _bd.AppUsuario.FirstOrDefault(u=>u.UserName==usuario);
            if (usuarioBd == null)
            {
                return true;
            }
            return false;
        }
        //public Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLogindDto)
        //{
        //    throw new NotImplementedException();
        //}
        public async Task<UsuarioLoginRespuestaDto>Login(UsuarioLoginDto usuarioLogindDto)
        {
            //var passwordEncriptado = obtenermd5(usuarioLogindDto.Password);
            var usuario = _bd.AppUsuario.FirstOrDefault(
                u => u.UserName.ToLower() == usuarioLogindDto.NombreUsuario.ToLower()
                );

            bool isValid = await _userManager.CheckPasswordAsync(usuario,usuarioLogindDto.Password);
            //validamos si  el usuario no existe con la convinacion de usuario y contrasena
            if (usuario == null || isValid==false) {
                return new UsuarioLoginRespuestaDto()
                {
                    Token = "",
                    Usuario = null
                };

            }
            //Aqui existe el usuario entonces podemos procesar el login
            var roles = await _userManager.GetRolesAsync(usuario);
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadorToken.CreateToken(tokenDescriptor);

            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario=_mapper.Map<UsuarioDatosDto>(usuario),
            };
            return usuarioLoginRespuestaDto;
        }

        public async Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistrodDto)
        {
           // var passwordEncriptado = obtenermd5(usuarioRegistrodDto.Password);

            AppUsuario usuario = new AppUsuario()
            {
                UserName = usuarioRegistrodDto.NombreUsuario,
                Email = usuarioRegistrodDto.NombreUsuario,
                NormalizedEmail=usuarioRegistrodDto.NombreUsuario.ToUpper(),
                Nombre=usuarioRegistrodDto.Nombre
            };

            var result = await _userManager.CreateAsync(usuario, usuarioRegistrodDto.Password);

            if (result.Succeeded)
            {
                if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("Registrado"));
                }
                await _userManager.AddToRoleAsync(usuario, "Admin");
                var usuarioRetornado = _bd.AppUsuario.FirstOrDefault(u=>u.UserName==usuarioRegistrodDto.NombreUsuario);

                return _mapper.Map<UsuarioDatosDto>(usuarioRetornado);
            }
            //_bd.Usuario.Add(usuario);
            //await _bd.SaveChangesAsync();
            //usuario.Password = passwordEncriptado;
            //return usuario;
            return new UsuarioDatosDto();
        }

        //Metodo para encriptar contra con MD5
        //public static string obtenermd5(string valor)
        //{
        //    MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
        //    byte[] data =System.Text.Encoding.UTF8.GetBytes(valor);
        //    data = x.ComputeHash(data);
        //    string resp = "";
        //    for (int i = 0; i < data.Length; i++)
            
        //        resp += data[i].ToString("x2").ToLower();
        //        return resp;
            
        //}


    }
}
