using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ExpoMR.Controllers
{
    //[Authorize(Roles = "Proveedor")]
    public class ProveedoresController : Controller
    {
        private static IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public ProveedoresController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _configuration = config;
            _userManager = userManager;

        }

        private int getProveedor()
        {
            var user = _userManager.GetUserAsync(User);
            string username = user.Result.UserName.ToString();
            return GeneralesController.GetIdProveedor(username);
        }

        public IActionResult Articulos()
        {
            return View();
        }

        public IActionResult Precios(int idCliente = 0)
        {
            return View(idCliente);
        }

        public ActionResult Anuncios()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetAnuncios()
        {
            int idProveedor = getProveedor();

            string query = "SELECT *"
                           + " FROM mAnuncios"
                           + " WHERE Proveedor = " + idProveedor;
            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);


            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idAnuncio = col[0],
                           proveedor = col[1],
                           anuncio = col[2],
                           feInicio = DateTime.Parse(col[3].ToString()).ToString("dd-MM-yyyy"),
                           horaInicio = DateTime.Parse(col[3].ToString()).ToString("hh:mm tt"),
                           feTermino = DateTime.Parse(col[4].ToString()).ToString("dd-MM-yyyy"),
                           horaTermino = DateTime.Parse(col[4].ToString()).ToString("hh:mm tt"),
                           rank = col[5]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetAnuncio(int idAnuncio)
        {
            string query = "SELECT *"
                           + " FROM mAnuncios"
                           + " WHERE IdAnuncio = " + idAnuncio;
            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idAnuncio = col[0],
                           proveedor = col[1],
                           anuncio = col[2],
                           feInicio = DateTime.Parse(col[3].ToString()).ToString("yyyy-MM-dd"),
                           horaInicio = DateTime.Parse(col[3].ToString()).ToString("HH:mm"),
                           feTermino = DateTime.Parse(col[4].ToString()).ToString("yyyy-MM-dd"),
                           horaTermino = DateTime.Parse(col[4].ToString()).ToString("HH:mm"),
                           rank = col[5]
                       }).ToArray()
            };

            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public void GuardarAnuncio(int idAnuncio, string descripcion, string feInicio, string feTermino)
        {
            int idProveedor = getProveedor();
            string  query;

            feInicio = convierteFecha(feInicio);
            feTermino = convierteFecha(feTermino);

            if (idAnuncio == 0)
            {
                query = "INSERT INTO mAnuncios"
                        + " VALUES ( " + idProveedor + ",'" + descripcion + "','" + feInicio + "','" + feTermino + "', 1 )";
            }
            else
            {
                query = "UPDATE mAnuncios"
                        + " SET Anuncio = '" + descripcion + "', FeInicio = '" + feInicio + "', FeTermino = '" + feTermino + "'"
                        + " WHERE IdAnuncio = " + idAnuncio;
            }

            GeneralesController.sqlQuery("ExpoMRCS", query);
        }

        public string convierteFecha(string fecha)
        {
            string[] split = fecha.Split(' ');

            string[] splitFecha = split[0].Split('-');

            return splitFecha[2] + "/" + splitFecha[1] + "/" + splitFecha[0] + " " + split[2];
        }

        [HttpPost]
        public void eliminarAnuncio(int idAnuncio)
        {
            string query = "DELETE mAnuncios"
                           + " WHERE idAnuncio = " + idAnuncio;
            GeneralesController.sqlQuery("ExpoMRCS", query);
        }

        public ActionResult Monedas()
        {
            return View();
        }

        [HttpPost]
        public JsonResult getMonedas()
        {
            int idProveedor = getProveedor();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("MonedasSaldo", cnn);
            command.Parameters.AddWithValue("@idpersona", idProveedor);
            command.CommandType = CommandType.StoredProcedure;
            cnn.Open();

            SqlParameter returnParameter = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            command.ExecuteNonQuery();

            cnn.Close();

            return Json((int)returnParameter.Value);
        }

        public IActionResult AsignaMonedas(int idCliente)
        {

            return View(idCliente);
        }

        [HttpPost]
        public JsonResult getClienteAsignar(int idCliente)
        {
            string query = "Select idPersona AS idCliente, Nombre"
                           + " From mClientes"
                           + " Where idPersona = " + idCliente;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);


            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idCliente = col[0],
                           nombre = col[1]
                       }).ToArray()
            };


            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public JsonResult getMonedasContacto(int idCliente)
        {
            int idProveedor = getProveedor();

            string query = "Select SUM(MonedasGanadas) As MonedasContacto, SUM(Monedas) As Monedas"
                           + " From (Select mTransacciones.Monedas As MonedasGanadas, 0 as Monedas"
                           + " From mTransacciones"
                           + " Where mTransacciones.Origen = " + idProveedor
                           + " And mTransacciones.Destino = " + idCliente + " And mTransacciones.IdMotivo = 6"
                           + " union"
                           + " Select 0 As MonedasGanadas,"
                           + " dbo.GetMonedas(" + idProveedor + ", " + idCliente + ", 6, GETDATE(), null) as Monedas)T1";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);


            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           monedasContacto = col[0],
                           monedas = col[1]
                       }).ToArray()
            };


            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public JsonResult AsignarMonedas(int idCliente, int tipoAsignacion, int monedas)
        {
            int idProveedor = getProveedor();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("MonedasAsignar", cnn);
            command.Parameters.AddWithValue("@origen", idProveedor);
            command.Parameters.AddWithValue("@destino", idCliente);
            command.Parameters.AddWithValue("@idproducto", null);
            command.Parameters.AddWithValue("@usuario", idProveedor);
            if (tipoAsignacion == 1)
            {
                command.Parameters.AddWithValue("@idmotivo", 12);
                command.Parameters.AddWithValue("@monedas", monedas);
            }
            else
            {
                command.Parameters.AddWithValue("@idmotivo", 6);
                command.Parameters.AddWithValue("@monedas", null);
            }

            command.CommandType = CommandType.StoredProcedure;
            cnn.Open();

            SqlParameter returnParameter = command.Parameters.Add("@success", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            command.ExecuteNonQuery();

            cnn.Close();

            return Json((int)returnParameter.Value);
        }

        public IActionResult DetalleArticulo(int IdArticulo)
        {

            return View(IdArticulo);
        }

        public IActionResult CargarArticulo(string Nombre, string Descripcion, int CantidadArticulo)
        {
            int IdProveedor = getProveedor();
            string query = "DECLARE @ID INT"
                           + " INSERT mArticulos (Nombre, Descripcion, Cantidad, Proveedor)"
                           + " VALUES ('" + Nombre + "', '" + Descripcion + "'," + CantidadArticulo + ", " + IdProveedor + ") ";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult GetClientes()
        {
            int idProveedor = getProveedor();

            string query = "Select lVisita.Cliente, mClientes.Nombre"
                           + " From lVisita inner join"
                           + " mClientes on lVisita.Cliente = mClientes.IdPersona"
                           + " Where lVisita.IdProducto = 0"
                           + " And lVisita.Proveedor = " + idProveedor
                           + " And lVisita.Fecha > DATEADD(MINUTE, -CAST(dbo.GetSysVar('MinUltimoCliente') as int), GETDATE())"
                           + " Group by lVisita.Cliente, mClientes.Nombre"
                           + " Order by mClientes.Nombre";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);


            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           cliente = col[0],
                           nombre = col[1]
                       }).ToArray()
            };


            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult GetArticulos()
        {
            int IdProveedor = getProveedor();

            string query = "SELECT mArticulos.IdArticulo, mArticulos.Nombre, mArticulos.Descripcion,"
                           + " (SELECT TOP 1 dArticulos.IDdArticulos "
                           + " FROM dArticulos "
                           + " WHERE dArticulos.IdArticulo = mArticulos.IdArticulo "
                           + " ORDER BY dArticulos.NumFoto ASC) AS IDdArticulos"
                           + " FROM mArticulos"
                           + " WHERE mArticulos.Proveedor = " +IdProveedor
                           + " --AND mArticulos.Nombre LIKE '%' + @Filtro + '%'"
                           + " ORDER BY mArticulos.Nombre";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idarticulo = col[0],
                           nombre = col[1],
                           descripcion = col[2],
                           imgarticulo = GetFotosArticulos("item", col[3] == DBNull.Value ? 0 : Convert.ToInt32(col[3]))
                       }).ToArray()
            };

            return Json(jsonData.rows);

        }

        [HttpPost]
        public static byte[] GetFotosArticulos(string Tipo, int Id)
        {
            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand command = new SqlCommand("ExpoGetImage", conexion);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            conexion.Open();
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Type", Tipo));
            command.Parameters.Add(new SqlParameter("@Id", Id));
            command.Connection = conexion;
            sqlDA = new SqlDataAdapter(command);
            sqlDA.Fill(dataTable);
            conexion.Close();

            byte[] fotos = new byte[dataTable.Rows.Count];


            foreach (DataRow row in dataTable.Rows)
            {
                fotos = row.Field<byte[]>(0);
            }


            return fotos;

        }

        [HttpPost]
        public JsonResult articuloSeleccionado(int IdArticulo)
        {
            string query = "SELECT mArticulos.IdArticulo, mArticulos.Nombre, mArticulos.Descripcion, mArticulos.Cantidad"
                           + " FROM mArticulos"
                           + " WHERE mArticulos.IdArticulo =" + IdArticulo;

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           idarticulo = col[0],
                           nombre = col[1],
                           descripcion = col[2],
                           cantidad = col[3]

                       }).ToArray()
            };
            return Json(jsonData.rows[0]);
        }

        [HttpPost]
        public JsonResult imgArticulo(int IdArticulo)
        {
            string query = "SELECT dArticulos.IDdArticulos, dArticulos.NumFoto"
                           + " FROM dArticulos"
                           + " WHERE dArticulos.IdArticulo =" + IdArticulo
                           + " ORDER BY dArticulos.NumFoto ASC";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var jsonData = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           iddarticulo = col[0],
                           foto = GetFotosArticulos("item", col[0] == DBNull.Value ? 0 : Convert.ToInt32(col[0])),
                           numfoto = col[1],
                       }).ToArray()
            };
            return Json(jsonData.rows);
        }

        [HttpPost]
        public JsonResult articuloEditar(int IdArticulo, string Nombre, string Descripcion, int Cantidad)
        {

            ResponseModel rm = new ResponseModel();
            try
            {
                string query = "UPDATE mArticulos" 
                               + " SET Nombre = CASE WHEN '" + Nombre + "'" + " = '' THEN mArticulos.Nombre ELSE '" + Nombre + "' END,"
                               + " Descripcion = '" + Descripcion + "',"
                               + " Cantidad = CASE WHEN dbo.GetArticulosRegalados(" + IdArticulo + ") > " + Cantidad + " THEN mArticulos.Cantidad ELSE " + Cantidad + " END" 
                               + " WHERE mArticulos.IdArticulo = " + IdArticulo;

                DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

                rm.response = true;
                rm.message = "El articulo se modifico con exito";


            }
            catch (Exception e)
            {
                rm.message = e.Message;
            }
            return Json(rm);


        }

        [HttpPost]
        public JsonResult articuloEliminar(int IdArticulo)
        {
            ResponseModel rm = new ResponseModel();
            int valorRetorno = 0;
            try
            {
                string cadena = _configuration["ConnectionStrings:ExpoMRCS"];

                SqlConnection cnn = new SqlConnection(cadena);
                SqlCommand cmd = new SqlCommand("ArticulosEliminar", cnn);
                cnn.Open();
                // 2. set the command object so it knows to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;
                //// 3. add parameter to command, which will be passed to the stored procedure
                cmd.Parameters.Add(new SqlParameter("@idarticulo", IdArticulo));
                var returnParameter = cmd.Parameters.Add("@ValorRetorno", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
                valorRetorno = (int)returnParameter.Value;
                cnn.Close();



            }
            catch (Exception e)
            {
                rm.message = e.Message;
            }
            var resultado = new { ValorRetorno = valorRetorno };
            return Json(resultado);
        }

        [HttpPost]
        public JsonResult OrdenFoto(int IdArticulo)
        {
            string query = "SELECT Ordenes.OrdenFoto"
                           + " FROM (SELECT 1 OrdenFoto"
                           + " UNION"
                           + " SELECT 2 OrdenFoto"
                           + " UNION"
                           + " SELECT 3 OrdenFoto) AS Ordenes"
                           + " WHERE Ordenes.OrdenFoto NOT IN (SELECT dArticulos.NumFoto FROM dArticulos WHERE dArticulos.IdArticulo = " + IdArticulo + ")";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var datosSelect = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           ordenfoto = col[0]
                       }).ToArray()
            };

            return Json(datosSelect.rows);
        }

        [HttpPost]
        public JsonResult ArticuloEliminaFoto(int IDdArticulos)
        {
            try
            {
                string query = "DELETE dArticulos"
                               + " WHERE dArticulos.IDdArticulos = " + IDdArticulos;

                DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);


            }
            catch (Exception)
            {

                throw;
            }

            return Json(new { success = true });

        }

        [HttpPost]
        public void ArticuloGuardarOrdenFoto(int NumFoto, int IDdArticulos)
        {

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand();
            cnn.Open();
            cmd.CommandText = "DECLARE @IdArticulo INT = (SELECT dArticulos.IdArticulo FROM dArticulos WHERE dArticulos.IDdArticulos = " + IDdArticulos + ")" +
                              "DECLARE @NumFotoOld TINYINT = (SELECT dArticulos.NumFoto FROM dArticulos WHERE dArticulos.IDdArticulos =" + IDdArticulos + ")" +
                              "UPDATE dArticulos " +
                              "SET NumFoto = @NumFotoOld " +
                              "WHERE dArticulos.IdArticulo = @IdArticulo " +
                              "AND dArticulos.IDdArticulos <> " + IDdArticulos +
                              "AND dArticulos.NumFoto = " + NumFoto +
                              "UPDATE dArticulos " +
                              "SET NumFoto = " + NumFoto +
                              "WHERE dArticulos.IDdArticulos = " + IDdArticulos;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            cmd.ExecuteNonQuery();
            cnn.Close();

        }


        [HttpPost]
        public JsonResult getPreciosCliente(int cliente, int itemscount = 0)
        {
            int IdProveedor = getProveedor();

            SqlConnection conexion = new SqlConnection(_configuration["ConnectionStrings:IndigoCS"]);
            SqlCommand command = new SqlCommand("ExpoProveedorPrecios", conexion);
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA;
            conexion.Open();
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@cliente", cliente));
            command.Parameters.Add(new SqlParameter("@proveedor", IdProveedor));
            command.Parameters.Add(new SqlParameter("@itemscount", itemscount));
            command.Connection = conexion;
            sqlDA = new SqlDataAdapter(command);
            sqlDA.Fill(dataTable);
            conexion.Close();

            var jsonData = new
            {
                rows = (
                      from col in dataTable.AsEnumerable()
                      select new
                      {
                          idproducto = col[0],
                          codigo = col[1],
                          producto = col[2],
                          costo = col[3],
                          comprado = col[4],
                          descuentoexpo = col[5],
                          preciofinal = col[6],
                          descuentocliente = col[7] == DBNull.Value ? 0 : col[7],
                          preciocliente = col[8],
                          pedido = col[9],
                          carrito = col[10]

                      }).ToArray()
            };

            return Json(jsonData.rows);

        }

        [HttpPost]

        public JsonResult ddlLisCliente()
        {
            int idProveedor = getProveedor();

            string query = "Select dProductosDescuento.Cliente, mClientes.Nombre"
                           + " From dProductosDescuento inner join"
                           + " mProductos on dProductosDescuento.IdProducto = mProductos.IdProducto inner join"
                           + " mLaboratorios on mProductos.IdLaboratorio = mLaboratorios.IdLaboratorio inner join"
                           + " mClientes on dProductosDescuento.Cliente = mClientes.IdPersona"
                           + " Where mLaboratorios.Proveedor = " + idProveedor
                           + " Union"
                           + " Select lVisita.Cliente, mClientes.Nombre"
                           + " From lVisita inner join"
                           + " mClientes on lVisita.Cliente = mClientes.IdPersona"
                           + " Where lVisita.IdProducto = 0"
                           + " And lVisita.Proveedor = " + idProveedor
                           + " And lVisita.Fecha > DATEADD(MINUTE, - CAST(dbo.GetSysVar('MinUltimoCliente') as int), GETDATE())"
                           + " Group by Cliente, Nombre"
                           + " Order by Nombre ";

            DataTable dataTable = GeneralesController.sqlQueryDT("ExpoMRCS", query);

            var datosSelect = new
            {
                rows = (
                       from col in dataTable.AsEnumerable()
                       select new
                       {
                           cliente = col[0],
                           nombre = col[1]
                       }).ToArray()
            };

            return Json(datosSelect.rows);


        }

        [HttpPost]
        public IActionResult ArticuloGuardarImagen(IFormFile file, int IdArticulo, int orden)
        {
            try
            {
                // Verifica si se recibió un archivo
                if (file != null && file.Length > 0)
                {
                    byte[] imagenBytes;
                    using (var ms = new MemoryStream())
                    {
                        // Guardar la imagen en formato JPEG en el MemoryStream
                        Image imagen = Image.FromStream(file.OpenReadStream());
                        imagen.Save(ms, ImageFormat.Jpeg);

                        // Obtener el arreglo de bytes de la imagen
                        imagenBytes = ms.ToArray();
                    }
                    string connectionString = _configuration["ConnectionStrings:ExpoMRCS"]; // Reemplaza con tu cadena de conexión
                    string insertQuery = "IF(SELECT COUNT(dArticulos.IDdArticulos) " +
                                        "FROM dArticulos WHERE dArticulos.IdArticulo = " + IdArticulo + ") < 3 " +
                                        "BEGIN " +
                                        "INSERT dArticulos (IdArticulo, NumFoto, Foto) " +
                                        "VALUES (@IdArticulo, @NumFoto, @Foto) " +
                                        "END";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(insertQuery, connection);
                        command.Parameters.AddWithValue("@IdArticulo", IdArticulo);
                        command.Parameters.AddWithValue("@NumFoto", orden);
                        command.Parameters.AddWithValue("@Foto", imagenBytes);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }

            return Json(new { success = true });
        }

        [HttpPost]

        public void productoAgregarDescuento(int IdCliente, int IdProducto, decimal DescClienteNew)
        {
            int IdProveedor = getProveedor();

            SqlConnection cnn = new SqlConnection(_configuration["ConnectionStrings:ExpoMRCS"]);
            SqlCommand cmd = new SqlCommand();
            cnn.Open();
            cmd.CommandText = "Update dProductosDescuento"
                              + " Set Descuento =" + DescClienteNew + ","
                              + " Usuario =" + IdProveedor + ","
                              + " Fecha = GETDATE()"
                              + " Where dProductosDescuento.Cliente =" + IdCliente 
                              + " And dProductosDescuento.IdProducto =" + IdProducto
                              + " If Not Exists (Select dProductosDescuento.IdProducto"
                              + " From dProductosDescuento" 
                              + " Where dProductosDescuento.Cliente = " + IdCliente
                              + " And dProductosDescuento.IdProducto = "+ IdProducto + ")" 
                              + " Begin" 
                              + " Insert dProductosDescuento (IdProducto, Cliente, Descuento, Usuario, Fecha)" 
                              + " Values (" + IdProducto + "," + IdCliente + "," + DescClienteNew + ","  + IdProveedor + ", GETDATE())"
                              + " End";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            cmd.ExecuteNonQuery();
            cnn.Close();


        }

        [HttpPost]

        public bool ValidaDescuento(decimal DescExpo, decimal DesCliente, decimal DescClienteNew)
        {
            bool res = false;

            if (DesCliente > 0 && DescClienteNew > DesCliente)
                res = true;
            else if (DesCliente == 0 && DescClienteNew > DescExpo)
                res = true;

            return res;
        }

        public bool ValidaDescuentovsCosto(decimal DescClienteNew, decimal Costo)
        {
            bool res = false;

            if (DescClienteNew > Costo)
                res = true;
            return res;
        }
    }

}
