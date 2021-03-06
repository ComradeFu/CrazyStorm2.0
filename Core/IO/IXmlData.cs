﻿/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2017 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CrazyStorm.Core
{
    public interface IXmlData : ICloneable
    {
        XmlElement  BuildFromXml(XmlElement node);
        XmlElement  StoreAsXml(XmlDocument doc, XmlElement node);
    }
}
