﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nabu
{
    public class Config
    {
        public List<string> grupos = new List<string>();
        public string browser = ""; //tipo de navegador
        public string type = "";
        public string version = "";
        public int width;
        public int height;
    }
}