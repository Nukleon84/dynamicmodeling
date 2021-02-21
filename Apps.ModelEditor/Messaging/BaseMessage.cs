﻿using System;


namespace Apps.ModelEditor.Messaging
{
    public class BaseMessage
    {
        public DateTime TimeStamp { get; set; }

        public object Sender
        {
            get;
            set;
        }

        public object Parameter
        {
            get;
            set;
        }

        public BaseMessage()
        {
            TimeStamp = DateTime.Now;
        }
    }
}
