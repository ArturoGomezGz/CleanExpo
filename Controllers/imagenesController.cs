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
using System.Net.Mail;
using System.Threading.Tasks;


namespace ExpoMR.Controllers
{
    public class imagenesController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<string>> GetLogoProveedor(int IdProveedor)
        {
            StandVIewModel Logo = new StandVIewModel();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProveedor", IdProveedor);
            SqlDataReader dr = dB.ExecuteReader("GetLogoProv", ref parameters);

            while (dr.Read())
            {               
                Logo.Proveedor = (int)dr["Proveedor"];
                Logo.Nombre = (string)dr["Nombre"];
                Logo.Logo = (dr["Logo"] != DBNull.Value ? (byte[])dr["Logo"] : Array.Empty<byte>());
            }

            if (Logo == null)
                return "Logo de proveedor no encontrado";

            try
            {
                return WebSite.ConvertToBase64(Logo.Logo);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetFotoProducto(int IdProducto)
        {
            byte[] fotos = Array.Empty<byte>();

            DB dB = new();
            SqlParameter[] parameters = new SqlParameter[1];
            parameters[0] = new SqlParameter("@IdProducto", IdProducto);

            SqlDataReader dr = dB.ExecuteReader("GetFotoProd", ref parameters);

            while (dr.Read())
            {
                fotos = (dr["Foto"] != DBNull.Value ? (byte[])dr["Foto"] : Array.Empty<byte>());
            }

            if (fotos == null || fotos.Length == 0)
                return "Foto de producto no encontrada";

            try
            {
                return WebSite.ConvertToBase64(fotos);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}