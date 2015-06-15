using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise05
{
    public delegate void ChangeSlotColor(Othello.ePlayer i_PlayerToControl, int i_slotRow, int i_slotCol);
    
    public class Othello
    {
        public enum ePlayer
        {
            Player1, Player2
        }

        private enum eSlotColor
        {
            Empty, White, Black
        }

        private class GameSlot
        {
            private int m_Row;
            private int m_Col;
            private eSlotColor m_SlotColor;
            private List<Direction> m_WhiteValidMovesDirections;
            private List<Direction> m_BlackValidMovesDirections;

            public int Row
            {
                get
                {
                    return m_Row;
                }
            }

            public int Col
            {
                get
                {
                    return m_Col;
                }
            }

            public eSlotColor Color
            {
                get
                {
                    return m_SlotColor;
                }

                set
                {
                    m_SlotColor = value;
                }
            }

            public List<Direction> WhiteValidMovesDirections
            {
                get
                {
                    return m_WhiteValidMovesDirections;
                }

                set
                {
                    m_WhiteValidMovesDirections = value;
                }
            }

            public List<Direction> BlackValidMovesDirections
            {
                get
                {
                    return m_BlackValidMovesDirections;
                }

                set
                {
                    m_BlackValidMovesDirections = value;
                }
            }

            public GameSlot(int i_Row, int i_Col, eSlotColor i_SlotColor)
            {
                m_Row = i_Row;
                m_Col = i_Col;
                m_SlotColor = i_SlotColor;
                m_WhiteValidMovesDirections = new List<Direction>();
                m_BlackValidMovesDirections = new List<Direction>();
            }

            public GameSlot CloneSlot()
            {
                GameSlot clonedSlot = new GameSlot(m_Row, m_Col, m_SlotColor);
                clonedSlot.m_SlotColor = m_SlotColor;

                foreach (Direction direction in m_WhiteValidMovesDirections)
                {
                    clonedSlot.m_WhiteValidMovesDirections.Add(direction);
                }

                foreach (Direction direction in m_BlackValidMovesDirections)
                {
                    clonedSlot.m_BlackValidMovesDirections.Add(direction);
                }

                return clonedSlot;
            }
        }

        private struct Direction
        {
            private int m_RowDirection;
            private int m_ColDirection;

            public int RowDirection
            {
                get
                {
                    return m_RowDirection;
                }

                set
                {
                    m_RowDirection = value;
                }
            }

            public int ColDirection
            {
                get
                {
                    return m_ColDirection;
                }

                set
                {
                    m_ColDirection = value;
                }
            }

            public Direction(int i_rowDirection, int i_colDirection)
            {
                m_RowDirection = i_rowDirection;
                m_ColDirection = i_colDirection;
            }
        }

        public event ChangeSlotColor OnChangeSlotColor;
        private GameSlot[,] m_GameBoard;
        private List<GameSlot> m_WhiteValidMoves;
        private List<GameSlot> m_BlackValidMoves;
        private List<GameSlot> m_PossibleMoveSlots;
        private int m_WhitePiecesCount;
        private int m_BlackPiecesCount;
        private const int k_DefaultBoardSize = 8;
        private float k_CornerStrengthWeight = 100f;
        private float k_NumberOfPiecesStrengthWeight = 10f;
        private float k_NumberOfPositionStrengthWeight = 10f;
        private const int k_MovesToCheckAhead = 5;
        private bool v_ReplacingExistingPieces = true;
        private bool v_IsMaximizing = true;

        /**
        * Create an Othello board with default size
        */
        public Othello()
            : this(k_DefaultBoardSize)
        {
        }

        /*
         * Create an Othello board with a specific size.
         * i_BoardSize - The size of the board. Must be an even number between 4 and 26
         */
        public Othello(int i_BoardSize)
        {
            if ((i_BoardSize < 4) || (i_BoardSize % 2 == 1))
            {
                // Since we can't use errors yet set the size to default
                i_BoardSize = k_DefaultBoardSize;
            }

            m_GameBoard = new GameSlot[i_BoardSize, i_BoardSize];
            RestartGame();
        }

        public Othello(int i_BoardSize, ChangeSlotColor i_ActionOnSlotChangeColor)
        {
            OnChangeSlotColor += i_ActionOnSlotChangeColor;
            if ((i_BoardSize < 4) || (i_BoardSize % 2 == 1))
            {
                // Since we can't use errors yet set the size to default
                i_BoardSize = k_DefaultBoardSize;
            }

            m_GameBoard = new GameSlot[i_BoardSize, i_BoardSize];
            RestartGame();
        }

        /**
        * Sets the game to it's initial state
        */
        public void RestartGame()
        {
            int middleRow = m_GameBoard.GetLength(0) / 2;
            m_BlackValidMoves = new List<GameSlot>();
            m_WhiteValidMoves = new List<GameSlot>();
            m_PossibleMoveSlots = new List<GameSlot>();
            m_BlackPiecesCount = 0;
            m_WhitePiecesCount = 0;

            // Clear the board
            for (int i = 0; i < m_GameBoard.GetLength(0); i++)
            {
                for (int j = 0; j < m_GameBoard.GetLength(0); j++)
                {
                    m_GameBoard[i, j] = new GameSlot(i, j, eSlotColor.Empty);
                }
            }

            // Set the middle pieces to a start position
            putPieceOnEmptySlot(middleRow, middleRow, eSlotColor.White);
            putPieceOnEmptySlot(middleRow - 1, middleRow - 1, eSlotColor.White);
            putPieceOnEmptySlot(middleRow, middleRow - 1, eSlotColor.Black);
            putPieceOnEmptySlot(middleRow - 1, middleRow, eSlotColor.Black);

            // Retrieve the initial valid moves for each color
            m_WhiteValidMoves = findValidMovesForColor(eSlotColor.White);
            m_BlackValidMoves = findValidMovesForColor(eSlotColor.Black);
        }

        /*
         * Check if a player has a legal move for the next turn
         * i_PlayerToCheck - The player to check if there are legal moves in the next turn
         * Returns - Whether a legal move exists
         */
        public bool LegalMoveExists(ePlayer i_PlayerToCheck)
        {
            bool legalMoveExists = false;

            if (i_PlayerToCheck == ePlayer.Player1)
            {
                legalMoveExists = m_BlackValidMoves.Count > 0;
            }
            else
            {
                legalMoveExists = m_WhiteValidMoves.Count > 0;
            }

            return legalMoveExists;
        }

        public List<int> GetLegalMovesForPlayer(ePlayer i_PlayerToGetMovesFor)
        {
            List<int> listOfLegalMoves = new List<int>();
            List<GameSlot> legalMovesOfPlayer = i_PlayerToGetMovesFor == ePlayer.Player1 ? m_BlackValidMoves : m_WhiteValidMoves;

            foreach (GameSlot gameSlot in legalMovesOfPlayer)
            {
                listOfLegalMoves.Add(gameSlot.Row);
                listOfLegalMoves.Add(gameSlot.Col);
            }

            return listOfLegalMoves;
        }

        /*
         * Performs a move for a given player
         * i_PlayerPlaying - The player making the move
         * i_Row - The row to place the player's piece
         * i_Col - The column to place the player's piece
         * Returns - True if the move is legal, False if it is not
         */

        public List<int> GetCurrentGameSlotsForPlayer(ePlayer i_Player)
        {
            List<int> playerCurrentGameSlotsList = new List<int>();
            eSlotColor playerColor = i_Player == ePlayer.Player1 ? eSlotColor.Black : eSlotColor.White;

            foreach (GameSlot gameSlot in m_GameBoard)
            {
                if (gameSlot.Color == playerColor)
                {
                    playerCurrentGameSlotsList.Add(gameSlot.Row);
                    playerCurrentGameSlotsList.Add(gameSlot.Col);
                }
            }

            return playerCurrentGameSlotsList;
        }

        public bool PerformMove(ePlayer i_PlayerPlaying, int i_Row, int i_Col)
        {
            eSlotColor playerColor = (i_PlayerPlaying == ePlayer.Player1) ? eSlotColor.Black : eSlotColor.White;

            // Translate the indices from game indices to object indices
            i_Row--;
            i_Col--;

            return performMoveOnBoard(playerColor, i_Row, i_Col);
        }

        /*
         * Performs a move for a given color
         * i_PlayerColor - The color of the player performing the move
         * i_Row - The row to place the player's piece
         * i_Col - The column to place the player's piece
         * Returns - True if the move is legal, False if it is not
         */
        private bool performMoveOnBoard(eSlotColor i_PlayerColor, int i_Row, int i_Col)
        {
            bool moveIsLegal = false;
            bool indicesAreLegal = false;
            List<GameSlot> availableMoves = null;

            indicesAreLegal = indexInGameRange(i_Row) && indexInGameRange(i_Col);

            if (indicesAreLegal)
            {
                availableMoves = (i_PlayerColor == eSlotColor.Black) ? m_BlackValidMoves : m_WhiteValidMoves;
                moveIsLegal = availableMoves.Remove(m_GameBoard[i_Row, i_Col]);
            }

            if (moveIsLegal)
            {
                putPieceOnEmptySlot(i_Row, i_Col, i_PlayerColor);
                changeOppositeColorPiecesAfterMove(i_Row, i_Col, i_PlayerColor);
                m_WhiteValidMoves = findValidMovesForColor(eSlotColor.White);
                m_BlackValidMoves = findValidMovesForColor(eSlotColor.Black);
            }

            return moveIsLegal;
        }

        /*
         * Get the player current score based on the number of pieces he has on the board
         * i_Player - The player which it's score will return
         * Returns - The score of the player given
         */
        public int GetPlayerScore(ePlayer i_Player)
        {
            return (i_Player == ePlayer.Player1) ? m_BlackPiecesCount : m_WhitePiecesCount;
        }

        /*
         * Make an automatic move for the player given
         * i_PlayerPlaying - The player that the move will perform on it's behalf
         * Returns - True if a move was performed, False if not
         */
        public bool PerformAutomaticMove(ePlayer i_PlayerPlaying)
        {
            List<GameSlot> playerMoves;
            bool moveWasPerformed = false;
            eSlotColor playerColor = eSlotColor.Black;

            if (i_PlayerPlaying == ePlayer.Player1)
            {
                playerMoves = m_BlackValidMoves;
            }
            else
            {
                playerMoves = m_WhiteValidMoves;
                playerColor = eSlotColor.White;
            }

            if (playerMoves.Count > 0)
            {
                GameSlot bestMove = findBestMoveForColor(playerColor);
                if (bestMove != null)
                {
                    moveWasPerformed = performMoveOnBoard(playerColor, bestMove.Row, bestMove.Col);
                }
            }

            return moveWasPerformed;
        }

        /*
         * Find's all valid moves on the board for a specific color
         * i_ColorToGetMoves - The color the valid moves are searched for
         * Returns - A list of game slots that are valid moves
         */
        private List<GameSlot> findValidMovesForColor(eSlotColor i_ColorToGetMoves)
        {
            List<GameSlot> validMovesFound = new List<GameSlot>();

            foreach (GameSlot gameSlot in m_PossibleMoveSlots)
            {
                if (checkIfSlotIsValidMoveForColor(gameSlot, i_ColorToGetMoves))
                {
                    validMovesFound.Add(gameSlot);
                }
            }

            return validMovesFound;
        }

        /*
         * Changes the appropriate opposite color pieces when a piece is put on the board
         * i_Row - The row the piece was put on
         * i_Col - The column the piece was put on
         * i_PlayerColor - The color of the move made
         */
        private void changeOppositeColorPiecesAfterMove(int i_Row, int i_Col, eSlotColor i_PlayerColor)
        {
            eSlotColor oppositeColor = (i_PlayerColor == eSlotColor.Black) ? eSlotColor.White : eSlotColor.Black;
            int piecesChangeCount = 0;
            List<Direction> validColorDirectionMoves = (i_PlayerColor == eSlotColor.Black) ? m_GameBoard[i_Row, i_Col].BlackValidMovesDirections : m_GameBoard[i_Row, i_Col].WhiteValidMovesDirections;

            foreach (Direction direction in validColorDirectionMoves)
            {
                int changedPieceRow = i_Row + direction.RowDirection;
                int changedPieceCol = i_Col + direction.ColDirection;

                while (indexInGameRange(changedPieceRow) && indexInGameRange(changedPieceCol) && m_GameBoard[changedPieceRow, changedPieceCol].Color == oppositeColor)
                {
                    m_GameBoard[changedPieceRow, changedPieceCol].Color = i_PlayerColor;
                    triggerSlotColorChange(i_PlayerColor, changedPieceRow, changedPieceCol);
                    piecesChangeCount++;
                    changedPieceRow += direction.RowDirection;
                    changedPieceCol += direction.ColDirection;
                }
            }

            updateColorPieceCount(piecesChangeCount, i_PlayerColor, v_ReplacingExistingPieces);
        }

        /*
         * Updates the number of pieces that each color holds on the board
         * i_ChangeInCount - The change in the piece count
         * i_ColorToAddCount - The color that the change will add to it's count
         * i_ReplacedExistingPieces - Whether the change was on an empty slot of on an opposite color piece
         */
        private void updateColorPieceCount(int i_ChangeInCount, eSlotColor i_ColorToAddCount, bool i_ReplacedExistingPieces)
        {
            if (i_ColorToAddCount == eSlotColor.Black)
            {
                m_BlackPiecesCount += i_ChangeInCount;
                if (i_ReplacedExistingPieces)
                {
                    m_WhitePiecesCount -= i_ChangeInCount;
                }
            }
            else
            {
                m_WhitePiecesCount += i_ChangeInCount;
                if (i_ReplacedExistingPieces)
                {
                    m_BlackPiecesCount -= i_ChangeInCount;
                }
            }
        }

        /*
         * Put a color piece on an empty slot on the board
         * i_Row - The row to put the piece on
         * i_Col - The column to put the piece on
         * i_PieceColor - The color of the piece to put
         */
        private void putPieceOnEmptySlot(int i_Row, int i_Col, eSlotColor i_PieceColor)
        {
            m_GameBoard[i_Row, i_Col].Color = i_PieceColor;
            m_PossibleMoveSlots.Remove(m_GameBoard[i_Row, i_Col]);
            updateColorPieceCount(1, i_PieceColor, !v_ReplacingExistingPieces);
            triggerSlotColorChange(i_PieceColor, i_Row, i_Col);
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int rowToCheck = i_Row + i;
                    int colToCheck = i_Col + j;
                    if (indexInGameRange(rowToCheck) && indexInGameRange(colToCheck) && (m_GameBoard[rowToCheck, colToCheck].Color == eSlotColor.Empty))
                    {
                        if (!m_PossibleMoveSlots.Contains(m_GameBoard[rowToCheck, colToCheck]))
                        {
                            m_PossibleMoveSlots.Add(m_GameBoard[rowToCheck, colToCheck]);
                        }
                    }
                }
            }
        }

        private void triggerSlotColorChange(eSlotColor i_PieceColor, int i_Row, int i_Col)
        {
            if (OnChangeSlotColor != null)
            {
                Othello.ePlayer currentPlayer = i_PieceColor == eSlotColor.Black ? ePlayer.Player1 : ePlayer.Player2;
                OnChangeSlotColor(currentPlayer, i_Row, i_Col);
            }
        }

        /*
         * Checks if a game slot can be a valid move for a given color
         * i_SlotToCheck - The slot to check
         * i_ColorToCheck - The color to check
         * Returns - True if the game slot is a valid move, False if not
         */
        private bool checkIfSlotIsValidMoveForColor(GameSlot i_SlotToCheck, eSlotColor i_ColorToCheck)
        {
            bool isValidMove = false;
            bool slotIsEmpty = false;
            List<Direction> colorValidMovesDirection = (i_ColorToCheck == eSlotColor.Black) ? i_SlotToCheck.BlackValidMovesDirections : i_SlotToCheck.WhiteValidMovesDirections;

            // Check that slot is empty
            slotIsEmpty = i_SlotToCheck.Color == eSlotColor.Empty;

            // Clear current available directions
            colorValidMovesDirection.Clear();

            if (slotIsEmpty)
            {
                // Check all directions
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if ((i == 0) && (j == 0))
                        {
                            continue;
                        }

                        if (checkValidMoveInDirection(i, j, i_SlotToCheck, i_ColorToCheck))
                        {
                            isValidMove = true;
                            colorValidMovesDirection.Add(new Direction(i, j));
                        }
                    }
                }
            }

            return isValidMove;
        }

        /*
         * Check if there is a legal move in a specific direction of the board
         * i_VerticalDiff - The vertical direction of the check
         * i_HorizontalDiff - The horizontal direction of the check
         * i_SlotToCheck - The game slot being checked
         * i_ColorToCheck - The color that will perform the move
         * Returns - True if there is a legal move in that direction, False if not
         */
        private bool checkValidMoveInDirection(int i_VerticalDiff, int i_HorizontalDiff, GameSlot i_SlotToCheck, eSlotColor i_ColorToCheck)
        {
            int rowToCheck = i_SlotToCheck.Row + i_VerticalDiff;
            int colToCheck = i_SlotToCheck.Col + i_HorizontalDiff;
            eSlotColor oppositeColor = (i_ColorToCheck == eSlotColor.Black) ? eSlotColor.White : eSlotColor.Black;

            // It is a valid move in this direction only if we are in the game area and we touch the opposite color
            bool isValidMove = indexInGameRange(rowToCheck) && indexInGameRange(colToCheck) && (m_GameBoard[rowToCheck, colToCheck].Color == oppositeColor);

            if (isValidMove)
            {
                bool reachedSameColorPiece = false;
                rowToCheck += i_VerticalDiff;
                colToCheck += i_HorizontalDiff;
                while (indexInGameRange(rowToCheck) && indexInGameRange(colToCheck))
                {
                    if (m_GameBoard[rowToCheck, colToCheck].Color == i_ColorToCheck)
                    {
                        reachedSameColorPiece = true;
                        break;
                    }
                    else if (m_GameBoard[rowToCheck, colToCheck].Color == eSlotColor.Empty)
                    {
                        break;
                    }

                    rowToCheck += i_VerticalDiff;
                    colToCheck += i_HorizontalDiff;
                }

                isValidMove = reachedSameColorPiece;
            }

            return isValidMove;
        }

        /*
         * Checks if a given index is in the game bounds
         * i_Index - The index to check
         * Returns - True if the given index is inbound, False if not
         */
        private bool indexInGameRange(int i_Index)
        {
            return (i_Index >= 0) && (i_Index < m_GameBoard.GetLength(0));
        }

        /*
         * Generates a string representation of the current state of the board
         * Returns - string representation of the board
         */
        public override string ToString()
        {
            StringBuilder gameBoardString = new StringBuilder();
            string header;
            string lineSeperator;

            // Create the line seperator
            lineSeperator = createLineSeperator();

            // Create the header
            header = createHeader();

            // Prepare the game board string
            gameBoardString.Append("   ");
            gameBoardString.AppendLine(header.ToString());
            gameBoardString.AppendLine(lineSeperator.ToString());
            for (int i = 0; i < m_GameBoard.GetLength(0); i++)
            {
                gameBoardString.Append(createGameBoardRow(i));
                gameBoardString.Append(System.Environment.NewLine);
                gameBoardString.AppendLine(lineSeperator.ToString());
            }

            return gameBoardString.ToString();
        }

        /*
         * Generates a string of a line seperator based on the board's size
         */
        private string createLineSeperator()
        {
            StringBuilder lineSeperator = new StringBuilder();

            lineSeperator.Append("   ");
            for (int i = 0; i < m_GameBoard.GetLength(0); i++)
            {
                lineSeperator.Append("====");
            }

            lineSeperator.Append("=");

            return lineSeperator.ToString();
        }

        /*
         * Generates a string of the board header based on the board's size
         */
        private string createHeader()
        {
            StringBuilder header = new StringBuilder();

            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = 0; i < m_GameBoard.GetLength(0); i++)
            {
                header.Append("  ");
                header.Append(alphabet[i]);
                header.Append(" ");
            }

            return header.ToString();
        }

        /*
         * Generates a string of a row in the board
         * i_RowNumber - The raw to generate
         */
        private string createGameBoardRow(int i_RowNumber)
        {
            StringBuilder gameBoardLine = new StringBuilder();

            gameBoardLine.Append(i_RowNumber + 1);
            gameBoardLine.Append("  | ");
            for (int j = 0; j < m_GameBoard.GetLength(0); j++)
            {
                switch (m_GameBoard[i_RowNumber, j].Color)
                {
                    case eSlotColor.White:
                        gameBoardLine.Append("O");
                        break;
                    case eSlotColor.Black:
                        gameBoardLine.Append("X");
                        break;
                    default:
                        gameBoardLine.Append(" ");
                        break;
                }

                gameBoardLine.Append(" | ");
            }

            return gameBoardLine.ToString();
        }

        /*
         * Calculates how strong the current color position is
         * i_PositionColor - The color to check it's position's strength
         * Returns - The strength of the position
         */
        private float calculateCurrentPositionStrength(eSlotColor i_PositionColor)
        {
            eSlotColor oppositeColor = (i_PositionColor == eSlotColor.Black) ? eSlotColor.White : eSlotColor.Black;
            float cornerStrength = calculateTheColorCornerStrength(i_PositionColor, oppositeColor) * k_CornerStrengthWeight;
            float numberOfPiecesStrength = calculateTheNumberOfPiecesStrength(i_PositionColor) * k_NumberOfPiecesStrengthWeight;
            float numberOfPositionsStrength = calculateTheNumberOfPossibleMovesStrength(i_PositionColor) * k_NumberOfPositionStrengthWeight;

            return cornerStrength + numberOfPiecesStrength + numberOfPositionsStrength;
        }

        /*
         * Calculate how strong the mobiligy factor of the color on the game board
         */
        private float calculateTheNumberOfPossibleMovesStrength(eSlotColor i_PositionColor)
        {
            float numberOfPossibleMovesStrength = 0;
            int numberOfPlayerPossibleMoves = (i_PositionColor == eSlotColor.Black) ? m_BlackValidMoves.Count : m_WhiteValidMoves.Count;
            int numberOfOppositionPossibleMoves = (i_PositionColor == eSlotColor.White) ? m_BlackValidMoves.Count : m_WhiteValidMoves.Count;

            if (numberOfOppositionPossibleMoves + numberOfPlayerPossibleMoves != 0)
            {
                numberOfPossibleMovesStrength = (numberOfPlayerPossibleMoves - numberOfOppositionPossibleMoves) * 1.0f / (numberOfPlayerPossibleMoves + numberOfOppositionPossibleMoves);
            }

            return numberOfPossibleMovesStrength;
        }

        /*
         * Calculates how strong the hold of the color on the game board corners
         * i_PositionColor - The color to check it's strength in the corners
         * Returns - The strength of the color's hold in the corners
         */
        private float calculateTheColorCornerStrength(eSlotColor i_PositionColor, eSlotColor i_OppositeColor)
        {

            int numberOfSameColorPiecesInCorner = 0;
            int numberOfOppositeColorPiecesInCorner = 0;
            float ratioOfPiecesInCorner = 0;

            numberOfSameColorPiecesInCorner = countPiecesOfSameColorInCorner(i_PositionColor);
            numberOfOppositeColorPiecesInCorner = countPiecesOfSameColorInCorner(i_OppositeColor);
            if ((numberOfOppositeColorPiecesInCorner + numberOfSameColorPiecesInCorner) != 0)
            {
                ratioOfPiecesInCorner = (numberOfSameColorPiecesInCorner - numberOfOppositeColorPiecesInCorner) * 1.0f / (numberOfSameColorPiecesInCorner + numberOfOppositeColorPiecesInCorner);
            }

            return ratioOfPiecesInCorner;
        }

        /*
         * Counts the number of pieces a color has in the corners of the board
         * i_ColorToCount - The color of the pieces we want to count
         */
        private int countPiecesOfSameColorInCorner(eSlotColor i_ColorToCount)
        {
            int numberOfSameColorPiecesInCorner = 0;
            int gameBoardSize = m_GameBoard.GetLength(0);

            numberOfSameColorPiecesInCorner += (m_GameBoard[0, 0].Color == i_ColorToCount) ? 1 : 0;
            numberOfSameColorPiecesInCorner += (m_GameBoard[0, gameBoardSize - 1].Color == i_ColorToCount) ? 1 : 0;
            numberOfSameColorPiecesInCorner += (m_GameBoard[gameBoardSize - 1, 0].Color == i_ColorToCount) ? 1 : 0;
            numberOfSameColorPiecesInCorner += (m_GameBoard[gameBoardSize - 1, gameBoardSize - 1].Color == i_ColorToCount) ? 1 : 0;
            numberOfSameColorPiecesInCorner += (m_GameBoard[0, 0].Color == i_ColorToCount) ? 1 : 0;

            return numberOfSameColorPiecesInCorner;
        }

        /*
         * Calculates how strong the current number of pieces of the color on the game board
         */
        private float calculateTheNumberOfPiecesStrength(eSlotColor i_PositionColor)
        {
            int numberOfPlayerPieces = ((i_PositionColor == eSlotColor.Black) ? m_BlackPiecesCount : m_WhitePiecesCount);
            int numberOfOpponentPieces = ((i_PositionColor == eSlotColor.White) ? m_BlackPiecesCount : m_WhitePiecesCount);

            return (numberOfPlayerPieces - numberOfOpponentPieces) * 1.0f / (numberOfPlayerPieces + numberOfOpponentPieces);
        }

        /*
         * Find the best move possible for given color
         * i_PlayerColor - The color we search a move for
         * Returns - The gameslot which is the best move possible
         */
        private GameSlot findBestMoveForColor(eSlotColor i_PlayerColor)
        {
            float maxValueOfMovesFound = float.MinValue;
            float minValueOfMovesFound = float.MaxValue;
            GameSlot bestMoveFound = null;
            List<GameSlot> playerAvailableMoves = (i_PlayerColor == eSlotColor.Black) ? m_BlackValidMoves : m_WhiteValidMoves;
            eSlotColor oppositePlayerColor = (i_PlayerColor == eSlotColor.Black) ? eSlotColor.White : eSlotColor.Black;
            playerAvailableMoves.Reverse();

            foreach (GameSlot availableMove in playerAvailableMoves)
            {
                float currentScore = 0;
                Othello moveWasPerformedState = cloneState();
                moveWasPerformedState.performMoveOnBoard(i_PlayerColor, availableMove.Row, availableMove.Col);
                currentScore = moveWasPerformedState.evaluateMoveStrength(oppositePlayerColor, !v_IsMaximizing, maxValueOfMovesFound, minValueOfMovesFound, 1, i_PlayerColor);
                if (currentScore > maxValueOfMovesFound)
                {
                    maxValueOfMovesFound = currentScore;
                    bestMoveFound = availableMove;
                }
            }

            return bestMoveFound;
        }

        /*
         * Calculate a performed move's strength
         * i_CurrentMoveColor - Which color will perfrom the current move if one is possible
         * i_IsMaximizingStrength - Do we want to get the highest strength possible or the lowest
         * i_MaxValueOfMovesFound - The maximum strength found in the search
         * i_MinStrengthOfMovesFound - The minimum strength found in the search
         * i_MovesPerformed - The number of prior moves performed before this
         * i_OriginalPlayerColor - The original player for which we want to calculate the move's strength
         * Returns - The strength of the position found
         */
        private float evaluateMoveStrength(eSlotColor i_CurrentMoveColor,
            bool i_IsMaximizingStrength,
            float i_MaxStrengthOfMovesFound,
            float i_MinStrengthOfMovesFound,
            int i_MovesPerformed,
            eSlotColor i_OriginalPlayerColor)
        {
            List<GameSlot> playingColorAvailableMoves = null;
            List<GameSlot> oppositionColorAvailableMoves = null;
            eSlotColor oppositeColor = eSlotColor.White;
            bool reachedEndOfCheck = i_MovesPerformed == k_MovesToCheckAhead;
            float scoreOfCurrentMove = i_IsMaximizingStrength ? float.MinValue : float.MaxValue;
            float strengthToReturn = 0;

            // Initiate which player is playing in current state
            initiateMoveParametersInCurrentState(i_CurrentMoveColor, ref playingColorAvailableMoves, ref oppositionColorAvailableMoves, ref oppositeColor);

            // If it is the last move to check or the game is over then just return a score
            reachedEndOfCheck = reachedEndOfCheck || ((playingColorAvailableMoves.Count == 0) && (oppositionColorAvailableMoves.Count == 0));
            if (reachedEndOfCheck)
            {
                strengthToReturn = calculateCurrentPositionStrength(i_OriginalPlayerColor);
            }
            else if (playingColorAvailableMoves.Count > 0)
            {
                foreach (GameSlot availableMove in playingColorAvailableMoves)
                {
                    if (i_MaxStrengthOfMovesFound >= i_MinStrengthOfMovesFound)
                    {
                        break;
                    }

                    float strengthOfMoveTested = 0;
                    Othello moveWasPerformedState = cloneState();
                    moveWasPerformedState.performMoveOnBoard(i_CurrentMoveColor, availableMove.Row, availableMove.Col);
                    strengthOfMoveTested = moveWasPerformedState.evaluateMoveStrength(oppositeColor, !i_IsMaximizingStrength, i_MaxStrengthOfMovesFound, i_MinStrengthOfMovesFound, i_MovesPerformed + 1, i_OriginalPlayerColor);
                    updateStrengthValues(i_IsMaximizingStrength, ref scoreOfCurrentMove, strengthOfMoveTested, ref i_MaxStrengthOfMovesFound, ref i_MinStrengthOfMovesFound, ref strengthToReturn);
                }
            }
            else
            {
                Othello stateCloneForNextColor = cloneState();
                float strengthOfMoveTested = stateCloneForNextColor.evaluateMoveStrength(oppositeColor, !i_IsMaximizingStrength, i_MaxStrengthOfMovesFound, i_MinStrengthOfMovesFound, i_MovesPerformed + 1, i_OriginalPlayerColor);
                updateStrengthValues(i_IsMaximizingStrength, ref scoreOfCurrentMove, strengthOfMoveTested, ref i_MaxStrengthOfMovesFound, ref i_MinStrengthOfMovesFound, ref strengthToReturn);
            }

            return strengthToReturn;
        }

        /*
         * Updates the current strength values found
         * i_IsMaximizingStrength - Whether we need to maximize the strength of minimize it
         * io_StrengthOfCurrentMove - The strength of the current move we are calculating
         * i_StrengthOfMoveTested - The strength of the move that was performed
         * io_MaxStrengthOfMovesFound - The maximum strength found until this point
         * io_MinStrengthOfMovesFound - The minimum strength found until this point
         * o_StrengthToReturn - The strength to return as result for a higher level check
         */
        private void updateStrengthValues(
            bool i_IsMaximizingStrength,
            ref float io_StrengthOfCurrentMove,
            float i_StrengthOfMoveTested,
            ref float io_MaxStrengthOfMovesFound,
            ref float io_MinStrengthOfMovesFound,
            ref float o_StrengthToReturn)
        {
            if (i_IsMaximizingStrength)
            {
                io_StrengthOfCurrentMove = Math.Max(io_StrengthOfCurrentMove, i_StrengthOfMoveTested);
                io_MaxStrengthOfMovesFound = Math.Max(io_MaxStrengthOfMovesFound, io_StrengthOfCurrentMove);
                o_StrengthToReturn = io_MaxStrengthOfMovesFound;
            }
            else
            {
                io_StrengthOfCurrentMove = Math.Min(io_StrengthOfCurrentMove, i_StrengthOfMoveTested);
                io_MinStrengthOfMovesFound = Math.Min(io_MinStrengthOfMovesFound, io_StrengthOfCurrentMove);
                o_StrengthToReturn = io_MinStrengthOfMovesFound;
            }
        }

        /*
         * Initializes all parameters needed for the calculation of a move strength
         * i_CurrentMoveColor - Which color will perfrom the current move if one is possible
         * o_PlayingColorMoves - All the legal moves of the playing color
         * o_OppositeColorMoves - All the legal moves of the opposite color
         * o_OppositeColor - The color of the opposite player
         */
        private void initiateMoveParametersInCurrentState(
            eSlotColor i_CurrentMoveColor,
            ref List<GameSlot> o_PlayingColorMoves,
            ref List<GameSlot> o_OppositeColorMoves,
            ref eSlotColor o_OppositeColor)
        {
            if (i_CurrentMoveColor == eSlotColor.Black)
            {
                o_PlayingColorMoves = m_BlackValidMoves;
                o_OppositeColorMoves = m_WhiteValidMoves;
                o_OppositeColor = eSlotColor.White;
            }
            else
            {
                o_PlayingColorMoves = m_WhiteValidMoves;
                o_OppositeColorMoves = m_BlackValidMoves;
                o_OppositeColor = eSlotColor.Black;
            }
        }

        /*
         * Clones the current state of the game
         * Returns - an Othello game
         */
        private Othello cloneState()
        {
            Othello clonedState = new Othello(m_GameBoard.GetLength(0));
            clonedState.m_BlackPiecesCount = m_BlackPiecesCount;
            clonedState.m_WhitePiecesCount = m_WhitePiecesCount;
            clonedState.m_BlackValidMoves = new List<GameSlot>();
            clonedState.m_WhiteValidMoves = new List<GameSlot>();

            for (int i = 0; i < clonedState.m_GameBoard.GetLength(0); i++)
            {
                for (int j = 0; j < clonedState.m_GameBoard.GetLength(0); j++)
                {
                    clonedState.m_GameBoard[i, j] = m_GameBoard[i, j].CloneSlot();
                }
            }

            foreach (GameSlot gameSlot in m_BlackValidMoves)
            {
                GameSlot copyOfGameSlot = clonedState.m_GameBoard[gameSlot.Row, gameSlot.Col];
                clonedState.m_BlackValidMoves.Add(copyOfGameSlot);
            }

            foreach (GameSlot gameSlot in m_WhiteValidMoves)
            {
                GameSlot copyOfGameSlot = clonedState.m_GameBoard[gameSlot.Row, gameSlot.Col];
                clonedState.m_WhiteValidMoves.Add(copyOfGameSlot);
            }

            return clonedState;
        }
    }

}
