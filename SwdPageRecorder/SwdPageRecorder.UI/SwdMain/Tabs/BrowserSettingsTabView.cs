using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SwdPageRecorder.WebDriver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

using FormKeys = System.Windows.Forms.Keys;

namespace SwdPageRecorder.UI
{
    public partial class BrowserSettingsTabView : UserControl, IView
    {
        public BrowserSettingsTabPresenter Presenter { get; private set; }
        private Control[] driverControls;

        public BrowserSettingsTabView()
        {
            InitializeComponent();
            Presenter = Presenters.BrowserSettingsTabPresenter;
            Presenter.InitWithView(this);

            HandleRemoteDriverSettingsEnabledStatus();

            driverControls = new Control[] { chkUseRemoteHub, grpRemoteConnection, ddlBrowserToStart };

            SetDesiredCapsAvailability(false);
            Presenter.InitDesiredCapabilities();


        }

        private void SetDesiredCapsAvailability(bool enabled)
        {
            grpDesiredCaps.DoInvokeAction(() => grpDesiredCaps.Enabled = enabled);
        }

        public class vendorBrowser
        {
            public string browser;
            public string version;
            public string platform;
        }

        private void seleniumVendor_SelectedIndexChanged(object sender, EventArgs e)
        {

            var dtVendorCapabilities = new Dictionary<string, Dictionary<string, Object>>();
            dtVendorCapabilities.Add("Sauce Labs", new Dictionary<string, Object>{ {"capabilities" ,  new Dictionary<string,string>{
        {"username" , "<USERNAME>"},
        {"accessKey" ,"<ACCESSKEY>"}}},{"hub_url" ,"http://ondemand.saucelabs.com:80/wd/hub/" },{ "help_url" , "https://www.browserstack.com/automate/c-sharp#configure-capabilities"},
   {       "platform" , "linux" },
   {       "browser" , "chrome"},
       {   "version" , "35"},

    });


            dtVendorCapabilities.Add("TestingBot", new Dictionary<string, Object>{ {"capabilities" ,  new Dictionary<string,string>{
        {"username" , "<USERNAME>"},
        {"accesskey" ,"<ACCESSKEY>"}}},{"hub_url" ,"" },{ "help_url" , "https://testingbot.com/features"},
   {       "platform" , "<PLATFORM>" },
   {       "browser" , "<BROWSER>"},
       {   "version" , "<VERSION>"},

    });

            dtVendorCapabilities.Add("BrowserStack", new Dictionary<string, Object>{ {"capabilities" ,  new Dictionary<string,string>{
        {"browserstack.user" , "<BROWSERSTACK.USER>"},
        {"browserstack.key" ,"<BROWSERSTACK.KEY>"}}},{"hub_url" ,"http://hub.browserstack.com/wd/hub/" },{ "help_url" , "https://www.browserstack.com/automate/c-sharp#configure-capabilities"},
   {       "platform" , "<PLATFORM>" },
   {       "browser" , "<BROWSER>"},
       {   "version" , "<VERSION>"},

    });
            string vendor = cbSeleniumVendor.SelectedItem.ToString();
            if (dtVendorCapabilities.ContainsKey(vendor))
            {
                // fill dtAdditonalCapabilities DataGridView with vendor-specific inputs
                dtAdditonalCapabilities.Rows.Clear();
                foreach (var configuration_input in new String[] { "browser", "platform", "version" })
                {
                    dtAdditonalCapabilities.Rows.Add(new String[] { configuration_input, dtVendorCapabilities[vendor][configuration_input].ToString()});
                }
                
                Object capabilities_input_object;
                dtVendorCapabilities[vendor].TryGetValue("capabilities", out capabilities_input_object);
                 Dictionary<string, string> capabilities_input = new Dictionary<string,string> ();
                 try
                 {
                     capabilities_input = capabilities_input_object as Dictionary<string, string>;
                 }
                 catch (Exception) { /* ignore */}
                
                foreach (string capability_name in capabilities_input.Keys) {
                    dtAdditonalCapabilities.Rows.Add(new String[] { capability_name, capabilities_input[capability_name].ToString() });
                }

            }
        }

        private void btnStartWebDriver_Click(object sender, EventArgs e)
        {
            var isRemoteDriver = chkUseRemoteHub.Checked;
            var startSeleniumServerIfNotStarted = chkAutomaticallyStartServer.Checked;
            var shouldMaximizeBrowserWindow = chkMaximizeBrowserWindow.Checked;

            var browserOptions = new WebDriverOptions()
            {
                BrowserName = ddlBrowserToStart.SelectedItem as string,
                IsRemote = isRemoteDriver,
                RemoteUrl = txtRemoteHubUrl.Text,
            };


            Presenter.StartNewBrowser(browserOptions, startSeleniumServerIfNotStarted, shouldMaximizeBrowserWindow);
        }

