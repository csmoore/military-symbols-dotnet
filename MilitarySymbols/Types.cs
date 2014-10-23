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

namespace MilitarySymbols
{
    ///////////////////////////////////////////////////////////
    // 2525D Types and Helpers 
    ///////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////
    // 2525D: A.5.2.1  Set A - First ten digits 
    // Version (Digits 1 and 2) 
    // Standard identity 1, Standard identity 2(AffiliationType) (Digits 3 and 4)
    // Symbol set (Digits 5 and 6)
    // Status (Digit 7)
    // HQ/Task Force/Dummy (Digit 8)
    // Amplifier/Descriptor (Digits 9 and 10)

    public enum StandardVersionType // Digits (1 & 2)
    {        
        NotSet       = 0,
        Current2525D = 10
    }

    public enum StandardIdentityRealExerciseSimType  // StandardIdentity 1 (Digit 3)
    {
        Reality = 0,
        Exercise = 1,
        Simulation = 2,
        NotSet = 9
    }

    public enum StandardIdentityAffiliationType       // StandardIdentity 2 (Digit 4)
    {
        Pending = 0,
        Unknown = 1,
        Assumed_Friend = 2,
        Friend = 3,
        Neutral = 4,
        Suspect_Joker = 5,
        Hostile = 6,       // + Faker
        Dont_Display_Frame = 8,
        NotSet = 9
    }

    public enum SymbolSetType // (Digits 5 & 6)
    {
        Unknown = 00, 
        Air = 01,
        Air_Missile = 02,
        Space = 05,
        Space_Missile = 06,
        Land_Unit = 10,
        Land_Civilian_Unit_Organization = 11,
        Land_Equipment = 15,
        Land_Installation = 20,
        Control_Measures = 25,
        Sea_Surface = 30,
        Sea_Subsurface = 35,
        Mine_Warfare = 36,
        Activities = 40,
        Atmospheric = 45,
        Oceanographic = 46,
        Meteorological_Space = 47,
        Signals_Intelligence_Space = 50,
        Signals_Intelligence_Air = 51,
        Signals_Intelligence_Land = 52,
        Signals_Intelligence_Surface = 53,
        Signals_Intelligence_Subsurface = 54,
        Cyberspace = 60,
        Special_Cases = 98,       
        NotSet = 99
    }

    public enum StatusType // (Digit 7)
    {
        Present = 0,
        Planned_Anticipated_Suspect = 1,
        Present_Fully_Capable = 2,
        Present_Damaged = 3,
        Present_Destroyed = 4,
        Present_Full_To_Capacity = 5,
        NotSet = 9
    }

    public enum HeadquartersTaskForceDummyType // (Digit 8)
    {
        NoHQTFDummyModifier = 0,
        Feint_Dummy = 1,
        Headquarters = 2,
        Feint_Dummy_Headquarters = 3,
        Task_Force = 4,
        Feint_Dummy_Task_Force = 5,
        Task_Force_Headquarters = 6,
        Feint_Dummy_Task_Force_Headquarters = 7
    }

    public enum EchelonMobilityGroupType // Amplifier 1 (Digit 9)
    {
        NoEchelonGroup = 0,
        Echelon_At_Brigade_And_Below = 1,
        Echelon_At_Division_And_Above = 2,
        Equipment_Mobility_On_Land = 3,
        Equipment_Mobility_On_Snow = 4,
        Equipment_Mobility_On_Water = 5,
        Naval_Towed_Array = 6
    }

    public enum EchelonMobilityType     // Amplifier 1, 2 (Digit 9 & 10)
    {
        NoEchelonMobility = 0,
        // Echelon_At_Brigade_And_Below = 10,
        Team_Crew = 11,
        Squad = 12,
        Section = 13,
        Platoon_Detachment = 14,
        Company_Battery_Troop = 15,
        Battalion_Squadron = 16,
        Regiment_Group = 17,
        Brigade = 18,

