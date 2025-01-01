using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface IUsuarioRepositorio
    {
        //ICollection<Usuario> GetUsuarios();
        ICollection<AppUsuario> GetUsuarios();

        AppUsuario GetUsuario(string usuarioId);
        bool IsUniqueUser(string usuario);

        Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLogindDto);
        //Task<Usuario> Registro(UsuarioRegistroDto usuarioRegistrodDto);

        Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistrodDto);
    }
}
