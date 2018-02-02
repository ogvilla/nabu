///////////////////////////////////////////////////////////////////////////
//  Copyright 2015 - 2020 Sabrina Prestigiacomo sabtvg@gmail.com
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  
///////////////////////////////////////////////////////////////////////////

var frasesDelDia = 16;
var frasesVistas = [];

var selectedNode;
var scale = window.innerWidth / 1920;
var estado = '';
var rotacionCiclo = 0;
var timerCiclo, timerFlores, timerArbol;
var rotFlores = 0;
var refreshInterval = 10000; //10seg
var lastArbolRecibidoTs = (new Date()).getTime();
var joyInterval;
var textAreas;
var preguntarAlSalir = false;
var propuestaTemp;
var scalex = window.innerWidth / 1920;
var scaley = window.innerHeight / 955; //1080-bordes de pantalla
var docsTimeScale = 15;
var usuario;  ////creo que no se usa
var grupoParam;
var emailParam;
var historico = false;
var historicoFecha;

//parametros para consenso
var vUsuarios, vActivos, vminSi, vmaxNo;

//tipo de visualizacion
var visual;  //Chrome, Zafari, InternetExplorer

//config general del sistema
var config;

//grupo
var grupo;

//arbol y usuario con sus flores
var arbolPersonal;

//modelos de documentos
var modelos;

//modelos de documentos de evaluacion
var modelosEvaluacion;

$(document).mousemove(function (event) {
    if (!preguntarAlSalir) {
        if (event.which == 1 && !event.ctrlKey) {
            //scroll
            if (lastMouse) {
                if (estado == "aprendemos") {
                    quesox -= lastMouse.clientX - event.clientX;
                    quesoy -= lastMouse.clientY - event.clientY;
                    translateQueso(quesox, quesoy);
                }
                else if (estado == "decidimos") {
                    translatex -= lastMouse.clientX - event.clientX;
                    translatey -= lastMouse.clientY - event.clientY;
                    //document.body.style.cursor = "all-scroll";
                    event.preventDefault();
                    translateArbol(translatex, translatey);
                }
            }
            lastMouse = { clientX: event.clientX, clientY: event.clientY };
        }
        else if (event.which == 1 && event.ctrlKey) {
            //zoom
            if (lastMouse) {
                if (estado == "aprendemos") {
                    quesoScale += (lastMouse.clientX - event.clientX) * 2 / window.innerWidth;
                    dibujarQueso();
                }
                else if (estado == "decidimos") {
                    treeScale += (lastMouse.clientX - event.clientX) * 2 / window.innerWidth;
                    //document.body.style.cursor = "w-resize";
                    event.preventDefault();
                    dibujarArbol(arbolPersonal.raiz);
                }
            }
            lastMouse = { clientX: event.clientX, clientY: event.clientY };
        }
        else {
            lastMouse = null;
            //document.getElementById("todo").style.cursor = "default";
        }
        //$("div").text(event.which + ", " + event.ctrlKey);
    }
});

function doLoad() {
    //settings
    window.onbeforeunload = preguntar;
    //background
    document.body.style.backgroundSize = window.innerWidth + 'px ' + window.innerHeight + 'px';
    //wait
    document.getElementById("florWait").style.top = (window.innerHeight / 2 - 100).toFixed(0) + 'px';
    document.getElementById("florWait").style.left = (window.innerWidth / 2 - 98).toFixed(0) + 'px';
    document.getElementById("florWait").style.visibility = "visible";

    //busco arboles
    getHttp("doMain.aspx?actn=getConfig&width=" + window.innerWidth + "&height=" + window.innerHeight,
        function (data) {
            getConfig(data);

            if (visual.level == 0) {
                //navegador no soporta nabu
                document.getElementById("florWait").style.visibility = "hidden";
                document.getElementById("noSoportado").style.visibility = "visible";
                document.getElementById("noSoportadoMsg").innerHTML = "Nab&uacute; no puede mostrarse<br /> en esta versi&oacute;n de navegador";
            }
            else if (!visual.screen){
                document.getElementById("florWait").style.visibility = "hidden";
                document.getElementById("noSoportado").style.visibility = "visible";
                document.getElementById("noSoportadoMsg").innerHTML = "Nab&uacute; no puede mostrarse<br /> en esta resoluci&oacute;n de pantalla.<br> M&iacute;nimo 800x600.";
            }
            else
                //continuo con la carga
                doLoad2();
        });
}

