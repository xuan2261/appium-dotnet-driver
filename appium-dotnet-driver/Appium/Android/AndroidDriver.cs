﻿using OpenQA.Selenium.Appium.Android.Interfaces;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace OpenQA.Selenium.Appium.Android
{
    public class AndroidDriver<W> : AppiumDriver<W>, IFindByAndroidUIAutomator<W>, IStartsActivity, 
        IHasNetworkConnection, 
        IAndroidDeviceActionShortcuts, 
        IPushesFiles where W : IWebElement
    {
        private static readonly string Platform = MobilePlatform.Android;

        private const string METASTATE_PARAM = "metastate";
        private const string CONNECTION_NAME_PARAM = "name";
        private const string CONNECTION_PARAM_PARAM = "parameters";
        private const string CONNECTION_NAME_VALUE = "network_connection";
        private const string DATA_PARAM = "data";
        private const string INTENT_PARAM = "intent";
        
        /// <summary>
        /// Initializes a new instance of the AndroidDriver class
        /// </summary>
        /// <param name="commandExecutor">An <see cref="ICommandExecutor"/> object which executes commands for the driver.</param>
        /// <param name="desiredCapabilities">An <see cref="DesiredCapabilities"/> object containing the desired capabilities of the browser.</param>
        public AndroidDriver(ICommandExecutor commandExecutor, DesiredCapabilities desiredCapabilities)
            : base(commandExecutor, SetPlatformToCapabilities(desiredCapabilities, Platform))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the AndroidDriver class. This constructor defaults proxy to http://127.0.0.1:4723/wd/hub
        /// </summary>
        /// <param name="desiredCapabilities">An <see cref="DesiredCapabilities"/> object containing the desired capabilities of the browser.</param>
        public AndroidDriver(DesiredCapabilities desiredCapabilities)
            : base(SetPlatformToCapabilities(desiredCapabilities, Platform))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AndroidDriver class
        /// </summary>
        /// <param name="remoteAddress">URI containing the address of the WebDriver remote server (e.g. http://127.0.0.1:4723/wd/hub).</param>
        /// <param name="desiredCapabilities">An <see cref="DesiredCapabilities"/> object containing the desired capabilities of the browser.</param>
        public AndroidDriver(Uri remoteAddress, DesiredCapabilities desiredCapabilities)
            : base(remoteAddress, SetPlatformToCapabilities(desiredCapabilities, Platform))
        {
        }

        /// <summary>
        /// Initializes a new instance of the AndroidDriver class using the specified remote address, desired capabilities, and command timeout.
        /// </summary>
        /// <param name="remoteAddress">URI containing the address of the WebDriver remote server (e.g. http://127.0.0.1:4723/wd/hub).</param>
        /// <param name="desiredCapabilities">An <see cref="DesiredCapabilities"/> object containing the desired capabilities of the browser.</param>
        /// <param name="commandTimeout">The maximum amount of time to wait for each command.</param>
        public AndroidDriver(Uri remoteAddress, DesiredCapabilities desiredCapabilities, TimeSpan commandTimeout)
            : base(remoteAddress, SetPlatformToCapabilities(desiredCapabilities, Platform), commandTimeout)
        {
        }

        #region IFindByAndroidUIAutomator Members

        public W FindElementByAndroidUIAutomator(string selector)
        {
            return (W) this.FindElement("-android uiautomator", selector);
        }

        public ReadOnlyCollection<W> FindElementsByAndroidUIAutomator(string selector)
        {
            return CollectionConverterUnility.
                            ConvertToExtendedWebElementCollection<W>(this.FindElements("-android uiautomator", selector));
        }

        #endregion IFindByAndroidUIAutomator Members

        /// <summary>
        /// Opens an arbitrary activity during a test. If the activity belongs to
        /// another application, that application is started and the activity is opened.
        ///
        /// This is an Android-only method.
        /// </summary>
        /// <param name="appPackage">The package containing the activity to start.</param>
        /// <param name="appActivity">The activity to start.</param>
        /// <param name="appWaitPackage">Begin automation after this package starts. Can be null or empty.</param>
        /// <param name="appWaitActivity">Begin automation after this activity starts. Can be null or empty.</param>
        /// <example>
        /// driver.StartActivity("com.foo.bar", ".MyActivity");
        /// </example>
        public void StartActivity(string appPackage, string appActivity, string appWaitPackage = "", string appWaitActivity = "")
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(appPackage));
            Contract.Requires(!String.IsNullOrWhiteSpace(appActivity));

            Dictionary<string, object> parameters = new Dictionary<string, object>() { {"appPackage", appPackage},
        																			   {"appActivity", appActivity},
        																			   {"appWaitPackage", appWaitPackage},
        																			   {"appWaitActivity", appWaitActivity} };

            this.Execute(AppiumDriverCommand.StartActivity, parameters);
        }

        #region Connection Type

        public ConnectionType ConnectionType
        {
            get
            {
                var commandResponse = this.Execute(AppiumDriverCommand.GetConnectionType, null);
                if (commandResponse.Status == WebDriverResult.Success)
                {
                    return (ConnectionType)(long)commandResponse.Value;
                }
                else
                {
                    throw new WebDriverException("The request to get the ConnectionType has failed.");
                }
            }
            set
            {
                Dictionary<string, object> values = new Dictionary<string, object>(){
                    {"type", value}
                };

                Dictionary<string, object> dictionary = new Dictionary<string, object>(){
                    {CONNECTION_NAME_PARAM, CONNECTION_NAME_VALUE},
                    {CONNECTION_PARAM_PARAM, values}
                };

                Execute(AppiumDriverCommand.SetConnectionType, dictionary);
            }
        }
        #endregion Connection Type

        /// <summary>
        /// Triggers device key event with metastate for the keypress
        /// </summary>
        /// <param name="connectionType"></param>
        public void KeyEvent(int keyCode, int metastate)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("keycode", keyCode);
            parameters.Add("metastate", metastate);
            this.Execute(AppiumDriverCommand.KeyEvent, parameters);
        }

        /// <summary>
        /// Toggles Location Services.
        /// </summary>
        public void ToggleLocationServices()
        {
            this.Execute(AppiumDriverCommand.ToggleLocationServices, null);
        }


        /// <summary>
        /// Check if the device is locked
        /// </summary>
        /// <returns>true if device is locked, false otherwise</returns>
        public bool IsLocked()
        {
            var commandResponse = this.Execute(AppiumDriverCommand.IsLocked, null);
            return (bool)commandResponse.Value;
        }


        /// <summary>
        /// Gets Current Device Activity.
        /// </summary>
        /// 
        public string CurrentActivity
        {
            get
            {
                var commandResponse = this.Execute(AppiumDriverCommand.GetCurrentActivity, null);
                return commandResponse.Value as string;
            }
        }

        /// <summary>
        /// Get test-coverage data
        /// </summary>
        /// <param name="intent">a string containing the intent.</param>
        /// <param name="path">a string containing the path.</param>
        /// <return>a base64 string containing the data</return> 
        public string EndTestCoverage(string intent, string path)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("intent", intent);
            parameters.Add("path", path);
            var commandResponse = this.Execute(AppiumDriverCommand.EndTestCoverage, parameters);
            return commandResponse.Value as string;
        }

        /// <summary>
        /// Pushes a File.
        /// </summary>
        /// <param name="pathOnDevice">path on device to store file to</param>
        /// <param name="base64Data">base 64 data to store as the file</param>
        public void PushFile(string pathOnDevice, string base64Data)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("path", pathOnDevice);
            parameters.Add("data", base64Data);
            this.Execute(AppiumDriverCommand.PushFile, parameters);
        }


        /// <summary>
        /// Open the notifications 
        /// </summary>
        public void OpenNotifications()
        {
            this.Execute(AppiumDriverCommand.OpenNotifications, null);
        }

        protected override RemoteWebElement CreateElement(string elementId)
        {
            return new AndroidElement(this, elementId);
        }

        /// <summary>
        /// Set "ignoreUnimportantViews" setting.
        /// See: https://github.com/appium/appium/blob/master/docs/en/advanced-concepts/settings.md
        /// </summary>
        public void IgnoreUnimportantViews(bool value)
        {
            base.UpdateSetting("ignoreUnimportantViews", value);
        }

        #region scrollTo, scrollToExact

        /// <summary>
        /// Scroll forward to the element which has a description or name which contains the input text.
        /// The scrolling is performed on the first scrollView present on the UI.
        /// </summary>
        /// <param name="text">text to look out for while scrolling</param>
        public override W ScrollTo(String text)
        {
            String uiScrollables = UiScrollable("new UiSelector().descriptionContains(\"" + text + "\")") +
                                   UiScrollable("new UiSelector().textContains(\"" + text + "\")");

            return FindElementByAndroidUIAutomator(uiScrollables);
        }

        /// <summary>
        /// Scroll forward to the element which has a description or name which exactly matches the input text.
        /// The scrolling is performed on the first scrollView present on the UI
        /// </summary>
        /// <param name="text">text to look out for while scrolling</param>
        public override W ScrollToExact(String text)
        {
            String uiScrollables = UiScrollable("new UiSelector().description(\"" + text + "\")") +
                                   UiScrollable("new UiSelector().text(\"" + text + "\")");
            return FindElementByAndroidUIAutomator(uiScrollables);
        }

        /// <summary>
        /// Scroll forward to the element which has a description or name which contains the input text.
        /// The scrolling is performed on the first scrollView with the given resource ID.
        /// </summary>
        /// <param name="text">text to look out for while scrolling</param>
        /// <param name="resId">resource ID of the scrollable View</param>
        public W ScrollTo(String text, String resId)
        {
            String uiScrollables = UiScrollable("new UiSelector().descriptionContains(\"" + text + "\")", resId) +
                                   UiScrollable("new UiSelector().textContains(\"" + text + "\")", resId);

            return FindElementByAndroidUIAutomator(uiScrollables);
        }

        /// <summary>
        /// Scroll forward to the element which has a description or name which exactly matches the input text.
        /// The scrolling is performed on the first scrollView present on the UI
        /// </summary>
        /// <param name="text">text to look out for while scrolling</param>
        /// <param name="resId">resource ID of the scrollable View</param>
        public W ScrollToExact(String text, String resId)
        {
            String uiScrollables = UiScrollable("new UiSelector().description(\"" + text + "\")", resId) +
                                   UiScrollable("new UiSelector().text(\"" + text + "\")", resId);
            return FindElementByAndroidUIAutomator(uiScrollables);
        }

        /// <summary>
        /// Creates a new UiScrollable-string to scroll on a View to move through it until a visible item that matches
        /// the selector is found.
        /// </summary>
        /// <param name="uiSelector">UiSelector-string to tell what to search for while scrolling</param>
        /// <param name="resId">Resource-ID of a scrollable View</param>
        /// <returns>UiScrollable-string that can be executed with FindElementByAndroidUIAutomator()</returns>
        private static string UiScrollable(string uiSelector, string resId)
        {
            return "new UiScrollable(new UiSelector().scrollable(true).resourceId(\"" + resId + "\")).scrollIntoView(" + uiSelector + ".instance(0));";
        }

        /// <summary>
        /// Creates a new UiScrollable-string to scroll on the first scrollable View in the layout to move through it until a visible item 
        /// that matches the selector is found.
        /// </summary>
        /// <param name="uiSelector">UiSelector-string to tell what to search for while scrolling</param>
        /// <returns>UiScrollable-string that can be executed with FindElementByAndroidUIAutomator()</returns>
        private static string UiScrollable(string uiSelector)
        {
            return "new UiScrollable(new UiSelector().scrollable(true).instance(0)).scrollIntoView(" + uiSelector + ".instance(0));";
        }

        #endregion
        
    }
}