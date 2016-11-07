﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace nabu
{
    public class Arbol
    {
        public string nombre = "";
        public int cantidadFlores = 5;
        public Grupo grupo;
        public Nodo raiz;
        public bool leftRight = false; //alterna agregado de nuevos nodos de un lado y del otro
        public List<Propuesta> propuestas = new List<Propuesta>();
        public DateTime born = DateTime.Now;
        public int lastDocID = 1;
        public int lastNodoID = 10;
        public bool simulacion = false;
        public Usuario lastSimUsuario;

        //modelos de documento para este arbol
        //POR AHORA SON TODOS
        public List<Modelo> modelos = Modelo.getModelos();

        //log de consensos alcanzados
        public List<LogDocumento> logDocumentos = new List<LogDocumento>();

        //condicion de consenso
        public float minSiPc = 60; //porcentaje minimo de usuarios implicados en el debate (en una rama) para alcanzar consenso
        public float maxNoPc = 15; //porcentaje maximo de usuarios en otras ramas del mismo debate (en una rama) para alcanzar consenso

        [NonSerialized]
        public Random rnd = new Random();

        public Nodo getMayorAgregar(int notLikeId) {
            //busco un candidato con muchas flores para agregarle otra
            var nodes = toList();
            NodoComparerMayor dc = new NodoComparerMayor();
            nodes.Sort(dc); //de mayor a menor
            List<Nodo> temp = new List<Nodo>();

            //1er filtro de candidato
            foreach (Nodo n in nodes)
                if (!n.consensoAlcanzado && n.nivel == 5 && n.id != notLikeId && n.flores < minSiPc)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //1er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (!n.consensoAlcanzado && n.id != notLikeId && n.flores < minSiPc)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //3er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (n.id != notLikeId && n.flores < minSiPc)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            return nodes[0];
        }

        public int getApoyos(Usuario u)
        {
    		return getApoyos(u, toList());
	    }

        private int getApoyos(Usuario u, List<Nodo> nodos)
        {
		int ret = 0;
		//obtengo nodos de este usuario
		List<int> mios = new List<int>();
		foreach (Nodo n in nodos)
		{
			if (n.email == u.email)
				mios.Add(n.id);
		}
		//obtengo apoyos
        foreach (Usuario u2 in grupo.usuarios)
		{
			foreach (int id in mios)
			{
				foreach (Flor f in u2.flores)
				if (f.id == id)
					ret++;
			}
		}
		return ret;
	}

        public void actualizarApoyos()
        {
		//recuento los apoyos que tiene cada usuario
		List<Nodo> nodos = toList();
        foreach (Usuario u in grupo.usuarios)
		{
			u.apoyos = getApoyos(u, nodos);		
		}
        }

        public Nodo getMayorQuitar(int notLikeId)
        {
            //busco un candidato con muchas flores para quitarle una
            var nodes = toList();
            NodoComparerMayor dc = new NodoComparerMayor();
            nodes.Sort(dc); //de mayor a menor
            List<Nodo> temp = new List<Nodo>();

            //1er filtro de candidato
            foreach (Nodo n in nodes)
                if (n.consensoAlcanzado && n.flores > 0 && n.id != notLikeId && n.nivel<5)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //1er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (n.consensoAlcanzado && n.flores > 0 && n.id != notLikeId)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //1er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (n.flores > 0 && n.id != notLikeId)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            return nodes[0];
        }

        public Nodo getMenorAgregar(int notLikeId)
        {
            //busco un candidato con pocas flores para agregarle una
            List<Nodo> nodes = toList();
            NodoComparerMenor dc = new NodoComparerMenor();
            nodes.Sort(dc); //de mayor a menor
            List<Nodo> temp = new List<Nodo>();

            //1er filtro de candidato
            foreach (Nodo n in nodes)
                if (!n.consensoAlcanzado && n.id != notLikeId && n.nivel == 5 && n.flores < minSiPc)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //1er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (!n.consensoAlcanzado && n.id != notLikeId && n.children.Count == 0 && n.flores < minSiPc)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //2er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (!n.consensoAlcanzado && n.id != notLikeId && n.flores < minSiPc)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //3er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (n.id != notLikeId)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            return nodes[0];
        }

        public Nodo getMenorQuitar(int notLikeId)
        {
            //busco un candidato con pocas flores para quitarle una
            var nodes = toList();
            NodoComparerMenor dc = new NodoComparerMenor();
            nodes.Sort(dc); //de mayor a menor
            List<Nodo> temp = new List<Nodo>();

            //1er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (n.consensoAlcanzado && n.flores > 0 && n.id != notLikeId)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //1er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (n.flores > 0 && n.id != notLikeId && n.nivel > 2)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            //2er filtro de candidato
            temp.Clear();
            foreach (Nodo n in nodes)
                if (n.flores > 0 && n.id != notLikeId)
                    temp.Add(n);
            if (temp.Count > 0) return rndElement(temp);

            return nodes[0];
        }

        public Nodo rndElement(List<Nodo> nodes)
        {
            int index = (int)Math.Ceiling(rnd.NextDouble() * nodes.Count) - 1;
            return nodes[index];
        }

        public List<Nodo> toList() {
            var nodes = toList2(raiz, new List<Nodo>());
            return nodes;
        }

        public string getEtiqueta(string prefijo, Nodo n)
        {
            //busco un id libre en este subarbol
            List<Nodo> subarbol = toList(n);
            int ret = 0;
            bool found = true;
            while (found)
            {
                found = false;
                ret++;
                foreach (Nodo hijo in subarbol)
                    if (hijo.nombre == prefijo + ret.ToString())
                        found = true;
            }
            return prefijo + ret.ToString();
        }

        public List<Nodo> toList(Nodo n)
        {
            var nodes = toList2(n, new List<Nodo>());
            return nodes;
        }

        private List<Nodo> scrumble(List<Nodo> nodes) {
            //scrumble
            for(int i = 0; i<nodes.Count / 3; i++)
            {
                int swapIndex = (int)Math.Ceiling(rnd.NextDouble() * nodes.Count) - 1;
                Nodo swap = nodes[swapIndex];
                nodes[swapIndex] = nodes[i];
                nodes[i] = swap;
            }
            return nodes;
        }

        private List<Nodo> toList2(Nodo node, List<Nodo> l) {
            List<Nodo> ret = l;
            ret.Add(node);
            if (node.children.Count > 0) {
                foreach (Nodo n in node.children) 
                {
                    ret = toList2(n, ret);
                }
            }
            return ret;
        }

        public float minSiValue
        {
            get
            {
                return (float)Math.Ceiling(grupo.usuarios.Count * minSiPc / 100);
            }
        }

        public float maxNoValue
        {
            get
            {
                return (float)Math.Ceiling(minSiValue * maxNoPc / 100);
            }
        }

        private bool comprobarConsenso()
        {
            List<Nodo> nodos = toList();
            foreach (Nodo n in nodos)
                if (n.nivel > 0)
                    if (comprobarConsenso(n))
                        return true;
            return false;
        }

        public LogDocumento getLogDocumento(int docID)
        {
            foreach (LogDocumento ld in logDocumentos)
            {
                if (ld.docID == docID)
                    return ld;
            }
            return null;
        }

        private bool comprobarConsenso(Nodo n)
        {
            bool ret = false;
            Modelo m = Modelo.getModelo(n.modeloID);
            List<Nodo> pathn = getPath(n.id);

            if (m != null && pathn.Count - 1 == m.niveles)
            {
                //es una hoja de documento completo, verifico condicion
                n.negados = getNegados(n);

                //condicion de consenso
                if (!n.consensoAlcanzado &&
                    n.flores >= minSiValue &&
                    n.negados <= maxNoValue)
                {
                    //esta rama ha alcanzado el consenso
                    //genero documento
                    DateTime fdate = DateTime.Now;
                    int docID = lastDocID++;
                    string fname = m.nombre + "_" + docID.ToString("0000");
                    
                    //guardo HTML
                    generarDocumentoHTML(n, fdate, fname);

                    //guardo documento
                    Documento doc = crearDocumento(n, fdate, fname);

                    //ejecuto proceso de consenso alcanzado
                    try
                    {
                        doc.grupo = grupo;
                        doc.EjecutarConsenso();
                    }
                    catch (Exception ex)
                    {
                        doc.addLog("EjecutarConsenso: <font color=red>" + ex.Message + "</font>");
                    }

                    //guardo el documento
                    doc.save();

                    grupo.save(grupo.path + "\\documentos\\"); //guardo copia del arbol

                    //notifico via email a todos los socios
                    if (!simulacion)
                    {
                        foreach (Usuario u in grupo.usuarios)
                            Tools.encolarMailNuevoConsenso(u.email, n.flores, n.negados, grupo.path + "\\documentos\\" + nombre + ".json");
                    }

                    //guardo el log historico en el arbol
                    Propuesta p = getPropuesta(n.id);  //obtengo el titulo del debate de cualquiera de las propuestas 
                    LogDocumento ld = new LogDocumento();
                    ld.fecha = fdate;
                    ld.titulo = p.titulo;
                    ld.icono = getModelo(n.modeloID).icono;
                    if (ld.titulo.Length > 50) ld.titulo = ld.titulo.Substring(0, 50);
                    ld.modeloNombre = getModelo(n.modeloID).nombre;
                    ld.modeloID = n.modeloID;
                    ld.x = n.x; if (ld.x == 0) ld.x = 90;
                    ld.docID = docID;
                    ld.fname = fname;
                    ld.arbol = nombre;
                    ld.objetivo = grupo.objetivo;
                    ld.flores = n.flores;
                    ld.negados = n.negados;
                    ld.URL = grupo.URL + "/grupos/" + nombre + "/documentos/" + fname + ".html";
                    logDocumentos.Add(ld);

                    //marco a todos los nodos del debate y sus propuestas
                    for (int i = 0; i < pathn.Count - 1; i++) //menos la raiz
                    {
                        pathn[i].consensoAlcanzado = true;
                        getPropuesta(pathn[i]).consensoAlcanzado = true;
                        foreach (Nodo n2 in pathn[i].children)
                            marcarConsenso(n2);
                    }

                    ret = true;
                }
            }
            return ret;
        }

        private void marcarConsenso(Nodo n)
        {
            //marco sus hijos
            n.consensoAlcanzado = true;
            getPropuesta(n).consensoAlcanzado = true;
            foreach (Nodo n2 in n.children)
            {
                n2.consensoAlcanzado = true;
                getPropuesta(n2).consensoAlcanzado = true;
                marcarConsenso(n2);
            }
        }

        private Documento crearDocumento(Nodo n, DateTime now, string fname)
        {
            Modelo m = getModelo(n.modeloID);
            Documento doc = new Documento();
            doc.fecha = now;
            doc.nombre = m.nombre;
            doc.fname = fname;
            doc.modeloID = n.modeloID;
            doc.path = grupo.path + "\\documentos\\" + fname + ".json";
            doc.URLPath = grupo.URL + "/grupos/" + grupo.nombre + "/documentos/" + fname + ".html";

            //obtengo el titulo
            //debo dibujar el documento
            //junto propuestas
            List<Propuesta> props = new List<Propuesta>();
            foreach (Nodo n1 in getPath(n.id))
            {
                Propuesta p = getPropuesta(n1);
                if (p != null) //la raiz
                    props.Add(p);
            }
            //armo HTML
            m.toHTML(props, this.grupo, "", 1024, Modelo.eModo.consenso);
            doc.titulo = m.titulo;
            
            //guardo propuestas
            doc.propuestas = props;
            
            //doc.raiz = raiz;

            return doc;
        }

        private void generarDocumentoHTML(Nodo n, DateTime now, string fname)
        {
            List<Nodo> pathn = getPath(n.id);
            Modelo m = getModelo(n.modeloID);

            if (!System.IO.Directory.Exists(grupo.path + "\\documentos"))
                System.IO.Directory.CreateDirectory(grupo.path + "\\documentos");

            if (!System.IO.File.Exists(grupo.path + "\\documentos\\styles.css"))
                System.IO.File.Copy(grupo.path + "\\..\\..\\styles.css", grupo.path + "\\documentos\\styles.css");

            //junto propuestas
            List<Propuesta> props = new List<Propuesta>();
            foreach (Nodo n1 in pathn)
            {
                Propuesta p = getPropuesta(n1);
                if (p != null) //la raiz
                    props.Add(p);
            }
           
            //firma consenso
            string ret = "";
            ret += "Documento escrito de forma cooperativa.<br>";
            ret += "Documento ID:" + fname + "<br>";
            ret += "Fecha de consenso: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "<br>";
            ret += "Ubicaci&oacute;n: <a target='_blank' href='" + grupo.URL + "/grupos/" + nombre + "/documentos/" + fname + ".html'>" + grupo.URL + "/grupos/" + nombre + "/documentos/" + fname + ".html</a><br>";
            ret += "Cooperativa: " + this.nombre + "<br>";
            ret += "Objetivo: " + this.grupo.objetivo + "<br>";
            ret += "Usuarios: " + this.grupo.usuarios.Count + "<br>";
            ret += "Activos: " + this.grupo.activos + "<br>";
            ret += "Si: (&ge; " + this.minSiPc + "%): " + n.flores + "<br>";
            ret += "No: (&le; " + this.maxNoPc + "%): " + n.negados + "<br>";

            //armo HTML
            m.firmaConsenso = ret;
            string html = m.toHTML(props, this.grupo, "", 1024, Modelo.eModo.consenso);
           
            //escribo
            System.IO.File.WriteAllText(grupo.path + "\\documentos\\" + fname + ".html", html);
        }

        //private string JSON_decode(string s) {
        //    //esta funcion esta reptida del lado del cliente
        //    s = s.Replace("[euro]", "€");
        //    s = s.Replace("[pound]", "&pound;");
        //    s = s.Replace("[mayor]", "&gt;");
        //    s = s.Replace("[menor]","&lt;");
        //    s = s.Replace("[amp]","&");
        //    s = s.Replace("[deg]", "&deg;");
        //    s = s.Replace("[ordf]", "&ordf;");
        //    s = s.Replace("[h64]", "&#64;");
        //    s = s.Replace("[Ntilde]", "&Ntilde;");
        //    s = s.Replace("[ntilde]", "&ntilde;");
        //    s = s.Replace("[ccedil]", "&ccedil;");
        //    s = s.Replace("[h43]", "&#43;");
        //    s = s.Replace("[h45]", "&#45;");
        //    s = s.Replace("[iquest]", "&iquest;");
        //    s = s.Replace("[h63]", "&#63;");
        //    s = s.Replace("[h35]", "&#35;");
        //    s = s.Replace("[frasl]","/");
        //    s = s.Replace("[h92]","\\");
        //    s = s.Replace("[h61]", "&#61;");
        //    s = s.Replace("[h36]","$");
        //    s = s.Replace("[h124]","|");
        //    s = s.Replace("[lsquo]","\"");
        //    s = s.Replace("[ldquo]", "\"");
        //    s = s.Replace("\\n", "<br>");
        //    return s;
        //}


        private int getNegados(Nodo n)
        {
            int ret = 0;
            List<Nodo> pathn = getPath(n.id);
            for (int i = 1; i < pathn.Count - 1; i++) //la raiz me la salto porque solo miro en el  mismo debate
            {
                foreach (Nodo n2 in pathn[i].children)
                    if (n2.id != pathn[i - 1].id) //busco en todos los hijos del padre que no sea yo mismo
                        ret += getHijosNegados(n2);
            }
            return ret;
        }

        private int getHijosNegados(Nodo n)
        {
            int ret = n.flores;
            foreach (Nodo n2 in n.children)
                ret += getHijosNegados(n2);
            return ret;
        }

        //public void setModelosDocumentoDefault()
        //{
        //    //creo modelos de documentos default

        //    d = new ModeloDocumento();
        //    d.id = 2;
        //    d.nombre = "Comision";
        //    d.crear(0, "Resumen y motivacion", "&iquest;porque neceistamos una nueva comision? &iquest;Que actividades realizara?", 2000);
        //    d.crear(1, "Objetivo de la comision", "&iquest;Que debe lograr la nueva comision?", 3500);
        //    d.crear(1, "Descripcion de actividades de la comision", "Detalla las actividades a realizar", 3500);
        //    d.crear(1, "A quien van dirigidas sus actuaciones", "", 3500);
        //    d.crear(2, "Capacidades necesarias", "", 3500);
        //    d.crear(3, "Composicion de la comision", "", 3500);
        //    d.crear(4, "Como medir su eficiencia", "", 3500);
        //    modelosDocumento.Add(d);

        //    d = new ModeloDocumento();
        //    d.id = 3;
        //    d.nombre = "Evento";
        //    d.crear(0, "Resumen y motivacion", "&iquest;Como sera el evento?", 3500);
        //    d.crear(1, "Objetivo del evento", "", 3500);
        //    d.crear(1, "Descripcion", "", 3500);
        //    d.crear(1, "A quien va dirigido el evento", "", 3500);
        //    d.crear(2, "Lugar", "", 3500);
        //    d.crear(2, "Materiles", "", 3500);
        //    d.crear(2, "Transporte", "", 3500);
        //    d.crear(3, "Organizacion del evento", "", 3500);
        //    d.crear(4, "Como medir su eficiencia", "", 3500);
        //    modelosDocumento.Add(d);

        //    d = new ModeloDocumento();
        //    d.id = 4;
        //    d.nombre = "Metodologia";
        //    d.crear(0, "Resumen y motivacion", "&iquest;Como sera la metodologia?", 3500);
        //    d.crear(1, "Para que sirve", "", 3500);
        //    d.crear(1, "Descripcion", "", 3500);
        //    d.crear(1, "A quien va dirigida", "", 3500);
        //    d.crear(2, "Definicion de la metodologia", "", 4500);
        //    d.crear(3, "Como medir su eficiencia", "", 3500);
        //    d.crear(3, "Como implantarla", "", 3500);
        //    d.crear(4, "Fases del desarrollo", "", 3500);
        //    d.crear(4, "Tiempo de desarrollo", "", 3500);
        //    modelosDocumento.Add(d);
        //}

        public ArbolPersonal getArbolPersonal(string email)
        {
            return getArbolPersonal(email, 0);
        }

        public ArbolPersonal getArbolPersonal(string email, int nuevoNodoID)
        {
            Usuario u = grupo.getUsuario(email);
            if (u != null)
            {
                ArbolPersonal ap = new ArbolPersonal();
                ap.raiz = raiz;
                ap.objetivo = grupo.objetivo;
                ap.URLEstatuto = grupo.URLEstatuto;
                ap.nombre = grupo.nombre;
                ap.usuarios = grupo.usuarios.Count;
                ap.cantidadFlores = cantidadFlores;
                ap.activos = grupo.activos;
                ap.simulacion = simulacion;
                ap.nuevoNodoID = nuevoNodoID;
                ap.born = born;
                ap.documentos = logDocumentos.Count;

                ap.usuario = u;
                ap.minSiPc = minSiPc;
                ap.maxNoPc = maxNoPc;

                ap.minSiValue = minSiValue;
                ap.maxNoValue = maxNoValue;

                ap.logDocumentos = logDocumentos;

                return ap;
            }
            else
                throw new appException("El usuario no existe");
        }

        public Nodo addNodo(Nodo padre, Propuesta prop){
            //verifico que el usuario tiene al menos una flor disponible
            Usuario u = grupo.getUsuario(prop.email);
            if (nombre == null)
                throw new appException("Nombre de nodo no puede ser vacio");
            else if (u == null)
                throw new appException("El usuario no existe");
            else if (padre == null)
                throw new appException("El nodo no existe");
            else
            {
                //agrego nuevo nodo
                Nodo nuevo = new Nodo();
                nuevo.nombre = prop.etiqueta;
                nuevo.id = lastNodoID++;
                nuevo.modeloID = prop.modeloID;
                nuevo.email = prop.email;

                try
                {
                    //agrego al arbol
                    if (leftRight)
                        padre.children.Add(nuevo);
                    else
                        padre.children.Insert(0, nuevo);

                    //seguridad
                    if (prop.nivel != getPath(nuevo.id).Count - 1)  //quito la raiz
                        throw new Exception("El nivel de la propuesta no coincide con el del arbol");

                    //fijo nivel
                    nuevo.nivel = prop.nivel;

                    leftRight = !leftRight;

                    //agrego la propuesta
                    prop.nodoID = nuevo.id;  //ahora si tiene nodo
                    propuestas.Add(prop);

                    //consumo una flor
                    asignarflor(u, nuevo);
                }
                catch (Exception ex)
                {
                    //no se pudo agregar, quito nodo nuevo
                    padre.children.Remove(nuevo);
                    throw ex;
                }
                return nuevo;
            }
        }

        public Usuario getUsuarioConFloresDisponibles()
        {
            Usuario ret = null;
            foreach (Usuario u in grupo.usuarios)
            {
                if (u.floresDisponibles().Count > 0)
                {
                    ret = u;
                    break;
                }
            }
            return ret;
        }

        public Usuario quitarFlor(Nodo n)
        {
            Usuario u = null;
            Flor f = null;

            if (n.flores > 0)
            {
                //busco un usuario que haya votado ese nodo
                foreach (Usuario u2 in grupo.usuarios)
                {
                    foreach (Flor f2 in u2.flores)
                        if (f2.id == n.id)
                        {
                            f = f2;
                            u = u2;
                        }
                }

                //libero la flor al usuario
                n.flores -= 1;
                f.id = 0;

                //borro la parte de la rama que no tenga flores
                verifyNodoSinFlores(n.id);

                //actualizo negados
                actualizarNegados();

                //veo si algun nodo alcanza el consenso
                comprobarConsenso();
            }
            else
                throw new Exception("El nodo no tiene flores para quitar");
            return u;
        }

        public bool quitarFlor(Nodo n, Usuario u)
        {
            bool ret = false;
            Flor f = u.getFlor(n.id);

            if (f == null)
                throw new Exception("El usuario no tiene esa flor para quitar");

            //libero la flor al usuario
            n.flores -= 1;
            f.id = 0;
            f.born = DateTime.Now;

            //borro la parte de la rama que no tenga flores
            ret = verifyNodoSinFlores(n.id);

            //actualizo negados
            actualizarNegados();

            //veo si algun nodo alcanza el consenso
            comprobarConsenso();

            return ret;
        }

        public void asignarflor(Usuario u, Nodo n)
        {
            //si tiene una flor en nivel anterior la subo, si no uso una de las disponibles
            //si hay flor en pathn entonces la subo
            bool subida = false;

            if (n.consensoAlcanzado)
                throw new appException("Este debate ya ha alcanzado el consenso");

            foreach (Nodo padre in getPath(n.id))
            {
                Flor usada = u.getFlor(padre.id);
                if (usada != null)
                {
                    //hay flor en un nodo anterior la subo
                    padre.flores -= 1;
                    usada.id = n.id;
                    usada.born = DateTime.Now;
                    n.flores += 1;
                    subida = true;

                    break;
                }
            }
            if (!subida)
            {
                //uso una flor disponible
                List<Flor> disponibles = u.floresDisponibles();
                if (disponibles.Count > 0)
                {
                    disponibles[0].id = n.id;
                    disponibles[0].born = DateTime.Now;
                    n.flores += 1;
                }
                else
                    throw new appException("No tienes flores disponibles");
            }
            //compruebo consenso alcanzado
            comprobarConsenso(n);

            //actualizo negados
            actualizarNegados();
        }

        private void actualizarNegados()
        {
            List<Nodo> nodos = toList();
            foreach (Nodo n in nodos)
            {
                Modelo m = getModelo(n.modeloID);
                if (m != null && n.nivel == m.niveles)
                {
                    //es una hoja de final de documento
                    n.negados = getNegados(n);
                }
            }
        }

        private bool verifyNodoSinFlores(int id)
        {
            //borro la parte de la rama que no tenga flores
            List<Nodo> pathn = getPath(id);
            bool ret = false;
            Nodo n = pathn[0];
            while (n.flores <= 0 && n.children.Count == 0)
            {
                removeNodo(n.id);
                ret = true;  //se ha quitado el nodo
                pathn.RemoveAt(0);
                if (pathn.Count > 0)
                    n = pathn[0];
                else
                    break;
            }
            return ret;
        }

        public Propuesta getPropuesta(int id)
        {
            foreach (Propuesta op in propuestas)
            {
                if (op.nodoID == id)
                    return op;
            }
            return null;
        }

        private void removeNodo(int id)
        {
            //quito el nodo
            removeNodo(raiz, id);

            //quito la propuesta
            foreach (Propuesta op in propuestas)
            {
                if (op.nodoID == id)
                {
                    propuestas.Remove(op);
                    break;
                }
            }
        }

        private void removeNodo(Nodo padre, int id)
        {
            foreach (Nodo hijo in padre.children)
            {
                if (hijo.id == id)
                {
                    padre.children.Remove(hijo);
                    break;
                }
                else
                    removeNodo(hijo, id);
            }
        }

        public Nodo getNodo(int id)
        {
            List<Nodo> p = getPath(id);
            if (p != null && p.Count > 0)
                return p[0];
            else
                return null;
        }

        public List<Nodo> getPath(int id)
        {
            //lista de ancestros. incluye la raiz y incluye al propio nodo
            //index=0 es el nodo buscado
            //index=count-1 es la raiz
            List<Nodo> ret = new List<Nodo>();
            getPath(raiz, id, ret);
            return ret;
        }

        public Propuesta getPropuesta(Nodo n)
        {
            //devuelvo la propuesta resultado de comparar el texto con sus hermanos
            Propuesta op = getPropuesta(n.id);
            return op;
        }

        private void getPath(Nodo padre, int id, List<Nodo> ret)
        {      
            if (padre.id == id)
            {               
                ret.Add(padre);
            }
            else
            {
                foreach (Nodo hijo in padre.children)
                {
                    int count = ret.Count;
                    getPath(hijo, id, ret);
                    if (ret.Count > count) //encontrado
                    {
                        //agrego padres
                        ret.Add(padre);
                    }
                }
            }
        }

        public void actualizarModelosEnUso()
        {
            //marco los modelos de documentos que estan en uso
            foreach (Modelo m in modelos)
            {
                m.enUso = false;
                foreach (Propuesta p in propuestas)
                    if (p.modeloID == m.id)
                    {
                        m.enUso = true;
                        break;
                    }
            }
        }

        public Modelo getModelo(string modeloID)
        {
            Modelo ret = null;

            foreach (Modelo m in modelos)
            {
                if (m.id == modeloID)
                {
                    ret = m;
                }
            }
            return ret;
        }


    }
}