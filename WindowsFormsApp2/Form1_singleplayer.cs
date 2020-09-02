using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    
    public class MyPictureBox : PictureBox
    {
        public int coordinate_x = 0;
        public int coordinate_y = 0;
        public int trans_id;
    }
    public class Coordinates
    {
        public int x;
        public int y;
    }
    public class ChessPiece
    {
        public int id; //kladne biele, zaporne cierne
        public bool exist;
        public List<Coordinates> possible_moves = new List<Coordinates>();
    }
    public class Castling
    {
        public bool w_king_not_moved = true;
        public bool b_king_not_moved = true;
        public bool w_l_rook_not_moved = true;
        public bool w_r_rook_not_moved = true;
        public bool b_l_rook_not_moved = true;
        public bool b_r_rook_not_moved = true;
    }
    public static class AIBrain
    {

    }
    public class AI
    {
        int ai_turn;
        Coordinates move = new Coordinates();
        ChessPiece[,] game_state = new ChessPiece[8, 8];
        
        public void FindAITurn(int player_turn)
        {
            ai_turn = -1 * player_turn;
        }
        public void GetChessboardState(ChessPiece[,] state_of_game)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ChessPiece piece = new ChessPiece();
                    game_state[i, j] = piece;
                    game_state[i, j].id = state_of_game[i, j].id;
                    game_state[i, j].exist = state_of_game[i, j].exist;
                    game_state[i, j].possible_moves = state_of_game[i, j].possible_moves;
                    

                }
            }
        }
        public void FindBestMove(int a)
        {
            move = AIBrain.Max(game_state);

        }
        
    }
    public partial class TheChess : Form
    {
        int turn = 1;
        Boolean first_click = true;
        ChessPiece[,] chessboard = new ChessPiece[8, 8];
        List<MyPictureBox> picture_boxes = new List<MyPictureBox>();
        List<MyPictureBox> trans_list = new List<MyPictureBox>();
        int player_turn; //precita zo vstupu
        Coordinates from = new Coordinates();
        Castling castling = new Castling();
        /*public void DrawRectangle(EventArgs e)
        {
            Pen pen = new Pen(Brushes.Black, 5);
            pen.Width = 10;

            e.Graphics.DrawRectangle(pen, 0, 0, 600, 600);
        }*/
        public void CreatePBoxes()
        {
            int black = 180;
            int white = 0;
            Pen pen = new Pen(Brushes.Black, 5);
            pen.Width = 10;
            //nakresli obdlznik okolo
            //System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing)
            //System.Drawing.Graphics form_graphics;
            //form_graphics.FillRectangle()
            //DrawRectangle(pen, 0, 0, 600, 600);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    MyPictureBox pbox = new MyPictureBox();
                    picture_boxes.Add(pbox);

                    if (((i % 2 == 0) && (j % 2 == 1)) || ((i % 2 == 1) && (j % 2 == 0)))
                    {
                        pbox.BackColor = Color.FromArgb(black, 0, 0, 0);
                    }
                    else
                    {
                        pbox.BackColor = Color.FromArgb(white, 0, 0, 0);
                    }
                    pbox.Parent = this;
                    pbox.Height = 75;
                    pbox.Width = 75;
                    pbox.Visible = true;
                    pbox.coordinate_x = i;
                    pbox.coordinate_y = j;
                    pbox.Top = i * 75;
                    pbox.Left = j * 75;
                    pbox.SizeMode = PictureBoxSizeMode.StretchImage;
                    pbox.BorderStyle = BorderStyle.FixedSingle;
                    pbox.Click += PicClicked;
                }
            }
        }
        public void FillArray()
        {

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ChessPiece piece = new ChessPiece();
                    chessboard[i, j] = piece;
                    if (((i <= 1) || (i >= 6)))
                    {
                        chessboard[i, j].exist = true;
                        if ((i == 1) || (i == 6)) //pesiak
                        {
                            if (i == 6)
                            {
                                chessboard[i, j].id = 1;
                            }
                            if (i == 1)
                            {
                                chessboard[i, j].id = -1;
                            }
                        }
                        switch (j)
                        {
                            case 0: //veza
                            case 7:
                                if (i == 7)
                                {
                                    chessboard[i, j].id = 2;
                                }
                                if (i == 0)
                                {
                                    chessboard[i, j].id = -2;
                                }
                                break;
                            case 1: //jazdec
                            case 6:
                                if (i == 7)
                                {
                                    chessboard[i, j].id = 3;
                                }
                                if (i == 0)
                                {
                                    chessboard[i, j].id = -3;
                                }
                                break;
                            case 2: //strelec
                            case 5:
                                if (i == 7)
                                {
                                    chessboard[i, j].id = 4;
                                }
                                if (i == 0)
                                {
                                    chessboard[i, j].id = -4;
                                }
                                break;
                            case 3: //dama
                                if (i == 7)
                                {
                                    chessboard[i, j].id = 5;
                                }
                                if (i == 0)
                                {
                                    chessboard[i, j].id = -5;
                                }
                                break;
                            case 4: //kral
                                if (i == 7)
                                {
                                    chessboard[i, j].id = 6;
                                }
                                if (i == 0)
                                {
                                    chessboard[i, j].id = -6;
                                }
                                break;
                        }


                    }
                    else
                    {
                        chessboard[i, j].exist = false;
                        chessboard[i, j].id = 0;
                    }
                }
            }
            //vypln pole figurkami podla zaciatocneho rozostavenia
        }
        public bool IsInChessBoard(int x, int y)
        {
            if ((x < 8) && (y < 8) && (x >= 0) && (y >= 0))
                return (true);
            else
                return (false);
        }
        public void ClearPossibleMoves(int x, int y)
        {
            chessboard[x, y].possible_moves.Clear();
        }
        public void AddPossibleMove(int a, int b, int base_x, int base_y)
        {
            Coordinates cord = new Coordinates();
            cord.x = a;
            cord.y = b;
            chessboard[base_x, base_y].possible_moves.Add(cord);
        }
        public void DeleteCheckMoves(int x, int y)
        {
            List <Coordinates> to_delete = new List<Coordinates>();
            bool was_deleted = false;
            foreach (Coordinates coord in chessboard[x, y].possible_moves)
            { 

                int id_backup = chessboard[coord.x, coord.y].id;
                bool exist_backup = chessboard[coord.x, coord.y].exist;
                bool w_l_r_backup = castling.w_l_rook_not_moved;
                bool w_r_r_backup = castling.w_r_rook_not_moved;
                bool b_r_r_backup = castling.b_r_rook_not_moved;
                bool b_l_r_backup = castling.b_l_rook_not_moved;
                bool w_k_backup = castling.w_king_not_moved;
                bool b_k_backup = castling.b_king_not_moved;
                MakeMove(x, y, coord.x, coord.y);
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if ((chessboard[i, j].id * turn < 0) && (chessboard[i, j].exist == true))
                        {
                            UpdatePossibleMoves(i, j);
                        }
                    }
                }
               // UpdatePossibleMoves(i, j);//iba pre opacnu farbu
                if (FindCheck(6*turn) == true)
                {

                    to_delete.Add(coord);
                    was_deleted = true;

                    
                }
                //restoration,  lebo makemove meni stav ___ not moved
                chessboard[x, y].id = chessboard[coord.x, coord.y].id;
                chessboard[x, y].exist = chessboard[coord.x, coord.y].exist;
                chessboard[coord.x, coord.y].id = id_backup;
                chessboard[coord.x, coord.y].exist = exist_backup;
                chessboard[coord.x, coord.y].exist = exist_backup;
                castling.w_l_rook_not_moved = w_l_r_backup;
                castling.w_r_rook_not_moved = w_r_r_backup;
                castling.b_r_rook_not_moved = b_r_r_backup;
                castling.b_l_rook_not_moved = b_l_r_backup;
                castling.w_king_not_moved = w_k_backup;
                castling.b_king_not_moved = b_k_backup;
                //rosada
                if (((chessboard[x, y].id == 6) || (chessboard[x, y].id == -6))
                    && (y == 4) && ((coord.y == 6) || (coord.y == 2)))
                {
                    //posunutie vezi
                    if (coord.y == 6)
                    {

                        chessboard[x, 7].exist = true;
                        chessboard[x, 7].id = chessboard[x, 5].id;
                        chessboard[x, 5].exist = false;
                        chessboard[x, 5].id = 0;
                    }
                    if (coord.y == 2)
                    {
                        chessboard[x, 0].exist = true;
                        chessboard[x, 0].id = chessboard[x, 3].id;
                        chessboard[x, 3].exist = false;
                        chessboard[x, 3].id = 0;
                    }
                }
            }
            if (was_deleted)
            {
                foreach (Coordinates forbidden_move in to_delete)
                {
                    chessboard[x, y].possible_moves.Remove(forbidden_move);   
                }
            }

        }
        public void UpdatePossibleMoves(int x, int y)
        {
            //spocitaj moves pre figurku na suradniciach x,y
            ClearPossibleMoves(x, y);
            switch (chessboard[x, y].id)
            {
                case 1: //biely pesiak
                    if ((IsInChessBoard(x - 1, y) == true) && (chessboard[x - 1, y].exist == false))
                    {
                        AddPossibleMove(x - 1, y, x, y);

                    }
                    if ((IsInChessBoard(x - 1, y - 1) == true) &&
                        (chessboard[x - 1, y - 1].exist == true) && (chessboard[x - 1, y - 1].id < 0))
                    {/*
                        ChessPiece[,] chessboard_backup = new ChessPiece[8, 8];
                        chessboard_backup = chessboard;
                        MakeMove(x, y, x - 1, y - 1);
                        if (FindCheck(6 * turn) == false)
                        {
                            AddPossibleMove(x - 1, y - 1, x, y);
                        }
                        chessboard = chessboard_backup;*/
                        AddPossibleMove(x - 1, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x - 1, y + 1) == true) &&
                        (chessboard[x - 1, y + 1].exist == true) && (chessboard[x - 1, y + 1].id < 0))
                    {/*
                        ChessPiece[,] chessboard_backup = new ChessPiece[8, 8];
                        chessboard_backup = chessboard;
                        MakeMove(x, y, x - 1, y + 1);
                        if (FindCheck(6 * turn) == false)
                        {
                            AddPossibleMove(x - 1, y + 1, x, y);
                        }
                        chessboard = chessboard_backup;*/
                        AddPossibleMove(x - 1, y + 1, x, y);
                    }
                    if ((x == 6) && (chessboard[x - 2, y].exist == false) && (chessboard[x - 1, y].exist == false))
                    {/*
                        ChessPiece[,] chessboard_backup = new ChessPiece[8, 8];
                        chessboard_backup = chessboard;
                        MakeMove(x, y, x - 2, y);
                        if (FindCheck(6 * turn) == false)
                        {
                            AddPossibleMove(x - 2, y, x, y);
                        }
                        chessboard = chessboard_backup;*/
                        AddPossibleMove(x - 2, y, x, y);
                    }
                    break;
                case (-1)://cierny pesiak
                    if ((IsInChessBoard(x + 1, y) == true) && (chessboard[x + 1, y].exist == false))
                    {
                        /*
                        ChessPiece[,] chessboard_backup = new ChessPiece[8, 8];
                        chessboard_backup = chessboard;
                        MakeMove(x, y, x + 1, y);
                        if (FindCheck(6 * turn) == false)
                        {
                            AddPossibleMove(x + 1, y, x, y);
                        }
                        chessboard = chessboard_backup;*/
                        AddPossibleMove(x + 1, y, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y + 1) == true) &&
                        (chessboard[x + 1, y + 1].exist == true) && (chessboard[x + 1, y + 1].id > 0))
                    {/*
                        ChessPiece[,] chessboard_backup = new ChessPiece[8, 8];
                        chessboard_backup = chessboard;
                        MakeMove(x, y, x + 1, y + 1);
                        if (FindCheck(6 * turn) == false)
                        {
                            AddPossibleMove(x + 1, y + 1, x, y);
                        }
                        chessboard = chessboard_backup;*/
                        AddPossibleMove(x + 1, y + 1, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y - 1) == true) &&
                        (chessboard[x + 1, y - 1].exist == true) && (chessboard[x + 1, y - 1].id > 0))
                    {/*
                        ChessPiece[,] chessboard_backup = new ChessPiece[8, 8];
                        chessboard_backup = chessboard;
                        MakeMove(x, y, x + 1, y - 1);
                        if (FindCheck(6 * turn) == false)
                        {
                            AddPossibleMove(x + 1, y - 1, x, y);
                        }
                        chessboard = chessboard_backup;*/
                        AddPossibleMove(x + 1, y - 1, x, y);
                    }
                    if ((x == 1) && (chessboard[x + 2, y].exist == false) && (chessboard[x + 1, y].exist == false))
                    {/*
                        ChessPiece[,] chessboard_backup = new ChessPiece[8, 8];
                        chessboard_backup = chessboard;
                        MakeMove(x, y, x + 2, y);
                        if (FindCheck(6 * turn) == false)
                        {
                            AddPossibleMove(x + 2, y, x, y);
                        }
                        chessboard = chessboard_backup;
                        */
                        AddPossibleMove(x + 2, y, x, y);
                    }
                    break;
                case 2://veza
                case -2:
                    int i = 1;
                    while  ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].exist == false) )
                    {
                        AddPossibleMove(x + i, y, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x + i, y, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].exist == false))
                    {
                        AddPossibleMove(x, y + i, x ,y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].exist == false))
                    {
                        AddPossibleMove(x, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].exist == false))
                    {
                        AddPossibleMove(x - i, y, x , y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x - i, y, x ,y);
                    }
                    break;
                case 3: //jazdec
                case -3:
                    if ((IsInChessBoard(x - 2, y - 1) == true))
                    {
                        if (((chessboard[x - 2, y - 1].id * chessboard[x, y].id < 0) 
                            && (chessboard[x - 2, y - 1].exist == true))
                          || (chessboard[x - 2, y - 1].exist == false))
                        {
                            AddPossibleMove(x - 2, y - 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 2, y - 1) == true))
                    {
                        if (((chessboard[x + 2, y - 1].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 2, y - 1].exist == true))
                          || (chessboard[x + 2, y - 1].exist == false))
                        {
                            AddPossibleMove(x + 2, y - 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x - 2, y + 1) == true))
                    {
                        if (((chessboard[x - 2, y + 1].id * chessboard[x, y].id < 0)
                            && (chessboard[x - 2, y + 1].exist == true))
                          || (chessboard[x - 2, y + 1].exist == false))
                        {
                            AddPossibleMove(x - 2, y + 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 2, y + 1) == true))
                    {
                        if (((chessboard[x + 2, y + 1].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 2, y + 1].exist == true))
                          || (chessboard[x + 2, y + 1].exist == false))
                        {
                            AddPossibleMove(x + 2, y + 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x - 1, y - 2) == true))
                    {
                        if (((chessboard[x - 1, y - 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x - 1, y - 2].exist == true))
                          || (chessboard[x - 1, y - 2].exist == false))
                        {
                            AddPossibleMove(x - 1, y - 2, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 1, y - 2) == true))
                    {
                        if (((chessboard[x + 1, y - 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 1, y - 2].exist == true))
                          || (chessboard[x + 1, y - 2].exist == false))
                        {
                            AddPossibleMove(x + 1, y - 2, x, y);
                        }
                    }
                    if ((IsInChessBoard(x - 1, y + 2) == true))
                    {
                        if (((chessboard[x - 1, y + 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x - 1, y + 2].exist == true))
                          || (chessboard[x - 1, y + 2].exist == false))
                        {
                            AddPossibleMove(x - 1, y + 2, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 1, y + 2) == true))
                    {
                        if (((chessboard[x + 1, y + 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 1, y + 2].exist == true))
                          || (chessboard[x + 1, y + 2].exist == false))
                        {
                            AddPossibleMove(x + 1, y + 2, x, y);
                        }
                    }
                    break;
                case 4:
                case -4:
                    i = 1;
                    while ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].exist == false))
                    {
                        AddPossibleMove(x + i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x + i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].exist == false))
                    {
                        AddPossibleMove(x - i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x - i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].exist == false))
                    {
                        AddPossibleMove(x - i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x - i, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].exist == false))
                    {
                        AddPossibleMove(x + i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x + i, y - i, x, y);
                    }
                   
                    break;
                case 5:
                case -5:
                    i = 1;
                    while ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].exist == false))
                    {
                        AddPossibleMove(x + i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x + i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].exist == false))
                    {
                        AddPossibleMove(x - i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x - i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].exist == false))
                    {
                        AddPossibleMove(x - i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x - i, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].exist == false))
                    {
                        AddPossibleMove(x + i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x + i, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].exist == false))
                    {
                        AddPossibleMove(x + i, y, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x + i, y, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].exist == false))
                    {
                        AddPossibleMove(x, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].exist == false))
                    {
                        AddPossibleMove(x, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].exist == false))
                    {
                        AddPossibleMove(x - i, y, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(x - i, y, x, y);
                    }
                    break;
                case 6:
                case -6:
                    if ((IsInChessBoard(x - 1, y) == true) 
                        && ((chessboard[x - 1, y].exist == false)
                         ||(chessboard[x -1 , y].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x - 1, y, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y) == true)
                        && ((chessboard[x + 1, y].exist == false)
                         || (chessboard[x + 1, y].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x + 1, y, x, y);
                    }
                    if ((IsInChessBoard(x - 1, y - 1) == true)
                        && ((chessboard[x - 1, y - 1].exist == false)
                         || (chessboard[x - 1, y - 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x - 1, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y - 1) == true)
                        && ((chessboard[x + 1, y - 1].exist == false)
                         || (chessboard[x + 1, y - 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x + 1, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x, y - 1) == true)
                        && ((chessboard[x, y - 1].exist == false)
                         || (chessboard[x, y - 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x - 1, y + 1) == true)
                        && ((chessboard[x - 1, y + 1].exist == false)
                         || (chessboard[x - 1, y + 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x - 1, y + 1, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y + 1) == true)
                        && ((chessboard[x + 1, y + 1].exist == false)
                         || (chessboard[x + 1, y + 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x + 1, y + 1, x, y);
                    }
                    if ((IsInChessBoard(x, y + 1) == true)
                        && ((chessboard[x, y + 1].exist == false)
                         || (chessboard[x, y + 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(x, y + 1, x, y);
                    }
                    AddSwapMove(x, y, chessboard[x, y].id);
                    break;
            }
        }
        public void AddSwapMove(int king_x, int king_y, int king_id)
        {
            if (king_x == 0)
            {
                if ((chessboard[king_x, 5].exist == false) && (chessboard[king_x, 6].exist == false)
                    && (castling.b_king_not_moved == true) && (castling.b_r_rook_not_moved == true))
                {
                    Coordinates coord = new Coordinates();
                    coord.x = king_x;
                    coord.y = 6;
                    chessboard[king_x, king_y].possible_moves.Add(coord);

                }
                if ((chessboard[king_x, 1].exist == false) && (chessboard[king_x, 2].exist == false)
                    && (chessboard[king_x, 3].exist == false) && (castling.b_king_not_moved == true)
                    && (castling.b_l_rook_not_moved == true))
                {
                    Coordinates coord = new Coordinates();
                    coord.x = king_x;
                    coord.y = 2;
                    chessboard[king_x, king_y].possible_moves.Add(coord);

                }
            }
            if (king_x == 7)
            {
                if ((chessboard[king_x, 5].exist == false) && (chessboard[king_x, 6].exist == false)
                    && (castling.w_king_not_moved == true) && (castling.w_r_rook_not_moved == true))
                {
                    Coordinates coord = new Coordinates();
                    coord.x = king_x;
                    coord.y = 6;
                    chessboard[king_x, king_y].possible_moves.Add(coord);

                }
                if ((chessboard[king_x, 1].exist == false) && (chessboard[king_x, 2].exist == false)
                    && (chessboard[king_x, 3].exist == false) && (castling.w_king_not_moved == true) 
                    && (castling.w_l_rook_not_moved == true))
                {
                    Coordinates coord = new Coordinates();
                    coord.x = king_x;
                    coord.y = 2;
                    chessboard[king_x, king_y].possible_moves.Add(coord);

                }
            }

        }
        public void PicClicked(object sender, EventArgs e)
        {
            //if turn == true tj je player na tahu - iba pri sigleplayer
            /*Color black =  Color.FormArgb(180, 0, 0, 0);
            Color white = new Color.FromArgb(0, 0, 0, 0);
            Color blue = new Color.FromArgb(100, 0, 0, 255);*/
            MyPictureBox pic_click = sender as MyPictureBox;
            if ((first_click == true) || (pic_click.BackColor != Color.FromArgb(100, 0, 0, 255)))
            {
                foreach (MyPictureBox pbox in picture_boxes)
                {
                    if (((pbox.coordinate_x % 2 == 0) && (pbox.coordinate_y % 2 == 1)) //restore backcolor po kliku
                        || ((pbox.coordinate_x % 2 == 1) && (pbox.coordinate_y % 2 == 0)))
                    {
                        pbox.BackColor = Color.FromArgb(180, 0, 0, 0);
                    }
                    else
                    {
                        pbox.BackColor = Color.FromArgb(0, 0, 0, 0); 
                    }
                }
            }
            UpdatePossibleMoves(pic_click.coordinate_x,pic_click.coordinate_y);
            DeleteCheckMoves(pic_click.coordinate_x, pic_click.coordinate_y);
            //chessboard je pole figurok, FIRST CLICK
            if ((chessboard[pic_click.coordinate_x, pic_click.coordinate_y].exist == true)
                &&(first_click == true)
                &&(chessboard[pic_click.coordinate_x, pic_click.coordinate_y].id * turn > 0))
            {
                foreach (Coordinates coord in chessboard[pic_click.coordinate_x,pic_click.coordinate_y].possible_moves)
                {
                    
                    foreach (MyPictureBox pbox in picture_boxes) //zmen pozadie kam mozno tahat
                    {
                        if ((pbox.coordinate_x == coord.x) && (pbox.coordinate_y == coord.y))
                        {
                            pbox.BackColor = Color.FromArgb(100, 0, 0, 255); ;
                        }
                    }
                    
                }
                from.x = pic_click.coordinate_x;
                from.y = pic_click.coordinate_y;
                first_click = false;
            }
            if (first_click == false)  //SECOND CLICK
            {
                if (chessboard[pic_click.coordinate_x, pic_click.coordinate_y].id * turn > 0 ) //znovu klikol na svojho
                {
                    {
                        foreach (Coordinates coord in chessboard[pic_click.coordinate_x, pic_click.coordinate_y].possible_moves)
                        {
                            foreach (MyPictureBox pbox in picture_boxes) //zmen pozadie kam mozno tahat
                            {
                                if ((pbox.coordinate_x == coord.x) && (pbox.coordinate_y == coord.y))
                                {
                                    pbox.BackColor = Color.FromArgb(100, 0, 0, 255); ;
                                }
                            }
                        }
                        from.x = pic_click.coordinate_x;
                        from.y = pic_click.coordinate_y;
                        first_click = false;
                    }
                }
                if (pic_click.BackColor == Color.FromArgb(100, 0, 0, 255))//klikol na dostupne policko
                {

                    MakeMove(from.x, from.y,pic_click.coordinate_x,pic_click.coordinate_y);
                    foreach (MyPictureBox pbox in picture_boxes)
                    {
                        if (((pbox.coordinate_x % 2 == 0) && (pbox.coordinate_y % 2 == 1)) //restore backcolor po kliku
                            || ((pbox.coordinate_x % 2 == 1) && (pbox.coordinate_y % 2 == 0)))
                        {
                            pbox.BackColor = Color.FromArgb(180, 0, 0, 0);
                        }
                        else
                        {
                            pbox.BackColor = Color.FromArgb(0, 0, 0, 0); 
                        }
                    }
                    if (IsPawnToTransform(pic_click.coordinate_x, pic_click.coordinate_y) == true)
                    {
                        TransformPawn(pic_click.coordinate_x, pic_click.coordinate_y);
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if ((chessboard[i, j].id != 0 ) && (chessboard[i, j].exist == true))
                            {
                                UpdatePossibleMoves(i, j);
                                //DeleteCheckMoves(i, j);
                            }
                            
                        }
                    }
                    //------------------koniec tahu--------------------------------------------
                    turn = -1 * turn;
                    Redraw();
                    first_click = true;
                    if (FindCheck(6*turn) == true)
                    {
                        const string message = "You are in check";
                        MessageBox.Show(message,"Check", MessageBoxButtons.OK);
                    }
                }
            }

            
        }
        public void MakeMove(int from_x, int from_y, int to_x, int to_y)
        {
            //rosada
            if (((chessboard[from_x, from_y].id == 6) || (chessboard[from_x, from_y].id == -6))
                && (from_y == 4) && ((to_y == 6) || (to_y == 2)))
            {
                //posunutie vezi
                if (to_y == 6)
                {

                    chessboard[from_x, 5].exist = true;
                    chessboard[from_x, 5].id = chessboard[from_x, 7].id;
                    chessboard[from_x, 7].exist = false;
                    chessboard[from_x, 7].id = 0;
                }
                if  (to_y == 2)
                {
                    chessboard[from_x, 3].exist = true;
                    chessboard[from_x, 3].id = chessboard[from_x, 0].id;
                    chessboard[from_x, 0].exist = false;
                    chessboard[from_x, 0].id = 0;
                }
            }
            if (chessboard[from_x, from_y].id == 6)
            {
                castling.w_king_not_moved = false;    
            }
            if (chessboard[from_x, from_y].id == -6)
            {
                castling.b_king_not_moved = false;
            }
                //////////////////////////////////////////////////////////koniec rosady
            chessboard[to_x, to_y].exist = true;                    //pridanie cielu
            chessboard[to_x, to_y].id = chessboard[from_x, from_y].id;     //premiesnenie
            chessboard[from_x, from_y].exist = false;
            chessboard[from_x, from_y].id = 0;  
            
            
            
        }
        public void Redraw()
        {
            foreach (MyPictureBox pbox in picture_boxes)
            {
                if ((chessboard[pbox.coordinate_x, pbox.coordinate_y].id != 0)
                    && (chessboard[pbox.coordinate_x, pbox.coordinate_y].exist == true))
                {
                    pbox.Image = Image.FromFile(@"..\..\icons\" +
                        Convert.ToString(chessboard[pbox.coordinate_x, pbox.coordinate_y].id) + ".png");

                }
                else
                    pbox.Image = null;
            }
        }
        public Coordinates FindKingPosition(int king_id)
        {
            Coordinates king_position = new Coordinates();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (chessboard[i,j].id == king_id)
                    {
                        king_position.x = i;
                        king_position.y = j;
                        return (king_position);
                    }
                }
            }
            return (king_position);
        }
        public bool FindCheck(int king_id)
        {
            Coordinates king_position = FindKingPosition(king_id);
            for (int i = 0; i < 8; i++)
    
        {
                for (int j = 0; j < 8; j++)
                {
                    if ((king_id * chessboard[i,j].id < 0) && (chessboard[i,j].exist == true))
                    {
                        //ClearPossibleMoves(i, j);
                        //UpdatePossibleMoves(i, j);
                        foreach (Coordinates coord in chessboard[i, j].possible_moves)
                        {
                            if ((coord.x == king_position.x) && (coord.y == king_position.y))
                            {
                                return (true);
                            }
                        }
                        
                    }
                }
            }
            return (false);
        }
        public bool IsPawnToTransform(int x, int y)
        {
            if ((chessboard[x, y].id  == 1) && (chessboard[x, y].exist == true) && (x == 0))
            {
                return (true);
            }
            if ((chessboard[x, y].id == -1) && (chessboard[x, y].exist == true) && (x == 7))
            {
                return (true);
            }
            else
                return (false);
        }
        public void TransformPawn(int x, int y)
        {
            for (int i = 0; i < 4; i++)
            {
                MyPictureBox pawn_transform_opt = new MyPictureBox();
                int transform_id = (i + 2) * turn;
                pawn_transform_opt.trans_id = transform_id;
                pawn_transform_opt.coordinate_x = x;
                pawn_transform_opt.coordinate_y = y;
                pawn_transform_opt.Parent = this;
                pawn_transform_opt.Height = 100;
                pawn_transform_opt.Width = 100;
                pawn_transform_opt.Visible = true;
                pawn_transform_opt.Top = 300;
                pawn_transform_opt.Left = i * 100 + 700;
                pawn_transform_opt.SizeMode = PictureBoxSizeMode.StretchImage;
                pawn_transform_opt.BorderStyle = BorderStyle.FixedSingle;
                pawn_transform_opt.Click += TransClicked;
                pawn_transform_opt.Image = Image.FromFile(@"..\..\icons\" + Convert.ToString(transform_id) + ".png");
                trans_list.Add(pawn_transform_opt);
            }
        }
        public void TransClicked(object sender, EventArgs e)
        {
            MyPictureBox clicked = sender as MyPictureBox;
            chessboard[clicked.coordinate_x, clicked.coordinate_y].id = clicked.trans_id;
            Redraw();
            foreach (MyPictureBox trans_box in trans_list)
            {
                trans_box.Dispose();
            }
            UpdatePossibleMoves(clicked.coordinate_x, clicked.coordinate_y);
            if (FindCheck(6 * turn) == true)
            {

                const string message = "You are in check";
                MessageBox.Show(message, "Check", MessageBoxButtons.OK);
            }

        }
        public TheChess()
        {
            InitializeComponent();
            CreatePBoxes();
            FillArray();
            Redraw();

        }

        private void TheChess_Load(object sender, EventArgs e)
        {

        }
    }
}
