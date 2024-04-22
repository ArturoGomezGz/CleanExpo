var audio;
var playPromise;
var playing = false;

//function play() {
//    $.ajax({
//        url: path + 'Ruleta/GetData',
//        method: 'post',
//        contentType: "application/json; charset=utf-8",
//        dataType: 'json',
//        success: function (response) {
//            var str = response.result;
//            var data = str.split(',');
//            playRoulette(data[0], data[1], data[2], data[3]);
//        },
//        failure: function (response) {
//        }
//    });
//}

function playRoulette(deg, idPremio, premio, descripcion, foto) {
  
    var time = 5;//Math.floor((Math.random() * 7) + 5);
    rotationimg(deg, time);
    playAudio();
    time = (time * 1000) + 100;
    setTimeout(winnerAudio, time);
    time = time + 1000;
    setTimeout(showPrize, time, idPremio, premio, descripcion, foto);
}

function rotationimg(deg, time) {
    let rotacion = parseFloat(deg) + 720;
    var img = document.getElementById("imgRoulette");
    var css = '-webkit-transform: rotate(' + rotacion + 'deg); max-width: 100%; transition: ' + time + 's linear;';
    img.setAttribute
        (
            'style', css
        );
}

function getCurrentDeg() {
    var el = document.getElementById("imgRoulette");
    var st = window.getComputedStyle(el, null);
    var tm = st.getPropertyValue("-webkit-transform") ||
        st.getPropertyValue("-moz-transform") ||
        st.getPropertyValue("-ms-transform") ||
        st.getPropertyValue("-o-transform") ||
        st.getPropertyValue("transform") ||
        "none";
    if (tm != "none") {
        var values = tm.split('(')[1].split(')')[0].split(',');
        var angle = Math.round(Math.atan2(values[1], values[0]) * (180 / Math.PI));
        return (angle < 0 ? angle + 360 : angle);
    }
}

function playAudio() {
    if (!playing) {
        playing = true;
        audio = new Audio("../Media/slotmachine.mp3");
        audio.currentTime = 0;
        audio.loop = true;
        playPromise = audio.play();
    }
}

function winnerAudio() {
    if (playPromise !== undefined && playing) {
        playPromise.then(_ => {
            audio.pause();
            audio = new Audio("../Media/winner.mp3");
            audio.currentTime = 0;
            playPromise = audio.play();
        })
            .catch(error => {
            });
    }
}

function showPrize(idPremio, premio, descripcion, foto) {
    //document.getElementById('divPremio').style.display = "block";
    OpenModal('Premio_Modal')
    document.getElementById('imgPremio').src = "data:image/png;base64," + foto;
    $('p#pPremio').text(premio);
    $('p#pDescripcion').text(descripcion);
}