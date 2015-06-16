using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Exercise05
{
    public class FormGameBoard : Form
    {
        private const string k_Title = "Othello - {0}'s Turn";
        private const int k_SlotSize = 50;
        private const int k_SlotSpacingFromEachOther = 5;
        private const int k_SlotSpacingFromBorder = 15;
        private const int k_SlotTotalSpacing = k_SlotSize + k_SlotSpacingFromEachOther;
        private const bool k_EnableButton = true;
        private const string k_EndMenuText = "{0} ({1}/{2}) ({3}/{4})\nWould you like another round?";
        private readonly Color r_AvailableMoveColor = Color.SpringGreen;
        private readonly Color r_LockedMoveColor = Color.LightGray;
        private readonly Color r_Player1BackgroundColor = Color.Black;
        private readonly Color r_Player2BackgroundColor = Color.White;
        private readonly Color r_Player1TextColor = Color.White;
        private readonly Color r_Player2TextColor = Color.Black;
        private Othello m_OthelloGame;
        private Button[,] m_GameSlotsButtons;
        private Othello.ePlayer m_ActivePlayer = Othello.ePlayer.Player1;
        private Dictionary<Button, Point> m_CurrentValidMovesToPoint;
        private bool r_IsPlayingAgainstComputer;
        private int m_TotalPlayer1Wins;
        private int m_TotalPlayer2Wins;
        private bool m_MatchEnded;

        public FormGameBoard(int i_BoardSize, bool i_IsPlayingAgainstComputer)
        {
            this.Text = String.Format(k_Title, playerToColor(m_ActivePlayer));
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size((k_SlotSize + k_SlotSpacingFromEachOther) * (i_BoardSize + 1), (k_SlotSize + k_SlotSpacingFromEachOther + 2) * (i_BoardSize + 1));
            r_IsPlayingAgainstComputer = i_IsPlayingAgainstComputer;
            m_GameSlotsButtons = new Button[i_BoardSize, i_BoardSize];
            initBoardSlotsArea();
            m_MatchEnded = false;
            m_OthelloGame = new Othello(i_BoardSize, changeSlotControllingPlayer);
            nextTurn();
        }

        private void markAvailableMovesSlots(Othello.ePlayer i_activePlayer)
        {
            List<int> allActivePlayerMoves = m_OthelloGame.GetLegalMovesForPlayer(i_activePlayer);
            generateLegalMovesMap(allActivePlayerMoves);

            foreach (Button button in m_CurrentValidMovesToPoint.Keys)
            {
                button.BackColor = r_AvailableMoveColor;
                button.Enabled = k_EnableButton;
                button.Click += button_Click;
            }
        }

        void button_Click(object sender, EventArgs e)
        {
            Button selectedButton = (Button)sender;
            Point buttonIndices;
            int buttonRow;
            int buttonCol;

            m_CurrentValidMovesToPoint.TryGetValue(selectedButton, out buttonIndices);
            buttonRow = buttonIndices.X + 1;
            buttonCol = buttonIndices.Y + 1;
            //if (buttonRow == 1 && buttonCol == 1)
            //    Console.WriteLine("On 1,1 Player is: " + m_ActivePlayer);

            Console.WriteLine(buttonIndices.X + "," + buttonIndices.Y + " Player is: " + m_ActivePlayer);

            if (m_OthelloGame.PerformMove(m_ActivePlayer, buttonRow, buttonCol))
            {
                m_ActivePlayer = m_ActivePlayer == Othello.ePlayer.Player1 ? Othello.ePlayer.Player2 : Othello.ePlayer.Player1;
                nextTurn();
                while (m_ActivePlayer == Othello.ePlayer.Player2 && r_IsPlayingAgainstComputer && !m_MatchEnded)
                {
                    m_OthelloGame.PerformAutomaticMove(m_ActivePlayer);
                    Console.WriteLine("Active player is: " + m_ActivePlayer);
                    m_ActivePlayer = Othello.ePlayer.Player1;
                    nextTurn();
                }
            }

        }

        private void nextTurn()
        {
            clearMarkedAvailableMovesSlots();
            this.Text = String.Format(k_Title, playerToColor(m_ActivePlayer));
            this.Refresh();
            if (setPlayerToPlayNext())
            {
                markAvailableMovesSlots(m_ActivePlayer);
            }
            else
            {
                matchFinished();
            }

        }

        private void matchFinished()
        {
            int player1Points = m_OthelloGame.GetPlayerScore(Othello.ePlayer.Player1);
            int player2Points = m_OthelloGame.GetPlayerScore(Othello.ePlayer.Player2);
            string gameWinner;

            m_MatchEnded = true;
            if (player1Points > player2Points)
            {
                m_TotalPlayer1Wins++;
                gameWinner = "Black Won!!";
            }
            else if (player2Points > player1Points)
            {
                m_TotalPlayer2Wins++;
                gameWinner = "White Won!!";
            }
            else
            {
                gameWinner = "It's a tie!!";
            }

            DialogResult result = MessageBox.Show(string.Format(k_EndMenuText, gameWinner, player1Points, player2Points, m_TotalPlayer1Wins, m_TotalPlayer2Wins), "Othello", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                m_ActivePlayer = Othello.ePlayer.Player1;
                m_MatchEnded = false;
                initBoardSlotsArea();
                m_OthelloGame.RestartGame();
                nextTurn();
            }
            else
            {
                this.Close();
            }
        }

        private void changeSlotControllingPlayer(Othello.ePlayer i_PlayerToControl, int i_SlotRow, int i_SlotCol)
        {
            Color slotBackgroundColor = i_PlayerToControl == Othello.ePlayer.Player1 ? r_Player1BackgroundColor : r_Player2BackgroundColor;
            Color slotTextColor = i_PlayerToControl == Othello.ePlayer.Player1 ? r_Player1TextColor : r_Player2TextColor;
            m_GameSlotsButtons[i_SlotRow, i_SlotCol].BackColor = slotBackgroundColor;
            m_GameSlotsButtons[i_SlotRow, i_SlotCol].ForeColor = slotTextColor;
            m_GameSlotsButtons[i_SlotRow, i_SlotCol].Text = "O";
        }

        private bool setPlayerToPlayNext()
        {
            bool oneOfThePlayersHasValidMoves = true;
            if (!m_OthelloGame.LegalMoveExists(m_ActivePlayer))
            {
                m_ActivePlayer = m_ActivePlayer == Othello.ePlayer.Player1 ? Othello.ePlayer.Player2 : Othello.ePlayer.Player1;
                if (!m_OthelloGame.LegalMoveExists(m_ActivePlayer))
                {
                    oneOfThePlayersHasValidMoves = false;
                }
            }

            return oneOfThePlayersHasValidMoves;
        }

        private void clearMarkedAvailableMovesSlots()
        {
            if (m_CurrentValidMovesToPoint != null)
            {
                foreach (Button button in m_CurrentValidMovesToPoint.Keys)
                {
                    button.BackColor = button.BackColor == r_AvailableMoveColor ? r_LockedMoveColor : button.BackColor;
                    button.Enabled = !k_EnableButton;
                    button.Click -= button_Click;
                }
            }
        }

        private string playerToColor(Othello.ePlayer i_Player)
        {
            return i_Player == Othello.ePlayer.Player1 ? "Black" : "White";
        }

        private void initBoardSlotsArea()
        {
            foreach (Button button in m_GameSlotsButtons)
            {
                if (button != null)
                {
                    this.Controls.Remove(button);
                }
            }
            
            for (int i = 0; i < m_GameSlotsButtons.GetLength(0); i++)
            {
                for (int j = 0; j < m_GameSlotsButtons.GetLength(0); j++)
                {
                    m_GameSlotsButtons[i, j] = new Button();
                    m_GameSlotsButtons[i, j].Size = new Size(k_SlotSize, k_SlotSize);
                    m_GameSlotsButtons[i, j].Location = new Point(k_SlotTotalSpacing * i + k_SlotSpacingFromBorder, k_SlotTotalSpacing * j + k_SlotSpacingFromBorder);
                    m_GameSlotsButtons[i, j].Enabled = false;
                    m_GameSlotsButtons[i, j].BackColor = r_LockedMoveColor;
                    this.Controls.Add(m_GameSlotsButtons[i, j]);
                }
            }

            //markPlayerSlots(Othello.ePlayer.Player1);
            //markPlayerSlots(Othello.ePlayer.Player2);
        }

        private void markPlayerSlots(Othello.ePlayer i_Player)
        {
            List<int> allCurrentPlayerSlots = m_OthelloGame.GetCurrentGameSlotsForPlayer(i_Player);
            List<Button> allCurrentPlayerButtons = getButtonsFromSlotsList(allCurrentPlayerSlots);
            Color slotBackgroundColor = i_Player == Othello.ePlayer.Player1 ? Color.Black : Color.White;
            Color slotTextColor = i_Player == Othello.ePlayer.Player1 ? Color.White : Color.Black;

            foreach (Button button in allCurrentPlayerButtons)
            {
                button.BackColor = slotBackgroundColor;
                button.ForeColor = slotTextColor;
                button.Text = "O";
            }
        }

        private List<Button> getButtonsFromSlotsList(List<int> allCurrentPlayerSlots)
        {
            List<Button> currentPlayerButtons = new List<Button>();

            for (int i = 0; i < allCurrentPlayerSlots.Count; i += 2)
            {
                int buttonRow = allCurrentPlayerSlots[i];
                int buttonCol = allCurrentPlayerSlots[i + 1];
                currentPlayerButtons.Add(m_GameSlotsButtons[buttonRow, buttonCol]);
            }

            return currentPlayerButtons;
        }

        private void generateLegalMovesMap(List<int> allCurrentPlayerSlots)
        {
            m_CurrentValidMovesToPoint = new Dictionary<Button, Point>();

            for (int i = 0; i < allCurrentPlayerSlots.Count; i += 2)
            {
                int buttonRow = allCurrentPlayerSlots[i];
                int buttonCol = allCurrentPlayerSlots[i + 1];
                Point buttonIndices = new Point(buttonRow, buttonCol);
                m_CurrentValidMovesToPoint.Add(m_GameSlotsButtons[buttonRow, buttonCol], buttonIndices);
            }
        }

    }
}
