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
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MilitarySymbols;

namespace ExportBitmap
{
    class Program
    {
        static void Main(string[] args)
        {
            // string sidc = "25131800"; // Waypoint
            string sidc = "10031004131204000000"; // Anti Tank Task Force

            if (args.Length < 1)
            {
                Usage();
            }
            else
            {
                sidc = args[0];
            }

            // Set SVG Images Home if set in App Settings
            string appSettingsSvgHomeKey = "SVGImagesHome";
            var svgHomeFolderSetting = ConfigurationManager.AppSettings[appSettingsSvgHomeKey];
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
            {
                Console.WriteLine("Image folder does not exist, export failed!");
                return;
            }

            if (sidc.Length == 20)
                ExportFullSymbolId(sidc);
            else if (sidc.Length == 8)
                ExportSymbolSetEntityCode(sidc);
            else
                Usage();

        }

        static void Usage()
        {
            Console.WriteLine("ExportBitmap [SymbolIDCode]");
        }

        static void ExportSymbolId(SymbolIdCode id)
        {
            int width = 256, height = 256;

            Size exportSize = new Size(width, height);

            System.Drawing.Bitmap exportBitmap;

            bool success = Utilities.ExportSymbol(id, out exportBitmap, exportSize);

            if (!success || (exportBitmap == null))
            {
                Console.WriteLine("Export failed!");
                return;
            }

            string filepath = getFullFileName(id.Code);

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
            sidc.FullEntityCode = symbolSetEntityCode.Substring(2, 6);

            // Export all 4 affiliations if only SymbolSet & EntityCode supplied

            sidc.Affiliation = StandardIdentityAffiliationType.Friend;
            ExportSymbolId(sidc);

            sidc.Affiliation = StandardIdentityAffiliationType.Hostile;
            ExportSymbolId(sidc);

            sidc.Affiliation = StandardIdentityAffiliationType.Neutral;
            ExportSymbolId(sidc);

            sidc.Affiliation = StandardIdentityAffiliationType.Unknown;
            ExportSymbolId(sidc);

        }

        static string getFullFileName(string sidc)
        {
            string cwd = System.Environment.CurrentDirectory;

            string filepath = cwd + System.IO.Path.DirectorySeparatorChar;

            const string IMAGE_FILE_EXTENSION = ".png";

            filepath += (sidc + IMAGE_FILE_EXTENSION);

            return filepath;
        }
    }
}
