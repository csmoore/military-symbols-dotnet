﻿/* 
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
    /// (NOTE: a possible TODO could be to remove this MilitarySymbol dependency, 
    ///  this was just done for rapid prototyping)
    /// We might also replace this complicated logic with a table lookup, but for now
    /// it is a nice way to validate that the rules are actually consistent.
    /// </summary>
    public class MilitarySymbolToGraphicLayersMaker
    {
        // IMPORTANT: defaults to {exe folder}\MIL_STD_2525D_Symbols (mainly for deployment)
        private static readonly string DEFAULT_PATH =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MIL_STD_2525D_Symbols");

        // TODO/IMPORTANT: 
        // If you don't want to use the default: 
        // 1) You must uncomment & set this to the location on your machine &
        // 2) Change imageFilesHome default/TODO below
        private static readonly string ALTERNATE_PATH = 
                        @"[!!!!!!!!!!!SET_THIS_FOLDER_IF_DESIRED!!!!!!!!!!!]";

        // - OR - set this at Runtime with 
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
            // ALTERNATE_PATH // <-- TODO: SET THIS to ALTERNATE_PATH if you don't want to use default
            DEFAULT_PATH      // (and comment out this) 
            + System.IO.Path.DirectorySeparatorChar; // IMPORTANT/NOTE: Ends in DirectorySeparator

        const string ImageSuffix = ".svg";

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
            string mainIcon = GetMainIconName(milSymbol.Id.SymbolSet, milSymbol.Id.FullEntityCode);

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

            // WORKAROUND/TRICKY: some symbols have wacky _0, _1, _2, _3 thing instead of base version
            // Method: Find the frame from the affiliation & add _0, _1, _2, _3 accordingly
            if (!System.IO.File.Exists(mainIconNameFullPath))
            {
                string newFrameSuffix =
                    TypeUtilities.AffiliationFrameToSuffixName[milSymbol.Id.Affiliation] + ImageSuffix;

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
            string sModifier = milSymbol.Id.FirstModifier;

            if (modifierNumber == 2)
            {
                sModifierNumber = "2";
                sModifier = milSymbol.Id.SecondModifier;
            }

            string symbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(milSymbol.Id.SymbolSet, 2);

            string modifierIcon = symbolSetString + sModifier + sModifierNumber;
            sb.Append(modifierIcon);

            return sb.ToString();
        }

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

            string sModifierCode = (modifierNumber == 1) ? milSymbol.Id.FirstModifier : milSymbol.Id.SecondModifier;

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

            // TODO: exercise/sim frames - 
            // StandardIdentityRealExerciseSimType (just add "Sim" "Exercise")

            sb.Append("0_"); // TODO change to 1_, 2_ for Sim, Exercise

            StandardIdentityAffiliationType mappedAffiliation = affiliation;
            if (status == StatusType.Planned_Anticipated_Suspect)
            {
                // if a planning symbol, we need to swap out the affiliation
                mappedAffiliation = TypeUtilities.AffiliationToPlanningFrameMapping[affiliation];
            }

            string affiliationString = TypeUtilities.EnumHelper.getEnumValAsString(mappedAffiliation, 1);
            sb.Append(affiliationString);

            // map the actual symbolSet to the supported/available frame
            SymbolSetType mappedSymbolSet = TypeUtilities.SymbolSetToFrameMapping[symbolSet];

            string mappedSymbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(mappedSymbolSet, 2);
            sb.Append(mappedSymbolSetString);

            sb.Append(ImageSuffix);

            return sb.ToString();
        }

        public static string GetFrameIconNameWithFullPath(
                        StandardIdentityRealExerciseSimType realExerciseSim,
                        StandardIdentityAffiliationType affiliation,
                        SymbolSetType symbolSet,
                        StatusType status)
        {
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

            // Start with the Frame
            if (TypeUtilities.HasFrame(milSymbol.Id.SymbolSet))
            {
                //sb.Clear();
                //sb.Append(ImageFilesHome);

                string frameIconNameWithFullPath =
                    GetFrameIconNameWithFullPath(
                        milSymbol.Id.StandardIdentity,
                        milSymbol.Id.Affiliation,
                        milSymbol.Id.SymbolSet,
                        milSymbol.Id.Status);

                milSymbol.GraphicLayers.Add(frameIconNameWithFullPath);            
            }
            //////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////////
            // Main Icon Layer
            string mainIconNameFullPath = GetMainIconNameWithFullPath(ref milSymbol);

            milSymbol.GraphicLayers.Add(mainIconNameFullPath);
            //////////////////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////////////////////
            // Skip the remaining if no more layers needed
            //
            // TODO: Verify this logic
            //       Stop here for Control Measures (Lines/Areas for now) & 
            //       Symbols *without* frames
            //
            bool skipRemainingLayers = false;
            if ((milSymbol.Shape == ShapeType.Line) || (milSymbol.Shape == ShapeType.Area) || 
                (!TypeUtilities.HasFrame(milSymbol.Id.SymbolSet)))
                skipRemainingLayers = true;

            if (!skipRemainingLayers)
            {
                StringBuilder sb = new StringBuilder();

                // Center/Main Icon Modifiers: { # = 1 | 2 }
                // Appendices\{SymbolSetTypeName}\Mod{#}\{SymbolSetType} + {ModifierCode} + {#}

                // Main Icon Modfier 1
                if (!string.IsNullOrEmpty(milSymbol.Id.FirstModifier)
                    && (milSymbol.Id.FirstModifier != "00")) // TODO: find better way of checking that this isn't set/valid
                {
                    sb.Clear();
                    sb.Append(ImageFilesHome);

                    string modifierIconNameWithFolder = 
                        GetModfierIconNameWithFolder(ref milSymbol, 1);

                    sb.Append(modifierIconNameWithFolder);
                    milSymbol.GraphicLayers.Add(sb.ToString());
                }

                // Main Icon Modfier 2
                if (!string.IsNullOrEmpty(milSymbol.Id.SecondModifier)
                    && (milSymbol.Id.SecondModifier != "00")) // TODO: find better way of checking that this isn't set/valid
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

                if (milSymbol.Id.Status !=  StatusType.NotSet)
                {
                    string opConditionIconNameWithFullPath =
                        GetOperationalConditionNameWithFullPath(
                            milSymbol.Id.SymbolSet,
                            milSymbol.Id.Affiliation,
                            milSymbol.Id.Status);

                    if (opConditionIconNameWithFullPath.Length > 0)
                        milSymbol.GraphicLayers.Add(opConditionIconNameWithFullPath);
                }
                
                // Other? ex. "Land unit icons – special entity subtypes"

            } // end skipRemainingLayers

            //TODO: look at the layers to see if any do not exist:
            foreach (string graphicLayer in milSymbol.GraphicLayers)
            {
                if (!System.IO.File.Exists(graphicLayer))
                    System.Diagnostics.Trace.WriteLine("SetMilitarySymbolGraphicLayers: Could not find layer: " + graphicLayer);
            }

            if (milSymbol.GraphicLayers.Count == 0)
                return false;
            else
                return true;

        }

    }
}