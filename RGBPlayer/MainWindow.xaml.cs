using MahApps.Metro.Controls;
using System;
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
using RGBPlayer.Core;
using RGBPlayer.Models;

namespace RGBPlayer
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			RGBData.Current.Initialize();

			InitializeComponent();

			DataContext = new MainViewModel();
		}

		private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (NoteTab.IsSelected)
			{
				if (Keyboard.IsKeyDown(Key.Delete) || Keyboard.IsKeyDown(Key.Back))
				{
					var list = NoteList.SelectedItems.Cast<NoteData>().ToList();
					((MainViewModel)DataContext).DeleteNote(list);
				}
				else if (Keyboard.IsKeyDown(Key.A))
				{
					if (Keyboard.IsKeyDown(Key.LeftCtrl))
					{
						NoteList.SelectAll();
					}
					else
					{
						((MainViewModel)DataContext).KeyDown();
					}
				}
				else
				{
					((MainViewModel)DataContext).KeyDown();
				}
			}
			else if (TimingTab.IsSelected)
			{
				if (Keyboard.IsKeyDown(Key.T))
				{
					((MainViewModel)DataContext).InitBPM();
				}
			}
		}

		private void MetroWindow_KeyUp(object sender, KeyEventArgs e)
		{
			((MainViewModel)DataContext).KeyUp();
		}
	}
}
