using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBPlayer.Models
{
    public class NoteData
    {
		public Note Note { get; set; }
		public double NoteTime { get; set; }

		public string NoteTimeText => NoteTime.ToString(@"f3");
    }
}
