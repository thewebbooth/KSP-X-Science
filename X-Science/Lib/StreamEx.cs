using System;
using System.IO;



namespace ScienceChecklist
{
	/// <summary>
	/// Contains extension methods on the Stream class.
	/// </summary>
	internal static class StreamEx {
		/// <summary>
		/// Reads the Stream to the end and returns a byte array containing the contents of the Stream.
		/// </summary>
		/// <param name="stream">The stream to be read.</param>
		/// <returns>A byte array containing the contents of the Stream.</returns>
		public static byte[] ReadToEnd (this Stream stream) {
			long originalPosition = 0;

			if (stream.CanSeek) {
				originalPosition = stream.Position;
				stream.Position = 0;
			}

			try {
				var readBuffer = new byte[4096];

				int totalBytesRead = 0;
				int bytesRead;

				while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0) {
					totalBytesRead += bytesRead;

					if (totalBytesRead == readBuffer.Length) {
						int nextByte = stream.ReadByte();
						if (nextByte != -1) {
							var temp = new byte[readBuffer.Length * 2];
							Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
							Buffer.SetByte(temp, totalBytesRead, (byte) nextByte);
							readBuffer = temp;
							totalBytesRead++;
						}
					}
				}

				var buffer = readBuffer;
				if (readBuffer.Length != totalBytesRead) {
					buffer = new byte[totalBytesRead];
					Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
				}
				return buffer;
			} finally {
				if (stream.CanSeek) {
					stream.Position = originalPosition;
				}
			}
		}
	}
}
