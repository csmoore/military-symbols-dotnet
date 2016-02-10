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
    public class SymbolIdCode
    {
        public SymbolIdCode()
        {
        }

        public SymbolIdCode(string codeLength20orCodeLength8)
        {
            setSymbolIdCode(codeLength20orCodeLength8);
        }

        public SymbolIdCode(Dictionary<string, string> attributeSet)
        {
            // allow SIDC code to be one of the supplied attributes,
            const string SIDC_ATTRIBUTE = "sidc";
            if (attributeSet.ContainsKey(SIDC_ATTRIBUTE))
            {
                string codeLength20orCodeLength8 = attributeSet[SIDC_ATTRIBUTE];
                setSymbolIdCode(codeLength20orCodeLength8);
                // but if supplied stop here
                return;
            }

            // otherwise if other attributes supplies, set them individually
            const string REAL_SIM_EX_ATTRIBUTE = "context";
            if (attributeSet.ContainsKey(REAL_SIM_EX_ATTRIBUTE))
            {
                string contextString = attributeSet[REAL_SIM_EX_ATTRIBUTE];
                this.StandardIdentity = (StandardIdentityRealExerciseSimType)
                    TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StandardIdentityRealExerciseSimType),
                    contextString);
            }

            const string AFFILIATION_ATTRIBUTE = "identity";
            if (attributeSet.ContainsKey(AFFILIATION_ATTRIBUTE))
            {
                string affilString = attributeSet[AFFILIATION_ATTRIBUTE];
                this.Affiliation = (StandardIdentityAffiliationType)
                    TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StandardIdentityAffiliationType),
                    affilString);
            }

            const string SYMBOL_SET_ATTRIBUTE = "symbolset";
            if (attributeSet.ContainsKey(SYMBOL_SET_ATTRIBUTE))
            {
                string symbolSetString = attributeSet[SYMBOL_SET_ATTRIBUTE];
                this.SymbolSet = (SymbolSetType)
                    TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), 
                    symbolSetString);
            }

            const string SYMBOL_ENTITY_ATTRIBUTE = "symbolentity";
            if (attributeSet.ContainsKey(SYMBOL_ENTITY_ATTRIBUTE))
            {
                string symbolEntityString = attributeSet[SYMBOL_ENTITY_ATTRIBUTE];
                this.EntityCode = symbolEntityString;
            }

            const string MOD1_ATTRIBUTE = "modifier1";
            if (attributeSet.ContainsKey(MOD1_ATTRIBUTE))
            {
                string mod1String = attributeSet[MOD1_ATTRIBUTE];
                this.ModifierOne = mod1String;
            }

            const string MOD2_ATTRIBUTE = "modifier2";
            if (attributeSet.ContainsKey(MOD2_ATTRIBUTE))
            {
                string mod2String = attributeSet[MOD2_ATTRIBUTE];
                this.ModifierTwo = mod2String;
            }

            const string ECHELON_ATTRIBUTE = "echelon";
            if (attributeSet.ContainsKey(ECHELON_ATTRIBUTE))
            {
                string echString = attributeSet[ECHELON_ATTRIBUTE];
                this.EchelonMobility = (EchelonMobilityType)
                    TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(EchelonMobilityType),
                    echString);
            }

            const string HQ_TF_FD_ATTRIBUTE = "indicator";
            if (attributeSet.ContainsKey(HQ_TF_FD_ATTRIBUTE))
            {
                string hqTfFdString = attributeSet[HQ_TF_FD_ATTRIBUTE];
                this.HeadquartersTaskForceDummy = (HeadquartersTaskForceDummyType)
                    TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(HeadquartersTaskForceDummyType),
                    hqTfFdString);
            }

            const string STATUS_ATTRIBUTE = "operationalcondition";
            if (attributeSet.ContainsKey(STATUS_ATTRIBUTE))
            {
                string statusString = attributeSet[STATUS_ATTRIBUTE];                
                this.Status = (StatusType)
                    TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StatusType),
                    statusString);
            }

        }

        public bool IsValid 
        { 
            get 
            { 
                // Checks the mains properties that could go wrong, but not yet an ehaustive check

                bool symbolSetValid = !((this.SymbolSet == SymbolSetType.NotSet) ||  
                    (this.SymbolSet == SymbolSetType.Unknown));
                if (!symbolSetValid)
                    return false;

                const bool LOOKUP_IF_CODE_VALID = false;

                if (LOOKUP_IF_CODE_VALID)
                {
                    // This code will actually use the lookup tables to determine if these
                    // are valid codes within the data, use if a more agressive validity 
                    // check is needed
                    bool entityCodeValid = Utilities.IsEntityCodeValid(SymbolSet, EntityCode);
                    if (!entityCodeValid)
                        return false;

                    bool mod1Valid = Utilities.IsModifierCodeValid(SymbolSet, 1, ModifierOne);
                    bool mod2Valid = Utilities.IsModifierCodeValid(SymbolSet, 2, ModifierTwo);
                    if (!mod1Valid || !mod2Valid)
                        return false;
                }

                // If we made it here, probably/likely valid
                return true;
            }
        }

        public static SymbolIdCode DefaultSymbolIdCode
        {
            get
            {
                // TODO: detetmine if this is the behavior we want(just creates a new one every time)
                return new SymbolIdCode();
            }
        }

        /// <summary>
        /// The Full 20-digit code
        /// </summary>
        public string Code
        {
            get
            {
                populateCodeFromProperties();
                return code;
            }
            set
            {
                // if the value has changed
                if (code != value)
                {
                    string code2Check = value;

                    if (string.IsNullOrWhiteSpace(code2Check) ||
                         (code2Check.Length != 20))
                    {
                        System.Diagnostics.Trace.WriteLine("WARNING: Trying to set SymbolIdCode.Code from bad string length = " +
                            code2Check.Length);
                        return;
                    }             

                    ulong convertCheck;
                    if (!UInt64.TryParse(code2Check, out convertCheck))
                    {
                        System.Diagnostics.Trace.WriteLine("WARNING: Trying to set SymbolIdCode.Code from bad format, string = " +
                            code2Check);
                        return;
                    }

                    code = code2Check;

                    // Populate the objects field from this code
                    populatePropertiesFromCode();
                }
            }
        }
        protected string code = null;

        /// <summary>
        /// The 8-digit, most significant portions of the Code: symbol set & entity 
        /// </summary>
        public string ShortenedCode
        {
            get
            {
                StringBuilder sbCode = new StringBuilder();
                // SymbolSet (Digits 5 & 6)
                sbCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.SymbolSet, 2));
                // EntityCode (Digit 11-16)
                sbCode.Append(EntityCode);

                return sbCode.ToString();
            }
            set
            {
                string shortenedCodeIn = value;
                if (string.IsNullOrWhiteSpace(shortenedCodeIn) ||
                     (shortenedCodeIn.Length != 8))
                {
                    System.Diagnostics.Trace.WriteLine("WARNING: Trying to set SymbolIdCode.ShortenedCode from bad string length = " +
                        shortenedCodeIn.Length);
                    return;
                }

                uint convertCheck;
                if (!UInt32.TryParse(shortenedCodeIn, out convertCheck))
                {
                    System.Diagnostics.Trace.WriteLine("WARNING: Trying to set SymbolIdCode.Code from bad format, string = " +
                        shortenedCodeIn);
                    return;
                }

                string symbolSetAsStringIn = shortenedCodeIn.Substring(0, 2);
                this.SymbolSetAsString = symbolSetAsStringIn;

                this.EntityCode = shortenedCodeIn.Substring(2, 6);
            }
        }

        /// <summary>
        /// Creates an easier to read code, eliminates unset attributes
        /// </summary>
        public string HumanReadableCode(bool showAffiliation = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(SymbolSet, 2));
            sb.Append("_");
            sb.Append(EntityCode);

            if (showAffiliation)
            {
                sb.Append("_A-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(Affiliation));
            }

            if ((ModifierOne != "00") || (ModifierTwo != "00"))
            {
                sb.Append("_M1-");
                sb.Append(ModifierOne);
                sb.Append("_M2-");
                sb.Append(ModifierTwo);
            }

            if (StandardIdentity != StandardIdentityRealExerciseSimType.Reality)
            {
                sb.Append("_ID-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(StandardIdentity));
            }

            if (Status != StatusType.Present)
            {
                sb.Append("_S-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(Status));
            }

            if (HeadquartersTaskForceDummy != HeadquartersTaskForceDummyType.NoHQTFDummyModifier)
            {
                sb.Append("_HQ-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(HeadquartersTaskForceDummy));
            }

            if (EchelonMobility != EchelonMobilityType.NoEchelonMobility)
            {
                sb.Append("_EM-");
                sb.Append(TypeUtilities.EnumHelper.getEnumValAsString(EchelonMobility));
            }

            return sb.ToString();
        }

        // This is an readable form of the id with words instead of numbers:
        // "SymbolSet : Entity : EntityType : EntitySubType : Modifier1 : Modifier2"
        // "Air : Military : Fixed Wing : Bomber : Light"
        // TODO: WARNING: this triggers a name lookup in SymbolLookup so use sparingly
        public string Name
        {
            get
            {
                return Utilities.GetWellFormedName(this);
            }
        }

        /// <summary>
        /// This is a special/exceptional case that slightly complicates this class (&the standard)
        /// Land Unit's can have 95/96/97/98 set in the EntitySubtypeCode field which impacts
        /// changes the other rules for how one would normally 1) look up an entry in the 
        /// Entity Table, 2) Draw the layers, etc.
        /// </summary>
        public bool IsLandUnitSpecialEntity
        {
            get
            {
                if (this.SymbolSet != SymbolSetType.Land_Unit)
                    return false;

                if (TypeUtilities.EntitySubtypeCodeToLandUnitSpecialEntityName.Keys.Contains(EntitySubTypeField))
                    return true;
                else
                    return false;
            }
        }

        ///////////////////////////////////////////////////////////
        // 2525D: A.5.2.1  Set A - First ten digits 
        // Version (Digits 1 and 2) 
        // Standard identity 1, Standard identity 2(AffiliationType) (Digits 3 and 4)
        // Symbol set (Digits 5 and 6)
        // Status (Digit 7)
        // HQ/Task Force/Dummy (Digit 8)
        // Amplifier/Descriptor (Digits 9 and 10)

        public StandardVersionType StandardVersion // Digits (1 & 2)
        {
            get { return standardVersion; }
            // TODO: allow these to be set later, only allow the default for now
        }
        protected StandardVersionType standardVersion = StandardVersionType.Current2525D;

        public StandardIdentityRealExerciseSimType StandardIdentity // StandardIdentity 1 (Digit 3)
        {
            get { return standardIdentity; }
            set { standardIdentity = value; }
        }
        protected StandardIdentityRealExerciseSimType standardIdentity = 
            StandardIdentityRealExerciseSimType.Reality;

        public StandardIdentityAffiliationType Affiliation  // StandardIdentity 2 (Digit 4)
        {
            get
            {
                return affiliation;
            }
            set
            {
                affiliation = value;
            }
        }
        protected StandardIdentityAffiliationType affiliation = StandardIdentityAffiliationType.Unknown;

        public SymbolSetType SymbolSet // (Digits 5 & 6)
        {
            get
            {
                return symbolSet;
            }
            set
            {
                symbolSet = value;
            }
        }
        protected SymbolSetType symbolSet = SymbolSetType.NotSet;

        public string SymbolSetAsString // (Digits 5 & 6)
        {
            get
            {
                return TypeUtilities.EnumHelper.getEnumValAsString(symbolSet, 2);
            }
            set
            {
                string symbolSetAsString = value;

                SymbolSetType symbolSetVal = (SymbolSetType)TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetAsString);

                // just in case the enum couldn't be converted (but doesn't let you set this to Unknown)
                if (symbolSetVal != SymbolSetType.Unknown) 
                    symbolSet = symbolSetVal; 
            }
        }

        public StatusType Status // (Digit 7)
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
        protected StatusType status = StatusType.Present;

        public HeadquartersTaskForceDummyType HeadquartersTaskForceDummy // (Digit 8)
        {
            get
            {
                return headquartersTaskForceDummy;
            }
            set
            {
                headquartersTaskForceDummy = value;
            }
        }
        protected HeadquartersTaskForceDummyType headquartersTaskForceDummy 
            = HeadquartersTaskForceDummyType.NoHQTFDummyModifier;

        public EchelonMobilityType EchelonMobility    // Amplifier 1, 2 (Digit 9 & 10)
        {
            get
            {
                return echelonMobility;
            }
            set
            {
                echelonMobility = value;
            }
        }
        protected EchelonMobilityType echelonMobility = EchelonMobilityType.NoEchelonMobility;

        //
        ///////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////
        // 2525D: A.5.2.2  Set B - Second ten digits
        // Entity (Digits 11 and 12)
        // Entity type (Digits 13 and 14)
        // Entity subtype (Digits 15 and 16)
        // Sector 1 modifier (Digits 17 and 18)
        // Sector 2 modifier (Digits 19 and 20)
        //

        public string EntityField //  (Digit 11-12)
        {
            get
            {
                return entity;
            }
            set
            {
                string checkString = value;
                // An unfortunate side-effect of having 2 similarly-named "Entity"
                if (checkString.Length == 6)
                    System.Diagnostics.Trace.WriteLine("Appear to be setting wrong field: Use `EntityCode`");

                entity = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string entity = "00";

        public string EntityTypeField //  (Digit 13-14)
        {
            get
            {
                return entityType;
            }
            set
            {
                string checkString = value;
                entityType = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string entityType = "00";

        public string EntitySubTypeField //  (Digit 13-14)
        {
            get
            {
                return entitySubType;
            }
            set
            {
                string checkString = value;
                entitySubType = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string entitySubType = "00";

        /// <summary>
        /// The FULL/Complete 6-character Entity Code (Digit 11-16)  
        /// </summary>
        public string EntityCode 
        {
            get
            {
                return EntityField + EntityTypeField + EntitySubTypeField;
            }
            set
            {
                string checkString = value;
                string fullCode = TypeUtilities.ValidateAndZeroPad(checkString, 6);
                EntityField        = fullCode.Substring(0, 2);
                EntityTypeField    = fullCode.Substring(2, 2);
                EntitySubTypeField = fullCode.Substring(4, 2);
            }
        }

        public string ModifierOne //  (Digit 17-18)
        {
            get
            {
                return modifierOne;
            }
            set
            {
                string checkString = value;
                modifierOne = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string modifierOne = "00";

        public string ModifierTwo //  (Digit 19-20)
        {
            get
            {
                return modifierTwo;
            }
            set
            {
                string checkString = value;
                modifierTwo = TypeUtilities.ValidateAndZeroPad(checkString, 2);
            }
        }
        protected string modifierTwo = "00";
        ///////////////////////////////////////////////////////////

        public override string ToString()
        {
            return convertToString(true);
        }

        public List<string> Tags
        {
            get
            {
                tags.Clear();

                if (this.Affiliation != StandardIdentityAffiliationType.NotSet)
                    tags.Add(this.Affiliation.ToString());

                if (this.SymbolSet != SymbolSetType.NotSet)
                    tags.Add(TypeUtilities.EnumHelper.getStringFromEnum(this.SymbolSet));

                if (this.Status != StatusType.NotSet)
                    tags.Add(this.Status.ToString());

                if (this.HeadquartersTaskForceDummy != HeadquartersTaskForceDummyType.NoHQTFDummyModifier)
                    tags.Add(this.HeadquartersTaskForceDummy.ToString());

                if (this.EchelonMobility != EchelonMobilityType.NoEchelonMobility)
                    tags.Add(this.EchelonMobility.ToString());

                string name = this.Name; // Warning: we only want to call this property once
                if (!string.IsNullOrWhiteSpace(name))
                    tags.Add(name);

                return tags;
            }
        }
        List<string> tags = new List<string>();

        public string CodeFirstTen
        {
            get
            {
                return convertToStringFirst10(false); ;
            }
        }

        public string CodeSecondTen
        {
            get
            {
                return convertToStringSecond10(false);
            }
        }

        public string CodeFirstTenFormatted
        {
            get
            {
                return convertToStringFirst10(true); ;
            }
        }

        public string CodeSecondTenFormatted
        {
            get
            {
                return convertToStringSecond10(true); 
            }
        }

        private string convertToStringFirst10(bool formatted = false)
        {
            StringBuilder sbSymbolIdCode = new StringBuilder();

            // Digits 1-10

            if (formatted)
                sbSymbolIdCode.Append("(");

            // StandardVersion - Digits (1 & 2)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.StandardVersion));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // StandardIdentity 1 - Real/Exercise/Sim (Digit 3)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.StandardIdentity));

            // StandardIdentity 2 - Affiliation (Digit 4)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.Affiliation));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // SymbolSet (Digits 5 & 6)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.SymbolSet, 2));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // Status (Digit 7) 
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.Status));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // HeadquartersTaskForceDummy (Digit 8)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.HeadquartersTaskForceDummy));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // EchelonMobility    // Amplifier 1, 2 (Digit 9 & 10)
            sbSymbolIdCode.Append(TypeUtilities.EnumHelper.getEnumValAsString(this.EchelonMobility, 2));

            if (formatted)
                sbSymbolIdCode.Append(")");

            return sbSymbolIdCode.ToString();
        }

        private string convertToStringSecond10(bool formatted = false)
        {
            StringBuilder sbSymbolIdCode = new StringBuilder();

            // Digits 11-20

            if (formatted)
                sbSymbolIdCode.Append("(");

            // EntityCode (Digit 11-16)
            sbSymbolIdCode.Append(EntityCode);

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // ModifierOne (Digit 17-18)
            sbSymbolIdCode.Append(ModifierOne);

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // ModifierTwo (Digit 19-20)
            sbSymbolIdCode.Append(ModifierTwo);

            if (formatted)
                sbSymbolIdCode.Append(")");

            return sbSymbolIdCode.ToString();
        }

        private string convertToString(bool formatted = false)
        {
            StringBuilder sbSymbolIdCode = new StringBuilder();

            // Digits 1-10
            if (formatted)
                sbSymbolIdCode.Append("( ");

            sbSymbolIdCode.Append(convertToStringFirst10(formatted));

            if (formatted)
                sbSymbolIdCode.Append(", ");

            // Digits 11-20
            sbSymbolIdCode.Append(convertToStringSecond10(formatted));

            if (formatted)
                sbSymbolIdCode.Append(" )");

            return sbSymbolIdCode.ToString();
        }

        private void populateCodeFromProperties()
        {
            // TODO: we may not want to do this every time, but only
            // when a property has changed
            code = convertToString(false);
        }

        private void populatePropertiesFromCode()
        {
            if (code.Length < 20)
                return;

            string standardIdentityString = code.Substring(2, 1);
            this.StandardIdentity = (StandardIdentityRealExerciseSimType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StandardIdentityRealExerciseSimType), standardIdentityString);

            string affiliationString = code.Substring(3, 1);
            this.Affiliation = (StandardIdentityAffiliationType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StandardIdentityAffiliationType), affiliationString);

            string symbolSetString = code.Substring(4, 2);
            this.SymbolSet = (SymbolSetType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(SymbolSetType), symbolSetString);

            string statusString = code.Substring(6, 1);
            this.Status = (StatusType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(StatusType), statusString);

            string hqTFFDString = code.Substring(7, 1);
            this.HeadquartersTaskForceDummy = (HeadquartersTaskForceDummyType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(HeadquartersTaskForceDummyType), hqTFFDString);

            string echelonMobilityString = code.Substring(8, 2);
            this.EchelonMobility = (EchelonMobilityType)
                TypeUtilities.EnumHelper.getEnumFromHashCodeString(typeof(EchelonMobilityType), echelonMobilityString);

            this.EntityCode = code.Substring(10, 6);
            this.ModifierOne  = code.Substring(16, 2);
            this.ModifierTwo = code.Substring(18, 2);
        }

        private void setSymbolIdCode(string codeLength20orCodeLength8)
        {
            if (string.IsNullOrWhiteSpace(codeLength20orCodeLength8))
                System.Diagnostics.Trace.WriteLine("WARNING: Trying to create SymbolIdCode from bad string");

            if (codeLength20orCodeLength8.Length == 8)
                this.ShortenedCode = codeLength20orCodeLength8;
            else if (codeLength20orCodeLength8.Length == 20)
                this.Code = codeLength20orCodeLength8;
            else
                System.Diagnostics.Trace.WriteLine("WARNING: Trying to create SymbolIdCode from bad string length = " +
                    codeLength20orCodeLength8.Length);
        }
        public override bool Equals(System.Object obj)
        {
            if ((System.Object)obj == null)
                return false;

            SymbolIdCode sidc = obj as SymbolIdCode;
            if ((System.Object)sidc == null)
                return false;

            return Equals(sidc);
        }

        public bool Equals(SymbolIdCode sidc)
        {
            if ((System.Object)sidc == null)
                return false;

            return (Code == sidc.Code);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public static bool operator ==(SymbolIdCode sidc1, SymbolIdCode sidc2)
        {
            return sidc1.Equals(sidc2);
        }

        public static bool operator !=(SymbolIdCode sidc1, SymbolIdCode sidc2)
        {
            return !sidc1.Equals(sidc2);
        }   

    }

}
