using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExpoMR.Controllers
{
    public class StandController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Editar(int idProveedor) {

            SqlConnection cnn = new SqlConnection("Server=localhost\\MSSQLSERVER01;Database=ExpoMR;Integrated Security=true; MultipleActiveResultSets=true");
            SqlCommand cmd = new SqlCommand();
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlDA; cnn.Open();
            cmd.CommandText = "Select mProveedores.IdPersona as Proveedor, mProveedores.Nombre, mStands.Logo  From mStands inner join mProveedores on mStands.Proveedor = mProveedores.IdPersona where mProveedores.IdPersona =" + idProveedor + " Order by mStands.Rank asc ";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            sqlDA = new SqlDataAdapter(cmd);
            sqlDA.Fill(dataTable);
            cnn.Close();

            List<Img> ModelStands = new List<Img>();


            foreach (DataRow row in dataTable.Rows)
            {
                Img stand = new Img();
                stand.Proveedor = row.Field<int>(0);
                stand.Nombre = row.Field<string>(1);
                stand.img = row.Field<byte[]>(2);
                ModelStands.Add(stand);
            }
            return View(ModelStands[0]);
        }
    }
}
