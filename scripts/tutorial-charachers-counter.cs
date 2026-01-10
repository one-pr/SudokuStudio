#pragma warning disable format
#:property LangVersion=preview
#:property TargetFramework=net10.0
#:property ImplicitUsings=false
#:property Nullable=enable
#pragma warning restore format

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

const int trimmedLength = 70;
const string fileFilter = "*.md";
const string folderPath = @"..\docs\tutorial";
const string firstColumnTitle = "File";
const string secondColumnTitle = "Characters (CJK considered)";
const string thirdColumnTitle = "Characters";
const string fourthColumnTitle = "Images";

if (!Directory.Exists(folderPath))
{
	// Folder not found.
	return;
}

// Find for all Markdown files in the specified folder.
var files = Directory.GetFiles(folderPath, fileFilter, SearchOption.AllDirectories);
if (files.Length == 0)
{
	return;
}

var totalCount1 = 0L;
var totalCount2 = 0L;
var totalCount3 = 0L;
var firstColumnMaxWidth = int.MinValue;
var secondColumnMaxWidth = int.MinValue;
var thirdColumnMaxWidth = int.MinValue;
var fourthColumnMaxMaxWidth = int.MinValue;
var values = new List<(string File, long Count1, long Count2, long Count3)>();
foreach (var file in files)
{
	var content = File.ReadAllText(file);
	var count1 = 0L;
	var count2 = 0L;
	foreach (var c in content)
	{
		if (isCjk(c))
		{
			count1++;
			count2 += 2;
		}
		else
		{
			count1++;
			count2++;
		}
	}

	var count3 = ImagePattern.Matches(content).Count;

	var filePath = file.Length >= trimmedLength ? $"{file[..(trimmedLength - 2)]}.." : file;
	if (filePath.Length > firstColumnMaxWidth)
	{
		firstColumnMaxWidth = filePath.Length;
	}
	if (count1.ToString().Length > secondColumnMaxWidth)
	{
		secondColumnMaxWidth = count1.ToString().Length;
	}
	if (count2.ToString().Length > thirdColumnMaxWidth)
	{
		thirdColumnMaxWidth = count2.ToString().Length;
	}
	if (count3.ToString().Length > fourthColumnMaxMaxWidth)
	{
		fourthColumnMaxMaxWidth = count3.ToString().Length;
	}

	values.Add((filePath, count1, count2, count3));
	totalCount1 += count1;
	totalCount2 += count2;
	totalCount3 += count3;
}

if (firstColumnTitle.Length > firstColumnMaxWidth)
{
	firstColumnMaxWidth = firstColumnTitle.Length;
}
if (secondColumnTitle.Length > secondColumnMaxWidth)
{
	secondColumnMaxWidth = secondColumnTitle.Length;
}
if (thirdColumnTitle.Length > thirdColumnMaxWidth)
{
	thirdColumnMaxWidth = thirdColumnTitle.Length;
}
if (fourthColumnTitle.Length > fourthColumnMaxMaxWidth)
{
	fourthColumnMaxMaxWidth = fourthColumnTitle.Length;
}

// Print values.
var sb = new StringBuilder();
sb.AppendLine("Result:");
sb.AppendLine($"{firstColumnTitle.PadLeft(firstColumnMaxWidth)} | {secondColumnTitle.PadLeft(secondColumnMaxWidth)} | {thirdColumnTitle.PadLeft(thirdColumnMaxWidth)} | {fourthColumnTitle.PadLeft(fourthColumnMaxMaxWidth)}");
sb.AppendLine(new string('-', firstColumnMaxWidth + secondColumnMaxWidth + thirdColumnMaxWidth + fourthColumnMaxMaxWidth + 9));
foreach (var (file, count1, count2, count3) in values)
{
	sb.AppendLine($"{file.PadLeft(firstColumnMaxWidth)} | {count1.ToString().PadLeft(secondColumnMaxWidth)} | {count2.ToString().PadLeft(thirdColumnMaxWidth)} | {count3.ToString().PadLeft(fourthColumnMaxMaxWidth)}");
}
sb.AppendLine(new string('-', firstColumnMaxWidth + secondColumnMaxWidth + thirdColumnMaxWidth + fourthColumnMaxMaxWidth + 9));
sb.AppendLine($"{files.Length.ToString().PadLeft(firstColumnMaxWidth)} | {totalCount1.ToString().PadLeft(secondColumnMaxWidth)} | {totalCount2.ToString().PadLeft(thirdColumnMaxWidth)} | {totalCount3.ToString().PadLeft(fourthColumnMaxMaxWidth)}");
Console.WriteLine(sb.ToString());


static bool isCjk(char c)
	=> c >= '\u4E00' && c <= '\u9FFF'
	|| c >= '\u3400' && c <= '\u4DBF'
	|| c >= '\uF900' && c <= '\uFAFF'
	|| c >= '\u3040' && c <= '\u309F'
	|| c >= '\u30A0' && c <= '\u30FF'
	|| c >= '\uAC00' && c <= '\uD7AF';


internal static partial class Program
{
	[GeneratedRegex("""(?:!\[.*\]\(.*\)|<figure>.*?<\/figure>)""", RegexOptions.Singleline | RegexOptions.Compiled)]
	private static partial Regex ImagePattern { get; }
}
