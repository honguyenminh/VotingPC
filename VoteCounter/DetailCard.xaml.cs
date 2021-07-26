﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VoteCounter
{
    /// <summary>
    /// Interaction logic for DetailCard.xaml
    /// </summary>
    public partial class DetailCard : UserControl
    {
        public DetailCard()
        {
            InitializeComponent();
        }
        public DetailCard(long position, string name, long totalVotes)
        {
            InitializeComponent();
            positionTextBlock.Text = position.ToString();
            nameTextBlock.Text = name;
            totalVotesTextBlock.Text = totalVotes.ToString();
        }
    }
}
