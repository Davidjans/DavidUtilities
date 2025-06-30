
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class GenericImporterEditor : EditorWindow
{
    public StyleSheet styleSheet;
    
    private enum SourceType { GoogleSheets, FileSelection }
    private SourceType _currentSourceType = SourceType.GoogleSheets;

    private Button googleSheetsTab;
    private Button fileSelectionTab;
    private VisualElement googleSheetsContainer;
    private VisualElement fileSelectionContainer;
    
    private TextField googleSheetUrlField;
    private Label selectedFilePathLabel;
    private HelpBox sourceWarningBox;

    private ObjectField saveFolderField;
    private Button createButton;
    private Label statusLabel;
    
    private string _selectedCsvPath;

    [MenuItem("Tools/Recreated Importer")]
    public static void ShowWindow()
    {
        var window = GetWindow<GenericImporterEditor>();
        window.titleContent = new GUIContent("Import Data");
        window.minSize = new Vector2(500, 550); 
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        if (styleSheet != null) root.styleSheets.Add(styleSheet);
        
        root.Add(new HelpBox("Import and create ScriptableObjects from a CSV or Google Sheets for direct use.", HelpBoxMessageType.Info));

        var sourceSection = new VisualElement();
        sourceSection.AddToClassList("importer-section");
        sourceSection.Add(new Label("Source:"));
        
        var tabBar = new VisualElement();
        tabBar.AddToClassList("tab-bar");
        googleSheetsTab = new Button(() => SetSourceType(SourceType.GoogleSheets)) { text = "Google sheets" };
        fileSelectionTab = new Button(() => SetSourceType(SourceType.FileSelection)) { text = "File Selection" };
        tabBar.Add(googleSheetsTab);
        tabBar.Add(fileSelectionTab);
        sourceSection.Add(tabBar);

        googleSheetsContainer = new VisualElement();
        googleSheetsContainer.Add(new HelpBox("File > Share > Publish to web > Select the sheet instead of entire document > Change web page to CSV > Paste the link in here", HelpBoxMessageType.Info));
        googleSheetUrlField = new TextField("Google sheets link");
        googleSheetUrlField.RegisterValueChangedCallback(_ => UpdateButtonState());
        googleSheetsContainer.Add(googleSheetUrlField);
        sourceSection.Add(googleSheetsContainer);

        fileSelectionContainer = new VisualElement();
        var fileSelectButton = new Button(OnFileSelectionClicked) { text = "Select CSV File" };
        selectedFilePathLabel = new Label("No file selected.") { name = "file-path-label" };
        fileSelectionContainer.Add(fileSelectButton);
        fileSelectionContainer.Add(selectedFilePathLabel);
        sourceSection.Add(fileSelectionContainer);

        sourceWarningBox = new HelpBox("No valid source selected", HelpBoxMessageType.Warning);
        sourceSection.Add(sourceWarningBox);
        root.Add(sourceSection);

        var destinationSection = new VisualElement();
        destinationSection.AddToClassList("importer-section");
        destinationSection.Add(new Label("Destination:"));
        saveFolderField = new ObjectField("Save Folder");
        saveFolderField.objectType = typeof(DefaultAsset);
        saveFolderField.RegisterValueChangedCallback(_ => UpdateButtonState());
        destinationSection.Add(saveFolderField);
        root.Add(destinationSection);
        
        var exampleSection = new VisualElement();
        exampleSection.AddToClassList("importer-section");
        exampleSection.Add(new Label("Example Functionalities"));
        var createCsvButton = new Button(OnCreateCsvStructureClicked) { text = "Create CSV Structure" };
        exampleSection.Add(createCsvButton);
        root.Add(exampleSection);
        
        var footerSection = new VisualElement();
        footerSection.AddToClassList("importer-section");
        createButton = new Button(OnCreateClicked) { text = "Create Mock Objects", name = "create-button" };
        statusLabel = new Label("Status: Idle") { name = "status-label" };
        footerSection.Add(createButton);
        footerSection.Add(statusLabel);
        root.Add(footerSection);
        
        SetSourceType(_currentSourceType);
    }
    
    private void OnCreateCsvStructureClicked()
    {
        string header = "ItemName,Description,ItemType,IsUnique,Value,Weight";
        string exampleRow = "Iron Sword,A basic but reliable sword.,Weapon,FALSE,50,5.5";
        string csvContent = $"{header}\n{exampleRow}";

        string path = EditorUtility.SaveFilePanel("Save CSV Structure Template", "", "importer_template", "csv");

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                File.WriteAllText(path, csvContent);
                statusLabel.text = $"Status: Template saved to {Path.GetFileName(path)}";
                Debug.Log($"CSV template saved successfully at: {path}");
            }
            catch (Exception e)
            {
                statusLabel.text = "Status: Error saving template file.";
                Debug.LogError($"Failed to save CSV template: {e.Message}");
            }
        }
    }
    
    private void SetSourceType(SourceType type)
    {
        _currentSourceType = type;
        
        googleSheetsTab.EnableInClassList("active", type == SourceType.GoogleSheets);
        fileSelectionTab.EnableInClassList("active", type == SourceType.FileSelection);
        
        googleSheetsContainer.style.display = type == SourceType.GoogleSheets ? DisplayStyle.Flex : DisplayStyle.None;
        fileSelectionContainer.style.display = type == SourceType.FileSelection ? DisplayStyle.Flex : DisplayStyle.None;
        
        UpdateButtonState();
    }
    
    private void OnFileSelectionClicked()
    {
        string path = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            _selectedCsvPath = path;
            selectedFilePathLabel.text = Path.GetFileName(path);
        }
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool isSourceValid = false;
        if (_currentSourceType == SourceType.GoogleSheets)
        {
            isSourceValid = !string.IsNullOrWhiteSpace(googleSheetUrlField.value);
        }
        else
        {
            isSourceValid = !string.IsNullOrWhiteSpace(_selectedCsvPath);
        }

        bool isDestinationValid = saveFolderField.value != null;

        sourceWarningBox.style.display = isSourceValid ? DisplayStyle.None : DisplayStyle.Flex;
        createButton.SetEnabled(isSourceValid && isDestinationValid);
    }
    
    private async void OnCreateClicked()
    {
        string destinationPath = AssetDatabase.GetAssetPath(saveFolderField.value);
        if (string.IsNullOrEmpty(destinationPath) || !AssetDatabase.IsValidFolder(destinationPath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid destination folder.", "OK");
            return;
        }

        string csvData = null;
        if (_currentSourceType == SourceType.GoogleSheets)
        {
            csvData = await GetGoogleSheetData();
        }
        else
        {
            csvData = await File.ReadAllTextAsync(_selectedCsvPath);
        }

        if (string.IsNullOrEmpty(csvData))
        {
            statusLabel.text = "Status: Failed to get data.";
            return;
        }

        await ProcessImport(csvData, destinationPath);
    }

    private async Task<string> GetGoogleSheetData()
    {
        var url = googleSheetUrlField.value;
        var match = Regex.Match(url, @"/spreadsheets/d/([a-zA-Z0-9-_]+).*?(?:gid=([0-9]+)|$)");
        if (!match.Success)
        {
            statusLabel.text = "Status: Invalid Google Sheet URL format.";
            return null;
        }
        
        var sheetId = match.Groups[1].Value;
        var gid = match.Groups.Count > 2 && !string.IsNullOrEmpty(match.Groups[2].Value) ? match.Groups[2].Value : "0";
        var exportUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";
        
        statusLabel.text = "Status: Fetching from Google Sheet...";
        
        using (var webRequest = UnityWebRequest.Get(exportUrl))
        {
            var operation = webRequest.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return webRequest.downloadHandler.text;
            }
            else
            {
                statusLabel.text = $"Status: Error - {webRequest.error}";
                return null;
            }
        }
    }
    
    private async Task ProcessImport(string csvData, string destinationFolder)
    {
        var lines = csvData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        statusLabel.text = $"Status: Importing {lines.Length} rows...";
        int createdCount = 0;
        int warningCount = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var columns = line.Split(',').Select(p => p.Trim()).ToList();
            var dataObject = GenericCsvParser.ParseDataRow(columns, out var warnings);

            if (dataObject == null) continue;
            
            warningCount += warnings.Count;

            string assetName = string.IsNullOrEmpty(dataObject.itemName) ? $"New MockDataObject {i}" : dataObject.itemName;
            string assetPath = Path.Combine(destinationFolder, $"{assetName}.asset");
            AssetDatabase.CreateAsset(dataObject, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            createdCount++;

            await Task.Yield();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        statusLabel.text = $"Status: Complete! Created {createdCount} assets with {warningCount} warnings.";
    }
}
#endif