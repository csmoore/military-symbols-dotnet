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
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MilitarySymbols;

namespace ExportBitmap
{
    class Program
    {
        /// <summary>
        /// As a command line argument, takes either:
        /// {Symbol ID Code (SIDC)} 
        /// {CSV filename} 
        /// {"ALL"} 
        /// {SymbolSet Code}, ex: "10"
        /// 
        /// Makes these assumptions:
        /// For SIDC:
        /// if Length == 20 | Length == 8 ->  Delta Code (full or shortened) Supplied - export
        /// if Length == 15 | Length == 10 -> Charlie Code (full or shortened) Supplied - convert to Delta, then export
        /// if Length == 2 -> Export all  Delta Codes from that Symbol Set Code
        /// 
        /// For CSV File:
        /// First Column (Column 0) has the SIDC
        /// 
        /// </summary>
        static void Main(string[] args)
        {
            // string arg = "25131800"; // Waypoint
            string arg = "10031004131204000000"; // Anti Tank Task Force

            if (args.Length < 1)
            {
                Usage();
            }
            else
            {
                arg = args[0];
            }

            if (!CheckSettings())
                return;

            if (arg.StartsWith("ALL2525C"))  // export all known 2525C symbols
                ProcessAll2525C();
            else if ((arg.StartsWith("ALL") || arg.Length == 2)) // export all known symbols
                ProcessAll(arg);
            else if (arg.EndsWith("csv")) // csv file supplied
                ProcessCsv(arg);
            else if (arg.StartsWith("ATTRIBUTES"))
                ProcessAttributes(args);
            else if (arg.StartsWith("ATTRIBUTE_TEST"))
                ProcessAttributeTest();
            else                          // export a single code
                ProcessSidc(arg); 

        }

        static void ProcessAttributeTest()
        {
            Dictionary<string, string> attributeSet = new Dictionary<string, string>();

            attributeSet["context"] = "10";
            attributeSet["identity"] = "3";
            attributeSet["symbolset"] = "10";
            attributeSet["symbolentity"] = "110100";
            attributeSet["modifier1"] = "01";
            attributeSet["modifier2"] = "01";
            attributeSet["echelon"] = "01";
            attributeSet["indicator"] = "1";
            attributeSet["operationalcondition"] = "1";

            SymbolIdCode sidc = new SymbolIdCode(attributeSet);

            ExportSymbolId(sidc);
        }

        static void ProcessAttributes(string[] args)
        {
            Dictionary<string, string> attributeSet = new Dictionary<string, string>();

            if (args.Length > 1)
                attributeSet["context"] = args[1];
            if (args.Length > 2)
                attributeSet["identity"] = args[2];
            if (args.Length > 3)
                attributeSet["symbolset"] = args[3];
            if (args.Length > 4)
                attributeSet["symbolentity"] = args[4];
            if (args.Length > 5)
                attributeSet["modifier1"] = args[5];
            if (args.Length > 6)
                attributeSet["modifier2"] = args[6];
            if (args.Length > 7)
                attributeSet["echelon"] = args[7];
            if (args.Length > 8)
                attributeSet["indicator"] = args[8];
            if (args.Length > 9)
                attributeSet["operationalcondition"] = args[9];

            SymbolIdCode sidc = new SymbolIdCode(attributeSet);

            ExportSymbolId(sidc);
        }

        static void Usage()
        {
            Console.WriteLine("ExportBitmap [SymbolIDCode | {Filename}.csv | ALL | {SymbolSet Code}]");
        }

        static void ProcessSidc(string sidc)
        {
            if (sidc.Length == 20)
                ExportFullSymbolId(sidc);
            else if (sidc.Length == 8)
                ExportSymbolSetEntityCode(sidc);
            else if ((sidc.Length == 15) || (sidc.Length == 10))
                ExportCharlieSymbol(sidc);
            else
                Console.WriteLine("Failed to recognize SIDC: " + sidc + ", Length = " + sidc.Length);
        }

        static void ExportSymbolId(SymbolIdCode id, string optionalLegacyCode = "")
        {
            int width = 256, height = 256;

            Size exportSize = new Size(width, height);

            System.Drawing.Bitmap exportBitmap;

            bool success = Utilities.ExportSymbol(id, out exportBitmap, exportSize);

            if (!success || (exportBitmap == null))
            {
                Console.WriteLine("Export failed for ID:" + id.HumanReadableCode());
                return;
            }

            string imageFileName = id.HumanReadableCode();
            if (!string.IsNullOrWhiteSpace(optionalLegacyCode))
                imageFileName += "-" + optionalLegacyCode;

            string filepath = getFullFileName(imageFileName);

            Console.WriteLine("Exporting File: " + filepath);

            exportBitmap.Save(filepath);
        }
 
        static void ExportFullSymbolId(string sidcString)
        {
            if (sidcString.Length < 20)
            {
                Usage();
                return;
            }

            SymbolIdCode sidc = new SymbolIdCode();
            sidc.Code = sidcString;

            ExportSymbolId(sidc);
        }

        static void ExportSymbolSetEntityCode(string symbolSetEntityCode)
        {
            if (symbolSetEntityCode.Length < 8)
            {
                Usage();
                return;
            }

            SymbolIdCode sidc = new SymbolIdCode();

            string symbolSetString = symbolSetEntityCode.Substring(0, 2);

            sidc.SymbolSet = (SymbolSetType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetString);
            sidc.EntityCode = symbolSetEntityCode.Substring(2, 6);

            sidc.Affiliation = StandardIdentityAffiliationType.Friend;
            ExportSymbolId(sidc);

            // Export all 4 affiliations if only SymbolSet & EntityCode supplied (& has frame)
            if (TypeUtilities.HasFrame(sidc.SymbolSet))
            {
                sidc.Affiliation = StandardIdentityAffiliationType.Hostile;
                ExportSymbolId(sidc);

                sidc.Affiliation = StandardIdentityAffiliationType.Neutral;
                ExportSymbolId(sidc);

                sidc.Affiliation = StandardIdentityAffiliationType.Unknown;
                ExportSymbolId(sidc);
            }
        }