function doLoad2() {
    try {
        //segunda parte del load
        //sinconizo animaciones con el navegador
        try {
            var requestAnimationFrame = window.requestAnimationFrame || window.mozRequestAnimationFrame || window.webkitRequestAnimationFrame || window.msRequestAnimationFrame;
            window.requestAnimationFrame = requestAnimationFrame;
            window.requestAnimationFrame(animate);
        }
        catch (ex) {
            //el navegador no soporta requestAnimationFrame
            //no puedo usar tweens
        }

        treeScale = scale; //valor default

        //idioma
        idioma = getParameterByName('idioma');
        if (idioma == null) idioma = 'es';

        //obtengo datos de los parametros
        grupoParam = getParameterByName('grupo');
        emailParam = getParameterByName('email');

        //si hay cookie, login automatico si no login normal
        var cookie = "";
        if (grupoParam && grupoParam != "" && grupoParam != "null") {
            cookie = getCookie("nabu-" + grupoParam.replace(' ', ''));
            if (cookie == "") {
                //login normal con el grupo parametro
                loginEffectIn();
                calcularResize();
            }
            else {
                //login automatico to server
                //obtengo datos de cookie
                var vals = cookie.split("|");
                var usuario = { nombre: vals[0], email: vals[1], clave: vals[2], grupo: vals[3], isAdmin: vals[4], idioma: vals[5] };

                idioma = vals[5]; //para el tradcutor
                traducir();

                if (emailParam && emailParam == usuario.email) {
                    //la cookie es de este usuario
                    //intento login Automatico
                    loginAutomatico(emailParam, usuario.clave, grupoParam);
                }
                else if (emailParam && emailParam != usuario.email) {
                    //no tengo la clave de este usuario
                    //login normal con el grupo parametro
                    loginEffectIn();
                    calcularResize();
                }
                else {
                    //login con los datos de la cookie
                    loginAutomatico(usuario.email, usuario.clave, usuario.grupo);
                }
            }
        }
        else {
            //no hay parametro de grupo, ense�o lista de grupos
            gruposEffectIn();
            calcularResize();
        }
    }
    catch (ex) {
        //envio al server
        sendException(ex, "doLoad2");
    }
}

function traducir() {
    document.getElementById("loginAltaUsuario").innerHTML = tr("Alta de usuario");
    document.getElementById("btnEntrar").value = tr("Entrar");
    document.getElementById("email").placeholder = tr("email");
    document.getElementById("clave").placeholder = tr("clave");
    document.getElementById("tit1").innerHTML = tr("Democracia interactiva");
    document.getElementById("btnCancelar").value = tr("Cancelar");
    document.getElementById("lnkSim").innerHTML = tr("simulacion");
    document.getElementById("mejorVisto").innerHTML = tr("Mejor visto en");
    document.getElementById("version").innerHTML = tr("version beta");
    document.getElementById("lnkAyuda").innerHTML = tr("Ayuda");
    document.getElementById("titulo1").innerHTML = tr("Sociocracia");
    document.getElementById("titulo2").innerHTML = tr("Democracia interactiva");
    document.getElementById("oldPass").placeholder = tr("Clave actual");
    document.getElementById("newPass").placeholder = tr("Nueva clave");
    document.getElementById("repeat").placeholder = tr("Repitela");
    document.getElementById("btnCCCambiar").value = tr("Cambiar");
    document.getElementById("btnCCCancelar").value = tr("Cancelar");
    document.getElementById("auTit1").innerHTML = tr("Alta de usuario");
    document.getElementById("auTit2").innerHTML = tr("auTit2");
    document.getElementById("altaUsuarioNombre").placeholder = tr("Nombre completo");
    document.getElementById("altaUsuarioEmail").placeholder = tr("Email");
    document.getElementById("auEnviar").value = tr("Enviar solicitud");
    document.getElementById("auCerrar").innerHTML = tr("Cerrar");
    document.getElementById("ufTit1").innerHTML = tr("ufTit1");
    document.getElementById("ufAceptar").value = tr("Aceptar");
    document.getElementById("ufCancelar").value = tr("Cancelar");
}

function gruposEffectIn() {
    var s
    s = "<table style='margin:0px auto'>";
    s += "<tr><td><img src='res/logo2.png'></td>";
    s += "<td class='titulo0' style='font-size:38px'>Nab&uacute;</td></tr>"
    s += "</table><br><br>";
    s += "<div class='titulo1'><b>" + tr("Grupos") + "</b></div>";
    for (i in config.grupos) {
        var grupo = config.grupos[i];
        if (i > 0)
            s += "  -  ";
        s += "<a href='default.html?grupo=" + grupo;
        if (idioma)
            s += "&idioma=" + idioma;
        s += "' style='font-size:22px'>" + grupo + "</a>"
    }
    s += "<br><br><input type='button' class='btn' value='" + tr("Crear nuevo grupo") + "' onclick=\"document.location='creararbol.html?idioma=" + idioma + "'\" style='font-size:18px'>";

    document.getElementById("grupos").style.visibility = "visible";
    document.getElementById("grupos").style.top = window.innerHeight / 2 - 250 + 'px';
    document.getElementById("grupos").style.left = (window.innerWidth / 2 - 400) + 'px';
    document.getElementById("grupos").innerHTML = s;
    //wait
    document.getElementById("florWait").style.visibility = "hidden";

    estado = 'grupos';
}

