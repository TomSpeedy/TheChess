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
    public class Move
    {
        public int from_x;
        public int from_y;
        public int to_x;
        public int to_y;
    }
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
    public class AI
    {
        public Move move = new Move();
        public ChessPiece[,] game_state = new ChessPiece[8, 8];
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
    }
    public static class EvalData
    {
        public static void FillFromText(ref int[,,] array)
        {
            bool swap = false;
            for (int h = 0; h < 12; h++)
            {
                int id = h;
                
                if (swap)
                {
                    id = -(h/2 + 1);
                }
                else
                {
                    id = (h/2 + 1);
                }
                System.IO.StreamReader stream = new System.IO.StreamReader(@"..\..\eval_board_text\" + Convert.ToString(id)+ ".txt");
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            int a = stream.Read();
                            bool negative = false;
                            int value = 0;
                            while ((a != ',') && (a != ' '))
                            {
                                if (a == '-')
                                {
                                    negative = true;
                                    a = stream.Read();
                                }
                                else
                                {
                                    a -= 48;
                                    value *= 10;
                                    value += a;
                                    a = stream.Read();
                                }
                            }
                            if (negative)
                            {
                                array[h, i, j] = -value;
                            }
                            else
                            {
                                array[h, i, j] = value;
                            }
                        }
                        stream.ReadLine();
                    }
                }
                swap = !swap;
            }

        }
    }
    public static class Judge
    {
        public static int[,,] position_eval = new int[12, 8, 8];
        //public static List<ChessPiece>
        public static int Evaluate(ChessPiece[,] board, int ai_turn,int number_of_moves, int turn)
        {
            int ai_points = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((board[i, j].id != 0) && (board[i, j].exist == true))
                    {
                        int material_points = 0;
                        switch (board[i, j].id)
                        {
                            case 1:
                            case -1:
                                material_points = 100;
                                break;
                            case 2:
                            case -2:
                                material_points = 500;
                                break;
                            case 3:
                            case -3:
                                material_points = 320;
                                break;
                            case 4:
                            case -4:
                                material_points = 330;
                                break;
                            case 5:
                            case -5:
                                material_points = 900;
                                break;
                            default:
                                material_points = 0;
                                break;
                        }
                            int position_points = 0;
                            int index = 0;
                        if (board[i, j].id > 0)
                            {
                                index = (2 * board[i, j].id) - 2;
                            }
                        else
                            {
                                index = (-2 * board[i, j].id) - 1;
                            }
                            position_points = position_eval[index, i, j];

                        if (ai_turn == 1)
                        {
                            if (board[i, j].id > 0)
                            {
                                ai_points += material_points;
                                ai_points += position_points;

                            }
                            else
                            {
                                ai_points -= material_points;
                                ai_points -= position_points;
                            }

                        }
                        else
                        {
                            if (board[i, j].id < 0)
                            {
                                ai_points += material_points;
                                ai_points += position_points;
                            }
                            else
                            {
                                ai_points -= material_points;
                                ai_points -= position_points;
                            }
                        }
                        
                        if (number_of_moves == 0)
                        { 
                            if (turn == ai_turn)
                            {
                                ai_points += -15000;
                            }
                            else
                            {
                                ai_points += 15000;
                            }
                        }
                    }
                }
            }
            return (ai_points);
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
        AI ai = new AI();
        bool single_player = true;
        Timer timer = new Timer();
        List<Coordinates> to_delete = new List<Coordinates>();
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
        public void ClearPossibleMoves(ref ChessPiece[,] chessboard, int x, int y)
        {
            chessboard[x, y].possible_moves.Clear();
        }
        public void AddPossibleMove(ref ChessPiece[,] chessboard, int a, int b, int base_x, int base_y)
        {
            Coordinates cord = new Coordinates();
            cord.x = a;
            cord.y = b;
            chessboard[base_x, base_y].possible_moves.Add(cord);
        }
        public void DeleteCheckMoves(ref ChessPiece[,] chessboard, int x, int y)
        {
            to_delete.Clear();
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
                MakeMove(ref chessboard, x, y, coord.x, coord.y);
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if ((chessboard[i, j].id * turn < 0) && (chessboard[i, j].exist == true))
                        {
                            UpdatePossibleMoves(ref chessboard, i, j);
                        }
                    }
                }
               // UpdatePossibleMoves(i, j);//iba pre opacnu farbu
                if (FindCheck(ref chessboard, 6 *turn) == true)
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
        public void UpdatePossibleMoves(ref ChessPiece[,] chessboard, int x, int y)
        {
            //spocitaj moves pre figurku na suradniciach x,y
            ClearPossibleMoves(ref chessboard, x, y);
            switch (chessboard[x, y].id)
            {
                case 1: //biely pesiak
                    if ((IsInChessBoard(x - 1, y) == true) && (chessboard[x - 1, y].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - 1, y, x, y);

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
                        AddPossibleMove(ref chessboard, x - 1, y - 1, x, y);
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
                        AddPossibleMove(ref chessboard, x - 1, y + 1, x, y);
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
                        AddPossibleMove(ref chessboard, x - 2, y, x, y);
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
                        AddPossibleMove(ref chessboard, x + 1, y, x, y);
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
                        AddPossibleMove(ref chessboard, x + 1, y + 1, x, y);
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
                        AddPossibleMove(ref chessboard, x + 1, y - 1, x, y);
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
                        AddPossibleMove(ref chessboard, x + 2, y, x, y);
                    }
                    break;
                case 2://veza
                case -2:
                    int i = 1;
                    while  ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].exist == false) )
                    {
                        AddPossibleMove(ref chessboard, x + i, y, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x + i, y, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x, y + i, x ,y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - i, y, x , y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x - i, y, x ,y);
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
                            AddPossibleMove(ref chessboard, x - 2, y - 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 2, y - 1) == true))
                    {
                        if (((chessboard[x + 2, y - 1].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 2, y - 1].exist == true))
                          || (chessboard[x + 2, y - 1].exist == false))
                        {
                            AddPossibleMove(ref chessboard, x + 2, y - 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x - 2, y + 1) == true))
                    {
                        if (((chessboard[x - 2, y + 1].id * chessboard[x, y].id < 0)
                            && (chessboard[x - 2, y + 1].exist == true))
                          || (chessboard[x - 2, y + 1].exist == false))
                        {
                            AddPossibleMove(ref chessboard, x - 2, y + 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 2, y + 1) == true))
                    {
                        if (((chessboard[x + 2, y + 1].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 2, y + 1].exist == true))
                          || (chessboard[x + 2, y + 1].exist == false))
                        {
                            AddPossibleMove(ref chessboard, x + 2, y + 1, x, y);
                        }
                    }
                    if ((IsInChessBoard(x - 1, y - 2) == true))
                    {
                        if (((chessboard[x - 1, y - 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x - 1, y - 2].exist == true))
                          || (chessboard[x - 1, y - 2].exist == false))
                        {
                            AddPossibleMove(ref chessboard, x - 1, y - 2, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 1, y - 2) == true))
                    {
                        if (((chessboard[x + 1, y - 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 1, y - 2].exist == true))
                          || (chessboard[x + 1, y - 2].exist == false))
                        {
                            AddPossibleMove(ref chessboard, x + 1, y - 2, x, y);
                        }
                    }
                    if ((IsInChessBoard(x - 1, y + 2) == true))
                    {
                        if (((chessboard[x - 1, y + 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x - 1, y + 2].exist == true))
                          || (chessboard[x - 1, y + 2].exist == false))
                        {
                            AddPossibleMove(ref chessboard, x - 1, y + 2, x, y);
                        }
                    }
                    if ((IsInChessBoard(x + 1, y + 2) == true))
                    {
                        if (((chessboard[x + 1, y + 2].id * chessboard[x, y].id < 0)
                            && (chessboard[x + 1, y + 2].exist == true))
                          || (chessboard[x + 1, y + 2].exist == false))
                        {
                            AddPossibleMove(ref chessboard, x + 1, y + 2, x, y);
                        }
                    }
                    break;
                case 4:
                case -4:
                    i = 1;
                    while ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x + i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x + i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x - i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x - i, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x + i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x + i, y - i, x, y);
                    }
                   
                    break;
                case 5:
                case -5:
                    i = 1;
                    while ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x + i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y + i) == true) && (chessboard[x + i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x + i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - i, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y + i) == true) && (chessboard[x - i, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x - i, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y - i) == true) && (chessboard[x - i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x - i, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x + i, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y - i) == true) && (chessboard[x + i, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x + i, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x + i, y, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x + i, y) == true) && (chessboard[x + i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x + i, y, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x, y + i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y + i) == true) && (chessboard[x, y + i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x, y + i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x, y - i, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x, y - i) == true) && (chessboard[x, y - i].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x, y - i, x, y);
                    }
                    i = 1;
                    while ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - i, y, x, y);
                        i++;
                    }
                    if ((IsInChessBoard(x - i, y) == true) && (chessboard[x - i, y].id * chessboard[x, y].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x - i, y, x, y);
                    }
                    break;
                case 6:
                case -6:
                    if ((IsInChessBoard(x - 1, y) == true) 
                        && ((chessboard[x - 1, y].exist == false)
                         ||(chessboard[x -1 , y].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x - 1, y, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y) == true)
                        && ((chessboard[x + 1, y].exist == false)
                         || (chessboard[x + 1, y].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x + 1, y, x, y);
                    }
                    if ((IsInChessBoard(x - 1, y - 1) == true)
                        && ((chessboard[x - 1, y - 1].exist == false)
                         || (chessboard[x - 1, y - 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x - 1, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y - 1) == true)
                        && ((chessboard[x + 1, y - 1].exist == false)
                         || (chessboard[x + 1, y - 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x + 1, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x, y - 1) == true)
                        && ((chessboard[x, y - 1].exist == false)
                         || (chessboard[x, y - 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x - 1, y + 1) == true)
                        && ((chessboard[x - 1, y + 1].exist == false)
                         || (chessboard[x - 1, y + 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x - 1, y + 1, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y + 1) == true)
                        && ((chessboard[x + 1, y + 1].exist == false)
                         || (chessboard[x + 1, y + 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x + 1, y + 1, x, y);
                    }
                    if ((IsInChessBoard(x, y + 1) == true)
                        && ((chessboard[x, y + 1].exist == false)
                         || (chessboard[x, y + 1].id * chessboard[x, y].id < 0)))
                    {
                        AddPossibleMove(ref chessboard, x, y + 1, x, y);
                    }
                    AddSwapMove(ref chessboard, x, y, chessboard[x, y].id);
                    break;
            }
        }
        public void AddSwapMove(ref ChessPiece[,] chessboard, int king_x, int king_y, int king_id)
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
            if ((single_player == false) || (turn == player_turn))
            {
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
                UpdatePossibleMoves(ref chessboard, pic_click.coordinate_x, pic_click.coordinate_y);
                DeleteCheckMoves(ref chessboard, pic_click.coordinate_x, pic_click.coordinate_y);
                //chessboard je pole figurok, FIRST CLICK
                if ((chessboard[pic_click.coordinate_x, pic_click.coordinate_y].exist == true)
                    && (first_click == true)
                    && (chessboard[pic_click.coordinate_x, pic_click.coordinate_y].id * turn > 0))
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
                if (first_click == false)  //SECOND CLICK
                {
                    if (chessboard[pic_click.coordinate_x, pic_click.coordinate_y].id * turn > 0) //znovu klikol na svojho
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

                        MakeMove(ref chessboard, from.x, from.y, pic_click.coordinate_x, pic_click.coordinate_y);
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
                        if (IsPawnToTransform(ref chessboard, pic_click.coordinate_x, pic_click.coordinate_y) == true)
                        {
                            TransformPawn(pic_click.coordinate_x, pic_click.coordinate_y);
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                if ((chessboard[i, j].id * turn > 0) && (chessboard[i, j].exist == true))
                                {
                                    UpdatePossibleMoves(ref chessboard, i, j);
                                    //DeleteCheckMoves(i, j);
                                }

                            }
                        }
                        //------------------koniec tahu--------------------------------------------
                        turn = -1 * turn;
                        Redraw();
                        first_click = true;
                        if ((FindCheck(ref chessboard, 6 * turn) == true) && (single_player == false)
                            && (CountMoves(chessboard) != 0))
                        {
                            const string message = "You are in check";
                            MessageBox.Show(message, "Check", MessageBoxButtons.OK);
                        }
                        if ((FindCheck(ref chessboard, 6 * turn) == true)
                            && (CountMoves(chessboard) == 0))
                        {
                            if (turn == -1)
                            {
                                const string message = "White player wins, thanks for playing ;)";
                                MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                            }
                            else
                            {
                                const string message = "Black player wins, thanks for playing ;)";
                                MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                            }

                        }
                        else
                        {
                            timer.Start();
                        }
                    }
                }
            }
            
        }
        public void MakeMove(ref ChessPiece[,] board, int from_x, int from_y, int to_x, int to_y)
        {
            //rosada
            if (((board[from_x, from_y].id == 6) || (board[from_x, from_y].id == -6))
                && (from_y == 4) && ((to_y == 6) || (to_y == 2)))
            {
                //posunutie vezi
                if (to_y == 6)
                {

                    board[from_x, 5].exist = true;
                    board[from_x, 5].id = board[from_x, 7].id;
                    board[from_x, 7].exist = false;
                    board[from_x, 7].id = 0;
                }
                if  (to_y == 2)
                {
                    board[from_x, 3].exist = true;
                    board[from_x, 3].id = board[from_x, 0].id;
                    board[from_x, 0].exist = false;
                    board[from_x, 0].id = 0;
                }
            }
            if (board[from_x, from_y].id == 6)
            {
                castling.w_king_not_moved = false;    
            }
            if (board[from_x, from_y].id == -6)
            {
                castling.b_king_not_moved = false;
            }
            if ((board[from_x, from_y].id == -2) &&(from_x == 0) && (from_y == 0))
            {
                castling.b_l_rook_not_moved = false;
            }
            if ((board[from_x, from_y].id == -2) && (from_x == 0) && (from_y == 7))
            {
                castling.b_r_rook_not_moved = false;
            }
            if ((board[from_x, from_y].id == 2) && (from_x == 7) && (from_y == 0))
            {
                castling.w_l_rook_not_moved = false;
            }
            if ((board[from_x, from_y].id == 2) && (from_x == 7) && (from_y == 7))
            {
                castling.w_r_rook_not_moved = false;
            }
            //////////////////////////////////////////////////////////koniec rosady
            board[to_x, to_y].exist = true;                    //pridanie cielu
            board[to_x, to_y].id = board[from_x, from_y].id;     //premiesnenie
            board[from_x, from_y].exist = false;
            board[from_x, from_y].id = 0;  
            
            
            
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
        public Coordinates FindKingPosition(ref ChessPiece[,] chessboard, int king_id)
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
        public bool FindCheck(ref ChessPiece[,] chessboard, int king_id)
        {
            Coordinates king_position = FindKingPosition(ref chessboard, king_id);
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
        public bool IsPawnToTransform(ref ChessPiece[,] chessboard, int x, int y)
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
            UpdatePossibleMoves(ref chessboard, clicked.coordinate_x, clicked.coordinate_y);
            if (FindCheck(ref chessboard, 6 * turn) == true)
            {

                const string message = "You are in check";
                MessageBox.Show(message, "Check", MessageBoxButtons.OK);
            }

        }
        public void PlaySingle(int ai_turn)
        {
            
            ai.GetChessboardState(chessboard);
            FindBestMove(ref ai.game_state, 4);
            MakeMove(ref chessboard, ai.move.from_x, ai.move.from_y, ai.move.to_x, ai.move.to_y);
            if (IsPawnToTransform(ref chessboard, ai.move.to_x, ai.move.to_y) == true)
            {
                chessboard[ai.move.to_x, ai.move.to_y].id = 5 * chessboard[ai.move.to_x, ai.move.to_y].id;
                    //vymena za damu
            }
            Redraw();
            turn = turn * -1;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((chessboard[i, j].id * turn < 0) && (chessboard[i, j].exist))
                    {
                        UpdatePossibleMoves(ref chessboard, i, j);
                    }
                }
            }
            //UpdatePossibleMoves(chessboard, i, j)
            if ((FindCheck(ref chessboard, 6 * turn) == true)
                            && (CountMoves(chessboard) == 0))
            {
                if (turn == -1)
                {
                    const string message = "White player wins, thanks for playing ;)";
                    MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                }
                else
                {
                    const string message = "Black player wins, thanks for playing ;)";
                    MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                }
            }
            


        }
        public void PlayMulti(int random)
        {

        }
        public int CountMoves(ChessPiece[,] chess_state)
        {
            int sum = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((chess_state[i, j].id * turn > 0) && (chess_state[i, j].exist == true))
                    {
                        UpdatePossibleMoves(ref chess_state, i, j);
                        DeleteCheckMoves(ref chess_state, i, j);
                        sum = sum + chess_state[i, j].possible_moves.Count;
                    }
                }
            }
            return (sum);
        }
        public void FindBestMove(ref ChessPiece[,] game_state, int final_depth)
        {
            int max_value = Max(ref game_state, 0, final_depth, -35000, 35000);
        }
        public int Max(ref ChessPiece[,] state, int depth, int final_depth, int alpha, int beta)
        {
            int turn_backup = turn;
            int max = -20000;
            int number_of_moves = CountMoves(state);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((state[i, j].id * turn > 0) && (state[i, j].exist == true))
                    {
                        if    ((depth != final_depth) && (number_of_moves != 0))/*nie je sach mat*/

                        {
                            UpdatePossibleMoves(ref state, i, j);
                            DeleteCheckMoves(ref state, i, j);
                            List<Coordinates> possible_moves = new List<Coordinates>();
                            foreach (Coordinates possible_move in state[i, j].possible_moves)
                            {
                                Coordinates pos_move = new Coordinates();
                                pos_move.x = possible_move.x;
                                pos_move.y = possible_move.y;
                                possible_moves.Add(pos_move);
                            }
                            

                           
                            // len zdruzi pointre, treba priradit zrejme postupne
                            foreach (Coordinates possible_move in possible_moves)
                            {
                                if (alpha < beta)
                                {



                                    bool w_l_r_backup = castling.w_l_rook_not_moved;
                                    bool w_r_r_backup = castling.w_r_rook_not_moved;
                                    bool b_r_r_backup = castling.b_r_rook_not_moved;
                                    bool b_l_r_backup = castling.b_l_rook_not_moved;
                                    bool w_k_backup = castling.w_king_not_moved;
                                    bool b_k_backup = castling.b_king_not_moved;
                                    bool exist_backup = state[possible_move.x, possible_move.y].exist;
                                    int id_backup = state[possible_move.x, possible_move.y].id;
                                    MakeMove(ref state, i, j, possible_move.x, possible_move.y);
                                    turn = -1 * turn;

                                    int value = Min(ref state, depth + 1, final_depth, alpha, beta);
                                    if (alpha < value)
                                    {
                                        alpha = value;
                                    }
                                    turn = turn_backup;
                                    if (value > max)
                                    {
                                        max = value;
                                        if (depth == 0)
                                        {
                                            ai.move.from_x = i;
                                            ai.move.from_y = j;
                                            ai.move.to_x = possible_move.x;
                                            ai.move.to_y = possible_move.y;

                                        }
                                    }

                                    state[i, j].id = state[possible_move.x, possible_move.y].id;
                                    state[i, j].exist = true;
                                    state[possible_move.x, possible_move.y].exist = exist_backup;
                                    state[possible_move.x, possible_move.y].id = id_backup;
                                    castling.w_l_rook_not_moved = w_l_r_backup;
                                    castling.w_r_rook_not_moved = w_r_r_backup;
                                    castling.b_r_rook_not_moved = b_r_r_backup;
                                    castling.b_l_rook_not_moved = b_l_r_backup;
                                    castling.w_king_not_moved = w_k_backup;
                                    castling.b_king_not_moved = b_k_backup;
                                }
                                    //return (max);
                            }
                        }           
                        else
                        {
                            int value = Judge.Evaluate(state, turn, number_of_moves, turn);
                            /*if (number_of_moves == 0)
                            {
                                value = -10000;
                            }*/
                            if (value > max)
                            {
                                max = value;
                            }

                        }
                    }
                        //ohodnot
                }
            }
            return (max);
        }
        public int Min(ref ChessPiece[,] state, int depth, int final_depth, int alpha, int beta)
        {
            int turn_backup = turn;
            int min = 20000;
            int number_of_moves = CountMoves(state);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((state[i, j].id * turn > 0) && (state[i, j].exist == true))
                    {
                        if ((depth != final_depth) && (number_of_moves != 0))/*nie je sach mat*/

                        {
                            UpdatePossibleMoves(ref state, i, j);
                            DeleteCheckMoves(ref state, i, j);
                            List<Coordinates> possible_moves = new List<Coordinates>();
                            foreach (Coordinates possible_move in state[i, j].possible_moves)
                            {
                                Coordinates pos_move = new Coordinates();
                                pos_move.x = possible_move.x;
                                pos_move.y = possible_move.y;
                                possible_moves.Add(pos_move);
                            }
                            foreach (Coordinates possible_move in possible_moves)
                            {if (alpha < beta)
                                {

                                    bool w_l_r_backup = castling.w_l_rook_not_moved;
                                    bool w_r_r_backup = castling.w_r_rook_not_moved;
                                    bool b_r_r_backup = castling.b_r_rook_not_moved;
                                    bool b_l_r_backup = castling.b_l_rook_not_moved;
                                    bool w_k_backup = castling.w_king_not_moved;
                                    bool b_k_backup = castling.b_king_not_moved;
                                    bool exist_backup = state[possible_move.x, possible_move.y].exist;
                                    int id_backup = state[possible_move.x, possible_move.y].id;
                                    MakeMove(ref state, i, j, possible_move.x, possible_move.y);
                                    turn = -1 * turn;

                                    int value = Max(ref state, depth + 1, final_depth, alpha, beta);
                                    if (beta > value)
                                    {
                                        beta = value;
                                    }
                                    turn = turn_backup;
                                    if (value < min)
                                    {
                                        min = value;
                                        if (depth == 0)
                                        {
                                            ai.move.from_x = i;
                                            ai.move.from_y = j;
                                            ai.move.to_x = possible_move.x;
                                            ai.move.to_y = possible_move.y;

                                        }
                                    }

                                    state[i, j].id = state[possible_move.x, possible_move.y].id;
                                    state[i, j].exist = true;
                                    state[possible_move.x, possible_move.y].exist = exist_backup;
                                    state[possible_move.x, possible_move.y].id = id_backup;
                                    castling.w_l_rook_not_moved = w_l_r_backup;
                                    castling.w_r_rook_not_moved = w_r_r_backup;
                                    castling.b_r_rook_not_moved = b_r_r_backup;
                                    castling.b_l_rook_not_moved = b_l_r_backup;
                                    castling.w_king_not_moved = w_k_backup;
                                    castling.b_king_not_moved = b_k_backup;
                                }
                                    //return (max);
                            }
                        }
                        else
                        {
                            int value = Judge.Evaluate(state, -1* turn, number_of_moves, turn);
                            if (number_of_moves == 0)
                            {
                                value = 10000;
                            }
                            if (value < min)
                            {
                                min = value;
                            }

                        }
                    }
                    //ohodnot
                }
            }
            return (min);
        }
        public TheChess()
        {
            InitializeComponent();
            timer.Interval = 200;
            timer.Tick += TimerTick;
            EvalData.FillFromText(ref Judge.position_eval);
            CreatePBoxes();
            FillArray();
            Redraw();
            player_turn = -1;
            PlaySingle(player_turn * -1);
        }
        public void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
            PlaySingle(turn);
            
            
        }
        private void TheChess_Load(object sender, EventArgs e)
        {

        }
    }
}