        // Echelon_At_Division_And_Above = 20,
        Division = 21,
        Corps_MEF = 22,
        Army = 23,
        Army_Group_Front = 24,
        Region_Theater = 25,
        Command = 26,

        // Equipment_Mobility_On_Land = 30,
        Wheeled_Limited_Cross_Country = 31,
        Wheeled_Cross_Country = 32,
        Tracked = 33,
        Wheeled_And_Tracked_Combination = 34,
        Towed = 35,
        Rail = 36,
        Pack_Animals = 37,

        // Equipment_Mobility_On_Snow = 40,
        Over_Snow_Prime_Mover = 41,
        Sled = 42,

        // Equipment_Mobility_On_Water = 50,
        Barge = 51,
        Amphibious = 52,

        // Naval_Towed_Array = 60,
        Short_Towed_Array = 61,
        Long_Towed_Array = 62,
    }
    ///////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////
    // 2525D: A.5.2.2  Set B - Second ten digits
    // Entity (Digits 11 and 12)
    // Entity type (Digits 13 and 14)
    // Entity subtype (Digits 15 and 16)
    // Sector 1 modifier (Digits 17 and 18)
    // Sector 2 modifier (Digits 19 and 20)
    // NOTE: These are just digits based on symbol set so no consistent types used for these
    ///////////////////////////////////////////////////////////

    public enum ShapeType
    {
        Unknown,
        Point,
        Line,
        Area
    };

    public enum CentralIconType
    {
        NotSet,        // From "standard-speak" to human:
        MAIN,          // = Normal Case
        MAIN_1,        // = Normal Case - but can't have Modifier 1 because area is taken
        MAIN_2,        // = Normal Case - but can't have Modifier 2 because area is taken
        FULL_FRAME,    // = Touches Frame
        FULL_OCTAGON   // = Fills Full Central Icon Area
    };

    /// <summary>
    /// Helper class for converting Types to strings, codes, etc.
    /// </summary>
    public class TypeUtilities
    {
        public static string NameSeparator
        {
            get { return " : "; }
        }

        public static bool HasFrame(SymbolSetType symbolSet, string entityCode = "")
        {
            bool hasFrame = false;

            switch (symbolSet)
            {
                case SymbolSetType.Control_Measures : 
                case SymbolSetType.Atmospheric :
                case SymbolSetType.Oceanographic :
                case SymbolSetType.Meteorological_Space:
                case SymbolSetType.Special_Cases:
                    hasFrame = false;
                    break;
                case SymbolSetType.Sea_Surface :
                    if (entityCode == "150000") // exception case - ownship
                        hasFrame = false;
                    else 
                        hasFrame = true;
                    break;
                default :
                    hasFrame = true;
                    break;
            }

            return hasFrame;
        }

        public static bool HasFrameModifiers(SymbolSetType symbolSet)
        {
            bool hasFrameModifiers = false;

            switch (symbolSet)
            {
                case SymbolSetType.Land_Unit:
                case SymbolSetType.Land_Civilian_Unit_Organization:
                case SymbolSetType.Land_Equipment:
                case SymbolSetType.Land_Installation:
                    hasFrameModifiers = true;
                    break;
                default:
                    hasFrameModifiers = false;
                    break;
            }

            return hasFrameModifiers;
        }

        public static bool IsWeather(SymbolSetType symbolSet)
        {
            bool isWeather = false;

            switch (symbolSet)
            {
                case SymbolSetType.Atmospheric:
                case SymbolSetType.Oceanographic:
                case SymbolSetType.Meteorological_Space:
                    isWeather = true;
                    break;
            }

            return isWeather;
        }

        public static string ValidateAndZeroPad(string checkString, int requiredLength)
        {
            bool pass = !(string.IsNullOrEmpty(checkString) ||
                    (checkString.Length != requiredLength));

            if (pass)
                return checkString;

            string zeroPaddedString = string.Empty;
            if (checkString.Length <= requiredLength)
                zeroPaddedString = checkString.PadLeft(requiredLength, '0');
            else
                zeroPaddedString = zeroPaddedString.PadLeft(requiredLength, '0'); 

            return zeroPaddedString;
        }