function loginAutomatico(email, clave, grupo) {
    getHttp("doMain.aspx?actn=login&email=" + email
        + "&clave=" + clave
        + "&grupo=" + grupo,
        function (data) {
            try {
                //atrapo el error si es que hay
                if (data.substring(0, 6) == "Error=") {
                    //ha habido un error
                    //login normal
                    calcularResize();
                    loginEffectIn();
                }
                else {
                    //login ok, he recibido el arbol
                    var loginData = JSON.parse(data);

                    //guardo el grupo
                    grupo = loginData.grupo;

                    //guardo el arbol
                    arbolPersonal = loginData.arbolPersonal;

                    //para el traductor
                    idioma = arbolPersonal.idioma;

                    //guardo los modelos
                    modelos = loginData.modelos;
                    modelosEvaluacion = loginData.modelosEvaluacion;

                    //guardo cookie
                    setCookie("nabu-" + arbolPersonal.nombre,
                        arbolPersonal.usuario.nombre + "|" + arbolPersonal.usuario.email + "|" + arbolPersonal.usuario.clave + "|" + arbolPersonal.nombre + "|" + arbolPersonal.usuario.isAdmin + "|" + grupo.idioma,
                        7);

                    //activo menuppal
                    doMenuppal();

                    calcularResize();
                }
            }
            catch (ex) {
                //envio al server
                sendException(ex, "doLoad2.2");
            }
        });
}

function preguntar() {
    if (preguntarAlSalir)
        return tr("Se perderan los datos no guardados");
}

function doTimeBack() {
    historico = true;
    historicoFecha = new Date();
    historicoFecha.setDate(historicoFecha.getDate() - 1);
    showTimePanel();
    document.body.style.backgroundImage = "url('res/night.jpg')";
    document.body.style.backgroundSize = window.innerWidth + 'px ' + window.innerHeight + 'px';
}

function doTimePresent() {
    historico = false;
    showTimePanel();
    document.body.style.backgroundImage = "url('res/background.jpg')";
    document.body.style.backgroundSize = window.innerWidth + 'px ' + window.innerHeight + 'px';
}

function showTimePanel() {
    //timePanel
    if (historico) {
        var now = new Date();
        now.setDate(now.getDate() - 1);
        if (historicoFecha > now) 
            historicoFecha = now;
        document.getElementById("timePanel").style.visibility = "visible";
        document.getElementById("timePanel").style.top = (window.innerHeight - 120) + 'px';
        document.getElementById("timePanel").style.left = (window.innerWidth - 195) + 'px';
        document.getElementById("timeDia").innerHTML = historicoFecha.getDate() < 10 ? '0' + historicoFecha.getDate() : historicoFecha.getDate();
        document.getElementById("timeMes").innerHTML = historicoFecha.getMonth() + 1 < 10 ? '0' + (historicoFecha.getMonth() + 1) : historicoFecha.getMonth() + 1;
        document.getElementById("timeYear").innerHTML = historicoFecha.getFullYear();
        document.getElementById("timeBack").style.visibility = "hidden";
    }
    else {
        document.getElementById("timeBack").style.visibility = "visible";
        document.getElementById("timeBack").style.top = (window.innerHeight - 90) + 'px';
        document.getElementById("timeBack").style.left = (window.innerWidth - 60) + 'px';
        document.getElementById("timePanel").style.visibility = "hidden";
    }
}

