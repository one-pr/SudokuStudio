#!usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property LangVersion=preview
#:property Nullable=enable
#:property ImplicitUsings=false

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

var assetsDir = @"./tutorial/.gitbook/assets";
var markdownDir = @"./tutorial";

// Step 1: Find all PNG files and sort them.
var pngFiles = Directory.GetFiles(assetsDir, "*.png").OrderBy(Path.GetFileName).ToList();

// Step 2: Rename to temporary names to avoid conflicts.
var tempMapping = new Dictionary<string, string>();
var index = 0;
foreach (var file in pngFiles)
{
	var oldName = Path.GetFileName(file);
	var tempName = $"temp_{index:D4}.png";
	var tempPath = Path.Combine(assetsDir, tempName);

	File.Move(file, tempPath);
	tempMapping[oldName] = tempName;
	index++;
}

// Step 3: Convert temporary names to final names.
var finalMapping = new Dictionary<string, string>();
index = 0;
var tempFiles = Directory.GetFiles(assetsDir, "temp_*.png").OrderBy(Path.GetFileName);

foreach (var file in tempFiles)
{
	var oldTempName = Path.GetFileName(file);
	var finalName = $"images_{index:D4}.png";
	var finalPath = Path.Combine(assetsDir, finalName);

	File.Move(file, finalPath);
	finalMapping[oldTempName] = finalName;
	index++;
}

// Step 4: Build old-to-new mapping.
var oldToNewMap = new Dictionary<string, string>();
foreach (var oldName in tempMapping.Keys)
{
	oldToNewMap[oldName] = finalMapping[tempMapping[oldName]];
}

// Step 5: Update Markdown files.
var mdFiles = Directory.GetFiles(markdownDir, "*.md", SearchOption.AllDirectories);
foreach (var mdFile in mdFiles)
{
	var content = File.ReadAllText(mdFile, Encoding.UTF8);
	foreach (var kvp in oldToNewMap)
	{
		var oldPath = $"../.gitbook/assets/{kvp.Key}";
		var newPath = $"../.gitbook/assets/{kvp.Value}";
		content = content.Replace(oldPath, newPath);
	}
	File.WriteAllText(mdFile, content, new UTF8Encoding(false));
}

Console.WriteLine("Renaming and reference update completed.");
