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
using System.Linq;
using System.Web;

//representa un grupo de trabajo

namespace nabu.plataforma
{
    public class GrupoTrabajo
    {
        public int EID = 0;
        public string nombre = "";
        public string docURL = "";
        public DateTime docTs = Tools.minValue;
        public string revision = "";
        public string objetivo = "";
        public List<string> integrantes = new List<string>(); //emails
        public List<nabu.plataforma.Estrategia> estrategias = new List<nabu.plataforma.Estrategia>();
        public DateTime born = DateTime.Now;

        //datos del arbol si es que tiene
        public string grupoURL = "";
        public string grupoNombre = "";
        public string grupoOrganizacion = "";
        public string grupoIdioma = "";

    }
}