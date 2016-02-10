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
using System.IO;
using System.Drawing;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MilitarySymbols;

namespace TestMilitaryAttributesToBitmap
{
    class Program
    {
        static void Main(string[] args)
        { 
            ExportAttributesToBitmapTest();
        }

        static void ExportAttributesToBitmapTest()
        {
            // Step 1: Create a dictionary/map of well known attribute names to values
            Dictionary<string, string> attributeSet = new Dictionary<string, string>();

            attributeSet["context"]      = "1";
            attributeSet["identity"]     = "3";
            attributeSet["symbolset"]    = "10";
            attributeSet["symbolentity"] = "121105";
            attributeSet["modifier1"]    = "01";
            attributeSet["modifier2"]    = "01";
            attributeSet["echelon"]      = "11";
            attributeSet["indicator"]    = "7";
            attributeSet["operationalcondition"] = "1";

            // Step 2: Set the SVG Home Folder
            // This should be within the git clone of joint-military-symbology-xml 
            // ex: C:\Github\joint-military-symbology-xml\svg\MIL_STD_2525D_Symbols

            // This is called in CheckSettings below, but you should call yourself if
            // reusing this method 
            // Utilities.SetImageFilesHome(svgHomeFolderSetting)
            // Utilities.CheckImageFilesHomeExists()
            if (!CheckSettings())
            {
                Console.WriteLine("No SVG Folder, can't continue.");
                return;
            }

            // Step 3: Get the Layered Bitmap from the Library
            const int width = 256, height = 256;
            Size exportSize = new Size(width, height);

            System.Drawing.Bitmap exportBitmap;

            bool success = Utilities.ExportSymbolFromAttributes(attributeSet, out exportBitmap, exportSize);

            if (!success || (exportBitmap == null))
            {
                Console.WriteLine("Export failed!");
                return;
            }

            // Step 4: Use the Bitmap somewhere (saved to a file here for viewing)
            string imageFileName = "TestExport.png";

            exportBitmap.Save(imageFileName);
        }

        static bool CheckSettings()
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // IMPORTANT/TODO: For this to work:
            // 1. Clone Repo: Esri\joint-military-symbology-xml 
            // 2. Set Key "SVGImagesHome" in app.config 
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

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

            return true;
        }
    }
}
