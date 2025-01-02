using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V1
{

    [Route("api/v{version:apiVersion}/peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _pelRepo;

        private readonly IMapper _mapper;

        public PeliculasController(IPeliculaRepositorio pelRepo, IMapper mapper)
        {
            _pelRepo = pelRepo;
            _mapper = mapper;

        }

        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(CacheProfileName = "PorDefecto30Segundos")]

        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetPeliculas()
        {
            var listaPeliculas = _pelRepo.GetPeliculas();

            var listaPeliculasDto = new List<PeliculaDto>();

            foreach (var lista in listaPeliculas)
            {
                listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(lista));
            }
            return Ok(listaPeliculasDto);
        }


        [AllowAnonymous]
        [ResponseCache(CacheProfileName = "PorDefecto30Segundos")]

        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPelicula(int peliculaId)
        {
            var itemPelicula = _pelRepo.GetPelicula(peliculaId);

            if (itemPelicula == null)
            {
                return NotFound();
            }
            var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);
            return Ok(itemPeliculaDto);
        }





        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }
            if (crearPeliculaDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_pelRepo.ExistePelicula(crearPeliculaDto.Nombre))
            {
                ModelState.AddModelError("", "La pelicula ya existe");
                return StatusCode(404, ModelState);
            }
            var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);
            //if (!_pelRepo.CrearPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salio mal guardando el registro{pelicula.Nombre}");
            //    return StatusCode(404, ModelState);
            //}

            //Subida de archivos

            if (crearPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.id + System.Guid.NewGuid().ToString() + Path.GetExtension(crearPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file= new FileInfo(ubicacionDirectorio);
                if (file.Exists) 
                { 
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                   crearPeliculaDto.Imagen.CopyTo(fileStream);
                }
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;
            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600/400";
            }

            _pelRepo.CrearPelicula(pelicula);
            return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.id }, pelicula);
        }




        [Authorize(Roles = "Admin")]

        [HttpPatch("{peliculaId:int}", Name = "ActualizarPatchPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ActualizarPatchPelicula(int peliculaId, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }
            if (actualizarPeliculaDto == null || peliculaId != actualizarPeliculaDto.id)
            {
                return BadRequest(ModelState);
            }
            var peliculaExistente = _pelRepo.GetPelicula(peliculaId);
            if (peliculaExistente == null)
            {
                return NotFound($"No se encontro la pelicula con Id {peliculaExistente}");
            }

            var pelicula = _mapper.Map<Pelicula>(actualizarPeliculaDto);

            //if (!_pelRepo.ActualizarPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salio mal actualizando el registro{pelicula.Nombre}");
            //    return StatusCode(500, ModelState);
            //}


            //Subida de archivos

            if (actualizarPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.id + System.Guid.NewGuid().ToString() + Path.GetExtension(actualizarPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);
                if (file.Exists)
                {
                    file.Delete();
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    actualizarPeliculaDto.Imagen.CopyTo(fileStream);
                }
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;
            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600/400";
            }

            _pelRepo.ActualizarPelicula(pelicula);
            return NoContent();
        }






        [Authorize(Roles = "Admin")]

        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarPelicula(int peliculaId)
        {


            if (!_pelRepo.ExistePelicula(peliculaId))
            {
                return NotFound();
            }

            var pelicula = _pelRepo.GetPelicula(peliculaId);

            if (!_pelRepo.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salio mal borrando el registro{pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("GetPeliculasEnCategorias/{categoriaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPeliculasEnCategorias(int categoriaId)
        {
            var listaPeliculas = _pelRepo.GetPeliculasEnCategoria(categoriaId);

            if (listaPeliculas == null)
            {
                return NotFound();

            }
            var itemPeliculas = new List<PeliculaDto>();
            foreach (var pelicula in listaPeliculas)
            {
                itemPeliculas.Add(_mapper.Map<PeliculaDto>(pelicula));
            }

            return Ok(itemPeliculas);
        }




        [AllowAnonymous]
        [HttpGet("Buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Buscar(string nombre)
        {
            try
            {
                var resultados = _pelRepo.BuscarPelicula(nombre);
                if (resultados.Any())
                {
                    return Ok(resultados);
                }
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error recuperando datos");
            }

        }
    }
}
