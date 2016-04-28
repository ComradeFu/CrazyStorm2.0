﻿/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public static class VersionInfo
    {
        public const string AppName = "Crazy Storm";
        public const string Version = "2.0";
        public const string Extension = ".bgp";
        public static string AppTitle { get { return AppName + " " + Version; } }
    }
}
