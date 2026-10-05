using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System;

/* 
CyberCANOE Virtual Reality API for Unity3D
(C) 2016 Ryan Theriot, Jason Leigh, Laboratory for Advanced Visualization & Applications, University of Hawaii at Manoa.
Version: 2.0, June 20, 2022.
    Developed to support the Universal Render Pipeline.
    Stripped out Destiny code to only support simulator and wall.
    Also supports interlace, side by side and top bottom stereo modes.
 
This class loads the configuration XML file for the 3D wall and sets the correct settings for that platform.
If no XML files exists the program launches in a scalable window in Simulator mode.
Look at CCUnityConfig.xml for example of XML commands read.
The CCUnityConfig.xml file should be placed in a folder called CCUnityConfig and placed
at the same directory level as the application .exe file.
 */


public static class CC_CONFIG {
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public static string platform = "";
    private static bool wall;

    public static float interaxial;
    public static bool invertStereo;

    public static int screenWidth;
    public static int screenHeight;
    public static bool fullScreen;
    public static int stereoMode;
    public static float wallHeight;
    public static float wallWidth;
    public static Vector3 wallPosition = new Vector3();
    public static Vector3 headPosition = new Vector3();
    
    private static bool loaded;

    /// <summary>
    /// Checks for the XML config file existence and loads the file if it exists.
    /// </summary>
    /// <returns>Returns true if the config file was succesfully loaded</returns>
    public static bool LoadXMLConfig(string filename) {

        if (loaded) return loaded;

        string configFile = "";

        FileInfo fileCheckLocal = new FileInfo("CCUnityConfig.xml");
        FileInfo fileCheckCentral = new FileInfo(filename);

        if (fileCheckCentral.Exists) configFile = filename;

        // If local config file exists use local filename
        if (fileCheckLocal.Exists) configFile = "CCUnityConfig.xml";

        if (configFile != "")        
        {
            loadConfig(configFile);
            loaded = true;
            return true;
        }
        else
        {
            loaded = false;
            return false;
        }
    }


    /// <summary>
    /// Check to see if a config file was loaded.
    /// </summary>
    /// <returns>Returns true if a config file was loaded</returns>
    public static bool ConfigLoaded() {
        return loaded;
    }


    /// <summary>
    /// Check to see if the current platform is Wall according to the XML config file.
    /// </summary>
    /// <returns>Returns true if the current platform is Wall.</returns>
    public static bool IsWall() {
        return wall;
    }

    /// <summary>
    /// Check to see if tracking is enabled according to the XML config file.
    /// </summary>
    /// <returns>Returns true if tracking is enabled.</returns>
  //  public static bool IsTracking() {
  //      return tracking;
  //  }

    /// <summary>
    /// Sets the window of wall as a borderless window and sets to the resolution of Wall.
    /// </summary>
    private static void setWindowWall() {
        uint SWP_SHOWWINDOW = 0x0040;
        int GWL_STYLE = -16;
        int WS_BORDER = 1;

        SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_BORDER);
        SetWindowPos(GetForegroundWindow(), 0, 0, 0, screenWidth, screenHeight, SWP_SHOWWINDOW);
         SetWindowPos(GetForegroundWindow(), 0, 0, 0, screenWidth, screenHeight, SWP_SHOWWINDOW);
    }

    //Loads the XML file
    /// <summary>
    /// Loads the XML file.
    /// </summary>
    /// <param name="filePath">Name of the file path to read from.</param>
    private static void loadConfig(string filePath) {
        XmlDocument reader = new XmlDocument();
        reader.Load(filePath);

        XmlNode node = reader.DocumentElement.SelectSingleNode("/config/platform");

        platform = node.InnerText;

        if (platform.Equals("Wall")) {
            wall = true;
        }


        node = reader.DocumentElement.SelectSingleNode("/config/wallwidth");
        float.TryParse(node.InnerText, out wallWidth);

        node = reader.DocumentElement.SelectSingleNode("/config/wallheight");
        float.TryParse(node.InnerText, out wallHeight);   

        node = reader.DocumentElement.SelectSingleNode("/config/wallxposition");
        float.TryParse(node.InnerText, out wallPosition.x);     
        node = reader.DocumentElement.SelectSingleNode("/config/wallyposition");
        float.TryParse(node.InnerText, out wallPosition.y);     
        node = reader.DocumentElement.SelectSingleNode("/config/wallzposition");
        float.TryParse(node.InnerText, out wallPosition.z);     

        node = reader.DocumentElement.SelectSingleNode("/config/headxposition");
        float.TryParse(node.InnerText, out headPosition.x);     
        node = reader.DocumentElement.SelectSingleNode("/config/headyposition");
        float.TryParse(node.InnerText, out headPosition.y);     
        node = reader.DocumentElement.SelectSingleNode("/config/headzposition");
        float.TryParse(node.InnerText, out headPosition.z);     


        node = reader.DocumentElement.SelectSingleNode("/config/screenwidth");
        int.TryParse(node.InnerText, out screenWidth);

        node = reader.DocumentElement.SelectSingleNode("/config/screenheight");
        int.TryParse(node.InnerText, out screenHeight);

        node = reader.DocumentElement.SelectSingleNode("/config/fullscreen");
        if (node.InnerText.Equals("1")) {
            fullScreen = true;
            Screen.fullScreen = true;
        } else {
            Screen.fullScreen = false;
            fullScreen = false;
        }



        if (screenWidth > 0 && screenHeight > 0) {
            Screen.SetResolution(screenWidth, screenHeight, fullScreen);
        }

        node = reader.DocumentElement.SelectSingleNode("/config/stereomode");
        int.TryParse(node.InnerText, out stereoMode);

        node = reader.DocumentElement.SelectSingleNode("/config/invertstereo");
        if (node.InnerText.Equals("1")) {
            invertStereo = true;
        } else {
            invertStereo = false;
        }

        node = reader.DocumentElement.SelectSingleNode("/config/interaxial");
        float interax = 0;
        if (float.TryParse(node.InnerText, out interax)) {
            interaxial = interax;
        } else {
            interaxial = 0.061f;
        }

        loaded = true;

        if (IsWall() && !Application.isEditor) {
            setWindowWall();
        }

        Debug.Log (wallPosition);
        node = null;
        reader = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
