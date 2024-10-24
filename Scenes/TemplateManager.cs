using Godot;
using System;
using System.Collections.Generic;

public partial class TemplateManager : Node
{
	[Export] Button templateButton;
	[Export] Button leftTemplateButton;
	[Export] Button rightTemplateButton;

	public static TemplateManager instance = null;

	public List<string> names = null;
	public List<string> contents = null;
	public List<bool[,]> actives = null;

	public int currentTemplate = 4;
	public bool[,] GetCurrentActives() => actives[currentTemplate];

	void PreviousTemplate()
	{
		currentTemplate--;
		if (currentTemplate < 0) currentTemplate = names.Count - 1;
		templateButton.Text = names[currentTemplate];
	}

	void NextTemplate()
	{
		currentTemplate++;
		if (currentTemplate == names.Count) currentTemplate = 0;
		templateButton.Text = names[currentTemplate];
	}
	
	void SetTemplate() => Manager.instance.beatActives = GetCurrentActives();

	public override void _Ready()
	{
		instance ??= this;

		leftTemplateButton.Pressed += PreviousTemplate;
		rightTemplateButton.Pressed += NextTemplate;
		templateButton.Pressed += SetTemplate;
		
        var tuple = LoadTextFilesInDirectory("templates");
		names = tuple.names;
		contents = tuple.contents;
		actives = tuple.actives;

		templateButton.Text = names[currentTemplate];
	}

    (List<string> names, List<string> contents, List<bool[,]> actives) LoadTextFilesInDirectory(string folder)
    {
        string folderPath = $"res://{folder}/";
        using var dir = DirAccess.Open(folderPath);

        List<string> tempNames = new();
        List<string> tempContents = new();
		List<bool[,]> tempActives = new();
		
       	dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (!string.IsNullOrEmpty(fileName))
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".txt"))
			{
				string filePath = folderPath + fileName;
				using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

				tempNames.Add(fileName);
				tempContents.Add(file.GetAsText());
				tempActives.Add(ToActives(file.GetAsText()));
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();

        return (tempNames, tempContents, tempActives);
    }

    private bool[,] ToActives(string content)
    {
        string[] lines = content.Trim().Split('\n');
        bool[,] boolArray = new bool[4, 32];

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim().Substring(1);
            for (int j = 0; j < line.Length && j < 32; j++)
            {
                boolArray[i, j] = line[j] == '1';
            }
        }

        return boolArray;
    }
}
