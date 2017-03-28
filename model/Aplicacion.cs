﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nabu
{
    public class Aplicacion
    {
        public List<Grupo> grupos = new List<Grupo>();
        public HttpServerUtility server;
        public HttpRequest request;
        public DateTime lastSave = DateTime.MinValue;
        public int saveTime = 10; //guardar arboles cada x minutos
        public int cleanTime = 40; //quito arbol de memoria si no se toca en 20 minutos

        public Aplicacion(HttpServerUtility server, HttpRequest request)
        {
            this.server = server;
            this.request = request;
        }

        public void verifySave()
        {
            lock (this)
            {
                if (DateTime.Now.Subtract(lastSave).TotalMinutes > saveTime)
                {
                    saveGrupos();

                    depurarMemoria();

                    lastSave = DateTime.Now;
                }
            }
        }

        private void depurarMemoria()
        {
            //depuro arboles viejos de memoria
            lock (grupos)
            {
                int index = 0;
                while (index < grupos.Count)
                {
                    if (DateTime.Now.Subtract(grupos[index].ts).TotalMinutes > cleanTime)
                    {
                        //este grupo se puede quitar de memoria
                        //se asume que ya fue guardado
                        //si es simulacion borro temporales
                        Grupo g = grupos[index];
                        Arbol a = g.arbol;
                        if (a.simulacion)
                            if (System.IO.Directory.Exists(g.path))
                                System.IO.Directory.Delete(g.path, true);

                        grupos.RemoveAt(index);
                    }
                    else
                        index += 1;
                }
            }
        }

        public void saveGrupos()
        {
            lock (grupos)
            {
                foreach (Grupo g in grupos)
                {
                    lock (g)
                    {
                        g.save(server.MapPath("grupos/" + g.nombre));
                    }
                }
            }
        }

        public Grupo getGrupo(string nombre)
        {
            Grupo ret = null;

            lock (grupos)
            {
                foreach (Grupo g in grupos)
                {
                    if (g.nombre == nombre)
                    {
                        ret = g;
                    }
                }

                if (ret == null)
                {
                    //no existe en la lista lo busco en la carpeta y lo cargo
                    ret = loadGrupo(nombre);
                    grupos.Add(ret);
                }
            }

            return ret;
        }

        public Grupo loadGrupo(string nombre)
        {
            string jsonpath = server.MapPath("grupos/" + nombre + "/" + nombre + ".json");
            if (System.IO.File.Exists(jsonpath))
            {
                Grupo ret;
                System.IO.StreamReader fs = new System.IO.StreamReader(jsonpath, System.Text.Encoding.UTF8);
                //System.IO.StreamReader fs = System.IO.File.OpenText(jsonpath);
                string s = fs.ReadToEnd();
                fs.Close();

                List<Type> tipos = new List<Type>();
                tipos.Add(typeof(Arbol));
                tipos.Add(typeof(Usuario));
                tipos.Add(typeof(Nodo));
                tipos.Add(typeof(Comentario));
                tipos.Add(typeof(Variable));

                //tipos
                foreach (Organizacion org in Organizacion.getOrganizaciones())
                {
                    tipos.Add(org.GetType());
                    foreach (Modelo mod in org.getModelos())
                        tipos.Add(mod.GetType());
                }
                ret = Tools.fromJson<Grupo>(s, tipos);
                ret.path = server.MapPath("grupos/" + nombre);
                ret.URL = request.UrlReferrer.AbsoluteUri.Substring(0, request.UrlReferrer.AbsoluteUri.LastIndexOf("/"));

                //modelos viejos
                if (ret.arbol == null)
                {
                    ret.arbol = new Arbol();
                }

                //actualizo modelos
                ret.arbol.grupo = ret;  //padre del arbol, referencia ciclica, no se puede serializar
                return ret;
            }
            else
                throw new appException("El grupo no existe");
        }

        public void addLog(string accion, string ip, string grupo, string email, string descripcion)
        {
            lock (this)
            {
                string l = "<tr><td>" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "</td>";
                l += "<td>" + accion + "</td>";
                l += "<td>" + grupo + "</td>";
                l += "<td>" + email + "</td>";
                l += "<td>" + descripcion + "</td></tr>";

                string fname = server.MapPath("logs.html");

                if (!System.IO.File.Exists(fname))
                {
                    string hd;
                    hd = "<head>";
                    hd += "<title>Nab&uacute logs</title>";
                    hd += "<link rel=\"stylesheet\" type=\"text/css\" href=\"styles.css\">";
                    hd += "</head>";
                    hd += "<h1>Nab&uacute;</h1>";
                    hd += "<h2>Logs</h2>";
                    hd += "<table style='border: 1px solid gray; padding: 5px; border-radius: 10px;font-family: Verdana, Geneva, Tahoma, sans-serif; font-size: small;'>";
                    hd += "<tr>";
                    hd += "<td style='width:120px;'><b>IP</b></td>";
                    hd += "<td style='width:120px;'><b>Email</b></td>";
                    hd += "<td style='width:120px;'><b>Arbol</b></td>";
                    hd += "<td style='width:120px;'><b>TS</b></td>";
                    hd += "<td style='width:800px;'><b>Descripcion</b></td>";
                    hd += "</tr>";
                    System.IO.File.AppendAllText(fname, hd);
                }

                System.IO.File.AppendAllText(fname, l);
            }
        }
    }
}