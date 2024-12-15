using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.IdentityModel.Tokens;
using XSystem.Security.Cryptography;

namespace ApiPeliculas.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _bd;
        private string claveSecreta;
        public UsuarioRepositorio(ApplicationDbContext bd, IConfiguration config)
        {
            _bd = bd;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
        }

        public Usuario GetUsuario(int usuarioId)
        {
            return _bd.Usuario.FirstOrDefault(c=> c.Id == usuarioId);
        }

        public ICollection<Usuario> GetUsuarios()
        {
            return _bd.Usuario.OrderBy(c => c.NombreUsuario).ToList();
        }

        public bool IsUniqueUser(string usuario)
        {
            var usuarioBd = _bd.Usuario.FirstOrDefault(u=>u.NombreUsuario==usuario);
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
            var passwordEncriptado = obtenermd5(usuarioLogindDto.Password);
            var usuario = _bd.Usuario.FirstOrDefault(
                u => u.NombreUsuario.ToLower() == usuarioLogindDto.NombreUsuario.ToLower()
                 && u.Password==passwordEncriptado
                );
            //validamos si  el usuario no existe con la convinacion de usuario y contrasena
            if (usuario == null) {
                return new UsuarioLoginRespuestaDto()
                {
                    Token = "",
                    Usuario = null
                };

            }
            //Aqui existe el usuario entonces podemos procesar el login
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario.ToString()),
                    new Claim(ClaimTypes.Role, usuario.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadorToken.CreateToken(tokenDescriptor);

            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario=usuario
            };
            return usuarioLoginRespuestaDto;
        }

        public async Task<Usuario> Registro(UsuarioRegistroDto usuarioRegistrodDto)
        {
            var passwordEncriptado = obtenermd5(usuarioRegistrodDto.Password);

            Usuario usuario = new Usuario()
            {
                NombreUsuario = usuarioRegistrodDto.NombreUsuario,
                Password = passwordEncriptado,
                Nombre=usuarioRegistrodDto.Nombre,
                Role=usuarioRegistrodDto.Role
            };
            _bd.Usuario.Add(usuario);
            await _bd.SaveChangesAsync();
            usuario.Password = passwordEncriptado;
            return usuario;
        }

        //Metodo para encriptar contra con MD5
        public static string obtenermd5(string valor)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] data =System.Text.Encoding.UTF8.GetBytes(valor);
            data = x.ComputeHash(data);
            string resp = "";
            for (int i = 0; i < data.Length; i++)
            
                resp += data[i].ToString("x2").ToLower();
                return resp;
            
        }


    }
}
