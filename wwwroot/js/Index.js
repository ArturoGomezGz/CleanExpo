let path = Getpath();

//Configuracion para cierre automatico de session de usuario por inactividad

var inactivityTimeout = 3600000; // Tiempo de inactividad en milisegundos
var timer;

//Reiniciar el temporizador cuando haya actividad del usuario
window.addEventListener("mousemove", resetTimer);
window.addEventListener("mousedown", resetTimer);
window.addEventListener("keypress", resetTimer);
window.addEventListener("DOMMouseScroll", resetTimer);
window.addEventListener("mousewheel", resetTimer);
window.addEventListener("touchmove", resetTimer);
window.addEventListener("MSPointerMove", resetTimer);

//Comenzar el temporizador inicial
resetTimer();

function resetTimer()
{
    clearTimeout(timer);
    timer = setTimeout(logout, inactivityTimeout);
}

function logout()
{
    // Realizar acciones de cierre de sesión o redireccionamiento
    $("#btn_logout").click();
    // Ejemplo: window.location.href = "/logout";
}

//Configuracion para cierre automatico de session de usuario por inactividad

function GetMonedas()
{
    $.post(path + "Generales/GetMonedas",
        function (data)
        {
            $("#cantidad_monedas").html(data)
        }
    ).error(
        function ()
        {
            ShowAlert("No fue posible cargar sus datos", false);
        }
    );
}

function GetRuleta(IdPersona)
{
    $.post(path + "Generales/GetRuleta?IdPersona=" + IdPersona,
        function (data)
        {   
            $("#Ruleta").removeClass("btn-ruleta-inactiva btn-ruleta-clasica btn-ruleta-dorada btn-ruleta-platinum btn-ruleta-jugada");
            $("#Ruleta").addClass(data);
        }
    ).error(
        function ()
        {
            ShowAlert("No fue posible cargar sus datos", false);
        }
    );
}

function GetCarrito()
{
    $.post(path + "Generales/GetCarrito",
        function (data)
        {
            $("#cantidad_carrito").html(data)
        }
    ).error(
        function ()
        {
            ShowAlert("No fue posible cargar sus datos", false);
        }
    );
}

function FormatoFecha()
{
    var fechaActual = new Date();
    var diasSemana = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    var diaSemanaActual = diasSemana[fechaActual.getDay()];
    var dia = fechaActual.getDate();
    var mes = fechaActual.getMonth() + 1;

    return diaSemanaActual + ", " + dia + " " + Getmonth(mes) + " ";
}

function FormatoFechaDataTable(fecha)
{
    var fechaBD = new Date(fecha);  // Ejemplo de fecha, asegúrate de ajustarla según tu caso

    // Obtenemos los componentes de la fecha (día, mes, año)
    var dia = fechaBD.getDate();
    var mes = fechaBD.getMonth() + 1;  // Los meses en JavaScript van de 0 a 11, por eso sumamos 1
    var anio = fechaBD.getFullYear();

    // Formateamos la fecha en el formato dd/mm/yyyy
    var fechaFormateada = `${dia < 10 ? '0' : ''}${dia}/${mes < 10 ? '0' : ''}${mes}/${anio}`;

    // Muestra la fecha formateada
    return fechaFormateada;
}

function Getmonth(mes)
{
    var cadenaMesActual = "";
    switch (mes) {
        case 1:
            cadenaMesActual = "ene";
            break;
        case 2:
            cadenaMesActual = "feb";
            break;
        case 3:
            cadenaMesActual = "mar";
            break;
        case 4:
            cadenaMesActual = "abr";
            break;
        case 5:
            cadenaMesActual = "may";
            break;
        case 6:
            cadenaMesActual = "jun";
            break;
        case 7:
            cadenaMesActual = "jul";
            break;
        case 8:
            cadenaMesActual = "ago";
            break;
        case 9:
            cadenaMesActual = "sep";
            break;
        case 10:
            cadenaMesActual = "oct";
            break;
        case 11:
            cadenaMesActual = "nov";
            break;
        case 12:
            cadenaMesActual = "dic";
            break;
    }

    return cadenaMesActual;
}

function FormatoMoneda(number)
{
    const currencyFormatter = number.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });

    return currencyFormatter;
}

function ShowAlert(message, type = false, IdElement = "liveAlertPlaceholder") //false = danger, true = success
{ 
    let tipo = type ? "success" : "danger";
    const alertPlaceholder = document.getElementById(IdElement)
    alertPlaceholder.innerHTML = "";
    const appendAlert = (message, type) => {
        const wrapper = document.createElement('div')
        wrapper.innerHTML = [
            `<div class="alert alert-${type} alert-dismissible" role="alert">`,
            `   <div>${message}</div>`,
            '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
            '</div>'
        ].join('')

        alertPlaceholder.append(wrapper)
    }

    appendAlert(message, tipo)
}

function Getpath()
{
    var rutaRelativaCompleta = window.location.href;

     if (rutaRelativaCompleta.includes("ExpoMRdev"))
    {
        return "../../ExpoMRdev/";
    }
    else
    {
        return "../";
    }
}

function CloseModal(IdModal)
{
    var myModalEl = document.getElementById(IdModal);
    var modal = bootstrap.Modal.getInstance(myModalEl)
    modal.hide()
}

function OpenModal(IdModal)
{
    const myModal = new bootstrap.Modal(document.getElementById(IdModal));
    myModal.show();;
}

function toggleCollapse()
{
    var collapseElementList = [].slice.call(document.querySelectorAll('.collapse'))
    var collapseList = collapseElementList.map(function (collapseEl) {
        return new bootstrap.Collapse(collapseEl)
    })
}

function SendMail(IdPedido)
{
    $.post("/Notificaciones/SendOrder",
        {            
            IdPedido: IdPedido
        },
        function (data)
        {
            console.log(data)
        }).error(function ()
        {
            ShowAlert("No fue posible cargar sus datos", false);
        }
    );
}
