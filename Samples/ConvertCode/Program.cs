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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MilitarySymbols;

namespace ConvertCode
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
        /// if Length == 20 | Length == 8 ->  Delta Code (full or shortened) Supplied - convert to Charlie
        /// if Length == 15 | Length == 10 -> Charlie Code (full or shortened) Supplied - convert to Delta
        /// if Length == 2 -> Convert all  Delta Codes from that Symbol Set Code
        /// 
        /// For CSV File:
        /// First Column (Column 0) has the SIDC
        /// 
        /// </summary>
        static void Main(string[] args)
        {
            // Sample Test Case/Mapping
            // SFGAUCIL--AA---, 10031012111211000019, ( (10, 03, 10, 1, 2, 11), (121100, 00, 19) )
            string arg = "1003101211" + "1211000019";       
            // string arg = "SFGPUCIL--AA---";

            if (args.Length < 1)
            {
                Usage();
            }
            else
            {
                arg = args[0];
            }

            if (arg.StartsWith("ALL2525C"))  // export all known 2525C symbols
                ProcessAll2525C(); 
            else if ((arg.StartsWith("ALL") || arg.Length == 2)) // export all known symbols
                ProcessAll(arg);
            else if (arg.EndsWith("csv")) // csv file supplied
                ProcessCsv(arg);
            else
                ProcessSidc(arg);
        }

        static void Usage()
        {
            Console.WriteLine("ConvertCodes [SymbolIDCode | {Filename}.csv | ALL | {SymbolSet Code}]");
        }

        static void ProcessSidc(string sidc)
        {
            if ((sidc.Length == 20) || (sidc.Length == 8)) // Delta Code (full or shortened) Supplied
                ConvertDeltaToCharlie(sidc);
            else if ((sidc.Length == 15) || (sidc.Length == 10)) // Charlie Code (full or shortened) Supplied
                ConvertCharlieToDelta(sidc);
            else
                Console.WriteLine("NOT FOUND," + sidc + ",Failed to recognize SIDC Length = " + sidc.Length);
        }

        static void printConversionLine(string code2525Charlie, SymbolIdCode sidc, bool success = true, bool newline = true)
        {
            if ((success) || (code2525Charlie == "NOT FOUND"))
            {
                string simpleCode = sidc.HumanReadableCode(false);

                Console.Write(code2525Charlie + "," + simpleCode + "," + sidc.Name + "," + 
                    sidc.CodeFirstTen + ":" + sidc.CodeSecondTen + ",\"" + sidc.ToString() + "\"");
            }
            else
            {
                Console.Write(code2525Charlie + ",NOT FOUND,NOT FOUND,NOT FOUND,NOT FOUND");
            }

            if (newline)
                Console.WriteLine();
        }

        static void printConversionLine(string codeLegacy2525C, MilitarySymbol symbol2525D, bool success = true)
        {

            printConversionLine(codeLegacy2525C, symbol2525D.Id, success, false);

            if (success)
            {
                Console.WriteLine(",\"" + symbol2525D.TagsAsString + "\"");
            }
            else
            {
                Console.WriteLine(",NOT FOUND");
            }
        }

        static void ConvertDeltaToCharlie(string sidcString, bool newline = true)
        {
            SymbolIdCode sidc = new SymbolIdCode();

            if (sidcString.Length == 20)
            {
                sidc.Code = sidcString;
            }
            else if (sidcString.Length == 8)
            {
                string symbolSetString = sidcString.Substring(0, 2);

                sidc.SymbolSet = (SymbolSetType)
                    TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetString);
                sidc.EntityCode = sidcString.Substring(2, 6);
            }

            string code2525Charlie;
            bool success = Utilities.ConvertCodeDeltaToCharlie(sidc, out code2525Charlie);

            string simpleCode = sidc.HumanReadableCode();

            printConversionLine(code2525Charlie, sidc, success, newline);
        }

        static void ConvertCharlieToDelta(string code2525Charlie)
        {
            SymbolIdCode sidc = new SymbolIdCode();

            bool success = Utilities.ConvertCodeCharlieToDelta(code2525Charlie, out sidc);

            string simpleCode = sidc.HumanReadableCode();

            printConversionLine(code2525Charlie, sidc, success);
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
                if (line.StartsWith("#") || (string.IsNullOrWhiteSpace(line))) // allow "#" comment character
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

                if (string.IsNullOrWhiteSpace(sidc))
                    continue;

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
                ConvertDeltaToCharlie(matchSymbol.Id.Code, false);

                Console.WriteLine(",\"" + matchSymbol.TagsAsString + "\"");
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
                printConversionLine(matchSymbol.Legacy2525Code, matchSymbol);
            }
        }

    }
}
