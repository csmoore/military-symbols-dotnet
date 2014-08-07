/* 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MilitarySymbols;

namespace MilSymbolPicker
{
    /// <summary>
    /// 2525D Touch Symbol Picker Form
    /// </summary>
    public partial class FormPicker : Form
    {

        // TODO: this class is a bit of a mess, this was just quick prototyping
        //       probably are easier ways to do what I did here

        public FormPicker()
        {
            InitializeComponent();
        }

        const int BUTTON_ROWS = 5;
        const int BUTTON_COLS = 3;
        List<Button> buttonList = new List<Button>();

        int currentColRowIndex = 0;
        int currentColumn = 1;
        List<string> currentColValues;
        string currentEntityName = string.Empty;
        string currentEntityTypeName = string.Empty;
        string currentEntitySubTypeName = string.Empty;

        string NOT_SET = "NONE/NOT SET";

        MilitarySymbol currentSymbol = new MilitarySymbol();

        public enum PaneSequenceType  // Order of the button panes
        {
            NotSet              = 0,
            AffiliationPane     = 1,
            SymbolSetPane       = 2,
            EntityPane          = 3,
            EntityTypePane      = 4,
            EntitySubTypePane   = 5,
            Modifier1Pane       = 6,
            Modifier2Pane       = 7,
            EchelonMobilityPane = 8,
            HqTfFdPane          = 9,
            StatusPane          = 10,

            StartOver = 11,  // Important: needs to be exactly 1 more than last for navigation to work
        }

        PaneSequenceType previousPane = PaneSequenceType.NotSet;
        PaneSequenceType currentPane  = PaneSequenceType.AffiliationPane;
        SymbolLookup symbolLookup = new SymbolLookup();

        private void CheckSettings()
        {
            // Set SVG Images Home if set in App Settings
            string appSettingsSvgHomeKey = "SVGImagesHome";
            var svgHomeFolderSetting = System.Configuration.ConfigurationManager.AppSettings[appSettingsSvgHomeKey];
            if (!string.IsNullOrWhiteSpace(svgHomeFolderSetting))
            {
                if (!Utilities.SetImageFilesHome(svgHomeFolderSetting))
                {
                    Console.WriteLine("App.config setting for SVGImagesHome does not exist, export failed!");
                    Console.WriteLine("Setting: " + svgHomeFolderSetting);
                    return;
                }
            }
            if (!Utilities.CheckImageFilesHomeExists())
                MessageBox.Show("Images will not work: could not find folder: " + MilitarySymbolToGraphicLayersMaker.ImageFilesHome);

            string appSettingsShowCenterPoint = "ShowCenterPoint";
            bool showCenterPointSetting = Convert.ToBoolean(
                System.Configuration.ConfigurationManager.AppSettings[appSettingsShowCenterPoint]);
            if (showCenterPointSetting)
            {
                MilitarySymbolToGraphicLayersMaker.AddReferenceCenterPoint = true;
            }
        }

        private void FormPicker_Load(object sender, EventArgs e)
        {
            symbolLookup.Initialize();

            if (!symbolLookup.Initialized)
                MessageBox.Show(@"Symbol Search will not work: Could not initialize the Symbol Lookup (Do Data\*.csv files exist? Are they locked/open anywhere else?)");

            CheckSettings();

            buttonList.Add(this.button11);
            buttonList.Add(this.button12);
            buttonList.Add(this.button13);
            buttonList.Add(this.button14);
            buttonList.Add(this.button15);
            buttonList.Add(this.button21);
            buttonList.Add(this.button22);
            buttonList.Add(this.button23);
            buttonList.Add(this.button24);
            buttonList.Add(this.button25);
            buttonList.Add(this.button31);
            buttonList.Add(this.button32);
            buttonList.Add(this.button33);
            buttonList.Add(this.button34);
            buttonList.Add(this.button35);

            currentSymbol.Id.Affiliation = StandardIdentityAffiliationType.NotSet;
            currentSymbol.Id.SymbolSet = SymbolSetType.NotSet;

            SetPaneState();
        }

        public void SetPaneState()
        {
            bool changedState = (previousPane != currentPane);

            bool goingBackwards = (currentPane < previousPane); 

            if (changedState)
            {
                System.Diagnostics.Trace.WriteLine("New Pane State: " + currentPane 
                    + ", Old State = " + previousPane);

                if (currentPane == PaneSequenceType.AffiliationPane)
                {
                    this.labCol1.Text = "Affiliation";
                    this.labCol2.Visible = false;
                    this.labCol3.Visible = false;

                    currentColValues = TypeUtilities.EnumHelper.getEnumValuesAsStrings(typeof(StandardIdentityAffiliationType));

                    currentColRowIndex = 0;

                    enableColumnButtons(1, true);
                    setVisibilityColumnButtons(2, false);
                    setVisibilityColumnButtons(3, false);
                    currentColumn = 1;
                    setColumnValues();
                }
                else if (currentPane == PaneSequenceType.SymbolSetPane)
                {
                    this.resetSymbolState();

                    this.labCol2.Text = "Symbol Set";
                    this.labCol2.Visible = true;
                    this.labCol3.Visible = false;

                    currentColValues = TypeUtilities.EnumHelper.getEnumValuesAsStrings(typeof(SymbolSetType));
                   
                    currentColRowIndex = 0;

                    currentColumn = 2;

                    enableColumnButtons(2, true);
                    setColumnValues();
                    enableColumnButtons(1, false);
                    setVisibilityColumnButtons(3, false);
                }
                else if ((currentPane == PaneSequenceType.EntityPane) 
                    || (currentPane == PaneSequenceType.EntityTypePane) 
                    || (currentPane == PaneSequenceType.EntitySubTypePane))
                {
                    this.labCol3.Text = currentPane.ToString().Replace("Pane", ""); //  Entity/EntityType/etc.
                    this.labCol3.Visible = true;

                    if (currentPane == PaneSequenceType.EntityPane)
                        currentColValues = symbolLookup.GetDistinctEntries(this.currentSymbol.Id.SymbolSet);
                    else
                        if (currentPane == PaneSequenceType.EntityTypePane)
                            currentColValues = symbolLookup.GetDistinctEntries(
                                this.currentSymbol.Id.SymbolSet, 
                                currentEntityName);
                        else
                            if (currentPane == PaneSequenceType.EntitySubTypePane)
                                currentColValues = symbolLookup.GetDistinctEntries(
                                    this.currentSymbol.Id.SymbolSet, 
                                    currentEntityName, 
                                    currentEntityTypeName);

                    if (currentColValues.Count == 0)
                    {
                        // Advance to the next/previous pane if this is empty
                        if (goingBackwards)
                            currentPane--;
                        else
                            currentPane++;

                        resetColumn3State();
                    }
                    else
                    {
                        currentColValues.Insert(0, NOT_SET); // add as first element
                    }

                    currentColRowIndex = 0;

                    // if we are navigating backwards, and there were no entities (e.g. SpaceMet)
                    if (currentPane != PaneSequenceType.SymbolSetPane)
                    {
                        currentColumn = 3;
                        enableColumnButtons(3, true);
                        setColumnValues();
                        enableColumnButtons(1, false);
                        enableColumnButtons(2, false);
                    }
                }
                else if ((currentPane == PaneSequenceType.Modifier1Pane)
                      || (currentPane == PaneSequenceType.Modifier2Pane))
                {
                    this.labCol3.Text = currentPane.ToString().Replace("Pane", ""); 
                    this.labCol3.Visible = true;

                    if (currentPane == PaneSequenceType.Modifier1Pane)
                        currentColValues = symbolLookup.GetDistinctModifierNames(
                            this.currentSymbol.Id.SymbolSet, 1);
                    else
                        if (currentPane == PaneSequenceType.Modifier2Pane)
                            currentColValues = symbolLookup.GetDistinctModifierNames(
                                this.currentSymbol.Id.SymbolSet, 2);

                    if (currentColValues.Count == 0)
                    {
                        // Advance to the next/previous pane if this is empty
                        if (goingBackwards)
                            currentPane--;
                        else
                            currentPane++;

                        resetColumn3State();
                    }
                    else
                    {
                        currentColValues.Insert(0, NOT_SET); // Add as first element
                    }

                    currentColRowIndex = 0;

                    // if we are navigating backwards, and there were no modifiers (e.g. SpaceMet)
                    if (currentPane != PaneSequenceType.SymbolSetPane)
                    {
                        currentColumn = 3;
                        enableColumnButtons(3, true);
                        setColumnValues();
                        enableColumnButtons(1, false);
                        enableColumnButtons(2, false);
                    }
                }
                ////////////////////////////////////////////////////////////////
                //
                // Don't do these if non-framed
                //
                else if (currentPane == PaneSequenceType.EchelonMobilityPane)
                {
                    // Don't do if Non-framed
                    if (!TypeUtilities.HasFrame(this.currentSymbol.Id.SymbolSet))
                    {
                        if (goingBackwards)
                            currentPane = PaneSequenceType.EntitySubTypePane;
                        else
                            currentPane = PaneSequenceType.StartOver;

                        resetColumn3State();
                    }
                    else
                    {
                        this.labCol3.Text = "Echelon/Mobility";
                        this.labCol3.Visible = true;

                        currentColValues = TypeUtilities.EnumHelper.getEnumValuesAsStrings(typeof(EchelonMobilityType));

                        currentColRowIndex = 0;

                        currentColumn = 3;
                        enableColumnButtons(3, true);
                        setColumnValues();
                        enableColumnButtons(1, false);
                        enableColumnButtons(2, false);
                    }
                }
                else if (currentPane == PaneSequenceType.HqTfFdPane)
                {
                    this.labCol3.Text = "HQ/TF/FD";
                    this.labCol3.Visible = true;

                    currentColValues = TypeUtilities.EnumHelper.getEnumValuesAsStrings(typeof(HeadquartersTaskForceDummyType));

                    currentColRowIndex = 0;

                    currentColumn = 3;
                    enableColumnButtons(3, true);
                    setColumnValues();
                    enableColumnButtons(1, false);
                    enableColumnButtons(2, false);
                }
                else if (currentPane == PaneSequenceType.StatusPane)
                {
                    // Don't do if non-framed
                    if (!TypeUtilities.HasFrame(this.currentSymbol.Id.SymbolSet))
                    {
                        if (goingBackwards)
                            currentPane = PaneSequenceType.EntitySubTypePane;
                        else
                            currentPane = PaneSequenceType.StartOver;

                        resetColumn3State();
                    }
                    else
                    {
                        this.labCol3.Text = "Op Condition";
                        this.labCol3.Visible = true;

                        currentColValues = TypeUtilities.EnumHelper.getEnumValuesAsStrings(typeof(StatusType));

                        currentColRowIndex = 0;

                        currentColumn = 3;
                        enableColumnButtons(3, true);
                        setColumnValues();
                        enableColumnButtons(1, false);
                        enableColumnButtons(2, false);
                    }
                }
                //
                // End of don't do if non-framed
                //
                ////////////////////////////////////////////////////////////////
                else if (currentPane == PaneSequenceType.StartOver)
                {
                    this.labCol3.Text = "Finished";
                    this.labCol3.Visible = true;

                    currentColValues = new List<string>() { "Start Over" };

                    currentColRowIndex = 0;

                    currentColumn = 3;
                    enableColumnButtons(3, true);
                    setColumnValues();
                    enableColumnButtons(1, false);
                    enableColumnButtons(2, false);
                }
            }

            previousPane = currentPane;
        }

        void resetColumn3State()
        {
            button31.Visible = true;
            button31.Enabled = true; // just in case no values so PerformClick will work 
            button31.Text = string.Empty;
            button31.PerformClick(); // force the states to be re-evaluated
        }

        void setSymbolState(string valueSelected = "")
        {
            // TODO: Figure out a way to set this consistent naming scheme better
            string symbolSetName = currentSymbol.Id.SymbolSet.ToString().Replace("_", " ");

            if (string.IsNullOrEmpty(valueSelected))
            {
                // this is a special state (i.e. hack) to simulate a button press, to force 
                // into the next state, when the previous panel is empty
            }
            else if (currentPane == PaneSequenceType.AffiliationPane)
            {
                string affiliationSelectedString = valueSelected;

                StandardIdentityAffiliationType affiliationSelection =
                    (StandardIdentityAffiliationType)
                    TypeUtilities.EnumHelper.getEnumFromString(
                        typeof(StandardIdentityAffiliationType), affiliationSelectedString);

                currentSymbol.Id.Affiliation = affiliationSelection;

                currentPane = PaneSequenceType.SymbolSetPane;
            }
            else if (currentPane == PaneSequenceType.SymbolSetPane)
            {
                string symbolSetSelectedString = valueSelected;

                SymbolSetType symbolSetSelection = (SymbolSetType)
                    TypeUtilities.EnumHelper.getEnumFromString(
                        typeof(SymbolSetType), symbolSetSelectedString);

                currentSymbol.Id.SymbolSet = symbolSetSelection;

                currentPane = PaneSequenceType.EntityPane;
            }
            else if (currentPane == PaneSequenceType.EntityPane)
            {
                if (valueSelected == NOT_SET)
                {
                    currentPane = PaneSequenceType.Modifier1Pane;
                }
                else
                {
                    currentEntityName = valueSelected;

                    string entityCode = symbolLookup.GetEntityCode(currentSymbol.Id.SymbolSet, currentEntityName);

                    currentSymbol.Id.EntityCode = entityCode;

                    currentPane = PaneSequenceType.EntityTypePane;
                }
            }
            else if (currentPane == PaneSequenceType.EntityTypePane)
            {
                if (valueSelected == NOT_SET)
                {
                    currentPane = PaneSequenceType.Modifier1Pane;
                }
                else
                {
                    currentEntityTypeName = valueSelected;

                    string entityCode = symbolLookup.GetEntityCode(currentSymbol.Id.SymbolSet,
                        currentEntityName, currentEntityTypeName);

                    currentSymbol.Id.EntityCode = entityCode;

                    currentPane = PaneSequenceType.EntitySubTypePane;
                }
            }
            else if (currentPane == PaneSequenceType.EntitySubTypePane)
            {
                if (valueSelected == NOT_SET)
                {
                    currentPane = PaneSequenceType.Modifier1Pane;
                }
                else
                {
                    currentEntitySubTypeName = valueSelected;

                    string entityCode = symbolLookup.GetEntityCode(currentSymbol.Id.SymbolSet,
                        currentEntityName, currentEntityTypeName, currentEntitySubTypeName);

                    currentSymbol.Id.EntityCode = entityCode;

                    currentPane = PaneSequenceType.Modifier1Pane;
                }
            }
            else if (currentPane == PaneSequenceType.Modifier1Pane)
            {
                string currentModifier1Name = valueSelected;

                string modifier1Code = symbolLookup.GetModifierCodeFromName(
                    currentSymbol.Id.SymbolSet, 1, currentModifier1Name);

                currentSymbol.Id.ModifierOne = modifier1Code;

                currentPane = PaneSequenceType.Modifier2Pane;
            }
            else if (currentPane == PaneSequenceType.Modifier2Pane)
            {
                string currentModifier2Name = valueSelected;

                string modifier2Code = symbolLookup.GetModifierCodeFromName(
                    currentSymbol.Id.SymbolSet, 2, currentModifier2Name);

                currentSymbol.Id.ModifierTwo = modifier2Code;

                currentPane = PaneSequenceType.EchelonMobilityPane;
            }
            else if (currentPane == PaneSequenceType.EchelonMobilityPane)
            {
                string currentEchelonMobilityName = valueSelected;

                EchelonMobilityType echelonMobilitySelection =
                    (EchelonMobilityType)
                    TypeUtilities.EnumHelper.getEnumFromString(
                        typeof(EchelonMobilityType), currentEchelonMobilityName);

                currentSymbol.Id.EchelonMobility = echelonMobilitySelection;

                currentPane = PaneSequenceType.HqTfFdPane;
            }
            else if (currentPane == PaneSequenceType.HqTfFdPane)
            {
                string currentHqTfFdName = valueSelected;

                HeadquartersTaskForceDummyType hqTfFdSelection =
                    (HeadquartersTaskForceDummyType)
                    TypeUtilities.EnumHelper.getEnumFromString(
                        typeof(HeadquartersTaskForceDummyType), currentHqTfFdName);

                currentSymbol.Id.HeadquartersTaskForceDummy = hqTfFdSelection;

                currentPane = PaneSequenceType.StatusPane;
            }
            else if (currentPane == PaneSequenceType.StatusPane)
            {
                string currentStatusName = valueSelected;

                StatusType statusSelection =
                    (StatusType)TypeUtilities.EnumHelper.getEnumFromString(
                        typeof(StatusType), currentStatusName);

                currentSymbol.Id.Status = statusSelection;

                currentPane = PaneSequenceType.StartOver;
            }
            else if (currentPane == PaneSequenceType.StartOver)
            {
                // Reset the other values
                resetSymbolState(); 

                // Go back when we are done 
                currentPane = PaneSequenceType.SymbolSetPane;
            }

            setTagLabel();

            updatePictureBox();

            // Go To Next Pane 
            SetPaneState();
        }

        void resetSymbolState()
        {
            currentEntityName = string.Empty;
            currentEntityTypeName = string.Empty;
            currentEntitySubTypeName = string.Empty;

            StandardIdentityAffiliationType affil = currentSymbol.Id.Affiliation;

            currentSymbol.Id = SymbolIdCode.DefaultSymbolIdCode;
            currentSymbol.Id.Affiliation = affil;

            currentSymbol.GraphicLayers.Clear();

            this.cbLayers.Text = string.Empty;
            this.cbLayers.Items.Clear();

            // blank image
            pbPreview.Image = null;
            // or set to "No Image" graphic:
            // Bitmap noImage = new Bitmap(GetType(), "NoImage.png");
        }

        void setVisibilityColumnButtons(int column, bool visible = true)
        {
            for (int i = ((column - 1) * BUTTON_ROWS); i < BUTTON_ROWS * column; i++)
            {
                buttonList[i].Visible = visible;
            }

            if (column == 1)
            {
                butNextCol1.Visible = visible;
            }
            else if (column == 2)
            {
                butBackCol2.Visible = visible;
                butNextCol2.Visible = visible;
            }
            else if (column == 3)
            {
                butBackCol3.Visible = visible;
                butNextCol3.Visible = visible;
            }
        }

        void enableColumnButtons(int column, bool enabled = true)
        {
            for (int i = ((column - 1) * BUTTON_ROWS); i < BUTTON_ROWS * column; i++)
            {
                buttonList[i].Enabled = enabled;
                if (enabled)
                {
                    buttonList[i].UseVisualStyleBackColor = true; // reset the back color for hightlight ones
                    buttonList[i].Visible = true; // make sure visible also
                }
            }

            if (column == 1)
            {
                butNextCol1.Enabled = enabled;
                if (enabled)
                    butNextCol1.Visible = true; // make sure visible also
            }
            else if (column == 2)
            {
                butBackCol2.Enabled = enabled;
                butNextCol2.Enabled = enabled;
                if (enabled)
                {
                    butBackCol2.Visible = true;
                    butNextCol2.Visible = true;
                }
            }
            else if (column == 3)
            {
                butBackCol3.Enabled = enabled;
                butNextCol3.Enabled = enabled;
                if (enabled)
                {
                    butBackCol3.Visible = true;
                    butNextCol3.Visible = true;
                }
            }
        }

        void setColumnValues()
        {
            int column = currentColumn;

            for (int i = ((column - 1) * BUTTON_ROWS); i < BUTTON_ROWS * column; i++)
            {
                if (currentColRowIndex < currentColValues.Count)
                {
                    buttonList[i].Text = currentColValues[currentColRowIndex];
                    buttonList[i].Visible = true;
                    currentColRowIndex++;
                }
                else
                {
                    buttonList[i].Text = String.Empty;
                    buttonList[i].Visible = false;
                }
            }

            if (currentColRowIndex >= currentColValues.Count)
                currentColRowIndex = 0;
        }

        private void updatePictureBox()
        {
            if (!currentSymbol.Id.IsValid)
                return;

            MilitarySymbolToGraphicLayersMaker.SetMilitarySymbolGraphicLayers(ref currentSymbol);

            // Debug in case drawing layers crazed:
            // System.Diagnostics.Trace.WriteLine("MilitarySymbol State After SetMilitarySymbolGraphicLayers : ");
            // System.Diagnostics.Trace.WriteLine(this.currentSymbol);

            if (currentSymbol.GraphicLayers.Count == 0)
            {
                System.Diagnostics.Trace.WriteLine("WARNING: No Graphic Layers to Draw");
                return;
            }

            SvgSymbol.ImageSize = new Size(pbPreview.Width, pbPreview.Height);
            pbPreview.Image = SvgSymbol.GetBitmap(currentSymbol.GraphicLayers);

            // Set the Combo Box with the layers
            cbLayers.Items.Clear();
            int layerNumber = 0;
            foreach (string graphicLayer in currentSymbol.GraphicLayers)
            {
                layerNumber++;
                string simpleLayer = layerNumber.ToString() + ":" + 
                    graphicLayer.Replace(MilitarySymbolToGraphicLayersMaker.ImageFilesHome,
                    " ");

                if (!System.IO.File.Exists(graphicLayer))
                    simpleLayer = "[MISSING]:" + simpleLayer;

                cbLayers.Items.Add(simpleLayer);
            }
            cbLayers.SelectedIndex = 0;
        }

        private void setTagLabel()
        {
            // Also set SIDC here
            if (currentSymbol.Id.IsValid)
            {
                this.labSidcFirst10.Text  = currentSymbol.Id.CodeFirstTenFormatted;
                this.labSidcSecond10.Text = currentSymbol.Id.CodeSecondTenFormatted;
            }

            StringBuilder tagBuilder = new StringBuilder();

            foreach (string tag in this.currentSymbol.Tags)
            {
                tagBuilder.Append(tag.Replace('_', ' '));
                tagBuilder.Append(";");
            }

            // Add the unformatted code to the end in case needed
            tagBuilder.Append(currentSymbol.Id.Code);

            this.tbTags.Text = tagBuilder.ToString();
        }

        private void buttonPane_Click(object sender, EventArgs e)
        {
            // Happens on any Pane Button Click

            Button pressedButton = sender as Button;
            pressedButton.BackColor = Color.Yellow;

            string valuePressed = pressedButton.Text;

            setSymbolState(valuePressed);
        }

        private void butNextCol1_Click(object sender, EventArgs e)
        {
            setColumnValues();
        }

        private void butBackCol2_Click(object sender, EventArgs e)
        {
            if (currentColumn != 2)
            {
                System.Diagnostics.Trace.WriteLine("We shouldn't be here...");
                return;
            }

            currentColumn = 1;

            currentPane = PaneSequenceType.AffiliationPane;

            SetPaneState();
        }

        private void butNextCol2_Click(object sender, EventArgs e)
        {
            setColumnValues();
        }

        private void butBackCol3_Click(object sender, EventArgs e)
        {
            if (currentColumn != 3)
            {
                System.Diagnostics.Trace.WriteLine("We shouldn't be here...");
                return;
            }

            // Go back one pane
            currentPane--;
            SetPaneState();
        }

        private void butNextCol3_Click(object sender, EventArgs e)
        {
            setColumnValues();
        }

        private void pbPreview_Click(object sender, EventArgs e)
        {
            if (this.pbPreview.Image == null)
                return;

            // Easter Egg : save image file on click
            SaveFileDialog saveImageFile = new SaveFileDialog();

            string basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            saveImageFile.InitialDirectory = basePath;

            string imageFileName = "SampleImage.png";

            saveImageFile.FileName = imageFileName;
            saveImageFile.Filter = "Text files (*.png)|*.png|All files (*.*)|*.*";

            if (saveImageFile.ShowDialog() == DialogResult.OK)
            {
                Image saveImage = this.pbPreview.Image;
                saveImage.Save(saveImageFile.FileName);
            }
        }

        private void butReset_Click(object sender, EventArgs e)
        {
            currentPane = PaneSequenceType.AffiliationPane;

            resetSymbolState();

            SetPaneState();
        }

        private void butExtras_Click(object sender, EventArgs e)
        {
            FormExtras extras = new FormExtras();
            DialogResult dr = extras.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                if (extras.cbManuallyEnterCode.Checked)
                {
                    string id2Try = extras.tbManuallyEnterCode.Text.Trim();
                    SymbolIdCode tryCode = new SymbolIdCode(id2Try);

                    if (tryCode.IsValid)
                    {
                        this.currentSymbol.Id = tryCode;

                        this.currentPane = PaneSequenceType.StartOver;

                        setSymbolState();
                    }
                    else
                    {
                        MessageBox.Show("Could not create symbol from ID: " + id2Try);
                    }

                }
            }
        }
    }
}
