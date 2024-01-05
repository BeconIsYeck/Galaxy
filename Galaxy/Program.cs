using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Pastel;

using static Galaxy.Lines;


namespace Galaxy;

public static class Program {
	static readonly Color prmCol = Color.FromArgb(150, 0, 225);
	static readonly Color secCol = Color.FromArgb(200, 20, 150);

	static List<string> lines = [];

	static int bufferMin = 0;
	static int bufferMax = Console.WindowHeight - 1;

	static readonly int x = 6;
	static int          y = 1;

	static string dir = "";

	static void movePrint(bool scroll) {
		for (var i = bufferMin; i < bufferMax; i++) {
			if (i < lines.Count)
				Console.WriteLine(lines[i].Pastel(prmCol));
		}

		y = scroll ? y : 1;
	}

	public static void Main(string[] args) {
		Console.Title = "Galaxy";

		var drives = DriveInfo.GetDrives();

		AddAndPrint("@", TransferText(drives, (x) => "\t" + x.Name), ref lines, bufferMin, bufferMax, prmCol);

		while (true) {
			if (Console.KeyAvailable) {
				var key = Console.ReadKey(true);
				var lY = bufferMax - (Console.WindowHeight - y) + 1;
				try {
					switch (key.Key) {
						//-- Get Movement --//

						/*
						 *  Up
						 */

						case ConsoleKey.UpArrow or ConsoleKey.W: {
							if (lines.Count != 1) {
								if (y == 0) {
									if (bufferMin != 0) {
										bufferMin = Math.Clamp(bufferMin - 1, 0, lines.Count);
										bufferMax = Math.Clamp(bufferMax - 1, 0, lines.Count);

										Console.Clear();

										movePrint(true);
									}
								}
								else {
									var min = (lines.Count >= bufferMax) ? 0 : 1;

									var newY = Math.Clamp(y - 1, min, lines.Count - 1);

									y = newY;
								}
							}
						}
						break;

						/*
						 *  Down
						 */

						case ConsoleKey.DownArrow or ConsoleKey.S: {
							if (lines.Count != 1) {
								if (y == Console.WindowHeight - 1) {
									if (bufferMax != lines.Count) {
										bufferMin = Math.Clamp(bufferMin + 1, 0, lines.Count);
										bufferMax = Math.Clamp(bufferMax + 1, 0, lines.Count);

										Console.Clear();

										movePrint(true);
									}
								}
								else {
									var min = (lines.Count > bufferMax) ? 0 : 1;
									var max = (lines.Count - 1 > Console.WindowHeight) ? Console.WindowHeight - 1 : lines.Count - 1;

									var newY = Math.Clamp(y + 1, min, max);

									y = newY;
								}
							}
						}
						break;

						//-- Binds --//

						/*
						 *  Navigate in / open
						 */

						case ConsoleKey.Enter or ConsoleKey.E: {
							var possibleFilePath = dir + "\\" + lines[lY].Substring(1, lines[lY].Length - 1);

							// Open file
							if (lines[bufferMax - (Console.WindowHeight - y) + 1].Length != 4 && File.Exists(possibleFilePath)) {
								bufferMin = 0;
								bufferMax = Console.WindowHeight - 1;

								Console.Clear();
								Console.WriteLine($"Opened file: '{possibleFilePath}'".Pastel(secCol));
								Console.WriteLine("Refresh (/ | R)".Pastel(secCol));

								var psi = new ProcessStartInfo("cmd.exe");

								psi.Verb = "open";
								psi.UseShellExecute = true;
								psi.Arguments = $"/C start \"\" \"{possibleFilePath}\"\"";

								var pro = Process.Start(psi);
							}
							// Open directory
							else {
								bufferMin = 0;
								bufferMax = Console.WindowHeight - 1;

								dir += lines[lY].Remove(0, 1);

								string[] alldirs = Directory.GetDirectories(dir);
								string[] allFiles = Directory.GetFiles(dir);

								var dirsAndFiles =
									TransferText(alldirs, (x) => x.Replace(dir, ""))
									.Concat(
									TransferText(allFiles, (x) => Path.GetFileName(x)))
									.ToArray();

								AddAndPrint("@ " + dir, TransferText(dirsAndFiles, (x) => "\t" + x), ref lines, bufferMin, bufferMax, prmCol);

								y = 1;
							}
						}
						break;

						/*
						 *  Navigate out
						 */

						case ConsoleKey.Backspace or ConsoleKey.Q: {
							var i = 0;
							var back = 0;

							foreach (var c in dir) {
								if (c == '\\') {
									back = i;
								}

								i++;
							}

							if (dir.Length == 3 && IsDrive(dir.Remove(2))) {
								bufferMin = 0;
								bufferMax = Console.WindowHeight - 1;

								dir = "";

								AddAndPrint("@ " + dir, TransferText(drives, (x) => "\t" + x.Name), ref lines, bufferMin, bufferMax, prmCol);

								y = 1;
							}
							else {
								bufferMin = 0;
								bufferMax = Console.WindowHeight - 1;

								dir = dir.Remove(back);

								if (IsDrive(dir)) {
									dir += "\\";
								}

								string[] alldirs = Directory.GetDirectories(dir);
								string[] allFiles = Directory.GetFiles(dir);

								var dirsAndFiles =
									TransferText(alldirs, (x) => x.Replace(dir, ""))
									.Concat(
									TransferText(allFiles, (x) => Path.GetFileName(x)))
									.ToArray();

								AddAndPrint("@ " + dir, TransferText(dirsAndFiles, (x) => "\t" + x), ref lines, bufferMin, bufferMax, prmCol);

								y = 1;

							}
						}
						break;

						/*
						 *  Refresh
						 */

						case ConsoleKey.Oem2 or ConsoleKey.R: {
							bufferMin = 0;
							bufferMax = Console.WindowHeight - 1;

							string[] alldirs = Directory.GetDirectories(dir);
							string[] allFiles = Directory.GetFiles(dir);

							var dirsAndFiles =
								TransferText(alldirs, (x) => x.Replace(dir, ""))
								.Concat(
								TransferText(allFiles, (x) => Path.GetFileName(x)))
								.ToArray();

							AddAndPrint("@ " + dir, TransferText(dirsAndFiles, (x) => "\t" + x), ref lines, bufferMin, bufferMax, prmCol);

							y = 1;
						}
						break;

						/*
						 *  New File
						 */

						case ConsoleKey.OemPeriod or ConsoleKey.D: {
							Console.Clear();

							Console.WriteLine("New File".Pastel(secCol));

							var fileName = Console.ReadLine();

							File.Create(dir + "\\" + fileName);

							Console.WriteLine($"File: '{fileName}' created in: '{dir}\\'".Pastel(secCol));

							Console.WriteLine("\nRefresh (/ | R)".Pastel(secCol));

						}
						break;

						/*
						 *  New Directory
						 */

						case ConsoleKey.OemComma or ConsoleKey.A: {
							Console.Clear();

							Console.WriteLine("New Folder".Pastel(secCol));

							var dirName = Console.ReadLine();

							if (!Directory.Exists(dir + "\\" + dirName)) {
								Directory.CreateDirectory(dir + "\\" + dirName);

								Console.WriteLine($"Folder: '{dirName}' created in '{dir}\\'");
							}
							else {
								Console.WriteLine("Folder: {dir}\\{dirName} already exists".Pastel(secCol));
							}

							Console.WriteLine("\nRefresh (/ | R)".Pastel(secCol));
						}
						break;

						/*
						 *  Delete
						 */

						case ConsoleKey.Delete or ConsoleKey.F: {
							var aY = bufferMax - (Console.WindowHeight - y) + 1;

							var delPath = lines[aY].Substring(1, lines[aY].Length - 1);

							if (delPath.First() == '\\' || delPath.ToCharArray()[^1] == '\\') {
								delPath = dir + "" + delPath;
							}
							else {
								delPath = dir + "\\" + delPath;
							}

							Console.Clear();

							var filOrDir = IsFileOrDir(delPath);

							if (filOrDir == 1)
								Console.WriteLine($"Delete file: '{delPath}'".Pastel(secCol));

							if (filOrDir == 2)
								Console.WriteLine($"Delete folder: '{delPath}'".Pastel(secCol));


							Console.Write("Yes or no (".Pastel(secCol));
							Console.Write("y".Pastel(Color.FromArgb(0, 200, 0)));
							Console.Write("/".Pastel(secCol));
							Console.Write("n".Pastel(Color.FromArgb(200, 0, 0)));
							Console.Write("): ".Pastel(secCol));

							var delOp = Console.ReadLine();

							if (delOp != "y" && delOp != "n") {
								Console.WriteLine("Invalid option.");
							}
							else {
								if (delOp == "y") {
									try {
										if (filOrDir == 1) {
											Console.WriteLine("File deleted, deleting...".Pastel(secCol));

											File.Delete(delPath);
										}
										else if (filOrDir == 2) {
											Console.WriteLine("Folder detected, deleting...".Pastel(secCol));

											Directory.Delete(delPath);
										}
										else
											Console.WriteLine("Invalid path".Pastel(secCol));
									}
									catch (Exception e) {
										var bsCol = Console.ForegroundColor;

										Console.Clear();
										Console.ForegroundColor = ConsoleColor.Red;
										Console.WriteLine(e);

										Console.ForegroundColor = bsCol;
									}
								}
								else
									Console.WriteLine("No changes applied.".Pastel(secCol));

								Console.WriteLine("\nRefresh (/ | R)".Pastel(secCol));
							}
						}
						break;

						//-- Escape --//

						case ConsoleKey.Escape: {
							Console.Clear();
							Environment.Exit(0);
						}
						break;
					}
				}
				catch { }
			}

			//-- Set Movement --//

			try {
				Console.SetCursorPosition(x, y);
			}
			catch (Exception e) {
				var bsCol = Console.ForegroundColor;

				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(e.ToString());

				Console.ForegroundColor = bsCol;
			}
		}
	}
}