        static void ExportCharlieSymbol(string code2525Charlie)
        {
            SymbolIdCode sidc = new SymbolIdCode();

            bool success = Utilities.ConvertCodeCharlieToDelta(code2525Charlie, out sidc);

            if (!success)
            {
                Console.WriteLine("Could not convert to Delta SIDC: " + code2525Charlie);
                return;
            }

            Console.Write("2525C: " + code2525Charlie + ", ");
            ExportFullSymbolId(sidc.Code);
        }

        static string getFullFileName(string sidc)
        {
            string cwd = System.Environment.CurrentDirectory;

            string filepath = cwd + System.IO.Path.DirectorySeparatorChar;

            const string IMAGE_FILE_EXTENSION = ".png";

            filepath += (sidc + IMAGE_FILE_EXTENSION);

            return filepath;
        }

        static bool CheckSettings()
        {
            // Set SVG Images Home if set in App Settings
            string appSettingsSvgHomeKey = "SVGImagesHome";
            var svgHomeFolderSetting = ConfigurationManager.AppSettings[appSettingsSvgHomeKey];
            if (!string.IsNullOrWhiteSpace(svgHomeFolderSetting))
            {
                if (!Utilities.SetImageFilesHome(svgHomeFolderSetting))
                {
                    Console.WriteLine("App.config setting for SVGImagesHome does not exist, export failed!");
                    Console.WriteLine("Setting: " + svgHomeFolderSetting);
                    return false;
                }
            }

            if (!Utilities.CheckImageFilesHomeExists())
            {
                Console.WriteLine("Image folder does not exist, export failed!");
                return false;
            }

            // Debug setting to overlay Center Point/Crosshairs to check centering
            string appSettingsShowCenterPoint = "ShowCenterPoint";
            bool showCenterPointSetting = Convert.ToBoolean(ConfigurationManager.AppSettings[appSettingsShowCenterPoint]);
            if (showCenterPointSetting)
            {
                MilitarySymbolToGraphicLayersMaker.AddReferenceCenterPoint = true;
            }

            return true;
        }

        static void ProcessCsv(string csvFile)
        {
            if (!File.Exists(csvFile))
            {
                Console.WriteLine("Could not find file: " + csvFile);
                return;
            }

            bool firstRow = true;

            foreach (string line in File.ReadLines(csvFile))
            {
                if (line.StartsWith("#")) // allow "#" comment character
                    continue;

                if (firstRow)  // skip 1st/Header Row
                {
                    firstRow = false;
                    continue;
                }

                string[] values = line.Split(',');

                // Providing this as a setting in case your csv SIDCs are not at index 0 
                // & you don't want to change the data, just set this index
                const int SIDC_INDEX = 0;
                if (values.Length < (SIDC_INDEX + 1))
                    continue;

                string sidc = values[SIDC_INDEX];
                ProcessSidc(sidc);
            }
        }

        static void ProcessAll(string symbolSetAsString)
        {
            SymbolSetType symbolSet = SymbolSetType.NotSet;

            if (symbolSetAsString.Length == 2)
               symbolSet = (SymbolSetType)TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetAsString);

            List<MilitarySymbol> matchingSymbols = Utilities.GetMilitarySymbols(symbolSet);

            if (matchingSymbols.Count == 0)
                Console.WriteLine("No Symbols found for Set: " + symbolSetAsString);

            foreach (MilitarySymbol matchSymbol in matchingSymbols)
            {
                Console.Write("SimpleCode=" + matchSymbol.Id.HumanReadableCode());
                Console.Write(", Name=" + matchSymbol.Id.Name);
                Console.WriteLine(", Tags=" + matchSymbol.TagsAsString);

                // Unknown
                ExportSymbolId(matchSymbol.Id);

                // Export each affiliation if it has a frame
                if (TypeUtilities.HasFrame(matchSymbol.Id.SymbolSet))
                {
                    matchSymbol.Id.Affiliation = StandardIdentityAffiliationType.Friend;
                    ExportSymbolId(matchSymbol.Id);

                    matchSymbol.Id.Affiliation = StandardIdentityAffiliationType.Hostile;
                    ExportSymbolId(matchSymbol.Id);

                    matchSymbol.Id.Affiliation = StandardIdentityAffiliationType.Neutral;
                    ExportSymbolId(matchSymbol.Id);
                }
            }
        }

        static void ProcessAll2525C()
        {
            // Get all 2525C symbols that the system knows about
            List<MilitarySymbol> matchingSymbols = Utilities.GetMilitarySymbolsFromCharlie();

            if (matchingSymbols.Count == 0)
                Console.WriteLine("No 2525C Symbols found.");

            foreach (MilitarySymbol matchSymbol in matchingSymbols)
            {
                string legacyCode = matchSymbol.Legacy2525Code;
                Console.Write(legacyCode);
                Console.Write("," + matchSymbol.Id.HumanReadableCode(false));
                Console.Write("," + matchSymbol.Id.Name);
                Console.WriteLine(",\"" + matchSymbol.TagsAsString + "\"");

                ExportSymbolId(matchSymbol.Id, legacyCode);
            }
        }

    }
}
