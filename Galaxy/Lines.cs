using Pastel;
using System;
using System.Drawing;


namespace Galaxy;

internal static class Lines {
	public static int IsFileOrDir(string path) {
		if (File.Exists(path))
			return 1;

		if (Directory.Exists(path))
			return 2;

		return 0;
	}

	public static bool IsDrive(string mDrive) =>
		mDrive.Length == 2 && mDrive.Last() == ':';

	public static string[] TransferText<T>(T[] array, Func<T, string> transfer) {
		var ret = new string[array.Length];

		var i = 0;

		foreach (T v in array) {
			ret[i] = transfer(v);

			i++;
		}

		return ret;
	}

	public static void AddAndPrint(string start, string[] text, ref List<string> lines, int bufferMin, int bufferMax, Color txtColor) {
		lines.Clear();

		lines.Add(start);

		foreach (var t in text) {
			lines.Add(t);
		}

		var linesCln = new List<string>();

		foreach (var l in lines) {
			linesCln.Add(l);
		}

		Console.Clear();

		for (var i = bufferMin; i < bufferMax && i < linesCln?.Count; i++) {
			Console.WriteLine(linesCln[i].Pastel(txtColor));
		}
	}
}