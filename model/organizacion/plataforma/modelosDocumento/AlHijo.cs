﻿///////////////////////////////////////////////////////////////////////////
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


using System;
using System.Collections.Generic;
using System.Web;

//Los modelos deben darse de alta en Modelo.getModelosDocumento()

namespace nabu.plataforma.modelos
{
    public class AlHijo: Modelo
    {
        public AlHijo()
        {
            icono = "res/documentos/alhijo.png";
            niveles = 4;
            nombre = "AlHijo";
            descripcion = "Comunicado al grupo hijo";
            tipo = "intergrupal";

            crearVariables();
        }

        public override string carpeta()
        {
            return "AlHijo";
        }

        protected override void crearVariables()
        {
            variables.Clear();
            variables.Add(new Variable("s.etiqueta", 12, 1));
            variables.Add(new Variable("s.titulo", 70, 1));
            variables.Add(new Variable("s.hijonombre", 70, 1));
            variables.Add(new Variable("s.hijourl", 70, 1));
            variables.Add(new Variable("s.introduccion", 4000, 1));

            //nivel 2
            variables.Add(new Variable("s.situacionactual", 4000, 2));

            //nivel 3
            variables.Add(new Variable("s.propuesta", 4000, 3));

            //nivel 4
            variables.Add(new Variable("s.situaciondeseada", 12000, 4));
        }

        private void validar(Propuesta prop, string idioma)
        {
            if (prop != null)
            {
                if (prop.nivel == 1)
                {
                    if (getText("s.hijonombre", prop) == "")
                    {
                        addError(1, Tools.tr("Se debe seleciconar un grupo hijo para el envio", idioma));
                        getVariable("s.hijonombre").className = "errorfino";
                    }
                    if (getText("s.titulo", prop) == "")
                    {
                        addError(1, Tools.tr("El titulo del comunicado no puede ser vacio", idioma));
                        getVariable("s.titulo").className = "errorfino";
                    }
                    if (getText("s.introduccion", prop) == "")
                    {
                        addError(1, Tools.tr("La introduccion no puede ser vacia", idioma));
                        getVariable("s.vision").className = "errorfino";
                    }
                }
                else if (prop.nivel == 2)
                {
                    if (getText("s.situacionactual", prop) == "")
                    {
                        addError(2, Tools.tr("La situacion actual no ser vacia", idioma));
                    }
                }
                else if (prop.nivel == 3)
                {
                    if (getText("s.propuesta", prop) == "")
                    {
                        addError(2, Tools.tr("La propuesta no puede ser vacia", idioma));
                    }
                }
                else if (prop.nivel == 4)
                {
                    if (getText("s.situaciondeseada", prop) == "")
                    {
                        addError(2, Tools.tr("La situacion deseada no puede ser vacia", idioma));
                    }
                }
            }
        }