function calcularResize() {
    scalex = window.innerWidth / 1920;
    scaley = window.innerHeight / 955; //1080-bordes de pantalla

    //login
    document.getElementById("tip").style.top = (window.innerHeight / 2 + 130) + 'px';
    document.getElementById("tip").style.left = (window.innerWidth / 2 - 200) + 'px';
    document.getElementById("loginIn").style.top = window.innerHeight / 6 + 'px';
    document.getElementById("loginFlor").style.top = (window.innerHeight / 6 + 50 * scaley) + 'px';
    document.getElementById("loginIn").style.left = (window.innerWidth / 2 - 50) + 'px';
    document.getElementById("loginFlor").style.left = (window.innerWidth / 2 - 280) + 'px';

    //titulo
    document.getElementById("titulo").style.left = (window.innerWidth - 280) + 'px';
    document.getElementById("titulo0").style.fontSize = (50 * scaley).toFixed(0) + 'px';
    document.getElementById("titulo1").style.fontSize = (30 * scaley).toFixed(0) + 'px';
    document.getElementById("titulo2").style.fontSize = (25 * scaley).toFixed(0) + 'px';

    //atras
    document.getElementById("atras").style.width = (100 * scalex) + 'px';
    document.getElementById("atras").style.height = (100 * scaley).toFixed(0) + 'px';

    //pie
    document.getElementById("pie").style.top = (window.innerHeight - 25).toFixed(0) + 'px';
    document.getElementById("pie").style.left = (window.innerWidth / 2 - 300).toFixed(0) + 'px';
    document.getElementById("pie").style.visibility = 'visible';
    document.getElementById("lnkSim").href = 'simulacion.html?grupo=' + grupoParam;

    //frase del dia
    //busco una no vista
    var cant = 0;
    var index = Math.round(Math.random() * (frasesDelDia - 1));
    while (frasesVistas.indexOf(index) >= 0 && cant < frasesDelDia) {
        cant++;
        index = Math.round(Math.random() * (frasesDelDia - 1));
    }
    if (cant >= frasesDelDia)
        frasesVistas = [];
    document.getElementById("tip").innerHTML = tr("Frase" + index);
    frasesVistas.push(index);

    //background
    document.body.style.backgroundSize = window.innerWidth + 'px ' + window.innerHeight + 'px';

    //joystick
    document.getElementById("joystickArbol").style.top = (window.innerHeight - 190) + 'px';

    //timePanel
    showTimePanel();

    //resize del menuppal segun pantalla
    var menuscale = scaley * 1.1;
    document.getElementById("padrenombre").style.width = 800 * menuscale + 'px';
    document.getElementById("padrenombre").fontSize = (45 * scale).toFixed(0) + 'px';

    document.getElementById("hijos").style.width = 800 * menuscale + 'px';
    document.getElementById("hijos").style.top = (700 * scaley) + "px";

    document.getElementById("tituloppal").style.width = 800 * menuscale + 'px';
    document.getElementById("tituloppal").fontSize = (60 * scale).toFixed(0) + 'px';

    document.getElementById("menuppal").style.top = (window.innerHeight / 2 - 600 * menuscale / 2).toFixed(0) + 'px';
    document.getElementById("menuppal").style.left = (window.innerWidth / 2 - 800 * menuscale / 2).toFixed(0) + 'px';
    document.getElementById("menuppal").style.width = 800 * menuscale + 'px';
    document.getElementById("menuppal").style.height = 600 * menuscale + 'px';

    document.getElementById("ciclo").style.width = 508 * menuscale + 'px';
    document.getElementById("ciclo").style.height = 526 * menuscale + 'px';
    document.getElementById("ciclo").style.left = 159 * menuscale + 'px';
    document.getElementById("ciclo").style.top = 47 * menuscale + 'px';

    document.getElementById("ppal1").src = "res/" + idioma + "/debates.png";
    document.getElementById("ppal1").onmouseover = "this.src='res/" + idioma + "/debates2.png';";
    document.getElementById("ppal1").onmouseout = "this.src='res/" + idioma + "/debates.png';";
    document.getElementById("ppal1").title = tr("Arbol de decisiones");
    document.getElementById("ppal1").style.width = 269 * menuscale + 'px';
    document.getElementById("ppal1").style.height = 200 * menuscale + 'px';
    document.getElementById("ppal1").style.left = 35 * menuscale + 'px';
    document.getElementById("ppal1").style.top = 105 * menuscale + 'px';

    document.getElementById("ppal2").src = "res/" + idioma + "/noticias.png";
    document.getElementById("ppal2").onmouseover = "this.src='res/" + idioma + "/noticias2.png';";
    document.getElementById("ppal2").onmouseout = "this.src='res/" + idioma + "/noticias.png';";
    document.getElementById("ppal2").title = tr("Publicacion de argumentos");
    document.getElementById("ppal2").style.width = 269 * menuscale + 'px';
    document.getElementById("ppal2").style.height = 199 * menuscale + 'px';
    document.getElementById("ppal2").style.left = 35 * menuscale + 'px';
    document.getElementById("ppal2").style.top = 282 * menuscale + 'px';

    document.getElementById("ppal3").src = "res/" + idioma + "/estructuras.png";
    document.getElementById("ppal3").title = tr("Estructuras");
    document.getElementById("ppal3").style.width = 240 * menuscale + 'px';
    document.getElementById("ppal3").style.height = 90 * menuscale + 'px';
    document.getElementById("ppal3").style.left = 530 * menuscale + 'px';
    document.getElementById("ppal3").style.top = 200 * menuscale + 'px';

    document.getElementById("ppal4").src = "res/" + idioma + "/seguimiento.png";
    document.getElementById("ppal4").title = tr("Seguimiento");  
    document.getElementById("ppal4").style.width = 240 * menuscale + 'px';
    document.getElementById("ppal4").style.height = 84 * menuscale + 'px';
    document.getElementById("ppal4").style.left = 530 * menuscale + 'px';
    document.getElementById("ppal4").style.top = 287 * menuscale + 'px';

    document.getElementById("ppal5").src = "res/" + idioma + "/docConsensos.png";
    document.getElementById("ppal5").onmouseover = "this.src='res/" + idioma + "/docConsensos2.png';";
    document.getElementById("ppal5").onmouseout = "this.src='res/" + idioma + "/docConsensos.png';";
    document.getElementById("ppal5").title = tr("Documento de decision alcanzados");
    document.getElementById("ppal5").style.width = 124 * menuscale + 'px';
    document.getElementById("ppal5").style.height = 143 * menuscale + 'px';
    document.getElementById("ppal5").style.left = 338 * menuscale + 'px';
    document.getElementById("ppal5").style.top = 31 * menuscale + 'px';
    

    document.getElementById("ppal7").src = "res/" + idioma + "/docRealizacion.png";
    document.getElementById("ppal7").onmouseover = "this.src='res/" + idioma + "/docRealizacion2.png';";
    document.getElementById("ppal7").onmouseout = "this.src='res/" + idioma + "/docRealizacion.png';";
    document.getElementById("ppal7").title = tr("Documentos de resultado");
    document.getElementById("ppal7").style.width = 221 * menuscale + 'px';
    document.getElementById("ppal7").style.height = 145 * menuscale + 'px';
    document.getElementById("ppal7").style.left = 285 * menuscale + 'px';
    document.getElementById("ppal7").style.top = 439 * menuscale + 'px';

    document.getElementById("ppal8").src = "res/" + idioma + "/manifiesto.png";
    document.getElementById("ppal8").title = tr("Manifiesto del grupo");
    document.getElementById("ppal8").style.width = 130 * menuscale + 'px';
    document.getElementById("ppal8").style.height = 50 * menuscale + 'px';
    document.getElementById("ppal8").style.left = 210 * menuscale + 'px';
    document.getElementById("ppal8").style.top = 268 * menuscale + 'px';

    document.getElementById("ppal9").style.width = 64 * menuscale + 'px';
    document.getElementById("ppal9").style.height = 79 * menuscale + 'px';
    document.getElementById("ppal9").style.left = 140 * menuscale + 'px';
    document.getElementById("ppal9").style.top = 258 * menuscale + 'px';

    document.getElementById("ppal10").src = "res/" + idioma + "/politico.png";
    document.getElementById("ppal10").style.width = 35 * menuscale + 'px';
    document.getElementById("ppal10").style.height = 230 * menuscale + 'px';
    document.getElementById("ppal10").style.left = 0 * menuscale + 'px';
    document.getElementById("ppal10").style.top = 170 * menuscale + 'px';

    document.getElementById("ppal11").src = "res/" + idioma + "/operativo.png";
    document.getElementById("ppal11").style.width = 35 * menuscale + 'px';
    document.getElementById("ppal11").style.height = 230 * menuscale + 'px';
    document.getElementById("ppal11").style.left = 839 * menuscale + 'px';
    document.getElementById("ppal11").style.top = 150 * menuscale + 'px';

    //arbol
    translateArbol(translatex = 0, translatey = 0);
}

