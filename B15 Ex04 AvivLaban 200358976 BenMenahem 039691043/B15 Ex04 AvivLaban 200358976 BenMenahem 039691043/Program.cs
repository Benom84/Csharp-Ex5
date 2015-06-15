using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise05
{
    public static class Program
    {
        private static bool m_IsPlayAgainstComputer;
        private static int m_BoardSizeSelected;
        private static bool m_UserPreferencesReceived = false;
        
        public static void Main()
        {
            displayUserPreferencesForm();
            if (m_UserPreferencesReceived)
            {
                FormGameBoard othelloBoard = new FormGameBoard(m_BoardSizeSelected, m_IsPlayAgainstComputer);
                othelloBoard.ShowDialog();
            }
            
        }

        private static void displayUserPreferencesForm()
        {
            FormGameSettings gameSettingsForm = new FormGameSettings();
            gameSettingsForm.SetUserPreferences += getUserPreferences;
            gameSettingsForm.ShowDialog();
        }

        private static void getUserPreferences(bool i_IsPlayingAgainstComputer, int i_BoardSizeSelected) {
            m_IsPlayAgainstComputer = i_IsPlayingAgainstComputer;
            m_BoardSizeSelected = i_BoardSizeSelected;
            m_UserPreferencesReceived = true;
        }
    }
}
