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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MilitarySymbols;

namespace CodePermutations
{
    /// <summary>
    /// This utility is mainly intended to be used to create batch scripts for testing 
    /// all combinations of a symbol
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string sidc = "10121100";

            if (args.Length < 1)
            {
                Usage();
            }
            else
            {
                sidc = args[0];
            }

            if (sidc.StartsWith("ALL"))
                ProcessAll();
            else 
                if ((sidc.Length == 20) || (sidc.Length == 8)) // (full or shortened) code supplied
                    ProcessSidc(sidc);
                else
                    Usage();
        }

        static void Usage()
        {
            Console.WriteLine("CodePermutations [SymbolIDCode]");
        }

        static string delimiter     = " "; //  ",";
        static string baseString    = " "; // Ex: "ExportBitmap.exe"
        static string commentString = "REM";

        static void PrintCommentLine(string comment)
        {
            // just prints a formatted row with the comment and the same number of delimiters as PrintCodeLine
            if (!string.IsNullOrWhiteSpace(baseString))
                Console.Write(commentString + delimiter);

            Console.Write(comment);

            Console.WriteLine(delimiter + delimiter + delimiter);
        }

        static void PrintCodeLine(SymbolIdCode sidc, string optionalTag = "")
        {
            string simpleCode = sidc.HumanReadableCode(false);

            if (!string.IsNullOrWhiteSpace(baseString))
                Console.Write(baseString + delimiter);

            Console.Write(sidc.Code + delimiter +
                          simpleCode + delimiter + sidc.Name + delimiter);

            if (!string.IsNullOrWhiteSpace(optionalTag))
                Console.WriteLine(optionalTag);
            else
                Console.WriteLine();
        }

        static void ProcessSidc(string sidcString)
        {
            // base code
            SymbolIdCode sidc = new SymbolIdCode(sidcString);

            if (!sidc.IsValid)
            {
                Console.WriteLine("Could not create symbol from SIDC: " + sidcString);
                return;
            }

            if (TypeUtilities.IsWeather(sidc.SymbolSet))
            {
                // create a one version & then return, the rest don't apply
                PrintCommentLine("Weather Symbol Set: " + sidc.SymbolSet);
                PrintCodeLine(sidc, "Weather - only one version of symbol");
                return;
            }

            bool hasFrame = TypeUtilities.HasFrame(sidc.SymbolSet);

            PrintCommentLine("Affiliation/Planning Combinations");

            List<Enum> enums = TypeUtilities.EnumHelper.getEnumValues(typeof(StandardIdentityAffiliationType));
            foreach (Enum en in enums)
            {
                sidc.Affiliation = (StandardIdentityAffiliationType)en;
                string tag = TypeUtilities.EnumHelper.getStringFromEnum(en);
                if ((tag != "NotSet") && (tag != "Dont Display Frame"))
                {
                    PrintCodeLine(sidc, tag);

                    // create a planning version 
                    sidc.Status = StatusType.Planned_Anticipated_Suspect;
                    PrintCodeLine(sidc, "Planning");
                    sidc.Status = StatusType.Present; // Reset

                    // if it has a frame also do the Real/Exercise/Sim combos
                    if ((hasFrame) && 
                        ((sidc.Affiliation == StandardIdentityAffiliationType.Friend) ||
                         (sidc.Affiliation == StandardIdentityAffiliationType.Hostile) ||
                         (sidc.Affiliation == StandardIdentityAffiliationType.Suspect_Joker) ||
                         (sidc.Affiliation == StandardIdentityAffiliationType.Neutral) ||
                         (sidc.Affiliation == StandardIdentityAffiliationType.Unknown)))
                    {
                        PrintCommentLine("StandardIdentity Combinations for affiliation: " + en);

                        List<Enum> enums2 = TypeUtilities.EnumHelper.getEnumValues(typeof(StandardIdentityRealExerciseSimType));
                        foreach (Enum en2 in enums2)
                        {
                            sidc.StandardIdentity = (StandardIdentityRealExerciseSimType)en2;

                            string tag2 = TypeUtilities.EnumHelper.getStringFromEnum(en2);
                            if (tag2 != "NotSet")
                                PrintCodeLine(sidc, tag2);
                        }

                        // Reset
                        sidc.StandardIdentity = StandardIdentityRealExerciseSimType.Reality;
                    }

                }
            }

            // Reset
            sidc.Affiliation = StandardIdentityAffiliationType.Friend;

            // Does it have a frame? If not don't need to do the rest
            if (!hasFrame)
            {
                PrintCommentLine("Non-framed Symbol Set: " + sidc.SymbolSet + " - done");

                return;
            }

            if (TypeUtilities.HasFrameModifiers(sidc.SymbolSet))
            {
                PrintCommentLine("Headquarters/Task Force/Feint/Dummy Combinations");

                enums = TypeUtilities.EnumHelper.getEnumValues(typeof(HeadquartersTaskForceDummyType));
                foreach (Enum en in enums)
                {
                    sidc.HeadquartersTaskForceDummy = (HeadquartersTaskForceDummyType)en;
                    string tag = TypeUtilities.EnumHelper.getStringFromEnum(en);
                    PrintCodeLine(sidc, tag);
                }

                // Reset
                sidc.HeadquartersTaskForceDummy = HeadquartersTaskForceDummyType.NoHQTFDummyModifier;

                PrintCommentLine("Echelon/Mobility Combinations");

                enums = TypeUtilities.EnumHelper.getEnumValues(typeof(EchelonMobilityType));
                foreach (Enum en in enums)
                {
                    sidc.EchelonMobility = (EchelonMobilityType)en;
                    string tag = TypeUtilities.EnumHelper.getStringFromEnum(en);
                    PrintCodeLine(sidc, tag);
                }

                // Reset
                sidc.EchelonMobility = EchelonMobilityType.NoEchelonMobility;
            }

            SymbolLookup sl = Utilities.GetSymbolLookup();
            CentralIconType iconType = sl.GetEntityIconTypeFromCode(sidc.SymbolSet, sidc.EntityCode);

            if (iconType == CentralIconType.FULL_OCTAGON)
            {
                // Then we are done, no need for modifiers
                return;
            }

            if (iconType != CentralIconType.MAIN_1) // can't have modifier 1
            {
                PrintCommentLine("Modifier 1 Code Combinations");

                // Add all the modifiers
                List<string> modifierCode1s = sl.GetModifierCodesForSymbolSet(sidc.SymbolSet, 1);
                foreach (string mod1 in modifierCode1s)
                {
                    sidc.ModifierOne = mod1;
                    string tag = sl.GetModifierNameFromCode(sidc.SymbolSet, 1, mod1);
                    PrintCodeLine(sidc, tag);
                }
            }

            // Reset
            sidc.ModifierOne = "00";

            if (iconType != CentralIconType.MAIN_2) // can't have modifier 1
            {
                PrintCommentLine("Modifier 2 Code Combinations");

                List<string> modifierCode2s = sl.GetModifierCodesForSymbolSet(sidc.SymbolSet, 2);
                foreach (string mod2 in modifierCode2s)
                {
                    sidc.ModifierTwo = mod2;
                    string tag = sl.GetModifierNameFromCode(sidc.SymbolSet, 2, mod2);
                    PrintCodeLine(sidc, tag);
                }
            }

            // Reset
            sidc.ModifierTwo = "00";

            // Add those "Special Land Unit Entity Subtype combos in Land Unit
            if (sidc.SymbolSet == SymbolSetType.Land_Unit)
            {
                List<string> specialSubTypes = new List<string>() { "95", "96", "97", "98" };

                foreach (string subType in specialSubTypes)
                {
                    sidc.EntitySubTypeField = subType;
                    PrintCodeLine(sidc, subType);
                }
            }

        }

        static void ProcessAll()
        {
            SymbolLookup sl = Utilities.GetSymbolLookup();

            // Foreach symbol set pick a random entity and generate a symbol with permutations
            SymbolSetType symbolSet = SymbolSetType.NotSet;

            Random random = new Random();

            // find the symbol set selected
            List<Enum> symbolSetEnums = TypeUtilities.EnumHelper.getEnumValues(typeof(SymbolSetType));
            foreach (Enum en in symbolSetEnums)
            {
                symbolSet = (SymbolSetType)en;

                if (symbolSet == SymbolSetType.NotSet)
                    continue;

                List<MilitarySymbol> allSymbolsFromSet = Utilities.GetMilitarySymbols(symbolSet);

                int setCount = allSymbolsFromSet.Count;

                if (setCount <= 0)
                {
                    System.Diagnostics.Trace.WriteLine("No Symbols found in SymbolSet: " + symbolSet);
                    continue;
                }

                int randomOne = random.Next(0, setCount - 1);

                MilitarySymbol symbol = allSymbolsFromSet[randomOne];

                ProcessSidc(symbol.Id.Code);
            }

        }

    }

}
