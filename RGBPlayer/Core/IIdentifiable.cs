using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBPlayer.Core
{
	/// <summary>
	/// 문자열로 요소를 고유 식별할 수 있도록 합니다.
	/// </summary>
	public interface IIdentifiable
	{
		string Key { get; }
	}
}
