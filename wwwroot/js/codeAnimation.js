/*-------------------------------------COINS ANIMATION CODE---------------------------------------------------
---------------------------------NEED CREATEJS AND PRELOAD LIBRARIES------------------------------------------*/
//Variables of Animation
//Easel variables
var assets = [];//to save the images of preload js
var stage;
var spriteSheet;
var currentAnim;

//Global Variables
var eventNodeList = [];//Animations Queue

var regXY = 30; //canvas middle pint
var isDisplayed = false;
var curvePercent = 0;

//var positions
var originPoint = { x: 0, y: 0 };// x origin , y origin
var startPoint = { x: 0, y: 0 };
var controlPoint = { x: 0, y: 0 };
var movePoint = { x: 0, y: 0 };
//----------------------------------

//----------Functions---------------------------
//Show the animation (Any element to set start position)
function displayAnimation(element) {
    if (!isDisplayed) {

        checkElement(element);

        setCoordinates(element);

         loadSprite("FlipLeft");

        displayCanvas("block");

        isDisplayed = true;

        curvePercent = 0;//set start of the curve route

        playAudio("../Media/coin.wav");
    }
    else {
        addElement(element);
    }
}

function displayAnimationxy(x, y) {
    if (!isDisplayed) {
        let element = document.getElementById("pagina");
        checkElement(element);

        setCoordinatesxy(x, y);

        loadSprite("FlipLeft");

        displayCanvas("block");

        isDisplayed = true;

        curvePercent = 0;//set start of the curve route

        playAudio("../Media/coin.wav");
    }
    else {
        addElement(element);
    }
}

//Check if the element has an animation to display.
function checkElement(element) {
    if (eventNodeList.includes(element)) {
        eventNodeList.shift();
    }
}

//Add a element to display an animation
function addElement(element) {
    if (!eventNodeList.includes(element)) {
        eventNodeList.push(element);
    }
}

//set the start and end points to move canvas animation
function setCoordinates(el) {
    let endrect = document.getElementById("moneda").firstElementChild.getBoundingClientRect(); //Coins image container
    let rect = el.getBoundingClientRect();

    //set the endpoint of the coin animation
    originPoint.x = endrect.left - Math.floor((endrect.width / 5)) + window.scrollX;
    originPoint.y = endrect.top - Math.floor((endrect.height / 2)) + window.scrollY;

    //set the initial point of the animation
    startPoint.x = rect.left + window.scrollX + (rect.width / 2) - regXY;
    startPoint.y = rect.top + window.scrollY + (rect.height / 2) - regXY;

    //set the control point and initialize the movepont in the begining
    movePoint = startPoint;
    controlPoint.x = startPoint.x;
    controlPoint.y = originPoint.y;
}

function setCoordinatesxy(x, y) {
    let endrect = document.getElementById("moneda").firstElementChild.getBoundingClientRect(); //Coins image container

    //set the endpoint of the coin animation
    originPoint.x = endrect.left - Math.floor((endrect.width / 5)) + window.scrollX;
    originPoint.y = endrect.top - Math.floor((endrect.height / 2)) + window.scrollY;

    //set the initial point of the animation
    startPoint.x = x + window.scrollX - regXY;
    startPoint.y = y + window.scrollY - regXY;

    //set the control point and initialize the movepont in the begining
    movePoint = startPoint;
    controlPoint.x = startPoint.x;
    controlPoint.y = originPoint.y;
}

//canvas position and animation functions
function moveCanvas() {
    let canvas = document.getElementById("animaCoin");
    if (canvas && !isOrigin()) {

        updateMovePoint();

        canvas.style.top = movePoint.y + "px";//to change
        canvas.style.left = movePoint.x + "px";

        blinkAnimation();
    }
}

//Check the current position of Canvas vs Origin Point
function isOrigin() {
    if (movePoint.x <= originPoint.x && movePoint.y <= originPoint.y) {
        movePoint.x = originPoint.x;
        movePoint.y = originPoint.y;
        return true;
    }
    return false;
}

//Set the start positions xy to canvas and show or hide
function displayCanvas(display) {
    let canvas = document.getElementById("animaCoin");
    if (canvas) {
        if (display == "none") {
            canvas.style.display = display;
            isDisplayed = false;
            return;
        }
        canvas.style.top = startPoint.y + "px";
        canvas.style.left = startPoint.x + "px";
        canvas.style.display = display;
    }
}

//Change the position of canvas in x y 
function updateMovePoint() {
    if (curvePercent < 1) {
        curvePercent += 0.01;
        movePoint = setMoveBezier(curvePercent);
    }
    else {
        movePoint = originPoint;
    }
}

//Calc the next move in a curve (percent 0 - 1)
function setMoveBezier(percent) {
    var x = (1 - percent) * (1 - percent) * startPoint.x + 2 * (1 - percent) * percent * controlPoint.x + percent * percent * originPoint.x;
    var y = (1 - percent) * (1 - percent) * startPoint.y + 2 * (1 - percent) * percent * controlPoint.y + percent * percent * originPoint.y;
    return { x: x, y: y };
}

//change animation to end of moving canvas
function blinkAnimation() {
    if (isDisplayed && isOrigin()) {
        loadSprite("CoinShine");
        //-----addd an audio-------
        setTimeout(function () { displayCanvas("none"); }, 3000);
    }
}

//Audio functions 
function playAudio(url) {
    var audio = new Audio(url);
    audio.currentTime = 0;
    audio.play();
}

//Easel functions
function initCanvas() {
    //preload js lib
    var manifest = [
        { src: "../json/sheetCoin.json", id: "sheet1", type: "spritesheet" }
    ];

    var loader = new createjs.LoadQueue(true, "./");
    loader.on("fileload", handleFileLoad);
    loader.on("complete", handleComplete);
    loader.loadManifest(manifest);
}

//Preload Js
function handleFileLoad(event) {
    assets.push(event);
}

function handleComplete() {
    for (let i = 0; i < assets.length; i++) {
        let event = assets[i];
        let result = event.result;

        switch (event.item.id) {
            case 'sheet1':
                spriteSheet = result;
                break;
        }
    }
    initScene();
}

//change animation sprite
function loadSprite(animation) {
    if (animation != currentAnim) {
        currentAnim = animation;
        stage.removeChildAt(0);
        coin = new createjs.Sprite(spriteSheet, animation);
        coin.x = 0;
        coin.y = 0;

        stage.addChild(coin);
    }
}

function initScene() {
    stage = new createjs.Stage("animaCoin");

    loadSprite("Coin");

    // Add Coin to the stage, and add it as a listener to Ticker to get updates each frame.
    createjs.Ticker.on("tick", tick);

    createjs.Ticker.timingMode = createjs.Ticker.RAF;
    createjs.Ticker.addEventListener("tick", stage);
}

function tick(e) {
    moveCanvas();
    stage.update();

    //add functions per tick
    //Animate all the events one per time
    if (eventNodeList.length > 0) {
        displayAnimation(eventNodeList[0]);
    }
}