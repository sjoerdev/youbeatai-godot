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

	public void ReadTemplates()
	{
		var tuple = LoadTextFilesInDirectory("templates");

		names = new();
		contents = new();
		actives = new();

		names = tuple.names;
		contents = tuple.contents;
		actives = tuple.actives;
	}

	public override void _Ready()
	{
		instance ??= this;

		leftTemplateButton.Pressed += PreviousTemplate;
		rightTemplateButton.Pressed += NextTemplate;
		templateButton.Pressed += SetTemplate;
		
        ReadTemplates();
		templateButton.Text = names[currentTemplate];
	}

    public void CreateNewTemplate(string name, bool[,] actives)
    {
        string folderPath = "res://templates/";
        string filePath = folderPath + name + ".txt";

        var dir = DirAccess.Open(folderPath);
        if (dir.FileExists(filePath)) dir.Remove(filePath);

        string[] rowLabels = { "a", "b", "c", "d" };
        string formattedContent = "";
        for (int i = 0; i < 4; i++)
        {
            formattedContent += rowLabels[i];
            for (int j = 0; j < 32; j++)
            {
                formattedContent += actives[i, j] ? "1" : "0";
            }
            if (i < 3) formattedContent += "\n";
        }

        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(formattedContent);
        GD.Print($"Template {name}.txt created at {filePath}");
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
		if (fileName.EndsWith(".txt"))
		{
			string filePath = folderPath + fileName;
			var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

			tempNames.Add(fileName);
			tempContents.Add(file.GetAsText());
			tempActives.Add(ToActives(file.GetAsText()));
		}
		fileName = dir.GetNext();
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
