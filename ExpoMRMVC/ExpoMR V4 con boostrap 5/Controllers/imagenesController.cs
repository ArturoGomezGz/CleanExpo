using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nancy.Json;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace ExpoMR.Controllers
{
    public class imagenesController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<string>> GetLogoProveedor(int IdProveedor)
        {
            // Por ejemplo, espera un breve período de tiempo simulando una operación asíncrona.
            //await Task.Delay(3000); // Simulando una operación que toma 1 segundo.
            var dbContext = new ApplicationDbContextExpo();

            var img = dbContext.Stands.Where(x => x.Proveedor == IdProveedor).ToList();

            if (img == null)
            {
                return "Producto no encontrado";
            }
            foreach (var imagen in img)
            {
                var img2 = ConvertirDatosBinariosABase64(imagen.Logo);
            }
            try
            {
                return ConvertirDatosBinariosABase64(img.Select(x => x.Logo).FirstOrDefault());
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<ActionResult<MyModel>> MiMetodoAsync(int parametroEntero)
        {
            try
            {
                // Aquí puedes realizar operaciones asíncronas que utilicen el parámetro entero,
                // como llamar a servicios, acceder a la base de datos, etc.

                // Por ejemplo, simulemos una operación que toma 1 segundo.
                await Task.Delay(1000);

                // Construye un objeto MyModel con el resultado
                var resultado = new MyModel
                {
                    ParametroRecibido = parametroEntero,
                    OtroDato = "Alguna información adicional"
                };

                // Devuelve el objeto como JSON al cliente
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                // Maneja cualquier error y devuelve un resultado de error
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public class MyModel
        {
            public int ParametroRecibido { get; set; }
            public string OtroDato { get; set; }
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
