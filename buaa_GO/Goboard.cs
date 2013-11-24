/**
 *  Go Applet
 *  1996.11		xinz	written in Java
 *  2001.3		xinz	port to C#
 *  2001.5.10	xinz	file parsing, back/forward
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Resources;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
[assembly: CLSCompliant(true)]
//[assembly: NeutralResourcesLanguageAttribute("en-US")]
namespace GoWinApp
{

	public enum StoneColor : int
	{
		Black = 0, White = 1
	}


	/**
	 * 棋盘类
	 */
	public class GoBoard : System.Windows.Forms.Form
	{
		string [] strLabels; // {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

		int nSize;		                //棋盘大小
		const int nBoardMargin = 10;	//边界宽
		int nCoodStart = 4;
		const int	nBoardOffset = 20;
		int nEdgeLen = nBoardOffset + nBoardMargin;
		int nTotalGridWidth = 360 + 36;	//总的棋盘格子宽度
		int nUnitGridWidth = 22;		//单个格子宽度
		int nSeq = 0;
		Rectangle rGrid;		    //单个格子对象
		StoneColor m_colorToPlay;   // 当前下棋的颜色
		GoMove m_gmLastMove;	    //记录最新一步
		Boolean bDrawMark;	        //是否有画标记
		Boolean m_fAnyKill;	        //   是否有杀的动作
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        Spot [,] Grid;		        //存棋盘中点的数组
		Pen penGrid;//, penStoneW, penStoneB,penMarkW, penMarkB;
		Brush brStar, brBoard, brBlack, brWhite, m_brMark;
	
        // 查看功能的变量
        int nFFMove = 10;   //可前进10步
     //   int nRewindMove = 10;  // 可退后10步 

		GoTree	gameTree;

		/// <ZZZZZZZ>
		///   UI控件
		/// </ZZZZZZZ>
//		private System.ComponentModel.Container components;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button Rewind;
		private System.Windows.Forms.Button FForward;
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Button Open;
		private System.Windows.Forms.Button Back;
		private System.Windows.Forms.Button Forward;

		public GoBoard(int nSize)
		{
			//
			// 初始化棋盘，假如控件
			//
			InitializeComponent();

			//
			//   更改控件样式
			//

			this.nSize = nSize;  // 棋盘大小

			m_colorToPlay = StoneColor.Black;

			Grid = new Spot[nSize,nSize];
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					Grid[i,j] = new Spot();
            //penGrid = new Pen(Color.Brown, (float)0.5);
            //penStoneW = new Pen(Color.WhiteSmoke, (float)1);
            //penStoneB = new Pen(Color.Black,(float)1);
            //penMarkW = new Pen(Color.Blue, (float) 1);
            //penMarkB = new Pen(Color.Beige, (float) 1);

            //brStar = new SolidBrush(Color.Black);
            //brBoard = new SolidBrush(Color.Orange);
            brBlack = new SolidBrush(Color.Black);
            brWhite = new SolidBrush(Color.White);
            //m_brMark = new SolidBrush(Color.Red);

			rGrid = new Rectangle(nEdgeLen, nEdgeLen,nTotalGridWidth, nTotalGridWidth);
			strLabels = new string [] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};
			gameTree = new GoTree();
		}

		/// <ZZZZZZZ>
		///    初始化棋盘，加入各种控件
		///    对，就是各种控件
		/// </ZZZZZZZ>
		private void InitializeComponent()
		{
            this.Open = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.Rewind = new System.Windows.Forms.Button();
            this.Forward = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.FForward = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            ResourceManager stringManager;
            stringManager = new ResourceManager("en-US", Assembly.GetExecutingAssembly());

            // 
            // Open
            // 
            this.Open.Location = new System.Drawing.Point(534, 95);
            this.Open.Name = stringManager.GetString(
                "Open", CultureInfo.CurrentUICulture);//"Open";
            this.Open.Size = new System.Drawing.Size(67, 25);
            this.Open.TabIndex = 2;
            this.Open.Text = stringManager.GetString(
                "open", CultureInfo.CurrentUICulture);//"open";
            this.Open.Click += new System.EventHandler(this.OpenClick);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(611, 95);
            this.Save.Name = stringManager.GetString(
                "Save", CultureInfo.CurrentUICulture);//"Save";
            this.Save.Size = new System.Drawing.Size(67, 25);
            this.Save.TabIndex = 3;
            this.Save.Text = stringManager.GetString(
                "save", CultureInfo.CurrentUICulture);//"save";
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // Rewind
            // 
            this.Rewind.Location = new System.Drawing.Point(611, 60);
            this.Rewind.Name = stringManager.GetString(
                "Rewind", CultureInfo.CurrentUICulture);//"Rewind";
            this.Rewind.Size = new System.Drawing.Size(67, 25);
            this.Rewind.TabIndex = 5;
            this.Rewind.Text = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);// "<<";
            this.Rewind.Click += new System.EventHandler(this.RewindClick);
            // 
            // Forward
            // 
            this.Forward.Location = new System.Drawing.Point(534, 26);
            this.Forward.Name = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//"Forward";
            this.Forward.Size = new System.Drawing.Size(67, 25);
            this.Forward.TabIndex = 0;
            this.Forward.Text = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//">";
            this.Forward.Click += new System.EventHandler(this.ForwardClick);
            // 
            // Back
            // 
            this.Back.Location = new System.Drawing.Point(611, 26);
            this.Back.Name = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//"Back";
            this.Back.Size = new System.Drawing.Size(67, 25);
            this.Back.TabIndex = 1;
            this.Back.Text = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//"<";
            this.Back.Click += new System.EventHandler(this.BackClick);
            // 
            // FForward
            // 
            this.FForward.Location = new System.Drawing.Point(534, 60);
            this.FForward.Name = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//"FForward";
            this.FForward.Size = new System.Drawing.Size(67, 25);
            this.FForward.TabIndex = 4;
            this.FForward.Text = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//">>";
            this.FForward.Click += new System.EventHandler(this.FForwardClick);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(536, 138);
            this.textBox1.Multiline = true;
            this.textBox1.Name = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//"textBox1";
            this.textBox1.Size = new System.Drawing.Size(144, 335);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);// "please oepn a .sgf file to view, or just play on the board";
            this.textBox1.TextChanged += new System.EventHandler(this.TextBox1TextChanged);
            // 
            // GoBoard
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(768, 495);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Rewind);
            this.Controls.Add(this.FForward);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Open);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Forward);
            this.Name = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//"GoBoard";
            this.Text = stringManager.GetString(
                "minuteOutOfRangeMessage", CultureInfo.CurrentUICulture);//"Go_WinForm";
            this.Click += new System.EventHandler(this.GoBoardClick);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintHandler);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		protected void TextBox1TextChanged (object sender, System.EventArgs e)
		{
			return;
		}

		private void PaintHandler(Object sender, PaintEventArgs e)
		{
			UpdateGoBoard(e);
		}

		protected void SaveClick (object sender, System.EventArgs e)
		{
			return;
		}

		protected void OpenClick (object sender, System.EventArgs e)
		{
			OpenFile();
			showGameInfo();
		}

		protected void RewindClick (object sender, System.EventArgs e)
		{
			gameTree.Reset();
			ResetBoard();
            showGameInfo();
		}

		protected void FForwardClick (object sender, System.EventArgs e)
		{
            if (gameTree != null)
            {
                int i = 0; 
                GoMove gm = null;
                for (gm = gameTree.DoNext(); gm != null; gm = gameTree.DoNext()) 
                {
                    PlayNext(ref gm);
                    if (i++ > nFFMove)
                        break; 
                }
            }
		}

		protected void ForwardClick (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.DoNext();
			if (null != gm)
			{
				PlayNext(ref gm);
			}
		}

		private void showGameInfo()
		{
			//显示游戏信息
			textBox1.Clear();
			textBox1.AppendText(gameTree.Info);
		}

		protected void BackClick (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.DoPrev();	//倒退
            if (null != gm)
            {
                PlayPrev(gm);
            }
            else
            {
                ResetBoard();
                showGameInfo(); 
            }
		}

		Boolean onBoard(int x, int y) 
		{
			return (x>=0 && x<nSize && y>=0 && y<nSize);
		}

		protected void GoBoardClick (object sender, System.EventArgs e)
		{
			return;
		}

		private Point PointToGrid(int x, int y)
		{
			Point p= new Point(0,0);
			p.X = (x - rGrid.X + nUnitGridWidth/2) / nUnitGridWidth;
			p.Y = (y - rGrid.Y + nUnitGridWidth/2) / nUnitGridWidth;
			return p;
		}

		//判断点击位置是否离棋盘上的点足够近，即判断是否选中正确点
		// 判定是否足够近的范围是在1/3的单元宽度的矩形内
		private Boolean closeEnough(Point p, int x, int y)
		{
			if (x < rGrid.X+nUnitGridWidth*p.X-nUnitGridWidth/3 ||
				x > rGrid.X+nUnitGridWidth*p.X+nUnitGridWidth/3 ||
				y < rGrid.Y+nUnitGridWidth*p.Y-nUnitGridWidth/3 ||
				y > rGrid.Y+nUnitGridWidth*p.Y+nUnitGridWidth/3)
			{
				return false;
			}
			else 
				return true;
		}
        /// <ZZZZZZZ>
        /// 
        /// </ZZZZZZZ>
        /// <ZZZZZ ZZZZ="ZZZZZZ"></ZZZZZ>
        /// <ZZZZZ ZZZZ="Z"></ZZZZZ>
		private void MouseUpHandler(Object sender,MouseEventArgs e)
		{
			Point p;
			GoMove	gmThisMove;

			p = PointToGrid(e.X,e.Y);
			if (!onBoard(p.X, p.Y) || !closeEnough(p,e.X, e.Y)|| Grid[p.X,p.Y].HasStone())
				return; //无法在当前鼠标位置下棋

			// 如果可以下，记录下这步，更改游戏各种相关状态
			gmThisMove = new GoMove(p.X, p.Y, m_colorToPlay, 0);
            if (gmThisMove!=null)
			    PlayNext(ref gmThisMove);
			gameTree.AddMove(gmThisMove);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:验证公共方法的参数", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public void PlayNext(ref GoMove gm) 
		{
            if (gm != null)
            {
                Point p = gm.Point;
                m_colorToPlay = gm.Color;	// 记录当前颜色

                // 清除棋盘上点的标签和标记 
                clearLabelsAndMarksOnBoard();

                if (m_gmLastMove != null)
                    repaintOneSpotNow(m_gmLastMove.Point);

                bDrawMark = true;
                Grid[p.X, p.Y].SetStone(gm.Color);
                m_gmLastMove = new GoMove(p.X, p.Y, gm.Color, nSeq++);
                //加入各种标签
                SetLabelsOnBoard(gm);
                SetMarksOnBoard(gm);

                doDeadGroup(nextTurn(m_colorToPlay));
                //判断对方棋子是否被吃 ，被吃的棋子加入当前步骤的死亡棋子队列，记录颜色
                if (m_fAnyKill)
                    appendDeadGroup(ref gm, nextTurn(m_colorToPlay));
                else //清理当前颜色的死棋
                {
                    doDeadGroup(m_colorToPlay);
                    if (m_fAnyKill)
                        appendDeadGroup(ref gm, m_colorToPlay);
                }
                m_fAnyKill = false;

                optRepaint();

                // 交换颜色
                m_colorToPlay = nextTurn(m_colorToPlay);

                //显示当前步骤的评论
                textBox1.Clear();
                textBox1.AppendText(gm.Comment);
            }
		}

		private void appendDeadGroup(ref GoMove gm, StoneColor c)
		{
			ArrayList a = new ArrayList();
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].IsKilled())
					{
						Point pt = new Point(i,j);
						a.Add(pt);
						Grid[i,j].SetNoKilled();
					}
			gm.DeadGroup = a;
			gm.DeadGroupColor = c;
		}

		public void ResetBoard()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].RemoveStone();
			m_gmLastMove = null;
			Invalidate(null);
		}

        /*
         *  play the move so that the game situation is just BEFORE this move is played
         * what to do:
         * 	1. remove the current move from the board
         *  1.1 also remove the "lastmove" hightlght
         *	2. store the stones got killed by current move
         *  3. hightlight the new "lastmove"
         */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:验证公共方法的参数", MessageId = "0")]
        public void PlayPrev(GoMove gm)
		{
            if (gm != null)
            {
                Grid[gm.Point.X, gm.Point.Y].RemoveStone();
                return;
            }
            if (gm != null)
            {
                repaintOneSpotNow(gm.Point);
                return;
            }
       //     ArrayList a = gm.DeadGroup;
            if (gm.DeadGroup != null)
            {
                foreach (Point p in gm.DeadGroup)
                {
                    repaintOneSpotNow(p);
                    Grid[p.X, p.Y].SetStone(nextTurn(gm.Color));
                }
            }
            m_gmLastMove = gameTree.PeekPrev();
            // 清除棋盘上点的标签和标记 
            clearLabelsAndMarksOnBoard();
            if (m_gmLastMove != null)
                repaintOneSpotNow(m_gmLastMove.Point);
            return; 
        }

				
		
		Rectangle getUpdatedArea(int i, int j) 
		{
			int x = rGrid.X + i * nUnitGridWidth - nUnitGridWidth/2;
			int y = rGrid.Y + j * nUnitGridWidth - nUnitGridWidth/2;
			return new Rectangle(x,y, nUnitGridWidth, nUnitGridWidth);
		}

		/**
		 * 将更新状态的点变为不可更改
		 */
		private void optRepaint()
		{
			Rectangle r = new Rectangle(0,0,0,0);
            Region re;
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].IsUpdated()) 
					{
						r = getUpdatedArea(i,j);

                        using (re = new Region(r))
                        {
                            Invalidate(re);
                        }
                        
					}
		}

		/*
		 * 重新绘制点
		 */
		void repaintOneSpotNow(Point p)
		{
			Grid[p.X, p.Y].SetUpdated();
			bDrawMark = false;
			Rectangle r = getUpdatedArea(p.X, p.Y);
            using (Region rr = new Region(r))
            {
                Invalidate(rr);
            }
			Grid[p.X, p.Y].RESetUpdated();
			bDrawMark = true;
		}

		//记录步骤
		public void RecordMove(Point poi, StoneColor colorToPlay) 
		{
			Grid[poi.X,poi.Y].SetStone(colorToPlay);
			// 记录最新步骤
			m_gmLastMove = new GoMove(poi.X, poi.Y, colorToPlay, nSeq++);
		}

		static StoneColor nextTurn(StoneColor c) 
		{
			if (c == StoneColor.Black)
				return StoneColor.White;
			else 
				return StoneColor.Black;
		}

		/**
		 *	bury the dead stones in a group (same color). 
		 *	if a stone in one group is dead, the whole group is dead.
		*/
		void buryTheDead(int i, int j, StoneColor c) 
		{
			if (onBoard(i,j) && Grid[i,j].HasStone() && 
				Grid[i,j].Color() == c) 
			{
				Grid[i,j].Die();
				buryTheDead(i-1, j, c);
				buryTheDead(i+1, j, c);
				buryTheDead(i, j-1, c);
				buryTheDead(i, j+1, c);
			}
		}

		void cleanScanStatus()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].ClearScanned();
		}

		/**
		 * 清除死棋
		 */
		void doDeadGroup(StoneColor c) 
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					if (Grid[i,j].HasStone() &&
						Grid[i,j].Color() == c) 
					{
						if (calcLiberty(i,j,c) == 0)
						{
							buryTheDead(i,j,c);
							m_fAnyKill = true;
						}
						cleanScanStatus();
					}
		}


		/**
		 * 计算“气”
		 */
		int calcLiberty(int x, int y, StoneColor c) 
		{
			int lib = 0; // 当前棋子的气	
			
			if (!onBoard(x,y))
				return 0;
			if (Grid[x,y].IsScanned())
				return 0;

			if (Grid[x,y].HasStone()) 
			{
				if (Grid[x,y].Color() == c) 
				{
					//向四个方向深搜计算气
					Grid[x,y].SetScanned();
					lib += calcLiberty(x-1, y, c);
					lib += calcLiberty(x+1, y, c);
					lib += calcLiberty(x, y-1, c);
					lib += calcLiberty(x, y+1, c);
				} 
				else 
					return 0;
			} 
			else 
			{// 当前位置无棋子，“气”为1
				lib ++;
				Grid[x,y].SetScanned();
			}

			return lib;
		}


		/**
		 *  标记最新一步
		 */
		void markLastMove(Graphics g) 
		{
			Brush brMark;
			if (m_gmLastMove.Color == StoneColor.White)
				brMark = brBlack;
			else 
				brMark = brWhite;
			Point p = m_gmLastMove.Point;
			g.FillRectangle( brMark,
				rGrid.X + (p.X) * nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + (p.Y) * nUnitGridWidth - (nUnitGridWidth-1)/8,
				3, 3);
		}

		private void clearLabelsAndMarksOnBoard()
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].HasLabel())
						Grid[i,j].RESetLabel();
					if (Grid[i,j].HasMark())
						Grid[i,j].RESetMark();
				}

		}

		private void SetLabelsOnBoard(GoMove gm)
		{
			short	nLabel = 0;
			Point p;
			if (null != gm.Labels)
			{
		//		int i = gm.Labels.Count;
		//		i = gm.Labels.Capacity;

				System.Collections.IEnumerator myEnumerator = gm.Labels.GetEnumerator();
				while (myEnumerator.MoveNext())
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].SetLabel(++nLabel);
				}
			}
		}

		private void SetMarksOnBoard(GoMove gm)
		{
			Point p;
			if (null != gm.Labels)
			{
				System.Collections.IEnumerator myEnumerator = gm.Marks.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].SetMark();
				}
			}
		}

	    static private Point SwapXY(Point p)
		{
			Point pNew = new Point(p.Y,p.X);
			return pNew;
		}

		private void DrawBoard(Graphics g)
		{
			//画棋盘边界
			string[] strV= {"1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19"};
			string [] strH= {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

			Point p1 = new Point(nEdgeLen,nEdgeLen);
			Point p2 = new Point(nTotalGridWidth+nEdgeLen,nEdgeLen);
			g.FillRectangle(brBoard,nBoardOffset,nBoardOffset,nTotalGridWidth+nBoardOffset,nTotalGridWidth+nBoardOffset);
			for (int i=0;i<nSize; i++)
			{
				g.DrawString(strV[i],this.Font, brBlack, 0, nCoodStart+ nBoardOffset + nUnitGridWidth*i);
				g.DrawString(strH[i],this.Font, brBlack, nBoardOffset + nCoodStart + nUnitGridWidth*i, 0);
				g.DrawLine(penGrid, p1, p2);
				g.DrawLine(penGrid, SwapXY(p1), SwapXY(p2));

				p1.Y += nUnitGridWidth;
				p2.Y += nUnitGridWidth;
			}
			//画棋盘
            using (Pen penHi = new Pen(Color.WhiteSmoke, (float)0.5))
            {
                using (Pen penLow = new Pen(Color.Gray, (float)0.5))
                {

                    g.DrawLine(penHi, nBoardOffset, nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nBoardOffset);
                    g.DrawLine(penHi, nBoardOffset, nBoardOffset, nBoardOffset, nTotalGridWidth + 2 * nBoardOffset);
                    g.DrawLine(penLow, nTotalGridWidth + 2 * nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nBoardOffset + 1, nTotalGridWidth + 2 * nBoardOffset);
                    g.DrawLine(penLow, nTotalGridWidth + 2 * nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nBoardOffset + 1);
                }
            }
        }

		void UpdateGoBoard(PaintEventArgs e)
		{
			DrawBoard(e.Graphics);
			
			//画星
			drawStars(e.Graphics);

			//画点
			drawEverySpot(e.Graphics);
		}

		//画星
		void drawStar(Graphics g, int row, int col) 
		{
			g.FillRectangle(brStar,
				rGrid.X + (row-1) * nUnitGridWidth - 1, 
				rGrid.Y + (col-1) * nUnitGridWidth - 1, 
				3, 
				3);
		}

		//  19*19棋盘中画9个星
		void  drawStars(Graphics g)
		{
			drawStar(g, 4, 4);
			drawStar(g, 4, 10);
			drawStar(g, 4, 16);
			drawStar(g, 10, 4);
			drawStar(g, 10, 10);
			drawStar(g, 10, 16);
			drawStar(g, 16, 4);
			drawStar(g, 16, 10);
			drawStar(g, 16, 16);
		}

		/**
		 * 给定位置和颜色
         * 画棋子。
		 */
		void drawStone(Graphics g, int row, int col, StoneColor c) 
		{
			Brush br;
			if (c == StoneColor.White)
				br = brWhite;
			else 
				br = brBlack;
			
			Rectangle r = new Rectangle(rGrid.X+ (row) * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + (col) * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(br, r);
		}

		void drawLabel(Graphics g, int x, int y, short nLabel) 
		{
			if (nLabel ==0)
				return;
			nLabel --;
			nLabel %= 18;			//画边界标签计数

			//ZZZZZ ZZZ ZZ. 
			Rectangle r = new Rectangle(rGrid.X+ x * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(brBoard, r);

			g.DrawString(strLabels[nLabel],	//画标签
				this.Font, 
				brBlack, 
				rGrid.X+ (x) * nUnitGridWidth - (nUnitGridWidth-1)/4, 
				rGrid.Y + (y) * nUnitGridWidth - (nUnitGridWidth-1)/2);
		}

		void drawMark(Graphics g, int x, int y)
		{
			g.FillRectangle( m_brMark,
				rGrid.X + x* nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/8,
				5, 5);
		}

		void drawEverySpot(Graphics g) 
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].HasStone())
						drawStone(g, i, j, Grid[i,j].Color());
					if (Grid[i,j].HasLabel())
						drawLabel(g, i, j, Grid[i,j].getLabel);
					if (Grid[i,j].HasMark())
						drawMark(g, i, j);
				}
			//正在绘制则标记最新步骤 
			if (bDrawMark && m_gmLastMove != null)
				markLastMove(g);
		}

		//打开棋局
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)")]
        private void OpenFile()
		{
            ResourceManager stringManager;
            stringManager = new ResourceManager("en-US", Assembly.GetExecutingAssembly());
            using (OpenFileDialog openDlg = new OpenFileDialog())
            {
                openDlg.Filter = stringManager.GetString(
                "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*", CultureInfo.CurrentUICulture);// "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*";
                openDlg.FileName = "";
                openDlg.DefaultExt = ".sgf";
                openDlg.CheckFileExists = true;
                openDlg.CheckPathExists = true;

                DialogResult res = openDlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    if (!(openDlg.FileName).EndsWith(".sgf") && !(openDlg.FileName).EndsWith(".SGF"))
                        MessageBox.Show(stringManager.GetString(
                "Unexpected file format", CultureInfo.CurrentUICulture), stringManager.GetString(
                "Super Go Format", CultureInfo.CurrentUICulture), MessageBoxButtons.OK);
                    else
                    {
                        using (FileStream f = new FileStream(openDlg.FileName, FileMode.Open))
                        {
                            StreamReader r = new StreamReader(f);
                            string s = r.ReadToEnd();
                            gameTree = new GoTree(s);
                            gameTree.Reset();
                            ResetBoard();
                            //r.Close();
                        }
                    }
                }
            }
		}	
	}

	static public class GoTest
	{
		/// <ZZZZZZZ>
		/// 游戏开始
		/// </ZZZZZZZ>
        [STAThread]
		public static void Main() 
		{
			Application.Run(new GoBoard(19));
		}
	}

	
	//点类
	public class Spot 
	{
		private Boolean bEmpty;
		private Boolean bKilled;
        private StoneColor s;
		private short	m_nLabel;
		private Boolean m_bMark;
		private Boolean bScanned;
		private Boolean bUpdated; //记录点的各种状态
		/**
		 * 初始化
		 */
		public Spot() 
		{
			bEmpty = true;
			bScanned = false;
			bUpdated = false;
			bKilled = false;
		}
		
		public Boolean HasStone() { return !bEmpty;	}
		public Boolean IsEmpty() {	return bEmpty;	}
        public StoneColor ThisStone() { return s; }
		public StoneColor Color() {	return s;}

		public Boolean HasLabel() {return m_nLabel>0;}
		public Boolean HasMark() {return m_bMark;}
		public void SetLabel(short lab) {m_nLabel = lab; bUpdated = true; }
		public void SetMark() {m_bMark = true; bUpdated = true;}
		public void RESetLabel() {m_nLabel = 0; bUpdated = true;}
		public void RESetMark() {m_bMark = false; bUpdated = true;}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "get")]
        public short getLabel
        {
            get { return m_nLabel; } 
        }

		public Boolean IsScanned() { return bScanned;}
		public void SetScanned() {	bScanned = true;}
		public void ClearScanned() { bScanned = false; }

		public void SetStone(StoneColor col) 
		{
			if (bEmpty) 
			{
				bEmpty = false;
				s = col;
				bUpdated = true;
			} //  当前点无棋时更改状态
		}

		/*
		 * 棋子移除和死亡动作
		*/
		public void RemoveStone()
		{	//   移除棋子时的状态改变
			bEmpty = true;
			bUpdated = true;
		}
				
		//  棋子死亡时的状态改变
		public void Die() 
		{
			bKilled = true;
			bEmpty = true;
			bUpdated = true;
		} 

		public Boolean IsKilled() { return bKilled;}
		public void SetNoKilled() { bKilled = false;}

		public void RESetUpdated() { bUpdated = false; bKilled = false;}

		//(bUpdated)?判断是否更新
		public Boolean IsUpdated() 
		{ 
			if (bUpdated)
			{	// 将bUpdated 初始化，返回true
				bUpdated = false;
				return true;
			} 
			else 
				return false;
		}

		//   设为更新状态
		public void SetUpdated() { bUpdated = true; }
	}

	/**
	 * 步骤类
	 */
	public class GoMove 
	{
		StoneColor m_c;	//  黑/白
		Point m_pos;		//该步骤所在位置
		int m_n;			//记录步骤数（貌似没啥用）
		String m_comment;	//评论
		MoveResult m_mr;	//存步骤结果 

		ArrayList		m_alLabel; //标签数组 
		ArrayList		m_alMark; //标记数组

		//m_alDead存放死亡棋子
		//m_cDead存放死亡棋子的颜色 
		ArrayList		m_alDead;
		StoneColor	m_cDead;
		/**
		 * GoMove类构造函数，有两种，一种是根据玩家的指令构造，另一种是通过读文本构造
		 */
		public GoMove(int xx, int yy, StoneColor sc, int seq) 
		{
			m_pos = new Point(xx,yy);
			m_c = sc;
			m_n = seq;
			m_mr = new MoveResult();
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}

        public GoMove(String str, StoneColor cc)
        {
            if (str != null)
            {
                char cx = str[0];
                char cy = str[1];
                m_pos = new Point(0, 0);
                //计算位置
                m_pos.X = (int)((int)cx - (int)(char)'a');
                m_pos.Y = (int)((int)cy - (int)(char)'a');
                this.m_c = cc;
                m_alLabel = new ArrayList();
                m_alMark = new ArrayList();
            }
        }

		static private Point	StrToPoint(String str)
		{
			Point p = new Point(0,0);
			char cx = str[0];
			char cy = str[1];
			//通过文本的字符位置计算点位置 
			p.X = (int) ( (int)cx - (int)(char)'a');
			p.Y = (int) ( (int)cy - (int)(char)'a');
			return p;
		}


        public StoneColor Color
        { 
            get { return m_c; } 
        }

        public String Comment 
        {
            get
            {
                if (m_comment == null)
                    return string.Empty;
                else
                    return m_comment;
            }
            set
            {
                m_comment = value; 
            }
        }

		public int Seq
        {
            get { return m_n; }
            set {	m_n = value;}
        }

        public Point Point
        {
           get  { return m_pos; }
        }

        public MoveResult Result
        {
            get { return m_mr; }
            set { m_mr = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ArrayList DeadGroup
        {
            get { return m_alDead;}
            set {m_alDead = value;}
        }

        public StoneColor DeadGroupColor
        {
            get { return m_cDead; }
            set { m_cDead = value; }
        }
		
		public void AddLabel(String str) {m_alLabel.Add(StrToPoint(str));}
		
		public void AddMark(String str) {	m_alMark.Add(StrToPoint(str));}

        public ArrayList Labels
        {
            get { return m_alLabel; }
        }

        public ArrayList Marks
        {
            get { return m_alMark; }
        }
	}
	

	/**
	 * 记录结果的类MoveResult，
	 */
	public class MoveResult 
	{
        //private StoneColor color; 
        ////4种类型的结果
        //private Boolean bUpKilled;
        //private Boolean bDownKilled;
        //private Boolean bLeftKilled;
        //private Boolean bRightKilled;
        //private Boolean bSuicide;	//自杀了？
		public MoveResult() 
		{
            //bUpKilled = false;
            //bDownKilled = false;
            //bLeftKilled = false;
            //bRightKilled = false;
            //bSuicide = false;
		}
	}

	/**
	 *  棋子
     *  貌似没啥用，只是用来记录了一下颜色
	 */
    //public struct Stone 
    //{
    //    public StoneColor color; 
    //}

	/**
	 * 走棋谱的东西吧
     * GoVariation: 执行棋谱记录状态的变量吧
	 */
	public class GoVariation 
	{
	//	int m_id;			//步骤id
	//	string m_name;	//ZZZZZZZZZ ZZZZ. (ZZZZ.5, ZZZ.9, "ZZZZZ ZZZZZZ", ZZZ).步骤名字
		//ZZZZZZZZZZZZZ ZZZ;	//ZZZZZZZZZ ZZZZZZZZ ZZZZZ.	记录所走的所有步骤
		ArrayList m_moves; 
		int m_seq;			//ZZZZZZ ZZZ ZZZ ZZ ZZZZ ZZZZ.记录步骤队列的长度 
		int m_total;

		//ZZZZZZZZZZZ. 初始化构造，m_id没啥用
		public GoVariation()
		{
	//		m_id = id;
			m_moves = new ArrayList(10);
			m_seq = 0;
			m_total = 0;
		}

		public void AddAMove(GoMove gm) 
		{
            if (gm != null)
            {
                gm.Seq = m_total++;
                m_seq++;
                m_moves.Add(gm);
            }
		}

        //public void updateResult(GoMove gm) 
        //{
        //}

		public GoMove DoNext()
		{
			if (m_seq < m_total) 
			{
				return (GoMove)m_moves[m_seq++];
			} 
			else 
				return null;
		}

		public GoMove DoPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[--m_seq]);
			else 
				return null;
		}

		/*
		 *  获得上一步移动
		 */
		public GoMove PeekPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[m_seq-1]);
			else 
				return null;
		}

		public void Reset() {m_seq = 0;}
	}


	/**
	* id：编号 
	* seq: 步骤队列长度 
	*/
	struct VarStartPoint
	{
		int m_id; 
		int m_seq;
	}

	struct GameInfo 
	{
		public string gameName;
		public string playerBlack;
		public string playerWhite;
		public string rankBlack;
		public string rankWhite;
		public string result;
		public string date;
		public string km;
		public string size;
		public string comment;
        public string handicap;
        public string gameEvent;
        public string location;
        public string time;             // 时间 
        public string unknown_ff;   //未知ff 
        public string unknown_gm;
        public string unknown_vw; 
	}

	public class KeyValuePair 
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string key;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ArrayList alV;

		static private string	removeBackSlash(string strIn)
		{
			string strOut; 
			int		iSlash;

			strOut = string.Copy(strIn);
			if (strOut.Length < 2)
				return strOut;
			for (iSlash = strOut.Length-2; iSlash>=0; iSlash--)
			{
				if (strOut[iSlash] == '\\')		// && strOut[iSlash+]==']'
				{
					strOut = strOut.Remove(iSlash,1);
					if (iSlash>0)
						iSlash --;	// 去掉'\'
				}
			}
			return strOut;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:验证公共方法的参数", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String,System.Int32)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)")]
        public KeyValuePair(string kk, string vv)
		{
			this.key = string.Copy(kk);
			string strOneVal;
			int		iBegin, iEnd;
		
			//初始化队列，存放语句变量
			alV = new ArrayList(1);

			//如果是C
			if (kk!=null && kk.Equals("C"))
			{
				strOneVal = removeBackSlash(string.Copy(vv));
				//去除‘、’
				alV.Add(strOneVal);
				return;
			}
            if (vv != null)
                iBegin = vv.IndexOf("[");
            else
                iBegin = -1;
			if (iBegin == -1)	//如果不存在'['
			{
				alV.Add(vv);
				return; 
			}
			
			iBegin = 0;
			while (iBegin < vv.Length && iBegin>=0)
			{
				iEnd = vv.IndexOf("]", iBegin);
				if (iEnd > 0)
					strOneVal = vv.Substring(iBegin, iEnd-iBegin);
				else 
					strOneVal = vv.Substring(iBegin);	//取出[]中间的变量
				alV.Add(strOneVal);
				iBegin = vv.IndexOf("[", iBegin+1);
				if (iBegin > 0)
					iBegin ++;	//取出所有[]中间的变量
			}
		}
	}

	/**
	 * 执行树类
	 * 保存执行时的各种步骤以及变量状态 
	 */

	public class GoTree 
	{
		GameInfo _gi;		//保存游戏信息
		ArrayList _vars;		//变量表 
        //int _currVarId;		//当前变量ID
        //int _currVarNum;
		GoVariation _currVar;		//当前变量
		string	_stGameComment;

		// 读取棋谱文件的执行树初始化
		public GoTree(string ss)
		{
			_vars = new ArrayList(10);
            //_currVarNum = 0;
            //_currVarId = 0; 
			_currVar = new GoVariation();
			_vars.Add(_currVar);
			parseFile(ss);
		}

		//	手动玩的执行树初始化
		public GoTree()
		{
			_vars = new ArrayList(10);
            //_currVarNum = 0;
            //_currVarId = 0; 
			_currVar = new GoVariation();
			_vars.Add(_currVar);
		}

		public	string Info
		{
            get
            {
                return _gi.comment == null? string.Empty : _gi.comment;
            }
		}

		public void AddMove(GoMove gm) 
		{
			_currVar.AddAMove(gm);
		}

		/**
		 * 解析文件
		 */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.LastIndexOf(System.String,System.Int32)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String,System.Int32)")]
        Boolean parseFile(String goStr) 
		{
			int iBeg, iEnd=0; 
			while (iEnd < goStr.Length) 
			{
				if (iEnd > 0)
					iBeg = iEnd;
				else 
					iBeg = goStr.IndexOf(";", iEnd);
				iEnd = goStr.IndexOf(";", iBeg+1);
				if (iEnd < 0) //找不到 ";"
					iEnd = goStr.LastIndexOf(")", goStr.Length);		//找 ")"
				if (iBeg >= 0 && iEnd > iBeg) 
				{
					string section = goStr.Substring(iBeg+1, iEnd-iBeg-1);
					parseASection(section);
				} 
				else 
					break;
			}
			return true;
		}

        /// <ZZZZZZZ>
        /// 查找变量字符串结束的位置，
        /// 以']'结尾，
        /// 如果以"\]"结尾，则算以"]"结尾 
        /// </ZZZZZZZ>
        /// <ZZZZZ ZZZZ="ZZZ"></ZZZZZ>
        /// <ZZZZZZZ></ZZZZZZZ>
        static int findEndofValueStr(String sec)
        {
            int i = 0;
            //寻找第一个"]"而不是"\]"结尾的位置
            while (i >= 0)
            {
                i = sec.IndexOf(']', i+1);
                if (i > 0 && sec[i - 1] != '\\')
                    return i;    //返回 "]"位置. 
            }

            //如果不存在"]"，返回字符串末尾 
            return sec.Length - 1;		//返回字符串末尾位置
        }
        
        static public int FindEndofValueStrOld(String sec)
		{
			int i = 0;
            //这个函数没用到
			bool fOutside = false;
			
			for (i=0; sec!=null && i<sec.Length;i++)
			{
				if (sec!=null && sec[i] == ']')
				{
					if (i>1 && sec[i-1] != '\\') //和上一个函数没有本质上的区别
						fOutside = true;
				}
				else if (char.IsLetter(sec[i]) && fOutside && i>0)
					return i-1;
				else if (fOutside && sec[i] == '[')
					fOutside = false;
			}
            if (sec != null)
                return sec.Length - 1;
            else
                return 0;//这个函数没用到
		}

        static private string purgeCRLFSuffix(string inStr)
        {
            int iLast = inStr.Length - 1; //inStr长度

            if (iLast <= 0)
                return inStr; 

            while ((inStr[iLast] == '\r' || inStr[iLast] == '\n' || inStr[iLast] == ' '))
            {
                iLast--; 
            }
            if ((iLast+1) != inStr.Length)
                return inStr.Substring(0, iLast+1);  //取出末尾无用字符
            else
                return inStr; 
        }
 

		/**
		 * 解析句子
		 * ZZ{ZZ}这种结构的貌似没用到
		 * {}这种结构的句子貌似也没用
		 * ZZZZ: Z ZZZ ZZZ ZZZZZZZZZ ZZZZ ZZZZZZZZ ZZZZZZ, Z.Z. ZZZZZZ, ZZZZZ:  Z[ZZ][ZZ]. 一种结构的翻译
		 * 下棋语句的结构 
		 * Z ZZZZZ ZZ Z ZZZZZZ ZZZZZZZZ ZZ [ ZZZ ]. 一种结构的翻译
		 * ZZZZ: ZZZZZZZZ ( Z[ZZZZZZZZ]) ZZZ ZZZZ ZZZ ']' ZZZZZZZZZ ZZZZZZ ZZZ ZZZZZZZZ, ZZZZZ ZZ ZZZZZZZ ZZ "\]"   一种结构的翻译
		 * Z.Z.  Z[ZZZZZ ZZZZZ ZZ [4,Z\] ZZ ZZZZZ ZZZZZZ]   又是一种结构的翻译
         * 
		 */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.LastIndexOf(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)")]
        Boolean parseASection(String sec) 
		{
			int iKey = 0;
			int iValue = 0;
			int iLastValue = 0;
			KeyValuePair kv;
			ArrayList Section = new ArrayList(10);
			
            //try 
            //{
				iKey = sec.IndexOf("[");
				if (iKey < 0)
				{
					return false;
				}
                sec = purgeCRLFSuffix(sec);
 
				iValue = findEndofValueStr(sec); //寻找句子中"]"而不是"\]"的位置
				iLastValue = sec.LastIndexOf("]");
				if (iValue <= 0 || iLastValue <= 1)
				{
					return false;
				}
				sec = sec.Substring(0,iLastValue+1);
				while (iKey > 0 && iValue > iKey)//不断寻找[]
				{
					string key = sec.Substring(0,iKey);
					int iNonLetter = 0;
					while (!char.IsLetter(key[iNonLetter]) && iNonLetter < key.Length)
						iNonLetter ++;
					key = key.Substring(iNonLetter);
					//对于[]中变量的处理
					//处理
					string strValue = sec.Substring(iKey+1, iValue-iKey-1);
					//分析变量
					kv = new KeyValuePair(key, strValue);
					Section.Add(kv);
					if (iValue >= sec.Length)
						break;
					sec = sec.Substring(iValue+1);
					iKey = sec.IndexOf("[");
					if (iKey > 0)
					{
						iValue = findEndofValueStr(sec); //找"]"且不是"\]"的位置
					}
				}
            //}
            //catch
            //{
            //    return false;
            //}

			processASection(Section);
			return true;
		}


        /** 
         * 下边是解释句子
         * 解释了句子要执行
         * 执行就是让把所有操作存起来然后看 
         * 
         * 就是这样 
         */
        Boolean processASection(ArrayList arrKV) 
		{
			Boolean fMultipleMoves = false;   //判断多重移动
			GoMove gm = null; 
            
			string key, strValue;

			for (int i = 0;i<arrKV.Count;i++)
			{
				key = ((KeyValuePair)(arrKV[i])).key;
				for (int j=0; j<((KeyValuePair)(arrKV[i])).alV.Count; j++)
				{
					strValue = (string)(((KeyValuePair)(arrKV[i])).alV[j]);

                    if (key.Equals("B"))   //黑棋移动
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.Black);
                    }
                    else if (key.Equals("W"))  //白棋移动
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.White);
                    }
                    else if (key.Equals("C"))  //comment
                    {
                        //ZZZZZ.ZZZZZZ(Z>0);这应该是注释掉的语句吧
                        if (gm != null)
                            gm.Comment = strValue;
                        else	//看棋谱时的评论
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("L"))  //label
                    {
                        if (gm != null)
                            gm.AddLabel(strValue);
                        else	//看棋谱时label的储存
                            _stGameComment += strValue;
                    }

                    else if (key.Equals("M"))  //mark
                    {
                        if (gm != null)
                            gm.AddMark(strValue);
                        else	//同上 
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("AW"))		//multipleMove的白棋移动
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.White);
                    }
                    else if (key.Equals("AB"))		//multipleMove的黑棋移动
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.Black);
                    }
                    //else if (key.Equals("HA"))
                    //    _gi.handicap = (strValue);
                    //else if (key.Equals("BR"))
                    //    _gi.rankBlack = (strValue);
                    //else if (key.Equals("PB"))
                    //    _gi.playerBlack = (strValue);
                    //else if (key.Equals("PW"))
                    //    _gi.playerWhite = (strValue);
                    //else if (key.Equals("WR"))
                    //    _gi.rankWhite = (strValue);
                    //else if (key.Equals("DT"))
                    //    _gi.date = (strValue);
                    //else if (key.Equals("KM"))
                    //    _gi.km = (strValue);
                    //else if (key.Equals("RE"))
                    //    _gi.result = (strValue);
                    //else if (key.Equals("SZ"))
                    //    _gi.size = (strValue);
                    //else if (key.Equals("EV"))
                    //    _gi.gameEvent = (strValue);
                    //else if (key.Equals("PC"))
                    //    _gi.location = (strValue);
                    //else if (key.Equals("TM"))
                    //    _gi.time = (strValue);
                    //else if (key.Equals("GN"))
                    //    _gi.gameName = strValue;

                    //else if (key.Equals("FF"))
                    //    _gi.unknown_ff = (strValue);
                    //else if (key.Equals("GM"))
                    //    _gi.unknown_gm = (strValue);
                    //else if (key.Equals("VW"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("US"))
                    //    _gi.unknown_vw = (strValue);

                    //else if (key.Equals("BS"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("WS"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("ID"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("KI"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("SO"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("TR"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("LB"))
                    //    _gi.unknown_vw = (strValue);
                    //else if (key.Equals("RO"))
                    //    _gi.unknown_vw = (strValue);


                    //异常，无法识别的句子
                    else
                        System.Diagnostics.Debug.Assert(false, "unhandle key: " + key + " "+ strValue);

                    //多步操作时，向当前变量及中加入解释的移动 
                    if (fMultipleMoves)
                    {
                        _currVar.AddAMove(gm);
                    }
                }
            }

            //向当前变量及中加入解释的移动 
            if (!fMultipleMoves && gm != null)
            {
                _currVar.AddAMove(gm);
            }
			return true;
		} 

		public GoMove DoPrev() 
		{
			return _currVar.DoPrev();
		}

		public GoMove PeekPrev() 
		{
			return _currVar.PeekPrev();
		}

		public GoMove DoNext() 
		{
			return _currVar.DoNext();
		}

        //public void updateResult(GoMove gm) 
        //{
        //    _currVar.updateResult(gm);
        //}
		
		public void Reset()
		{
            //_currVarNum = 0;
            //_currVarId = 0; 
			_currVar.Reset();
		}
		static public void RewindToStart()
		{

		}
	} //终于写完了
}