function sendException(ex, flag) {
    if (arbolPersonal)
        getHttp("doMain.aspx?actn=exception&flag=" + flag + "&message=" + ex.message + "&stack=" + ex.stack + "&email=" + arbolPersonal.usuario.email + "&grupo=" + arbolPersonal.nombre, null);
    else
        getHttp("doMain.aspx?actn=exception&flag=" + flag + "&message=" + ex.message + "&stack=" + ex.stack, null);
}

function doManifiesto() {
    if (arbolPersonal.URLEstatuto != "")
        window.open(arbolPersonal.URLEstatuto);
}

function loginEffectIn(){
    //login effect
    try {
        if (visual.level == 1) {
            //sin efectos
            document.getElementById("tip").style.visibility = "visible";
            document.getElementById("loginIn").style.visibility = "visible";
            document.getElementById("loginFlor").style.visibility = "visible";

            document.getElementById("tip").style.top = (window.innerHeight / 2 + 130) + 'px';
            document.getElementById("tip").style.left = (window.innerWidth / 2 - 200) + 'px';
            document.getElementById("loginIn").style.top = window.innerHeight / 6 + 'px';
            document.getElementById("loginFlor").style.top = (window.innerHeight / 6 + 50 * scale) + 'px';
            document.getElementById("loginIn").style.left = (window.innerWidth / 2 - 50) + 'px';
            document.getElementById("loginFlor").style.left = (window.innerWidth / 2 - 280) + 'px';
            document.getElementById("pie").style.left = (window.innerWidth / 2 - 300) + 'px';
        }
        else {
            document.getElementById("tip").style.visibility = "hidden";
            document.getElementById("loginIn").style.visibility = "hidden";
            document.getElementById("loginFlor").style.visibility = "hidden";

            document.getElementById("tip").style.left = (window.innerWidth / 2 - 200) + 'px';
            document.getElementById("loginIn").style.top = window.innerHeight / 4 + 'px';
            document.getElementById("loginFlor").style.top = (window.innerHeight / 4 + 50 * scale) + 'px';

            efectoLeft(document.getElementById("loginIn"), 0, window.innerWidth, window.innerWidth / 2 - 50, TWEEN.Easing.Cubic.Out);
            efectoLeft(document.getElementById("loginFlor"), 200, -200, window.innerWidth / 2 - 280, TWEEN.Easing.Exponential.Out);
            efectoTop(document.getElementById("tip"), 800, window.innerHeight, window.innerHeight / 2 + 130, TWEEN.Easing.Elastic.Out);
            timerFlores = setInterval(function () {
                rotFlores += 0.3;
                document.getElementById("loginFlor").style.transform = "rotate(" + rotFlores + "deg)";
            }, 100);
        }

        //atras
        document.getElementById("atras").style.visibility = "visible";

        //nombre de grupo
        document.getElementById("loginGrupo").style.left = (window.innerWidth / 2 - 200) + 'px';
        document.getElementById("loginGrupo").style.top = '50px';
        document.getElementById("loginGrupo").style.visibility = "visible";
        document.getElementById("loginGrupo").innerHTML = grupoParam;

        //wait
        document.getElementById("florWait").style.visibility = "hidden";

        estado = 'login';
    }
    catch (ex) {
        //envio al server
        sendException(ex, "loginEffectIn");
    }
}

