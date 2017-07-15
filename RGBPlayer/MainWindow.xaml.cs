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
using ManagedBass;
using Microsoft.Win32;

namespace RGBPlayer
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		private readonly OpenFileDialog _FileDialog;
		private int _Channel;

		public string FileStatus
		{
			get
			{
				return (string)OpenButton.Content;
			}
			set
			{
				OpenButton.Content = value;
			}
		}

		~MainWindow()
		{
			Bass.Free();
		}

		public MainWindow()
		{
			InitializeComponent();

			_FileDialog = new OpenFileDialog
			{
				Filter = "재생가능 파일|*.mp3; *.ogg; *.wav|모든 파일|*.*"
			};

			if (!Bass.Init())
			{
				MessageBox.Show("Can't initialize device");
				Application.Current.Shutdown();
			}
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFile();
		}

		public void OpenFile()
		{
			if (!_FileDialog.ShowDialog().Value)
			{
				return;
			}

			// free tempo, stream, music & bpm/beat callbacks
			Bass.StreamFree(_Channel);
			Bass.MusicFree(_Channel);

			// create decode channel
			_Channel = Bass.CreateStream(_FileDialog.FileName, 0, 0, BassFlags.Decode);

			if (_Channel == 0)
				_Channel = Bass.MusicLoad(_FileDialog.FileName, 0, 0, BassFlags.MusicRamp | BassFlags.Prescan | BassFlags.Decode, 0);

			if (_Channel == 0)
			{
				// not a WAV/MP3 or MOD
				FileStatus = "재생할 파일을 선택해주세요.";
				return;
			}

			Bass.SampleGetChannel(_Channel);

			// update the button to show the loaded file name (without path)
			FileStatus = System.IO.Path.GetFileName(_FileDialog.FileName);

			// play new created stream
			Bass.ChannelPlay(_Channel);
		}
	}
}
