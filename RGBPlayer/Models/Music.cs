using ManagedBass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBPlayer.Models
{
	public class Music
	{
		public string Name { get; set; }

		private int _Channel;
		public int Channel
		{
			get
			{
				return _Channel;
			}
			set
			{
				Bass.SampleFree(_Channel);
				_Channel = value;
			}
		}
		public double CurrentTime
		{
			get
			{
				return Bass.ChannelBytes2Seconds(Channel, Bass.ChannelGetPosition(Channel));
			}
			set
			{
				PreviousNote = value;
				PreviousBeat = value;
				Bass.ChannelSetPosition(Channel, Bass.ChannelSeconds2Bytes(Channel, value));
			}
		}
		public double Length
		{
			get
			{
				return Bass.ChannelBytes2Seconds(Channel, Bass.ChannelGetLength(Channel));
			}
		}
		private double _Tempo;
		public double Tempo
		{
			get
			{
				return _Tempo;
			}
			set
			{
				_Tempo = value;
				Bass.ChannelSetAttribute(Channel, ChannelAttribute.Tempo, Tempo);
			}
		}

		public PlaybackState IsActive => Bass.ChannelIsActive(Channel);

		public double PreviousNote { get; set; }
		public double PreviousBeat { get; set; }

		public void Play()
		{
			Bass.ChannelPlay(Channel);
		}

		public void Pause()
		{
			Bass.ChannelPause(Channel);
		}

		public void Stop()
		{
			Bass.ChannelPause(Channel);
			CurrentTime = 0;
		}
	}
}
