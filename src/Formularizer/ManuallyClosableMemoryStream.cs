using System.IO;

namespace Formularizer
{
	internal class ManuallyClosableMemoryStream : MemoryStream
	{
		public ManuallyClosableMemoryStream()
		{
			CanClose = false;
		}

		public bool CanClose { get; set; }

		public override void Close()
		{
			if (CanClose) base.Close();
		}
	}
}