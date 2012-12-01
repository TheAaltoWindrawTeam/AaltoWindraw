﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AaltoWindraw.Network
{
    public class Commons
    {
        public enum PacketType{
            DRAWING_REQUEST,
            WHO_REQUEST,
            ITEMS_REQUEST,

            SEND_SCORE,
            SEND_DRAWING,

            SCORE_STORED,
            SCORE_NOT_STORED,
            DRAWING_STORED,
            DRAWING_NOT_STORED,
        }
    }
}