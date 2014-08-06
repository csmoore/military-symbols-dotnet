using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MilitarySymbols;

namespace CodePermutations
{
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

        static void PrintCodeLine(SymbolIdCode sidc, string optionalTag = "")
        {
            string baseString = ""; // Ex: "ExportBitmap.exe"
            string delimiter = ","; //  " ";
            string simpleCode = sidc.HumanReadableCode(false);

            if (!string.IsNullOrWhiteSpace(baseString))
                Console.Write(baseString + delimiter);

            Console.Write(sidc.Code + delimiter + 
                          simpleCode + delimiter + sidc.Name);

            if (!string.IsNullOrWhiteSpace(optionalTag))
                Console.WriteLine(delimiter + optionalTag);
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

            List<Enum> enums = TypeUtilities.EnumHelper.getEnumValues(typeof(StandardIdentityAffiliationType));
            foreach (Enum en in enums)
            {
                sidc.Affiliation = (StandardIdentityAffiliationType)en;
                string tag = TypeUtilities.EnumHelper.getStringFromEnum(en);
                if ((tag != "NotSet") && (tag != "Dont Display Frame"))
                    PrintCodeLine(sidc, tag);
            }

            // Reset
            sidc.Affiliation = StandardIdentityAffiliationType.Friend;

            // Does it have a frame? If not don't need to do the rest
            if (!TypeUtilities.HasFrame(sidc.SymbolSet))
            {
                if (!TypeUtilities.IsWeather(sidc.SymbolSet))
                {
                    // create a planning version & then return, the rest don't apply
                    sidc.Status = StatusType.Planned_Anticipated_Suspect;
                    PrintCodeLine(sidc, "Planning");
                }

                return;
            }

            enums = TypeUtilities.EnumHelper.getEnumValues(typeof(StandardIdentityRealExerciseSimType));
            foreach (Enum en in enums)
            {
                sidc.StandardIdentity = (StandardIdentityRealExerciseSimType)en;

                string tag = TypeUtilities.EnumHelper.getStringFromEnum(en);
                if (tag != "NotSet")
                    PrintCodeLine(sidc, tag);
            }

            // Reset
            sidc.StandardIdentity = StandardIdentityRealExerciseSimType.Reality;

            enums = TypeUtilities.EnumHelper.getEnumValues(typeof(HeadquartersTaskForceDummyType));
            foreach (Enum en in enums)
            {
                sidc.HeadquartersTaskForceDummy = (HeadquartersTaskForceDummyType)en;
                string tag = TypeUtilities.EnumHelper.getStringFromEnum(en);
                PrintCodeLine(sidc, tag);
            }

            // Reset
            sidc.HeadquartersTaskForceDummy = HeadquartersTaskForceDummyType.NoHQTFDummyModifier;

            enums = TypeUtilities.EnumHelper.getEnumValues(typeof(EchelonMobilityType));
            foreach (Enum en in enums)
            {
                sidc.EchelonMobility = (EchelonMobilityType)en;
                string tag = TypeUtilities.EnumHelper.getStringFromEnum(en);
                PrintCodeLine(sidc, tag);
            }

            // Reset
            sidc.EchelonMobility = EchelonMobilityType.NoEchelonMobility;

            SymbolLookup sl = Utilities.GetSymbolLookup();
            CentralIconType iconType = sl.GetEntityIconTypeFromCode(sidc.SymbolSet, sidc.EntityCode);

            if (iconType == CentralIconType.FULL_OCTAGON)
            {
                // Then we are done, no need for modifiers
                return;
            }

            if (iconType != CentralIconType.MAIN_1) // can't have modifier 1
            {
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
        }

        static void ProcessAll()
        {
            SymbolLookup sl = Utilities.GetSymbolLookup();

            // Foreach symbol set pick a random entity and generate a symbol with permutations
            SymbolSetType symbolSet = SymbolSetType.NotSet;

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
                    continue;

                Random random = new Random();
                int randomOne = random.Next(0, setCount - 1);

                MilitarySymbol symbol = allSymbolsFromSet[randomOne];

                ProcessSidc(symbol.Id.Code);
            }


        }

    }

}
