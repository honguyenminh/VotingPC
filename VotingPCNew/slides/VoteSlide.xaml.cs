﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AsyncDialog;
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using VotingPC;
using VotingPCNew.Controls;
using VotingPCNew.Scanner;

namespace VotingPCNew.Slides;

/// <summary>
/// Interaction logic for VoteSlide
/// </summary>
public partial class VoteSlide
{
    private AsyncDialogManager _dialogs;
    private AsyncDatabaseManager _db;
    private ScannerManager _scanner;

    public VoteSlide()
    {
        InitializeComponent();
    }

    public string TopTitle
    {
        get => topTitle.Text;
        set => topTitle.Text = value;
    }

    public void InjectDependencies(AsyncDialogManager dialogManager,
        AsyncDatabaseManager databaseManager, ScannerManager scannerManager)
    {
        _dialogs = dialogManager;
        _db = databaseManager;
        _scanner = scannerManager;
    }

    // TODO: add the title thing on top of this divider to config
    private static readonly DividerNavigationItem s_divider = new();
    private Dictionary<string, (Sector, VirtualizingStackPanel)> _sectorVotePanels;

    /// <summary>
    /// Set item source to be used for populating the vote UI
    /// </summary>
    public void SetItemsSource(List<Sector> sectors)
    {
        _sectorVotePanels = new(sectors.Count);
        List<BaseNavigationItem> navigationItems = new(sectors.Count + 2) {s_divider};
        foreach (var sector in sectors)
        {
            // Add sector to nav list
            FirstLevelNavigationItem navItem = new()
            {
                Label = sector.Name,
                Icon = PackIconKind.CameraControl // This icon is god-level generic
            };
            navigationItems.Add(navItem);

            // Populate vote panels
            VirtualizingStackPanel votePanel = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(32)
            };
            foreach (var candidate in sector.Candidates)
            {
                RadioTextBox textBox = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 12, 0, 12),
                    Text = $"{candidate.Gender}   {candidate.Name}",
                    IsChecked = true
                };
                votePanel.Children.Add(textBox);
            }

            // Add the sector to dictionary for easier lookup later
            _sectorVotePanels.Add(sector.Name, (sector, votePanel));
        }

        navigationItems.Add(s_divider);
        sideNav.Items = navigationItems;
        if (sectors.Count != 0)
        {
            sideNav.SelectedItem = navigationItems[1];
        }
    }

    /// <summary>
    /// Occurs when the item selected in sidenav bar is changed
    /// </summary>
    /// <remarks>
    /// Apply selected sector's style and candidate list
    /// </remarks>
    private void NavigationItem_SelectionChanged(object sender, NavigationItemSelectedEventArgs args)
    {
        // Get selected sector's name
        string selectedItem = ((NavigationItem) args.NavigationItem)?.Label;
        if (selectedItem is null) return; // In case nav item is reset from code, and nothing is selected
        // Get the selected sector's info and apply styling
        var (sector, panel) = _sectorVotePanels[selectedItem];
        title.Text = sector.Title;
        subtitle.Text = sector.Subtitle;
        Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString(sector.Color)!);
        // TODO: move text below to config
        maxVote.Text = $"Chọn tối đa {sector.Max} người";
        candidateCard.Content = panel;
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate votes
        bool isInvalid = false;
        // TODO: move this to config file
        // TODO TODO: better yet, make this auto validate
        StringBuilder errorsBuilder = new("Phiếu bầu không hợp lệ tại:");
        foreach (var (sector, panel) in _sectorVotePanels.Values)
        {
            int totalVoted = 0;
            foreach (RadioTextBox textBox in panel.Children)
            {
                if (textBox.IsChecked) totalVoted++;
            }

            if (totalVoted > sector.Max)
            {
                isInvalid = true;
                errorsBuilder.Append("\n - " + sector.Name);
            }
        }

        if (isInvalid)
        {
            string errors = errorsBuilder.ToString();
            await _dialogs.ShowTextDialog(errors, "Trở lại");
            return;
        }

        // Increase vote count for each candidate after validated
        foreach (var (sector, panel) in _sectorVotePanels.Values)
        {
            for (int i = 0; i < sector.Candidates.Count; i++)
            {
                var textBox = (RadioTextBox) panel.Children[i];
                if (textBox.IsChecked)
                {
                    sector.Candidates[i].Votes++;
                }
            }
        }

        // Save data to database
        _dialogs.ShowLoadingDialog("Đang lưu phiếu bầu");
        await _db.SaveCurrentData();
        await _dialogs.CloseDialog();

        await _dialogs.ShowTextDialog(
            title: "Hoàn tất",
            text: "Đã nộp phiếu bầu. Chúc một ngày tốt lành.",
            buttonLabel: "HOÀN TẤT"
        );

        PreviousSlide();
        ResetSlide();
        await Task.Delay(1000);
        await _scanner.StartScan(NextSlide);
    }

    private void ResetSlide()
    {
        // Set selected slide back to first slide if exists
        if (sideNav.Items.Count != 2)
            sideNav.SelectedItem = ((List<BaseNavigationItem>) sideNav.Items)[1];
        // Reset all checkboxes
        foreach (var (_, panel) in _sectorVotePanels.Values)
        foreach (RadioTextBox textBox in panel.Children)
            textBox.IsChecked = true;
    }

    // Transitioner commands.
    // TODO: Should be moved to separate class
    public void NextSlide()
    {
        Transitioner.MoveNextCommand.Execute(null, this);
    }

    public void PreviousSlide()
    {
        Transitioner.MovePreviousCommand.Execute(null, this);
    }
}