        // Mapping the SymbolSet to the "Dimension" or expected frame
        public static Dictionary<SymbolSetType, SymbolSetType> SymbolSetToFrameMapping 
            = new Dictionary<SymbolSetType, SymbolSetType>()
        {  
            { SymbolSetType.Unknown, SymbolSetType.Unknown },
            { SymbolSetType.NotSet, SymbolSetType.Unknown },

            { SymbolSetType.Air, SymbolSetType.Air },
            { SymbolSetType.Air_Missile, SymbolSetType.Air },

            { SymbolSetType.Space, SymbolSetType.Space },
            { SymbolSetType.Space_Missile, SymbolSetType.Space },

            { SymbolSetType.Land_Unit, SymbolSetType.Land_Unit },
            { SymbolSetType.Land_Civilian_Unit_Organization, SymbolSetType.Land_Unit },
            { SymbolSetType.Land_Equipment, SymbolSetType.Sea_Surface },
            { SymbolSetType.Land_Installation, SymbolSetType.Land_Installation },

            { SymbolSetType.Control_Measures, SymbolSetType.NotSet },

            { SymbolSetType.Cyberspace, SymbolSetType.Sea_Surface },

            { SymbolSetType.Atmospheric, SymbolSetType.NotSet },
            { SymbolSetType.Oceanographic, SymbolSetType.NotSet },
            { SymbolSetType.Meteorological_Space, SymbolSetType.NotSet },

            { SymbolSetType.Sea_Surface, SymbolSetType.Sea_Surface },
            { SymbolSetType.Sea_Subsurface, SymbolSetType.Sea_Subsurface },
            { SymbolSetType.Mine_Warfare, SymbolSetType.Sea_Subsurface },

            { SymbolSetType.Activities,  SymbolSetType.Activities }, 

            { SymbolSetType.Signals_Intelligence_Space, SymbolSetType.Space },
            { SymbolSetType.Signals_Intelligence_Air, SymbolSetType.Air },
            { SymbolSetType.Signals_Intelligence_Land, SymbolSetType.Sea_Surface },
            { SymbolSetType.Signals_Intelligence_Surface, SymbolSetType.Sea_Surface },
            { SymbolSetType.Signals_Intelligence_Subsurface,  SymbolSetType.Sea_Subsurface },

            { SymbolSetType.Special_Cases, SymbolSetType.Unknown }
        }; // end SymbolSetToFrameType

        public static Dictionary<SymbolSetType, string> SymbolSetToFolderName = new Dictionary<SymbolSetType, string>()
        {
            { SymbolSetType.Unknown, "" },
            { SymbolSetType.NotSet, "" }, 
            { SymbolSetType.Air, "Air" },
            { SymbolSetType.Air_Missile, "Air" },

            { SymbolSetType.Space, "Space" },
            { SymbolSetType.Space_Missile, "Space" },

            { SymbolSetType.Land_Unit, "Land" },
            { SymbolSetType.Land_Civilian_Unit_Organization, "Land" },
            { SymbolSetType.Land_Equipment, "Land" },
            { SymbolSetType.Land_Installation, "Land" },

            { SymbolSetType.Control_Measures, "ControlMeasures" },

            { SymbolSetType.Cyberspace, "Cyberspace" },

            { SymbolSetType.Atmospheric, @"METOC\Atmospheric" },
            { SymbolSetType.Oceanographic, @"METOC\Oceanographic" },
            { SymbolSetType.Meteorological_Space, @"METOC\Space" },

            { SymbolSetType.Sea_Surface, "SeaSurface" },
            { SymbolSetType.Sea_Subsurface, "SeaSubsurface" },
            { SymbolSetType.Mine_Warfare, "SeaSubsurface" },

            { SymbolSetType.Activities, "Activities" }, 

            { SymbolSetType.Signals_Intelligence_Space, "SigInt" },
            { SymbolSetType.Signals_Intelligence_Air, "SigInt" },
            { SymbolSetType.Signals_Intelligence_Land, "SigInt" },
            { SymbolSetType.Signals_Intelligence_Surface, "SigInt" },
            { SymbolSetType.Signals_Intelligence_Subsurface, "SigInt" },

            { SymbolSetType.Special_Cases, "" }
        }; //  end SymbolSetToFolderName dictionary declaration

