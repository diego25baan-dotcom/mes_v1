#region Using directives

using System;
using System.Collections.Generic;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;
using FTOptix.InfluxDBStore;
using FTOptix.InfluxDBStoreRemote;

#endregion

public class PanelLoaderToPlcTag : BaseNetLogic
{
    public override void Start()
    {
        // Get the PanelLoader object
        panelLoader = InformationModel.Get<PanelLoader>(LogicObject.GetVariable("PanelLoader").Value);
        if (panelLoader == null)
            throw new CoreConfigurationException("PanelLoader object not found");

        currentPanelVariable = panelLoader.GetVariable("CurrentPanel");

        // Get the PageChangeTag variable
        pageChangeTag = InformationModel.GetVariable(LogicObject.GetVariable("PageChangeTag").Value);
        if (pageChangeTag == null)
            throw new CoreConfigurationException("PageChangeTag object not found");

        // Create a new LongRunningTask object to index all pages
        myLongRunningTask = new LongRunningTask(CreatePagesList, LogicObject);
        // Start the LongRunningTask
        myLongRunningTask.Start();
    }

    public override void Stop()
    {
        // Delete the indexing task (if running)
        myLongRunningTask?.Dispose();
        // Unsubscribe from the PageChangeTag variable
        pageChangeTag.VariableChange -= PageChangeTag_VariableChange;
        // Unsubscribe from the CurrentPanel variable
        currentPanelVariable.VariableChange -= CurrentPanel_VariableChange;
    }

    /// <summary>
    /// This method is used to update the list of pages in the project.
    /// It is called when the user wants to refresh the list of pages.
    /// It stops the current indexing task (if running) and creates a new one to index all pages.
    /// </summary>
    [ExportMethod]
    public void UpdatePagesList() 
    {
        // Reset all operations
        Stop();
        // Create a new LongRunningTask object to index all pages
        myLongRunningTask = new LongRunningTask(CreatePagesList, LogicObject);
        // Start the LongRunningTask
        myLongRunningTask.Start();
    }

    /// <summary>
    /// This method is used to index the list of pages in the project starting from a head
    /// node (every subfolder are scanned too) and generate a dictionary where the key is
    /// ScreenId and the value is the NodeIds of the page.
    /// The resulting dictionary is used to change the displayed page based on the ScreenId variable.
    /// </summary>
    private void CreatePagesList()
    {
        // Blank current dictionary of screens
        pagesDictionary.Clear();

        // Get the ScreensFolder variable (head folder where all the pages are located)
        NodeId screensFolderPointer = LogicObject.GetVariable("ScreensFolder").Value;
        var screensFolder = InformationModel.Get(screensFolderPointer);
        if (screensFolder == null)
        {
            Log.Error("ChangePageFromTag", "ScreensFolder object not found");
            return;
        }

        // Get a list of all screens with a "ScreenId" variable
        RecursiveScreensSearch(screensFolder);

        // Subscribe to the PageChangeTag variable
        pageChangeTag.VariableChange += PageChangeTag_VariableChange;
        // Subscribe to the CurrentPanel variable
        currentPanelVariable.VariableChange += CurrentPanel_VariableChange;

        // Trigger the CurrentPanel_VariableChange method to set the initial page (optional)
        CurrentPanel_VariableChange(null, null);
    }

    /// <summary>
    /// This method is used to change the page based on the PageChangeTag variable.
    /// A subscription is set up to listen for changes to the PageChangeTag variable.
    /// If the requested ID is found in the dictionary, the page is changed.
    /// If the requested ID is not found, a warning is logged.
    /// </summary>
    /// <param name="sender">Object which triggered the change</param>
    /// <param name="e">Object that was changed</param>
    private void PageChangeTag_VariableChange(object sender, VariableChangeEventArgs e)
    {
        int newValue;
        try
        {
            // Get the new value of the PageChangeTag variable
            newValue = pageChangeTag.Value;
        }
        catch
        {
            // If the value is not an integer, set it to 0
            newValue = 0;
        }

        if (newValue > 0)
        {
            // Check if the page exists in the dictionary
            var pageExists = pagesDictionary.TryGetValue((uint)newValue, out var destinationPage);

            // If the page does not exist, log a warning, if it exists then change the page
            if (!pageExists)
            {
                Log.Warning("PageChangeTag", "Page with ID [" + pageChangeTag.Value + "] does not exist");
            }
            else
            {
                var newPage = InformationModel.Get(destinationPage);
                if (newPage != null)
                    panelLoader.ChangePanel(newPage);
                else
                    Log.Error("PageChangeTag", "Page with ID [" + pageChangeTag.Value + "] does not exist anymore, call the UpdatePagesList method to refresh the pages dictionary ");
            }
        }
    }

