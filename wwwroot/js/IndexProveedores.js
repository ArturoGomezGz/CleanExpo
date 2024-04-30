let path = Getpath();
var inactivityTimeout = 500000; // Tiempo de inactividad en milisegundos (1 minutos)
var timer;

function GetMonedas()
{
    $.post(path + "Generales/GetMonedas",
        function (data)
        {
            $("#cantidad_monedas").html(data)
        }).error(function ()
        {
            //alerta(false, "No fue posible cargar sus datos")
        }
    );
}

function GetCarrito()
{
    $.post(path + "Generales/GetCarrito",
        function (data)
        {
            $("#cantidad_carrito").html(data)
        }).error(function ()
        {
            //alerta(false, "No fue posible cargar sus datos")
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

function Getmonth(mes)
{
    var cadenaMesActual = "";

    switch (mes)
    {
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
    const currencyFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' });
    const formattedNumber = currencyFormatter.format(number);

    return formattedNumber;
}

function ShowAlert(message, type = false, IdElement = "liveAlertPlaceholder")  //false = danger, true = success
{
    let tipo = type ? "success" : "danger";
    const alertPlaceholder = document.getElementById(IdElement)
    alertPlaceholder.innerHTML = "";

    const appendAlert = (message, type) =>
    {
        const wrapper = document.createElement('div')
        wrapper.innerHTML =
        [
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

    if (rutaRelativaCompleta.includes("expomr"))
    {
        return "../../expomr/";
    }
    else
    {
        return "../";
    }
}

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


// Reiniciar el temporizador cuando haya actividad del usuario
window.addEventListener("mousemove", resetTimer);
window.addEventListener("mousedown", resetTimer);
window.addEventListener("keypress", resetTimer);
window.addEventListener("DOMMouseScroll", resetTimer);
window.addEventListener("mousewheel", resetTimer);
window.addEventListener("touchmove", resetTimer);
window.addEventListener("MSPointerMove", resetTimer);

// Comenzar el temporizador inicial
resetTimer();