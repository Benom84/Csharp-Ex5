using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Exercise05
{
    public delegate void ClickSetUserPreferences(bool i_IsPlayingAgainstComputer, int i_BoardSize);

    class FormGameSettings : Form
    {
        public event ClickSetUserPreferences SetUserPreferences;
        private const string k_Title = "Othello - Game Settings";
        private const string k_PlayAgainstComputerDescription = "Play against the computer";
        private const string k_PlayAgainstFriendDescription = "Play against your friend";
        private const string k_BoardSizeButtonDescription = "Board Size: {0}x{0} (click to increase)";
        private const int k_MinBoardSize = 6;
        private const int k_MaxBoardSize = 12;
        private const bool k_PlayingAgainstComputer = true;
        private Button m_ButtonChangeBoardSize;
        private Button m_ButtonPlayAgainstComputer;
        private Button m_ButtonPlayAgainstFriend;
        private int m_BoardSizeSelected;

        
        public FormGameSettings()
        {
            this.Text = k_Title;
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.Size = new Size(290, 180);
            this.StartPosition = FormStartPosition.CenterScreen;
            m_BoardSizeSelected = k_MinBoardSize;
            initControls();

           
        }

        private void initControls() {
            m_ButtonChangeBoardSize = new Button();
            m_ButtonChangeBoardSize.Text = String.Format(k_BoardSizeButtonDescription, m_BoardSizeSelected);
            m_ButtonChangeBoardSize.Location = new Point(10, 10);
            m_ButtonChangeBoardSize.Width = 250;
            m_ButtonChangeBoardSize.Height = 40;
            m_ButtonChangeBoardSize.Click += new EventHandler(m_ButtonChangeBoardSizeClick);

            m_ButtonPlayAgainstComputer = new Button();
            m_ButtonPlayAgainstComputer.Text = k_PlayAgainstComputerDescription;
            m_ButtonPlayAgainstComputer.Location = new Point(10, 80);
            m_ButtonPlayAgainstComputer.Width = 120;
            m_ButtonPlayAgainstComputer.Height = 40;
            m_ButtonPlayAgainstComputer.Click += new EventHandler(m_ButtonPlayAgainstComputerClick);

            m_ButtonPlayAgainstFriend = new Button();
            m_ButtonPlayAgainstFriend.Text = k_PlayAgainstFriendDescription;
            m_ButtonPlayAgainstFriend.Location = new Point(140, 80);
            m_ButtonPlayAgainstFriend.Width = 120;
            m_ButtonPlayAgainstFriend.Height = 40;
            m_ButtonPlayAgainstFriend.Click += new EventHandler(m_ButtonPlayAgainstFriendClick);

            this.Controls.Add(m_ButtonChangeBoardSize);
            this.Controls.Add(m_ButtonPlayAgainstComputer);
            this.Controls.Add(m_ButtonPlayAgainstFriend);
        }

        private void m_ButtonPlayAgainstFriendClick(object sender, EventArgs e)
        {
            if (SetUserPreferences != null)
            {
                SetUserPreferences.Invoke(!k_PlayingAgainstComputer, m_BoardSizeSelected);
                this.Close();
            }
        }

        private void m_ButtonPlayAgainstComputerClick(object sender, EventArgs e)
        {
            if (SetUserPreferences != null)
            {
                SetUserPreferences.Invoke(k_PlayingAgainstComputer, m_BoardSizeSelected);
                this.Close();
            }
        }

        private void m_ButtonChangeBoardSizeClick(object sender, EventArgs e)
        {
            m_BoardSizeSelected = Math.Min(k_MaxBoardSize, m_BoardSizeSelected + 2);
            m_ButtonChangeBoardSize.Text = String.Format(k_BoardSizeButtonDescription, m_BoardSizeSelected);
        }
    }
}
