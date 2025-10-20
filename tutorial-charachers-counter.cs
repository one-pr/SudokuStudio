#:property LangVersion=preview
#:property TargetFramework=net10.0

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

const string folderPath = @"docs\tutorial";
if (!Directory.Exists(folderPath))
{
	// Folder not found.
	return;
}

// Findf for all Markdown files in the specified folder.
var files = Directory.GetFiles(folderPath, "*.md", SearchOption.AllDirectories);
if (files.Length == 0)
{
	return;
}

var totalCount1 = 0L;
var totalCount2 = 0L;
Console.WriteLine("Result:");
Console.WriteLine("--------------------------------------------");
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

	Console.WriteLine($"{file,-50}{count1,10}{count2,10}");
	totalCount1 += count1;
	totalCount2 += count2;
}

Console.WriteLine("--------------------------------------------");
Console.WriteLine($"Total files count: {files.Length}");
Console.WriteLine($"Characters count: {totalCount2}");
Console.WriteLine($"Characters count (CJK characters considered): {totalCount1}");


static bool isCjk(char c)
	=> c >= '\u4E00' && c <= '\u9FFF'
	|| c >= '\u3400' && c <= '\u4DBF'
	|| c >= '\uF900' && c <= '\uFAFF'
	|| c >= '\u3040' && c <= '\u309F'
	|| c >= '\u30A0' && c <= '\u30FF'
	|| c >= '\uAC00' && c <= '\uD7AF';
