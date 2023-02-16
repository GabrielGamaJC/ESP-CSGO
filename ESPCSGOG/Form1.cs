using ezOverLay;
using Swed32;
using winformtemplate;

namespace ESPCSGOG
{
    public partial class Form1 : Form
    {
        ez ez = new ez();
        const int localplayer = 0xDEA964;
        const int entitylist = 0x4DFFEF4;
        const int viewmatrix = 0x4DF0D24;
        const int xyz = 0x138;
        const int team = 0xF4;
        const int dormant = 0xED;
        const int health = 0x100;


        Swed swed;
        Pen teampen = new Pen(Color.Blue, 3);
        Pen enemypen = new Pen(Color.Red, 3);
        entity player = new entity();
        List<entity> list = new List<entity>();

        IntPtr client;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            CheckForIllegalCrossThreadCalls = false;
            swed = new Swed("csgo");
            client = swed.GetModuleBase("client.dll");
            ez.SetInvi(this);

            ez.DoStuff("Counter-Strike: Global Offensive - Direct3D 9", this);
            Thread thread = new Thread(main) { IsBackground = true };
            thread.Start();




        }


        void main()
        {
            while (true)
            {

                updatelocal();
                updateentities();
                Form1 f = this;
                f.Refresh();

                Thread.Sleep(20);
            }

        }


        void updatelocal()
        {

            var buffer = swed.ReadPointer(client, localplayer);
            player.team = swed.ReadInt(buffer, team);
          //  swed.WriteBytes(buffer, 0x100, BitConverter.GetBytes(998));
        }


        void updateentities()
        {
            list.Clear();
            var entityl = swed.ReadPointer(client, entitylist);

            for (int i = 0; i < 32; i++)
            {


                var buffer = swed.ReadPointer(entityl, i * 0x10);


                // var tm = BitConverter.ToInt32(swed.ReadBytes(buffer, team, 4), 0);
                //  var dorm = BitConverter.ToInt32(swed.ReadBytes(buffer, dormant, 4), 0);
                // var hp = BitConverter.ToInt32(swed.ReadBytes(buffer, health, 4), 0);

                var tm = swed.ReadInt(buffer, team);
                var dorm = swed.ReadInt(buffer, dormant);
                var hp = swed.ReadInt(buffer, health);
                //  ent.health = swed.ReadInt(ent.BaseAddress, Offsets.iHealth);
                // ent.team = swed.ReadInt(ent.BaseAddress, Offsets.iTeam);

                // MessageBox.Show(hp.ToString());

                var coords = swed.ReadBytes(buffer, xyz, 12);

                var ent = new entity
                {
                    x = BitConverter.ToSingle(coords, 0),
                    y = BitConverter.ToSingle(coords, 4),
                    z = BitConverter.ToSingle(coords, 8),
                    team = tm,
                    health = hp,

                };
              //  ent.bot = WorldToScreen(readmatrix(), ent.x, ent.y, ent.z, Width, Height);
             //   ent.top = WorldToScreen(readmatrix(), ent.x, ent.y, ent.z + 58, Width, Height);

                //MessageBox.Show(hp.ToString());

                if (hp > 0 && hp < 101)
                    list.Add(ent);
                // MessageBox.Show("add");
               // MessageBox.Show("hp  "+hp);
                
            }

        }


        viewmatrix readmatrix()
        {
            var viewMatrix = new viewmatrix();
            var mtx = swed.ReadMatrix(client + viewmatrix);
            

            viewMatrix.m11 = mtx[0];
            viewMatrix.m12 = mtx[1];
            viewMatrix.m13 = mtx[2];
            viewMatrix.m14 = mtx[3];

            viewMatrix.m21 = mtx[4];
            viewMatrix.m22 = mtx[5];
            viewMatrix.m23 = mtx[6];
            viewMatrix.m24 = mtx[7];

            viewMatrix.m31 = mtx[8];
            viewMatrix.m32 = mtx[9];
            viewMatrix.m33 = mtx[10];
            viewMatrix.m34 = mtx[11];

            viewMatrix.m41 = mtx[12];
            viewMatrix.m42 = mtx[13];
            viewMatrix.m43 = mtx[14];
            viewMatrix.m44 = mtx[15];

            return viewMatrix;

        }


        Point WorldToScreen(viewmatrix mtx, float x, float y, float z, int width, int height)
        {
            var twoD = new Point();

            float screenW = (mtx.m41 * x) + (mtx.m42 * y) + (mtx.m43 * z) + mtx.m44;

            if (screenW > 0.001f)
            {
                float screenX = (mtx.m11 * x) + (mtx.m12 * y) + (mtx.m13 * z) + mtx.m14;
                float screenY = (mtx.m21 * x) + (mtx.m22 * y) + (mtx.m23 * z) + mtx.m24;

                float camX = width / 2f;
                float camY = height / 2f;

                float X = camX + (camX * screenX / screenW);

                float Y = camY - (camY * screenY / screenW);

                twoD.X = (int)X;
                twoD.Y = (int)Y;

                return twoD;


            }

            else
            {
                return new Point(-99, -99);
            }

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            if (list.Count > 0)
            {
                try
                {

                    foreach (var ent in list)
                    {
                        ent.bot = WorldToScreen(readmatrix(), ent.x, ent.y, ent.z, this.Width, this.Height);
                            ent.top = WorldToScreen(readmatrix(), ent.x, ent.y, ent.z + 58, this.Width, this.Height);
                        // MessageBox.Show(ent.health.ToString());
                        //     g.DrawRectangle(teampen, ent.rect());
                        //     g.DrawLine(teampen, new Point(Width / 2, Height), ent.bot);

                        if (ent.bot.X > 0)
                        {
                            if (ent.team == player.team) //&& ent.bot.X > 0 && ent.bot.X < Width && ent.top.Y > 0 && ent.bot.Y < Height)
                            {
                                // MessageBox.Show("oi");
                                g.DrawRectangle(teampen, ent.rect());
                                g.DrawLine(teampen, new Point(Width / 2, Height), ent.bot);
                                g.DrawRectangle(teampen, CalcRec(ent.bot, ent.top));

                            }

                            else //if (ent.team != player.team && ent.bot.X > 0 && ent.bot.X < Width && ent.top.Y > 0 && ent.bot.Y < Height)
                            {
                                g.DrawRectangle(enemypen, ent.rect());
                                g.DrawLine(enemypen, new Point(Width / 2, Height), ent.bot);
                                g.DrawRectangle(enemypen, CalcRec(ent.bot, ent.top));

                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public Rectangle CalcRec(Point feet, Point head)
        {

            var rect = new Rectangle();
            rect.X = head.X - (feet.Y - head.Y) / 4;
            rect.Y = head.Y;

            rect.Width = (feet.Y - head.Y) / 2;
            rect.Height = feet.Y - head.Y;
            return rect;
        }

    }
}