using System.Net;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V1
{
    [Route("apiv{version:apiVersion}/usuarios")]
    [ApiController]
    public class UsuariosV1Controller : ControllerBase
    {
        private readonly IUsuarioRepositorio _usRepo;

        protected RespuestaAPI _respuestaAPI;

        private readonly IMapper _mapper;

        public UsuariosV1Controller(IUsuarioRepositorio usRepo, IMapper mapper)
        {
            _usRepo = usRepo;
            _mapper = mapper;
            _respuestaAPI = new();

        }

        [Authorize(Roles = "Admin")]

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsuarios()
        {
            var listaUsuarios = _usRepo.GetUsuarios();

            var listaUsuarioDto = new List<UsuarioDto>();

            foreach (var lista in listaUsuarios)
            {
                listaUsuarioDto.Add(_mapper.Map<UsuarioDto>(lista));
            }
            return Ok(listaUsuarioDto);
        }


        [Authorize(Roles = "Admin")]

        [HttpGet("{usuarioId:int}", Name = "GetUsuario")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUsuario(int usuarioId)
        {
            var itemUsuario = _usRepo.GetUsuario(usuarioId);

            if (itemUsuario == null)
            {
                return NotFound();
            }
            var itemUsuarioDto = _mapper.Map<UsuarioDto>(itemUsuario);
            return Ok(itemUsuarioDto);
        }


        [AllowAnonymous]
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Registro([FromBody] UsuarioRegistroDto usuarioRegistroDto)
        {

            bool validarNombreUsuarioUnico = _usRepo.IsUniqueUser(usuarioRegistroDto.NombreUsuario);

            if (!validarNombreUsuarioUnico)

            {

                _respuestaAPI.StatusCode = HttpStatusCode.BadRequest;

                _respuestaAPI.IsSuccess = false;

                _respuestaAPI.ErrorMessages.Add("El nombre de usuario ya existe");

                return BadRequest(_respuestaAPI);

            }



            var usuario = await _usRepo.Registro(usuarioRegistroDto);

            if (usuario == null)

            {

                _respuestaAPI.StatusCode = HttpStatusCode.BadRequest;

                _respuestaAPI.IsSuccess = false;

                _respuestaAPI.ErrorMessages.Add("Error en el registro");

                return BadRequest(_respuestaAPI);

            }



            return CreatedAtRoute("GetUsuario", new { usuarioId = usuario.Id }, usuario);



        }


        [AllowAnonymous]
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDto usuarioLogindDto)
        {
            var respuestaLogin = await _usRepo.Login(usuarioLogindDto);

            if (respuestaLogin.Usuario == null || string.IsNullOrEmpty(respuestaLogin.Token))

            {

                _respuestaAPI.StatusCode = HttpStatusCode.BadRequest;

                _respuestaAPI.IsSuccess = false;

                _respuestaAPI.ErrorMessages.Add("El nombre de usuario o password son incorrectos");

                return BadRequest(_respuestaAPI);

            }

            _respuestaAPI.StatusCode = HttpStatusCode.OK;

            _respuestaAPI.IsSuccess = true;

            _respuestaAPI.Result = respuestaLogin;

            return Ok(_respuestaAPI);



        }


    }
}
