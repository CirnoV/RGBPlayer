using RGBPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RGBPlayer.Core
{
    public class RGBData
	{
		public static RGBData Current { get; } = new RGBData();
		
		public MasterTable<Note> NoteList { get; private set; }

		public void Initialize()
		{
			List<Note> note = new List<Note>();

			note.Add(new Note("Normal", "hitnote.mp3", "Normal_Note_Array", Key.A, Key.S));
			note.Add(new Note("Clap", "hitnote2.mp3", "Clap_Note_Array", Key.F, Key.D));
			note.Add(new Note("Symbols", "hitnote3.mp3", "Symbols_Note_Array", Key.G, Key.H));

			NoteList = new MasterTable<Note>(note);
		}
	}
}
