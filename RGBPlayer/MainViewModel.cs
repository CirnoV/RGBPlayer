using ManagedBass;
using Microsoft.Win32;
using RGBPlayer.Core;
using RGBPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace RGBPlayer
{
	public class MainViewModel : Notifier
	{
		private readonly OpenFileDialog _FileDialog;
		private int _BGMChannel;
		private double _BGMLength => Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetLength(_BGMChannel));
		private double _BGMCurrentTime => Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetPosition(_BGMChannel));

		DispatcherTimer _MusicTimer;

		private double _BPMTemp;

		#region BPM 변경통지 프로퍼티
		private int _BPM;
		public int BPM
		{
			get
			{
				return _BPM;
			}
			set
			{
				_BPM = value;
				RaisePropertyChanged(nameof(BPM));
			}
		}
		#endregion

		#region BPMOffset 변경통지 프로퍼티
		private int _BPMOffset;
		public int BPMOffset
		{
			get
			{
				return _BPMOffset;
			}
			set
			{
				_BPMOffset = value;
				RaisePropertyChanged(nameof(BPMOffset));
			}
		}
		#endregion

		#region NoteData 변경통지 프로퍼티
		private ObservableCollection<NoteData> _NoteData;
		public ObservableCollection<NoteData> NoteData
		{
			get
			{
				return _NoteData;
			}
			set
			{
				_NoteData = value;
				RaisePropertyChanged(nameof(NoteData));
			}
		}
		#endregion

		#region FileStatus 변경통지 프로퍼티
		private string _FileStatus = "재생할 파일을 선택해주세요.";
		public string FileStatus
		{
			get
			{
				return _FileStatus;
			}
			set
			{
				_FileStatus = value;
				RaisePropertyChanged(nameof(FileStatus));
			}
		}
		#endregion

		#region Music 프로퍼티
		public string MusicTime
		{
			get
			{
				return _BGMCurrentTime.ToString("f3");
			}
		}
		#endregion

		#region MusicSlider 변경통지 프로퍼티
		private double _MusicSlider;
		public double MusicSlider
		{
			get
			{
				_MusicSlider = (_BGMCurrentTime / _BGMLength) * 100;
				return _MusicSlider;
			}
			set
			{
				var pos = Bass.ChannelSeconds2Bytes(_BGMChannel, _BGMLength * (value / 100));
				Bass.ChannelSetPosition(_BGMChannel, pos);
				_IgnoreNoteTime = _BGMLength * (value / 100);

				_MusicSlider = value;
			}
		}
		#endregion

		#region IsPlayNote 변경통지 프로퍼티
		private bool _IsPlayNote;
		public bool IsPlayNote
		{
			get
			{
				return _IsPlayNote;
			}
			set
			{
				_IsPlayNote = value;
				RaisePropertyChanged(nameof(IsPlayNote));
			}
		}
		#endregion

		#region DelegateCommand
		private DelegateCommand _LoadCommand;
		public ICommand LoadCommand
		{
			get
			{
				return _LoadCommand ?? (_LoadCommand = new DelegateCommand(x => OpenFile()));
			}
		}

		private DelegateCommand _PlayCommand;
		public ICommand PlayCommand
		{
			get
			{
				return _PlayCommand ?? (_PlayCommand = new DelegateCommand(x => PlayFile()));
			}
		}

		private DelegateCommand _PauseCommand;
		public ICommand PauseCommand
		{
			get
			{
				return _PauseCommand ?? (_PauseCommand = new DelegateCommand(x => PauseFile()));
			}
		}

		private DelegateCommand _StopCommand;
		public ICommand StopCommand
		{
			get
			{
				return _StopCommand ?? (_StopCommand = new DelegateCommand(x => StopFile()));
			}
		}

		private DelegateCommand _TestCommand;
		public ICommand TestCommand
		{
			get
			{
				return _TestCommand ?? (_TestCommand = new DelegateCommand(x => Test()));
			}
		}

		private DelegateCommand _OrderNoteCommand;
		public ICommand OrderNoteCommand
		{
			get
			{
				return _OrderNoteCommand ?? (_OrderNoteCommand = new DelegateCommand(x => OrderNote()));
			}
		}

		private DelegateCommand _CopyNoteCommand;
		public ICommand CopyNoteCommand
		{
			get
			{
				return _CopyNoteCommand ?? (_CopyNoteCommand = new DelegateCommand(x => CopyNote()));
			}
		}

		private DelegateCommand _InitBPMCommand;
		public ICommand InitBPMCommand
		{
			get
			{
				return _InitBPMCommand ?? (_InitBPMCommand = new DelegateCommand(x => InitBPM()));
			}
		}
		#endregion

		~MainViewModel()
		{
			Bass.Free();
		}

		public MainViewModel()
		{
			_FileDialog = new OpenFileDialog
			{
				Filter = "재생가능 파일|*.mp3; *.ogg; *.wav|모든 파일|*.*"
			};

			if (!Bass.Init())
			{
				MessageBox.Show("Can't initialize device");
				Application.Current.Shutdown();
			}

			_MusicTimer = new DispatcherTimer();
			_MusicTimer.Tick += _MusicTimer_Tick;
			_MusicTimer.Interval = TimeSpan.FromSeconds(0.01);
			_MusicTimer.Start();

			_NoteData = new ObservableCollection<NoteData>();
			//_NoteData = new List<Models.NoteData>();
		}

		private double _IgnoreNoteTime;
		private void _MusicTimer_Tick(object sender, EventArgs e)
		{
			if (IsPlayNote && Bass.ChannelIsActive(_BGMChannel) == PlaybackState.Playing)
			{
				double currentPos = Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetPosition(_BGMChannel));
				var note = NoteData.OrderBy(x => x.NoteTime).FirstOrDefault(x => x.NoteTime > _IgnoreNoteTime && x.NoteTime <= currentPos);

				if(note != null)
				{
					PlayNote(note.Note);
					_IgnoreNoteTime = note.NoteTime;
				}
			}

			RaisePropertyChanged(nameof(MusicTime));
			RaisePropertyChanged(nameof(MusicSlider));
		}

		public void PlayFile()
		{
			Bass.ChannelPlay(_BGMChannel);
			_IgnoreNoteTime = Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetPosition(_BGMChannel));
		}

		public void PauseFile()
		{
			if (Bass.ChannelIsActive(_BGMChannel) == PlaybackState.Playing)
			{
				Bass.ChannelPause(_BGMChannel);
			}
			else
			{
				Bass.ChannelPlay(_BGMChannel);
				_IgnoreNoteTime = Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetPosition(_BGMChannel));
			}
		}

		public void StopFile()
		{
			Bass.ChannelStop(_BGMChannel);
			Bass.ChannelSetPosition(_BGMChannel, 0);
			_IgnoreNoteTime = 0;

			RaisePropertyChanged(nameof(MusicTime));
			RaisePropertyChanged(nameof(MusicSlider));
		}

		public void OpenFile()
		{
			if (!_FileDialog.ShowDialog().Value)
			{
				return;
			}

			Bass.StreamFree(_BGMChannel);

			_BGMChannel = Bass.CreateStream(_FileDialog.FileName, 0, 0, BassFlags.Prescan);

			if (_BGMChannel == 0)
			{
				FileStatus = "재생할 파일을 선택해주세요.";
				return;
			}

			//Bass.SampleGetChannel(_BGMChannel);

			FileStatus = System.IO.Path.GetFileName(_FileDialog.FileName);
		}

		public void Test()
		{
			Bass.ChannelSetPosition(_BGMChannel, Bass.ChannelSeconds2Bytes(_BGMChannel, 10.0));
		}

		public void OrderNote()
		{
			NoteData = new ObservableCollection<Models.NoteData>(NoteData.OrderBy(x => x.NoteTime));
			RaisePropertyChanged(nameof(NoteData));
		}

		public void CopyNote()
		{
			OrderNote();
			StringBuilder sb = new StringBuilder();
			foreach (var note in NoteData)
			{
				sb.AppendFormat("PushArrayCell({0}, {1});", note.Note.ScriptName, note.NoteTime.ToString("f3"));
				sb.AppendLine();
			}

			Clipboard.SetText(sb.ToString());
		}

		public void PlayNote(Note note)
		{
			//EntryAssembly/sound/FileName
			var soundPath = Path.Combine(
				Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
				"sound",
				note.FileName
			);

			int channel = Bass.CreateStream(soundPath, 0, 0, BassFlags.Prescan | BassFlags.AutoFree);
			Bass.ChannelPlay(channel);
		}

		#region KeyBinding
		public void DeleteNote(IList<NoteData> note)
		{
			NoteData = new ObservableCollection<Models.NoteData>(NoteData.Except(note));
			//NoteData = NoteData.Except(note).ToList();
		}

		public void KeyDown()
		{
			foreach (var note in RGBData.Current.NoteList)
			{
				if ((Keyboard.IsKeyDown(note.Value.Input1) && !note.Value.IsInput1Down)
					|| (Keyboard.IsKeyDown(note.Value.Input2) && !note.Value.IsInput2Down))
				{
					if (Keyboard.IsKeyDown(note.Value.Input1))
					{
						note.Value.IsInput1Down = true;
					}
					if (Keyboard.IsKeyDown(note.Value.Input2))
					{
						note.Value.IsInput2Down = true;
					}

					PlayNote(note.Value);

					NoteData.Add(
						new NoteData
						{
							Note = note.Value,
							NoteTime = Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetPosition(_BGMChannel))
						}
					);
					_IgnoreNoteTime = Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetPosition(_BGMChannel));
					RaisePropertyChanged(nameof(NoteData));
				}
			}
		}

		public void KeyUp()
		{
			foreach (var note in RGBData.Current.NoteList)
			{
				if (Keyboard.IsKeyUp(note.Value.Input1))
				{
					note.Value.IsInput1Down = false;
				}
				if (Keyboard.IsKeyUp(note.Value.Input2))
				{
					note.Value.IsInput2Down = false;
				}
			}
		}

		public void InitBPM()
		{
			if(BPMOffset == 0 || BPMOffset == -1000)
			{
				BPMOffset = Convert.ToInt32(Bass.ChannelBytes2Seconds(_BGMChannel, Bass.ChannelGetPosition(_BGMChannel)) * 1000);
			}
			else
			{

			}
		}
		#endregion
	}
}