        override protected string toHTMLContenido(int nivel, Propuesta prop, Grupo g, string email, int width)
        {
            string ret = "";
            Usuario u = g.getUsuario(email);
            bool tieneFlores = false;
            if (u != null) tieneFlores = u.floresDisponibles().Count > 0;

            bool editar = (prop == null && tieneFlores && modo != eModo.prevista && modo != eModo.consenso)
                || (prop != null && prop.esPrevista() && (modo == eModo.revisar || modo == eModo.editar));
            editar = editar && !consensoAlcanzado;
            bool puedeVariante = prop != null && !prop.esPrevista() && modo == eModo.editar && tieneFlores && !consensoAlcanzado;


            //validaciones de este nivel
            validar(prop, g.idioma);

            if (nivel == 1)
            {
                ret += HTMLEncabezado(prop, g, email, width);

                if (g.hijos.Count == 0)
                    ret += "<div class='titulo1'>" + Tools.tr("No hay grupos hijos en este grupo", g.idioma) + "</div>";
                else
                {
                    //hijo
                    string listahijos = "|";
                    foreach (Hijo h in g.hijos)
                        listahijos += h.nombre + "|";

                    ret += "<div class='titulo2'>" + Tools.tr("Comunicado al grupo hijo", g.idioma) + ":" + HTMLLista("s.hijonombre", listahijos, prop, 250, tieneFlores, g.idioma) + "</div>";

                    //fiho url
                    if (getText("s.hijonombre", prop) != "")
                    {
                        foreach (Hijo h in g.hijos)
                            if (h.nombre == getText("s.hijonombre", prop))
                                prop.bag["s.hijourl"] = h.URL;
                    }

                    //tema
                    ret += "<div class='tema'>" + Tools.tr("Introduccion", g.idioma) + "</div>";
                    if (editar)
                        ret += "<div class='smalltip' style='width:" + width + "px'>"
                            + Tools.tr("alhijo.introduccion", g.idioma)
                            + "</div>";

                    ret += HTMLArea("s.introduccion", prop, width, 120, tieneFlores, g.idioma);

                    //variante
                    if (puedeVariante)
                        ret += "<div style='width:" + width + "px;text-align:right;'><input type='button' class='btn' value='" + Tools.tr("Proponer variante", g.idioma) + "' onclick='doVariante(" + prop.nodoID + ")'></div>";
                }
            }
            else if (nivel == 2)
            {
                if (g.hijos.Count != 0)
                {
                    //Mision
                    ret += "<div class='tema'>" + Tools.tr("Situacion actual", g.idioma) + "</div>";
                    if (editar)
                        ret += "<div class='smalltip' style='width:" + width + "px'>" + Tools.tr("alhijo.situacionactual", g.idioma) + "</div>";

                    ret += HTMLArea("s.situacionactual", prop, width, 290, tieneFlores, g.idioma);

                    //variante
                    if (puedeVariante)
                        ret += "<div style='width:" + width + "px;text-align:right;'><input type='button' class='btn' value='" + Tools.tr("Proponer variante", g.idioma) + "' onclick='doVariante(" + prop.nodoID + ")'></div>";
                }

            }
            else if (nivel == 3)
            {
                if (g.hijos.Count != 0)
                {
                    //Objetivo
                    ret += "<div class='tema'>" + Tools.tr("Propuesta", g.idioma) + "</div>";
                    if (editar)
                        ret += "<div class='smalltip' style='width:" + width + "px'>" + Tools.tr("alhijo.propuesta", g.idioma) + "</div>";

                    ret += HTMLArea("s.propuesta", prop, width, 290, tieneFlores, g.idioma);

                    //variante
                    if (puedeVariante)
                        ret += "<div style='width:" + width + "px;text-align:right;'><input type='button' class='btn' value='" + Tools.tr("Proponer variante", g.idioma) + "' onclick='doVariante(" + prop.nodoID + ")'></div>";
                }
            }
            else if (nivel == 4)
            {
                if (g.hijos.Count != 0)
                {
                    ret += "<div class='tema'>" + Tools.tr("Situacion deseada", g.idioma) + "</div>";
                    if (editar)
                        ret += "<div class='smalltip' style='width:" + width + "px'>" + Tools.tr("alhijo.situaciondeseada", g.idioma) + "</div>";

                    ret += HTMLArea("s.situaciondeseada", prop, width, 550, tieneFlores, g.idioma);

                    //variante
                    if (puedeVariante)
                        ret += "<div style='width:" + width + "px;text-align:right;'><input type='button' class='btn' value='" + Tools.tr("Proponer variante", g.idioma) + "' onclick='doVariante(" + prop.nodoID + ")'></div>";
                }
            }
            else
            {
                throw new Exception(Tools.tr("Nivel [%1] no existe en este modelo", nivel.ToString(), g.idioma));
            }

            if (prop != null) prop.niveles = niveles; //esto es importante si cambian los niveles para que se traspase luego al nodo

            //fin nivel
            if (prop != null && prop.nodoID != 0 && modo != eModo.consenso)
                ret += HTMLFlores(g.arbol.getNodo(prop.nodoID), false, g.getUsuario(email));

            //mensajes de error
            if (errores.ContainsKey(nivel))
            {
                ret += "<div class='error' style='width:" + (width-4) + "px'>" + errores[nivel] + "</div>";
            }
            return ret;
        }

        public override void ejecutarConsenso(Documento doc)
        {
            try
            {
                string htmlpath = doc.path.Replace(".json", ".html");
                string hijonombre = (string)doc.getValor("s.hijonombre");
                string hijourl = (string)doc.getValor("s.hijourl");
                string ret = Tools.getFileHttp(hijourl + "/doAprendemos.aspx?actn=crearEvaluacionAlHijo"
                    + "&docnombre=" + doc.nombre
                    + "&doctitulo=" + doc.titulo
                    + "&docmodeloid=AlHijo"
                    + "&grupohijo=" + hijonombre
                    + "&grupopadre=" + doc.grupo.nombre,
                    htmlpath);
                if (ret == "ok")
                    doc.addLog(Tools.tr("Documento publicado en Evaluamos en el grupo hijo", doc.grupo.idioma) + ": " + hijonombre);
                else
                    doc.addLog(Tools.tr("Respuesta del grupo hijo", doc.grupo.idioma) + ": " + ret);
            }
            catch (Exception ex)
            {
                doc.addLog(Tools.tr("No se pudo publicar documento en el grupo hijo", doc.grupo.idioma) + ": " + ex.Message);
            }
        }


    }
}

