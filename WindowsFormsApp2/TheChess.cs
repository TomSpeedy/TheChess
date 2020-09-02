using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*Zapoctovy program (Programovani II)
  Tomas Celko
  MFFUK, 2018/2019, LS
*/
namespace WindowsFormsApp2
{
    public static class Pat
    {

        public static string[,] buffer = new string[2, 5];
        public static string GetHash(ChessPiece[,] chessboard)
        {
            string str = "";
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    str += Convert.ToString(chessboard[i, j].id);
                }
            }
            return (str);
        }

        public static void AddToBuffer(ChessPiece[,] chessboard, int turn)
        {
            if (turn == 1)
            {
                for (int j = 0; j < 4; j++)
                {
                    buffer[0, 4 - j] = buffer[0, 3 - j];
                }
                buffer[0, 0] = GetHash(chessboard);
            }
            else
            {
                for (int j = 0; j < 4; j++)
                {
                    buffer[1, 4 - j] = buffer[1, 3 -j];
                }
                buffer[1, 0] = GetHash(chessboard);
            }
        }
        public static void InitBuffer()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    buffer[i, j] = Convert.ToString(i * 10 + j);
                }
            }
        }
        public static bool IsDraw(ChessPiece[,] chessboard,int turn)
        {
            if (turn == -1)
            {
                if ((buffer[0,0] == buffer[0,2]) && (buffer[0,4] == buffer[0, 2]))
                {
                    return (true);
                }
            }
            else
            {
                if ((buffer[1, 0] == buffer[1, 2]) && (buffer[1, 4] == buffer[1, 2]))
                {
                    return (true);
                }
            }
            int sum_white = 0;
            int sum_black = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((chessboard[i, j].id == 1) || (chessboard[i, j].id == -1))
                    {
                        return (false);
                    }
                    if (chessboard[i, j].id > 0)
                    {
                        if (chessboard[i, j].id != 2)
                        {
                            sum_white += chessboard[i, j].id;
                        }
                        else
                        {
                            sum_black += 5;
                        }
                    }
                    else
                    {
                        if (chessboard[i, j].id != -2)
                        {
                            sum_black -= chessboard[i, j].id;
                        }
                        else
                        {
                            sum_black += 5;
                        }
                    }
                }
            }
            if ((sum_white >= 11) || (sum_black >= 11))
            {
                return (false);
            }
            else
            {
                return (true);
            }
         
        }

    }
    public class Move //udaje o tahu
    {
        public int from_x;
        public int from_y;
        public int to_x;
        public int to_y;
    }
    public class MyPictureBox : PictureBox //upraveny picturebox
    {
        public int coordinate_x = 0;
        public int coordinate_y = 0;
        public int trans_id;
    }
    public class Coordinates //suradnice
    {
        public int x;
        public int y;
    }
    public class ChessPiece //figurka
    {
        public int id; //kladne biele, zaporne cierne
        public bool exist;
        public List<Coordinates> possible_moves = new List<Coordinates>();
        public int position_x;
        public int position_y;
    }
    public class Castling //data potrebne pre rosadu
    {
        public bool w_king_not_moved = true;
        public bool b_king_not_moved = true;
        public bool w_l_rook_not_moved = true;
        public bool w_r_rook_not_moved = true;
        public bool b_l_rook_not_moved = true;
        public bool b_r_rook_not_moved = true;
    }
    public class AI //data podstatne pre AI
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
    public static class EvalData //nacita data podstatne pre rozhodcu
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
    public static class Judge //hodnoti sachovnicu
    {
        public static int[,,] position_eval = new int[12, 8, 8];
        public static int Evaluate(ChessPiece[,] board, int ai_turn,int number_of_moves, int turn, bool is_check)
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
                        
                        
                    }
                }
            }
            if ((number_of_moves == 0) && (is_check == true))
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
            if (((is_check == false) && (number_of_moves == 0)) || (Pat.IsDraw(board, turn) == true))
            {
                ai_points = 0;
            }
            return (ai_points);
        }
        
    }
    public partial class Intro : Form  //vytvorenie a obsluha tlacitok, pomocou ktorych sa vybera typ hry
    {
        CheckBox singleplayer = new CheckBox();
        CheckBox multiplayer = new CheckBox();
        CheckBox ai_vs_ai = new CheckBox();
        CheckBox white = new CheckBox();
        CheckBox black = new CheckBox();
        Button button = new Button();
        TextBox textbox1 = new TextBox();
        TextBox textbox2 = new TextBox();
        List<CheckBox> check_boxes_ai1 = new List<CheckBox>();
        List<CheckBox> check_boxes_ai2 = new List<CheckBox>();
        public int gamemode;
        public int player_turn;
        public int depth_ai1;
        public int depth_ai2;
        public bool completed = false;
        public void InitializeIntro()
        {
            this.Height = 500;
            this.Width = 700;
            this.Name = "Welcome";
            this.Text = "TheChess";
            this.BackgroundImage = Image.FromFile(@"..\..\intro_background\background.png");
            InitSingle();
            InitMulti();
            InitAIvsAI();
            InitButton();
            InitTextBox1();

        }
        public void InitTextBox1()
        {
            string message = "Welcome to TheChess, choose your gamemode:";
            textbox1.Height = 50;
            textbox1.Width = 600;
            textbox1.Top = 50;
            textbox1.Left = 25;
            textbox1.Text = message;
            textbox1.ReadOnly = true;
            textbox1.Font = new Font("Arial", 18, FontStyle.Bold);
            textbox1.Parent = this;
        }
        public void InitTextBox2()
        {
            string message = "Choose the difficulty of the second AI";
            textbox2.Height = 50;
            textbox2.Width = 600;
            textbox2.Top = 200;
            textbox2.Left = 25;
            textbox2.Text = message;
            textbox2.ReadOnly = true;
            textbox2.Font = new Font("Arial", 18, FontStyle.Bold);
            textbox2.Parent = this;
        }
        public void InitSingle()
        {
            string message = " SinglePlayer ";
            singleplayer.Height = 100;
            singleplayer.Width = 200;
            singleplayer.Left = 30;
            singleplayer.Top = 100;
            singleplayer.Parent = this;
            singleplayer.Text = message;
            singleplayer.Font = new Font("Arial", 15, FontStyle.Bold);
            singleplayer.Click += SinglePlayerClicked;
        }
        public void InitMulti()
        {
            string message = " MultiPlayer ";
            multiplayer.Height = 100;
            multiplayer.Width = 200;
            multiplayer.Left = 230;
            multiplayer.Top = 100;
            multiplayer.Parent = this;
            multiplayer.Text = message;
            multiplayer.Font = new Font("Arial", 15, FontStyle.Bold);
            multiplayer.Click += MultiPlayerClicked;
        }
        public void InitAIvsAI()
        {
            string message = " AI versus AI ";
            ai_vs_ai.Height = 100;
            ai_vs_ai.Width = 200;
            ai_vs_ai.Left = 430;
            ai_vs_ai.Top = 100;
            ai_vs_ai.Parent = this;
            ai_vs_ai.Text = message;
            ai_vs_ai.Font = new Font("Arial", 15, FontStyle.Bold);
            ai_vs_ai.Click += AIvsAIClicked;
        }
        public void InitButton()
        {
            string message = "OK";
            button.Height = 50;
            button.Width = 200;
            button.Left = 430;
            button.Top = 400;
            button.Parent = this;
            button.Text = message;
            button.Font = new Font("Arial", 15, FontStyle.Bold);
            button.Click += ButtonClicked;
        }
        public void InitWhite()
        {
            string message = " White ";
            white.Height = 100;
            white.Width = 200;
            white.Left = 30;
            white.Top = 100;
            white.Parent = this;
            white.Text = message;
            white.Font = new Font("Arial", 15, FontStyle.Bold);
            white.Click += WhiteClicked;
           
        }
        public void InitBlack()

        {
            string message = " Black ";
            black.Height = 100;
            black.Width = 200;
            black.Left = 230;
            black.Top = 100;
            black.Parent = this;
            black.Text = message;
            black.Font = new Font("Arial", 15, FontStyle.Bold);
            black.Click += BlackClicked;
        }
        public void InitDepth1(int depth)
        {
            CheckBox ai1_depth = new CheckBox();
            string message = "Depth " + Convert.ToString(depth);
            ai1_depth.Tag = depth;
            ai1_depth.Height = 100;
            ai1_depth.Width = 150;
            ai1_depth.Left = 30 + (depth) * 150;
            ai1_depth.Top = 100;
            ai1_depth.Parent = this;
            ai1_depth.Text = message;
            ai1_depth.Font = new Font("Arial", 15, FontStyle.Bold);
            ai1_depth.Click += AI1DepthClicked;
            check_boxes_ai1.Add(ai1_depth);
        }
        public void InitDepth2(int depth)
        {
            CheckBox ai2_depth = new CheckBox();
            string message = "Depth " + Convert.ToString(depth);
            ai2_depth.Tag = depth;
            ai2_depth.Height = 100;
            ai2_depth.Width = 150;
            ai2_depth.Left = 30 + (depth) * 150;
            ai2_depth.Top = 250;
            ai2_depth.Parent = this;
            ai2_depth.Text = message;
            ai2_depth.Font = new Font("Arial", 15, FontStyle.Bold);
            ai2_depth.Click += AI2DepthClicked;
            check_boxes_ai2.Add(ai2_depth);
        }
        public void AI1DepthClicked(object sender, EventArgs e )
        {
            CheckBox check_box_clicked = sender as CheckBox;
            foreach(CheckBox check_box in check_boxes_ai1)
            {
                if (Convert.ToInt32(check_box.Tag) != Convert.ToInt32(check_box_clicked.Tag))
                {
                    check_box.Checked = false;
                }
            }
        }
        public void AI2DepthClicked(object sender, EventArgs e)
        {
            CheckBox check_box_clicked = sender as CheckBox;
            foreach (CheckBox check_box in check_boxes_ai2)
            {
                if (Convert.ToInt32(check_box.Tag) != Convert.ToInt32(check_box_clicked.Tag))
                {
                    check_box.Checked = false;
                }
            }
        }
        public void WhiteClicked(object sender, EventArgs e)
        {
            black.Checked = false;
        }
        public void BlackClicked(object sender, EventArgs e)
        {
            white.Checked = false;
        }
        public void ButtonClicked(object sender, EventArgs e)
        {
            if (button.Text == "OK")
            {
                if ((singleplayer.Checked) || (multiplayer.Checked) || (ai_vs_ai.Checked))
                {
                    singleplayer.Dispose();
                    multiplayer.Dispose();
                    ai_vs_ai.Dispose();
                    button.Text = "Start";
                    if (singleplayer.Checked)
                    {
                        textbox1.Text = "Choose the color of your pieces";
                        InitWhite();
                        InitBlack();
                        //InitTextBox1(1);
                        gamemode = -1;
                        for (int i = 0; i < 4; i++)
                        {
                            InitDepth2(i);
                        }
                        InitTextBox2();
                        textbox2.Text = "Choose the difficulty of AI (search depth)";

                    }
                    else
                    {
                        if (multiplayer.Checked)
                        {
                            gamemode = 0;
                            completed = true;
                            this.Close();
                      

                        }
                        else
                        {
                            gamemode = 1;
                            InitTextBox1();
                            textbox1.Text = "Choose the difficulty of the first AI";
                            InitTextBox2();
                            for (int i = 0; i < 4; i++)
                            {
                                InitDepth1(i);
                                InitDepth2(i);
                            }

                        }
                        //completed = true;
                        //this.Close();
                    }
                }
                else
                {
                    const string message = "Choose one of the gamemodes";
                    MessageBox.Show(message, "Error choosing gamemode", MessageBoxButtons.OK);
                }
            }
            else
            {
                if (singleplayer.Checked)
                {
                    bool check = false;
                    foreach (CheckBox check_box in check_boxes_ai2)
                    {
                        if (check_box.Checked == true)
                        {
                            check = true;
                            depth_ai2 = Convert.ToInt32(check_box.Tag);
                        }
                    }
                    if (((white.Checked) || (black.Checked)) && (check))
                    {
                        if (white.Checked)
                        {
                            player_turn = 1;
                        }
                        else
                        {
                            player_turn = -1;
                        }
                        completed = true;
                        this.Close();

                    }
                    else
                    {
                        const string message = "Choose one of the colors and difficulty";
                        MessageBox.Show(message, "Error choosing color or difficulty", MessageBoxButtons.OK);
                    }
                }
                if ((!singleplayer.Checked) && (!multiplayer.Checked))
                {
                    bool check1 = false;
                    foreach (CheckBox check_box in check_boxes_ai1)
                    {
                        if (check_box.Checked == true)
                        {
                            check1 = true;
                            depth_ai1 = Convert.ToInt32(check_box.Tag);
                        }
                    }
                    bool check2 = false;
                    foreach (CheckBox check_box in check_boxes_ai2)
                    {
                        if (check_box.Checked == true)
                        {
                            depth_ai2 = Convert.ToInt32(check_box.Tag);
                            check2 = true;
                        }
                    }
                    if (check1 && check2)
                    {
                        completed = true;
                        this.Close();
                    }
                    else
                    {
                        const string message = "Choose the AI depth";
                        MessageBox.Show(message, "Error choosing AI depth", MessageBoxButtons.OK);
                    }
                }
            }
        }
        public void  SinglePlayerClicked(object sender  , EventArgs e )
        {
            multiplayer.Checked = false;
            ai_vs_ai.Checked = false;

        }
        public void MultiPlayerClicked(object sender, EventArgs e)
        {
            singleplayer.Checked = false;
            ai_vs_ai.Checked = false;

        }
        public void AIvsAIClicked(object sender, EventArgs e)
        {
            singleplayer.Checked = false;
            multiplayer.Checked = false;
        }
    }
    public partial class TheChess : Form
    {
        int turn = 1;
        int player_turn;
        bool first_click = true;
        bool single_player = false;
        bool multi_player = false;
        bool first_call = false;
        Coordinates from = new Coordinates();
        Castling castling = new Castling();
        Button button = new Button();
        TextBox transformation = new TextBox();
        TextBox turn_textbox = new TextBox();
        Timer timer = new Timer();
        AI ai = new AI();
        Intro intro = new Intro();
        List<MyPictureBox> picture_boxes = new List<MyPictureBox>();
        List<MyPictureBox> trans_list = new List<MyPictureBox>();
        List<Coordinates> to_delete = new List<Coordinates>();
        ChessPiece[,] chessboard = new ChessPiece[8, 8];
        public void CreatePBoxes() //vytvori pictureboxy a vlozi do nich spravne pozadie
        {
            int black = 180;
            int white = 0;
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
        public void FillArray()  //vypln pole figurkami podla zaciatocneho rozostavenia
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
           
        } 
        public bool IsInChessBoard(int x, int y) //zisti, ci policko existuje
        {
            if ((x < 8) && (y < 8) && (x >= 0) && (y >= 0))
                return (true);
            else
                return (false);
        } 
        public void ClearPossibleMoves(ref ChessPiece[,] chessboard, int x, int y) //vynuluj zoznam moznych tahov
        {
            chessboard[x, y].possible_moves.Clear();
        } 
        public void AddPossibleMove(ref ChessPiece[,] chessboard, int a, int b, int base_x, int base_y)
        {
            Coordinates cord = new Coordinates();
            cord.x = a;
            cord.y = b;
            chessboard[base_x, base_y].possible_moves.Add(cord);
        } //pridaj mozny tah do zoznamu
        public void DeleteCheckMoves(ref ChessPiece[,] chessboard, int x, int y)
        {
            to_delete.Clear();
            bool was_deleted = false;
            //zalohovanie possible moves
            List<Coordinates>[,] chessboard_backup = new List<Coordinates>[8,8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((chessboard[i, j].id * turn < 0) && (chessboard[i, j].exist == true))
                    {
                        List<Coordinates> possible_moves_backup = new List<Coordinates>();
                        foreach (Coordinates possible_move in chessboard[i, j].possible_moves)
                        {
                            Coordinates pos_move = new Coordinates();
                            pos_move.x = possible_move.x;
                            pos_move.y = possible_move.y;
                            possible_moves_backup.Add(pos_move);
                        }
                        /*
                        foreach (Coordinates possible_move in possible_moves_backup)
                        {
                            Coordinates pos_move = new Coordinates();
                            possible_move.x = possible_move.x;
                            pos_move.y = possible_move.y;
                            possible_moves_backup.Add(pos_move);
                        }*/
                        chessboard_backup[i, j] = possible_moves_backup;
                    }
                }
            }
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
            //obnovenie zoznamu tahov
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((chessboard[i, j].id * turn < 0) && (chessboard[i, j].exist == true))
                    {
                        ClearPossibleMoves(ref chessboard, i, j);
                        foreach (Coordinates back_pos_move in chessboard_backup[i, j])
                        {
                            Coordinates restore_move = new Coordinates();
                            restore_move.x = back_pos_move.x;
                            restore_move.y = back_pos_move.y;
                            chessboard[i, j].possible_moves.Add(back_pos_move);
                        }
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

        } //odstran tahy, ktore by priviedli hraca do sachu
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
                    {
                        AddPossibleMove(ref chessboard, x - 1, y - 1, x, y);
                    }
                    if ((IsInChessBoard(x - 1, y + 1) == true) &&
                        (chessboard[x - 1, y + 1].exist == true) && (chessboard[x - 1, y + 1].id < 0))
                    {
                        AddPossibleMove(ref chessboard, x - 1, y + 1, x, y);
                    }
                    if ((x == 6) && (chessboard[x - 2, y].exist == false) && (chessboard[x - 1, y].exist == false))
                    {
                        AddPossibleMove(ref chessboard, x - 2, y, x, y);
                    }
                    break;
                case (-1)://cierny pesiak
                    if ((IsInChessBoard(x + 1, y) == true) && (chessboard[x + 1, y].exist == false))
                    {
                        
                        AddPossibleMove(ref chessboard, x + 1, y, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y + 1) == true) &&
                        (chessboard[x + 1, y + 1].exist == true) && (chessboard[x + 1, y + 1].id > 0))
                    {
                        AddPossibleMove(ref chessboard, x + 1, y + 1, x, y);
                    }
                    if ((IsInChessBoard(x + 1, y - 1) == true) &&
                        (chessboard[x + 1, y - 1].exist == true) && (chessboard[x + 1, y - 1].id > 0))
                    {
                        AddPossibleMove(ref chessboard, x + 1, y - 1, x, y);
                    }
                    if ((x == 1) && (chessboard[x + 2, y].exist == false) && (chessboard[x + 1, y].exist == false))
                    {
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
        } //spocitaj mozne tahy pre figurku
        public void AddSwapMove(ref ChessPiece[,] chessboard, int king_x, int king_y, int king_id) //zisti moznost rosady
        {
            if ((king_x == 0) && (FindCheck(ref chessboard, king_id) == false))
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
            if ((king_x == 7)  && (FindCheck(ref chessboard, king_id) == false))
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
            if ((multi_player == true) || ((turn == player_turn) && (single_player == true))) //obnov farby po kliknuti
            {
                MyPictureBox pic_click = sender as MyPictureBox;
                if ((first_click == true) || (pic_click.BackColor != Color.FromArgb(100, 0, 0, 255)))
                {
                    foreach (MyPictureBox pbox in picture_boxes)
                    {
                        if (((pbox.coordinate_x % 2 == 0) && (pbox.coordinate_y % 2 == 1))
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
                        else
                        {
                            if (turn == -1)
                            {
                                turn_textbox.Text = "TURN: White";

                            }
                            else
                            {
                                turn_textbox.Text = "TURN: Black";
                            }
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
                        Pat.AddToBuffer(chessboard, turn);
                        turn = -1 * turn;
                        if (((FindCheck(ref chessboard, 6 * turn) == false) && (CountMoves(ref chessboard) == 0)) || (Pat.IsDraw(chessboard, turn) == true))
                        {
                            const string message = "It is draw, thanks for playing ;)";
                            MessageBox.Show(message, "Draw", MessageBoxButtons.OK);
                            this.Close();
                        }
                        Redraw();
                        first_click = true;
                        if ((FindCheck(ref chessboard, 6 * turn) == true) && (single_player == false)
                            && (CountMoves(ref chessboard) != 0))
                        {
                            const string message = "You are in check";
                            MessageBox.Show(message, "Check", MessageBoxButtons.OK);
                        }
                        if ((FindCheck(ref chessboard, 6 * turn) == true)
                            && (CountMoves(ref chessboard) == 0))
                        {
                            if (turn == -1)
                            {
                                const string message = "White player wins, thanks for playing ;)";
                                MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                                this.Close();
                            }
                            else
                            {
                                const string message = "Black player wins, thanks for playing ;)";
                                MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                                this.Close();
                            }

                        }
                        else
                        {
                            if ((single_player && 
                                (IsPawnToTransform(ref chessboard, pic_click.coordinate_x, pic_click.coordinate_y) == false)))
                            {
                                timer.Enabled = true;
                            }
                        }
                    }
                }
            }
            
        } //handler na kliknutie policka
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
            board[to_x, to_y].exist = board[from_x, from_y].exist;                    //pridanie cielu
            board[to_x, to_y].id = board[from_x, from_y].id;     //premiesnenie
            board[from_x, from_y].exist = false;
            board[from_x, from_y].id = 0;  
            
            
            
        } //urobi tah
        public void Redraw() //vykresli stav sachovnice
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
        public void InitTurnInfo()
        {
            string message = "TURN: White";
            turn_textbox.Height = 50;
            turn_textbox.Width = 200;
            turn_textbox.Top = 50;
            turn_textbox.Left = 650;
            turn_textbox.Text = message;
            turn_textbox.ReadOnly = true;
            turn_textbox.Font = new Font("Arial", 18, FontStyle.Bold);
            turn_textbox.Parent = this;
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
        } //vrati poziciu hladaneho krala
        public bool FindCheck(ref ChessPiece[,] chessboard, int king_id)
        {
            Coordinates king_position = FindKingPosition(ref chessboard, king_id);
            for (int i = 0; i < 8; i++)
    
        {
                for (int j = 0; j < 8; j++)
                {
                    if ((king_id * chessboard[i,j].id < 0) && (chessboard[i,j].exist == true))
                    {
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
        } //zisti, ci je dany kral v sachu
        public bool IsPawnToTransform(ref ChessPiece[,] chessboard, int x, int y) //zisti, ci je mozne transformova pesiaka
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
        public void TransformPawn(int x, int y) //vytor moznost transformovat pesiaka
        {
            transformation.Show();
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
                pawn_transform_opt.Left = i * 100 + 630;
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
            if (turn == 1)
            {
                turn_textbox.Text = "TURN: White";
            }
            else
            {
                turn_textbox.Text = "TURN: Black";
            }
            transformation.Hide();
            foreach (MyPictureBox trans_box in trans_list)
            {
                trans_box.Dispose();
            }
            Pat.AddToBuffer(chessboard, -1 * turn);
            UpdatePossibleMoves(ref chessboard, clicked.coordinate_x, clicked.coordinate_y);
            if (((FindCheck(ref chessboard, 6 * turn) == false) && (CountMoves(ref chessboard) == 0)) || (Pat.IsDraw(chessboard, turn) == true))
            {
                const string message = "It is draw, thanks for playing ;)";
                MessageBox.Show(message, "Draw", MessageBoxButtons.OK);
                this.Close();
            }
            if ((FindCheck(ref chessboard, 6 * turn) == true)
                && (CountMoves(ref chessboard) !=0) && ((turn == player_turn) || multi_player == true))
            { 
                const string message = "You are in check";
                MessageBox.Show(message, "Check", MessageBoxButtons.OK);
            }
            if ((FindCheck(ref chessboard, 6 * turn) == true)
                            && (CountMoves(ref chessboard) == 0))
            {
                if (turn == -1)
                {
                    const string message = "White player wins, thanks for playing ;)";
                    MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                    this.Close();
                }
                else
                {
                    const string message = "Black player wins, thanks for playing ;)";
                    MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                    this.Close();
                }
            }
            if (single_player)                   
            {
                timer.Enabled = true;
            }

        } //handler pre vyber transformacie pesiaka
        public void PlaySingle(int ai_turn)
        {
            if (turn == ai_turn)
            {
                ai.GetChessboardState(chessboard);
                if (single_player)
                {
                    FindBestMove(ref ai.game_state, intro.depth_ai2);
                }
                else
                {
                    if (turn == 1)
                    {
                        FindBestMove(ref ai.game_state, intro.depth_ai1);
                    }
                    else
                    {
                        FindBestMove(ref ai.game_state, intro.depth_ai2);
                    }
                }

                MakeMove(ref chessboard, ai.move.from_x, ai.move.from_y, ai.move.to_x, ai.move.to_y);
                if (IsPawnToTransform(ref chessboard, ai.move.to_x, ai.move.to_y) == true)
                {
                    chessboard[ai.move.to_x, ai.move.to_y].id = 5 * chessboard[ai.move.to_x, ai.move.to_y].id;
                    //vymena za damu
                }
                Pat.AddToBuffer(chessboard, turn);
                turn = turn * -1;
                if (turn == 1)
                {
                    turn_textbox.Text = "TURN: White";
                }
                else
                {
                    turn_textbox.Text = "TURN: Black";
                }
                Redraw();
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
                if ((Pat.IsDraw(chessboard, turn) == true) || ((FindCheck(ref chessboard, 6 * turn) == false) && (CountMoves(ref chessboard) == 0)))
                {
                    const string message = "It is draw, thanks for playing ;)";
                    MessageBox.Show(message, "Draw", MessageBoxButtons.OK);
                    this.Close();
                }
                if ((FindCheck(ref chessboard, 6 * turn) == true) && (single_player == true)
                            && (CountMoves(ref chessboard) != 0))
                {
                    const string message = "You are in check";
                    MessageBox.Show(message, "Check", MessageBoxButtons.OK);
                }
                if ((FindCheck(ref chessboard, 6 * turn) == true)
                                && (CountMoves(ref chessboard) == 0))
                {
                    if (turn == -1)
                    {
                        const string message = "White player wins, thanks for playing ;)";
                        MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                        this.Close();
                    }
                    else
                    {
                        const string message = "Black player wins, thanks for playing ;)";
                        MessageBox.Show(message, "Check-mate", MessageBoxButtons.OK);
                        this.Close();
                    }
                }
                if ((!multi_player) && (!single_player))
                {
                    timer.Enabled = true;
                }
            }


        } //pre singleplayer obsluhuje hru AI
        public int CountMoves(ref ChessPiece[,] chess_state)
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
        } //spocita pocet moznych tahov prehraca na tahu
        public void FindBestMove(ref ChessPiece[,] game_state, int final_depth)
        {
            if (final_depth >= 3)
            {
                int guess = Max(ref game_state, 0, final_depth - 1, -35000, 35000);
            }
            first_call = true;
            int max_value = Max(ref game_state, 0, final_depth, -35000, 35000);
        } //spocita tah pre AI
        public int Max(ref ChessPiece[,] state, int depth, int final_depth, int alpha, int beta) //simuluje tah hraca
        {
            int turn_backup = turn;
            int max = -50000;
            int number_of_moves;
            
            if ((first_call == true) && final_depth >= 3)
            {
                bool was_transformed = false;
                first_call = false;
                bool w_l_r_backup = castling.w_l_rook_not_moved;
                bool w_r_r_backup = castling.w_r_rook_not_moved;
                bool b_r_r_backup = castling.b_r_rook_not_moved;
                bool b_l_r_backup = castling.b_l_rook_not_moved;
                bool w_k_backup = castling.w_king_not_moved;
                bool b_k_backup = castling.b_king_not_moved;
                bool exist_backup = state[ai.move.to_x, ai.move.to_y].exist;
                int id_backup = state[ai.move.to_x, ai.move.to_y].id;
                //make backup of buffer
                string[,] buffer_backup = new string[2, 5];
                for(int i = 0; i < 2; i++)
                {
                    for(int j = 0; j < 5; j++)
                    {
                        buffer_backup[i, j] = Pat.buffer[i, j];
                    }
                }
                MakeMove(ref state, ai.move.from_x , ai.move.from_y, ai.move.to_x, ai.move.to_y);
                if (IsPawnToTransform(ref state, ai.move.to_x, ai.move.to_y))
                {
                    state[ai.move.to_x, ai.move.to_y].id *= 5;
                    was_transformed = true;
                }
                Pat.AddToBuffer(state, turn);
                turn = -1 * turn;
                int value;
                number_of_moves = CountMoves(ref state);
                
                if ((number_of_moves != 0) && (depth != final_depth))
                {
                    value = Min(ref state, depth + 1, final_depth, alpha, beta);
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if ((state[i, j].id * turn < 0) && (state[i, j].exist == true))
                            {
                                UpdatePossibleMoves(ref state, i, j);
                            }
                        }
                    }
                    bool is_check = FindCheck(ref state, 6 * turn);
                    value = Judge.Evaluate(state, -1*turn, number_of_moves, turn, is_check);
                }
                if (value > max)
                {
                    max = value;
                }
                if (alpha < value)
                {
                    alpha = value;
                }
                turn = turn_backup;
                //restore buffer
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Pat.buffer[i, j] = buffer_backup[i, j];
                    }
                }
                if (was_transformed)
                {
                    state[ai.move.to_x, ai.move.to_y].id /= 5; 
                }
                state[ai.move.from_x, ai.move.from_y].id = state[ai.move.to_x, ai.move.to_y].id;
                state[ai.move.from_x, ai.move.from_y].exist = true;
                state[ai.move.to_x, ai.move.to_y].exist = exist_backup;
                state[ai.move.to_x, ai.move.to_y].id = id_backup;
                castling.w_l_rook_not_moved = w_l_r_backup;
                castling.w_r_rook_not_moved = w_r_r_backup;
                castling.b_r_rook_not_moved = b_r_r_backup;
                castling.b_l_rook_not_moved = b_l_r_backup;
                castling.w_king_not_moved = w_k_backup;
                castling.b_king_not_moved = b_k_backup;

            }
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((state[i, j].id * turn > 0) && (state[i, j].exist == true))
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
                        {
                            bool was_transformed = false;
                            if (alpha >= beta)
                            {
                                return (beta);
                            }
                            else
                            {
                                bool w_l_r_backup = castling.w_l_rook_not_moved;
                                bool w_r_r_backup = castling.w_r_rook_not_moved;
                                bool b_r_r_backup = castling.b_r_rook_not_moved;
                                bool b_l_r_backup = castling.b_l_rook_not_moved;
                                bool w_k_backup = castling.w_king_not_moved;
                                bool b_k_backup = castling.b_king_not_moved;
                                bool exist_backup = state[possible_move.x, possible_move.y].exist;
                                int id_backup = state[possible_move.x, possible_move.y].id;
                                string[,] buffer_backup = new string[2, 5];
                                for (int k = 0; k < 2; k++)
                                {
                                    for (int l = 0; l < 5; l++)
                                    {
                                        buffer_backup[k, l] = Pat.buffer[k, l];
                                    }
                                }
                                MakeMove(ref state, i, j, possible_move.x, possible_move.y);
                                if (IsPawnToTransform(ref state, possible_move.x, possible_move.y))
                                {
                                    state[possible_move.x, possible_move.y].id *= 5;
                                    was_transformed = true;
                                }
                                Pat.AddToBuffer(state, turn);
                                int value;
                                turn = -1 * turn;
                                number_of_moves = CountMoves(ref state);

                                if ((depth != final_depth) && (number_of_moves != 0))
                                {


                                    value = Min(ref state, depth + 1, final_depth, alpha, beta);
                                }
                                else
                                {
                                    for (int k = 0; k < 8; k++)
                                    {
                                        for (int l = 0; l < 8; l++)
                                        {
                                            if ((state[k, l].id * turn < 0) && (state[k, l].exist == true))
                                            {
                                                UpdatePossibleMoves(ref state, k, l);
                                            }
                                        }
                                    }
                                    bool is_check = FindCheck(ref state, 6 * turn);
                                    value = Judge.Evaluate(state, -1 * turn, number_of_moves, turn, is_check);

                                }
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
                                for (int k = 0; k < 2; k++)
                                {
                                    for (int l = 0; l < 5; l++)
                                    {
                                        Pat.buffer[k, l] = buffer_backup[k, l];
                                    }
                                }
                                if (was_transformed)
                                {
                                    state[possible_move.x, possible_move.y].id /= 5;
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

                        }

                    }
                        
                }
            }
            return (max);
        }  
        public int Min(ref ChessPiece[,] state, int depth, int final_depth, int alpha, int beta) //simuluje tah protihraca
        {
            int turn_backup = turn;
            int min = 50000;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((state[i, j].id * turn > 0) && (state[i, j].exist == true))
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
                        {
                            bool was_transformed = false;
                            if (alpha >= beta)

                            {
                                return (alpha);
                            }
                            else
                            {

                                bool w_l_r_backup = castling.w_l_rook_not_moved;
                                bool w_r_r_backup = castling.w_r_rook_not_moved;
                                bool b_r_r_backup = castling.b_r_rook_not_moved;
                                bool b_l_r_backup = castling.b_l_rook_not_moved;
                                bool w_k_backup = castling.w_king_not_moved;
                                bool b_k_backup = castling.b_king_not_moved;
                                bool exist_backup = state[possible_move.x, possible_move.y].exist;
                                int id_backup = state[possible_move.x, possible_move.y].id;
                                string[,] buffer_backup = new string[2, 5];
                                for (int k = 0; k < 2; k++)
                                {
                                    for (int l = 0; l < 5; l++)
                                    {
                                        buffer_backup[k, l] = Pat.buffer[k, l];
                                    }
                                }
                                MakeMove(ref state, i, j, possible_move.x, possible_move.y);
                                if (IsPawnToTransform(ref state, possible_move.x, possible_move.y))
                                {
                                    state[possible_move.x, possible_move.y].id *= 5;
                                }
                                Pat.AddToBuffer(state, turn);
                                int value;
                                turn = -1 * turn;
                                int number_of_moves = CountMoves(ref state);
                                if ((depth != final_depth) && (number_of_moves != 0))
                                {


                                    value = Max(ref state, depth + 1, final_depth, alpha, beta);
                                }
                                else
                                {
                                    for (int k = 0; k < 8; k++)
                                    {
                                        for (int l = 0; l < 8; l++)
                                        {
                                            if ((state[k, l].id * turn < 0) && (state[k, l].exist == true))
                                            {
                                                UpdatePossibleMoves(ref state, k, l);
                                            }
                                        }
                                    }
                                    bool is_check = FindCheck(ref state, 6 * turn);
                                    value = Judge.Evaluate(state, turn, number_of_moves, turn, is_check);
                                }
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
                                for (int k = 0; k < 2; k++)
                                {
                                    for (int l = 0; l < 5; l++)
                                    {
                                        Pat.buffer[k, l] = buffer_backup[k, l];
                                    }
                                }
                                if (was_transformed)
                                {
                                    state[possible_move.x, possible_move.y].id /= 5;
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
                            
                        }
                       
                    }
                }
            }
            return (min);
        }
        public TheChess() //prvotna inicializacia hlavneho formulara
        {
            InitializeComponent();
            this.Width = 1400;
            this.Height = 800;
 
            timer.Tick += TimerTick;
            EvalData.FillFromText(ref Judge.position_eval);
            InitStart();
      
            intro.InitializeIntro();
            intro.Show();


        }
        public void InitStart()
        {
            string message = " Start the game";
            button.Height = 100;
            button.Width = 200;
            button.Left = 30;
            button.Top = 100;
            button.Parent = this;
            button.Text = message;
            button.Font = new Font("Arial", 15, FontStyle.Bold);
            button.Click += StartClicked;
            this.Show();
            
            
        } //druhotna inicializacia hlavneho formulara
        public void StartClicked(object sender, EventArgs e)
        {
            Pat.InitBuffer();
            if (intro.completed)
            {
                if (intro.gamemode == -1)
                {
                    timer.Interval = 100;
                }
                else
                {
                    timer.Interval = 1000;
                }
                transformation.Left = 630;
                transformation.Top = 200;
                transformation.Height = 50;
                transformation.Width = 510;
                transformation.Font = new Font("Arial", 16, FontStyle.Bold);
                transformation.ReadOnly = true;
                transformation.Text = "Choose a piece you want to trasform the pawn to:";
                transformation.Parent = this;
                transformation.Hide();
                //this.BackgroundImage = Image.FromFile(@"..\..\main_background\background.jpg");
                button.Dispose();
                CreatePBoxes();
                FillArray();
                InitTurnInfo();
                Redraw();
                player_turn = intro.player_turn;
                if (intro.gamemode == -1)
                {
                    single_player = true;
                    if (player_turn == -1)
                    {
                        PlaySingle(player_turn * -1);
                    }
                }
                if (intro.gamemode == 0)
                {
                    multi_player = true;
                }
                if (intro.gamemode == 1)
                {
                    PlaySingle(1);
                }
                
            }
            else
            {
                const string message = "Please, choose your gamemode first ";
                MessageBox.Show(message, "Error starting game", MessageBoxButtons.OK);
            }
        }  //spustenie hry
        public void TimerTick(object sender, EventArgs e)
        { 
            timer.Enabled = false;
            PlaySingle(turn);
        } //spusti tah AI
        private void TheChess_Load(object sender, EventArgs e)
        {
        
        }
    }
}