function loginEffectOut() {
    //login effect
    efectoLeft(document.getElementById("loginIn"), 0, window.innerWidth / 2 - 50, window.innerWidth, TWEEN.Easing.Cubic.Out);
    efectoLeft(document.getElementById("loginFlor"), 200, window.innerWidth / 2 - 280, -250, TWEEN.Easing.Linear.None);
    efectoTop(document.getElementById("tip"), 0, window.innerHeight / 2 + 130, window.innerHeight, TWEEN.Easing.Exponential.Out);
    clearInterval(timerFlores);
    timerFlores = setInterval(function () {
        rotFlores -= 8;
        document.getElementById("loginFlor").style.transform = "rotate(" + rotFlores + "deg)";
    }, 50);
    setTimeout(function () {
        //fin efecto, limpio cosas
        clearInterval(timerFlores);
        document.getElementById("loginIn").style.visibility = "hidden";
        document.getElementById("loginFlor").style.visibility = "hidden";
        document.getElementById("tip").style.visibility = "hidden";
    }, 1000);
    document.getElementById("loginGrupo").style.visibility = "hidden";
}

function animate(time) {
    requestAnimationFrame(animate);
    TWEEN.update(time);
}

function getConfig(data) {
    config = JSON.parse(data);
    visual = getVisualizacion(config);
}

function doLogin() {
    email = document.getElementById("email").value;
    var clave = document.getElementById("clave").value;
    var loginResponse = document.getElementById("loginResponse");
    loginResponse.innerHTML = '';

    //login to server
    getHttp("doMain.aspx?actn=login&email=" + email
        + "&clave=" + clave
        + "&grupo=" + grupoParam,
        function (data) {
            if (data.substring(0, 6) == "Error=") {
                //ha habido un error
                var loginResponse = document.getElementById("loginResponse");
                loginResponse.innerHTML = '<font color=red>' + data.substring(6) + '</font>';
            }
            else {
                //login effect
                //login ok, he recibido el arbol
                var loginData = JSON.parse(data);

                //guardo el grupo
                grupo = loginData.grupo;

                //guardo el arbol
                arbolPersonal = loginData.arbolPersonal;

                //guardo los modelos
                modelos = loginData.modelos;

                //guardo cookie
                setCookie("nabu-" + grupo.nombre
                    , arbolPersonal.usuario.nombre + "|" + arbolPersonal.usuario.email + "|" + arbolPersonal.usuario.clave + "|" + grupo.nombre + "|" + arbolPersonal.usuario.isAdmin + "|" + grupo.idioma, 7);

                //efecto login out
                loginEffectOut();

                //activo menuppal
                doMenuppal(data);
            }
        });
}

function doMenuppal() {
    //paso a menuppal       

    //menu ppal
    setTimeout(function () {
        //link al padre
        var padreURL = arbolPersonal.padreURL + "/default.html?grupo=" + arbolPersonal.padreNombre + "&email=" + arbolPersonal.usuario.email;
        document.getElementById("padrenombre").innerHTML = "<a href='" + padreURL + "'>" + arbolPersonal.padreNombre + "</a>";

        //nombre del arbol
        document.getElementById("tituloppal").innerHTML = arbolPersonal.nombre;
        document.getElementById("titulo").style.visibility = "visible";

        //hijos
        var hijos = "<nobr>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
        for (q in arbolPersonal.hijos) {
            var thijo = arbolPersonal.hijos[q];
            var hijoURL = thijo.URL + "/default.html?grupo=" + thijo.nombre + "&email=" + arbolPersonal.usuario.email;
            hijos += "<td><a href='" + hijoURL + "'>" + thijo.nombre + "</a></td>";
            hijos += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
        }
        hijos += "</nobr>";
        document.getElementById("hijos").innerHTML = hijos;

        //atras
        document.getElementById("atras").style.visibility = "visible";

        //panel de usuario
        document.getElementById("usuario").innerHTML = "<div style='color:blue;cursor: pointer; font-size:14px; margin: 10px;' onclick='showCambiarClave();'>" + arbolPersonal.usuario.nombre + "</div>";
        document.getElementById("panelUsuario").style.visibility = 'hidden';
        document.getElementById("floresDisponibles").innerHTML = getFloresDisponibles().length;

        //panel consenso
        document.getElementById("panelConsenso").style.visibility = 'hidden';

        //panel grupo
        actualizarDatosGrupo();

        //panel usuario        
        document.getElementById("panelUsuario").style.visibility = 'visible';

        //opciiones de menu
        var mnu = "";
        if (arbolPersonal.usuario.isAdmin) {
            //adminOptions
            mnu += "<a class='titulo3' href='bosque.html?grupo=" + grupoParam + "'>" + tr("El bosque") + "</a><br>";
            mnu += "<a class='titulo3' href='modificararbol.html?grupo=" + grupoParam + "'>" + tr("El arbol") + "</a><br>";
            mnu += "<a class='titulo3' href='usuarios.html?grupo=" + grupoParam + "'>" + tr("Usuarios") + "</a><br>";
            mnu += "<a class='titulo3' href='mailer.html?grupo=" + grupoParam + "'>" + tr("Mailer") + "</a><br>";
        }
        else {
            //user options
            mnu += "<a class='titulo3' href='bosque.html?grupo=" + grupoParam + "'>" + tr("El bosque") + "</a><br>";
            mnu += "<a class='titulo3' href='verusuarios.html?grupo=" + grupoParam + "'>" + tr("Usuarios") + "</a><br>";
        }
        if (arbolPersonal.usuario.isSecretaria) {
            //adminOptions
            mnu += "<a class='titulo3' href='actas.html?grupo=" + grupoParam + "'>" + tr("Actas") + "</a><br>";
        }
        document.getElementById("mnuOptions").innerHTML = mnu;

        //pongo icono ce manifiesto
        if (arbolPersonal && arbolPersonal.URLEstatuto != "")
            document.getElementById("ppal9").src = "res/documentos/manifiesto.png";
        else
            document.getElementById("ppal9").src = "res/noManifiesto.png";

        //activo la rueda
        if (arbolPersonal && arbolPersonal.URLEstatuto != "" && timerCiclo == null)
            timerCiclo = setInterval(function () {
                document.getElementById("ciclo").style.transform = 'rotate(' + rotacionCiclo++ + 'deg)';
            }, 100);
        else
            timerCiclo == null;

        //menuppal
        //var menuscale = scale * 1.1;
        //document.getElementById("menuppal").style.top = (window.innerHeight / 2 - 300 * menuscale).toFixed(0) + 'px';
        //document.getElementById("menuppal").style.left = (window.innerWidth / 2 - 400 * menuscale).toFixed(0) + 'px';
        if (visual.level == 1) {
            document.getElementById("menuppal").style.visibility = 'visible';
        }
        else {
            efectoOpacity(document.getElementById("menuppal"), 0, 0, 1, TWEEN.Easing.Cubic.Out);
        }
    }, 1000); //doy tiempo a que salga el logInEffectOut()

    
    estado = 'menuppal';

    //wait
    document.getElementById("florWait").style.visibility = "hidden";
}

