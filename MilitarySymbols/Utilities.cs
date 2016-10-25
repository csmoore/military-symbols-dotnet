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

        public static bool ExportSymbolFromAttributes(Dictionary<string, string> attributeSet, 
            out System.Drawing.Bitmap exportBitmap,
            System.Drawing.Size size)
        {
            SymbolIdCode sidc = null;

            if (attributeSet.ContainsKey("legacysymbolidcode"))
            {
                string legacySymbolIdCode = attributeSet["legacysymbolidcode"];
                bool success = ConvertCodeCharlieToDelta(legacySymbolIdCode, out sidc);
            }
            else
            {
                sidc = new SymbolIdCode(attributeSet);
            }

            return ExportSymbol(sidc, out exportBitmap, size);
        }

        public static bool ExportSymbol(SymbolIdCode code, out System.Drawing.Bitmap exportBitmap,
            System.Drawing.Size size)
        {
            if (!code.IsValid)
            {
                exportBitmap = null;
                System.Diagnostics.Trace.WriteLine("Export Failed! SIDC is not valid:" + code);
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

            SymbolLookup codeLookup = GetSymbolLookup();

            string symbolSetString, entityString, mod1String, mod2String;

            bool success = codeLookup.GetDeltaCodeFromCharlie(code2525CharlieUpper, out symbolSetString,
                out entityString, out mod1String, out mod2String);

            if (!success || string.IsNullOrEmpty(symbolSetString) || string.IsNullOrEmpty(entityString) 
                || entityString.Length != 6)
                return false;

            // TRICKY/WORKAROUND: to ones with "TABLE D-V. Land unit icons–special entity subtypes"
            if (entityString.EndsWith("95") || entityString.EndsWith("96") ||
                entityString.EndsWith("97") || entityString.EndsWith("98"))
            {
                entityString = entityString.Substring(0, 4) + "00";

                // TODO: test/implement this in MilitarySymbol + drawing to see if we can get it to show up
            }

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
            if (!TypeUtilities.IsWeather(code2525Delta.SymbolSet))
            {
                foreach (KeyValuePair<StatusType, char> kvp in TypeUtilities.StatusToCharlieChar)
                {
                    if (kvp.Value == planningChar)
                    {
                        code2525Delta.Status = kvp.Key;
                        break;
                    }
                }
            } // ! IsWeather

            if (code2525Charlie.Length < 12) // allow codes to be 10 or 15, but don't go past here if not > 12
                return true;

            if (TypeUtilities.HasFrame(code2525Delta.SymbolSet))
            {
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
            } // HasFrame

            return true;
        }

        public static bool ConvertCodeDeltaToCharlie(SymbolIdCode code2525Delta,
            out string code2525Charlie)
        {
            // Just monitoring how often this is being called for now:
            System.Diagnostics.Trace.WriteLine("ConvertCodeDeltaToCharlie");

            code2525Charlie = "NOT FOUND";

            SymbolLookup codeLookup = GetSymbolLookup();

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

            bool isWeather = (code2525CharlieFromLookup[0] == 'W');

            char replaceChar1 = charlieAffilationChar;
            if (isWeather) // Don't replace weather with affiliation
                replaceChar1 = code2525CharlieFromLookup[1];

            StringBuilder sbCharlieCode = new StringBuilder();
            sbCharlieCode.Append(code2525CharlieFromLookup[0]);
            sbCharlieCode.Append(replaceChar1);
            sbCharlieCode.Append(code2525CharlieFromLookup[2]);

            if (isWeather)
                sbCharlieCode.Append(code2525CharlieFromLookup[3]);
            else
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
            SymbolLookup milSymbolLookup = GetSymbolLookup();

            List<MilitarySymbol> matchingSymbols = milSymbolLookup.GetMilitarySymbols(symbolSet);

            return matchingSymbols;
        }

        /// <summary>
        /// Returns all symbols found from Charlie Table
        /// </summary>
        public static List<MilitarySymbol> GetMilitarySymbolsFromCharlie()
        {
            SymbolLookup milSymbolLookup = GetSymbolLookup();

            List<MilitarySymbol> matchingSymbols = milSymbolLookup.GetMilitarySymbolsFromCharlie();

            return matchingSymbols;
        }

        /// <summary>
        /// Checks if an entity code exists in the table and therefore is valid
        /// </summary>
        public static bool IsEntityCodeValid(SymbolSetType symbolSet, string entityCode)
        {
            SymbolLookup milSymbolLookup = GetSymbolLookup();

            if (entityCode == "000000") // allow this
                return true;

            if ((symbolSet == SymbolSetType.Unknown) ||
                (symbolSet == SymbolSetType.NotSet) ||
                (string.IsNullOrWhiteSpace(entityCode)) ||
                (entityCode.Length != 6))
                return false;

            // if it can't find a shape for the symbol then (presumably) it doesn't exist
            return (milSymbolLookup.GetEntityShapeFromCode(symbolSet, entityCode) 
                != ShapeType.Unknown);
        }

        /// <summary>
        /// Checks if an entity code exists in the table and therefore is valid
        /// </summary>
        public static bool IsModifierCodeValid(SymbolSetType symbolSet, int modifierNumber, /* 1 or 2*/
            string modifierCode)
        {
            SymbolLookup milSymbolLookup = GetSymbolLookup();

            if ((symbolSet == SymbolSetType.Unknown) ||
                (symbolSet == SymbolSetType.NotSet) ||
                (modifierNumber < 1) ||
                (modifierNumber > 2) ||
                (string.IsNullOrWhiteSpace(modifierCode)) ||
                (modifierCode.Length != 2))
                return false;

            if (modifierCode == "00")
                return true;

            // if it can't find a shape for the symbol then (presumably) it doesn't exist
            return (milSymbolLookup.GetModifierNameFromCode(symbolSet, modifierNumber, modifierCode)
                != string.Empty);
        }

        /// <summary>
        /// Creates an consistent name from a Symbol ID Code
        /// (not in SymbolIdCode because it needs SymbolLookup)
        /// </summary>
        public static string GetWellFormedName(SymbolIdCode id)
        {
            // Just monitoring how often this is being called for now:
            // System.Diagnostics.Trace.WriteLine("GetWellFormedName");

            SymbolLookup milSymbolLookup = GetSymbolLookup();

            StringBuilder sb = new StringBuilder();
            string symbolSetName = TypeUtilities.EnumHelper.getStringFromEnum(id.SymbolSet);
            sb.Append(symbolSetName);

            string entityName = string.Empty;
            string entityTypeName = string.Empty;
            string entitySubTypeName = string.Empty;

            bool found = milSymbolLookup.GetEntityNamesFromCode(id.SymbolSet, id.EntityCode,
                out entityName, out entityTypeName, out entitySubTypeName);

            if (!found)
                return sb.ToString();

            sb.Append(TypeUtilities.NameSeparator);
            sb.Append(entityName);

            if (!string.IsNullOrWhiteSpace(entityTypeName))
            {
                sb.Append(TypeUtilities.NameSeparator);
                sb.Append(entityTypeName);
            }

            if (!string.IsNullOrWhiteSpace(entitySubTypeName))
            {
                sb.Append(TypeUtilities.NameSeparator);
                sb.Append(entitySubTypeName);
            }

            if (id.IsLandUnitSpecialEntity)
            {
                sb.Append(TypeUtilities.NameSeparator);
                sb.Append(TypeUtilities.EntitySubtypeCodeToLandUnitSpecialEntityName[id.EntitySubTypeField]);
            }

            string modifier1Name = 
                milSymbolLookup.GetModifierNameFromCode(id.SymbolSet, 1, id.ModifierOne);
            string modifier2Name = 
                milSymbolLookup.GetModifierNameFromCode(id.SymbolSet, 2, id.ModifierTwo);

            if (!string.IsNullOrWhiteSpace(modifier1Name))
            {
                sb.Append(TypeUtilities.NameSeparator);
                sb.Append("M1-");
                sb.Append(modifier1Name);
            }

            if (!string.IsNullOrWhiteSpace(modifier2Name))
            {
                sb.Append(TypeUtilities.NameSeparator);
                sb.Append("M2-");
                sb.Append(modifier2Name);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Find shape for Symbol ID
        /// </summary>
        public static ShapeType GetShapeForId(SymbolIdCode id)
        {
            if ((id == null) || (!id.IsValid))
                return ShapeType.Unknown;

            SymbolLookup milSymbolLookup = GetSymbolLookup();

            return milSymbolLookup.GetEntityShapeFromCode(id.SymbolSet, id.EntityCode);
        }
 
        /// <summary>
        /// Find Main IconType for Symbol ID
        /// </summary>
        public static CentralIconType GetCentralIconForId(SymbolIdCode id)
        {
            if ((id == null) || (!id.IsValid))
                return CentralIconType.NotSet;

            SymbolLookup milSymbolLookup = GetSymbolLookup();

            return milSymbolLookup.GetEntityIconTypeFromCode(id.SymbolSet, id.EntityCode);
        }
        
       
        public static SymbolLookup GetSymbolLookup()
        {
            if (symbolLookup == null)
                symbolLookup = new SymbolLookup();

            return symbolLookup;
        }
        private static SymbolLookup symbolLookup = null;

    }
}