        public static Dictionary<StandardIdentityAffiliationType, StandardIdentityAffiliationType>
            AffiliationToAffiliationFrameMapping = new Dictionary<StandardIdentityAffiliationType, StandardIdentityAffiliationType>()
        {
            { StandardIdentityAffiliationType.Pending, StandardIdentityAffiliationType.Unknown },
            { StandardIdentityAffiliationType.Unknown, StandardIdentityAffiliationType.Unknown },
            { StandardIdentityAffiliationType.Assumed_Friend, StandardIdentityAffiliationType.Friend },
            { StandardIdentityAffiliationType.Friend, StandardIdentityAffiliationType.Friend },
            { StandardIdentityAffiliationType.Neutral, StandardIdentityAffiliationType.Neutral },            
            { StandardIdentityAffiliationType.Suspect_Joker, StandardIdentityAffiliationType.Hostile },
            { StandardIdentityAffiliationType.Hostile, StandardIdentityAffiliationType.Hostile },
            { StandardIdentityAffiliationType.Dont_Display_Frame, StandardIdentityAffiliationType.Unknown },
            { StandardIdentityAffiliationType.NotSet, StandardIdentityAffiliationType.Unknown }
        };

        // The suffix used for those center icons that need to touch the frame
        // Decoder _0 - Unknown, _1 - Friend, _2 - Neutral, _3 - Hostile
        public static Dictionary<StandardIdentityAffiliationType, string> AffiliationFrameToSuffixName 
            = new Dictionary<StandardIdentityAffiliationType, string>()
        {
            { StandardIdentityAffiliationType.Pending, "_0" },
            { StandardIdentityAffiliationType.Unknown, "_0" },
            { StandardIdentityAffiliationType.Assumed_Friend, "_1" },
            { StandardIdentityAffiliationType.Friend, "_1" },
            { StandardIdentityAffiliationType.Neutral, "_2" },
            { StandardIdentityAffiliationType.Suspect_Joker, "_3" },
            { StandardIdentityAffiliationType.Hostile, "_3" },
            { StandardIdentityAffiliationType.Dont_Display_Frame, "_0" },
            { StandardIdentityAffiliationType.NotSet, "_0" }
        };

        public static Dictionary<StandardIdentityAffiliationType, char> AffiliationToCharlieChar
            = new Dictionary<StandardIdentityAffiliationType, char>()
        {
            { StandardIdentityAffiliationType.Pending, 'P' },
            { StandardIdentityAffiliationType.Unknown, 'U' },
            { StandardIdentityAffiliationType.Assumed_Friend, 'A' },
            { StandardIdentityAffiliationType.Friend, 'F' },
            { StandardIdentityAffiliationType.Neutral, 'N' },
            { StandardIdentityAffiliationType.Suspect_Joker, 'S' },
            { StandardIdentityAffiliationType.Hostile, 'H' },
            { StandardIdentityAffiliationType.Dont_Display_Frame, 'U' },
            { StandardIdentityAffiliationType.NotSet, 'U' }
        };

        public static Dictionary<StatusType, char> StatusToCharlieChar
            = new Dictionary<StatusType, char>()
        {
            { StatusType.Present, 'P' },
            { StatusType.Planned_Anticipated_Suspect, 'A' },
            { StatusType.Present_Fully_Capable, 'C' },
            { StatusType.Present_Damaged, 'D' },
            { StatusType.Present_Destroyed, 'X' },
            { StatusType.Present_Full_To_Capacity, 'F' },
            { StatusType.NotSet, 'P' }
        };