function doCloseHelp() {
    document.getElementById("help").style.visibility = "hidden";
}

function showCambiarClave() {
    document.getElementById("altaUsuarioNombre").value = "";
    document.getElementById("altaUsuarioEmail").value = "";
    document.getElementById("cambiarClaveMsg").innerHTML = "";
    document.getElementById("cambiarClave").style.top = '240px';
    document.getElementById("cambiarClave").style.visibility = 'visible';
    efectoLeft(document.getElementById("cambiarClave"), 0, -230, 20, TWEEN.Easing.Cubic.Out);
}

function doAltaUsuario() {
    var list = config.grupos;
    var grupo = getParameterByName('grupo') + "";
    var selected = false;
    var grupos = document.getElementById("altaUsuarioGrupos");
    for (var q in list) {
        var option = document.createElement("option");
        option.text = list[q];
        if (list[q].toLowerCase() == grupo.toLowerCase()) {
            option.selected = true;
            selected = true;
        }
        grupos.add(option);
    }
    if (selected)
        grupos.disabled = true;

    document.getElementById("altaUsuarioMsg").innerHTML = "";
    document.getElementById("altaUsuario").style.left = (window.innerWidth / 2 - 330 / 2).toFixed(0) + 'px';
    document.getElementById("altaUsuario").style.visibility = 'visible';
    efectoTop(document.getElementById("altaUsuario"), 0, -330, window.innerHeight / 2 - 330 / 2, TWEEN.Easing.Cubic.Out);
}

function doAltaUsuarioEnviar() {
    var nombre = document.getElementById("altaUsuarioNombre");
    var email = document.getElementById("altaUsuarioEmail");
    var grupos = document.getElementById("altaUsuarioGrupos");
    var msg = document.getElementById("altaUsuarioMsg");

    if (nombre == "")
        msg.innerHTML = "<font color=green>" + tr("Nombre no puede ser vacio") + "</font>";
    else if (email == "")
        msg.innerHTML = "<font color=green>" + tr("Email no puede ser vacio") + "</font>";
    else {
        getHttp("doMain.aspx?actn=sendMailAlta&grupo=" + grupos.value
            + "&nombre=" + nombre.value
            + "&usuarioemail=" + email.value,
            + "&email=" + arbolPersonal.usuario.email
            + "&clave=" + arbolPersonal.usuario.clave,
            function (data) {
                //atrapo el error si es que hay
                if (data.substring(0, 6) == "Error=") {
                    //ha habido un error
                    msg.innerHTML = "<font color=red>" + data + "</font>";
                }
                else {
                    nombre.value = "";
                    email.value = "";
                    msg.innerHTML = "<font color=green>" + tr("Mensaje enviado al coordinador") + "</font>";
                }
            });
    }
}

function doCambiarClave() {
    var oldPass = document.getElementById("oldPass").value;
    var newPass = document.getElementById("newPass").value;
    var repeat = document.getElementById("repeat").value;

    if (newPass != repeat)
        document.getElementById("cambiarClaveMsg").innerHTML = "<font color='red'>" + tr("Las nuevas claves no coinciden") + "</font>";
    else {
        getHttp("doMain.aspx?actn=cambiarClave&grupo=" + arbolPersonal.nombre
            + "&email=" + arbolPersonal.usuario.email
            + "&claveActual=" + oldPass
            + "&nuevaClave=" + newPass,
            function (data) {
                if (data != '')
                    document.getElementById("cambiarClaveMsg").innerHTML = "<font color='red'>" + data.substring(6) + "</font>";
                else 
                    efectoLeft(document.getElementById("cambiarClave"), 0, 20, -260, TWEEN.Easing.Cubic.Out, function () {
                        document.getElementById("cambiarClave").style.visibility = 'hidden';
                    });
            });
    }
}

