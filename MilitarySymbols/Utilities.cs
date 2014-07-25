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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilitarySymbols
{
    public class Utilities
    {
        public static bool SetImageFilesHome(string pathToImages)
        {
            MilitarySymbolToGraphicLayersMaker.ImageFilesHome = pathToImages;

            return CheckImageFilesHomeExists();
        }

        public static bool CheckImageFilesHomeExists()
        {
            string pathToImages = MilitarySymbolToGraphicLayersMaker.ImageFilesHome;

            if (!System.IO.Directory.Exists(pathToImages))
            {
                System.Diagnostics.Trace.WriteLine("Image folder does not exist: " + pathToImages);
                return false;
            }

            // Check for known file
            string checkFile = "license.txt";
            string checkFullPathToFile = System.IO.Path.Combine(pathToImages, checkFile);

            if (!System.IO.File.Exists(checkFullPathToFile))
            {
                System.Diagnostics.Trace.WriteLine("Image folder not of expected format: " + pathToImages);
                return false;
            }

            return true;
        }

        public static bool ExportSymbol(SymbolIdCode code, out System.Drawing.Bitmap exportBitmap,
            System.Drawing.Size size)
        {
            if (!code.IsValid)
            {
                exportBitmap = null;
                return false;
            }

            SvgSymbol svgSymbol = new SvgSymbol();
            SvgSymbol.ImageSize = size;

            MilitarySymbol milSymbol = new MilitarySymbol();
            milSymbol.Id = code;

            MilitarySymbolToGraphicLayersMaker.SetMilitarySymbolGraphicLayers(ref milSymbol);

            exportBitmap = SvgSymbol.GetBitmap(milSymbol.GraphicLayers);

            return (exportBitmap != null); 
        }

        public static bool ConvertCodeCharlieToDelta(string code2525Charlie,
            out SymbolIdCode code2525Delta)
        {
            code2525Delta = SymbolIdCode.DefaultSymbolIdCode;

            if (code2525Charlie.Length < 10)
                return false;

            string code2525CharlieUpper = code2525Charlie.ToUpper();

            SymbolLookup codeLookup = getSymbolLookup();

            string symbolSetString, entityString, mod1String, mod2String;

            bool success = codeLookup.GetDeltaCodeFromCharlie(code2525CharlieUpper, out symbolSetString,
                out entityString, out mod1String, out mod2String);

            if (!success || string.IsNullOrEmpty(symbolSetString) || string.IsNullOrEmpty(entityString))
                return false;

            code2525Delta.SymbolSet = (SymbolSetType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetString);
            code2525Delta.EntityCode = entityString;
            if (!string.IsNullOrEmpty(mod1String))
                code2525Delta.ModifierOne = mod1String;
            if (!string.IsNullOrEmpty(mod2String))
                code2525Delta.ModifierTwo = mod2String;

            char affilChar = code2525CharlieUpper[1];
            foreach (KeyValuePair<StandardIdentityAffiliationType, char> kvp in TypeUtilities.AffiliationToCharlieChar)
            {
                if (kvp.Value == affilChar)
                {
                    code2525Delta.Affiliation = kvp.Key;
                    break;
                }
            }

            char planningChar = code2525CharlieUpper[3];
            foreach (KeyValuePair<StatusType, char> kvp in TypeUtilities.StatusToCharlieChar)
            {
                if (kvp.Value == planningChar)
                {
                    code2525Delta.Status = kvp.Key;
                    break;
                }
            }

            if (code2525Charlie.Length < 12) // allow codes to be 10 or 15, but don't go past here if not > 12
                return true;

            char hqFdTfChar = code2525CharlieUpper[10];
            foreach (KeyValuePair<HeadquartersTaskForceDummyType, char> kvp in TypeUtilities.HqTfFdToCharlieChar)
            {
                if (kvp.Value == hqFdTfChar)
                {
                    code2525Delta.HeadquartersTaskForceDummy = kvp.Key;
                    break;
                }
            }

            char echelonChar = code2525CharlieUpper[11];
            foreach (KeyValuePair<EchelonMobilityType, char> kvp in TypeUtilities.EchelonMobilityToCharlieChar)
            {
                if (kvp.Value == echelonChar)
                {
                    code2525Delta.EchelonMobility = kvp.Key;
                    break;
                }
            }

            return true;
        }

        public static bool ConvertCodeDeltaToCharlie(SymbolIdCode code2525Delta,
            out string code2525Charlie)
        {
            code2525Charlie = "NOT FOUND";

            SymbolLookup codeLookup = getSymbolLookup();

            string symbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(
                code2525Delta.SymbolSet, 2);
            string entityString = code2525Delta.EntityCode;
            string mod1String = code2525Delta.ModifierOne;
            string mod2String = code2525Delta.ModifierTwo;

            string code2525CharlieFromLookup;

            bool success = codeLookup.GetCharlieCodeFromDelta(symbolSetString, 
                entityString, mod1String, mod2String, out code2525CharlieFromLookup);

            if (!success || string.IsNullOrEmpty(code2525CharlieFromLookup)
                || code2525CharlieFromLookup.Length < 15)
                return false;

            char charlieAffilationChar = TypeUtilities.AffiliationToCharlieChar[code2525Delta.Affiliation];

            // TODO: get/set planning, echelon, other
            
            char replaceChar1 = charlieAffilationChar;
            if (code2525CharlieFromLookup[0] == 'W') // Don't replace weather with affiliation
                replaceChar1 = code2525CharlieFromLookup[1];

            StringBuilder sbCharlieCode = new StringBuilder();
            sbCharlieCode.Append(code2525CharlieFromLookup[0]);
            sbCharlieCode.Append(replaceChar1);
            sbCharlieCode.Append(code2525CharlieFromLookup[2]); 
            sbCharlieCode.Append(TypeUtilities.StatusToCharlieChar[code2525Delta.Status]); 
            sbCharlieCode.Append(code2525CharlieFromLookup.Substring(4, 6));
            sbCharlieCode.Append(TypeUtilities.HqTfFdToCharlieChar[code2525Delta.HeadquartersTaskForceDummy]);
            sbCharlieCode.Append(TypeUtilities.EchelonMobilityToCharlieChar[code2525Delta.EchelonMobility]); 
            sbCharlieCode.Append("---"); 

            code2525Charlie = sbCharlieCode.ToString();

            return true;
        }

        /// <summary>
        /// Returns all symbols within a symbol set (affiliation will be set to Unknown)
        /// </summary>
        /// <param name="symbolSet">(symbolSet == NotSet) returns all symbols</param>
        /// <returns></returns>
        public static List<MilitarySymbol> GetMilitarySymbols(SymbolSetType symbolSet = SymbolSetType.NotSet)
        {
            SymbolLookup milSymbolLookup = getSymbolLookup();

            List<MilitarySymbol> matchingSymbols = milSymbolLookup.GetMilitarySymbols(symbolSet);

            return matchingSymbols;
        }

        /// <summary>
        /// Creates an easier to read code, eliminates unset attributes
        /// </summary>
        public static string GetHumanReadableCode(SymbolIdCode id, bool showAffiliation = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(id.SymbolSet, 2));
            sb.Append("_");
            sb.Append(id.EntityCode);

            if (showAffiliation)
            {
                sb.Append("_");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(id.Affiliation));
            }

            if ((id.ModifierOne != "00") || (id.ModifierTwo != "00"))
            {
                sb.Append("_M1-");
                sb.Append(id.ModifierOne);
                sb.Append("_M2-");
                sb.Append(id.ModifierTwo);
            }

            if (id.Status != StatusType.Present)
            {
                sb.Append("_S-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(id.Status));
            }

            if (id.HeadquartersTaskForceDummy != HeadquartersTaskForceDummyType.NoHQTFDummyModifier)
            {
                sb.Append("_HQ-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(id.HeadquartersTaskForceDummy));
            }

            if (id.EchelonMobility != EchelonMobilityType.NoEchelonMobility)
            {
                sb.Append("_EM-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(id.EchelonMobility));
            }

            return sb.ToString();
        }

       
        public static SymbolLookup getSymbolLookup()
        {
            if (symbolLookup == null)
                symbolLookup = new SymbolLookup();

            return symbolLookup;
        }
        private static SymbolLookup symbolLookup = null;

    }
}
