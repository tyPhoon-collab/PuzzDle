namespace PiecesEnum
{
    public enum Pieces{ 
        NONE = -1,
        ATK,     //RED,
        DEF,     //BLUE,
        HEALTH,  //GREEN,
        MONEY,   //LIGHT,
        FOOD,    //DARK,
        MAX,
        EFK
    }

    public enum Effects{ 
        NONE = -1,
        CRYSTAL,
        TREASURE,
        RARE_PIECES_MAX,
        BOMB,
        DELETE_SAME_PIECE
    }
}
