using RGBPlayer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBPlayer.Models
{
	public class BPM : Notifier
	{
		private double _Value;
		public double Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				RaisePropertyChanged(nameof(Value));
			}
		}
		private int _Offset;
		public int Offset
		{
			get
			{
				return _Offset;
			}
			set
			{
				_Offset = value;
				RaisePropertyChanged(nameof(Offset));
			}
		}
	}
}