function doHideCambiarClave() {
    efectoLeft(document.getElementById("cambiarClave"), 0, 20, -260, TWEEN.Easing.Cubic.In, function () {
            document.getElementById("cambiarClave").style.visibility = 'hidden';
        });
}

function doAtras() {
    document.getElementById("cambiarClave").style.visibility = 'hidden';

    if (estado == 'login') {
        //login voy a grupos
        loginEffectOut();
        setTimeout(gruposEffectIn, 600); //doy tiempo al login a irse
        calcularResize();
        document.getElementById("atras").style.visibility = "hidden";
    }
    else if (estado == 'aprendemos') {
        //aprendemos voy a menuppal
        document.getElementById("panelQueso").style.visibility = 'hidden';

        document.getElementById("quesoDiv").style.visibility = "hidden";
        document.getElementById("quesoDiv").style.display = "none";

        document.getElementById("panelUsuario").style.visibility = 'visible';
        document.getElementById("menuEvaluacion").style.visibility = 'hidden';
        document.getElementById("modelos").style.visibility = "hidden";
        document.getElementById("joystickQueso").style.visibility = "hidden";
       
        quesoPersonal = null;
        estado = 'menuppal';
        actualizarDatosGrupo();
        clearInterval(quesoInterval);

        //activo el menuppal
        if (visual.level == 1) {
            document.getElementById("menuppal").style.visibility = "visible";
        }
        else {
            //menu ppal
            efectoOpacity(document.getElementById("menuppal"), 0, 0, 1, TWEEN.Easing.Cubic.Out);
        }
    }
    else if (estado == 'menuppal') {
        //menu ppal voy a login
        efectoOpacity(document.getElementById("menuppal"), 0, 1, 0, TWEEN.Easing.Cubic.Out, function () { document.getElementById("menuppal").style.visibility = "hidden"; });
        document.getElementById("panelGrupo").style.visibility = 'hidden';

        arbolPersonal = null;
        propuestas = [];
        setCookie("nabu-" + grupoParam, "", 1);

        document.getElementById("panelUsuario").style.visibility = 'hidden';
        document.getElementById("titulo").style.visibility = "hidden";
        document.getElementById("email").value = '';
        document.getElementById("clave").value = '';
        setTimeout(loginEffectIn, 600); //doy tiempo al menu a irse

        document.getElementById("mnuOptions").innerHTML = "";
        document.getElementById("panelUsuario").style.visibility = 'hidden';

        document.getElementById("atras").style.visibility = "hidden";
        root = { "name": "?" };
        clearInterval(timerFlores);

        estado = 'login';
    }
    else if (estado == 'decidimos') {
        //decidimos voy a menuppal
        var objetivo = document.getElementById("objetivo");
        objetivo.innerHTML = arbolPersonal.objetivo;
        objetivo.style.visibility = 'hidden';

        document.getElementById("panelUsuario").style.visibility = 'visible';

        //efecto de salida: podo el arbol
        var children = arbolPersonal.raiz.children;
        var logDecisiones = arbolPersonal.logDecisiones;
        arbolPersonal.raiz.children = [];
        arbolPersonal.logDecisiones = [];
        dibujarArbol(arbolPersonal.raiz);
        arbolPersonal.raiz.children = children;
        arbolPersonal.logDecisiones = logDecisiones;


        document.getElementById("joystickArbol").style.visibility = 'hidden';
        document.getElementById("modelos").style.visibility = "hidden";
        document.getElementById("panelConsenso").style.visibility = 'hidden';
        document.getElementById("documento").style.visibility = 'hidden';
        if (menu) menu.style.visibility = "hidden";
        clearInterval(timerFlores);
        clearInterval(timerArbol);
        hidePanelDer();
        hidePanelIzq();

        //doy tiempo a salir al arbol
        setTimeout(function () {
            document.getElementById("arbol").style.visibility = 'hidden';

            //activo el menuppal
            if (visual.level == 1) {
                document.getElementById("menuppal").style.visibility = "visible";
            }
            else {
                //menu ppal
                efectoOpacity(document.getElementById("menuppal"), 0, 0, 1, TWEEN.Easing.Linear.None);
            }

            timerCiclo = setInterval(function () {
                document.getElementById("ciclo").style.transform = 'rotate(' + rotacionCiclo++ + 'deg)';
            }, 100);

        }, 800); //doy tiempo a salir al arbol

        estado = 'menuppal';
        actualizarDatosGrupo();
    }
}

function resize() {
    calcularResize();
}

function doDecisiones() {
    document.location = "decisiones.html?grupo=" + grupoParam;
}

function doResultados() {
    document.location = "resultados.html?grupo=" + grupoParam;
}

function doEstructuras() {
    document.location = "estructuras.html?grupo=" + grupoParam;
}

function doSeguimiento() {
    document.location = "seguimiento.html?grupo=" + grupoParam;
}
