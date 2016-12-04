﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

public class Character
    {

    private string _title;
    private string _firstName;
    private string _lastName;
    public CharacterState State { get; set; }
    private string _endingTriggeredId=null;

    public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;

        }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;

         }

        }

    public string EndingTriggeredId
    { get { return _endingTriggeredId; } }




        public Character():base()
        {
            
      
            State = CharacterState.Viable;
        

        }


    }

