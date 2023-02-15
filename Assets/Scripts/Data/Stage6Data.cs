using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;


public class Stage6Data
{
    [XmlAttribute]
    public int Level;
    [XmlAttribute]
    public string Blocks;
    [XmlAttribute]
    public string Cells;
}