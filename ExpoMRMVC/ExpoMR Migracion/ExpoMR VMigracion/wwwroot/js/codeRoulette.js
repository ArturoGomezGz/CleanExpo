var audio;
var playPromise;
var playing = false;

function displayButton(){
    //let imgbtn = document.getElementById("imgButton");
    //imgbtn.src = "/Imagenes/botonSuerte.png";
    //imgbtn.style.height = "170px";
    //imgbtn.style.width = "170px";
    //imgbtn.style.top = "215px";
    //imgbtn.style.left= "215px";
}

function hideButton(){
    //let imgbtn = document.getElementById("imgButton");
    //imgbtn.src = "/Imagenes/botonGira.png";
    //imgbtn.style.height = "147px";
    //imgbtn.style.width = "147px";
    //imgbtn.style.top = "225px";
    //imgbtn.style.left= "225px";
}


function playRoulette(deg, idPremio, premio, descripcion) {
    var time = Math.floor((Math.random() * 7) + 5);
    rotationimg(deg, time);
    playAudio();
    time = (time * 1000) + 100;
    setTimeout(winnerAudio, time);
    time = time + 1000;
    setTimeout(showPrize, time, idPremio, premio, descripcion);
}

function rotationimg(deg, time){
    var img = document.getElementById("imgRoulette"); 
    var css = '-webkit-transform: rotate(' + deg + 'deg); width: -webkit-fill-available; transition: ' + time + 's linear;';
    img.setAttribute
	(
        'style', css
    );
}

function getCurrentDeg(){
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
        var angle = Math.round(Math.atan2(values[1],values[0]) * (180/Math.PI));
        return (angle < 0 ? angle + 360 : angle); 
    }
}

function playAudio() {
    if(!playing){
        playing = true;
        audio = new Audio("../Media/slotmachine.mp3");
        audio.currentTime = 0;
        audio.loop = true;
        playPromise = audio.play();
    }
}

function winnerAudio(){
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

function showPrize(idPremio, premio, descripcion) {
    document.getElementById('divPremio').style.display = "block";
    document.getElementById('imgPremio').src = "IhImages.ashx?Type=prize&Size=150&Id=" + idPremio;
    $('p#pPremio').text(premio);
    $('p#pDescripcion').text(descripcion);
}