    /// <summary>
    /// This method is used to set the PageChangeTag variable to the ScreenId variable
    /// of the current page whenever it changes from the HMI side (by user interaction for example).
    /// If the current page does not have a ScreenId variable, the PageChangeTag is set to 0.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CurrentPanel_VariableChange(object sender, VariableChangeEventArgs e)
    {
        // Get the current page and set the PageChangeTag value to the ScreenId variable
        // anytime the current page changes to a different object
        var currentPanel = InformationModel.Get(panelLoader.CurrentPanel);
        if (currentPanel != null)
        {
            var screenIdVariable = currentPanel.GetVariable("ScreenId");
            if (screenIdVariable != null)
                pageChangeTag.Value = screenIdVariable.Value;
            else
                pageChangeTag.Value = 0;
        }
    }

    /// <summary>
    /// The method is called recursively to search through all folders and subfolders, starting
    /// from a head node (usually the Screens folder) to index all pages with a valid ScreenId.
    /// For this script, a "page" is any UI container type that has a ScreenId variable.
    /// </summary>
    /// <param name="inputObject">Project node where to start searching for children with a valid ScreenId variable</param>
    private void RecursiveScreensSearch(IUANode inputObject)
    {
        foreach (var childNode in inputObject.Children)
        {
            try
            {
                // If the object is a folder, recursively search it
                if (childNode is FTOptix.Core.Folder)
                {
                    Log.Verbose1("FindPages.Folder", "Found folder with name [" + childNode.BrowseName + "] and Type: [" + childNode.GetType().ToString() + "]");
                    RecursiveScreensSearch(childNode);
                }
                else if (childNode is FTOptix.UI.BaseUIObjectType)
                {
                    // If the object is a UI object type, check if it has a ScreenId variable
                    var screenIdVariable = childNode.GetVariable("ScreenId");
                    if (screenIdVariable == null)
                    {
                        Log.Verbose1("FindPages.Variable", "Found object with name [" + childNode.BrowseName + "] and Type: [" + childNode.GetType().ToString() + "] but it does not have a ScreenId variable");
                        continue;
                    }

                    // If the ScreenId variable is valid, add it to the dictionary
                    int pageId = screenIdVariable.Value;
                    if (pageId > 0)
                    {
                        Log.Debug("FindPages", "Found page with name [" + childNode.BrowseName + "] and ID: [" + pageId.ToString() + "]");
                        if (pagesDictionary.TryGetValue((uint)pageId, out var destinationPage))
                            Log.Warning("FindPages.Page", "Found page with name [" + childNode.BrowseName + "] and duplicate ID: [" + pageId.ToString() + "]");
                        else
                            pagesDictionary.Add((uint)pageId, childNode.NodeId);
                    }
                    else
                    {
                        // If the ScreenId variable is invalid, log a warning to the user
                        Log.Warning("FindPages.Page", "Found page with name [" + childNode.BrowseName + "] and invalid ID: [" + pageId.ToString() + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("FindPages.Catch", "Exception thrown: " + ex.Message);
            }
        }
    }

    private PanelLoader panelLoader;
    private IUAVariable pageChangeTag;
    private IUAVariable currentPanelVariable;
    private readonly Dictionary<uint, NodeId> pagesDictionary = new Dictionary<uint, NodeId>();
    private LongRunningTask myLongRunningTask;
}
