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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilitarySymbols
{
    /// <summary>
    /// General processing is to convert a Symbol Id Code into a list of strings
    /// These strings represent the paths to the set of images/layers that *should*
    /// represent that Symbol Id
    /// For convenience, it takes a MilitarySymbol object in it main entry method:
    /// SetMilitarySymbolGraphicLayers and sets the GraphicLayers to these strings
    /// We might also replace this complicated logic with a table lookup, but for now
    /// it is a nice way to validate that the rules are actually consistent.
    /// </summary>
    public class MilitarySymbolToGraphicLayersMaker
    {
        // IMPORTANT: defaults to {exe folder}\MIL_STD_2525D_Symbols (mainly for deployment)
        private static readonly string DEFAULT_PATH =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MIL_STD_2525D_Symbols");

        // IMPORTANT: - OR - you can set this at Runtime with 
        // Utilities.SetImageFilesHome -or- MilitarySymbolToGraphicLayersMaker.ImageFilesHome

        // Allow this property to be set externally at runtime (if desired/needed)
        public static string ImageFilesHome
        {
            get { return imageFilesHome; }
            set
            {
                string checkForDirectorySeparator = value;

                // but make sure it ends in a DirectorySeparatorChar
                checkForDirectorySeparator = 
                    checkForDirectorySeparator.TrimEnd(System.IO.Path.DirectorySeparatorChar) + 
                    System.IO.Path.DirectorySeparatorChar;

                imageFilesHome = checkForDirectorySeparator;
            }
        }
        private static string imageFilesHome =
            DEFAULT_PATH 
            + System.IO.Path.DirectorySeparatorChar; // IMPORTANT/NOTE: Ends in DirectorySeparator

        const string ImageSuffix = ".svg";

        public static bool AddReferenceCenterPoint
        {
            get;
            set;
        }

        public static bool UseCivilianFrames
        {
            get;
            set;
        }

        // IMPORTANT: If you don't have the expected SVG Files, in the expected folder format
        //            then this class will not do anything.
        // The assumed/expected Folder structure:
        // {ImageFilesHome} <--- SEE DEFINITION BELOW
        //  |- Echelon
        //  |- Frames
        //  |- Headquarters
        //  |- Appendices
        //     |- Air
        //     |- Control Measures
        //     |- Cyberspace
        //     |- Land
        // (etc.)

        public static string GetIconFolderName(SymbolSetType symbolSet)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Appendices");
            sb.Append(System.IO.Path.DirectorySeparatorChar);

            string symbolSetSubFolderName = string.Empty;
            if (TypeUtilities.SymbolSetToFolderName.ContainsKey(symbolSet))
                symbolSetSubFolderName = TypeUtilities.SymbolSetToFolderName[symbolSet];

            sb.Append(symbolSetSubFolderName);
            sb.Append(System.IO.Path.DirectorySeparatorChar);

            return sb.ToString();
        }

        public static string GetMainIconName(SymbolSetType symbolSet, string fullEntityCode)
        {
            if (fullEntityCode.Length != 6)
            {
                return "00000000";
            }

            StringBuilder sb = new StringBuilder();

            string symbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            sb.Append(symbolSetString);
            sb.Append(fullEntityCode);

            return sb.ToString();
        }

        public static string GetMainIconName(ref MilitarySymbol milSymbol)
        {
            string mainIcon;

            if (milSymbol.Id.IsLandUnitSpecialEntity) // Special/Exceptional Case for this
            {
                string altEntityCode = milSymbol.Id.EntityField + milSymbol.Id.EntityTypeField + "00";
                mainIcon = GetMainIconName(milSymbol.Id.SymbolSet, altEntityCode);
            }
            else
                mainIcon = GetMainIconName(milSymbol.Id.SymbolSet, milSymbol.Id.EntityCode);

            return mainIcon;
        }

        // MainIconNameWithFolder
        // Appendices\{SymbolSetTypeName}\SymbolSetType + EntityCode 
        // e.g. Appendices\Air\01110000.svg ==> { 01 } + { 110000 }
        public static string GetMainIconNameWithFolder(ref MilitarySymbol milSymbol)
        {
            StringBuilder sb = new StringBuilder();

            string currentAppendixHome = GetIconFolderName(milSymbol.Id.SymbolSet);
            sb.Append(currentAppendixHome);

            string mainIconName = GetMainIconName(ref milSymbol);

            sb.Append(mainIconName);
            sb.Append(ImageSuffix);

            return sb.ToString();
        }

        // same as ImageFilesHome + GetMainIconNameWithFolder
        public static string GetMainIconNameWithFullPath(ref MilitarySymbol milSymbol)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ImageFilesHome);

            string mainIconNameWithoutImageFilesHome = GetMainIconNameWithFolder(ref milSymbol);
            sb.Append(mainIconNameWithoutImageFilesHome);

            string mainIconNameFullPath = sb.ToString();

            // WORKAROUND/TRICKY: some center icon symbols that need to touch fram have _0, _1, _2, _3 suffix instead of base version
            // Method: Find the frame suffix from the affiliation & add _0, _1, _2, _3 accordingly
            // Also test for 2 other exceptional cases while we are at it
            if (!System.IO.File.Exists(mainIconNameFullPath))
            {
                StandardIdentityAffiliationType mappedAffiliation = milSymbol.Id.Affiliation;

                if (milSymbol.Id.StandardIdentity == StandardIdentityRealExerciseSimType.Exercise)
                {
                    // Exceptional case for these 2 (has a friend frame)
                    if ((mappedAffiliation == StandardIdentityAffiliationType.Suspect_Joker) ||
                        (mappedAffiliation == StandardIdentityAffiliationType.Hostile))
                        mappedAffiliation = StandardIdentityAffiliationType.Friend;
                }

                string newFrameSuffix =
                    TypeUtilities.AffiliationFrameToSuffixName[mappedAffiliation] + ImageSuffix;

                // more exceptional cases:
                if (milSymbol.Id.SymbolSet == SymbolSetType.Control_Measures)
                {
                    // use ".a" instead - 6 symbols
                    newFrameSuffix = ".a" + ImageSuffix;
                }
                else if (milSymbol.Id.SymbolSet == SymbolSetType.Atmospheric)
                {
                    // use ".0" instead - only 1 symbol 45140200.0 
                    newFrameSuffix = ".0" + ImageSuffix;
                }

                string subMainIconName = mainIconNameFullPath;
                subMainIconName = subMainIconName.Replace(ImageSuffix, newFrameSuffix);
                if (System.IO.File.Exists(subMainIconName)) // if the other file/version exists, use that one
                    mainIconNameFullPath = subMainIconName;
            }

            return mainIconNameFullPath;
        }

        public static string GetModfierIconName(SymbolSetType symbolSet, int modifierNumber, int modifierCodeInt)
        {
            StringBuilder sb = new StringBuilder();

            if (!((modifierNumber == 1) || (modifierNumber == 2)))
                return string.Empty;

            string sModifierNumber = modifierNumber.ToString();

            string modifierCode = modifierCodeInt.ToString();
            // this one has to be 2 chars:
            if (modifierCode.Length < 2)
                modifierCode = modifierCode.PadLeft(2, '0');

            string symbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            string modifierIcon = symbolSetString + modifierCode + sModifierNumber;
            sb.Append(modifierIcon);

            return sb.ToString();
        }

        public static string GetModfierIconName(ref MilitarySymbol milSymbol, int modifierNumber)
        {
            StringBuilder sb = new StringBuilder();

            if (!((modifierNumber == 1) || (modifierNumber == 2)))
                return string.Empty;

            string sModifierNumber = "1";
            string sModifier = milSymbol.Id.ModifierOne;

            if (modifierNumber == 2)
            {
                sModifierNumber = "2";
                sModifier = milSymbol.Id.ModifierTwo;
            }

            string symbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(milSymbol.Id.SymbolSet, 2);

            string modifierIcon = symbolSetString + sModifier + sModifierNumber;
            sb.Append(modifierIcon);

            return sb.ToString();
        }

        // Center/Main Icon Modifiers: { # = 1 | 2 }, format = 
        // Appendices\{SymbolSetTypeName}\Mod{#}\{SymbolSetType} + {ModifierCode} + {#}
        public static string GetModfierIconNameWithFolder(SymbolSetType symbolSet, int modifierNumber, int modifierCodeInt)
        {
            StringBuilder sb = new StringBuilder();

            if (!((modifierNumber == 1) || (modifierNumber == 2)))
                return string.Empty;

            string sSubFolderName = "mod" + modifierNumber.ToString();

            string currentAppendixHome = GetIconFolderName(symbolSet);
            sb.Append(currentAppendixHome);

            sb.Append(sSubFolderName);
            sb.Append(System.IO.Path.DirectorySeparatorChar);

            string modifierIcon = GetModfierIconName(symbolSet, modifierNumber, modifierCodeInt);
            sb.Append(modifierIcon);

            sb.Append(ImageSuffix);

            return sb.ToString();
        }

        // same as ImageFilesHome + GetModfierIconNameWithFolder
        public static string GetModfierIconNameWithFullPath(SymbolSetType symbolSet, int modifierNumber, int modifierCodeInt)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ImageFilesHome);

            string modifierIconNameWithoutImageFilesHome = GetModfierIconNameWithFolder(
                symbolSet, modifierNumber, modifierCodeInt);

            sb.Append(modifierIconNameWithoutImageFilesHome);

            return sb.ToString();
        }

        public static string GetModfierIconNameWithFolder(ref MilitarySymbol milSymbol, int modifierNumber)
        {
            if (!((modifierNumber == 1) || (modifierNumber == 2)))
                return string.Empty;

            string sModifierCode = (modifierNumber == 1) ? milSymbol.Id.ModifierOne : milSymbol.Id.ModifierTwo;

            int modifierCodeInt = Convert.ToInt32(sModifierCode);

            string modifierIconNameWithFolder = GetModfierIconNameWithFolder(
                milSymbol.Id.SymbolSet, modifierNumber, modifierCodeInt);

            return modifierIconNameWithFolder;
        }

        // Echelon/Mobility/Towed Array Amplifier Icon
        // = StandardIdentityAffiliationType + EchelonMobilityType
        // "Amplifier (Echelon): Uses SIDC positions 4 and 9-10."
        // ex. Friend, Team Crew = 311 
        //      --> 3 (Friend) + 11 (Echelon = Team/Crew)
        //
        public static string GetEchelonIconNameWithFolder(
            SymbolSetType symbolSet,
            StandardIdentityAffiliationType affiliation,
            EchelonMobilityType echelonMobility)
        {
            if (echelonMobility == EchelonMobilityType.NoEchelonMobility)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            if (echelonMobility < EchelonMobilityType.Wheeled_Limited_Cross_Country)
            {
                sb.Append("Echelon");
            }
            else
            {
                sb.Append("Amplifier");
            }

            sb.Append(System.IO.Path.DirectorySeparatorChar);

            StandardIdentityAffiliationType mappedAffiliation =
                    TypeUtilities.AffiliationToAffiliationFrameMapping[affiliation];

            string affiliationString = 
                TypeUtilities.EnumHelper.getEnumValAsString(mappedAffiliation, 1);
            sb.Append(affiliationString);

            // IMPORTANT NOTE: 
            // Only Land Symbol Sets (Unit, Civilian, Equipment, Installation)
            // have echelon symbols so they only match/line up with those frames

            string echelonString = TypeUtilities.EnumHelper.getEnumValAsString(echelonMobility, 2);
            sb.Append(echelonString);

            sb.Append(ImageSuffix);

            return sb.ToString();
        }

        // same as ImageFilesHome + GetEchelonIconNameWithFolder
        public static string GetEchelonIconNameWithFullPath(
            SymbolSetType symbolSet,
            StandardIdentityAffiliationType affiliation,
            EchelonMobilityType echelonMobility)
        {
            if (echelonMobility == EchelonMobilityType.NoEchelonMobility)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(ImageFilesHome);

            string echelonIconNameWithoutImageFilesHome =
                GetEchelonIconNameWithFolder(symbolSet, affiliation, echelonMobility);

            sb.Append(echelonIconNameWithoutImageFilesHome);

            return sb.ToString();
        }

        // Headquarters(HQ)/Task Force(TF)/Feint/Dummy(FD) HQ/TF/FD Amplifier Icon
        // = StandardIdentityAffiliationType + SymbolSetType + HeadquartersTaskForceDummyType
        // Uses SIDC positions 4-6 and position 8
        // ex. Friend(3), Land_Unit (10), Feint_Dummy (1) = 3101
        //      --> 3 (Friend) + Land_Unit (10) + 1 (Feint_Dummy)
        //
        public static string GetHqTfFdIconNameWithFolder(
            SymbolSetType symbolSet,
            StandardIdentityAffiliationType affiliation,
            HeadquartersTaskForceDummyType hqTfFd)
        {
            if (hqTfFd == HeadquartersTaskForceDummyType.NoHQTFDummyModifier)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("HQTFFD");
            sb.Append(System.IO.Path.DirectorySeparatorChar);

            StandardIdentityAffiliationType mappedAffiliation =
                    TypeUtilities.AffiliationToAffiliationFrameMapping[affiliation];

            string affiliationValueString =
                TypeUtilities.EnumHelper.getEnumValAsString(mappedAffiliation, 1);
            sb.Append(affiliationValueString);

            // map the actual symbolSet to the supported/availble frame
            SymbolSetType mappedSymbolSet = TypeUtilities.SymbolSetToFrameMapping[symbolSet];

            // Exceptional cases for this one 
            if ((symbolSet == SymbolSetType.Activities) || (symbolSet == SymbolSetType.NotSet) ||
                (symbolSet == SymbolSetType.Unknown))
                mappedSymbolSet = SymbolSetType.Land_Unit; 

            string mappedSymbolSetValueString = TypeUtilities.EnumHelper.getEnumValAsString(mappedSymbolSet, 2);
            sb.Append(mappedSymbolSetValueString);

            string hqTfFdValueString = TypeUtilities.EnumHelper.getEnumValAsString(hqTfFd, 1);
            sb.Append(hqTfFdValueString);
            sb.Append(ImageSuffix);

            return sb.ToString();
        }

        // same as ImageFilesHome + GetHqTfFdIconNameWithFolder
        public static string GetHqTfFdIconNameWithFullPath(
            SymbolSetType symbolSet,
            StandardIdentityAffiliationType affiliation,
            HeadquartersTaskForceDummyType hqTfFd)
        {
            if (hqTfFd == HeadquartersTaskForceDummyType.NoHQTFDummyModifier)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(ImageFilesHome);

            string hqTfFdIconNameWithoutImageFilesHome =
                GetHqTfFdIconNameWithFolder(symbolSet, affiliation, hqTfFd);

            sb.Append(hqTfFdIconNameWithoutImageFilesHome);

            return sb.ToString();
        }

        // OperationalCondition Amplifier Icon
        // = StandardIdentityAffiliationType + SymbolSetType + StatusType
        // Uses SIDC positions 3-7 + "2"
        // ex. Friend(03), Land_Unit (10), Damaged (3) = 031012
        //      --> 03 (Friend) + Land_Unit (10) + 1 (Damaged) + "2" (2nd format)
        //
        public static string GetOperationalConditionNameWithFolder(
            SymbolSetType symbolSet,
            StandardIdentityAffiliationType affiliation,
            StatusType status)
        {
            if ((status == StatusType.NotSet) || 
                (status == StatusType.Present) || 
                (status == StatusType.Planned_Anticipated_Suspect))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("OCA");
            sb.Append(System.IO.Path.DirectorySeparatorChar);

            StandardIdentityAffiliationType mappedAffiliation =
                    TypeUtilities.AffiliationToAffiliationFrameMapping[affiliation];

            // TRICKY: used 2 digits for some reason
            string affiliationValueString =
                TypeUtilities.EnumHelper.getEnumValAsString(mappedAffiliation, 2);
            sb.Append(affiliationValueString);

            // map the actual symbolSet to the supported/availble frame
            SymbolSetType mappedSymbolSet = TypeUtilities.SymbolSetToFrameMapping[symbolSet];

            string mappedSymbolSetValueString = TypeUtilities.EnumHelper.getEnumValAsString(mappedSymbolSet, 2);
            sb.Append(mappedSymbolSetValueString);

            string opConditionString = TypeUtilities.EnumHelper.getEnumValAsString(status, 1);
            sb.Append(opConditionString);

            sb.Append("2");

            sb.Append(ImageSuffix);

            return sb.ToString();
        }

        // same as ImageFilesHome + GetOperationalConditionIconNameWithFolder
        public static string GetOperationalConditionNameWithFullPath(
            SymbolSetType symbolSet,
            StandardIdentityAffiliationType affiliation,
            StatusType status)
        {
            if ((status == StatusType.NotSet) ||
                (status == StatusType.Present) ||
                (status == StatusType.Planned_Anticipated_Suspect))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(ImageFilesHome);

            string opConditionNameWithoutImageFilesHome =
                GetOperationalConditionNameWithFolder(symbolSet, affiliation, status);

            sb.Append(opConditionNameWithoutImageFilesHome);

            return sb.ToString();
        }

        // Land Unit Special Entity Icon
        public static string GetLandUnitSpecialEntityIconNameWithFullPath(SymbolIdCode id)
        {
            if (!id.IsLandUnitSpecialEntity)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(ImageFilesHome);
            sb.Append(@"Appendices\Land\10xxxx");
            sb.Append(id.EntitySubTypeField);
            sb.Append(TypeUtilities.AffiliationFrameToSuffixName[id.Affiliation]);
            sb.Append(ImageSuffix);

            return sb.ToString();
        }
        
        // Frame Icon
        // StandardIdentityAffiliationType + SymbolSetType
        // IMPORTANT: StandardIdentityAffiliationType must be in 
        // { Unknown, Space, Air,Land_Unit, Land_Installation, 
        //   Sea_Surface, Subsurface, Activity } 
        // See: TypeUtilities.SymbolSetToFrameType for mapping
        // ex. 0520
        public static string GetFrameIconNameWithFolder(
                        StandardIdentityRealExerciseSimType realExerciseSim,
                        StandardIdentityAffiliationType affiliation,
                        SymbolSetType symbolSet,
                        StatusType status)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Frames");
            sb.Append(System.IO.Path.DirectorySeparatorChar);

            if (realExerciseSim == StandardIdentityRealExerciseSimType.Reality)
                sb.Append("0_");
            else if (realExerciseSim == StandardIdentityRealExerciseSimType.Exercise)
            {
                sb.Append("Exercise");
                sb.Append(System.IO.Path.DirectorySeparatorChar);
                sb.Append("1_");
            }
            else if (realExerciseSim == StandardIdentityRealExerciseSimType.Simulation)
            {
                sb.Append("Sim");
                sb.Append(System.IO.Path.DirectorySeparatorChar);
                sb.Append("2_");
            }

            string affiliationString = TypeUtilities.EnumHelper.getEnumValAsString(affiliation, 1);
            sb.Append(affiliationString);

            // map the actual symbolSet to the supported/available frame
            SymbolSetType mappedSymbolSet = TypeUtilities.SymbolSetToFrameMapping[symbolSet];

            string mappedSymbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(mappedSymbolSet, 2);
            sb.Append(mappedSymbolSetString);

            string status_suffix = "_0"; // normal case

            // Planned / Present Status (but only for standard/non-anticipated frames)
            if (status == StatusType.Planned_Anticipated_Suspect)
                if ((affiliation == StandardIdentityAffiliationType.Friend) ||
                    (affiliation == StandardIdentityAffiliationType.Hostile) ||
                    (affiliation == StandardIdentityAffiliationType.Neutral) ||
                    (affiliation == StandardIdentityAffiliationType.Unknown))
                    status_suffix = "_1";

            sb.Append(status_suffix);

            if (UseCivilianFrames)
            {
                if (affiliation <= StandardIdentityAffiliationType.Neutral)
                    sb.Append("c");
            }

            sb.Append(ImageSuffix);

            return sb.ToString();
        }

        public static string GetFrameIconNameWithFullPath(
                        StandardIdentityRealExerciseSimType realExerciseSim,
                        StandardIdentityAffiliationType affiliation,
                        SymbolSetType symbolSet,
                        StatusType status)
        {
            if (affiliation >= StandardIdentityAffiliationType.Dont_Display_Frame)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append(ImageFilesHome);

            string frameIconNameWithoutImageFilesHome =
                GetFrameIconNameWithFolder(realExerciseSim, affiliation, symbolSet, status);

            sb.Append(frameIconNameWithoutImageFilesHome);

            return sb.ToString();
        }

        public static bool SetMilitarySymbolGraphicLayers(ref MilitarySymbol milSymbol)
        {
            if (!System.IO.Directory.Exists(ImageFilesHome))
            {
                System.Diagnostics.Trace.WriteLine("--> Images Home *DOES NOT EXIST* : " + ImageFilesHome);
                return false;
            }

            if ((milSymbol == null) || (milSymbol.Id == null) ||
                (milSymbol.GraphicLayers == null) || (!milSymbol.Id.IsValid))
                return false;

            milSymbol.GraphicLayers.Clear();

            //////////////////////////////////////////////////////////////////////////
            // Assembly the layers

            // Start with the Frame (if framed)
            if (TypeUtilities.HasFrame(milSymbol.Id.SymbolSet, milSymbol.Id.EntityCode))
            {
                string frameIconNameWithFullPath =
                    GetFrameIconNameWithFullPath(
                        milSymbol.Id.StandardIdentity,
                        milSymbol.Id.Affiliation,
                        milSymbol.Id.SymbolSet,
                        milSymbol.Id.Status);

                if (!string.IsNullOrEmpty(frameIconNameWithFullPath))
                    milSymbol.GraphicLayers.Add(frameIconNameWithFullPath);            
            }
            //////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////////
            // Main Icon Layer (all symbols should have this)
            string mainIconNameFullPath = GetMainIconNameWithFullPath(ref milSymbol);

            milSymbol.GraphicLayers.Add(mainIconNameFullPath);
            //////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////////
            // Skip the remaining if no more layers needed
            bool skipRemainingLayers = !TypeUtilities.HasFrame(milSymbol.Id.SymbolSet);

            if (!skipRemainingLayers)
            {
                StringBuilder sb = new StringBuilder();

                // Main Icon Modfier 1
                if (!string.IsNullOrEmpty(milSymbol.Id.ModifierOne)
                    && (milSymbol.Id.ModifierOne != "00")) // TODO: find better way of checking that this isn't set/valid
                {
                    sb.Clear();
                    sb.Append(ImageFilesHome);

                    string modifierIconNameWithFolder = 
                        GetModfierIconNameWithFolder(ref milSymbol, 1);

                    sb.Append(modifierIconNameWithFolder);
                    milSymbol.GraphicLayers.Add(sb.ToString());
                }

                // Main Icon Modfier 2
                if (!string.IsNullOrEmpty(milSymbol.Id.ModifierTwo)
                    && (milSymbol.Id.ModifierTwo != "00")) // TODO: find better way of checking that this isn't set/valid
                {
                    sb.Clear();
                    sb.Append(ImageFilesHome);

                    string modifierIconNameWithFolder = 
                        GetModfierIconNameWithFolder(ref milSymbol, 2);

                    sb.Append(modifierIconNameWithFolder);
                    milSymbol.GraphicLayers.Add(sb.ToString());
                }

                // Echelon/Mobility Modifier
                if (milSymbol.Id.EchelonMobility != EchelonMobilityType.NoEchelonMobility)
                {
                    string echelonIconNameWithFullPath =
                        GetEchelonIconNameWithFullPath(
                            milSymbol.Id.SymbolSet,
                            milSymbol.Id.Affiliation,
                            milSymbol.Id.EchelonMobility);

                    if (echelonIconNameWithFullPath.Length > 0)
                        milSymbol.GraphicLayers.Add(echelonIconNameWithFullPath);
                }

                // Headquarters/TF/FD Modifier
                if (milSymbol.Id.HeadquartersTaskForceDummy !=  
                    HeadquartersTaskForceDummyType.NoHQTFDummyModifier)
                {
                    string hqTfFdIconNameWithFullPath =
                        GetHqTfFdIconNameWithFullPath(
                            milSymbol.Id.SymbolSet,
                            milSymbol.Id.Affiliation,
                            milSymbol.Id.HeadquartersTaskForceDummy);

                    if (hqTfFdIconNameWithFullPath.Length > 0)
                        milSymbol.GraphicLayers.Add(hqTfFdIconNameWithFullPath);
                }

                // Status/Operational Condition Icon 
                // (Note: if "planned" was set in the frame used above)
                if ((milSymbol.Id.Status !=  StatusType.NotSet) &&
                    (milSymbol.Id.Status > StatusType.Planned_Anticipated_Suspect))
                {
                    string opConditionIconNameWithFullPath =
                        GetOperationalConditionNameWithFullPath(
                            milSymbol.Id.SymbolSet,
                            milSymbol.Id.Affiliation,
                            milSymbol.Id.Status);

                    if (opConditionIconNameWithFullPath.Length > 0)
                        milSymbol.GraphicLayers.Add(opConditionIconNameWithFullPath);
                }

                if (milSymbol.Id.IsLandUnitSpecialEntity)
                {
                    // TABLE D-V. "Land unit icons – special entity subtypes"
                    string landUnitSpecialEntityIconNameWithFullPath =
                        GetLandUnitSpecialEntityIconNameWithFullPath(milSymbol.Id);

                    if (landUnitSpecialEntityIconNameWithFullPath.Length > 0)
                        milSymbol.GraphicLayers.Add(landUnitSpecialEntityIconNameWithFullPath);
                }

            } // end skipRemainingLayers

            // check this before we add debug/unknown icons at the end
            int validLayerCount = 0;
            // Look at the layers to see if any do not exist:
            foreach (string graphicLayer in milSymbol.GraphicLayers)
            {
                if (!System.IO.File.Exists(graphicLayer))
                    System.Diagnostics.Trace.WriteLine("SetMilitarySymbolGraphicLayers: Could not find layer: " + graphicLayer);
                else
                    validLayerCount++;
            }

            bool success = validLayerCount > 0;

            if (AddReferenceCenterPoint)
            {
                // For testing if icons are centered properly:
                // Outer Circle:
                string centerIcon = ImageFilesHome +
                    @"Appendices\ControlMeasures\25200500.svg"; // (Large Yellow Circle)
                milSymbol.GraphicLayers.Add(centerIcon);

                // Center Dot/Crosshair:
                string centerIcon2 = ImageFilesHome +
                    // @"Appendices\Land\10130300.svg"; // (Center Black Circle)
                    @"Appendices\Land\10161300_2.svg";  // (Center Black Crosshairs)
                milSymbol.GraphicLayers.Add(centerIcon2);
            }

            if (!success)
            {
                // add a default symbol
                string defaultUnknownIcon = ImageFilesHome + @"Appendices\98100000.svg";
                milSymbol.GraphicLayers.Add(defaultUnknownIcon);
            }

            return success;

        }

    }
}
