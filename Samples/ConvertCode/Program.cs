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
        /// Takes either a Symbol ID Code (SIDC) or a CSV filename as a command line argument
        /// 
        /// Makes these assumptions:
        /// For SIDC:
        /// if Length == 20 | Length == 8 ->  Delta Code (full or shortened) Supplied - convert to Charlie
        /// if Length == 15 | Length == 10 -> Charlie Code (full or shortened) Supplied - convert to Delta
        /// 
        /// For CSV File:
        /// First Column (Column 0) has the SIDC
        /// 
        /// </summary>
        static void Main(string[] args)
        {
            // Sample Test Case/Mapping
            // SFGAUCIL--AA---, 10031012111211000019, ( (10, 03, 10, 1, 2, 11), (121100, 00, 19) )
            string arg = "10031012111211000019";       
            // string arg = "SFGPUCIL--AA---";

            if (args.Length < 1)
            {
                Usage();
            }
            else
            {
                arg = args[0];
            }

            if (arg.Contains("csv")) // csv file supplied
                ProcessCsv(arg);
            else
                ProcessSidc(arg);
        }

        static void Usage()
        {
            Console.WriteLine("ConvertCodes [SymbolIDCode]");
        }

        static void ProcessSidc(string sidc)
        {
            if ((sidc.Length == 20) || (sidc.Length == 8)) // Delta Code (full or shortened) Supplied
                ConvertDeltaToCharlie(sidc);
            else if ((sidc.Length == 15) || (sidc.Length == 10)) // Charlie Code (full or shortened) Supplied
                ConvertCharlieToDelta(sidc);
            else
                Console.WriteLine("Failed to recognize SIDC: " + sidc + ", Length = " + sidc.Length);
        }

        static void ConvertDeltaToCharlie(string sidcString)
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
                sidc.FullEntityCode = sidcString.Substring(2, 6);
            }

            string code2525Charlie;
            bool success = Utilities.ConvertCodeDeltaToCharlie(sidc, out code2525Charlie);

            Console.WriteLine(sidcString + "," + code2525Charlie);
        }

        static void ConvertCharlieToDelta(string code2525Charlie)
        {
            SymbolIdCode sidc = new SymbolIdCode();

            bool success = Utilities.ConvertCodeCharlieToDelta(code2525Charlie, out sidc);

            string symbolSetString = TypeUtilities.EnumHelper.getEnumValAsString(sidc.SymbolSet, 2);
            string simpleCode = symbolSetString + sidc.FullEntityCode;

            if (success)
                Console.WriteLine(code2525Charlie + "," + simpleCode + ","
                    + sidc.CodeFirstTen + ":" + sidc.CodeSecondTen + ",\"" + sidc.ToString() + "\"");
            else
                Console.WriteLine(code2525Charlie + ",NOT FOUND,NOT FOUND,NOT FOUND");
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

    }
}
