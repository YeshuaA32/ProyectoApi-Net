using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface IUsuarioRepositorio
    {
        ICollection<Usuario> GetUsuarios();

        Usuario GetUsuario(int usuarioId);
        bool IsUniqueUser(string usuario);

        Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLogindDto);
        Task<Usuario> Registro(UsuarioRegistroDto usuarioRegistrodDto);
    }
}
