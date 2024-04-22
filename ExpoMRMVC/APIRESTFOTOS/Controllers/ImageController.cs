using APIRESTFOTOS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace APIRESTFOTOS.Controllers
{
    public class ImageController : ControllerBase
    {
        //[Route("api/[controller]")]
        //[ApiController]

        public readonly FotosContext _dbcontext;

        public ImageController(FotosContext _context)
        {
            _dbcontext = _context;  
        }

        [HttpGet]
        [Route("GetFoto/{IdProducto:int}")]

        public IActionResult GetFoto(int IdProducto)
        {
            //MFoto oFoto = _dbcontext.MFotos.Find(IdProducto);
            var img = _dbcontext.MFotos.Where(imagen => imagen.IdProducto == IdProducto).ToList();

            if (img == null)
            {
                return BadRequest("Producto no encontrado");
            }
            foreach (var imagen in img)
            {
                var img2 =  ConvertirDatosBinariosABase64(imagen.Foto);

            }
            try
            {
                /*oFoto = _dbcontext.MFotos.Where(f => f.IdProducto == IdProducto).FirstOrDefault()*/;
                
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", Response = img });
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string ConvertirDatosBinariosABase64(byte[] datosBinarios)
        {
            if (datosBinarios == null || datosBinarios.Length == 0)
            {
                return null;
            }

            return Convert.ToBase64String(datosBinarios);
        }

    }
}
