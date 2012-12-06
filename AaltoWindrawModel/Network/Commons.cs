using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AaltoWindraw.Network
{
    public class Commons
    {
        public enum PacketType{
            // Requests from client
            DRAWING_REQUEST,
            DRAWING_BY_ID_REQUEST,
            WHO_REQUEST,
            ITEMS_REQUEST,
            HIGHSCORES_REQUEST,

            IS_HIGHSCORE_REQUEST,

            SEND_SCORE,
            SEND_DRAWING,
            SEND_ITEM,

            // Responses from server
            IS_HIGHSCORE,
            IS_NOT_HIGHSCORE,

            DRAWING_FOUND,
            NO_DRAWING_FOUND,
            SCORE_STORED,
            SCORE_NOT_STORED,
            DRAWING_STORED,
            DRAWING_NOT_STORED,
            ITEM_SAVED,
            ITEM_NOT_SAVED,
        }
    }
}
