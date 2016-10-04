﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DraggableElementDisplay: DraggableToSlot
    {
    public Element Element;

        public void Awake()
        {
            Element=new Element("","","");
        }

        public void DisplayName(Element e)
    {
        Text nameText = GetComponentsInChildren<Text>().Single(t => t.name == "txtElementName");
        nameText.text = e.Label;
    }

    public void DisplayIcon(Element e)
    {
        Image elementImage = GetComponentsInChildren<Image>().Single(i => i.name == "imgElementIcon");
        Sprite elementSprite = Resources.Load<Sprite>("FlatIcons/png/32px/" + e.Id);
        elementImage.sprite = elementSprite;
    }

    public void DisplayQuantity(int quantity)
    {
        Text quantityText = GetComponentsInChildren<Text>().Single(t => t.name == "txtQuantity");
        quantityText.text = quantity.ToString();

    }

        public void PopulateForElementId(string elementId,ContentManager cm)
        {
        Element = cm.PopulateElementForId(elementId);
        DisplayName(Element);
        DisplayIcon(Element);
    }



}