        private void HandleRemoteDriverSettingsEnabledStatus()
        {
            grpRemoteConnection.DoInvokeAction(
                    () => { grpRemoteConnection.Enabled = chkUseRemoteHub.Checked; grpDesiredCaps.Enabled = chkUseRemoteHub.Checked; });

            ChangeBrowsersList(chkUseRemoteHub.Checked);
        }

        private void ChangeBrowsersList(bool showAll)
        {
            var selectedItem = ddlBrowserToStart.SelectedItem;
            string previousValue = "";

            if (selectedItem != null)
            {
                previousValue = ddlBrowserToStart.SelectedItem as string;
            }

            ddlBrowserToStart.Items.Clear();

            string[] addedItems = null;
            if (showAll)
            {
                addedItems = WebDriverOptions.allWebdriverBrowsersSupported;
                ddlBrowserToStart.Items.AddRange(addedItems);
            }
            else
            {
                addedItems = WebDriverOptions.embededWebdriverBrowsersSupported;
                ddlBrowserToStart.Items.AddRange(addedItems);
            }

            int index = Array.IndexOf(addedItems, previousValue);
            index = index >= 0 ? index : 0;
            ddlBrowserToStart.SelectedIndex = index;

        }

        private void chkUseRemoteHub_CheckedChanged(object sender, EventArgs e)
        {
            HandleRemoteDriverSettingsEnabledStatus();
        }



        private void SetControlsState(string startButtonCaption, bool isEnabled)
        {
            btnStartWebDriver.DoInvokeAction(() => btnStartWebDriver.Text = startButtonCaption);

            foreach (var control in driverControls)
            {
                btnStartWebDriver.DoInvokeAction(() => control.Enabled = isEnabled);
            }
            HandleRemoteDriverSettingsEnabledStatus();
        }

        internal void DriverIsStopping()
        {
            SetControlsState("Start", true);
            SetDesiredCapsAvailability(false);
        }

        internal void DriverWasStarted()
        {
            SetControlsState("Stop", false);
            SetDesiredCapsAvailability(true);
        }

        internal void DisableDriverStartButton()
        {
            btnStartWebDriver.DoInvokeAction(() => btnStartWebDriver.Enabled = false);
        }

        internal void EnableDriverStartButton()
        {
            btnStartWebDriver.DoInvokeAction(() => btnStartWebDriver.Enabled = true);
        }

        internal void SetStatus(string status)
        {

            lblWebDriverStatus.DoInvokeAction(() => lblWebDriverStatus.Text = status);
        }

        private void btnTestRemoteHub_Click(object sender, EventArgs e)
        {
            Presenter.TestRemoteHub(txtRemoteHubUrl.Text);
        }

        internal void SetTestResult(string result, bool isOk)
        {
            lblRemoteHubStatus.Text = result;
            lblRemoteHubStatus.ForeColor = (isOk) ? Color.Green : Color.Red;
        }

        internal void SetBrowserStartupSettings(WebDriverOptions browserOptions)
        {
            Action action = new Action(() =>
            {
                chkUseRemoteHub.Checked = browserOptions.IsRemote;

                var index = ddlBrowserToStart.Items.IndexOf(browserOptions.BrowserName);

                ddlBrowserToStart.SelectedIndex = index;

                txtRemoteHubUrl.Text = browserOptions.RemoteUrl;
            });

            if (this.InvokeRequired)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }

        }

        internal void ClickOnStartButton()
        {
            btnStartWebDriver.DoInvokeAction(() => btnStartWebDriver.PerformClick());
        }

        private void tabPage2_Enter(object sender, System.EventArgs e)
        {
            InitializeDataGridView();

        }

        private void InitializeDataGridView()
        {
            string[] row1 = new string[] { "Meatloaf", "Main Dish", "ground beef",
            "**" };
            string[] row2 = new string[] { "Chocolate Cheesecake", "Dessert", 
            "cream cheese", "***" };

            object[] rows = new object[] { row1, row2 };
            foreach (string[] rowArray in rows)
            {
                dtAdditonalCapabilities.Rows.Add(rowArray);
            }
        }

        private void tabPage1_Enter(object sender, System.EventArgs e)
        {
            Presenter.LoadCapabilities();
        }

        private void lnkSeleniumDownloadPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://docs.seleniumhq.org/download/");
        }

        internal void DisableMaximizeBrowserChackBox()
        {
            chkMaximizeBrowserWindow.DoInvokeAction(() =>
            {
                chkMaximizeBrowserWindow.Enabled = false;
            });

        }

        internal void EnableMaximizeBrowserChackBox()
        {
            chkMaximizeBrowserWindow.DoInvokeAction(() =>
            {
                chkMaximizeBrowserWindow.Enabled = true;
            });
        }
    }
}
