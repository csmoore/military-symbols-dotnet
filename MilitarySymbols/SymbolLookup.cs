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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilitarySymbols
{
    /// <summary>
    /// Symbol Search/Lookup - use several lookup tables to provide military symbol 
    ///                        search/query capabilities
    /// Tables: (1) EntityTable (2) Modifier Table (3) Legacy Id Codes
    /// 
    /// WARNING: This class is highly dependent on the *column names* for its queries
    ///          If you change the format/columns(mainly the names) of the dependent
    ///          data tables, these queries/code may also need to be changed.
    /// </summary>
    public class SymbolLookup
    {
        // TODO: this class is a bit of a mess, this was just quick prototyping,
        //       refactor/cleanse later - I know that this is just a big, complicated ball
        //       of query/lookup code, but hopefully some of the methods make sense for the
        //       the types of things you need to query about mil-symbols i.e.:
        //       get{Distinct}Modifiers, get{Distinct}Entities, get{Type}Code etc.

        public SymbolLookup()
        {
        }

        /// <summary>
        /// Search based on the one or more attributes supplied
        /// Default param (symbolSet == NotSet) returns all symbols with affiliation set to Unknown
        /// </summary>
        public List<MilitarySymbol> GetMilitarySymbols(SymbolSetType symbolSet = SymbolSetType.NotSet,
            StandardIdentityAffiliationType affiliation = StandardIdentityAffiliationType.Unknown,
            string entityNameString = "", string entityTypeNameString = "", 
            string entitySubTypeNameString = "", bool exactEntityMatchOnly = false)
        {
            Initialize(); 

            List<MilitarySymbol> symbolList = new List<MilitarySymbol>();

            if (EntityTable == null)
                return symbolList; // nothing

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            var results = from row in EntityTable.AsEnumerable()
                      where (row.Field<string>("SymbolSet") == symbolSetToSearch)
                      select row;

            if (symbolSet == SymbolSetType.NotSet)
                results = from row in EntityTable.AsEnumerable()
                          select row;

            // Check that search returned something
            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("WARNING: Empty result of EntityTable search: " + symbolSetToSearch);
                // TODO: add search params to the debug output
                return symbolList; // empty list
            }

            ////////////////////////////////////////////////////////////////
            // Filters for Entity, EntityType, EntitySubType if these are suppied

            // TRICKY: exactEntityMatchOnly if we only want the 1 that matches exactly, versus all of the
            // type, subtype matches
            if (exactEntityMatchOnly)
            {
                results = from row in results
                            where ((row.Field<string>("Entity") == entityNameString) 
                            & (row.Field<string>("EntityType") == entityTypeNameString)
                            & (row.Field<string>("EntitySubType") == entitySubTypeNameString))
                            select row;
            }
            else if (!string.IsNullOrEmpty(entityNameString))
            {
                // filter results if this is set
                results = from row in results
                            where (row.Field<string>("Entity") == entityNameString)
                            select row;

                if (!string.IsNullOrEmpty(entityTypeNameString))
                {
                    // filter results if this is set
                    results = from row in results
                                where (row.Field<string>("EntityType") == entityTypeNameString)
                                select row;

                    if (!string.IsNullOrEmpty(entitySubTypeNameString))
                    {
                        // filter results if this is set
                        results = from row in results
                                    where (row.Field<string>("EntitySubType") == entitySubTypeNameString)
                                    select row;

                    }
                }
            }

            // Check that search returned something
            resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("WARNING: Empty result of search");
                // TODO: add search params to the debug output
                return symbolList; // empty list
            }

            foreach (DataRow row in results)
            {
                string symbolSetString = row["SymbolSet"] as string;
                string entityCode = row["Code"] as string;
                string geoType = row["GeometryType"] as string;

                string entityName = row["Entity"] as string;
                string entityTypeName = row["EntityType"] as string;
                string entitySubTypeName = row["EntitySubType"] as string;

                string centralIconTypeString = row["IconType"] as string;

                MilitarySymbol createSymbol = CreateSymbolFromStringProperties( 
                    affiliation, symbolSetString, entityCode, geoType,
                    entityName, entityTypeName, entitySubTypeName, centralIconTypeString);

                if (createSymbol != null)
                    symbolList.Add(createSymbol);
            }

            return symbolList;
        }

        public string GetModifierNameFromCode(SymbolSetType symbolSet, int modifierNumber,
            string modifierCodeString)
        {
            Initialize();

            if ((ModifierTable == null) || (modifierCodeString == "00"))
                return string.Empty;

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            string modifierToSearch = modifierNumber.ToString();

            var results = from row in ModifierTable.AsEnumerable()
                          where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                               & (row.Field<string>("Code") == modifierCodeString)
                               & (row.Field<string>("ModifierNumber") == modifierToSearch))
                          select row;

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Modifier Code not found: " + modifierCodeString);
                return string.Empty;
            }

            string modifierName = string.Empty;

            foreach (DataRow row in results)
            {
                modifierName = row["Name"] as string;

                // We only care about the 1st result
                break;
            }

            return modifierName;
        }

        public string GetModifierCodeFromName(SymbolSetType symbolSet, int modifierNumber, 
            string modifierNameString)
        {
            Initialize(); 

            if (ModifierTable == null)
                return string.Empty;

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            // assmumes that the names will be unique within a symbol set
            // if not, we will also need the modifier number as an input
            string modifierToSearch = modifierNumber.ToString();

            var results = from row in ModifierTable.AsEnumerable()
                           where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                & (row.Field<string>("Name") == modifierNameString)
                                & (row.Field<string>("ModifierNumber") == modifierToSearch))
                           select row;

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Modifier Name not found: " + modifierNameString);
                return string.Empty;
            }

            string modifierCode = string.Empty;

            foreach (DataRow row in results)
            {
                modifierCode = row["Code"] as string;

                // We only care about the 1st result
                break;
            }

            return modifierCode;
        }

        public string GetModifierCategoryFromName(SymbolSetType symbolSet, string modifierNameString,
            int modfierNumber = 1)
        {
            Initialize(); 

            if (ModifierTable == null)
                return string.Empty;

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            string modifierToSearch = modfierNumber.ToString();

            var results = from row in ModifierTable.AsEnumerable()
                          where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                               & (row.Field<string>("Name") == modifierNameString)
                                & (row.Field<string>("ModifierNumber") == modifierToSearch))
                          select row;

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Modifier Name not found: " + modifierNameString);
                return string.Empty;
            }

            string modifierCategory = string.Empty;

            foreach (DataRow row in results)
            {
                modifierCategory = row["Category"] as string;

                // We only care about the 1st result
                break;
            }

            return modifierCategory;
        }

        public List<string> GetModifierCodesForSymbolSet(SymbolSetType symbolSet, int modfierNumber = 1)
        {
            Initialize();

            List<string> modifierCodeStrings = new List<string>();

            if (ModifierTable == null)
                return modifierCodeStrings; // nothing

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            string modifierToSearch = modfierNumber.ToString();

            var results = (from row in ModifierTable.AsEnumerable()
                           where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                & (row.Field<string>("ModifierNumber") == modifierToSearch))
                           select row.Field<string>("Code"));

            foreach (var result in results)
            {
                modifierCodeStrings.Add(result);
            }

            return modifierCodeStrings;
        }

        public List<string> GetDistinctModifierNames(SymbolSetType symbolSet, int modfierNumber = 1)
        {
            Initialize();

            List<string> distinctResultStrings = new List<string>();

            if (ModifierTable == null)
                return distinctResultStrings; // nothing

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            string modifierToSearch = modfierNumber.ToString();

            var results = (from row in ModifierTable.AsEnumerable()
                           where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                & (row.Field<string>("ModifierNumber") == modifierToSearch))
                           select row.Field<string>("Name")).Distinct();

            foreach (var result in results)
            {
                distinctResultStrings.Add(result);
            }

            return distinctResultStrings;
        }

        public List<string> GetDistinctEntries(SymbolSetType symbolSet, string entityNameString = "",
            string entityTypeNameString = "")
        {
            Initialize();

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            List<string> distinctResultStrings = new List<string>();

            if (EntityTable == null)
                return distinctResultStrings; // nothing

            // TODO: remove/refactor redundant code below

            if (string.IsNullOrEmpty(entityNameString))
            {
                // Distinct Entities for a SymbolSet
                var results = (from row in EntityTable.AsEnumerable()
                               where (row.Field<string>("SymbolSet") == symbolSetToSearch)
                               select row.Field<string>("Entity")).Distinct();

                int resultCount = results.Count();
                if (resultCount < 1)
                {
                    System.Diagnostics.Trace.WriteLine("WARNING: Empty result of search");
                    return distinctResultStrings; // empty list
                }

                foreach (var result in results)
                {
                    distinctResultStrings.Add(result);
                }
            }
            else if (string.IsNullOrEmpty(entityTypeNameString))
            {
                // Distinct EntityType for Entities
                var results = (from row in EntityTable.AsEnumerable()
                               where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                    & (row.Field<string>("Entity") == entityNameString))
                               select row.Field<string>("EntityType")).Distinct();

                int resultCount = results.Count();
                if (resultCount < 1)
                {
                    System.Diagnostics.Trace.WriteLine("WARNING: Empty result of search");
                    return distinctResultStrings; // empty list
                }

                foreach (var result in results)
                {
                    if (!string.IsNullOrWhiteSpace(result))
                        distinctResultStrings.Add(result);
                }

            }
            else
            {
                //  Distinct EntitySubType for EntityTypes
                var results = (from row in EntityTable.AsEnumerable()
                               where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                    & (row.Field<string>("Entity") == entityNameString)
                                    & (row.Field<string>("EntityType") == entityTypeNameString))
                               select row.Field<string>("EntitySubType")).Distinct();

                int resultCount = results.Count();
                if (resultCount < 1)
                {
                    System.Diagnostics.Trace.WriteLine("WARNING: Empty result of search");
                    return distinctResultStrings; // empty list
                }

                foreach (var result in results)
                {
                    if (!string.IsNullOrWhiteSpace(result))
                        distinctResultStrings.Add(result);
                }
            }

            return distinctResultStrings;
        }

        public ShapeType GetEntityShapeFromCode(SymbolSetType symbolSet, string entityCode)
        {
            Initialize();

            if ((EntityTable == null) || (symbolSet == SymbolSetType.NotSet) ||
                (entityCode == "000000") || entityCode.Length != 6)
                return ShapeType.Unknown;

            string entityCodeToSearch = entityCode;
            if (symbolSet == SymbolSetType.Land_Unit)
            {
                // See SymbolIdCode.IsLandUnitSpecialEntity for explanation of this exceptional case
                string entitySubType = entityCode.Substring(4, 2);
                if (TypeUtilities.EntitySubtypeCodeToLandUnitSpecialEntityName.Keys.Contains(entitySubType))
                {
                    entityCodeToSearch = entityCode.Substring(0, 4) + "00";
                }
            }

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            var results = from row in EntityTable.AsEnumerable()
                          where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                & (row.Field<string>("Code") == entityCodeToSearch))
                          select row;

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Entity Code not found: " + entityCode);
                return ShapeType.Unknown;
            }

            ShapeType shape = ShapeType.Unknown;

            foreach (DataRow row in results)
            {
                string geoType = row["GeometryType"] as string;
                shape = (ShapeType)TypeUtilities.EnumHelper.getEnumFromString(typeof(ShapeType), geoType);
                break;
            }

            return shape;
        }

        public CentralIconType GetEntityIconTypeFromCode(SymbolSetType symbolSet, string entityCode)
        {
            Initialize();

            if ((EntityTable == null) || (symbolSet == SymbolSetType.NotSet) ||
                (entityCode == "000000") || entityCode.Length != 6)
                return CentralIconType.NotSet;

            string entityCodeToSearch = entityCode;
            if (symbolSet == SymbolSetType.Land_Unit)
            {
                // See SymbolIdCode.IsLandUnitSpecialEntity for explanation of this exceptional case
                string entitySubType = entityCode.Substring(4, 2);
                if (TypeUtilities.EntitySubtypeCodeToLandUnitSpecialEntityName.Keys.Contains(entitySubType))
                {
                    entityCodeToSearch = entityCode.Substring(0, 4) + "00";
                }
            }

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            var results = from row in EntityTable.AsEnumerable()
                          where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                & (row.Field<string>("Code") == entityCodeToSearch))
                          select row;

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Entity Code not found: " + entityCode);
                return CentralIconType.NotSet;
            }

            CentralIconType iconFormat = CentralIconType.NotSet;

            foreach (DataRow row in results)
            {
                string centralIconTypeString = row["IconType"] as string;
                iconFormat = (CentralIconType)TypeUtilities.EnumHelper.getEnumFromString(typeof(CentralIconType), centralIconTypeString);
                break;
            }

            return iconFormat;
        }

        public bool GetEntityNamesFromCode(SymbolSetType symbolSet, string entityCode, 
            out string entityName, out string entityTypeName, out string entitySubTypeName)
        {
            Initialize();

            entityName = string.Empty;
            entityTypeName = string.Empty;
            entitySubTypeName = string.Empty; 

            if ((EntityTable == null) || (symbolSet == SymbolSetType.NotSet) ||
                (entityCode == "000000") || entityCode.Length != 6)
                return false;

            string entityCodeToSearch = entityCode;
            if (symbolSet == SymbolSetType.Land_Unit)
            {
                // See SymbolIdCode.IsLandUnitSpecialEntity for explanation of this exceptional case
                string entitySubType = entityCode.Substring(4, 2);
                if (TypeUtilities.EntitySubtypeCodeToLandUnitSpecialEntityName.Keys.Contains(entitySubType))
                {
                    entityCodeToSearch = entityCode.Substring(0, 4) + "00";
                }
            }

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            var results = from row in EntityTable.AsEnumerable()
                          where ((row.Field<string>("SymbolSet") == symbolSetToSearch)
                                & (row.Field<string>("Code") == entityCodeToSearch))
                          select row;

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Entity Code not found: " + entityCode);
                return false;
            }

            foreach (DataRow row in results)
            {
                entityName = row["Entity"] as string;
                entityTypeName = row["EntityType"] as string;
                entitySubTypeName = row["EntitySubType"] as string;

                // We only care about the 1st result
               break;
            }

            return true;
        }

        public string GetEntityCode(SymbolSetType symbolSet, string entityNameString, 
            string entityTypeNameString = "", string entitySubTypeNameString = "")
        {
            Initialize(); 

            if (EntityTable == null)
                return string.Empty;

            string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

            var results = from row in EntityTable.AsEnumerable()
                          where ((row.Field<string>("SymbolSet") == symbolSetToSearch) 
                                & (row.Field<string>("Entity") == entityNameString))
                          select row;
           
            if (!string.IsNullOrEmpty(entityTypeNameString))
            {
                // filter results if this is set
                results = from row in results
                          where (row.Field<string>("EntityType") == entityTypeNameString)
                          select row;
            }

            if (!string.IsNullOrEmpty(entitySubTypeNameString))
            {
                // filter results if this is set
                results = from row in results
                          where (row.Field<string>("EntitySubType") == entitySubTypeNameString)
                          select row;
            }

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Entity Name not found: " + entityNameString);
                return string.Empty;
            }

            string entityCode = string.Empty;

            foreach (DataRow row in results)
            {
                entityCode = row["Code"] as string;

                // We only care about the 1st result
                break;
            }

            return entityCode;
        }

        /// <summary>
        /// Creates a symbol by looking up the SymbolSet + {any} Entity Name 
        /// And then it sets the Affiliation if provided
        /// - this is just a simple/basic test
        /// </summary>
        public MilitarySymbol CreateSymbolByEntityName(string entityName,
                        SymbolSetType symbolSet = SymbolSetType.NotSet,
                        StandardIdentityAffiliationType affiliation = StandardIdentityAffiliationType.Unknown)
        {
            Initialize(); 

            if (EntityTable == null)
                return null;

            var results = from row in EntityTable.AsEnumerable()
                          where ((row.Field<string>("EntitySubType").Contains(entityName)
                            | (row.Field<string>("EntityType").Contains(entityName)
                            | (row.Field<string>("Entity").Contains(entityName)))))
                            select row;

            int resultCount = results.Count();

            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Entity Name not found: " + entityName);
                return null;
            }

            if (symbolSet != SymbolSetType.NotSet)
            {
                // filter results if this is set
                string symbolSetToSearch = TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);

                results = from row in results
                          where (row.Field<string>("SymbolSet") == symbolSetToSearch)
                              select row;
            }

            resultCount = results.Count();

            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Entity Name not found: " + entityName +
                    " in SymbolSet: " + symbolSet);
                return null;
            }

            MilitarySymbol retSymbol = null; 

            foreach (DataRow row in results)
            {
                string symbolSetString = row["SymbolSet"] as string;
                string entityCode      = row["Code"] as string;
                string geoType         = row["GeometryType"] as string;

                string entityNameFromLookup        = row["Entity"] as string;
                string entityTypeNameFromLookup    = row["EntityType"] as string;
                string entitySubTypeNameFromLookup = row["EntitySubType"] as string;

                string centralIconTypeString = row["IconType"] as string;

                retSymbol = CreateSymbolFromStringProperties(
                    affiliation, symbolSetString, entityCode, geoType,
                    entityNameFromLookup, entityTypeNameFromLookup, entitySubTypeNameFromLookup, 
                    centralIconTypeString);

                // TODO: figure out what to do if we get more than 1 result
                break;
            }

            return retSymbol;
        }

        public MilitarySymbol CreateSymbolFromStringProperties(
            // SymbolSetType symbolSet,
            StandardIdentityAffiliationType affiliation,
            string symbolSetString, string entityCode, string geoType,
            string entityName, string entityTypeName, string entitySubTypeName,
            string centralIconTypeString)
        {
            if ((string.IsNullOrEmpty(symbolSetString) || symbolSetString.Length != 2))
            {
                System.Diagnostics.Trace.WriteLine("CreateSymbolFromStringProperties - Bad Symbol Set Code");
                return null;
            }

            if ((string.IsNullOrEmpty(entityCode) || entityCode.Length != 6))
            {
                System.Diagnostics.Trace.WriteLine("CreateSymbolFromStringProperties - Bad Entity Code");
                return null;
            }

            MilitarySymbol retSymbol = new MilitarySymbol();

            SymbolIdCode sidc = new SymbolIdCode();
            sidc.Affiliation = affiliation;
            sidc.SymbolSetAsString = symbolSetString;
            sidc.EntityCode = entityCode;

            SymbolSetType symbolSet = (SymbolSetType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetString);

            string symbolSetName = TypeUtilities.EnumHelper.getStringFromEnum(symbolSet);

            retSymbol.Id = sidc;
            retSymbol.Shape = (ShapeType)TypeUtilities.EnumHelper.getEnumFromString(typeof(ShapeType), geoType);
            retSymbol.CentralIconFormat = (CentralIconType)TypeUtilities.EnumHelper.getEnumFromString(typeof(CentralIconType), centralIconTypeString);

            return retSymbol;
        }

        public bool GetCharlieCodeFromDelta(
            string symbolSetString, string entityString,
            string mod1String,  string mod2String, out string charlieCode)
        {
            Initialize();

            charlieCode = string.Empty;

            var results = from row in LegacyCodeMappingTable.AsEnumerable()
                          where (row.Field<string>("2525DeltaSymbolSet") == symbolSetString)
                                & (row.Field<string>("2525DeltaEntity") == entityString)
                                & (row.Field<string>("2525DeltaMod1") == mod1String)
                                & (row.Field<string>("2525DeltaMod2") == mod2String)
                          select row;

            int resultCount = results.Count();
            if (resultCount < 1)
            {
                // Try one more time without modifiers 
                // (TODO: this is a bit inefficient - if performance is a problem - refactor)
                results = from row in LegacyCodeMappingTable.AsEnumerable()
                          where (row.Field<string>("2525DeltaSymbolSet") == symbolSetString)
                                & (row.Field<string>("2525DeltaEntity") == entityString)
                          select row;
            }

            resultCount = results.Count();
            if (resultCount < 1)
            {
                // Uncomment if you want this info, but this is a normal result for many symbols
                // System.Diagnostics.Trace.WriteLine("Charlie Code not found: " + symbolSetString
                //    + " : " + entityString + " : " + mod1String + " : " + mod2String);
                return false;
            }
            else if (resultCount > 1)
            {
                System.Diagnostics.Trace.WriteLine("Warning Mutiple, Charlie Codes found, may not be correct or require modifiers: " 
                    + symbolSetString + " : " + entityString + " : " + mod1String + " : " + mod2String);
            }

            foreach (DataRow row in results)
            {
                charlieCode = row["2525Charlie"] as string;
                if (!string.IsNullOrEmpty(charlieCode))
                {
                    // may be muliple results with this table (some that do not have 2525C set)
                    break; 
                }
            }

            return !string.IsNullOrEmpty(charlieCode);
        }

        public bool GetDeltaCodeFromCharlie(string charlieCode, 
            out string symbolSetString, out string entityString,
            out string mod1String, out string mod2String)
        {
            if (!LegacyMappingInitialized)
                Initialize();

            symbolSetString = string.Empty;
            entityString    = string.Empty;
            mod1String      = string.Empty;
            mod2String      = string.Empty;

            if ((LegacyCodeMappingTable == null) || string.IsNullOrEmpty(charlieCode) 
                || (charlieCode.Length < 10))
                return false;

            bool isWeather = (charlieCode[0] == 'W');

            char replaceAffilChar = '*';
            if (isWeather)
                replaceAffilChar = charlieCode[1];

            StringBuilder sbLookupCharlieCode = new StringBuilder();
            sbLookupCharlieCode.Append(charlieCode[0]);
            sbLookupCharlieCode.Append(replaceAffilChar);
            sbLookupCharlieCode.Append(charlieCode[2]);
            if (isWeather)
                sbLookupCharlieCode.Append(charlieCode[3]);
            else
                sbLookupCharlieCode.Append('P');
            sbLookupCharlieCode.Append(charlieCode.Substring(4, 6));

            string lookupCharlieCode = sbLookupCharlieCode.ToString();

            var results = from row in LegacyCodeMappingTable.AsEnumerable()
                          where (row.Field<string>("2525Charlie1stTen") == lookupCharlieCode)
                          select row;

            int resultCount = results.Count();

            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("Charlie Code not found: " + lookupCharlieCode);
                return false;
            }

            bool found = false;

            foreach (DataRow row in results)
            {
                symbolSetString = row["2525DeltaSymbolSet"] as string;
                entityString = row["2525DeltaEntity"] as string;
                mod1String = row["2525DeltaMod1"] as string;
                mod2String = row["2525DeltaMod2"] as string;

                if ((resultCount > 1) && (charlieCode[0] == 'W'))
                {
                    string fullCharlieCode = row["2525Charlie"] as string;

                    if (fullCharlieCode == charlieCode)
                    {
                        found = true;
                        break;
                    }
                }
                else
                {
                    found = true;
                    break; // should only be 1 result for the rest
                }
            }

            return found;
        }

        /// <summary>
        /// Search the Charlie table and create a list of all of these symbols that can be 
        /// created/mapped to Delta
        /// </summary>
        public List<MilitarySymbol> GetMilitarySymbolsFromCharlie()
        {
            Initialize();

            List<MilitarySymbol> symbolList = new List<MilitarySymbol>();

            var results = from row in LegacyCodeMappingTable.AsEnumerable()
                          select row;

            // Check that search returned something
            int resultCount = results.Count();
            if (resultCount < 1)
            {
                System.Diagnostics.Trace.WriteLine("WARNING: Empty result of LegacyCodeMappingTable search");
                // TODO: add search params to the debug output
                return symbolList; // empty list
            }

            foreach (DataRow row in results)
            {
                string legacyCode = row["2525Charlie"] as string;

                if (!string.IsNullOrWhiteSpace(legacyCode))
                {
                    MilitarySymbol milSymbol = new MilitarySymbol();
                    milSymbol.Legacy2525Code = legacyCode;

                    symbolList.Add(milSymbol);
                }
            }

            return symbolList;
        }

        public bool Initialized
        {
            get
            {
                if ((EntityTable == null) || (ModifierTable == null) || (LegacyCodeMappingTable == null))
                    return false;

                if ((EntityTable.Rows != null) && (EntityTable.Columns != null) &&
                    (EntityTable.Rows.Count > 0) && (EntityTable.Columns.Count > 0) && 
                    (ModifierTable.Rows != null) && (ModifierTable.Columns != null) &&
                    (ModifierTable.Rows.Count > 0) && (ModifierTable.Columns.Count > 0) && 
                    (LegacyCodeMappingTable.Rows != null) && (LegacyCodeMappingTable.Columns != null) &&
                    (LegacyCodeMappingTable.Rows.Count > 0) && (LegacyCodeMappingTable.Columns.Count > 0))
                {
                    // Any other check needed? required column names?
                    return true;
                }

                return false;
            }
        }

        public bool LegacyMappingInitialized
        {
            get
            {
                if (LegacyCodeMappingTable == null)
                    return false;

                if ((LegacyCodeMappingTable.Rows != null) && (LegacyCodeMappingTable.Columns != null) &&
                    (LegacyCodeMappingTable.Rows.Count > 0) && (LegacyCodeMappingTable.Columns.Count > 0))
                    return true;

                return false;
            }
        }

        public DataTable EntityTable
        {
            get { return entityTable; }
        }
        private DataTable entityTable = null;

        public DataTable ModifierTable
        {
            get { return modifierTable; }
        }
        private DataTable modifierTable = null;

        public DataTable LegacyCodeMappingTable
        {
            get { return legacyCodeMappingTable; }
        }
        private DataTable legacyCodeMappingTable = null; 

        public bool Initialize()
        {
            if (Initialized)
                return true;

            // string basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            string assemblyPath = System.IO.Path.GetDirectoryName(
                              Uri.UnescapeDataString(
                                      new Uri(asm.CodeBase).LocalPath));
            string basePath = System.IO.Path.Combine(assemblyPath, "Data");

            string csvEntityTableFileName = "All_Entities.csv";
            string csvEntityTableFullPath = System.IO.Path.Combine(basePath, csvEntityTableFileName);
            CsvToTableMaker csvTableEntity = new CsvToTableMaker();
            csvTableEntity.LoadTable(csvEntityTableFullPath);
            entityTable = csvTableEntity.Table;

            string csvModifierTableFileName = "All_Modifiers.csv";
            string csvModifierTableFullPath = System.IO.Path.Combine(basePath, csvModifierTableFileName);
            CsvToTableMaker csvTableModifier = new CsvToTableMaker();
            csvTableModifier.LoadTable(csvModifierTableFullPath);
            modifierTable = csvTableModifier.Table;

            string csvLegacyCodeMappingTableFileName = "LegacyMappingTableCtoD.csv";
            string csvLegacyCodeMappingTableFullPath = System.IO.Path.Combine(basePath, csvLegacyCodeMappingTableFileName);
            CsvToTableMaker csvTableLegacyCodeMapping = new CsvToTableMaker();
            csvTableLegacyCodeMapping.LoadTable(csvLegacyCodeMappingTableFullPath);
            legacyCodeMappingTable = csvTableLegacyCodeMapping.Table;

            // In case you need to check these:
            //   csvTableEntity.DebugOutput();
            //   csvTableModifier.DebugOutput();
            //   csvTableLegacyCodeMapping.DebugOutput();

            if ((csvTableEntity.IsValid) && (csvTableModifier.IsValid) &&
                (csvTableLegacyCodeMapping.IsValid))
                return true;
            else
                return false;
        }

    }
}
