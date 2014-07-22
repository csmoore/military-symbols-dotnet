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
using System.Text;
using System.Threading.Tasks;

using MilitarySymbols;

namespace ConvertCode
{
    class Program
    {
        static void Main(string[] args)
        {
            // Sample Test Case/Mapping
            // SFGAUCIL--AA---, 10031012111211000019, ( (10, 03, 10, 1, 2, 11), (121100, 00, 19) )
            string sidc = "10031012111211000019";       
            // string sidc = "SFGPUCIL--AA---";

            if (args.Length < 1)
            {
                Usage();
            }
            else
            {
                sidc = args[0];
            }

            if ((sidc.Length == 20) || (sidc.Length == 8)) // Delta Code (full or shortened) Supplied
                ConvertDeltaToCharlie(sidc);
            else if ((sidc.Length == 15) || (sidc.Length == 10)) // Charlie Code (full or shortened) Supplied
                ConvertCharlieToDelta(sidc);
            else
                Usage();
        }

        static void Usage()
        {
            Console.WriteLine("ConvertCodes [SymbolIDCode]");
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

    }
}
