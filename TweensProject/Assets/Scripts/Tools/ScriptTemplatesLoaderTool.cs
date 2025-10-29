#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Author : Auguste Paccapelo

public class ScriptTemplatesLoaderTool : EditorWindow
{
    // Templates
    // Path of your templates
    private const string TEMPLATES_FOLDER_PATH = "C:\\Documents\\Programmation\\Templates\\Unity\\Scripts";
    // Template availables Macros
    private const string KEYWORD_REPLACEMENT_SCRIPTNAME = "#SCRIPTNAME#";
    private const string KEYWORD_REPLACEMENT_PROJECTNAME = "#PROJECTNAME#";
    private const string KEYWORD_REPLACEMENT_HERITAGE = "#BASE#";

    private const string EXTENSION_CS = ".cs";
    private const string DEFAULT_TEMPLATE_DESTINATION_FOLDER = "Assets";
    private const string STRING_TOREMOVE_FINAL_PATH = "Assets";

    // Window
    // Where button will be in the Editor
    private const string MENU_PATH = "Assets/Create/MyTemplates";
    private const string WINDOW_NAME = "AllCustomTemplates";
    private const string DEFAULT_SCRIPT_NAME_MESSAGE = "Script's name : ";
    private const string DEFAULT_HERITAGE_MESSAGE = "Heritage : ";
    private const string DEFAULT_HERITAGE = "MonoBehaviour";
    private const string BUTTON_CREATE_SCRIPT_TEXT = "Create";
    private const string DEFAULT_SCRIPT_NAME = "NewScript";
    private const string CHOOSE_TEMPLATE_TEXT = "Choose a template : ";
    private const string TEMPLATES_BUTTON_TYPE = "button";

    private string templatesFolderPath;
    private List<string> allTemplatesNames;
    string currentTemplateChoose;
    private string currentScriptName;
    private string currentScriptHeritage;

    [MenuItem(MENU_PATH)]
    private static void OpenTemplatesWindow()
    {
        // Create the window
        EditorWindow window = GetWindow<ScriptTemplatesLoaderTool>(true, WINDOW_NAME);
        // Show the window
        window.ShowUtility();
    }

    // Call one time when window open
    private void OnEnable()
    {
        //templatesFolderPath = Path.Combine(EditorApplication.applicationContentsPath, TEMPLATES_FOLDER_PATH);
        templatesFolderPath = TEMPLATES_FOLDER_PATH;
        allTemplatesNames = GetAllTemplatesNames(templatesFolderPath);
        currentScriptName = DEFAULT_SCRIPT_NAME;
        currentScriptHeritage = DEFAULT_HERITAGE;
        currentTemplateChoose = "";
    }

    // Call each frame (Unity reprint all interface each frame, so you can put your text, buttons, etc, only here)
    private void OnGUI()
    {
        EditorGUILayout.LabelField(CHOOSE_TEMPLATE_TEXT, EditorStyles.boldLabel);

        CreateTemplatesButtons();

        CreateScriptNameField();

        CreateScriptHeritageField();

        CreateCreateButton();
    }

    private void CreateTemplatesButtons()
    {
        foreach (string templateName in allTemplatesNames)
        {
            if (GUILayout.Toggle(currentTemplateChoose == templateName, templateName, TEMPLATES_BUTTON_TYPE))
            {
                currentTemplateChoose = templateName;
                if (currentScriptName == DEFAULT_SCRIPT_NAME || allTemplatesNames.Contains(currentScriptName)) currentScriptName = templateName;
            }
        }
    }

    private void CreateScriptNameField()
    {
        currentScriptName = EditorGUILayout.TextField(DEFAULT_SCRIPT_NAME_MESSAGE, currentScriptName);
    }

    private void CreateScriptHeritageField()
    {
        currentScriptHeritage = EditorGUILayout.TextField(DEFAULT_HERITAGE_MESSAGE, currentScriptHeritage);
    }

    private void CreateCreateButton()
    {
        if (currentTemplateChoose != "" && GUILayout.Button(BUTTON_CREATE_SCRIPT_TEXT))
        {
            CreateScriptFromTemplate(currentScriptName, currentScriptHeritage, currentTemplateChoose, templatesFolderPath, EXTENSION_CS);
            Close();
        }
    }

    private static void CreateScriptFromTemplate(string scriptName, string scriptHeritage, string templateName, string templateFolderPath, string fileExtension)
    {
        // Get all needed variables
        string destinationFolder = GetDestinationFolder();
        string filePath = Path.Combine(destinationFolder, scriptName) + fileExtension;
        string scriptContent = GetTemplateContent(templateName, templateFolderPath, scriptName, scriptHeritage, fileExtension);
        // Create Script
        File.WriteAllText(filePath, scriptContent);
        // Force Unity to load and update the script created
        AssetDatabase.Refresh();
        string relativePath = filePath.Replace(Application.dataPath, STRING_TOREMOVE_FINAL_PATH);
        var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
        Selection.activeObject = asset;
    }

    private static string GetDestinationFolder()
    {
        string folderPath = null;
        Object[] allObjectsSelected = Selection.GetFiltered<Object>(SelectionMode.Assets);
        if (allObjectsSelected.Length > 0)
        {
            // Get folder currently selected
            Object obj = allObjectsSelected.First();
            folderPath = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(folderPath)) folderPath = Path.GetDirectoryName(folderPath);
        }
        if (folderPath is null) folderPath = DEFAULT_TEMPLATE_DESTINATION_FOLDER;
        // Get absolute path to folder
        string fullPath = Application.dataPath;
        int numCharacToRemove = STRING_TOREMOVE_FINAL_PATH.Length;
        fullPath = fullPath.Substring(0, fullPath.Length - numCharacToRemove);
        fullPath = Path.Combine(fullPath, folderPath);

        return fullPath;
    }

    private static string GetTemplateContent(string templateName, string templatePath, string scriptName, string scriptHeritage, string fileExtension)
    {
        string path = Path.Combine(templatePath, templateName) + fileExtension;
        if (!File.Exists(path))
        {
            Debug.Log("Template not found.");
            return "";
        }

        string scriptContent = File.ReadAllText(path);
        // Replace all key words
        scriptContent = scriptContent.Replace(KEYWORD_REPLACEMENT_PROJECTNAME, Application.productName)
            .Replace(KEYWORD_REPLACEMENT_SCRIPTNAME, scriptName).Replace(KEYWORD_REPLACEMENT_HERITAGE, scriptHeritage);
        return scriptContent;
    }

    private static List<string> GetAllTemplatesNames(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.Log("Folder not found.");
            return null;
        }
        List<string> templatesNames = new List<string>();
        string[] allTemplatesPaths = Directory.GetFiles(folderPath);
        foreach (string templatePath in allTemplatesPaths)
        {
            templatesNames.Add(Path.GetFileNameWithoutExtension(templatePath));
        }
        return templatesNames;
    }
}
#endif