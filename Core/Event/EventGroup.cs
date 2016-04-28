﻿/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class EventGroup : ICloneable, IXmlData
    {
        #region Private Members
        [XmlAttribute]
        string name;
        [XmlAttribute]
        string condition;
        IList<string> events;
        #endregion

        #region Public Members
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public string Condition
        {
            get { return condition; }
            set { condition = value; }
        }
        public IList<string> Events { get { return events; } }
        #endregion

        #region Constructor
        public EventGroup()
        {
            name = "NewEventGroup";
            events = new ObservableCollection<string>();
        }
        #endregion

        #region Public Methods
        public object Clone()
        {
            var clone = MemberwiseClone() as EventGroup;
            clone.events = new ObservableCollection<string>();
            foreach (var item in events)
                clone.events.Add(item);

            return clone;
        }
        public XmlElement BuildFromXml(XmlDocument doc, XmlElement node)
        {
            throw new NotImplementedException();
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var eventGroupNode = doc.CreateElement("EventGroup");
            XmlHelper.StoreFields(this, doc, eventGroupNode);
            //events
            XmlHelper.StoreList(events, doc, eventGroupNode, "Events");
            node.AppendChild(eventGroupNode);
            return node;
        }
        #endregion
    }
}
