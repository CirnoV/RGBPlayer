using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RGBPlayer.Core
{
	/// <summary>
	/// 프로퍼티 변경통지를 도움
	/// </summary>
	public class Notifier : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void PropertyEvent(string PropertyName, Action Handler, bool RaiseRegistered = false)
		{
			this.PropertyChanged += (_, e) =>
			{
				if (PropertyName == e.PropertyName)
					Handler?.Invoke();
			};

			if (RaiseRegistered)
				Handler?.Invoke();
		}
	}
}