        public static Dictionary<HeadquartersTaskForceDummyType, char> HqTfFdToCharlieChar
            = new Dictionary<HeadquartersTaskForceDummyType, char>()
        {
            { HeadquartersTaskForceDummyType.NoHQTFDummyModifier, '-' },
            { HeadquartersTaskForceDummyType.Feint_Dummy, 'F' },
            { HeadquartersTaskForceDummyType.Headquarters, 'A' },
            { HeadquartersTaskForceDummyType.Feint_Dummy_Headquarters, 'C' },
            { HeadquartersTaskForceDummyType.Task_Force, 'E' },
            { HeadquartersTaskForceDummyType.Feint_Dummy_Task_Force, 'G' },
            { HeadquartersTaskForceDummyType.Task_Force_Headquarters, 'B' },
            { HeadquartersTaskForceDummyType.Feint_Dummy_Task_Force_Headquarters, 'D' }
        };

        public static Dictionary<EchelonMobilityType, char> EchelonMobilityToCharlieChar
            = new Dictionary<EchelonMobilityType, char>()
        {
            { EchelonMobilityType.NoEchelonMobility, '-' },
            { EchelonMobilityType.Team_Crew, 'A' },
            { EchelonMobilityType.Squad, 'B' },
            { EchelonMobilityType.Section, 'C' },
            { EchelonMobilityType.Platoon_Detachment, 'D' },
            { EchelonMobilityType.Company_Battery_Troop, 'E' },
            { EchelonMobilityType.Battalion_Squadron, 'F' },
            { EchelonMobilityType.Regiment_Group, 'G' },
            { EchelonMobilityType.Brigade, 'H' },
            { EchelonMobilityType.Division, 'I' },
            { EchelonMobilityType.Corps_MEF, 'J' },
            { EchelonMobilityType.Army, 'K' },
            { EchelonMobilityType.Army_Group_Front, 'L' },
            { EchelonMobilityType.Region_Theater, 'M' },
            { EchelonMobilityType.Command, 'M' },

            { EchelonMobilityType.Wheeled_Limited_Cross_Country, 'O' },
            { EchelonMobilityType.Wheeled_Cross_Country, 'P' },
            { EchelonMobilityType.Tracked, 'Q' },
            { EchelonMobilityType.Wheeled_And_Tracked_Combination, 'R' },
            { EchelonMobilityType.Towed, 'S' },
            { EchelonMobilityType.Rail, 'T' },
            { EchelonMobilityType.Pack_Animals, 'W' },
            { EchelonMobilityType.Over_Snow_Prime_Mover, 'U' },
            { EchelonMobilityType.Sled, 'V' },
            { EchelonMobilityType.Barge, 'X' },
            { EchelonMobilityType.Amphibious, 'Y' },
            { EchelonMobilityType.Short_Towed_Array, '-' },
            { EchelonMobilityType.Long_Towed_Array, '-' }
        };

        public static Dictionary<string, string> EntitySubtypeCodeToLandUnitSpecialEntityName
            = new Dictionary<string, string>()  
        {
            { "95", "Headquarters Element" }, 
            { "96", "Division and Below Support" }, 
            { "97", "Corps Support" }, 
            { "98", "Theater/Echelons Above Corps Support" }
        };

        /// <summary>
        /// A very general purpose enum, string, int converter class
        /// Not all of these methods are used by this library
        /// </summary>
        public class EnumHelper
        {            
            public static List<string> getEnumValuesAsStrings(System.Type type)
            {
                List<string> values = new List<string>();
                Array enumValues = Enum.GetValues(type);
                foreach (Enum en in enumValues)
                {
                    string enumString = en.ToString();
                    enumString = enumString.Replace('_', ' ');

                    values.Add(enumString);
                }
                return values;
            }

            public static List<Enum> getEnumValues(System.Type type)
            {
                List<Enum> values = new List<Enum>();
                Array enumValues = Enum.GetValues(type);
                foreach (Enum en in enumValues)
                {
                    values.Add(en);
                }
                return values;
            }

