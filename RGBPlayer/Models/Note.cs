using RGBPlayer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RGBPlayer.Models
{
	public class Note : IIdentifiable
	{
		public string Key => NoteName;

		public string NoteName { get; }
		public string FileName { get; }
		public string ScriptName { get; }

		public Key Input1 { get; }
		public bool IsInput1Down { get; set; }

		public Key Input2 { get; }
		public bool IsInput2Down { get; set; }

		public Note(string note, string file, string script, Key in1, Key in2)
		{
			NoteName = note;
			FileName = file;
			ScriptName = script;

			Input1 = in1;
			Input2 = in2;
			IsInput1Down = false;
			IsInput2Down = false;
		}
	}
}
