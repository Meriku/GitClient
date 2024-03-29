﻿using CommunityToolkit.Mvvm.Messaging;
using GItClient.Core;
using GItClient.Core.Controllers;
using GItClient.Core.Convertors;
using GItClient.Core.Models;
using GItClient.MVVM.Assets;
using Gu.Wpf.Adorners;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GItClient.MVVM.View.PartialView
{
    /// <summary>
    /// This is partial view of CommitsHistory
    /// When this view is loaded all commits are displayed
    /// </summary>
    public partial class CommitsHistoryPartialView : UserControl
    {
        private const string EMPTY_REPOSITORIES_TEXT = "It's curently empty here :( \nInit, clone or open a new repository";

        private int FontSize = 12;
        public const int ELLIPSE_SIZE = 8;

        private Dictionary<int, System.Windows.Media.Color> ColorByGeneration;

        public CommitsHistoryPartialView()
        {
            // TODO: ScrollViewer design

            InitializeComponent();

            RenderCommits();

            WeakReferenceMessenger.Default.Register<RepositoryChangedMessage>(this, (r, m) =>
            { RenderCommits(); });

            ColorByGeneration = new Dictionary<int, System.Windows.Media.Color> {
                { 0, Colors.White},
                { 1, Colors.Yellow},
                { 2, Colors.Green},
                { 3, Colors.Red},
                { 4, Colors.Blue},
                { 5, Colors.Violet}
            };
        }

        private void RenderCommits()
        {
            Dispatcher.Invoke( async () =>
            {
                MainGrid.RowDefinitions.Clear();

                MainCanvas.Children.Clear();
                MainGrid.Children.Clear();

                if (RepositoriesController.IsEmpty)
                {
                    RenderWelcomeView();
                    return;
                }

                var CurrentRepository = RepositoriesController.GetCurrentRepository();
                if (CurrentRepository.CommitsHolder.IsEmpty)
                {
                    if (CurrentRepository.CommitsHolder.IsLoading)
                    {
                        RenderWaitingView(CurrentRepository);
                        return;
                    }
                    else
                    {
                        RepositoriesController.StartLoadRepositoryCommits(CurrentRepository.GenName);
                        RenderWaitingView(CurrentRepository);
                        return;
                    }
                }


                await RenderCommitsViewImpl(CurrentRepository);
            });
        }

        private void RenderWelcomeView()
        {
            var row = new RowDefinition();
            row.Height = new GridLength(60);

            var textblock = new TextBlock();

            textblock.Text = EMPTY_REPOSITORIES_TEXT;
            textblock.Margin = new Thickness(10);
            textblock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            textblock.FontSize = 15;
            textblock.Height = 50;
            textblock.HorizontalAlignment = HorizontalAlignment.Center;

            MainGrid.RowDefinitions.Add(row);
            MainGrid.Children.Add(textblock);
            Grid.SetRow(textblock, 1);
            Grid.SetColumn(textblock, 2);
        }

        private void RenderWaitingView(Repository currentRepository)
        {
            var spinner = new LoadingSpinner();

            spinner.Name = "Spinner";
            spinner.Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            spinner.Diameter = MainGrid.ActualHeight < MainGrid.ActualWidth ? MainGrid.ActualHeight / 2 : MainGrid.ActualWidth / 2;
            spinner.Thickness = 5;
            spinner.IsLoading = true;
            spinner.Visibility = Visibility.Visible;

            MainGrid.SizeChanged += GridChanged_ResizeSpinner;

            MainGrid.Children.Add(spinner);
            Grid.SetRow(spinner, 1);
            Grid.SetColumn(spinner, 0);
            Grid.SetColumnSpan(spinner, 4);


            Task.Run(async () => 
            {
                await currentRepository.CommitsHolder.WaitAsync(); // if commits are loading - wait
                currentRepository.CommitsHolder.Release();

                WeakReferenceMessenger.Default.Send(new RepositoryChangedMessage("WaitingEnded"));
            });
        }

        private async Task RenderCommitsViewImpl(Repository currentRepository)
        {
            MainGrid.SizeChanged -= GridChanged_ResizeSpinner;

            var EmptyFirstRow = new RowDefinition() { Height = new GridLength(5) };
            MainGrid.RowDefinitions.Add(EmptyFirstRow);

            // Canvas for ellipses and lines for graph
            MainGrid.Children.Add(MainCanvas);
            Grid.SetRow(MainCanvas, 0);
            Grid.SetRowSpan(MainCanvas, int.MaxValue);
            Grid.SetColumn(MainCanvas, 0);

            // Variables for commits text, style
            var fontSize = FontSize;
            var fontFamily = new System.Windows.Media.FontFamily("Roboto-Light");
            var fontColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 190, 190));
            var gridRowHeight = GetRowHeight();

            var graphNodes = new Dictionary<string, TreeViewItem>();
            var commits = currentRepository.CommitsHolder.Commits;

            for (var i = 0; i < commits.Length; i++)
            {
                var currentRow = new RowDefinition() { Height = new GridLength(gridRowHeight) };
                MainGrid.RowDefinitions.Add(currentRow);
            }

            var maxLevel = 1;
            for (var i = commits.Length - 1; i >= 0 ; i--)
            {
                var rowsCount = MainGrid.RowDefinitions.Count - 1;
                var commit = commits[i];

                var textblockHash = CreateTextBlock(commit.ShortHash);
                var textblockMessage = CreateTextBlock(commit.Subject);
                var textblockAuthor = CreateTextBlock(commit.AuthorName);
                var textblockDate = CreateTextBlock(commit.ShortDateString);

                MainGrid.Children.Add(textblockHash);
                MainGrid.Children.Add(textblockMessage);
                MainGrid.Children.Add(textblockAuthor);
                MainGrid.Children.Add(textblockDate);

                Grid.SetRow(textblockHash, i + 1);
                Grid.SetColumn(textblockHash, 1);

                Grid.SetRow(textblockMessage, i + 1);
                Grid.SetColumn(textblockMessage, 2);

                Grid.SetRow(textblockAuthor, i + 1);
                Grid.SetColumn(textblockAuthor, 3);

                Grid.SetRow(textblockDate, i + 1);
                Grid.SetColumn(textblockDate, 4);


                var graphNode = new TreeViewItem()
                {
                    Body = CreateEllipse(commit, graphNodes),
                    Commit = commit
                };
                graphNodes[commit.Hash] = graphNode;
                

                MainCanvas.Children.Add(graphNode.Body);

                Canvas.SetLeft(graphNode.Body, 5 + commit.Level * 10);
                Canvas.SetTop(graphNode.Body, i * gridRowHeight + EmptyFirstRow.Height.Value + ELLIPSE_SIZE / 2);
                Canvas.SetZIndex(graphNode.Body, 10);
                DrawLine(graphNodes, graphNode);

                if (commit.Level > maxLevel)
                {
                    maxLevel = commit.Level;
                }
            }

            MainGrid.ColumnDefinitions[0].Width = new GridLength(maxLevel * 10 + 30);
        }

        private void DrawLine(Dictionary<string, TreeViewItem> allNodes, TreeViewItem node)
        {
            var maxLineLength = 150;
            for (var i = 0; i < node.Commit.ParentHashes.Length; i++)
            {
                var parentHash = node.Commit.ParentHashes[i];
                var parentNode = allNodes[parentHash];

                var line = new Line();
                line.Y1 = Canvas.GetTop(node.Body) + ELLIPSE_SIZE / 2;
                line.X1 = Canvas.GetLeft(node.Body) + ELLIPSE_SIZE / 2;
                line.Y2 = Canvas.GetTop(parentNode.Body) + ELLIPSE_SIZE / 2;
                line.X2 = Canvas.GetLeft(parentNode.Body) + ELLIPSE_SIZE / 2;

                line.StrokeThickness = 1;

                if (parentNode.Commit.Level > node.Commit.Level)
                {
                    line.Stroke = parentNode.Body.Fill;
                }
                else
                {
                    line.Stroke = node.Body.Fill;
                }            

                if ((line.Y2 - line.Y1) > maxLineLength)
                {
                    line.X2 = line.X1;
                    var extraLine = new Line();
                    extraLine.Y1 = line.Y2;
                    extraLine.Y2 = line.Y2;
                    extraLine.X1 = line.X2;
                    extraLine.X2 = Canvas.GetLeft(parentNode.Body) + ELLIPSE_SIZE / 2;

                    extraLine.Stroke = line.Stroke;
                    extraLine.StrokeThickness = 1;
                    MainCanvas.Children.Add(extraLine);
                }

                MainCanvas.Children.Add(line);
            }
        }

        private TextBlock CreateTextBlock(string text)
        {
            var fontSize = FontSize;
            var fontFamily = new System.Windows.Media.FontFamily("Roboto-Light");
            var fontColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 190, 190));

            var textblock = new TextBlock();
            textblock.Foreground = fontColor;
            textblock.FontSize = fontSize;
            textblock.FontFamily = fontFamily;
            textblock.Text = text;

            return textblock;
        }
        private Ellipse CreateEllipse(GitCommit commit, Dictionary<string, TreeViewItem> allNodes)
        {
            var result = new Ellipse()
            {
                Height = ELLIPSE_SIZE,
                Width = ELLIPSE_SIZE,
                Fill = new SolidColorBrush(Colors.White),
                ToolTip = new ToolTip() { Content = commit.Hash + " " + "Parents: " + string.Join(", ", commit.ParentHashes), Foreground = new SolidColorBrush(Colors.Black) },
                HorizontalAlignment = HorizontalAlignment.Left
            };

            if (commit.Level == 0) { return result; };
            
            var parentHash = commit.ParentHashes.FirstOrDefault();
            if (parentHash != null && allNodes.ContainsKey(parentHash))
            {
                var parent = allNodes[parentHash];
                if (parent.Commit.Level == commit.Level)
                {
                    result.Fill = parent.Body.Fill;
                }
                else
                {
                    result.Fill = new SolidColorBrush(Helper.GetRandomColor());
                }
            }

            return result;
        }

        private void ResizeGridText()
        {
            var rowHeight = GetRowHeight();

            for (var i = 1; i < MainGrid.RowDefinitions.Count; i++)
            {
                MainGrid.RowDefinitions[i].Height = new GridLength(rowHeight);
            }
            foreach (var child in MainGrid.Children)
            {
                if (child is TextBlock textBlock)
                {
                    textBlock.FontSize = FontSize;
                }
            }
        }

        private void GridChanged_ResizeSpinner(object o, SizeChangedEventArgs e)
        {
            var gridChild = MainGrid.Children[0];

            if (gridChild != null && gridChild is LoadingSpinner spinner)
            {
                spinner.Diameter = MainGrid.ActualHeight < MainGrid.ActualWidth ? MainGrid.ActualHeight / 2 : MainGrid.ActualWidth / 2;
            }
        }

        private void PreviewMouseWheel_ResizeCommitsFont(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return;
            }

            if (e.Delta > 0 && FontSize < 30)
            {
                FontSize++;
                ResizeGridText();
            }
            else if (e.Delta < 0 && FontSize > 8)
            {
                FontSize--;
                ResizeGridText();
            }

        }

        private double GetRowHeight()
        {
            var fontSize = FontSize;
            var fontFamily = new System.Windows.Media.FontFamily("Roboto-Light");
            var rowHeight = Math.Ceiling(fontSize * fontFamily.LineSpacing) + 5;
            return rowHeight;
        }

    }

    public class TreeViewItem 
    {
        public Ellipse Body { get; set; }
        public GitCommit Commit { get; set; }
        public int CollumnIndex { get; set; }

        public TreeViewItem() { }
    }

}
