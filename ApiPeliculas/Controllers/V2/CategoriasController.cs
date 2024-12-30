using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V2
{
    [Authorize(Roles = "Admin")]
    //[ResponseCache(Duration =20)]
    // [ResponseCache(CacheProfileName= "PorDefecto30Segundos")]
    //[Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/categorias")]
    [ApiController]
    // [EnableCors("PoliticaCors")]
    [ApiVersion("2.0")]
    [Obsolete("Este endpoint esta obsoleta, por favor utilice la version 1.0")]

    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _ctRepo;

        private readonly IMapper _mapper;

        public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;

        }


        [HttpGet("GetString")]
        [AllowAnonymous]

        [MapToApiVersion("2.0")]
        public IEnumerable<string> Get()
        {
            return new string[] { "Yeshua", "Benjamin", "Alonzo" };
        }



    }
}