            static public int getIndexFromEnum(System.Type type, Enum en)
            {
                int index = 0;

                System.Array enumValues = Enum.GetValues(type);
                foreach (Enum enumValue in enumValues)
                {
                    if (enumValue.GetHashCode() == en.GetHashCode())
                        break;

                    index++;
                }

                // if none found, just return the first value
                return index;
            }

            static public Enum getEnumFromIndex(System.Type type, int index)
            {
                System.Array enumValues = Enum.GetValues(type);
                return (Enum)enumValues.GetValue(index);
            }

            static public string getEnumValAsString(Enum theEnum, int requiredLength = 1)
            {
                int hashCode = theEnum.GetHashCode();
                string hashCodeString = Convert.ToString(hashCode);

                if (hashCodeString.Length < requiredLength)
                    hashCodeString = hashCodeString.PadLeft(requiredLength, '0');

                return hashCodeString;
            }

            static public Enum getEnumFromHashCodeString(System.Type type, string hashCodeString)
            {
                int hashCode = 0;
                try
                {
                    hashCode = Convert.ToInt32(hashCodeString);
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrWhiteSpace(hashCodeString))
                        throw;

                    System.Diagnostics.Trace.WriteLine("getEnumFromHashCodeString: Exception Converting String to Int: " + hashCodeString + ", Msg: " +  ex.Message);
                }

                int index = 0;

                System.Array enumValues = Enum.GetValues(type);

                bool found = false;

                foreach (Enum enumValue in enumValues)
                {
                    if (enumValue.GetHashCode() == hashCode)
                    {
                        found = true;
                        break;
                    }

                    index++;
                }

                if (!found)
                {
                    System.Diagnostics.Trace.WriteLine("Warning Enum Value not found for value: " + hashCodeString);
                    index = 0;
                }

                return (Enum)enumValues.GetValue(index);
            }

            static public string getStringFromEnum(Enum theEnum)
            {
                string enumString = theEnum.ToString();

                string enumStringNoUnderScores = enumString.Replace('_', ' ');

                return enumStringNoUnderScores;                
            }

            static public Enum getEnumFromString(System.Type type, string enumString)
            {
                int index = 0;

                string enumStringNoSpaces = enumString.Replace(' ', '_');

                System.Array enumValues = Enum.GetValues(type);

                bool found = false;
                foreach (Enum enumValue in enumValues)
                {
                    if (enumValue.ToString() == enumStringNoSpaces)
                    {
                        found = true;
                        break;
                    }

                    index++;
                }

                if (!found)
                {
                    System.Diagnostics.Trace.WriteLine("Warning Enum Value not found: " + enumString);
                    index = 0; // just return first value
                }

                return (Enum)enumValues.GetValue(index);
            }

            static public string getEnumStringFromChar(System.Type type, char enumChar)
            {
                int index = 0;

                string enumString = "NOT FOUND";
                bool found = false;

                System.Array enumValues = Enum.GetValues(type);
                foreach (Enum enumValue in enumValues)
                {
                    if (enumValue.GetHashCode() == enumChar)
                    {
                        found = true;
                        enumString = enumValue.ToString();
                        break;
                    }

                    index++;
                }

                if (found)
                {
                    string enumStringWithSpaces = enumString.Replace('_', ' ');
                    return enumStringWithSpaces;
                }

                return null;
            }

            static public char getEnumCharFromString(System.Type type, string enumString)
            {
                int index = 0;

                char enumChar = '-';
                string enumStringNoSpaces = enumString.Replace(' ', '_');

                System.Array enumValues = Enum.GetValues(type);

                foreach (Enum enumValue in enumValues)
                {
                    if (enumValue.ToString() == enumStringNoSpaces)
                    {
                        enumChar = (char)enumValue.GetHashCode();
                        break;
                    }

                    index++;
                }

                return enumChar;
            }

        } // Utility Class EnumHelper

    } // Class TypeUtilities

}