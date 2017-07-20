﻿using ManagedBass;
using ManagedBass.Fx;
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace RGBPlayer
{
	public class MainViewModel : Notifier
	{
		private readonly OpenFileDialog _FileDialog;
		private Music Music;

		DispatcherTimer _MusicTimer;

		private int _BPMTemp;
		private List<int> _BPMTimingList;

		private BPM _CurrentBPM
		{
			get
			{
				BPM bpm = BPMList.OrderBy(x => x.Offset).LastOrDefault(x => x.Offset <= Music.CurrentTime * 1000);

				if (bpm == null)
				{
					bpm = BPMList.First();
				}

				BPM newBPM = new BPM();
				double bpmMul = 1.0;

				if (NoteTabSelected)
				{
					switch (NoteDiv)
					{
						case 1:
							bpmMul = 1.0;
							break;
						case 2:
							bpmMul = 2.0;
							break;
						case 3:
							bpmMul = 3.0;
							break;
						case 4:
							bpmMul = 4.0;
							break;
						case 5:
							bpmMul = 8.0;
							break;
						case 6:
							bpmMul = 12.0;
							break;
						case 7:
							bpmMul = 16.0;
							break;
						default:
							break;
					}
				}

				newBPM.Value = bpm.Value * bpmMul;
				newBPM.Offset = bpm.Offset;

				return newBPM;
			}
		}
		private double _BeatDelay => 1 / (_CurrentBPM.Value / 60.0);
		private int _BeatMul => Convert.ToInt32((Music.CurrentTime - _CurrentBPM.Offset / 1000.0) / _BeatDelay);
		private double _NextBeatTime => (_BeatDelay * _BeatMul) + _CurrentBPM.Offset / 1000.0;

		#region BPMList 변경통지 프로퍼티
		private ObservableCollection<BPM> _BPMList;
		public ObservableCollection<BPM> BPMList
		{
			get
			{
				return _BPMList;
			}
			set
			{
				RaisePropertyChanged(nameof(BPMList));
			}
		}
		#endregion

		#region SelectedBPM 변경통지 프로퍼티
		private BPM _SelectedBPM;
		public BPM SelectedBPM
		{
			get
			{
				if(_SelectedBPM == null)
				{
					_SelectedBPM = BPMList.First();
				}
				return _SelectedBPM;
			}
			set
			{
				_SelectedBPM = value;
				RaisePropertyChanged(nameof(SelectedBPM));
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

		#region MusicTime 프로퍼티
		public string MusicTime
		{
			get
			{
				return Music.CurrentTime.ToString("f3");
			}
		}
		#endregion

		#region MusicSlider 변경통지 프로퍼티
		private double _MusicSlider;
		public double MusicSlider
		{
			get
			{
				_MusicSlider = (Music.CurrentTime / Music.Length) * 100;
				return _MusicSlider;
			}
			set
			{
				Music.CurrentTime = Music.Length * (value / 100);
				_MusicSlider = value;
			}
		}
		#endregion

		#region MusicTempo 변경통지 프로퍼티
		public double MusicTempo
		{
			get
			{
				return Music.Tempo;
			}
			set
			{
				Music.Tempo = value;
				RaisePropertyChanged(nameof(MusicTempo));
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

		#region NoteDiv 변경통지 프로퍼티
		private double _NoteDiv;
		public double NoteDiv
		{
			get
			{
				return _NoteDiv;
			}
			set
			{
				_NoteDiv = value;
				RaisePropertyChanged(nameof(NoteDiv));
			}
		}
		#endregion

		#region TimimgTabSelected 변경통지 프로퍼티
		private bool _TimimgTabSelected;
		public bool TimimgTabSelected
		{
			get
			{
				return _TimimgTabSelected;
			}
			set
			{
				//노트와 비트 초기화용
				Music.CurrentTime = Music.CurrentTime;

				_TimimgTabSelected = value;
				RaisePropertyChanged(nameof(TimimgTabSelected));
			}
		}
		#endregion

		#region NoteTabSelected 변경통지 프로퍼티
		private bool _NoteTabSelected;
		public bool NoteTabSelected
		{
			get
			{
				return _NoteTabSelected;
			}
			set
			{
				//노트와 비트 초기화용
				Music.CurrentTime = Music.CurrentTime;

				_NoteTabSelected = value;
				RaisePropertyChanged(nameof(NoteTabSelected));
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

		private DelegateCommand _OrderBPMCommand;
		public ICommand OrderBPMCommand
		{
			get
			{
				return _OrderBPMCommand ?? (_OrderBPMCommand = new DelegateCommand(x => OrderBPM()));
			}
		}

		private DelegateCommand _SetOffsetCommand;
		public ICommand SetOffsetCommand
		{
			get
			{
				return _SetOffsetCommand ?? (_SetOffsetCommand = new DelegateCommand(x => SetOffset()));
			}
		}

		private DelegateCommand _AddTimingCommand;
		public ICommand AddTimingCommand
		{
			get
			{
				return _AddTimingCommand ?? (_AddTimingCommand = new DelegateCommand(x => AddTiming()));
			}
		}

		private DelegateCommand _RemoveTimingCommand;
		public ICommand RemoveTimingCommand
		{
			get
			{
				return _RemoveTimingCommand ?? (_RemoveTimingCommand = new DelegateCommand(x => RemoveTiming()));
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

			Music = new Music();

			TimimgTabSelected = true;
			NoteTabSelected = false;

			_MusicTimer = new DispatcherTimer();
			_MusicTimer.Tick += _MusicTimer_Tick;
			_MusicTimer.Interval = TimeSpan.FromSeconds(0.01);
			_MusicTimer.Start();

			_NoteData = new ObservableCollection<NoteData>();
			//_NoteData = new List<Models.NoteData>();

			_BPMList = new ObservableCollection<BPM>
			{
				new BPM { Value = 0, Offset = 0 }
			};
			_BPMTimingList = new List<int>();
		}

		private void _MusicTimer_Tick(object sender, EventArgs e)
		{
			if (Music.IsActive == PlaybackState.Playing)
			{
				if (IsPlayNote && !TimimgTabSelected)
				{
					var noteList = NoteData
						.OrderBy(x => x.NoteTime)
						.Where(x => x.NoteTime > Music.PreviousNote && x.NoteTime <= Music.CurrentTime);

					var firstNote = noteList.FirstOrDefault();

					noteList = noteList.Where(x => x.NoteTime == firstNote?.NoteTime).ToList();
					
					foreach (var note in noteList)
					{
						PlayNote(note.Note);
						Music.PreviousNote = note.NoteTime;
					}
				}else if (_CurrentBPM.Value != 0)
				{
					if(Music.PreviousBeat < _NextBeatTime && Music.CurrentTime >= _NextBeatTime)
					{
						Music.PreviousBeat = _NextBeatTime;

						if (TimimgTabSelected)
						{
							string beat = "beat.wav";

							if (_BeatMul % 4 == 0)
							{
								beat = "beat2.wav";
							}

							//EntryAssembly/sound/FileName
							var soundPath = Path.Combine(
								Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
								"sound",
								beat
							);

							int channel = Bass.CreateStream(soundPath, 0, 0, BassFlags.Prescan | BassFlags.AutoFree);
							Bass.ChannelPlay(channel);
						}
					}
				}
			}

			RaisePropertyChanged(nameof(MusicTime));
			RaisePropertyChanged(nameof(MusicSlider));
		}

		public void PlayFile()
		{
			Music.Play();
		}

		public void PauseFile()
		{
			if (Music.IsActive == PlaybackState.Playing)
			{
				Music.Pause();
			}
			else
			{
				Music.Play();
			}
		}

		public void StopFile()
		{
			Music.Stop();

			RaisePropertyChanged(nameof(MusicTime));
			RaisePropertyChanged(nameof(MusicSlider));
		}

		public void OpenFile()
		{
			if (!_FileDialog.ShowDialog().Value)
			{
				return;
			}

			Music.Channel = Bass.CreateStream(_FileDialog.FileName, 0, 0, BassFlags.Decode);

			if (Music.Channel == 0)
			{
				FileStatus = "재생할 파일을 선택해주세요.";
				return;
			}

			Music.Channel = BassFx.TempoCreate(Music.Channel, BassFlags.AutoFree | BassFlags.FxFreeSource);

			FileStatus = Path.GetFileName(_FileDialog.FileName);
			Bass.ChannelSetAttribute(Music.Channel, ChannelAttribute.Volume, 0.8);
			Bass.ChannelSetAttribute(Music.Channel, ChannelAttribute.TempoUseQuickAlgorithm, 1);
			Bass.ChannelSetAttribute(Music.Channel, ChannelAttribute.TempoOverlapMilliseconds, 4);
			Bass.ChannelSetAttribute(Music.Channel, ChannelAttribute.TempoSequenceMilliseconds, 30);
		}

		public void Test()
		{

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

		public void InitBPM()
		{
			if (TimimgTabSelected)
			{
				var bpm = SelectedBPM;
				if (bpm.Offset == 0 && Music.CurrentTime != -1)
				{
					bpm.Offset = Convert.ToInt32(Music.CurrentTime * 1000);
					_BPMTemp = bpm.Offset;
					_BPMTimingList.Clear();
				}
				else if (Music.IsActive == PlaybackState.Playing)
				{
					int newOffset = Convert.ToInt32(Music.CurrentTime * 1000);

					//타이밍 5회 이상 입력
					if (_BPMTimingList.Count() > 5)
					{
						int timing = (newOffset - _BPMTemp) - Convert.ToInt32(_BPMTimingList.Average());
						if (timing <= 120)
						{
							_BPMTimingList.Add(newOffset - _BPMTemp);

							bpm.Value = 60.0 / (_BPMTimingList.Average() / 1000.0);

							_BPMTemp = newOffset;

							Music.PreviousBeat = Music.CurrentTime + 1;
						}
						else
						{
							Music.CurrentTime = _BPMTemp / 1000.0;
						}
					}
					else
					{
						_BPMTimingList.Add(newOffset - _BPMTemp);
						_BPMTemp = newOffset;
					}
				} 
			}
		}

		public void OrderBPM()
		{
			BPMList = new ObservableCollection<BPM>(BPMList.OrderBy(x => x.Offset));
		}

		public void SetOffset()
		{
			_SelectedBPM.Offset = Convert.ToInt32(Music.CurrentTime * 1000);
		}

		public void AddTiming()
		{
			BPMList.Add(new BPM { Value = 0, Offset = 0 });
		}

		public void RemoveTiming()
		{
			if (BPMList.Count() > 1)
			{
				BPMList.Remove(SelectedBPM);
			}
		}

		#region KeyBinding
		public void DeleteNote(IList<NoteData> note)
		{
			NoteData = new ObservableCollection<Models.NoteData>(NoteData.Except(note));
			//NoteData = NoteData.Except(note).ToList();
		}

		public void KeyDown()
		{
			if (NoteTabSelected)
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

						double noteTime = Music.CurrentTime;
						double previousBeat = Music.PreviousBeat;
						double nextBeat = _NextBeatTime;

						noteTime =
							Math.Abs(previousBeat - noteTime) < Math.Abs(nextBeat - noteTime)
							? previousBeat
							: nextBeat;

						if (!NoteData.Any(x => x.NoteTime == noteTime && x.Note == note.Value))
						{
							NoteData.Add(
								new NoteData
								{
									Note = note.Value,
									NoteTime = noteTime
								}
							);
							Music.PreviousNote = noteTime;
						}
						RaisePropertyChanged(nameof(NoteData));
					}
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

		#endregion
	}
}
